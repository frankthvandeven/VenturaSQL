using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using VenturaSQL;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio.AutoCreate
{
    internal class RecordsetAutoCreator
    {
        private const string CRLF = "\r\n";
        private const int LINELENGTH = 140;
        // CATALOG.SCHEMA.TABLE

        private Project _project;

        internal RecordsetAutoCreator(Project project)
        {
            _project = project; // only used for AdoConnector()
        }

        private TableList _table_list;
        private bool _include_catalog_in_recordset_name;
        private bool _include_schema_in_recordset_name;
        private AdoConnector _connector;

        internal List<AutoCreateRecordset> CreateRecordsets()
        {
            _table_list = new TableList();

            _table_list.CollectListOfTables(_project);

            CalcGrouping();

            List<AutoCreateRecordset> recordsets = new List<AutoCreateRecordset>(); 

            _connector = AdoConnectorHelper.Create(_project.ProviderInvariantName, _project.MacroConnectionString);

            using (DbConnection connection = _connector.OpenConnection())
            {
                foreach (var item in _table_list)
                {
                    if (item.Exclude == false) // selected means excluded!
                    {
                        QueryInfo query_info = GetQueryInfo(item.PreliminaryTableName);
                        TableName complete_tablename = GetCompleteTableName(query_info);
                        List<AutoCreateKeyColumn> key_columns = CollectKeyColumns(query_info);
                        List<string> column_list = CollectColumnList(_connector, query_info);

                        if (key_columns.Count > 0)
                        {
                            AutoCreateRecordset acr = CreatePriKeyRecordset(connection, complete_tablename, key_columns, column_list);

                            recordsets.Add(acr);
                        }

                        if (_project.AutoCreateSettings.CreateGetAll == true)
                        {
                            AutoCreateRecordset acr = CreateGetAllRecordset(connection, complete_tablename, key_columns, column_list);

                            recordsets.Add(acr);
                        }

                        if (_project.AutoCreateSettings.CreateIncremental == true)
                        {
                            AutoCreateRecordset acr = CreateIncrementalRecordset(connection, complete_tablename, key_columns, column_list);

                            recordsets.Add(acr);
                        }

                    }
                }

            }

            List<AutoCreateRecordset> sorted = recordsets.OrderBy(z => z.ClassName).ToList();

            return sorted;
        }

        private void CalcGrouping()
        {
            // When all the catalog names are the same, we omit it from the Recordset classname.
            var grouped_catalognames = from x in _table_list
                                           /*where x.Selected == true*/
                                       group x by x.PreliminaryTableName.BaseCatalogName;

            if (grouped_catalognames.Count() == 1)
                _include_catalog_in_recordset_name = false;
            else
                _include_catalog_in_recordset_name = true;

            // When all the schema names are the same, we omit it from the Recordset classname.
            var grouped_schemanames = from x in _table_list
                                          /*where x.Selected == true*/
                                      group x by x.PreliminaryTableName.BaseCatalogName + "." + x.PreliminaryTableName.BaseSchemaName;

            if (grouped_schemanames.Count() == 1)
                _include_schema_in_recordset_name = false;
            else
                _include_schema_in_recordset_name = true;
        }

        private AutoCreateRecordset CreatePriKeyRecordset(DbConnection connection, TableName tablename, List<AutoCreateKeyColumn> key_columns, List<string> columnlist)
        {
            if (key_columns == null)
                throw new ArgumentNullException("key_columns");

            AutoCreateRecordset rs = new AutoCreateRecordset();
            rs.ClassName = "PriKey_" + CreateClassName(tablename);
            rs.UpdateableTableName = null;
            rs.KeyColumns = key_columns;

            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ");

            int linewidth = sb.Length;
            int last_one = columnlist.Count - 1;

            for (int i = 0; i < columnlist.Count; i++)
            {
                var column = columnlist[i];

                sb.Append(column);

                if (i < last_one) 
                    sb.Append(",");

                linewidth += column.Length;

                if (linewidth > LINELENGTH)
                {
                    sb.Append(CRLF);
                    linewidth = 0;
                }
            }

            if (linewidth > 0)
                sb.Append(CRLF);

            sb.Append("FROM ");
            sb.Append(tablename.ScriptTableName);

            rs.UpdateableTableName = tablename;

            sb.Append(CRLF);
            sb.Append("WHERE ");

            for (int i = 0; i < key_columns.Count; i++)
            {
                if (i > 0)
                {
                    sb.Append(CRLF);
                    sb.Append("AND ");
                }

                AutoCreateKeyColumn ps = key_columns[i];

                sb.Append(_connector.QuotePrefix);
                sb.Append(ps.ColumnName);
                sb.Append(_connector.QuoteSuffix);
                sb.Append(" = ");
                sb.Append(_connector.ParameterPrefix);
                sb.Append(ps.ColumnName);
            }

            rs.SqlScript = sb.ToString();

            return rs;
        }

        private AutoCreateRecordset CreateGetAllRecordset(DbConnection connection, TableName tablename, List<AutoCreateKeyColumn> key_columns, List<string> columnlist)
        {
            AutoCreateRecordset rs = new AutoCreateRecordset();
            rs.ClassName = "GetAll_" + CreateClassName(tablename);

            if (key_columns.Count == 0)
                rs.UpdateableTableName = null; // readonly
            else
                rs.UpdateableTableName = tablename; // null would make it readonly. tablename makes it updateable.

            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ");

            int linewidth = sb.Length;
            int last_one = columnlist.Count - 1;

            for (int i = 0; i < columnlist.Count; i++)
            {
                var column = columnlist[i];

                sb.Append(column);

                if (i < last_one)
                    sb.Append(",");

                linewidth += column.Length;

                if (linewidth > LINELENGTH)
                {
                    sb.Append(CRLF);
                    linewidth = 0;
                }
            }

            if (linewidth > 0)
                sb.Append(CRLF);

            sb.Append("FROM ");
            sb.Append(tablename.ScriptTableName);

            rs.SqlScript = sb.ToString();

            return rs;
        }

        private AutoCreateRecordset CreateIncrementalRecordset(DbConnection connection, TableName tablename, List<AutoCreateKeyColumn> key_columns, List<string> columnlist)
        {
            AutoCreateRecordset rs = new AutoCreateRecordset();
            rs.ClassName = "Incr_" + CreateClassName(tablename);
            rs.AsIncremental = true;

            if (key_columns.Count == 0)
                rs.UpdateableTableName = null; // readonly
            else
                rs.UpdateableTableName = tablename; // null would make it readonly. tablename makes it updateable.

            StringBuilder sb = new StringBuilder();

            sb.Append("SELECT ");

            int linewidth = sb.Length;
            int last_one = columnlist.Count - 1;

            for (int i = 0; i < columnlist.Count; i++)
            {
                var column = columnlist[i];

                sb.Append(column);

                if (i < last_one)
                    sb.Append(",");

                linewidth += column.Length;

                if (linewidth > LINELENGTH)
                {
                    sb.Append(CRLF);
                    linewidth = 0;
                }
            }

            if (linewidth > 0)
                sb.Append(CRLF);

            sb.Append("FROM ");
            sb.Append(tablename.ScriptTableName);

            sb.Append(CRLF);
            sb.Append("ORDER BY ");

            for (int i = 0; i < key_columns.Count; i++)
            {
                if (i > 0)
                    sb.Append(",");

                AutoCreateKeyColumn ps = key_columns[i];
                sb.Append(_connector.QuotePrefix);
                sb.Append(ps.ColumnName);
                sb.Append(_connector.QuoteSuffix);
            }

            sb.Append(CRLF);

            string piv = MainWindow.ViewModel.CurrentProject.ProviderInvariantName;
            char pref = MainWindow.ViewModel.CurrentProject.ParameterPrefix;

            if ( piv == "System.Data.SqlClient" )
            {
                sb.Append($"OFFSET {pref}RowOffset ROWS FETCH NEXT {pref}RowLimit ROWS ONLY");
            }
            else if(piv == "Microsoft.Data.Sqlite" || piv == "System.Data.SQLite")
            {
                sb.Append($"LIMIT {pref}RowLimit OFFSET {pref}RowOffset");
            }
            else
            {
                sb.Append($"OFFSET {pref}RowOffset LIMIT {pref}RowLimit");
                sb.Append(CRLF);
                sb.Append(CRLF);
                sb.Append($"-- VenturaSQL doesn't know the incremental loading (pagination) SQL syntax for provider '{piv}' and used the default OFFSET/LIMIT syntax.");
                sb.Append(CRLF);
                sb.Append("-- If the above syntax is wrong, contact Frank and ask for Ventura to be updated and for now disable the automatic creation of 'Incr' recordsets in [Project Settings]-[Auto Create Recordsets]-[Settings].");
            }

            rs.SqlScript = sb.ToString();

            return rs;
        }

        private QueryInfo GetQueryInfo(TableName preliminary_tablename)
        {
            string sql_statement = $"SELECT * FROM {preliminary_tablename.ScriptTableName} WHERE 1=0";

            return QueryInfo.CreateInstance(sql_statement);
        }


        /// <summary>
        /// It is important to get the full tablename from DbDataReader.GetSchemaTable() and NOT from DbConnection.GetSchema("Tables").
        /// There can (an will) be a difference between the reported Server/Catalog/Schema/Table names.
        /// This would screw up the updatable table name.
        /// </summary>
        /// <param name="query_info"></param>
        /// <returns></returns>
        private TableName GetCompleteTableName(QueryInfo query_info)
        {
            if (query_info.ResultSets.Count != 1)
                throw new DataException("Method PreliminaryTableNameToCompleteTableName did not yield any resultsets.");

            ResultSetInfo resultset = query_info.ResultSets[0];

            if (resultset.Tables.Count == 0)
                throw new DataException("Method PreliminaryTableNameToCompleteTableName did not yield any table information.");

            TableInfo tableinfo = resultset.Tables[0];

            return tableinfo.TableName;
        }

        private List<AutoCreateKeyColumn> CollectKeyColumns(QueryInfo query_info)
        {
            List<AutoCreateKeyColumn> key_columns = new List<AutoCreateKeyColumn>();

            DataTable adoschematable = query_info.ResultSets[0].AdoSchemaTable;

            DataRowCollection rows = adoschematable.Rows;

            for (int i = 0; i < rows.Count; i++)
            {
                SchemaRowInfo row_info = new SchemaRowInfo(rows[i]);

                if (row_info.IsKey)
                {
                    AutoCreateKeyColumn ps = new AutoCreateKeyColumn();
                    ps.ColumnName = row_info.BaseColumnName;
                    ps.ColumnType = row_info.DataType;

                    DbType dbtype = TypeTools.FrameworkTypeToDbType(row_info.DataType);

                    ps.DbTypeString = "DbType." + dbtype.ToString();

                    key_columns.Add(ps);
                }
            }

            return key_columns;
        }

        /// <summary>
        /// Returns a list of column names including quotes, for example [Firstname]
        /// </summary>
        private List<string> CollectColumnList(AdoConnector connector, QueryInfo query_info)
        {
            List<string> list = new List<string>();

            DataTable adoschematable = query_info.ResultSets[0].AdoSchemaTable;

            DataRowCollection rows = adoschematable.Rows;

            for (int i = 0; i < rows.Count; i++)
            {
                SchemaRowInfo row_info = new SchemaRowInfo(rows[i]);

                list.Add(_connector.QuotePrefix + row_info.BaseColumnName + _connector.QuoteSuffix);
            }

            return list;
        }

        internal class AutoCreateRecordset
        {
            internal string ClassName;
            internal TableName UpdateableTableName;
            internal string SqlScript;
            internal bool AsIncremental;
            internal List<AutoCreateKeyColumn> KeyColumns = null;
        }

        internal class AutoCreateKeyColumn
        {
            internal string ColumnName;
            internal Type ColumnType;
            internal string DbTypeString;
        }

        private string CreateClassName(TableName tablename)
        {
            StringBuilder sb = new StringBuilder();

            if (_include_catalog_in_recordset_name == true)
                sb.Append(tablename.BaseCatalogName + "_");

            if (_include_schema_in_recordset_name == true)
                sb.Append(tablename.BaseSchemaName + "_");

            sb.Append(tablename.BaseTableName);
            sb.Append("_Recordset");

            string class_name = sb.ToString();

            class_name = ConvertToValidIdentifier(class_name);

            return class_name;
        }

        private string ConvertToValidIdentifier(string input)
        {
            if (input.Length == 0)
                throw new ArgumentOutOfRangeException("input");

            StringBuilder new_name = new StringBuilder();

            for (int i = 0; i < input.Length; i++)
            {
                char c = input[i];

                if (char.IsDigit(c))
                {
                    if (i == 0) // first character cannot be digit
                        new_name.Append("_");

                    new_name.Append(c);
                }
                else if (char.IsNumber(c))
                    new_name.Append(c);
                else if (char.IsLetter(c))
                    new_name.Append(c);
                else if (c == '_')
                    new_name.Append(c);
                else
                    new_name.Append("_");
            }

            string output = new_name.ToString();

            // Remove double underscores
            while (output.Contains("__"))
                output = output.Replace("__", "_");

            if (output == "_")
                throw new Exception($"Converter was unable to turn '{input}' into a valid C# identifier. Not enough data.");

            return output;

        }

    }
}
