using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Windows;
using System.Xml;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio.ProjectActions
{
    internal class LoadFromFile
    {
        private Version _writer_version;

        internal LoadFromFile()
        {
        }

        // XMLDocument explained with examples:
        // http://www.codeproject.com/Articles/169598/Parse-XML-Documents-by-XMLDocument-and-XDocument

        internal Project Load(string filename)
        {
            Project project = new Project();

            Action action = () =>
            {
                // Note: XmlNode is an ancestor of XmlElement, so XmlElement has more functionality
                XmlDocument doc = new XmlDocument();

                doc.Load(filename);

                XmlNode root = doc.DocumentElement;

                _writer_version = new Version(root.Attributes["writerversion"].Value);

                XmlNode inputDatabaseNode = root.SelectSingleNode(@"/project/inputdatabase");

                if (inputDatabaseNode.Attributes["providerinvariantname"] != null)
                    project.ProviderInvariantName = inputDatabaseNode.Attributes["providerinvariantname"].Value;

                project.ConnectionString = inputDatabaseNode.Attributes["connectstring"].Value;

                XmlNode provider_settings_node = root.SelectSingleNode(@"/project/providersettings");

                if (provider_settings_node != null)
                {
                    project.AdvancedSettings.IncludeServerName = provider_settings_node.Attributes["includeservername"].Value == "yes";
                    project.AdvancedSettings.IncludeCatalogName = provider_settings_node.Attributes["includecatalogname"].Value == "yes";
                    project.AdvancedSettings.IncludeSchemaName = provider_settings_node.Attributes["includeschemaname"].Value == "yes";

                    project.AdvancedSettings.ColumnServer = provider_settings_node.Attributes["columnserver"].Value;
                    project.AdvancedSettings.ColumnCatalog = provider_settings_node.Attributes["columncatalog"].Value;
                    project.AdvancedSettings.ColumnSchema = provider_settings_node.Attributes["columnschema"].Value;
                    project.AdvancedSettings.ColumnTable = provider_settings_node.Attributes["columntable"].Value;
                    project.AdvancedSettings.ColumnType = provider_settings_node.Attributes["columntype"].Value;
                }

                for (int i = 0; i < project.VisualStudioProjects.Count; i++)
                {
                    VisualStudioProjectItem vs_projectitem = project.VisualStudioProjects[i];
                    XmlNode outputProjectNode = root.SelectSingleNode($"outputproject{i + 1}");

                    if (outputProjectNode == null)
                    {
                        vs_projectitem.OutputProjectFilename = "";
                        vs_projectitem.ProjectEnabled = false;
                        vs_projectitem.TargetPlatform = "NETStandard";
                        vs_projectitem.GenerateDirectAdoConnectionCode = true;
                    }
                    else
                    {
                        string vs_filename = outputProjectNode.InnerText;

                        // Begin: VenturaSQL upgrade from 1.0 to 1.1. Absolute path to relative path
                        if (vs_filename.Length >= 2 && vs_filename[1] == ':') // starts with drive like 'c:'
                        {
                            string basepath = Path.GetDirectoryName(filename);
                            vs_filename = StudioGeneral.GetRelativePath(basepath, vs_filename);
                        }
                        // End: VenturaSQL upgrade from 1.0 to 1.1. Absolute path to relative path

                        vs_projectitem.OutputProjectFilename = vs_filename;

                        if (outputProjectNode.Attributes["enabled"] != null)
                            vs_projectitem.ProjectEnabled = outputProjectNode.Attributes["enabled"].Value == "yes";

                        if (outputProjectNode.Attributes["targetplatform"] != null)
                            vs_projectitem.TargetPlatform = outputProjectNode.Attributes["targetplatform"].Value;

                        // Begin: Version 1.1 only supports the "NETStandard" platform.
                        if (vs_projectitem.TargetPlatform != "NETStandard")
                            vs_projectitem.TargetPlatform = "NETStandard";
                        // End: Version 1.1 only supports the "NETStandard" platform.

                        if (outputProjectNode.Attributes["generatedirectadoconnectioncode"] != null)
                            vs_projectitem.GenerateDirectAdoConnectionCode = outputProjectNode.Attributes["generatedirectadoconnectioncode"].Value == "yes";
                    }
                }

                // The autocreate node.
                XmlNode autocreate_node = root.SelectSingleNode("/project/autocreate");

                if (autocreate_node != null)
                {
                    project.AutoCreateSettings.Enabled = autocreate_node.Attributes["enabled"].Value == "yes";
                    project.AutoCreateSettings.Folder = autocreate_node.Attributes["folder"].Value;
                    project.AutoCreateSettings.CreateGetAll = autocreate_node.Attributes["getall"].Value == "yes";

                    if (autocreate_node.Attributes["incremental"] != null)
                        project.AutoCreateSettings.CreateIncremental = autocreate_node.Attributes["incremental"].Value == "yes";

                    // Read the 'excludedtablename' nodes.
                    foreach (XmlNode tablename_node in autocreate_node)
                    {
                        string server = tablename_node.Attributes["server"].Value;
                        string catalog = tablename_node.Attributes["catalog"].Value;
                        string schema = tablename_node.Attributes["schema"].Value;
                        string table = tablename_node.Attributes["table"].Value;
                        project.AutoCreateSettings.ExcludedTablenames.Add(new TableName(server, catalog, schema, table));
                    }
                }
                // begin: build the folder structure viewmodel
                XmlNode folderstructureNode = root.SelectSingleNode("/project/folderstructure");

                foreach (XmlNode foldernode in folderstructureNode)
                {
                    bool selected = false;

                    if (foldernode.Attributes["selected"] != null) // "selected" only supported since version 1.1
                        selected = (foldernode.Attributes["selected"].Value == "yes") ? true : false;

                    bool expanded = (foldernode.Attributes["expanded"].Value == "yes") ? true : false;

                    FolderItem fi = project.FolderStructure.FetchOrCreateFolderItem(foldernode.InnerText);
                    fi.IsSelected = selected;
                    fi.IsExpanded = expanded;
                }
                // end: build the folder structure viewmodel

                XmlNode definitionsNode = root.SelectSingleNode(@"/project/projectitems");

                foreach (XmlNode singleDefinitionNode in definitionsNode)
                {
                    string definitiontype = singleDefinitionNode.Name;

                    if (definitiontype == "recordsetdefinition")
                        ReadRecordsetDefinition(project, singleDefinitionNode);

                } // end of foreach

                project.ResetModified();

            }; // end of Action block

            Application.Current.Dispatcher.Invoke(action);

            return project;
        } // end of Load method

        private void ReadRecordsetDefinition(Project project, XmlNode singleDefinitionNode)
        {
            string classname = singleDefinitionNode.Attributes["classname"].Value;

            // Begin: since version 1.0.42 the classname includes the word "Recordset"
            if (classname.ToLower().EndsWith("recordset") == false)
                classname = classname + "Recordset";
            // End: since version 1.0.42 the classname includes the word "Recordset"

            RecordsetItem definitionitem = new RecordsetItem(project, null, classname);

            if (singleDefinitionNode.Attributes["enabled"] != null)
                definitionitem.Enabled = singleDefinitionNode.Attributes["enabled"].Value == "yes";

            bool selected = false;

            if (singleDefinitionNode.Attributes["selected"] != null) // "selected" only supported since version 1.1
                selected = (singleDefinitionNode.Attributes["selected"].Value == "yes") ? true : false;

            definitionitem.IsSelected = selected;

            for (int i = 0; i < project.VisualStudioProjects.Count; i++)
            {
                XmlNode node = singleDefinitionNode.SelectSingleNode($"project{i + 1}");

                if (node == null)
                    definitionitem.OutputProjects[i].Selected = true;
                else
                    definitionitem.OutputProjects[i].Selected = node.InnerText == "yes";
            }

            string outputfolder = singleDefinitionNode.SelectSingleNode("outputfolder").InnerText;


            XmlNode summary_node = singleDefinitionNode.SelectSingleNode("classsummary");

            // begin: conversion as description was changed to classsummary
            if (summary_node == null)
                summary_node = singleDefinitionNode.SelectSingleNode("description");
            // end: conversion as description was changed to classsummary

            definitionitem.ClassSummary = summary_node.InnerText;

            if (singleDefinitionNode.SelectSingleNode("implementdatabinding") != null)
                definitionitem.ImplementDatabinding = singleDefinitionNode.SelectSingleNode("implementdatabinding").InnerText == "yes";

            if (singleDefinitionNode.SelectSingleNode("rowloadincremental") != null)
                definitionitem.RowloadIncremental = singleDefinitionNode.SelectSingleNode("rowloadincremental").InnerText == "yes";

            XmlNode sqlscript_node = singleDefinitionNode.SelectSingleNode("sqlscript");

            // begin: conversion as sqlstatement was changed to sqlscript
            if (sqlscript_node == null)
                sqlscript_node = singleDefinitionNode.SelectSingleNode("sqlstatement");
            // end: conversion as sqlstatement was changed to sqlscript

            definitionitem.SqlScript = sqlscript_node.InnerText;

            XmlNode parametersNode = singleDefinitionNode.SelectSingleNode("parameters");

            foreach (XmlNode paramNode in parametersNode)
            {
                if (_writer_version < Version.Parse("2.5.0.0"))
                {
                    // Quite a complicated conversion SqlDbtype is replaced by DbType
                    ParameterItem parameteritem = convert_Old_SqlDbStyle_Parameter(project, paramNode);

                    if (parameteritem != null)
                        definitionitem.Parameters.Add(parameteritem);
                }
                else
                {
                    // regular parameter loading
                    ParameterItem parameteritem = new ParameterItem(project);
                    parameteritem.Name = paramNode.Attributes["name"].Value;
                    parameteritem.DbTypeString = paramNode.Attributes["dbtypestring"].Value;
                    parameteritem.FullTypename = paramNode.Attributes["fulltypename"].Value;
                    parameteritem.Input = paramNode.Attributes["input"].Value == "yes";
                    parameteritem.Output = paramNode.Attributes["output"].Value == "yes";
                    parameteritem.DesignValue = paramNode.Attributes["designvalue"].Value;

                    // setdbtype was added during 2.5 development.
                    XmlAttribute attr_dbtype = paramNode.Attributes["setdbtype"];
                    parameteritem.SetDbType = attr_dbtype == null ? false : attr_dbtype.Value == "yes";

                    parameteritem.SetLength = paramNode.Attributes["setlength"].Value == "yes";
                    parameteritem.SetPrecision = paramNode.Attributes["setprecision"].Value == "yes";
                    parameteritem.SetScale = paramNode.Attributes["setscale"].Value == "yes";
                    parameteritem.Length = int.Parse(paramNode.Attributes["length"].Value);
                    parameteritem.Precision = byte.Parse(paramNode.Attributes["precision"].Value);
                    parameteritem.Scale = byte.Parse(paramNode.Attributes["scale"].Value);
                    definitionitem.Parameters.Add(parameteritem);
                }

            }


            XmlNode resultsetsNode = singleDefinitionNode.SelectSingleNode("resultsets");

            if (resultsetsNode != null) /* compatible with 1.0.27 and earlier */
            {
                foreach (XmlNode resultsetNode in resultsetsNode)
                {
                    string resultset_name = resultsetNode.Attributes["name"].Value;

                    ResultsetItem resultsetitem = new ResultsetItem(project, definitionitem.Resultsets.Count + 1, resultset_name);

                    if (resultsetNode.Attributes["enabled"] != null)
                        resultsetitem.Enabled = resultsetNode.Attributes["enabled"].Value == "yes";

                    XmlNode tablenameNode = resultsetNode.SelectSingleNode("updateabletablename");

                    if (tablenameNode == null)
                    {
                        resultsetitem.UpdateableTableName = null;

                        // Begin: Old style updateable tablename as a string.
                        XmlAttribute utn_attribute = resultsetNode.Attributes["updateabletablename"];

                        if (utn_attribute != null)
                            resultsetitem.UpdateableTableName = TableNameStringToObject(utn_attribute.Value);
                        // End: Old style updateable tablename as a string.
                    }
                    else
                    {
                        string server = tablenameNode.Attributes["server"].Value;
                        string catalog = tablenameNode.Attributes["catalog"].Value;
                        string schema = tablenameNode.Attributes["schema"].Value;
                        string table = tablenameNode.Attributes["table"].Value;
                        resultsetitem.UpdateableTableName = new TableName(server, catalog, schema, table);
                    }

                    resultsetitem.GenerateReferencedTablesList(null); // Adds the UpdateableTableName to the list for the dropdown combobox

                    definitionitem.Resultsets.Add(resultsetitem);
                }
            }

            XmlNode userdefinedcolumnsNode = singleDefinitionNode.SelectSingleNode("userdefinedcolumns");
            foreach (XmlNode userdefinedcolumnNode in userdefinedcolumnsNode)
            {
                UDCItem udcitem = new UDCItem(project);
                udcitem.ColumnName = userdefinedcolumnNode.Attributes["name"].Value;

                udcitem.FullTypename = userdefinedcolumnNode.Attributes["fulltypename"].Value; 

                definitionitem.UserDefinedColumns.Add(udcitem);
            }

            project.FolderStructure.AddRecordsetItem(outputfolder, definitionitem);

        }

        private TableName TableNameStringToObject(string input)
        {
            input = input.Trim();

            if (input.Length == 0)
                return null;

            string[] arr = input.Split('.');

            for (int i = 0; i < arr.Length; i++)
            {
                arr[i] = arr[i].Replace("[", "").Replace("]", "");
            }

            List<string> list = new List<string>(arr);

            while (list.Count < 4)
                list.Insert(0, "");

            return new TableName(list[0], list[1], list[2], list[3]);
        }


        private string convert_TypeCode_To_FrameworkType(string typecode_string)
        {
            if (typecode_string == "Boolean")
                return typeof(bool).FullName;
            else if (typecode_string == "Byte")
                return typeof(byte).FullName;
            else if (typecode_string == "DateTime")
                return typeof(DateTime).FullName;
            else if (typecode_string == "Decimal")
                return typeof(decimal).FullName;
            else if (typecode_string == "Single")
                return typeof(Single).FullName;
            else if (typecode_string == "Double")
                return typeof(Double).FullName;
            else if (typecode_string == "Int16")
                return typeof(Int16).FullName;
            else if (typecode_string == "Int32")
                return typeof(Int32).FullName;
            else if (typecode_string == "Int64")
                return typeof(Int64).FullName;
            else if (typecode_string == "String")
                return typeof(String).FullName;
            else if (typecode_string == "Guid")
                return typeof(Guid).FullName;
            else if (typecode_string == "Bytes")
                return typeof(byte[]).FullName;
            else if (typecode_string == "Object")
                return typeof(object).FullName;
            else if (typecode_string == "TimeSpan")
                return typeof(TimeSpan).FullName;
            else if (typecode_string == "DateTimeOffset")
                return typeof(DateTimeOffset).FullName;
            else
                throw new Exception($"convert_TypeCode_To_FrameworkType, error unknown typecode {typecode_string}");
        }

        private ParameterItem convert_Old_SqlDbStyle_Parameter(Project project, XmlNode paramNode)
        {
            // Conversion
            bool enabled = true;

            if (paramNode.Attributes["enabled"] != null)
                enabled = paramNode.Attributes["enabled"].Value == "yes";

            if (enabled == false)
                return null;

            string _name = "";
            string _dbtype_string = "";
            string _fulltypename = "";
            bool _input = true;
            bool _output = false;
            string _designvalue = "";
            bool _set_dbtype = false;
            bool _set_length = false;
            bool _set_precision = false;
            bool _set_scale = false;
            int _length = 0;
            byte _precision = 0;
            byte _scale = 0;

            _name = paramNode.Attributes["name"].Value;

            string provider_type_string = paramNode.Attributes["providertype"].Value;

            MetaTypeRepository meta = new MetaTypeRepository();

            meta.SqlTypeNameToDbType(provider_type_string, out DbType meta_dbtype, out Type meta_frameworktype);

            _dbtype_string = "DbType." + meta_dbtype.ToString();
            _fulltypename = meta_frameworktype.FullName;

            if (paramNode.Attributes["input"] != null)
                _input = paramNode.Attributes["input"].Value == "yes";

            if (paramNode.Attributes["output"] != null)
                _output = paramNode.Attributes["output"].Value == "yes";

            if (paramNode.Attributes["designvalue"] != null)
                _designvalue = paramNode.Attributes["designvalue"].Value.Trim();
            else if (paramNode.Attributes["testvalue0"] != null) /* the 3 testvalues are replaced by 1 designvalue */
                _designvalue = paramNode.Attributes["testvalue0"].Value.Trim();

            string length_string = "";
            string precision_string = "";
            string scale_string = "";

            // Trim the string values:

            if (paramNode.Attributes["length"] != null)
                length_string = paramNode.Attributes["length"].Value.Trim();

            if (length_string == "MAX")
                length_string = "-1";

            if (paramNode.Attributes["precision"] != null)
                precision_string = paramNode.Attributes["precision"].Value.Trim();

            if (paramNode.Attributes["scale"] != null)
                scale_string = paramNode.Attributes["scale"].Value.Trim();

            // The new 'do set' switches:

            if (length_string.Length > 0)
                _set_length = true;

            if (precision_string.Length > 0)
                _set_precision = true;

            if (scale_string.Length > 0)
                _set_scale = true;

            if (_output == true || _set_length == true || _set_precision == true || _set_scale == true)
                _set_dbtype = true;

            // Convert to the new numeric type:

            if (int.TryParse(length_string, out int temp_length))
                _length = temp_length;

            if (byte.TryParse(precision_string, out byte temp_precision))
                _precision = temp_precision;

            if (byte.TryParse(scale_string, out byte temp_scale))
                _scale = temp_scale;

            // Create the new ParameterItem
            ParameterItem item = new ParameterItem(project);

            item.Name = _name;
            item.DbTypeString = _dbtype_string;
            item.FullTypename = _fulltypename;
            item.Input = _input;
            item.Output = _output;
            item.DesignValue = _designvalue;
            item.SetDbType = _set_dbtype;
            item.SetLength = _set_length;
            item.SetPrecision = _set_precision;
            item.SetScale = _set_scale;
            item.Length = _length;
            item.Precision = _precision;
            item.Scale = _scale;

            return item;
        }

    }
}
