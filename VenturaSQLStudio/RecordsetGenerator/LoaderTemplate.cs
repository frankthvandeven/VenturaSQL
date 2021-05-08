using System;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using VenturaSQL;
using VenturaSQLStudio.Ado;
using VenturaSQLStudio.Helpers; 

namespace VenturaSQLStudio {
    internal class LoaderTemplate
    {
        private const string CRLF = "\r\n";
        private const string TAB = "\t";
        private const string QUOTE = "\"";

        private RecordsetItem _recordsetItem;
        private QueryInfo _queryinfo;

        private LoaderTemplateMode _templatemode;

        private int _updateableResultsetsCount;

        private VenturaSqlSchema _parameterschema;
        private List<VenturaSqlColumn> _inputparameters;
        private List<VenturaSqlColumn> _outputparameters;

        private DateTime _timestamp;

        /// <summary>
        /// Generate a recordset with incremental loading support, with the IRecordsetIncremental interface.
        /// </summary>
        //private bool _incremental = false;

        internal LoaderTemplate(LoaderTemplateMode templatemode, RecordsetItem recordsetitem, DateTime timestamp, QueryInfo query_info)
        {
            //if (query_info.SqlScriptContainsRowOffsetAndRowLimitParameters())
            //    if (query_info.ResultSets.Count == 1)
            //        _incremental = true;

            _templatemode = templatemode;

            _recordsetItem = recordsetitem;

            _timestamp = timestamp;

            _parameterschema = recordsetitem.Parameters.AsVenturaSqlSchema();

            _inputparameters = new List<VenturaSqlColumn>();
            _outputparameters = new List<VenturaSqlColumn>();

            foreach (VenturaSqlColumn parameter in _parameterschema)
            {
                if (parameter.Input)
                    _inputparameters.Add(parameter);

                if (parameter.Output)
                    _outputparameters.Add(parameter);
            }

            _queryinfo = query_info;

            _updateableResultsetsCount = 0;

            for (int i = 0; i < _queryinfo.ResultSets.Count; i++)
            {
                ResultsetItem item = recordsetitem.Resultsets[i];
                if (item.UpdateableTableName != null)
                    _updateableResultsetsCount++;
            }

        }

        internal void StandAloneLoaderClass(StringBuilder sb, bool AdoDirectSwitch, VenturaSqlPlatform generatortarget)
        {
            sb.Append(TAB + @"/// <summary>" + CRLF);

            if (_queryinfo.ResultSets.Count == 0)
            {
                sb.Append(TAB + "/// This recordset contains no resultsets." + CRLF);
            }
            else
            {
                sb.Append(TAB + $"/// This recordset contains {_queryinfo.ResultSets.Count} resultsets." + CRLF);
            }

            if (StringTools.AllLinesAreEmpty(_recordsetItem.ClassSummary) == false)
            {
                SmartSplit split = new SmartSplit();
                split.FirstLinePrefix = TAB + "/// ";
                split.OtherLinePrefix = TAB + "/// ";
                split.LinesToSplit = StringTools.StripCRLFAndSpaces(_recordsetItem.ClassSummary);
                split.ExecSplit(sb);
            }

            sb.Append(TAB + @"/// </summary>" + CRLF);

            sb.Append(TAB + $"public partial class {_recordsetItem.ClassName} : IRecordsetBase" + CRLF);
            sb.Append(TAB + "{" + CRLF);

            this.DeclarePrivates(sb, AdoDirectSwitch);

            for (int x = 0; x < _queryinfo.ResultSets.Count; x++)
            {
                string resultsetname = _recordsetItem.Resultsets[x].ResultsetName;
                sb.Append(TAB + TAB + $"private Multi{resultsetname} _resultset{x + 1};" + CRLF);
            }

            sb.Append(CRLF);

            sb.Append(TAB + TAB + "public " + _recordsetItem.ClassName + "()" + CRLF + TAB + TAB + "{" + CRLF);

            this.InsertInConstructor(sb, AdoDirectSwitch);

            sb.Append(TAB + TAB + "}" + CRLF + CRLF); // end of constructor

            this.InsertPropertiesAndMethods(sb, AdoDirectSwitch, generatortarget);

            for (int x = 0; x < _queryinfo.ResultSets.Count; x++)
            {
                string resultsetname = _recordsetItem.Resultsets[x].ResultsetName;
                sb.Append(TAB + TAB + $"public Multi{resultsetname} {resultsetname}" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + $"get {{ return _resultset{x + 1}; }}" + CRLF);
                sb.Append(TAB + TAB + "}" + CRLF + CRLF);
            }

            // Begin: insert Recordsets
            for (int i = 0; i < _queryinfo.ResultSets.Count; i++)
            {
                ResultSetInfo info = _queryinfo.ResultSets[i];
                ResultsetItem item = _recordsetItem.Resultsets[i];
                RecordsetTemplate template = new RecordsetTemplate(_recordsetItem, item, info, _queryinfo);

                string resultset_typename = $"Multi{item.ResultsetName}";
                string record_typename = $"Multi{item.ResultsetName}Record";

                template.GenerateCSharp(sb, AdoDirectSwitch, generatortarget, null, resultset_typename, record_typename);
            }
            // End: insert Recordsets

            sb.Append(TAB + "}" + CRLF); // end of class
        }

