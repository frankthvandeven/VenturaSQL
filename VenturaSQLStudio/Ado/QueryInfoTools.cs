using System;
using System.Collections.Generic;
using System.Text;
using System.Data;

namespace VenturaSQLStudio.Ado
{
    public static class QueryInfoTools
    {

        /// <summary>
        /// When retrieving the schema of a query resultset using DataReader.GetSchemaTable() the schema information will contain extra
        /// rows containing column information about primary keys used by the query. These rows (each row containing column information) all have
        /// the IsHidden column set to true. This method will remove all rows marked IsHidden, and finally remove the IsHidden column itself.
        /// </summary>
        public static void RemoveIsHiddenRowsFromSchemaTable(DataTable datatable_containing_schema)
        {
            if (datatable_containing_schema.Columns.IndexOf("IsHidden") == -1)
                return; // Nothing to do.

            //datatable.Columns["IsHidden"].ReadOnly = false;

            for (int x = datatable_containing_schema.Rows.Count - 1; x >= 0; x--)
            {
                DataRow row = datatable_containing_schema.Rows[x];
                if ( row["IsHidden"] != DBNull.Value)
                {
                    bool ishidden = (bool)row["IsHidden"];
                    if (ishidden == true)
                        row.Delete();
                }
            }

            datatable_containing_schema.Columns.Remove("IsHidden");
            datatable_containing_schema.AcceptChanges();
        }

        /// <summary>
        /// If the column does not exist, or has a value of DBNull then an exception is thrown.  
        /// </summary>
        public static T RowValue<T>(this DataRow ado_schema_row, string column_name)
        {
            if (ado_schema_row.ColumnExists(column_name) == false)
                throw new KeyNotFoundException($"Column {column_name} not in schema. ADO.NET Provider.");

            if (ado_schema_row[column_name] is DBNull)
                throw new NoNullAllowedException($"Schema column {column_name} is DBNull. Not allowed. ADO.NET Provider.");

            return (T)ado_schema_row[column_name];
        }

        /// <summary>
        /// If the column does not exist or the value is DBNull then the default value specified will be returned.
        /// </summary>
        public static T RowValue<T>(this DataRow ado_schema_row, string column_name, T default_value)
        {
            if (column_name == null || column_name == "")
                return default_value;

            if (ado_schema_row.ColumnExists(column_name) == false)
                return default_value;

            if (ado_schema_row[column_name] is DBNull)
                return default_value;

            return (T)ado_schema_row[column_name];
        }

        public static bool ColumnExists(this DataRow ado_schema_row, string column_name)
        {
            if (ado_schema_row.Table.Columns.Contains(column_name) == true)
                return true;

            return false;
        }


    } // end of class


} // end of namespace
