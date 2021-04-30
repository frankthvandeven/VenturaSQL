using Microsoft.Data.Sqlite;
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;

namespace VenturaSQLStudio
{
    internal static class DbConnectionExtensions
    {
        internal static DataTable GetSchemaExtension(this DbConnection connection, string collectionName)
        {
            if (collectionName.ToUpper() != "TABLES")
                throw new ArgumentOutOfRangeException("Only collectionName 'TABLES' is supported.");

            if (connection is Microsoft.Data.Sqlite.SqliteConnection)
            {
                return MicrosoftSqLiteSchema((Microsoft.Data.Sqlite.SqliteConnection)connection);
            }
            else
            {
                return connection.GetSchema(collectionName);
            }
        }

        /// <summary>
        /// Retrieves table schema information for the database
        /// </summary>
        private static DataTable MicrosoftSqLiteSchema(Microsoft.Data.Sqlite.SqliteConnection connection)
        {
            DataTable dataTable = new DataTable("Tables")
            {
                Locale = CultureInfo.InvariantCulture
            };

            dataTable.Columns.Add("TABLE_CATALOG", typeof(string));
            dataTable.Columns.Add("TABLE_SCHEMA", typeof(string));
            dataTable.Columns.Add("TABLE_NAME", typeof(string));
            dataTable.Columns.Add("TABLE_TYPE", typeof(string));
            dataTable.Columns.Add("TABLE_ID", typeof(long));
            dataTable.Columns.Add("TABLE_ROOTPAGE", typeof(int));
            dataTable.Columns.Add("TABLE_DEFINITION", typeof(string));

            dataTable.BeginLoadData();

            string catalog = "main";
            string masterTableName = "sqlite_master";
            string sql_statement = $"SELECT [type], [name], [tbl_name], [rootpage], [sql], [rowid] FROM [{catalog}].[{masterTableName}] WHERE [type] LIKE 'table'";

            string strType = null;
            string strTable = null;

            using (SqliteCommand sQLiteCommand = new SqliteCommand(sql_statement, connection))
            {
                using (SqliteDataReader datareader = sQLiteCommand.ExecuteReader())
                {
                    while (datareader.Read())
                    {
                        string str = datareader.GetString(0);
                        if (string.Compare(datareader.GetString(2), 0, "SQLITE_", 0, 7, StringComparison.OrdinalIgnoreCase) == 0)
                        {
                            str = "SYSTEM_TABLE";
                        }
                        if (string.Compare(strType, str, StringComparison.OrdinalIgnoreCase) != 0 && strType != null || string.Compare(datareader.GetString(2), strTable, StringComparison.OrdinalIgnoreCase) != 0 && strTable != null)
                        {
                            continue;
                        }
                        DataRow row = dataTable.NewRow();
                        row["TABLE_CATALOG"] = catalog;
                        row["TABLE_NAME"] = datareader.GetString(2);
                        row["TABLE_TYPE"] = str;
                        row["TABLE_ID"] = datareader.GetInt64(5);
                        row["TABLE_ROOTPAGE"] = datareader.GetInt32(3);
                        row["TABLE_DEFINITION"] = datareader.GetString(4);
                        dataTable.Rows.Add(row);
                    }
                }
            }

            dataTable.AcceptChanges();
            dataTable.EndLoadData();

            return dataTable;
        }



    }
}