        internal void DeclarePrivates(StringBuilder sb, bool AdoDirectSwitch)
        {
            if (_recordsetItem.RowloadIncremental)
            {
                sb.Append(TAB + TAB + "private bool _has_more_rows = false;" + CRLF);
                sb.Append(TAB + TAB + "private Connector _incremental_connector = null;" + CRLF);
                sb.Append(TAB + TAB + "private int _incremental_offset = 0;" + CRLF);
                sb.Append(TAB + TAB + "private int _last_exec_startindex = -1;" + CRLF);
                sb.Append(TAB + TAB + "private int _last_exec_count = 0;" + CRLF);
            }

            sb.Append(TAB + TAB + "private IResultsetBase[] _resultsets;" + CRLF);

            if (AdoDirectSwitch == true)
                sb.Append(TAB + TAB + "private string _sqlscript;" + CRLF);

            if (_inputparameters.Count > 0)
                sb.Append(TAB + TAB + "private object[] _inputparametervalues;" + CRLF);

            if (_outputparameters.Count > 0)
                sb.Append(TAB + TAB + "private object[] _outputparametervalues;" + CRLF);

            if (_inputparameters.Count > 0)
                sb.Append(TAB + TAB + "private InputParamHolder _inputparamholder;" + CRLF);

            if (_outputparameters.Count > 0)
                sb.Append(TAB + TAB + "private OutputParamHolder _outputparamholder;" + CRLF);

            if (_parameterschema.Count > 0)
                sb.Append(TAB + TAB + "private VenturaSqlSchema _parameterschema;" + CRLF);

            sb.Append(TAB + TAB + "private int _rowlimit = 500;" + CRLF);
            sb.Append(TAB + TAB + @"private const string CRLF = ""\r\n"";" + CRLF);
            sb.Append(CRLF);
        }

