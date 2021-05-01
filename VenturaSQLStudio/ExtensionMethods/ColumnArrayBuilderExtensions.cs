using System;
using System.Data;
using VenturaSQL;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio
{
    /// <summary>
    /// Builds a list of VenturaColumn type columns that can be used to initialize a VenturaSchema object.
    /// </summary>
    public static class ColumnArrayBuilderExtensions
    {

        /// <summary>
        /// Schema will be read from QueryInfo.ResultSet. QueryInfo.ResultSet contains a DataTable with Schema information as returned by Ado.Net's GetSchemaTable() method.
        /// </summary>
        /// <param name="updateableTablename">
        /// The name of the updateable table as a fully qualified table name. This is used for setting the Updateable property.
        /// </param>
        public static void Add(this ColumnArrayBuilder builder, ResultSetInfo resultset, TableName updateableTablename)
        {
            if (resultset.AdoSchemaTable.Columns.IndexOf("IsHidden") != -1) // does the column exist?
                throw new VenturaException("The QueryInfo.AdoSchemaTable should have the IsHidden rows and column removed.");

            if (resultset.AdoSchemaTable.Rows.Count < 1)
                throw new VenturaException("The QueryInfo.AdoSchemaTable does not contain any rows.");

            for (int x = 0; x < resultset.AdoSchemaTable.Rows.Count; x++)
            {
                SchemaRowInfo row_info = new SchemaRowInfo(resultset.AdoSchemaTable.Rows[x]);

                TableName fullyQualifiedTablename = row_info.GetTableName();

                VenturaColumn column = new VenturaColumn(row_info.ColumnName, row_info.DataType, row_info.AllowDBNull);

                // BaseServerName/BaseCatalogName/BaseSchemaName/BaseTableName/BaseColumnName
                column.BaseServerName = row_info.BaseServerName;
                column.BaseCatalogName = row_info.BaseCatalogName;
                column.BaseSchemaName = row_info.BaseSchemaName;
                column.BaseTableName = row_info.BaseTableName;
                column.BaseColumnName = row_info.BaseColumnName;

                // sql data

                // note that GetSchemaTable() does not include the DbType.

                if (row_info.ColumnSize != int.MaxValue)
                    column.ColumnSize = row_info.ColumnSize;

                if (row_info.NumericPrecision != 255)
                    column.NumericPrecision = row_info.NumericPrecision;

                if (row_info.NumericScale != 255)
                    column.NumericScale = row_info.NumericScale;

                column.ProviderType = row_info.ProviderType;

                // bool section

                column.IsUnique = row_info.IsUnique;
                column.IsKey = row_info.IsKey;
                column.IsReadOnly = row_info.IsReadOnly;
                column.IsIdentity = row_info.IsIdentity;
                column.IsAutoIncrement = row_info.IsAutoIncrement;

                column.IsAliased = row_info.IsAliased;
                column.IsExpression = row_info.IsExpression;
                column.IsRowGuid = row_info.IsRowGuid;
                column.IsLong = row_info.IsLong;

                column.XmlSchemaCollectionDatabase = row_info.XmlSchemaCollectionDatabase;
                column.XmlSchemaCollectionOwningSchema = row_info.XmlSchemaCollectionOwningSchema;
                column.XmlSchemaCollectionName = row_info.XmlSchemaCollectionName;
                column.UdtAssemblyQualifiedName = row_info.UdtAssemblyQualifiedName;

                // For CData Software ADO.NET drivers.
                column.Description = row_info.Description;

                // For VenturaSQL internal use
                if (fullyQualifiedTablename != null && updateableTablename != null)
                    if (fullyQualifiedTablename == updateableTablename)
                        if (row_info.IsReadOnly == false && row_info.IsIdentity == false && row_info.IsAutoIncrement == false)
                            column.Updateable = true;

                builder.Add(column);

            } // foreach row

        }
    }
}