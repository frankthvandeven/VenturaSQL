using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace VenturaSQLStudio
{

    public static class UDC_Repository
    {
        private static ObservableCollection<UDC_RepositoryItem> _list;

        static UDC_Repository() // A static constructor is guaranteed to be called only once. It is thread safe.
        {
            _list = new ObservableCollection<UDC_RepositoryItem>();

            _list.Add(new UDC_RepositoryItem("Boolean (bool)", typeof(bool)));
            _list.Add(new UDC_RepositoryItem("Byte (byte)", typeof(byte)));
            _list.Add(new UDC_RepositoryItem("DateTime", typeof(DateTime)));
            _list.Add(new UDC_RepositoryItem("Decimal (decimal)", typeof(decimal)));
            _list.Add(new UDC_RepositoryItem("Single (float)", typeof(float)));
            _list.Add(new UDC_RepositoryItem("Double (double)", typeof(double)));
            _list.Add(new UDC_RepositoryItem("Int16 (short)", typeof(short)));
            _list.Add(new UDC_RepositoryItem("Int32 (int)", typeof(int)));
            _list.Add(new UDC_RepositoryItem("Int64 (long)", typeof(long)));
            _list.Add(new UDC_RepositoryItem("String (string)", typeof(string)));
            _list.Add(new UDC_RepositoryItem("Guid", typeof(Guid)));
            _list.Add(new UDC_RepositoryItem("Bytes (byte[])", typeof(byte[])));
            _list.Add(new UDC_RepositoryItem("Object (object)", typeof(object)));
            _list.Add(new UDC_RepositoryItem("TimeSpan", typeof(TimeSpan)));
            _list.Add(new UDC_RepositoryItem("DateTimeOffset", typeof(DateTimeOffset)));
        }

        public static ObservableCollection<UDC_RepositoryItem> List
        {
            get { return _list; }
        }

    } // End of class
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
