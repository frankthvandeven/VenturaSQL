using System.Collections.Generic;
using System.Data;

namespace VenturaSQLStudio
{
    // Anonymous type example:
    // var v = new { Amount = 108, Message = "Hello" };

    public static class VenturaSqlDbTypeRepository
    {
        private static volatile VenturaSqlDbTypeInfo[] _list;
        private static volatile VenturaSqlDbTypeInfo[] _parameterlist;

        static VenturaSqlDbTypeRepository() // A static constructor is guaranteed to be called only once. It is thread safe.
        {
            CreateLists();
        }

        public static VenturaSqlDbTypeInfo[] List
        {
            get { return _list; }
        }

        public static VenturaSqlDbTypeInfo[] ParameterList
        {
            get { return _parameterlist; }
        }

        public static VenturaSqlDbTypeInfo GetItem(SqlDbType venturasqldbtype)
        {
            for (int i = 0; i < _list.Length; i++)
            {
                if (_list[i].VenturaSqlDbType == venturasqldbtype)
                    return _list[i];
            }

            return null;
        }

        public static VenturaCode GetVenturaCode(SqlDbType venturasqldbtype)
        {
            return GetItem(venturasqldbtype).VenturaCode;
        }

        /// <summary>
        /// Only to be called by static constructor.
        /// </summary>
        private static void CreateLists()
        {
            List<VenturaSqlDbTypeInfo> temp_list = new List<VenturaSqlDbTypeInfo>();

            // There are 3 VenturaSqlDbType types that need a SPECIAL TREATMENT:
            //
            // 1. SqlDbType.Variant = The underderlying type is variable.
            // 2. SqlDbType.Udt = The underlying type is a CLR class type.
            // 3. SqlDbType.Structured = The underlying type is a DataTable. Not implemented yet. Also not: Structured only exists as an SqlParameter, and never in a returned Resultset.

            // There is one type that is not supported by Ventura in a RESULTSET in VenturaSQL Studio:
            // 1. SqlDbType.Variant. Not worth investing the time to support it.

            // There are 3 types that are not supported by Ventura as a PARAMETER in VenturaSQL Studio:
            // 1. SqlDbType.Variant. Not worth investing the time to support it.
            // 2. SqlDbType.Udt. Not yet supported, but planned for later. Problem: where to find the underlying CLR type from within VenturaSQL Studio?
            // 3. SqlDbType.Structured. Not yet supported. Planned for later.

            #region Hard coded information for eaxch (Ventura)SqlDbType.

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.BigInt, VenturaCode.Int64)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 8,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Binary, VenturaCode.Bytes)
            {
                FixedSizeOnDisk = false,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 8000,

                LengthMax = 8000,
                AllowMAXForLength = false,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Bit, VenturaCode.Boolean)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Char, VenturaCode.String)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 8000,

                LengthMax = 8000,
                AllowMAXForLength = false,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.DateTime, VenturaCode.DateTime)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 8,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Decimal, VenturaCode.Decimal)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 17,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = 1,
                PrecisionMax = 38, // For example 999.99 has a precision of 5 and a scale of 2.
                ScaleMin = 0,
                ScaleMax = 38 // scale must be less or equal to Precision.
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Float, VenturaCode.Double)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 8,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Image, VenturaCode.Bytes)
            {
                FixedSizeOnDisk = false,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 2147483647,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Int, VenturaCode.Int32)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 4,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Money, VenturaCode.Decimal)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 8,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.NChar, VenturaCode.String)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 2,
                MaximumPossibleNumberOfElements = 4000,

                LengthMax = 4000,
                AllowMAXForLength = false,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.NText, VenturaCode.String)
            {
                FixedSizeOnDisk = false,
                BytesPerElement = 2,
                MaximumPossibleNumberOfElements = 1073741823,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.NVarChar, VenturaCode.String)
            {
                FixedSizeOnDisk = false,
                BytesPerElement = 2,
                MaximumPossibleNumberOfElements = 1073741823,

                LengthMax = 4000,
                AllowMAXForLength = true,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Real, VenturaCode.Single)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 4,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.UniqueIdentifier, VenturaCode.Guid)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 16,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.SmallDateTime, VenturaCode.DateTime)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 4,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.SmallInt, VenturaCode.Int16)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 2,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.SmallMoney, VenturaCode.Decimal)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 4,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Text, VenturaCode.String)
            {
                FixedSizeOnDisk = false,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 2147483647,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Timestamp, VenturaCode.Bytes)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 8,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.TinyInt, VenturaCode.Byte)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.VarBinary, VenturaCode.Bytes)
            {
                FixedSizeOnDisk = false,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 8000,

                LengthMax = 8000,
                AllowMAXForLength = true,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.VarChar, VenturaCode.String)
            {
                FixedSizeOnDisk = false,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 2147483647,

                LengthMax = 8000,
                AllowMAXForLength = true,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Variant, VenturaCode.Object)
            {
                AllowedInResultset = false,
                AllowedInParameterList = false,

                FixedSizeOnDisk = false,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 8000,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });


            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Xml, VenturaCode.String)
            {
                FixedSizeOnDisk = false,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 2147483647,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            // The UDT is an special type, and will be recognized by Ventura and treated different from other types.
            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Udt, VenturaCode.Object)
            {
                AllowedInParameterList = false,

                FixedSizeOnDisk = false,
                BytesPerElement = 1,
                MaximumPossibleNumberOfElements = 2147483647,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            // Structured is only used in SqlParameters and will never be part of a Result set. This is a special type,
            // and will be recognized by Ventura and treated different from other types.
            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Structured, VenturaCode.Object)
            {
                AllowedInResultset = false, /* Will never be in Resultset anyway */
                AllowedInParameterList = false, /* Not yet implemented */

                FixedSizeOnDisk = true,
                BytesPerElement = -1,
                MaximumPossibleNumberOfElements = -1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            // Date data ranging in value from January 1,1 AD through December 31, 9999 AD.
            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Date, VenturaCode.DateTime)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 3,
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = null,
                ScaleMax = null
            });

            // Time data based on a 24-hour clock. Time value range is 00:00:00 through 23:59:59.9999999 with an accuracy of 100 nanoseconds.
            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.Time, VenturaCode.TimeSpan)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 5, /* time(0/1/2) is 3 bytes. time(3/4) is 4 bytes. time(5/6/7) is 5 bytes */
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = 0,
                ScaleMax = 7
            });

            // Date and time data. Date value range is from January 1,1 AD through December 31, 9999 AD.
            // Time value range is 00:00:00 through 23:59:59.9999999 with an accuracy of 100 nanoseconds.
            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.DateTime2, VenturaCode.DateTime)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 7, /* datetime2(0/1/2) is 6 bytes. time(3/4) is 7 bytes. time(5/6/7) is 8 bytes */
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = 0,
                ScaleMax = 7
            });

            // Date and time data with time zone awareness. Date value range is from January 1,1 AD through December 31, 9999 AD.
            // Time value range is 00:00:00 through 23:59:59.9999999 with an accuracy of 100 nanoseconds.Time zone value range is - 14:00 through + 14:00.
            temp_list.Add(new VenturaSqlDbTypeInfo(SqlDbType.DateTimeOffset, VenturaCode.DateTimeOffset)
            {
                FixedSizeOnDisk = true,
                BytesPerElement = 10, /* datetimeoffset(0/1/2) is 8 bytes. datetimeoffset(3/4) is 9 bytes. datetimeoffset(5/6/7) is 10 bytes */
                MaximumPossibleNumberOfElements = 1,

                LengthMax = null,
                AllowMAXForLength = null,

                PrecisionMin = null,
                PrecisionMax = null,
                ScaleMin = 0,
                ScaleMax = 7
            });

            #endregion

            _list = temp_list.ToArray();
            _parameterlist = temp_list.FindAll(a => a.AllowedInParameterList == true).ToArray();

        }

    } // End of class VenturaSqlDbTypeRepository


    public class VenturaSqlDbTypeInfo
    {
        private SqlDbType _venturasqldbtype;
        private VenturaCode _venturacode;

        private string _csharptype;
        private string _csharptypenullable;
        private bool _isvaluetype;

        internal VenturaSqlDbTypeInfo(SqlDbType venturasqldbtype, VenturaCode venturacode)
        {
            _venturasqldbtype = venturasqldbtype;
            _venturacode = venturacode;

            _csharptype = VenturaCodeRepository.GetCSharpType(venturacode);
            _csharptypenullable = VenturaCodeRepository.GetCSharpTypeNullable(venturacode);
            _isvaluetype = VenturaCodeRepository.GetIsValueType(venturacode);
        }

        public SqlDbType VenturaSqlDbType
        {
            get { return _venturasqldbtype; }
        }

        public VenturaCode VenturaCode
        {
            get { return _venturacode; }
        }

        public string CSharpType
        {
            get { return _csharptype; }
        }

        public string CSharpTypeNullable
        {
            get { return _csharptypenullable; }
        }

        /// <summary>
        /// Returns true if the underlying CLR type is a value type.
        /// If true, it means that the CLR type is NOT nullable.
        /// </summary>
        public bool IsValueType
        {
            get { return _isvaluetype; }
        }

        /// <summary>
        /// If false, then VenturaSQL Studio will report the SqlDbType as not supported when returned in a Result set and abort the operation.
        /// </summary>
        public bool AllowedInResultset { get; set; } = true;

        /// <summary>
        /// If true then this SqlDbType is available in the parameterlist in VenturaSQL Studio.
        /// </summary>
        public bool AllowedInParameterList { get; set; } = true;


        public int BytesPerElement { get; set; }
        public int MaximumPossibleNumberOfElements { get; set; }

        /// <summary>
        /// If false, we know is slower to store than a fixed size type.
        /// </summary>
        public bool FixedSizeOnDisk { get; set; }

        /// <summary>
        /// This is for setting SqlParameter.Size.
        /// When setting LengthMax to a non null value, you must also set AllowMAXForLength to a non null value.
        /// </summary>
        public int? LengthMax { get; set; } = null;

        /// <summary>
        /// This is for setting SqlParameter.Size. varchar(MAX) is set as .Size = -1.
        /// This is a different maximum length than MaxLength. For example VarChar is maximum 8000, but VarChar(MAX) is 2147483647.
        /// When setting AllowMAXForLength to a non null value, you must also set LengthMax to a non null value.
        /// </summary>
        public bool? AllowMAXForLength { get; set; } = null;

        /// <summary>
        /// This is for setting SqlParameter.Precision.
        /// When setting PrecisionMin to a non null value, you must also set PrecisionMax to a non null value.
        /// </summary>
        public int? PrecisionMin { get; set; } = null;

        /// <summary>
        /// This is for setting SqlParameter.Precision.
        /// When setting PrecisionMax to a non null value, you must also set PrecisionMin to a non null value.
        /// </summary>
        public int? PrecisionMax { get; set; } = null;

        /// <summary>
        /// This is for setting SqlParameter.Scale.
        /// When setting ScaleMin to a non null value, you must also set ScaleMax to a non null value.
        /// </summary>
        public int? ScaleMin { get; set; } = null;

        /// <summary>
        /// This is for setting SqlParameter.Scale.
        /// When setting ScaleMax to a non null value, you must also set ScaleMin to a non null value.
        /// </summary>
        public int? ScaleMax { get; set; } = null;

        /// <summary>
        /// For ComboBox.
        /// </summary>
        public string DisplayString
        {
            get { return _venturasqldbtype.ToString() + " (" + _csharptype + ")"; }
        }

        /// <summary>
        /// For ComboBox.
        /// </summary>
        public string DataString
        {
            get { return _venturasqldbtype.ToString(); }
        }



    }

} // End of namespace