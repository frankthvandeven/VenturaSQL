using System;
using System.Collections.Generic;

namespace VenturaSQLStudio
{

    public static class VenturaSqlCodeRepository
    {
        private static volatile VenturaSqlCodeInfo[] _list;

        static VenturaSqlCodeRepository() // A static constructor is guaranteed to be called only once. It is thread safe.
        {
            CreateList();
        }

        public static VenturaSqlCodeInfo[] List
        {
            get { return _list; }
        }

        public static VenturaSqlCodeInfo GetItem(VenturaSqlCode venturasqlcode)
        {
            for (int i = 0; i < _list.Length; i++)
            {
                if (_list[i].VenturaSqlCode == venturasqlcode)
                    return _list[i];
            }

            throw new InvalidOperationException($"VenturaSqlCode {venturasqlcode} not found in repository. Should not happen.");
        }

        /// <summary>
        /// Returns null if not found.
        /// </summary>
        public static VenturaSqlCodeInfo GetItem(Type type)
        {
            for (int i = 0; i < _list.Length; i++)
            {
                if (_list[i].Type == type)
                    return _list[i]; ;
            }

            return null;
        }

        public static string GetCSharpType(VenturaSqlCode venturasqlcode)
        {
            return GetItem(venturasqlcode).CSharpType;
        }

        public static string GetCSharpTypeNullable(VenturaSqlCode venturasqlcode)
        {
            return GetItem(venturasqlcode).CSharpTypeNullable;
        }

        public static bool GetIsValueType(VenturaSqlCode venturasqlCode)
        {
            return GetItem(venturasqlCode).IsValueType;
        }


        /// <summary>
        /// Only to be called by static constructor.
        /// </summary>
        private static void CreateList()
        {
            List<VenturaSqlCodeInfo> temp_list = new List<VenturaSqlCodeInfo>();

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Boolean)
            {
                Type = typeof(bool),
                CSharpType = "bool",
                CSharpTypeNullable = "bool?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Byte)
            {
                Type = typeof(byte),
                CSharpType = "byte",
                CSharpTypeNullable = "byte?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.DateTime)
            {
                Type = typeof(DateTime),
                CSharpType = "DateTime",
                CSharpTypeNullable = "DateTime?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Decimal)
            {
                Type = typeof(decimal),
                CSharpType = "decimal",
                CSharpTypeNullable = "decimal?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Single)
            {
                Type = typeof(float),
                CSharpType = "float",
                CSharpTypeNullable = "float?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Double)
            {
                Type = typeof(double),
                CSharpType = "double",
                CSharpTypeNullable = "double?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Int16)
            {
                Type = typeof(short),
                CSharpType = "short",
                CSharpTypeNullable = "short?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Int32)
            {
                Type = typeof(int),
                CSharpType = "int",
                CSharpTypeNullable = "int?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Int64)
            {
                Type = typeof(long),
                CSharpType = "long",
                CSharpTypeNullable = "long?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.String)
            {
                Type = typeof(string),
                CSharpType = "string",
                CSharpTypeNullable = "string",
                IsValueType = false
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Guid)
            {
                Type = typeof(Guid),
                CSharpType = "Guid",
                CSharpTypeNullable = "Guid?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Bytes)
            {
                Type = typeof(byte[]),
                CSharpType = "byte[]",
                CSharpTypeNullable = "byte[]",
                IsValueType = false
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.Object)
            {
                Type = typeof(object),
                CSharpType = "object",
                CSharpTypeNullable = "object",
                IsValueType = false
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.TimeSpan)
            {
                Type = typeof(TimeSpan),
                CSharpType = "TimeSpan",
                CSharpTypeNullable = "TimeSpan?",
                IsValueType = true
            });

            temp_list.Add(new VenturaSqlCodeInfo(VenturaSqlCode.DateTimeOffset)
            {
                Type = typeof(DateTimeOffset),
                CSharpType = "DateTimeOffset",
                CSharpTypeNullable = "DateTimeOffset?",
                IsValueType = true
            });

            _list = temp_list.ToArray();
        }

    } // End of class VenturaSqlCodeRepository


    public class VenturaSqlCodeInfo
    {
        private VenturaSqlCode _venturasqlcode;

        public VenturaSqlCodeInfo(VenturaSqlCode venturasqlcode)
        {
            _venturasqlcode = venturasqlcode;
        }

        public VenturaSqlCode VenturaSqlCode
        {
            get { return _venturasqlcode; }
        }

        public Type Type { get; set; }

        public string CSharpType { get; set; }
        public string CSharpTypeNullable { get; set; }

        public bool IsValueType { get; set; }

        public string DisplayString
        {
            get { return $"{_venturasqlcode} ({CSharpTypeNullable})"; }
        }

        public string DataString
        {
            get { return _venturasqlcode.ToString(); }
        }

    }

    public enum VenturaSqlCode : byte
    {
        Boolean = 1,
        Byte = 2,
        DateTime = 3,
        Decimal = 4,
        Single = 5,
        Double = 6,
        Int16 = 7,
        Int32 = 8,
        Int64 = 9,
        String = 10,
        Guid = 11,
        Bytes = 12,
        Object = 13,
        TimeSpan = 14,
        DateTimeOffset = 15

    }

}


//public Type GetColumnCodeType
//{
//    get
//    {
//        switch (this.ColumnCode)
//        {
//            case VenturaSqlCode.Boolean:
//                //return typeof(System.Boolean);
//                return Type.GetType("System.Boolean");
//            case VenturaSqlCode.Byte:
//                return Type.GetType("System.Byte");
//            case VenturaSqlCode.DateTime:
//                return Type.GetType("System.DateTime");
//            case VenturaSqlCode.Decimal:
//                return Type.GetType("System.Decimal");
//            case VenturaSqlCode.Single:
//                return Type.GetType("System.Single");
//            case VenturaSqlCode.Double:
//                return Type.GetType("System.Double");
//            case VenturaSqlCode.Int16:
//                return Type.GetType("System.Int16");
//            case VenturaSqlCode.Int32:
//                return Type.GetType("System.Int32");
//            case VenturaSqlCode.Int64: 
//                return Type.GetType("System.Int64");
//            case VenturaSqlCode.String:
//                return Type.GetType("System.String");
//            case VenturaSqlCode.Guid:
//                return Type.GetType("System.Guid");
//            case VenturaSqlCode.Bytes:
//                return typeof(System.Byte[]);
//                //return Type.GetType("System.Byte[]");
//        } // end of switch
//        return null;
//    } // get
//} // end of method