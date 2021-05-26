using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.IO;
using VenturaSQL;

namespace VenturaSQLStudio.Ado
{
    /// <summary>
    /// Collects all kinds of information relating to a Sql script.
    /// This is the class that makes VenturaSQL Studio possible.
    /// QueryInfo contains a list of type ResultsetInfo.
    /// </summary>
    public class QueryInfo
    {
        #region Static methods (class factory)
        internal static QueryInfo CreateInstance(RecordsetItem recordsetitem)
        {
            Project project = MainWindow.ViewModel.CurrentProject;
            AdoConnector connector = AdoConnectorHelper.Create(project.ProviderInvariantName, project.MacroConnectionString);

            char parameter_prefix = MainWindow.ViewModel.CurrentProject.ParameterPrefix;

            string sql_script = recordsetitem.SqlScript;

            List<DbParameter> parameters = new List<DbParameter>();

            // Start with the automatic parameters.

            DbParameter db_parameter = connector.CreateParameter(connector.ParameterPrefix + "DesignMode", true);
            parameters.Add(db_parameter);

            db_parameter = connector.CreateParameter(connector.ParameterPrefix + "RowOffset", 0);
            parameters.Add(db_parameter);

            db_parameter = connector.CreateParameter(connector.ParameterPrefix + "RowLimit", 500);
            parameters.Add(db_parameter);

            foreach (ParameterItem recordset_parameter in recordsetitem.Parameters)
            {
                parameters.Add(recordset_parameter.CreateDesignValueDbParameter(connector));
            }

            return CreateInstance(sql_script, parameters);
        }

        /// <summary>
        /// Create and execute.
        /// </summary>
        public static QueryInfo CreateInstance(string sql_script, List<DbParameter> parameters = null)
        {
            QueryInfo q = new QueryInfo(sql_script, parameters);

            q.ExecuteSqlScriptAndCollectData();
            
            return q;
        }
        #endregion

        private AdoConnector _ado_connector;
        private string _sql_script;
        private List<DbParameter> _parameters;
        private List<ResultSetInfo> _resultsets;

        /// <summary>
        /// QueryInfo will use the RecordsetItem for 2 things:
        /// a) The SQL script.
        /// b) Use the Parameterlist of the RecordsetItem to set SqlParameters so the SQL script can be run without
        /// ADO.NET reporting missing parameters.
        /// </summary>
        private QueryInfo(string sql_script, List<DbParameter> parameters)
        {
            Project project = MainWindow.ViewModel.CurrentProject;

            _ado_connector = AdoConnectorHelper.Create(project.ProviderInvariantName, project.MacroConnectionString);
            _sql_script = sql_script;
            _parameters = parameters;

            _resultsets = new List<ResultSetInfo>();
        }

        public void ExecuteSqlScriptAndCollectData()
        {

            if (_sql_script == null)
                return;

            if (SqlScriptIsEmpty() == true)
                throw new VenturaSqlException("There is no SQL script.");

            using (DbConnection connection = _ado_connector.OpenConnection())
            {
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    ExecuteTheReader(connection, transaction);
                    transaction.Rollback();
                }
            }

        }