        internal void InsertInConstructor(StringBuilder sb, bool AdoDirectSwitch)
        {
            if (_templatemode == LoaderTemplateMode.StandAlone)
            {
                for (int i = 0; i < _queryinfo.ResultSets.Count; i++)
                {
                    string resultsetname = _recordsetItem.Resultsets[i].ResultsetName;
                    sb.Append(TAB + TAB + TAB + $"_resultset{i + 1} = new Multi{resultsetname}();" + CRLF);
                }
                sb.Append(CRLF);

                sb.Append(TAB + TAB + TAB + "_resultsets = new IResultsetBase[] { ");

                for (int i = 0; i < _queryinfo.ResultSets.Count; i++)
                {
                    if (i > 0)
                        sb.Append(", ");

                    sb.Append($"_resultset{i + 1}");
                }

                sb.Append(" };" + CRLF);
            }
            else if (_templatemode == LoaderTemplateMode.InjectInsideRecordset)
            {
                sb.Append(TAB + TAB + TAB + "_resultsets = new IResultsetBase[] { this };" + CRLF);
            }

            sb.Append(CRLF);

            if (AdoDirectSwitch == true)
            {
                // Begin: split up a multi line sql script
                List<string> lines = new List<string>();
                StringReader strReader = new StringReader(_recordsetItem.SqlScript);

                while (true)
                {
                    string line = strReader.ReadLine();

                    if (line == null) break;

                    if (line.Trim().Length > 0)
                        lines.Add(line);
                }

                for (int x = 0; x < lines.Count; x++)
                {
                    string line = lines[x];

                    if (x == 0)
                        sb.Append(TAB + TAB + TAB + "_sqlscript = @\"" + line + "\"");
                    else
                        sb.Append(TAB + TAB + TAB + "             @\"" + line + "\"");

                    if (x < (lines.Count - 1))
                        sb.Append(" + CRLF +" + CRLF);
                    else
                        sb.Append(";" + CRLF);
                }

            }

            sb.Append(CRLF);
            // End: split up a multi line sql script

            // Begin: init parameter array
            if (_inputparameters.Count > 0)
                sb.Append(TAB + TAB + TAB + $"_inputparametervalues = new object[{_parameterschema.Count}];" + CRLF);

            if (_outputparameters.Count > 0)
                sb.Append(TAB + TAB + TAB + $"_outputparametervalues = new object[{_parameterschema.Count}];" + CRLF);
            // End: init parameter array

            if (_inputparameters.Count > 0)
                sb.Append(TAB + TAB + TAB + $"_inputparamholder = new InputParamHolder(_inputparametervalues);" + CRLF);

            if (_outputparameters.Count > 0)
                sb.Append(TAB + TAB + TAB + $"_outputparamholder = new OutputParamHolder(_outputparametervalues);" + CRLF);

            if (_inputparameters.Count > 0 || _outputparameters.Count > 0)
                sb.Append(CRLF);

            // Begin: Define parameter schema
            if (_parameterschema.Count > 0)
            {
                sb.Append(TAB + TAB + TAB + "ColumnArrayBuilder param_array = new ColumnArrayBuilder();" + CRLF + CRLF);
                foreach (VenturaSqlColumn parameter in _parameterschema)
                {
                    string columnsize = "null";
                    string precision = "null";
                    string scale = "null";

                    if (parameter.ColumnSize != null)
                        columnsize = parameter.ColumnSize.ToString();

                    if (parameter.NumericPrecision != null)
                        precision = parameter.NumericPrecision.ToString();

                    if (parameter.NumericScale != null)
                        scale = parameter.NumericScale.ToString();

                    sb.Append(TAB + TAB + TAB);
                    sb.Append($"param_array.AddParameterColumn(\"{parameter.ColumnName}\", typeof({parameter.ShortTypeNameAsCSharpString()}), {(parameter.Input ? "true" : "false")}, {(parameter.Output ? "true" : "false")}, DbType.{parameter.DbType}, {columnsize}, {precision}, {scale});");
                    sb.Append(CRLF);
                }

                sb.Append(CRLF);
                sb.Append(TAB + TAB + TAB + "_parameterschema = new VenturaSqlSchema(param_array);" + CRLF + CRLF);
            }
            // End: Define parameter schema

        }

