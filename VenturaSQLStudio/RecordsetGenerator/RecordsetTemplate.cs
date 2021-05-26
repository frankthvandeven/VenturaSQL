using System.Collections.Generic;
using System.Text;
using VenturaSQL;
using VenturaSQLStudio.Ado;
using VenturaSQLStudio.Helpers;

namespace VenturaSQLStudio {
    internal class RecordsetTemplate
    {
        private const string CRLF = "\r\n";
        private const string TAB = "\t";
        private const string QUOTE = "\"";

        private string PRE = "";

        private RecordsetItem _recordsetItem;
        private ResultsetItem _resultsetItem;
        private ResultSetInfo _resultsetinfo;

        //private QueryInfo _query_info;

        //private bool _incremental = false;

        internal RecordsetTemplate(RecordsetItem recordsetItem, ResultsetItem resultsetitem, ResultSetInfo resultsetinfo, QueryInfo query_info)
        {
            _recordsetItem = recordsetItem;
            _resultsetItem = resultsetitem;
            _resultsetinfo = resultsetinfo;
            //_query_info = query_info;

        }

        /// <summary>
        /// Generates the source code and returns it as a string.
        /// </summary>
        internal void GenerateCSharp(StringBuilder sb, bool AdoDirectSwitch, VenturaSqlPlatform generatortarget, LoaderTemplate loadertemplate, string recordsetname, string recordname)
        {
            #region Prepare
            if (loadertemplate == null)
                PRE = TAB; // insert an extra tab when this recordset is inside a Loader class.

            // Begin: calculate the VenturaSqlSchema

            ColumnArrayBuilder builder = new ColumnArrayBuilder();

            builder.Add(_resultsetinfo, _resultsetItem.UpdateableTableName);

            VenturaSqlSchema schema = new VenturaSqlSchema(builder);

            // End: calculate the VenturaSqlSchema

            // At this point the following must be guaranteed:
            // The _recordsetItem.UpdateableTableName is either empty or is filled and valid, and _updateableTableInfo is null or assigned accordingly.
            //
            // Definition of valid:
            // 1. All primary keys for the updateable table are present in the resultset of the query.
            // 2. The updateable table has 1 or more primary keys.
            //
            // Note that the BaseColumnName refers to the primary key column name in the updateable table,
            // whereas ColumnName might be an alias specific to this resultset.
            //
            // _updateableTableInfo contains info on the updateable table. If null, the Recordset will be generated as readonly.

            TableInfo updateableTableInfo = null;

            if (_resultsetItem.UpdateableTableName != null)
            {
                updateableTableInfo = _resultsetinfo.Tables.Find(a => a.TableName == _resultsetItem.UpdateableTableName);

                if (updateableTableInfo == null)
                    throw new VenturaSqlException($"The selected updateable table {_resultsetItem.UpdateableTableName.ScriptTableName} is not referenced in the resultset of the query.");
            }
            #endregion

            #region Recordset class CSharp code.      
            sb.Append(PRE + TAB + @"/// <summary>" + CRLF);

            if (updateableTableInfo == null)
            {
                sb.Append(PRE + TAB + @"/// The resultset is read-only." + CRLF);
            }
            else
            {
                // The updateable table is guaranteed to have 1 or more primary key columns.
                sb.Append(PRE + TAB + $"/// The updateable table is {updateableTableInfo.TableName.ScriptTableName}. Updateable table column information:" + CRLF);

                int column_count = updateableTableInfo.PrimaryKeys.Count + updateableTableInfo.OtherColumns.Count;
                int present_count = column_count - updateableTableInfo.MissingOtherColumns.Count;

                sb.Append(PRE + TAB + $"/// • {present_count} out of {column_count} table columns are present in the resultset." + CRLF);

                SmartSplit split = new SmartSplit();
                split.FirstLinePrefix = PRE + TAB + "/// • ";
                split.OtherLinePrefix = PRE + TAB + "///   ";

                split.LinesToSplit = $"All primary key columns are present in the resultset: {ReadableColumnInfoString(updateableTableInfo.PrimaryKeys)}.";
                split.ExecSplit(sb);

                //sb.Append(PRE + TAB + $"/// • Primary key column(s) present in the resultset: {ReadableColumnInfoString(updateableTableInfo.PrimaryKeys)}." + CRLF);

                if (updateableTableInfo.MatchingOtherColumns.Count == 0)
                {
                    if (updateableTableInfo.PrimaryKeys.Count == 1)
                        sb.Append(PRE + TAB + $"/// • Apart from the primary key column, there are no table columns present in the resultset." + CRLF);
                    else
                        sb.Append(PRE + TAB + $"/// • Apart from the all primary key columns, there are no table columns present in the resultset." + CRLF);
                }
                else
                {
                    string multiple = updateableTableInfo.MatchingOtherColumns.Count > 1 ? "s" : "";
                    split.LinesToSplit = $"Non-primary key column{multiple} present in the resultset: {ReadableColumnInfoString(updateableTableInfo.MatchingOtherColumns)}.";
                    split.ExecSplit(sb);

                    //sb.Append(PRE + TAB + $"/// • Non-primary key column(s) present in the resultset: {ReadableColumnInfoString(updateableTableInfo.MatchingOtherColumns)}." + CRLF);
                }

                if (updateableTableInfo.MissingOtherColumns.Count > 0)
                {
                    string multiple = updateableTableInfo.MissingOtherColumns.Count > 1 ? "s" : "";
                    split.LinesToSplit = $"Non-primary key column{multiple} not present in the resultset: {ReadableColumnInfoString(updateableTableInfo.MissingOtherColumns)}.";
                    split.ExecSplit(sb);

                    //sb.Append(PRE + TAB + $"/// • Non-primary key column(s) not present in the resultset: {ReadableColumnInfoString(updateableTableInfo.MissingOtherColumns)}." + CRLF);
                }

                if (loadertemplate != null) // there is not outside-class to display the ClassSummary
                {
                    if (StringTools.AllLinesAreEmpty(_recordsetItem.ClassSummary) == false)
                    {
                        split.FirstLinePrefix = PRE + TAB + "/// ";
                        split.OtherLinePrefix = PRE + TAB + "/// ";
                        split.LinesToSplit = StringTools.StripCRLFAndSpaces(_recordsetItem.ClassSummary);
                        split.ExecSplit(sb);
                    }
                }

            }
            sb.Append(PRE + TAB + @"/// </summary>" + CRLF);

            // Begin: class definition
            string inherit_from_classname = "ResultsetData";

            if (_recordsetItem.ImplementDatabinding == true)
                inherit_from_classname = "ResultsetObservable";

            sb.Append(PRE + TAB + $"public partial class {recordsetname} : {inherit_from_classname}<{recordsetname}, {recordname}>");

            if (loadertemplate != null)
                sb.Append(", IRecordsetBase");

            if (_recordsetItem.RowloadIncremental)
                sb.Append(", IRecordsetIncremental");

            sb.Append(CRLF);

            // End: class definition

            sb.Append(PRE + TAB + "{" + CRLF);

            if (loadertemplate != null)
                loadertemplate.DeclarePrivates(sb, AdoDirectSwitch);

            // constructor
            sb.Append(PRE + TAB + TAB + "public " + recordsetname + "()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);

            if (loadertemplate != null)
                loadertemplate.InsertInConstructor(sb, AdoDirectSwitch);

            // Begin: Schema definition
            sb.Append(PRE + TAB + TAB + TAB + "ColumnArrayBuilder schema_array = new ColumnArrayBuilder();" + CRLF + CRLF);

            for (int x = 0; x < schema.Count; x++)
            {
                VenturaSqlColumn column = schema[x];

                sb.Append(PRE + TAB + TAB + TAB + $"schema_array.Add(new VenturaSqlColumn(\"{column.ColumnName}\", typeof({column.ShortTypeNameAsCSharpString()}), {column.IsNullable.ToString().ToLower()})" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "{" + CRLF);

                List<string> list = new List<string>();

                if (column.Updateable == true) list.Add($"Updateable = {column.Updateable.ToString().ToLower()}");

                if (column.DbType != null) list.Add($"DbType = DbType.{column.DbType}");
                if (column.ColumnSize != null) list.Add($"ColumnSize = {column.ColumnSize}");
                if (column.NumericPrecision != null) list.Add($"NumericPrecision = {column.NumericPrecision}");
                if (column.NumericScale != null) list.Add($"NumericScale = {column.NumericScale}");
                if (column.ProviderType != 0 && AdoDirectSwitch) list.Add($"ProviderType = {column.ProviderType}");
                if (column.IsUnique == true) list.Add($"IsUnique = {column.IsUnique.ToString().ToLower()}");
                if (column.IsKey == true) list.Add($"IsKey = {column.IsKey.ToString().ToLower()}");
                if (column.IsAliased == true) list.Add($"IsAliased = {column.IsAliased.ToString().ToLower()}");
                if (column.IsExpression == true) list.Add($"IsExpression = {column.IsExpression.ToString().ToLower()}");
                if (column.IsIdentity == true) list.Add($"IsIdentity = {column.IsIdentity.ToString().ToLower()}");
                if (column.IsAutoIncrement == true) list.Add($"IsAutoIncrement = {column.IsAutoIncrement.ToString().ToLower()}");
                if (column.IsRowGuid == true) list.Add($"IsRowGuid = {column.IsRowGuid.ToString().ToLower()}");
                if (column.IsLong == true) list.Add($"IsLong = {column.IsLong.ToString().ToLower()}");
                if (column.IsReadOnly == true) list.Add($"IsReadOnly = {column.IsReadOnly.ToString().ToLower()}");
                if (column.XmlSchemaCollectionDatabase.Length > 0 && AdoDirectSwitch) list.Add($"XmlSchemaCollectionDatabase = \"{column.XmlSchemaCollectionDatabase}\"");
                if (column.XmlSchemaCollectionOwningSchema.Length > 0 && AdoDirectSwitch) list.Add($"XmlSchemaCollectionOwningSchema = \"{column.XmlSchemaCollectionOwningSchema}\"");
                if (column.XmlSchemaCollectionName.Length > 0 && AdoDirectSwitch) list.Add($"XmlSchemaCollectionName = \"{column.XmlSchemaCollectionName}\"");
                if (column.UdtAssemblyQualifiedName.Length > 0 && AdoDirectSwitch) list.Add($"UdtAssemblyQualifiedName = \"{column.UdtAssemblyQualifiedName}\"");

                // BaseServerName/BaseCatalogName/BaseSchemaName/BaseTableName/BaseColumnName
                if (column.BaseServerName.Length > 0 && AdoDirectSwitch) list.Add($"BaseServerName = \"{column.BaseServerName}\"");
                if (column.BaseCatalogName.Length > 0 && AdoDirectSwitch) list.Add($"BaseCatalogName = \"{column.BaseCatalogName}\"");
                if (column.BaseSchemaName.Length > 0 && AdoDirectSwitch) list.Add($"BaseSchemaName = \"{column.BaseSchemaName}\"");
                if (column.BaseTableName.Length > 0 && AdoDirectSwitch) list.Add($"BaseTableName = \"{column.BaseTableName}\"");
                if (column.BaseColumnName.Length > 0 && AdoDirectSwitch) list.Add($"BaseColumnName = \"{column.BaseColumnName}\"");

                for (int i = 0; i < list.Count; i++)
                {
                    sb.Append(PRE + TAB + TAB + TAB + TAB + list[i]);
                    if (i < list.Count - 1) sb.Append(","); // add a comma to all lines but the last one
                    sb.Append(CRLF);
                }

                sb.Append(PRE + TAB + TAB + TAB + "});" + CRLF);

                sb.Append(CRLF);
            } // end of columns loop

            sb.Append(PRE + TAB + TAB + TAB + "((IResultsetBase)this).Schema = new VenturaSqlSchema(schema_array);" + CRLF);
            // End: Schema definition

            if (_resultsetItem.UpdateableTableName != null)
                sb.Append(PRE + TAB + TAB + TAB + $"((IResultsetBase)this).UpdateableTablename = \"{_resultsetItem.UpdateableTableName.CSharpTableName}\";" + CRLF);

            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF); // end of constructor

            // Database columns.
            for (int x = 0; x < schema.Count; x++)
            {
                VenturaSqlColumn column = schema[x];
                sb.Append(PRE + TAB + TAB + @"/// <summary>" + CRLF);

                foreach (var line in ColumnSummary(column))
                    sb.Append(PRE + TAB + TAB + @"/// " + line + CRLF);

                sb.Append(PRE + TAB + TAB + @"/// </summary>" + CRLF);

                sb.Append(PRE + TAB + TAB + "public " + column.ShortTypeNameForColumnProperty() + " " + column.PropertyName() + CRLF);
                sb.Append(PRE + TAB + TAB + "{" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "get { if (CurrentRecord == null) throw new InvalidOperationException(VenturaSqlStrings.CURRENT_RECORD_NOT_SET); return CurrentRecord." + column.PropertyName() + "; }" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "set { if (CurrentRecord == null) throw new InvalidOperationException(VenturaSqlStrings.CURRENT_RECORD_NOT_SET); CurrentRecord." + column.PropertyName() + " = value; }" + CRLF);
                sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            } // end of columns loop

            sb.Append(PRE + TAB + TAB + "public void ResetToUnmodified()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "if (CurrentRecord == null) throw new InvalidOperationException(VenturaSqlStrings.CURRENT_RECORD_NOT_SET);" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "CurrentRecord.ResetToUnmodified();" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(PRE + TAB + TAB + "public void ResetToUnmodifiedExisting()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "if (CurrentRecord == null) throw new InvalidOperationException(VenturaSqlStrings.CURRENT_RECORD_NOT_SET);" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "CurrentRecord.ResetToUnmodifiedExisting();" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(PRE + TAB + TAB + "public void ResetToExisting()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "if (CurrentRecord == null) throw new InvalidOperationException(VenturaSqlStrings.CURRENT_RECORD_NOT_SET);" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "CurrentRecord.ResetToExisting();" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            // User Defined columns
            for (int x = 0; x < _recordsetItem.UserDefinedColumns.Count; x++)
            {
                UDCItem udc_column = _recordsetItem.UserDefinedColumns[x];

                sb.Append(PRE + TAB + TAB + @"/// <summary>" + CRLF);
                sb.Append(PRE + TAB + TAB + @"/// User Defined Column." + CRLF);
                sb.Append(PRE + TAB + TAB + @"/// </summary>" + CRLF);

                sb.Append(PRE + TAB + TAB + $"public {udc_column.ShortTypeName} {udc_column.PropertyName}" + CRLF);
                sb.Append(PRE + TAB + TAB + "{" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + $"get {{ if (CurrentRecord == null) throw new InvalidOperationException(VenturaSqlStrings.CURRENT_RECORD_NOT_SET); return CurrentRecord.{udc_column.PropertyName}; }}" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + $"set {{ if (CurrentRecord == null) throw new InvalidOperationException(VenturaSqlStrings.CURRENT_RECORD_NOT_SET); CurrentRecord.{udc_column.PropertyName} = value; }}" + CRLF);
                sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);
            }

            // Append Method
            sb.Append(PRE + TAB + TAB + "public void Append()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + $"int index = this.RecordCount;" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + $"this.InsertItem(index, new {recordname}());" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "this.CurrentRecordIndex = index;" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(PRE + TAB + TAB + $"public void Append({recordname} record)" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + $"int index = this.RecordCount;" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "this.InsertItem(index, record);" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "this.CurrentRecordIndex = index;" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(PRE + TAB + TAB + $"public {recordname} NewRecord()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + $"return new {recordname}();" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(PRE + TAB + TAB + $"protected override {recordname} InternalCreateExistingRecordObject(object[] columnvalues) => new {recordname}(columnvalues);" + CRLF + CRLF);

            if (loadertemplate != null)
                loadertemplate.InsertPropertiesAndMethods(sb, AdoDirectSwitch, generatortarget);

            sb.Append(PRE + TAB + "}" + CRLF + CRLF); // end of class
            #endregion

            #region Record class CSharp code.
            //==================================================================
            // BEGIN OF RECORD
            // ==================================================================
            string implement_interface = "IRecordBase";

            if (_recordsetItem.ImplementDatabinding == true)
                implement_interface += ", INotifyPropertyChanged";

            sb.Append(PRE + TAB + $"public sealed partial class {recordname} : {implement_interface}" + CRLF);
            sb.Append(PRE + TAB + "{" + CRLF);

            sb.Append(PRE + TAB + TAB + "private DataRecordStatus _recordstatus;" + CRLF);
            sb.Append(PRE + TAB + TAB + "private bool _started_with_dbvalues;" + CRLF);

            sb.Append(CRLF);

            // PRIVATE VARIABLE SECTION
            // Properties for database Columns in Record.
            foreach (var column in schema)
            {
                sb.Append(PRE + TAB + TAB + "private " + column.ShortTypeNameForColumnProperty() + " " + column.PrivateVariableName_Current() + ";");
                sb.Append(" private " + column.ShortTypeNameForColumnProperty() + " " + column.PrivateVariableName_Original() + ";");
                sb.Append(" private bool " + column.PrivateVariableName_Modified() + ";");
                sb.Append(CRLF);
            }

            sb.Append(CRLF);

            // Privates for user defined Columns in Record.
            foreach (var udc_column in _recordsetItem.UserDefinedColumns)
                sb.Append(PRE + TAB + TAB + "private " + udc_column.ShortTypeName + " " + udc_column.PrivateVariableName + ";" + CRLF);

            sb.Append(CRLF);

            // Constructor for NEW record.
            sb.Append(PRE + TAB + TAB + $"public {recordname}()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);

            foreach (var column in schema)
                sb.Append(PRE + TAB + TAB + TAB + column.InitToDefaultValueAsSourceCode() + CRLF);

            sb.Append(PRE + TAB + TAB + TAB + "_started_with_dbvalues = false;" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "_recordstatus = DataRecordStatus.New;" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF); // end of constructor

            // Constructor for EXISTING record.
            sb.Append(PRE + TAB + TAB + $"public {recordname}(object[] columnvalues)" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);

            for (int i = 0; i < schema.Count; i++)
            {
                VenturaSqlColumn column = schema[i];
                sb.Append(PRE + TAB + TAB + TAB + $"{column.PrivateVariableName_Current()} = ({column.ShortTypeNameForColumnProperty()})columnvalues[{i}];" + CRLF);
            }

            sb.Append(PRE + TAB + TAB + TAB + "_started_with_dbvalues = true;" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "_recordstatus = DataRecordStatus.Existing;" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF); // end of constructor

            // Properties for database Columns in Record.
            foreach (var column in schema)
                GenerateDataBaseColumnProperty(sb, column);

            // Properties for user defined Columns in Record.
            foreach (var udc_column in _recordsetItem.UserDefinedColumns)
                GenerateUserDefinedColumnProperty(sb, udc_column);

            // IsModified(string) is not part of an Interface yet.
            sb.Append(PRE + TAB + TAB + "public bool IsModified(string column_name)" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);

            foreach (var column in schema)
                sb.Append(PRE + TAB + TAB + TAB + $"if (column_name == \"{column.PropertyName()}\") return {column.PrivateVariableName_Modified()};" + CRLF);

            sb.Append(PRE + TAB + TAB + TAB + "throw new ArgumentOutOfRangeException(String.Format(VenturaSqlStrings.UNKNOWN_COLUMN_NAME, column_name));" + CRLF);

            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            #region IRecordBase

            // int IRecordBase.ModifiedColumnCount() method
            sb.Append(PRE + TAB + TAB + "public int ModifiedColumnCount()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "int count = 0;" + CRLF);

            foreach (VenturaSqlColumn column in schema)
                sb.Append(PRE + TAB + TAB + TAB + $"if ({column.PrivateVariableName_Modified()} == true) count++;" + CRLF);

            sb.Append(PRE + TAB + TAB + TAB + "return count;" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            sb.Append(PRE + TAB + TAB + "public bool PendingChanges()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);

            if (_resultsetItem.UpdateableTableName == null) // read-only resultset
            {
                sb.Append(PRE + TAB + TAB + TAB + "return false;" + CRLF);
            }
            else
            {
                sb.Append(PRE + TAB + TAB + TAB + "if (_recordstatus == DataRecordStatus.New || _recordstatus == DataRecordStatus.ExistingDelete) return true;" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "int count = 0;" + CRLF);

                var countable_columns = schema.FindAll(c => c.Updateable == true && c.IsKey == true);

                if (countable_columns.Count > 0)
                {
                    sb.Append(PRE + TAB + TAB + TAB + "if (_started_with_dbvalues)" + CRLF);
                    sb.Append(PRE + TAB + TAB + TAB + "{" + CRLF);

                    foreach (var column in countable_columns)
                        sb.Append(PRE + TAB + TAB + TAB + TAB + $"if ({column.PrivateVariableName_Modified()}) count++;" + CRLF);

                    sb.Append(PRE + TAB + TAB + TAB + "}" + CRLF);
                }

                foreach (VenturaSqlColumn column in schema)
                    if (column.Updateable == true && column.IsKey == false)
                        sb.Append(PRE + TAB + TAB + TAB + $"if ({column.PrivateVariableName_Modified()} == true) count++;" + CRLF);

                sb.Append(PRE + TAB + TAB + TAB + "return count > 0;" + CRLF);
            }

            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);


            // Public RecordStatus property
            sb.Append(PRE + TAB + TAB + "public DataRecordStatus RecordStatus()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "return _recordstatus;" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            // IRecordBase.RecordStatus property (hidden behind an interface)
            sb.Append(PRE + TAB + TAB + "DataRecordStatus IRecordBase.RecordStatus" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "get { return _recordstatus; }" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "set { _recordstatus = value; }" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            // ValidateBeforeSaving
            sb.Append(PRE + TAB + TAB + "void IRecordBase.ValidateBeforeSaving(int record_index_to_display)" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);

            var validateable_columns = schema.FindAll(c => c.Updateable == true && c.IsKey == true && c.IsNullable == false);

            if (_resultsetItem.UpdateableTableName != null && validateable_columns.Count > 0) // not a read-only resultset
            {
                sb.Append(PRE + TAB + TAB + TAB + "if (_started_with_dbvalues) return;" + CRLF);

                // Added on 15 April 2019
                sb.Append(PRE + TAB + TAB + TAB + "if (_recordstatus == DataRecordStatus.Existing) return;" + CRLF);

                foreach (VenturaSqlColumn column in validateable_columns)
                    sb.Append(PRE + TAB + TAB + TAB + $"if ({column.PrivateVariableName_Modified()} == false) throw new Exception(string.Format(VenturaSqlStrings.VALUE_NOT_SET_MSG, record_index_to_display, \"{column.PropertyName()}\"));" + CRLF);
            }

            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            // WriteChangesToTrackArray
            sb.Append(PRE + TAB + TAB + "void IRecordBase.WriteChangesToTrackArray(TrackArray track_array)" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);

            if (_resultsetItem.UpdateableTableName != null)
            {
                // The current-side values NEW RECORD.
                // For an INSERT, NULL values don't need to be shipped, as that is the default value.
                sb.Append(PRE + TAB + TAB + TAB + "if (_recordstatus == DataRecordStatus.New)" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "{" + CRLF);

                for (int i = 0; i < schema.Count; i++)
                {
                    VenturaSqlColumn column = schema[i];
                    if (column.Updateable == true) // a readonly-column can never be written
                    {
                        string if_statement = "";

                        if (column.IsNullable == true)
                            if_statement = $"if ({column.PrivateVariableName_Current()} != null) ";

                        sb.Append(PRE + TAB + TAB + TAB + TAB + if_statement + $"track_array.AppendDataValue({i}, {column.PrivateVariableName_Current()});" + CRLF);
                    }
                }

                //sb.Append(PRE + TAB + TAB + TAB + TAB + "if (track_array.HasData == false) return;" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "}" + CRLF);

                // The current-side values EXISTING RECORD.
                // For an ExistingDelete we don't ship values that are set.
                sb.Append(PRE + TAB + TAB + TAB + "else if (_recordstatus == DataRecordStatus.Existing)" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "{" + CRLF);

                var writable_key_columns = schema.FindAll(c => c.Updateable == true && c.IsKey == true);
                var writable_nonkey_columns = schema.FindAll(c => c.Updateable == true && c.IsKey == false);

                if (writable_key_columns.Count > 0)
                {
                    sb.Append(PRE + TAB + TAB + TAB + TAB + "if (_started_with_dbvalues)" + CRLF);
                    sb.Append(PRE + TAB + TAB + TAB + TAB + "{" + CRLF);

                    foreach (var column in writable_key_columns)
                        sb.Append(PRE + TAB + TAB + TAB + TAB + TAB + $"if ({column.PrivateVariableName_Modified()}) track_array.AppendDataValue({column.ColumnOrdinal}, {column.PrivateVariableName_Current()});" + CRLF);

                    sb.Append(PRE + TAB + TAB + TAB + TAB + "}" + CRLF);
                }

                foreach (var column in writable_nonkey_columns)
                        sb.Append(PRE + TAB + TAB + TAB + TAB + $"if ({column.PrivateVariableName_Modified()}) track_array.AppendDataValue({column.ColumnOrdinal}, {column.PrivateVariableName_Current()});" + CRLF);

                sb.Append(PRE + TAB + TAB + TAB + TAB + "if (track_array.HasData == false) return;" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "}" + CRLF + CRLF);

                var all_key_columns = schema.FindAll(c => c.IsKey == true);

                // The PriKeys
                if (all_key_columns.Count > 0)
                {
                    sb.Append(PRE + TAB + TAB + TAB + "if (_recordstatus == DataRecordStatus.Existing || _recordstatus == DataRecordStatus.ExistingDelete)" + CRLF);
                    sb.Append(PRE + TAB + TAB + TAB + "{" + CRLF);

                    foreach (var column in all_key_columns)
                        sb.Append(PRE + TAB + TAB + TAB + TAB + $"track_array.AppendPrikeyValue({column.ColumnOrdinal}, ({column.PrivateVariableName_Modified()} && _started_with_dbvalues) ? {column.PrivateVariableName_Original()} : {column.PrivateVariableName_Current()});" + CRLF);

                    sb.Append(PRE + TAB + TAB + TAB + "}" + CRLF + CRLF);
                }

                sb.Append(PRE + TAB + TAB + TAB + "if (_recordstatus == DataRecordStatus.New) track_array.Status = TrackArrayStatus.DataForINSERT;" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "else if (_recordstatus == DataRecordStatus.Existing) track_array.Status = TrackArrayStatus.DataForUPDATE;" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "else if (_recordstatus == DataRecordStatus.ExistingDelete) track_array.Status = TrackArrayStatus.DataForDELETE;" + CRLF);
            }

            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            //// OnAfterSave
            //sb.Append(PRE + TAB + TAB + "void IRecordBase.OnAfterSave()" + CRLF);
            //sb.Append(PRE + TAB + TAB + "{" + CRLF);
            //sb.Append(PRE + TAB + TAB + TAB + "if (_recordstatus == DataRecordStatus.ExistingDelete)" + CRLF);
            //sb.Append(PRE + TAB + TAB + TAB + TAB + "throw new System.Exception(\"IRecordBase.OnAfterSave ran into a Deleted record.Should not happen.\"); // Should have been done in RecordData.cs already." + CRLF);
            //sb.Append(CRLF);
            //sb.Append(PRE + TAB + TAB + TAB + "ResetTodddddExisting();" + CRLF);
            //sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            // StartedWithDbValues()
            sb.Append(PRE + TAB + TAB + "public bool StartedWithDbValues()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "return _started_with_dbvalues;" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);


            // ResetToUnmodified()
            sb.Append(PRE + TAB + TAB + "/// <summary>" + CRLF);
            sb.Append(PRE + TAB + TAB + "/// Resets all columns to not-modified status." + CRLF);
            sb.Append(PRE + TAB + TAB + "/// </summary>" + CRLF);
            sb.Append(PRE + TAB + TAB + "public void ResetToUnmodified()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);


            sb.Append(PRE + TAB + TAB + TAB + "if (_started_with_dbvalues == true)" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "{" + CRLF);
            //sb.Append(PRE + TAB + TAB + TAB + TAB + "_started_with_dbvalues = false;" + CRLF);

            foreach (var column in schema)
                sb.Append(PRE + TAB + TAB + TAB + TAB + $"if ({column.PrivateVariableName_Modified()}) {column.PrivateVariableName_Original()} = default({column.ShortTypeNameForColumnProperty()});" + CRLF);

            sb.Append(PRE + TAB + TAB + TAB + "}" + CRLF);

            foreach (var column in schema)
                sb.Append(PRE + TAB + TAB + TAB + $"{column.PrivateVariableName_Modified()} = false;" + CRLF);

            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            // ResetToUnmodifiedExisting()
            sb.Append(PRE + TAB + TAB + "/// <summary>" + CRLF);
            sb.Append(PRE + TAB + TAB + "/// Resets the record to DataRecordStatus.Existing. Like it was freshly loaded from the database." + CRLF);
            sb.Append(PRE + TAB + TAB + "/// </summary>" + CRLF);
            sb.Append(PRE + TAB + TAB + "public void ResetToUnmodifiedExisting()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "ResetToUnmodified();" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "_recordstatus = DataRecordStatus.Existing;" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            // ResetToExisting()
            sb.Append(PRE + TAB + TAB + "/// <summary>" + CRLF);
            sb.Append(PRE + TAB + TAB + "/// Resets the record to DataRecordStatus.Existing." + CRLF);
            sb.Append(PRE + TAB + TAB + "/// </summary>" + CRLF);
            sb.Append(PRE + TAB + TAB + "public void ResetToExisting()" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "_recordstatus = DataRecordStatus.Existing;" + CRLF);
            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            // This is used to insert the updated identity column value back into the Record.
            sb.Append(PRE + TAB + TAB + "void IRecordBase.SetIdentityColumnValue(object value)" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);

            if (schema.IdentityColumn != null)
            {
                sb.Append(PRE + TAB + TAB + TAB + $"{schema.IdentityColumn.PrivateVariableName_Current()} = ({schema.IdentityColumn.ShortTypeNameForColumnProperty()})value;" + CRLF);

                if (_recordsetItem.ImplementDatabinding == true)
                {
                    sb.Append(PRE + TAB + TAB + TAB + $"OnPropertyChanged(\"{schema.IdentityColumn.PropertyName()}\");" + CRLF);
                    sb.Append(PRE + TAB + TAB + TAB + $"OnAfterPropertyChanged(\"{schema.IdentityColumn.PropertyName()}\");" + CRLF);
                }
            }

            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);

            #endregion IRecordBase

            if (_recordsetItem.ImplementDatabinding == true)
            {
                sb.Append(PRE + TAB + TAB + "partial void OnAfterPropertyChanged(string propertyName);" + CRLF + CRLF);

                sb.Append(PRE + TAB + TAB + "public event PropertyChangedEventHandler PropertyChanged;" + CRLF + CRLF);

                sb.Append(PRE + TAB + TAB + "private void OnPropertyChanged(string propertyName)" + CRLF);
                sb.Append(PRE + TAB + TAB + "{" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + "PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));" + CRLF);
                sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);
            }

            sb.Append(PRE + TAB + "}" + CRLF); // end of record class

            // end of record class
            #endregion

        } // end of method generate

        #region Generate database Column property & generate User Defined Column property

        private void GenerateDataBaseColumnProperty(StringBuilder sb, VenturaSqlColumn column)
        {
            sb.Append(PRE + TAB + TAB + @"/// <summary>" + CRLF);

            foreach (var line in ColumnSummary(column))
                sb.Append(PRE + TAB + TAB + @"/// " + line + CRLF);

            //if (column.ColumnSource == VenturaSqlColumnSource.SqlServer && column.ProviderType == VenturaSqlDbType.Udt && (generatortarget == VenturaSqlPlatform.UWP || generatortarget == VenturaSqlPlatform.Android || generatortarget == VenturaSqlPlatform.iOS))
            //    sb.Append(PRE + TAB + TAB + $@"/// Since CLR-UDT objects cannot be deserialized on the {generatortarget} platform, this property returns the raw serialization output as a byte array." + CRLF);

            sb.Append(PRE + TAB + TAB + @"/// </summary>" + CRLF);

            sb.Append(PRE + TAB + TAB + "public " + column.ShortTypeNameForColumnProperty() + " " + column.PropertyName() + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);

            // The Getter.
            sb.Append(PRE + TAB + TAB + TAB + $"get {{ return {column.PrivateVariableName_Current()}; }}" + CRLF);

            // The Setter.
            sb.Append(PRE + TAB + TAB + TAB + "set" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "{" + CRLF);

            if (column.ForbidNullValue() == true)
                sb.Append(PRE + TAB + TAB + TAB + TAB + $"if (value == null) throw new ArgumentNullException(\"{column.PropertyName()}\", VenturaSqlStrings.SET_NULL_MSG);" + CRLF);

            sb.Append(PRE + TAB + TAB + TAB + TAB + $"if (_started_with_dbvalues == false) {column.PrivateVariableName_Modified()} = true;" + CRLF);

            sb.Append(PRE + TAB + TAB + TAB + TAB + $"if ({column.PrivateVariableName_Current()} == value) return;" + CRLF);

            sb.Append(PRE + TAB + TAB + TAB + TAB + "if (_started_with_dbvalues == true)" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + TAB + "{" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + TAB + TAB + $"if ({column.PrivateVariableName_Modified()} == false) {{ {column.PrivateVariableName_Original()} = {column.PrivateVariableName_Current()}; {column.PrivateVariableName_Modified()} = true; }} // existing record and column is not modified" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + TAB + TAB + $"else {{ if (value == {column.PrivateVariableName_Original()}) {{ {column.PrivateVariableName_Original()} = default({column.ShortTypeNameForColumnProperty()}); {column.PrivateVariableName_Modified()} = false; }} }} // existing record and column is modified" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + TAB + "}" + CRLF);

            sb.Append(PRE + TAB + TAB + TAB + TAB + $"{column.PrivateVariableName_Current()} = value;");

            if (_recordsetItem.ImplementDatabinding == true)
                sb.Append($" OnPropertyChanged(\"{column.PropertyName()}\"); OnAfterPropertyChanged(\"{column.PropertyName()}\");");

            sb.Append(CRLF);

            sb.Append(PRE + TAB + TAB + TAB + "}" + CRLF); //CLOSE SET

            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);
        }

        private void GenerateUserDefinedColumnProperty(StringBuilder sb, UDCItem udc_column)
        {
            sb.Append(PRE + TAB + TAB + @"/// <summary>" + CRLF);
            sb.Append(PRE + TAB + TAB + @"/// User Defined Column." + CRLF);
            sb.Append(PRE + TAB + TAB + @"/// </summary>" + CRLF);

            sb.Append(PRE + TAB + TAB + $"public {udc_column.ShortTypeName} {udc_column.PropertyName}" + CRLF);
            sb.Append(PRE + TAB + TAB + "{" + CRLF);

            // The Getter.
            sb.Append(PRE + TAB + TAB + TAB + $"get {{ return {udc_column.PrivateVariableName}; }}" + CRLF); //CLOSE GET

            // The Setter.
            sb.Append(PRE + TAB + TAB + TAB + "set" + CRLF);
            sb.Append(PRE + TAB + TAB + TAB + "{" + CRLF);

            sb.Append(PRE + TAB + TAB + TAB + TAB + $"if ({udc_column.PrivateVariableName} == value) return;" + CRLF);

            sb.Append(PRE + TAB + TAB + TAB + TAB + $"{udc_column.PrivateVariableName} = value;" + CRLF);

            if (_recordsetItem.ImplementDatabinding == true)
            {
                sb.Append(PRE + TAB + TAB + TAB + TAB + $"OnPropertyChanged(\"{udc_column.PropertyName}\");" + CRLF);
                sb.Append(PRE + TAB + TAB + TAB + TAB + $"OnAfterPropertyChanged(\"{udc_column.PropertyName}\");" + CRLF);
            }

            sb.Append(PRE + TAB + TAB + TAB + "}" + CRLF); //CLOSE SET

            sb.Append(PRE + TAB + TAB + "}" + CRLF + CRLF);
        }

        private List<string> ColumnSummary(VenturaSqlColumn column)
        {
            List<string> list = new List<string>();

            StringBuilder sb = new StringBuilder();

            if (column.Updateable == true)
                sb.Append("Database Column Updateable.");
            else
                sb.Append("Database Column NotUpdateable.");

            if (column.BaseTableName != "")
            {
                sb.Append(" Table ");
                sb.Append(column.AsTableName().ScriptTableName);
                sb.Append(".");
            }

            if (column.DbType != null)
            {
                sb.Append(" DbType.");
                sb.Append(column.DbType.ToString());
            }

            if (column.IsKey)
                sb.Append(" PrimaryKey.");

            if (column.IsReadOnly == true)
                sb.Append(" Readonly.");
            else
                sb.Append(" NotReadonly.");

            if (column.IsNullable)
                sb.Append(" AllowNull.");
            else
                sb.Append(" NotNull.");

            if (column.IsIdentity)
                sb.Append(" IsIdentity.");

            if (column.IsAutoIncrement)
                sb.Append(" AutoIncrement.");


            if (column.IsRowGuid)
                sb.Append(" IsRowGuid.");

            if (column.IsExpression == true)
                sb.Append(" Expression.");

            string text = sb.ToString();

            list.Add(sb.ToString());


            if (column.Description != null)
            {
                List<string> description_lines = StringTools.WordWrap(column.Description, 120);

                for (int i = 0; i < description_lines.Count; i++)
                {
                    list.Add(description_lines[i]);
                    // Add a '.' at the end?
                }
            }

            return list;
        } // end of method

        private string ReadableColumnInfoString(List<string> list)
        {
            StringBuilder sb = new StringBuilder();

            int count = 0;

            foreach (string name in list)
            {
                count++;

                if (count > 1 && count < list.Count)
                    sb.Append(", ");
                else if (count == list.Count && list.Count > 1)
                    sb.Append(" and ");

                sb.Append(name); ;
            }

            return sb.ToString();
        }

        #endregion



    } // end of class

} // end of namespace
