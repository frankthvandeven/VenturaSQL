using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Xml;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio.ProjectActions
{
    internal class SaveToFile
    {

        internal void Save(Project project, string filename)
        {
            Action action = () =>
            {

                using (MemoryStream memorystream = new MemoryStream())
                {
                    XmlTextWriter writer = new XmlTextWriter(memorystream, Encoding.UTF8);

                    writer.Formatting = Formatting.Indented;

                    writer.WriteStartDocument();
                    writer.WriteComment("Do not edit this document manually, or at least make a backup before doing so.");

                    writer.WriteStartElement("project");
                    writer.WriteAttributeString("writerversion", MainWindow.ViewModel.VenturaSqlVersion.ToString());

                    writer.WriteStartElement("inputdatabase");
                    writer.WriteAttributeString("providerinvariantname", project.ProviderInvariantName);
                    writer.WriteAttributeString("connectstring", project.ConnectionString);
                    writer.WriteEndElement();

                    writer.WriteStartElement("providersettings");
                    writer.WriteAttributeString("includeservername", project.AdvancedSettings.IncludeServerName ? "yes" : "no");
                    writer.WriteAttributeString("includecatalogname", project.AdvancedSettings.IncludeCatalogName ? "yes" : "no");
                    writer.WriteAttributeString("includeschemaname", project.AdvancedSettings.IncludeSchemaName ? "yes" : "no");
                    writer.WriteAttributeString("columnserver", project.AdvancedSettings.ColumnServer);
                    writer.WriteAttributeString("columncatalog", project.AdvancedSettings.ColumnCatalog);
                    writer.WriteAttributeString("columnschema", project.AdvancedSettings.ColumnSchema);
                    writer.WriteAttributeString("columntable", project.AdvancedSettings.ColumnTable);
                    writer.WriteAttributeString("columntype", project.AdvancedSettings.ColumnType);
                    writer.WriteEndElement();

                    for (int i = 0; i < project.VisualStudioProjects.Count; i++)
                    {
                        VisualStudioProjectItem vs_project = project.VisualStudioProjects[i];

                        writer.WriteStartElement($"outputproject{i + 1}");
                        writer.WriteAttributeString("enabled", vs_project.ProjectEnabled ? "yes" : "no");
                        writer.WriteAttributeString("targetplatform", vs_project.TargetPlatform);
                        writer.WriteAttributeString("generatedirectadoconnectioncode", vs_project.GenerateDirectAdoConnectionCode ? "yes" : "no");
                        writer.WriteString(vs_project.OutputProjectFilename);
                        writer.WriteEndElement();
                    }


                    writer.WriteStartElement("autocreate");
                    writer.WriteAttributeString("enabled", project.AutoCreateSettings.Enabled ? "yes" : "no");
                    writer.WriteAttributeString("folder", project.AutoCreateSettings.Folder);
                    writer.WriteAttributeString("getall", project.AutoCreateSettings.CreateGetAll ? "yes" : "no");
                    writer.WriteAttributeString("incremental", project.AutoCreateSettings.CreateIncremental ? "yes" : "no");

                    foreach (TableName tablename in project.AutoCreateSettings.ExcludedTablenames)
                    {
                        writer.WriteStartElement("excludedtablename");
                        writer.WriteAttributeString("server", tablename.BaseServerName);
                        writer.WriteAttributeString("catalog", tablename.BaseCatalogName);
                        writer.WriteAttributeString("schema", tablename.BaseSchemaName);
                        writer.WriteAttributeString("table", tablename.BaseTableName);
                        writer.WriteEndElement();
                    }

                    writer.WriteEndElement(); // autocreate

                    writer.WriteStartElement("folderstructure");

                    // sample FolderListItem: OutputFolder: "Ventura\Generated", FolderItem: The folder item that is part of the viewmodel for the TreeView
                    List<RootItem.FolderListItem> pathlist = project.FolderStructure.GetFolderList();

                    foreach (RootItem.FolderListItem pathlistitem in pathlist)
                    {
                        writer.WriteStartElement("folder");
                        writer.WriteAttributeString("selected", pathlistitem.FolderItem.IsSelected ? "yes" : "no");
                        writer.WriteAttributeString("expanded", pathlistitem.FolderItem.IsExpanded ? "yes" : "no");
                        writer.WriteString(pathlistitem.OutputFolder);
                        writer.WriteEndElement(); // folder
                    }

                    writer.WriteEndElement(); // folderstructure


                    writer.WriteStartElement("projectitems");

                    foreach (RootItem.FolderListItem pathlistitem in pathlist)
                        foreach (ITreeViewItem projectitem in pathlistitem.FolderItem.Children.Where(child => child.ItemKind != TreeViewModelKind.FolderItem))
                        {
                            if (projectitem is RecordsetItem)
                                _saveRecordsetDefinition(writer, (RecordsetItem)projectitem, pathlistitem.OutputFolder);
                        }

                    writer.WriteEndElement(); // projectitems

                    writer.WriteEndElement();
                    writer.WriteEndDocument();

                    writer.Flush();

                    memorystream.Position = 0;

                    // The actual writing to disk.
                    using (FileStream filestream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Write))
                    {
                        filestream.SetLength(0);

                        memorystream.CopyTo(filestream);
                    } // end of using filestream
                } // end of using memorystream

                project.ResetModified();
            }; // end of Action block

            Application.Current.Dispatcher.Invoke(action);

        }

        private void _saveRecordsetDefinition(XmlWriter writer, RecordsetItem definition, string outputfolder)
        {
            writer.WriteStartElement("recordsetdefinition");
            writer.WriteAttributeString("classname", definition.ClassName);
            writer.WriteAttributeString("enabled", definition.Enabled ? "yes" : "no");

            // Selected means "currently selected (focussed) item" in the TreeView
            writer.WriteAttributeString("selected", definition.IsSelected ? "yes" : "no");

            for (int i = 0; i < definition.OutputProjects.Count; i++)
            {
                bool selected = definition.OutputProjects[i].Selected;
                writer.WriteElementString($"project{i + 1}", selected ? "yes" : "no");
            }

            writer.WriteElementString("outputfolder", outputfolder);
            writer.WriteElementString("classsummary", definition.ClassSummary);
            writer.WriteElementString("implementdatabinding", definition.ImplementDatabinding ? "yes" : "no");
            writer.WriteElementString("rowloadincremental", definition.RowloadIncremental ? "yes" : "no");
            writer.WriteElementString("sqlscript", definition.SqlScript);

            _saveDefinitionParameters(definition.Parameters, writer);
            _saveResultsetList(definition.Resultsets, writer);
            _saveDefinitionUserDefinedColumns(definition.UserDefinedColumns, writer);

            writer.WriteEndElement(); // recordsetdefinition

        }

        private void _saveDefinitionParameters(ParameterCollection parameters, XmlWriter writer)
        {
            writer.WriteStartElement("parameters");

            foreach (ParameterItem item in parameters)
            {
                writer.WriteStartElement("parameter");
                writer.WriteAttributeString("name", item.Name);
                writer.WriteAttributeString("fulltypename", item.FullTypename);
                writer.WriteAttributeString("input", item.Input ? "yes" : "no");
                writer.WriteAttributeString("output", item.Output ? "yes" : "no");
                writer.WriteAttributeString("designvalue", item.DesignValue);
                writer.WriteAttributeString("setdbtype", item.SetDbType ? "yes" : "no");
                writer.WriteAttributeString("setlength", item.SetLength ? "yes" : "no");
                writer.WriteAttributeString("setprecision", item.SetPrecision ? "yes" : "no");
                writer.WriteAttributeString("setscale", item.SetScale ? "yes" : "no");
                writer.WriteAttributeString("dbtypestring", item.DbTypeString);
                writer.WriteAttributeString("length", item.Length.ToString());
                writer.WriteAttributeString("precision", item.Precision.ToString());
                writer.WriteAttributeString("scale", item.Scale.ToString());
                writer.WriteEndElement(); // parameter
            }

            writer.WriteEndElement(); // parameters
        }

        private void _saveResultsetList(ResultsetCollection resultsets, XmlWriter writer)
        {
            writer.WriteStartElement("resultsets");

            foreach (ResultsetItem item in resultsets)
            {
                writer.WriteStartElement("resultset");
                writer.WriteAttributeString("name", item.ResultsetName);
                writer.WriteAttributeString("enabled", item.Enabled ? "yes" : "no");

                if(item.UpdateableTableName != null)
                {
                    writer.WriteStartElement("updateabletablename");
                    writer.WriteAttributeString("server", item.UpdateableTableName.BaseServerName);
                    writer.WriteAttributeString("catalog", item.UpdateableTableName.BaseCatalogName);
                    writer.WriteAttributeString("schema", item.UpdateableTableName.BaseSchemaName);
                    writer.WriteAttributeString("table", item.UpdateableTableName.BaseTableName);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        private void _saveDefinitionUserDefinedColumns(UDCCollection columns, XmlWriter writer)
        {
            writer.WriteStartElement("userdefinedcolumns");

            foreach (UDCItem item in columns)
            {
                writer.WriteStartElement("userdefinedcolumn");
                writer.WriteAttributeString("name", item.ColumnName);
                writer.WriteAttributeString("fulltypename", item.FullTypename);
                writer.WriteEndElement(); // userdefinedcolumn
            }

            writer.WriteEndElement(); // userdefinedcolumns
        }






    }
}