        internal void InsertPropertiesAndMethods(StringBuilder sb, bool AdoDirectSwitch, VenturaSqlPlatform generatortarget)
        {

            // The hash is calculated from all schema columns plus the Sql script.
            // So when a Sql script or the schema for the query result columns change, the hash changes too.
            byte[] hash = this.CalculateHash();

            // Generate a literal byte array definition.
            StringBuilder HashLiteral = new StringBuilder(2048);
            HashLiteral.Append("new byte[] { ");

            for (int x = 0; x < hash.Length; x++)
            {
                HashLiteral.Append(hash[x].ToString());
                if (x < (hash.Length - 1)) HashLiteral.Append(", ");
            }

            HashLiteral.Append(" }");

            sb.Append(TAB + TAB + "byte[] IRecordsetBase.Hash" + CRLF); // was public
            sb.Append(TAB + TAB + "{" + CRLF);
            sb.Append(TAB + TAB + TAB + "get { return " + HashLiteral.ToString() + "; }" + CRLF);
            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(TAB + TAB + "string IRecordsetBase.HashString" + CRLF); // was public
            sb.Append(TAB + TAB + "{" + CRLF);
            sb.Append(TAB + TAB + TAB + "get { return \"" + this.ToHexadecimalString(hash) + "\"; }" + CRLF);
            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(TAB + TAB + "VenturaSqlPlatform IRecordsetBase.GeneratorTarget" + CRLF);
            sb.Append(TAB + TAB + "{" + CRLF);
            sb.Append(TAB + TAB + TAB + $"get {{ return VenturaSqlPlatform.{generatortarget}; }}" + CRLF);
            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            Version studio_version = MainWindow.ViewModel.VenturaSqlVersion;

            sb.Append(TAB + TAB + "Version IRecordsetBase.GeneratorVersion" + CRLF);
            sb.Append(TAB + TAB + "{" + CRLF);
            sb.Append(TAB + TAB + TAB + $"get {{ return new Version({studio_version.Major},{studio_version.Minor},{studio_version.Build}); }}" + CRLF);
            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(TAB + TAB + "DateTime IRecordsetBase.GeneratorTimestamp" + CRLF);
            sb.Append(TAB + TAB + "{" + CRLF);
            sb.Append(TAB + TAB + TAB + $"get {{ return new DateTime({_timestamp.Year}, {_timestamp.Month}, {_timestamp.Day}, {_timestamp.Hour}, {_timestamp.Minute}, {_timestamp.Second}); }} // {_timestamp.ToLongDateString()} at {_timestamp.ToLongTimeString()}" + CRLF);

            sb.Append(TAB + TAB + "}" + CRLF + CRLF);


            sb.Append(TAB + TAB + "string IRecordsetBase.GeneratorProviderInvariantName" + CRLF); // was public
            sb.Append(TAB + TAB + "{" + CRLF);

            if (AdoDirectSwitch == true)
                sb.Append(TAB + TAB + TAB + "get { return \"" + MainWindow.ViewModel.CurrentProject.ProviderInvariantName + "\"; }" + CRLF);
            else
                sb.Append(TAB + TAB + TAB + "get { return null; }" + CRLF);

            sb.Append(TAB + TAB + "}" + CRLF + CRLF);


            sb.Append(TAB + TAB + "IResultsetBase[] IRecordsetBase.Resultsets" + CRLF); // was public
            sb.Append(TAB + TAB + "{" + CRLF);
            sb.Append(TAB + TAB + TAB + "get { return _resultsets; }" + CRLF);
            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            // InitialConnector
            if (_recordsetItem.RowloadIncremental)
            {
                sb.Append(TAB + TAB + "Connector IRecordsetIncremental.IncrementalConnector" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + "get { return _incremental_connector; }" + CRLF);
                sb.Append(TAB + TAB + TAB + "set { _incremental_connector = value; }" + CRLF);
                sb.Append(TAB + TAB + "}" + CRLF + CRLF);

                sb.Append(TAB + TAB + "int IRecordsetIncremental.IncrementalOffset" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + "get { return _incremental_offset; }" + CRLF);
                sb.Append(TAB + TAB + TAB + "set { _incremental_offset = value; }" + CRLF);
                sb.Append(TAB + TAB + "}" + CRLF + CRLF);


                sb.Append(TAB + TAB + "int IRecordsetIncremental.LastExecCount" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + "get { return _last_exec_count; }" + CRLF);
                sb.Append(TAB + TAB + TAB + "set { _last_exec_count = value; }" + CRLF);
                sb.Append(TAB + TAB + "}" + CRLF + CRLF);

                sb.Append(TAB + TAB + "int IRecordsetIncremental.LastExecStartIndex" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + "get { return _last_exec_startindex; }" + CRLF);
                sb.Append(TAB + TAB + TAB + "set { _last_exec_startindex = value; }" + CRLF);
                sb.Append(TAB + TAB + "}" + CRLF + CRLF);

                sb.Append(TAB + TAB + "bool IRecordsetIncremental.HasMoreRows" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + "get { return _has_more_rows; }" + CRLF);
                sb.Append(TAB + TAB + TAB + "set { _has_more_rows = value; }" + CRLF);
                sb.Append(TAB + TAB + "}" + CRLF + CRLF);

                sb.Append(TAB + TAB + "bool HasMoreRows" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + "get { return _has_more_rows; }" + CRLF);
                sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            }

            sb.Append(TAB + TAB + "string IRecordsetBase.SqlScript" + CRLF); // was public
            sb.Append(TAB + TAB + "{" + CRLF);

            if (AdoDirectSwitch == true)
                sb.Append(TAB + TAB + TAB + "get { return _sqlscript; }" + CRLF);
            else
                sb.Append(TAB + TAB + TAB + "get { return null; }" + CRLF);

            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(TAB + TAB + "VenturaSqlSchema IRecordsetBase.ParameterSchema" + CRLF); // was public
            sb.Append(TAB + TAB + "{" + CRLF);
            sb.Append(TAB + TAB + TAB + "get { ");

            if (_parameterschema.Count > 0)
                sb.Append("return _parameterschema;");
            else
                sb.Append("return null;");

            sb.Append(" }" + CRLF);
            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(TAB + TAB + "/// <summary>" + CRLF);
            sb.Append(TAB + TAB + "/// For internal use by VenturaSQL only. Use SetExecSqlParams() instead." + CRLF);
            sb.Append(TAB + TAB + "/// </summary>" + CRLF);
            sb.Append(TAB + TAB + "object[] IRecordsetBase.InputParameterValues" + CRLF); // was public
            sb.Append(TAB + TAB + "{" + CRLF);

            if (_inputparameters.Count > 0)
                sb.Append(TAB + TAB + TAB + "get { return _inputparametervalues; }" + CRLF);
            else
                sb.Append(TAB + TAB + TAB + "get { return null; }" + CRLF);

            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(TAB + TAB + "/// <summary>" + CRLF);
            sb.Append(TAB + TAB + "/// For internal use by VenturaSQL only. Use Output property instead." + CRLF);
            sb.Append(TAB + TAB + "/// </summary>" + CRLF);
            sb.Append(TAB + TAB + "object[] IRecordsetBase.OutputParameterValues" + CRLF); // was public
            sb.Append(TAB + TAB + "{" + CRLF);

            if (_outputparameters.Count > 0)
                sb.Append(TAB + TAB + TAB + "get { return _outputparametervalues; }" + CRLF);
            else
                sb.Append(TAB + TAB + TAB + "get { return null; }" + CRLF);

            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            if (_inputparameters.Count > 0)
            {
                sb.Append(TAB + TAB + "public InputParamHolder InputParam" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + "get { return _inputparamholder; }" + CRLF);
                sb.Append(TAB + TAB + "}" + CRLF + CRLF);
            }

            if (_outputparameters.Count > 0)
            {
                sb.Append(TAB + TAB + "public OutputParamHolder OutputParam" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + "get { return _outputparamholder; }" + CRLF);
                sb.Append(TAB + TAB + "}" + CRLF + CRLF);
            }

            sb.Append(TAB + TAB + "public int RowLimit" + CRLF);
            sb.Append(TAB + TAB + "{" + CRLF);
            sb.Append(TAB + TAB + TAB + "get { return _rowlimit; }" + CRLF);
            sb.Append(TAB + TAB + TAB + "set { _rowlimit = value; }" + CRLF);
            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            // Begin: SetExecSqlParams function
            if (_inputparameters.Count > 0)
            {
                sb.Append(TAB + TAB + "public void SetExecSqlParams(");
                for (int x = 0; x < _inputparameters.Count; x++)
                {
                    VenturaSqlColumn parameter = _inputparameters[x];
                    sb.Append(parameter.ShortTypeNameForColumnProperty() + " " + parameter.ColumnNameWithoutPrefix());
                    if (x < (_inputparameters.Count - 1))
                        sb.Append(", ");
                }
                sb.Append(")" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);

                for (int x = 0; x < _inputparameters.Count; x++)
                {
                    VenturaSqlColumn parameter = _inputparameters[x];
                    sb.Append(TAB + TAB + TAB + $"_inputparametervalues[{parameter.ColumnOrdinal}] = {parameter.ColumnNameWithoutPrefix()};" + CRLF);
                }
                sb.Append(TAB + TAB + "}" + CRLF + CRLF);
            }
            // End: SetExecSqlParams function 

            GenerateExecSql(sb, CodeStyle.Synchronous, generatortarget);
            GenerateExecSql(sb, CodeStyle.Async, generatortarget);

            if (_updateableResultsetsCount > 0)
            {
                //if (generatortarget == VenturaSqlPlatform.AspNet || generatortarget == VenturaSqlPlatform.WPF || generatortarget == VenturaSqlPlatform.WinForms)
                GenerateSaveChanges(sb, CodeStyle.Synchronous);
                GenerateSaveChanges(sb, CodeStyle.Async);
            }

            if (_inputparameters.Count > 0)
            {
                sb.Append(TAB + TAB + "public class InputParamHolder" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);

                sb.Append(TAB + TAB + TAB + "private object[] _values;" + CRLF);
                sb.Append(CRLF);
                sb.Append(TAB + TAB + TAB + "public InputParamHolder(object[] values)" + CRLF);
                sb.Append(TAB + TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + TAB + "_values = values;" + CRLF);
                sb.Append(TAB + TAB + TAB + "}" + CRLF + CRLF);

                foreach (VenturaSqlColumn parameter in _inputparameters)
                {
                    sb.Append(TAB + TAB + TAB + "public " + parameter.ShortTypeNameForColumnProperty() + " " + parameter.ColumnNameWithoutPrefix() + CRLF);
                    sb.Append(TAB + TAB + TAB + "{" + CRLF);
                    sb.Append(TAB + TAB + TAB + TAB + $"get {{ return ({parameter.ShortTypeNameForColumnProperty()})_values[{parameter.ColumnOrdinal}]; }}" + CRLF);
                    sb.Append(TAB + TAB + TAB + TAB + $"set {{ _values[{parameter.ColumnOrdinal}] = value; }}" + CRLF);
                    sb.Append(TAB + TAB + TAB + "}" + CRLF + CRLF);
                }

                sb.Append(TAB + TAB + "}" + CRLF + CRLF);
            }

            if (_outputparameters.Count > 0)
            {
                sb.Append(TAB + TAB + "public class OutputParamHolder" + CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);

                sb.Append(TAB + TAB + TAB + "private object[] _values;" + CRLF);
                sb.Append(CRLF);
                sb.Append(TAB + TAB + TAB + "public OutputParamHolder(object[] values)" + CRLF);
                sb.Append(TAB + TAB + TAB + "{" + CRLF);
                sb.Append(TAB + TAB + TAB + TAB + "_values = values;" + CRLF);
                sb.Append(TAB + TAB + TAB + "}" + CRLF + CRLF);

                foreach (VenturaSqlColumn parameter in _outputparameters)
                {
                    sb.Append(TAB + TAB + TAB + "public " + parameter.ShortTypeNameForColumnProperty() + " " + parameter.ColumnNameWithoutPrefix() + CRLF);
                    sb.Append(TAB + TAB + TAB + "{" + CRLF);
                    sb.Append(TAB + TAB + TAB + TAB + $"get {{ return ({parameter.ShortTypeNameForColumnProperty()})_values[{parameter.ColumnOrdinal}]; }}" + CRLF);
                    sb.Append(TAB + TAB + TAB + "}" + CRLF + CRLF);
                }

                sb.Append(TAB + TAB + "}" + CRLF + CRLF);
            }

        }

