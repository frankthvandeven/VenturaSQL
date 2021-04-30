using Microsoft.SqlServer.Server;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Data.SqlTypes;
using System.Linq;
using System.Windows;

namespace VenturaSQLStudio {
    public class MetaTypeRepository : ObservableCollection<MetaType>
    {

        public MetaTypeRepository()
        {

            this.Add(new MetaType(19, 255, 8, true, false, false, 127, 38, "bigint", typeof(long), typeof(SqlInt64), SqlDbType.BigInt, DbType.Int64, 0));
            this.Add(new MetaType(15, 255, 8, true, false, false, 62, 109, "float", typeof(double), typeof(SqlDouble), SqlDbType.Float, DbType.Double, 0));
            this.Add(new MetaType(7, 255, 4, true, false, false, 59, 109, "real", typeof(float), typeof(SqlSingle), SqlDbType.Real, DbType.Single, 0));
            this.Add(new MetaType(255, 255, -1, false, false, false, 173, 173, "binary", typeof(byte[]), typeof(SqlBinary), SqlDbType.Binary, DbType.Binary, 2));
            this.Add(new MetaType(255, 255, -1, false, false, false, 173, 173, "timestamp", typeof(byte[]), typeof(SqlBinary), SqlDbType.Timestamp, DbType.Binary, 2));
            this.Add(new MetaType(255, 255, -1, false, false, false, 165, 165, "varbinary", typeof(byte[]), typeof(SqlBinary), SqlDbType.VarBinary, DbType.Binary, 2));
            this.Add(new MetaType(255, 255, -1, false, true, true, 165, 165, "varbinary", typeof(byte[]), typeof(SqlBinary), SqlDbType.VarBinary, DbType.Binary, 2));
            this.Add(new MetaType(255, 255, -1, false, false, false, 37, 173, string.Empty, typeof(byte[]), typeof(SqlBinary), SqlDbType.Int | SqlDbType.SmallInt, DbType.Binary, 2));
            this.Add(new MetaType(255, 255, -1, false, true, false, 34, 34, "image", typeof(byte[]), typeof(SqlBinary), SqlDbType.Image, DbType.Binary, 0));
            this.Add(new MetaType(255, 255, 1, true, false, false, 50, 104, "bit", typeof(bool), typeof(SqlBoolean), SqlDbType.Bit, DbType.Boolean, 0));
            this.Add(new MetaType(3, 255, 1, true, false, false, 48, 38, "tinyint", typeof(byte), typeof(SqlByte), SqlDbType.TinyInt, DbType.Byte, 0));
            this.Add(new MetaType(5, 255, 2, true, false, false, 52, 38, "smallint", typeof(short), typeof(SqlInt16), SqlDbType.SmallInt, DbType.Int16, 0));
            this.Add(new MetaType(10, 255, 4, true, false, false, 56, 38, "int", typeof(int), typeof(SqlInt32), SqlDbType.Int, DbType.Int32, 0));
            this.Add(new MetaType(255, 255, -1, false, false, false, 175, 175, "char", typeof(string), typeof(SqlString), SqlDbType.Char, DbType.AnsiStringFixedLength, 7));
            this.Add(new MetaType(255, 255, -1, false, false, false, 167, 167, "varchar", typeof(string), typeof(SqlString), SqlDbType.VarChar, DbType.AnsiString, 7));
            this.Add(new MetaType(255, 255, -1, false, true, true, 167, 167, "varchar", typeof(string), typeof(SqlString), SqlDbType.VarChar, DbType.AnsiString, 7));
            this.Add(new MetaType(255, 255, -1, false, true, false, 35, 35, "text", typeof(string), typeof(SqlString), SqlDbType.Text, DbType.AnsiString, 0));
            this.Add(new MetaType(255, 255, -1, false, false, false, 239, 239, "nchar", typeof(string), typeof(SqlString), SqlDbType.NChar, DbType.StringFixedLength, 7));
            this.Add(new MetaType(255, 255, -1, false, false, false, 231, 231, "nvarchar", typeof(string), typeof(SqlString), SqlDbType.NVarChar, DbType.String, 7));
            this.Add(new MetaType(255, 255, -1, false, true, true, 231, 231, "nvarchar", typeof(string), typeof(SqlString), SqlDbType.NVarChar, DbType.String, 7));
            this.Add(new MetaType(255, 255, -1, false, true, false, 99, 99, "ntext", typeof(string), typeof(SqlString), SqlDbType.NText, DbType.String, 7));
            this.Add(new MetaType(38, 4, 17, true, false, false, 108, 108, "decimal", typeof(decimal), typeof(SqlDecimal), SqlDbType.Decimal, DbType.Decimal, 2));
            this.Add(new MetaType(255, 255, -1, false, true, true, 241, 241, "xml", typeof(string), typeof(SqlXml), SqlDbType.Xml, DbType.Xml, 0));
            this.Add(new MetaType(23, 3, 8, true, false, false, 61, 111, "datetime", typeof(DateTime), typeof(SqlDateTime), SqlDbType.DateTime, DbType.DateTime, 0));
            this.Add(new MetaType(16, 0, 4, true, false, false, 58, 111, "smalldatetime", typeof(DateTime), typeof(SqlDateTime), SqlDbType.SmallDateTime, DbType.DateTime, 0));
            this.Add(new MetaType(19, 255, 8, true, false, false, 60, 110, "money", typeof(decimal), typeof(SqlMoney), SqlDbType.Money, DbType.Currency, 0));
            this.Add(new MetaType(10, 255, 4, true, false, false, 122, 110, "smallmoney", typeof(decimal), typeof(SqlMoney), SqlDbType.SmallMoney, DbType.Currency, 0));
            this.Add(new MetaType(255, 255, 16, true, false, false, 36, 36, "uniqueidentifier", typeof(Guid), typeof(SqlGuid), SqlDbType.UniqueIdentifier, DbType.Guid, 0));
            //this.Add(new MetaType(255, 255, -1, true, false, false, 98, 98, "sql_variant", typeof(object), typeof(object), SqlDbType.Variant, DbType.Object, 0));
            this.Add(new MetaType(255, 255, -1, false, false, true, 240, 240, "udt", typeof(object), typeof(object), SqlDbType.Udt, DbType.Object, 0));
            //this.Add(new MetaType(255, 255, -1, false, false, false, 243, 243, "table", typeof(IEnumerable<DbDataRecord>), typeof(IEnumerable<DbDataRecord>), SqlDbType.Structured, DbType.Object, 0));
            //this.Add(new MetaType(255, 255, -1, false, false, false, 31, 31, "", typeof(SqlDataRecord), typeof(SqlDataRecord), SqlDbType.Structured, DbType.Object, 0));
            this.Add(new MetaType(255, 255, 3, true, false, false, 40, 40, "date", typeof(DateTime), typeof(DateTime), SqlDbType.Date, DbType.Date, 0));
            this.Add(new MetaType(255, 7, -1, false, false, false, 41, 41, "time", typeof(TimeSpan), typeof(TimeSpan), SqlDbType.Time, DbType.Time, 1));
            this.Add(new MetaType(255, 7, -1, false, false, false, 42, 42, "datetime2", typeof(DateTime), typeof(DateTime), SqlDbType.DateTime2, DbType.DateTime2, 1));
            this.Add(new MetaType(255, 7, -1, false, false, false, 43, 43, "datetimeoffset", typeof(DateTimeOffset), typeof(DateTimeOffset), SqlDbType.DateTimeOffset, DbType.DateTimeOffset, 1));
        }

        public void SqlTypeNameToDbType(string sql_typename, out DbType dbtype, out Type frameworktype)
        {
            SqlDbType sqldbtype = (SqlDbType)Enum.Parse(typeof(SqlDbType), sql_typename);

            MetaType metatype = this.FirstOrDefault(z => z.SqlDbType == sqldbtype);

            if (metatype == null)
            {
                MessageBox.Show($"MetaType did not find {sqldbtype}. Contact support for help converting this venproj file.");
                Application.Current.Shutdown();
            }

            dbtype = metatype.DbType;
            frameworktype = metatype.ClassType;
        }

    }
}