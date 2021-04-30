using System;
using System.Data;

namespace VenturaSQLStudio {
    public class MetaType
    {
        internal readonly Type ClassType;
        internal readonly Type SqlType;
        internal readonly int FixedLength;
        internal readonly bool IsFixed;
        internal readonly bool IsLong;
        internal readonly bool IsPlp;
        internal readonly byte Precision;
        internal readonly byte Scale;
        internal readonly byte TDSType;
        internal readonly byte NullableType;
        internal readonly string TypeName;
        internal readonly SqlDbType SqlDbType;
        internal readonly DbType DbType;
        internal readonly byte PropBytes;
        internal readonly bool IsAnsiType;
        internal readonly bool IsBinType;
        internal readonly bool IsCharType;
        internal readonly bool IsNCharType;
        internal readonly bool IsSizeInCharacters;
        internal readonly bool IsNewKatmaiType;
        internal readonly bool IsVarTime;
        internal readonly bool Is70Supported;
        internal readonly bool Is80Supported;
        internal readonly bool Is90Supported;
        internal readonly bool Is100Supported;

        public MetaType(byte precision, byte scale, int fixedLength, bool isFixed, bool isLong, bool isPlp, byte tdsType, byte nullableTdsType, string typeName, Type classType, Type sqlType, SqlDbType sqldbType, DbType dbType, byte propBytes)
        {
            this.Precision = precision;
            this.Scale = scale;
            this.FixedLength = fixedLength;
            this.IsFixed = isFixed;
            this.IsLong = isLong;
            this.IsPlp = isPlp;
            this.TDSType = tdsType;
            this.NullableType = nullableTdsType;
            this.TypeName = typeName;
            this.SqlDbType = sqldbType;
            this.DbType = dbType;
            this.ClassType = classType;
            this.SqlType = sqlType;
            this.PropBytes = propBytes;
            this.IsAnsiType = _IsAnsiType(sqldbType);
            this.IsBinType = _IsBinType(sqldbType);
            this.IsCharType = _IsCharType(sqldbType);
            this.IsNCharType = _IsNCharType(sqldbType);
            this.IsSizeInCharacters = _IsSizeInCharacters(sqldbType);
            this.IsNewKatmaiType = _IsNewKatmaiType(sqldbType);
            this.IsVarTime = _IsVarTime(sqldbType);
            this.Is70Supported = _Is70Supported(this.SqlDbType);
            this.Is80Supported = _Is80Supported(this.SqlDbType);
            this.Is90Supported = _Is90Supported(this.SqlDbType);
            this.Is100Supported = _Is100Supported(this.SqlDbType);
        }

        private bool _Is100Supported(SqlDbType type)
        {
            if (_Is90Supported(type) || SqlDbType.Date == type || SqlDbType.Time == type || SqlDbType.DateTime2 == type)
            {
                return true;
            }
            return SqlDbType.DateTimeOffset == type;
        }

        private bool _Is70Supported(SqlDbType type)
        {
            if (type == SqlDbType.BigInt || type <= SqlDbType.BigInt)
            {
                return false;
            }
            return type <= SqlDbType.VarChar;
        }

        private bool _Is80Supported(SqlDbType type)
        {
            if (type < SqlDbType.BigInt)
            {
                return false;
            }
            return type <= SqlDbType.Variant;
        }

        private bool _Is90Supported(SqlDbType type)
        {
            if (_Is80Supported(type) || SqlDbType.Xml == type)
            {
                return true;
            }
            return SqlDbType.Udt == type;
        }

        private bool _IsAnsiType(SqlDbType type)
        {
            if (type == SqlDbType.Char || type == SqlDbType.VarChar)
            {
                return true;
            }
            return type == SqlDbType.Text;
        }

        private bool _IsBinType(SqlDbType type)
        {
            if (type == SqlDbType.Image || type == SqlDbType.Binary || type == SqlDbType.VarBinary || type == SqlDbType.Timestamp || type == SqlDbType.Udt)
            {
                return true;
            }
            return type == (SqlDbType.Int | SqlDbType.SmallInt);
        }

        private bool _IsCharType(SqlDbType type)
        {
            if (type == SqlDbType.NChar || type == SqlDbType.NVarChar || type == SqlDbType.NText || type == SqlDbType.Char || type == SqlDbType.VarChar || type == SqlDbType.Text)
            {
                return true;
            }
            return type == SqlDbType.Xml;
        }

        private bool _IsNCharType(SqlDbType type)
        {
            if (type == SqlDbType.NChar || type == SqlDbType.NVarChar || type == SqlDbType.NText)
            {
                return true;
            }
            return type == SqlDbType.Xml;
        }

        private bool _IsNewKatmaiType(SqlDbType type)
        {
            return SqlDbType.Structured == type;
        }

        private bool _IsSizeInCharacters(SqlDbType type)
        {
            if (type == SqlDbType.NChar || type == SqlDbType.NVarChar || type == SqlDbType.Xml)
            {
                return true;
            }
            return type == SqlDbType.NText;
        }

        internal bool _IsVarTime(SqlDbType type)
        {
            if (type == SqlDbType.Time || type == SqlDbType.DateTime2)
            {
                return true;
            }
            return type == SqlDbType.DateTimeOffset;
        }
    }

}