        private void ExecuteTheReader(DbConnection connection, DbTransaction transaction)
        {
            _resultsets.Clear();

            DbDataReader datareader = null;

            try
            {
                DbCommand command = _ado_connector.CreateCommand();

                command.CommandText = _sql_script;
                command.Connection = connection;
                command.Transaction = transaction;

                if (_parameters != null)
                {
                    foreach (var db_parameter in _parameters)
                    {
                        command.Parameters.Add(db_parameter);
                    }
                }

                try
                {
                    if (command is Microsoft.Data.Sqlite.SqliteCommand)
                        datareader = command.ExecuteReader();
                    else
                        datareader = command.ExecuteReader(CommandBehavior.KeyInfo | CommandBehavior.SchemaOnly);


                }
                catch (Exception ex)
                {
                    throw new ExecuteSqlScriptException(command.CommandText, ex);
                }


                while (true)
                {
                    // Cache the Schema into a DataTable list.
                    DataTable adoschematable = datareader.GetSchemaTable();

                    // "|| adoschematable.Rows.Count == 0" was added for SqLite as it will always return a DataTable, but without rows.
                    if (adoschematable == null || adoschematable.Rows.Count == 0) // The Sql script generated no resultsets!
                        break;

                    // After retrieving the Schema with GetSchemaTable, you MUST remove rows where IsHidden is set to true.
                    QueryInfoTools.RemoveIsHiddenRowsFromSchemaTable(adoschematable);

                    FixSqlScriptSchema(adoschematable);

                    _resultsets.Add(new ResultSetInfo(adoschematable));

                    if (datareader.NextResult() == false)
                        break;
                }

                datareader.Close();

                // Collect information for each table referenced in each resultset.
                foreach (ResultSetInfo resultset_info in _resultsets)
                    resultset_info.FillReferencedTablesList(_ado_connector, connection, transaction);
            }
            finally
            {
                StudioGeneral.SmartClose(datareader);
            }

        }

        private void FixSqlScriptSchema(DataTable script_schema)
        {
            // Find the column in the sql-script schema
            foreach (DataRow script_schema_row in script_schema.Rows)
            {
                //SchemaRowInfo script_schema_row_info = new SchemaRowInfo(script_schema_row);

                FixRow(script_schema_row);

            }
        }

        private void FixRow(DataRow script_schema_row)
        {
            // System.Data.SQLite
            if (_ado_connector.ProviderInvariantName == "System.Data.SQLite")
            {
                // The BaseSchemaName is always 'sqlite_default_schema'. Remove it.
                if (script_schema_row.ColumnExists("BaseSchemaName") == true)
                    script_schema_row.SetField("BaseSchemaName", DBNull.Value);

                // System.Data.SQLite provider does not set IsReadOnly correctly, but it does set IsExpression.
                // This compensates the behavior.
                if (script_schema_row.ColumnExists("IsExpression") == true && script_schema_row.ColumnExists("IsReadOnly") == true)
                {
                    if (script_schema_row.RowValue<bool>("IsExpression") == true && script_schema_row.RowValue<bool>("IsReadOnly") == false)
                        script_schema_row.SetField("IsReadOnly", true);
                }
            }

            // Microsoft.Data.SqLite
            if (_ado_connector.ProviderInvariantName == "Microsoft.Data.Sqlite")
            {
                // A BLOB column has a variable type. It can return Int32, String etc.. depending on the value.
                // GetSchemaTable() returns the type that was in the first data row or something.
                // We force it to Object type since the type is variable.

                if (script_schema_row.ColumnExists("DataTypeName") == true)
                {
                    string datatypename = script_schema_row.RowValue<string>("DataTypeName");

                    if (datatypename == "BLOB")
                        script_schema_row.SetField("DataType", typeof(object));
                    else if (datatypename == "TEXT")
                        script_schema_row.SetField("DataType", typeof(string));
                    else if (datatypename == "REAL")
                        script_schema_row.SetField("DataType", typeof(Double));
                    else if (datatypename == "NUMERIC")
                        script_schema_row.SetField("DataType", typeof(Decimal));
                    else if (datatypename == "INTEGER")
                        script_schema_row.SetField("DataType", typeof(Int64));
                }

                // The BaseServerName is the full path to the database .db file. Remove it.
                if (script_schema_row.ColumnExists("BaseServerName") == true)
                    script_schema_row.SetField("BaseServerName", DBNull.Value);

            }

        }

        public List<ResultSetInfo> ResultSets
        {
            get { return _resultsets; }
        }

        private bool SqlScriptIsEmpty()
        {
            StringReader strReader = new StringReader(_sql_script);
            int characters = 0;

            while (true)
            {
                string line = strReader.ReadLine();

                if (line == null) break;

                characters += line.Trim().Length;
            }

            return (characters == 0);
        }

    } // end of class

    //public class QueryInfoParameter
    //{
    //    public string Name { get; set; }
    //    public object Value { get; set; }
    //}

} // end of namespace