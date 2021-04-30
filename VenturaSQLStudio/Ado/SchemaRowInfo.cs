using System;
using System.Data;
using VenturaSQL;

namespace VenturaSQLStudio.Ado
{
    // Name              Type          Oracle       Twitter
    // ================  ============  ======       =======
    // ColumnName        string        Yes          Yes
    // ColumnOrdinal     int           Yes          Yes
    // ColumnSize        int           Yes          Yes
    // NumericPrecision  short         Yes (short)  Int32
    // NumericScale      short         Yes (short)  Int32
    // DataType          Type          Yes          Yes
    // ProviderType      int           Yes          Yes
    // IsLong            bool          Yes          No
    // AllowDBNull       bool          Yes          Yes
    // IsAliased         bool          Yes          No
    // IsExpression      bool          Yes          No
    // IsKey             bool          Yes          Yes (null)
    // IsUnique          bool          Yes          Yes
    // BaseServerName    string        No           Yes
    // BaseSchemaName    string        Yes          Yes
    // BaseTableName     string        Yes          Yes
    // BaseColumnName    string        Yes          Yes
    // 
    //
    // IsLong                          -            String
    // IsRowVersion                                 String


    /// <summary>
    /// Makes the datarow columns available as easy to access properties.
    /// Throws an exception when an expected column is missing, this can happen with a ADO.NET Provider.
    /// </summary>
    public class SchemaRowInfo
    {
        public string BaseServerName { get; } // default is empty string
        public string BaseCatalogName { get; } // default is empty string
        public string BaseSchemaName { get; } // default is empty string
        public string BaseTableName { get; } // default is empty string
        public string BaseColumnName { get; } // default is empty string.
        public bool IsKey { get; } // default is false
        public string ColumnName { get; }// must exist
        public int ColumnSize { get; } // must exist
        public byte NumericPrecision { get; } // default value is 255 (means not specified)
        public byte NumericScale { get; } // default value is 255 (means not specified)

        public int ProviderType { get; } // We assume every provider has this one. And as an Int32
        public Type DataType { get; } // must exist

        public bool IsUnique { get; } // default false
        public bool IsReadOnly { get; } // default false
        public bool IsIdentity { get; } // default false
        public bool IsAutoIncrement { get; } // default false

        public bool AllowDBNull { get; } // default true
        public bool IsAliased { get; } // default false
        public bool IsExpression { get; } // default false
        public bool IsRowGuid { get; } // default false
        public bool IsLong { get; } // default false

        public string XmlSchemaCollectionDatabase { get; } // default is empty string
        public string XmlSchemaCollectionOwningSchema { get; } // default is empty string
        public string XmlSchemaCollectionName { get; } // default is empty string
        public string UdtAssemblyQualifiedName { get; } // default is empty string

        public string Description { get; } // default is null. Support for CData Software's drivers!

        public SchemaRowInfo(DataRow ado_schema_row)
        {
            DataRow row = ado_schema_row; // shorten the name

            // The Base properties point to the original table-column where the column data came from.
            BaseServerName = row.RowValue("BaseServerName", "");
            BaseCatalogName = row.RowValue("BaseCatalogName", "");
            BaseSchemaName = row.RowValue("BaseSchemaName", "");
            BaseTableName = row.RowValue("BaseTableName", "");
            BaseColumnName = row.RowValue("BaseColumnName", "");

            if (BaseColumnName == "" & BaseTableName != "")
                throw new DataException($"Provider schema error. The provider returned a BaseTableName ('{BaseTableName}') but not a BaseColumnName.");

            IsKey = row.RowValue<bool>("IsKey", false); // default is false

            ColumnName = row.RowValue<string>("ColumnName"); // must exist and have a value.
            ColumnSize = row.RowValue<int>("ColumnSize", int.MaxValue);

            NumericPrecision = FindPrecision(row);
            NumericScale = FindScale(row);

            ProviderType = row.RowValue<int>("ProviderType", 0);


            // DataType
            Type type = row.RowValue<Type>("DataType", typeof(object));

            if (TypeTools.IsGenericTypeNullable(type))
                throw new DataException("The Provider returned a generic Nullable<> data type. Should not happen. Contact support.");

            DataType = type;

            // Flags
            IsUnique = row.RowValue("IsUnique", false); // default false
            IsReadOnly = row.RowValue("IsReadOnly", false); // default false
            IsIdentity = row.RowValue("IsIdentity", false); // default false
            IsAutoIncrement = row.RowValue("IsAutoIncrement", false); // default false

            AllowDBNull = row.RowValue<bool>("AllowDBNull", true); // default true
            IsAliased = row.RowValue("IsAliased", false); // default false
            IsExpression = row.RowValue("IsExpression", false); // default false
            IsRowGuid = row.RowValue("IsRowGuid", false); // default false
            IsLong = row.RowValue("IsLong", false); // default false

            XmlSchemaCollectionDatabase = row.RowValue<string>("XmlSchemaCollectionDatabase", ""); // default empty string
            XmlSchemaCollectionOwningSchema = row.RowValue<string>("XmlSchemaCollectionOwningSchema", ""); // default empty string
            XmlSchemaCollectionName = row.RowValue<string>("XmlSchemaCollectionName", ""); // default empty string
            UdtAssemblyQualifiedName = row.RowValue<string>("UdtAssemblyQualifiedName", ""); // default empty string

            // CData Software specific
            Description = row.RowValue<string>("Description", null); // default is null
        }

        private byte FindPrecision(DataRow row)
        {
            // NumericPrecision can be int/short/byte. 
            object o = row.RowValue<object>("NumericPrecision", null);

            if (o == null)
                return 255;

            int int_value = Convert.ToInt32(o);

            if (int_value < 0 || int_value > 255)
                int_value = 255;

            return (byte)int_value;
        }

        private byte FindScale(DataRow row)
        {
            // NumericScale can be int/short/byte. 
            object o = row.RowValue<object>("NumericScale", null);

            if (o == null)
                return 255;

            int int_value = Convert.ToInt32(o);

            if (int_value < 0 || int_value > 255)
                int_value = 255;

            return (byte)int_value;
        }

        public TableName GetTableName()
        {
            if (BaseTableName == "")
                return null;

            return new TableName(BaseServerName, BaseCatalogName, BaseSchemaName, BaseTableName);
        }

    } // end of class

} // end of namespace

