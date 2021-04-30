using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using VenturaSQL;

namespace VenturaSQLStudio.Ado
{

    public class TableInfo
    {
        private TableName _tablename;

        private List<string> _primarykeys;
        private List<string> _matchingprikeys;
        private List<string> _missingprikeys;

        private List<string> _othercolumns;
        private List<string> _matchingothercolumns;
        private List<string> _missingothercolumns;

        public TableInfo(AdoConnector connector, DbConnection connection, DbTransaction transaction, TableName tablename, DataTable adoschematable)
        {
            _tablename = tablename;

            _primarykeys = new List<string>();
            _matchingprikeys = new List<string>();
            _missingprikeys = new List<string>();

            _othercolumns = new List<string>();
            _matchingothercolumns = new List<string>();
            _missingothercolumns = new List<string>();

            DataTable table_schema = GetTableSchema(connector, connection, transaction);

            // Fill the primary key and othercolumns list.
            foreach (DataRow row in table_schema.Rows)
            {
                SchemaRowInfo row_info = new SchemaRowInfo(row);

                if (row_info.IsKey == true)
                    _primarykeys.Add(row_info.BaseColumnName); // ColumnName and BaseColumnName are always equal in this situation
                else
                    _othercolumns.Add(row_info.BaseColumnName);
            }

            FixSqlScriptSchema(adoschematable, table_schema);

            FillMatchingAndMissingPriKeysList(adoschematable);
            FillMatchingAndMissingOtherColumnsList(adoschematable);

        }

        #region Properties that only return information

        /// <summary>
        /// Returns the fully specified table name. Can be used in an Sql script.
        /// Full table name = MainServer + '.' + MainCatalogName + '.' + MainSchemaName + '.' + MainTableName
        /// The names are surrounded by square brackets as per T-Sql standard.
        /// For example: [WORK-PC].[AdventureWorks2014].[dbo].[This is the table name]
        /// </summary>
        public TableName TableName
        {
            get { return _tablename; }
        }

        /// <summary>
        /// A list containing names of the primary key columns for the table.
        /// </summary>
        public List<string> PrimaryKeys
        {
            get { return _primarykeys; }
        }

        /// <summary>
        /// A list of this table's primary keys that are present in the resultset for the Sql script specified.
        /// </summary>
        public List<string> MatchingPriKeys
        {
            get { return _matchingprikeys; }
        }

        /// <summary>
        /// A list of this table's primary keys that are missing in the resultset for the Sql script specified.
        /// </summary>
        public List<string> MissingPriKeys
        {
            get { return _missingprikeys; }
        }

        /// <summary>
        /// A list containing names of all the remaining columns for the table, that is all columns, minus the primary keys.
        /// Primary keys are in the .PrimaryKeys list.
        /// .PrimaryKeys plus .OtherColumns = all the columns of the table.
        /// </summary>
        public List<string> OtherColumns
        {
            get { return _othercolumns; }
        }

        /// <summary>
        /// A list of this table's columns that are present in the resultset for the Sql script specified.
        /// This list excludes primary keys. Primary key information is in the .MatchingPrimaryKeys list.
        /// </summary>
        public List<string> MatchingOtherColumns
        {
            get { return _matchingothercolumns; }
        }

        /// <summary>
        /// A list of this table's columns that are missing in the resultset for the Sql script specified.
        /// This list excludes primary keys. Primary key information is in the .MatchingPrimaryKeys list.
        /// </summary>
        public List<string> MissingOtherColumns
        {
            get { return _missingothercolumns; }
        }

        #endregion

        private DataTable GetTableSchema(AdoConnector connector, DbConnection connection, DbTransaction transaction)
        {
            string script = $"SELECT * FROM {_tablename.ScriptTableName} WHERE 1=0;";

            DbDataReader datareader = null;

            try
            {
                DbCommand command = connector.CreateCommand(script, connection, transaction);

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

                return datareader.GetSchemaTable();
            }
            finally
            {
                if (datareader != null) datareader.Close();
            }

        } // end of method

        private void FixSqlScriptSchema(DataTable script_schema, DataTable table_schema)
        {
            foreach (DataRow table_schema_row in table_schema.Rows)
            {
                SchemaRowInfo table_schema_row_info = new SchemaRowInfo(table_schema_row);

                // Find the column in the sql-script schema
                foreach (DataRow script_schema_row in script_schema.Rows)
                {
                    SchemaRowInfo script_schema_row_info = new SchemaRowInfo(script_schema_row);

                    TableName tablename_in_sqlscript = script_schema_row_info.GetTableName();

                    // check if we found a matching column.
                    if (tablename_in_sqlscript == _tablename && script_schema_row_info.BaseColumnName == table_schema_row_info.BaseColumnName)
                    {
                        FixRow(script_schema_row, table_schema_row);
                        break;
                    }
                }
            }

        }

