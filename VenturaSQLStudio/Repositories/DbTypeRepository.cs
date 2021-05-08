using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.Linq;

namespace VenturaSQLStudio {
    public class DbTypeRepository : ObservableCollection<DbTypeRepositoryItem>
    {
        public DbTypeRepository()
        {
            FillByTask();
            FillByCodeType();
            FillByDbType();
        }

        #region By Task
        private void FillByTask()
        {
            this.Add(new DbTypeRepositoryItem("task_int32")
            {
                Group = ParameterGroup.ByTask,
                Title = "Integer",
                Description = "Set the parameter to an Int32 type.",
                BasedOn = DbType.Int32 // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("task_string")
            {
                Group = ParameterGroup.ByTask,
                Title = "String",
                Description = "Set the parameter to a String.",
                BasedOn = DbType.String // will extract the data from the specified DbType
            });
        }
        #endregion

        #region By .NET Framework Type
        private void FillByCodeType()
        {
            /// Based on the System.TypeCode enum and the VenturaSqlCode enum.

            // Not added yet:

            // An integral type representing signed 8-bit integers with values between -128 and 127.
            // SByte   = 5,

            // An integral type representing unsigned 16-bit integers with values between 0 and 65535.
            //UInt16 = 8,

            ///// An integral type representing unsigned 32-bit integers with values between 0 and 4294967295.
            //UInt32 = 10,

            ///// An integral type representing unsigned 64-bit integers with values between 0 and 18446744073709551615.
            //UInt64 = 12,


            this.Add(new DbTypeRepositoryItem("code_boolean")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Boolean (bool)",
                Description = "A simple type representing Boolean values of true or false.",
                BasedOn = DbType.Boolean // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_byte")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Byte (byte)",
                Description = "An integral type representing unsigned 8-bit integers with values between 0 and 255.",
                BasedOn = DbType.Byte // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_datetime")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "DateTime (DateTime)",
                Description = "A type representing a date and time value.",
                BasedOn = DbType.DateTime // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_decimal")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Decimal (decimal)",
                Description = "A simple type representing values ranging from 1.0 x 10 -28 to approximately 7.9 x 10 28 with 28-29 significant digits.",
                BasedOn = DbType.Decimal // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_single")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Single (float)",
                Description = "A floating point type representing values ranging from approximately 1.5 x 10 -45 to 3.4 x 10 38 with a precision of 7 digits.",
                BasedOn = DbType.Single // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_double")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Double (double)",
                Description = "A floating point type representing values ranging from approximately 5.0 x 10 -324 to 1.7 x 10 308 with a precision of 15-16 digits.",
                BasedOn = DbType.Double // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_int16")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Int16 (short)",
                Description = "An integral type representing signed 16-bit integers with values between -32768 and 32767.",
                BasedOn = DbType.Int16 // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_int32")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Int32 (int)",
                Description = "An integral type representing signed 32-bit integers with values between -2147483648 and 2147483647.",
                BasedOn = DbType.Int32 // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_int64")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Int64 (long)",
                Description = "An integral type representing signed 64-bit integers with values between -9223372036854775808 and 9223372036854775807.",
                BasedOn = DbType.Int64 // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_string")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "String (string)",
                Description = "A sealed class type representing Unicode character strings.",
                BasedOn = DbType.String // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_guid")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Guid (Guid)",
                Description = "Represents a globally unique identifier (GUID).",
                BasedOn = DbType.Guid // will extract the data from the specified DbType
            });
            //Bytes = 12,
            this.Add(new DbTypeRepositoryItem("code_bytes")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Bytes (byte[])",
                Description = "An array of unsigned 8-bit integers with values between 0 and 255.",
                BasedOn = DbType.Binary // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_object")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "Object (object)",
                Description = "A general type representing any reference or value type not explicitly represented by another Type.",
                BasedOn = DbType.Object // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_timespan")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "TimeSpan",
                Description = "Represents a time interval.",
                BasedOn = DbType.Time // will extract the data from the specified DbType
            });

            this.Add(new DbTypeRepositoryItem("code_datetimeoffset")
            {
                Group = ParameterGroup.ByCodeType,
                Title = "DateTimeOffset",
                Description = "Represents a point in time, typically expressed as a date and time of day, relative to Coordinated Universal Time (UTC).",
                BasedOn = DbType.DateTimeOffset // will extract the data from the specified DbType
            });

        }
        #endregion