        private enum CodeStyle
        {
            Synchronous = 0,
            Async = 1
        }

        private void GenerateExecSql(StringBuilder sb, CodeStyle codestyle, VenturaSqlPlatform generatortarget)
        {
            if (codestyle == CodeStyle.Synchronous)
                sb.Append(TAB + TAB + "public void ExecSql(");
            else
                sb.Append(TAB + TAB + "public async Task ExecSqlAsync(");

            for (int x = 0; x < _inputparameters.Count; x++)
            {
                if (x > 0)
                    sb.Append(", ");

                VenturaSqlColumn parameter = _inputparameters[x];
                sb.Append(parameter.ShortTypeNameForColumnProperty() + " " + parameter.ColumnNameWithoutPrefix());
            }
            sb.Append(")" + CRLF);
            sb.Append(TAB + TAB + "{" + CRLF);

            for (int x = 0; x < _inputparameters.Count; x++)
            {
                VenturaSqlColumn parameter = _inputparameters[x];
                sb.Append(TAB + TAB + TAB + $"_inputparametervalues[{parameter.ColumnOrdinal}] = {parameter.ColumnNameWithoutPrefix()};" + CRLF);
            }

            if (codestyle == CodeStyle.Synchronous)
                sb.Append(TAB + TAB + TAB + "Transactional.ExecSql(VenturaSqlConfig.DefaultConnector, new IRecordsetBase[] { this });" + CRLF);
            else
                sb.Append(TAB + TAB + TAB + "await Transactional.ExecSqlAsync(VenturaSqlConfig.DefaultConnector, new IRecordsetBase[] { this });" + CRLF);

            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            ///

            if (codestyle == CodeStyle.Synchronous)
                sb.Append(TAB + TAB + "public void ExecSql(Connector connector");
            else
                sb.Append(TAB + TAB + "public async Task ExecSqlAsync(Connector connector");

            for (int x = 0; x < _inputparameters.Count; x++)
            {
                sb.Append(", ");

                VenturaSqlColumn parameter = _inputparameters[x];
                sb.Append(parameter.ShortTypeNameForColumnProperty() + " " + parameter.ColumnNameWithoutPrefix());
            }
            sb.Append(")" + CRLF);
            sb.Append(TAB + TAB + "{" + CRLF);

            for (int x = 0; x < _inputparameters.Count; x++)
            {
                VenturaSqlColumn parameter = _inputparameters[x];
                sb.Append(TAB + TAB + TAB + $"_inputparametervalues[{parameter.ColumnOrdinal}] = {parameter.ColumnNameWithoutPrefix()};" + CRLF);
            }

            if (codestyle == CodeStyle.Synchronous)
                sb.Append(TAB + TAB + TAB + "Transactional.ExecSql(connector, new IRecordsetBase[] { this });" + CRLF);
            else
                sb.Append(TAB + TAB + TAB + "await Transactional.ExecSqlAsync(connector, new IRecordsetBase[] { this });" + CRLF);

            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            if (_recordsetItem.RowloadIncremental)
            {
                if (codestyle == CodeStyle.Synchronous)
                    sb.Append(TAB + TAB + "public void ExecSqlIncremental()");
                else
                    sb.Append(TAB + TAB + "public async Task ExecSqlIncrementalAsync()");

                sb.Append(CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);

                if (codestyle == CodeStyle.Synchronous)
                    sb.Append(TAB + TAB + TAB + "Transactional.ExecSqlIncremental(this);" + CRLF);
                else
                    sb.Append(TAB + TAB + TAB + "await Transactional.ExecSqlIncrementalAsync(this);" + CRLF);

                sb.Append(TAB + TAB + "}" + CRLF + CRLF);

                if (codestyle == CodeStyle.Synchronous)
                    sb.Append(TAB + TAB + "public void ExecSqlNextPage()");
                else
                    sb.Append(TAB + TAB + "public async Task ExecSqlNextPageAsync()");

                sb.Append(CRLF);
                sb.Append(TAB + TAB + "{" + CRLF);

                if (codestyle == CodeStyle.Synchronous)
                    sb.Append(TAB + TAB + TAB + "Transactional.ExecSqlNextPage(this);" + CRLF);
                else
                    sb.Append(TAB + TAB + TAB + "await Transactional.ExecSqlNextPageAsync(this);" + CRLF);

                sb.Append(TAB + TAB + "}" + CRLF + CRLF);

            }

        }

