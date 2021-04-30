using System;
using System.Collections.Generic;

namespace VenturaSQLStudio
{

    public static class VenturaCodeRepository
    {
        private static volatile VenturaCodeInfo[] _list;

        static VenturaCodeRepository() // A static constructor is guaranteed to be called only once. It is thread safe.
        {
            CreateList();
        }

        public static VenturaCodeInfo[] List
        {
            get { return _list; }
        }

        public static VenturaCodeInfo GetItem(VenturaCode venturacode)
        {
            for (int i = 0; i < _list.Length; i++)
            {
                if (_list[i].VenturaCode == venturacode)
                    return _list[i];
            }

            throw new InvalidOperationException($"VenturaCode {venturacode} not found in repository. Should not happen.");
        }

        /// <summary>
        /// Returns null if not found.
        /// </summary>
        public static VenturaCodeInfo GetItem(Type type)
        {
            for (int i = 0; i < _list.Length; i++)
            {
                if (_list[i].Type == type)
                    return _list[i]; ;
            }

            return null;
        }

        public static string GetCSharpType(VenturaCode venturacode)
        {
            return GetItem(venturacode).CSharpType;
        }

        public static string GetCSharpTypeNullable(VenturaCode venturacode)
        {
            return GetItem(venturacode).CSharpTypeNullable;
        }

        public static bool GetIsValueType(VenturaCode venturacode)
        {
            return GetItem(venturacode).IsValueType;
        }


        /// <summary>
        /// Only to be called by static constructor.
        /// </summary>
        private static void CreateList()
        {
            List<VenturaCodeInfo> temp_list = new List<VenturaCodeInfo>();

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Boolean)
            {
                Type = typeof(bool),
                CSharpType = "bool",
                CSharpTypeNullable = "bool?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Byte)
            {
                Type = typeof(byte),
                CSharpType = "byte",
                CSharpTypeNullable = "byte?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.DateTime)
            {
                Type = typeof(DateTime),
                CSharpType = "DateTime",
                CSharpTypeNullable = "DateTime?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Decimal)
            {
                Type = typeof(decimal),
                CSharpType = "decimal",
                CSharpTypeNullable = "decimal?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Single)
            {
                Type = typeof(float),
                CSharpType = "float",
                CSharpTypeNullable = "float?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Double)
            {
                Type = typeof(double),
                CSharpType = "double",
                CSharpTypeNullable = "double?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Int16)
            {
                Type = typeof(short),
                CSharpType = "short",
                CSharpTypeNullable = "short?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Int32)
            {
                Type = typeof(int),
                CSharpType = "int",
                CSharpTypeNullable = "int?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Int64)
            {
                Type = typeof(long),
                CSharpType = "long",
                CSharpTypeNullable = "long?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.String)
            {
                Type = typeof(string),
                CSharpType = "string",
                CSharpTypeNullable = "string",
                IsValueType = false
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Guid)
            {
                Type = typeof(Guid),
                CSharpType = "Guid",
                CSharpTypeNullable = "Guid?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Bytes)
            {
                Type = typeof(byte[]),
                CSharpType = "byte[]",
                CSharpTypeNullable = "byte[]",
                IsValueType = false
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.Object)
            {
                Type = typeof(object),
                CSharpType = "object",
                CSharpTypeNullable = "object",
                IsValueType = false
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.TimeSpan)
            {
                Type = typeof(TimeSpan),
                CSharpType = "TimeSpan",
                CSharpTypeNullable = "TimeSpan?",
                IsValueType = true
            });

            temp_list.Add(new VenturaCodeInfo(VenturaCode.DateTimeOffset)
            {
                Type = typeof(DateTimeOffset),
                CSharpType = "DateTimeOffset",
                CSharpTypeNullable = "DateTimeOffset?",
                IsValueType = true
            });

            _list = temp_list.ToArray();
        }

    } // End of class VenturaCodeRepository


    public class VenturaCodeInfo
    {
        private VenturaCode _venturacode;

        public VenturaCodeInfo(VenturaCode venturacode)
        {
            _venturacode = venturacode;
        }

        public VenturaCode VenturaCode
        {
            get { return _venturacode; }
        }

        public Type Type { get; set; }

        public string CSharpType { get; set; }
        public string CSharpTypeNullable { get; set; }

        public bool IsValueType { get; set; }

        public string DisplayString
        {
            get { return $"{_venturacode} ({CSharpTypeNullable})"; }
        }

        public string DataString
        {
            get { return _venturacode.ToString(); }
        }

    }

    public enum VenturaCode : byte
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

    } // end of enum VenturaCode

} // End of namespace


//public Type GetColumnCodeType
//{
//    get
//    {
//        switch (this.ColumnCode)
//        {
//            case VenturaCode.Boolean:
//                //return typeof(System.Boolean);
//                return Type.GetType("System.Boolean");
//            case VenturaCode.Byte:
//                return Type.GetType("System.Byte");
//            case VenturaCode.DateTime:
//                return Type.GetType("System.DateTime");
//            case VenturaCode.Decimal:
//                return Type.GetType("System.Decimal");
//            case VenturaCode.Single:
//                return Type.GetType("System.Single");
//            case VenturaCode.Double:
//                return Type.GetType("System.Double");
//            case VenturaCode.Int16:
//                return Type.GetType("System.Int16");
//            case VenturaCode.Int32:
//                return Type.GetType("System.Int32");
//            case VenturaCode.Int64:
//                return Type.GetType("System.Int64");
//            case VenturaCode.String:
//                return Type.GetType("System.String");
//            case VenturaCode.Guid:
//                return Type.GetType("System.Guid");
//            case VenturaCode.Bytes:
//                return typeof(System.Byte[]);
//                //return Type.GetType("System.Byte[]");
//        } // end of switch
//        return null;
//    } // get
//} // end of method