        private void FillByDbType()
        {
            // DbType is enum that sets the actual DbParameter.DbType.

            this.Add(new DbTypeRepositoryItem("dbtype_ansistring")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.AnsiString,
                FrameworkType = typeof(string),
                Title = "DbType.AnsiString (varchar) (string)",
                Description = "A variable-length stream of non-Unicode characters ranging between 1 and 8,000 characters.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_binary")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Binary,
                FrameworkType = typeof(byte[]),
                Title = "DbType.Binary (binary, timestamp, varbinary, image) (byte[])",
                Description = "A variable-length stream of binary data ranging between 1 and 8,000 bytes.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_byte")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Byte,
                FrameworkType = typeof(byte),
                Title = "DbType.Byte (byte)",
                Description = "An 8-bit unsigned integer ranging in value from 0 to 255.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_boolean")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Boolean,
                FrameworkType = typeof(bool),
                Title = "DbType.Boolean (bool)",
                Description = "A simple type representing Boolean values of true or false.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_currency")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Currency,
                FrameworkType = typeof(decimal),
                Title = "DbType.Currency (decimal)",
                Description = "A currency value ranging from -2 63 (or -922,337,203,685,477.5808) to 2 63 -1 (or +922,337,203,685,477.5807) with an accuracy to a ten-thousandth of a currency unit.",
                SetPrecision = true,
                SetScale = true
            });

            this.Add(new DbTypeRepositoryItem("dbtype_date")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Date,
                FrameworkType = typeof(DateTime),
                Title = "DbType.Date (DateTime)",
                Description = "A type representing a date value.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_datetime")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.DateTime,
                FrameworkType = typeof(DateTime),
                Title = "DbType.DateTime",
                Description = "A type representing a date and time value.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_decimal")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Decimal,
                FrameworkType = typeof(decimal),
                Title = "DbType.Decimal (decimal)",
                Description = "A simple type representing values ranging from 1.0 x 10 -28 to approximately 7.9 x 10 28 with 28-29 significant digits.",
            });
            /// <summary>

            this.Add(new DbTypeRepositoryItem("dbtype_double")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Double,
                FrameworkType = typeof(double),
                Title = "DbType.Double (double)",
                Description = "A floating point type representing values ranging from approximately 5.0 x 10 -324 to 1.7 x 10 308 with a precision of 15-16 digits.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_guid")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Guid,
                FrameworkType = typeof(Guid),
                Title = "DbType.Guid",
                Description = "A globally unique identifier (or GUID).",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_int16")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Int16,
                FrameworkType = typeof(Int16),
                Title = "DbType.Int16 (short)",
                Description = "An integral type representing signed 16-bit integers with values between -32768 and 32767.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_int32")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Int32,
                FrameworkType = typeof(Int32),
                Title = "DbType.Int32 (int)",
                Description = "An integral type representing signed 32-bit integers with values between -2147483648 and 2147483647.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_int64")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Int64,
                FrameworkType = typeof(Int64),
                Title = "DbType.Int64 (long)",
                Description = "An integral type representing signed 64-bit integers with values between -9223372036854775808 and 9223372036854775807.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_object")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Object,
                FrameworkType = typeof(object),
                Title = "DbType.Object (object)",
                Description = "A general type representing any reference or value type not explicitly represented by another DbType value.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_sbyte")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.SByte,
                FrameworkType = typeof(sbyte),
                Title = "DbType.SByte (sbyte)",
                Description = "An integral type representing signed 8-bit integers with values between -128 and 127.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_single")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Single,
                FrameworkType = typeof(float),
                Title = "DbType.Single (float)",
                Description = "A floating point type representing values ranging from approximately 1.5 x 10 -45 to 3.4 x 10 38 with a precision of 7 digits.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_string")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.String,
                FrameworkType = typeof(string),
                Title = "DbType.String (string)",
                Description = "A type representing Unicode character strings.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_time")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Time,
                FrameworkType = typeof(TimeSpan),
                Title = "DbType.Time (TimeSpan)",
                Description = "A type representing a SQL Server DateTime value. If you want to use a SQL Server time value, use System.Data.SqlDbType.Time.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_uint16")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.UInt16,
                FrameworkType = typeof(UInt16),
                Title = "DbType.UInt16 (ushort)",
                Description = "An integral type representing unsigned 16-bit integers with values between 0 and 65535.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_uint32")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.UInt32,
                FrameworkType = typeof(UInt32),
                Title = "DbType.UInt32 (uint)",
                Description = "An integral type representing unsigned 32-bit integers with values between 0 and 4294967295.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_uint64")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.UInt64,
                FrameworkType = typeof(UInt64),
                Title = "DbType.UInt64 (ulong)",
                Description = "An integral type representing unsigned 64-bit integers with values between 0 and 18446744073709551615.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_varnumeric")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.VarNumeric,
                FrameworkType = typeof(decimal),
                Title = "DbType.VarNumeric (decimal)",
                Description = "A variable-length numeric value.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_ansistringfixedlength")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.AnsiStringFixedLength, // this is a char.
                FrameworkType = typeof(string),
                Title = "DbType.AnsiStringFixedLength (char) (string)",
                Description = "A fixed-length stream of non-Unicode characters.",

            });

            this.Add(new DbTypeRepositoryItem("dbtype_stringfixedlength")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.StringFixedLength, // this is a nchar
                FrameworkType = typeof(string),
                Title = "DbType.StringFixedLength (nchar) (string)",
                Description = "A fixed-length string of Unicode characters.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_xml")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.Xml,
                FrameworkType = typeof(string),
                Title = "DbType.Xml (string)",
                Description = "A parsed representation of an XML document or fragment.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_datetime2")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.DateTime2,
                FrameworkType = typeof(DateTime),
                Title = "DbType.DateTime2 (DateTime)",
                Description = "Date and time data. Date value range is from January 1,1 AD through December 31, 9999 AD. Time value range is 00:00:00 through 23:59:59.9999999 with an accuracy of 100 nanoseconds.",
            });

            this.Add(new DbTypeRepositoryItem("dbtype_datetimeoffset")
            {
                Group = ParameterGroup.ByDbType,
                DbType = DbType.DateTimeOffset,
                FrameworkType = typeof(DateTimeOffset),
                Title = "DbType.DateTimeOffset (DateTimeOffset)",
                Description = "Date and time data with time zone awareness. Date value range is from January 1,1 AD through December 31, 9999 AD. Time value range is 00:00:00 through 23:59:59.9999999 with an accuracy of 100 nanoseconds. Time zone value range is -14:00 through +14:00.",
            });

        }


    }
}