        private void GenerateSaveChanges(StringBuilder sb, CodeStyle codestyle)
        {

            if (codestyle == CodeStyle.Synchronous)
                sb.Append(TAB + TAB + "public void SaveChanges()" + CRLF);
            else
                sb.Append(TAB + TAB + "public async Task SaveChangesAsync()" + CRLF);

            sb.Append(TAB + TAB + "{" + CRLF);

            if (codestyle == CodeStyle.Synchronous)
                sb.Append(TAB + TAB + TAB + "Transactional.SaveChanges(VenturaSqlConfig.DefaultConnector, new IRecordsetBase[] { this });" + CRLF);
            else
                sb.Append(TAB + TAB + TAB + "await Transactional.SaveChangesAsync(VenturaSqlConfig.DefaultConnector, new IRecordsetBase[] { this });" + CRLF);

            sb.Append(TAB + TAB + "}" + CRLF + CRLF);


            if (codestyle == CodeStyle.Synchronous)
                sb.Append(TAB + TAB + "public void SaveChanges(Connector connector)" + CRLF);
            else
                sb.Append(TAB + TAB + "public async Task SaveChangesAsync(Connector connector)" + CRLF);

            sb.Append(TAB + TAB + "{" + CRLF);

            if (codestyle == CodeStyle.Synchronous)
                sb.Append(TAB + TAB + TAB + "Transactional.SaveChanges(connector, new IRecordsetBase[] { this });" + CRLF);
            else
                sb.Append(TAB + TAB + TAB + "await Transactional.SaveChangesAsync(connector, new IRecordsetBase[] { this });" + CRLF);

            sb.Append(TAB + TAB + "}" + CRLF + CRLF);

        }