        /// <summary>
        /// Both schema rows are guaranteed to be about the same table and same column.
        /// </summary>
        private void FixRow(DataRow script_schema_row, DataRow table_schema_row)
        {
            // select Orders.OrderID, Orders.OrderDate, Orders.ShipAddress, Orders.ShipCity, Orders.CustomerID, Customers.FirstName, Customers.LastName
            // from Orders join Customers on Customers.CustomerID = Orders.CustomerID order by OrderDate desc limit 50        
            //
            // The SQLite provider does not set the IsKey column in case of a JOIN query.
            // This sets the column afterwards.
            if (table_schema_row.RowValue<bool>("IsKey") == true && script_schema_row.ColumnExists("IsKey") == true)
                if (script_schema_row.RowValue<bool>("IsKey") == false)
                    script_schema_row.SetField("IsKey", true);

        }

        /// <summary>
        /// Fills the _matchingprikeys and _missingprikeys Lists.
        /// </summary>
        private void FillMatchingAndMissingPriKeysList(DataTable adoschematable)
        {
            foreach (string prikeyname in _primarykeys)
            {
                bool found = false;

                foreach (DataRow row in adoschematable.Rows)
                {
                    SchemaRowInfo row_info = new SchemaRowInfo(row);

                    TableName tablename_in_query = row_info.GetTableName();

                    string columnName = row_info.BaseColumnName;

                    if (tablename_in_query == _tablename && columnName == prikeyname)
                    {
                        found = true;
                        break;
                    }
                }

                if (found == true)
                    _matchingprikeys.Add(prikeyname);
                else
                    _missingprikeys.Add(prikeyname);
            }
        }

        /// <summary>
        /// Fills the _matchingothercolumns and _missingothercolumns Lists.
        /// </summary> 
        private void FillMatchingAndMissingOtherColumnsList(DataTable adoschematable)
        {
            foreach (string columnname in _othercolumns)
            {
                bool found = false;

                foreach (DataRow row in adoschematable.Rows)
                {
                    SchemaRowInfo row_info = new SchemaRowInfo(row);

                    TableName tablename_in_query = row_info.GetTableName();

                    string columnname_in_query = row_info.BaseColumnName;

                    if (tablename_in_query == _tablename && columnname_in_query == columnname)
                    {
                        found = true;
                        break;
                    }
                }

                if (found == true)
                    _matchingothercolumns.Add(columnname);
                else
                    _missingothercolumns.Add(columnname);
            }
        }



    }

} // end of namespace


/*
        public string MainTablePrimaryKeyInfo()
        {
            if (_maintableprimarykeycolumnnames.Count == 0)
                return "Resultset does not contain any primary keys for the main table.";

            StringBuilder sb = new StringBuilder();
            sb.Append("Main table's primary keys: ");

            int count = 0;

            foreach (string name in _maintableprimarykeycolumnnames)
            {
                if (count != 0)
                    sb.Append(", ");

                sb.Append(name); ;
                count++;
            }

            return sb.ToString();
        }

        public string MatchingPriKeysInfo()
        {
            if (_matchingprikeycolumnnames.Count == 0)
                return "Resultset is not matching any primary keys columns for the main table.";

            StringBuilder sb = new StringBuilder();
            sb.Append("Main table matching primary keys in this resultset: ");

            int count = 0;

            foreach (string name in _matchingprikeycolumnnames)
            {
                if (count != 0)
                    sb.Append(", ");

                sb.Append(name); ;
                count++;
            }

            return sb.ToString();
        }


        public string MissingPriKeysInfo()
        {
            if (_missingprikeycolumnnames.Count == 0)
                return "Resultset is not missing any primary keys columns for the main table.";

            StringBuilder sb = new StringBuilder();
            sb.Append("Main table missing primary keys in this resultset: ");

            int count = 0;

            foreach (string name in _missingprikeycolumnnames)
            {
                if (count != 0)
                    sb.Append(", ");

                sb.Append(name); ;
                count++;
            }

            return sb.ToString();
        }
*/
