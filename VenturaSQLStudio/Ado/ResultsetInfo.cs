using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using VenturaSQL;

namespace VenturaSQLStudio.Ado
{
    /// <summary>
    /// 
    /// </summary>
    public class ResultSetInfo
    {
        private DataTable _adoschematable;
        private List<TableInfo> _tableinfo;

        public ResultSetInfo(DataTable adoschematable)
        {
            _adoschematable = adoschematable;
            _tableinfo = new List<TableInfo>();
        }

        /// <summary>
        /// Returns the raw schema information for the query's resultset.
        /// </summary>
        public DataTable AdoSchemaTable
        {
            get { return _adoschematable; }
        }

        /// <summary>
        /// Returns information on the tables referenced in the query resultset.
        /// </summary>
        public List<TableInfo> Tables
        {
            get { return _tableinfo; }
        }

        public void FillReferencedTablesList(AdoConnector connector, DbConnection connection, DbTransaction transaction)
        {
            DataRowCollection rows = _adoschematable.Rows;

            for (int i = 0; i < rows.Count; i++)
            {
                SchemaRowInfo row_info = new SchemaRowInfo(rows[i]);

                TableName table_name = row_info.GetTableName();

                if (table_name != null)
                {
                    TableInfo info_found = _tableinfo.Find(a => a.TableName == table_name);

                    if (info_found == null)
                    {
                        TableInfo new_info = new TableInfo(connector, connection, transaction, table_name, _adoschematable);

                        _tableinfo.Add(new_info);
                    }

                }
            }

        }

    } // end of class

} // end of namespace