        /// <summary>
        /// The hash is meant as a unique signature. It consists of:
        /// a) Sql script b) The columns of each resultset
        /// </summary>
        private byte[] CalculateHash()
        {
            MemoryStream ms = new MemoryStream(8192);
            BinaryWriter bw = new BinaryWriter(ms);

            if (_recordsetItem.SqlScript != null)
            {
                bw.Write(_recordsetItem.SqlScript);
            }

            for (int x = 0; x < _queryinfo.ResultSets.Count; x++)
            {
                ColumnArrayBuilder column_array_builder = new ColumnArrayBuilder();

                column_array_builder.Add(_queryinfo.ResultSets[x], _recordsetItem.Resultsets[x].UpdateableTableName);

                // Add the UDC's (user defined columns) to the main-schema
                foreach (UDCItem udcitem in _recordsetItem.UserDefinedColumns)
                {
                    bw.Write(udcitem.ColumnName);
                    bw.Write(udcitem.FullTypename);
                }

                VenturaSqlSchema schema = new VenturaSqlSchema(column_array_builder);
                schema.WriteSchemaToStream(ms);
            }

            MD5CryptoServiceProvider md5 = new MD5CryptoServiceProvider();

            ms.Position = 0; // DO NOT FORGET!

            return md5.ComputeHash(ms);
        }

        private string ToHexadecimalString(byte[] bytes)
        {
            string hexString = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                hexString += bytes[i].ToString("X2");
            }
            return hexString;
        }


    }

    internal enum LoaderTemplateMode
    {
        StandAlone,
        InjectInsideRecordset
    }

}
