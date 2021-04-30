using System.Collections.Generic;
using VenturaSQL;

namespace VenturaSQLStudio
{
    public static class TargetPlatformList
    {
        public static List<TargetPlatformListItem> GetList
        {
            get
            {
                List<TargetPlatformListItem> list = new List<TargetPlatformListItem>();

                list.Add(new TargetPlatformListItem(".NET Standard", VenturaPlatform.NETStandard, AdoSupport.YesUserCanChoose));

                // Long ago...
                //list.Add(new TargetPlatformListItem("Asp.Net", VenturaPlatform.AspNet, AdoSupport.Obligated));
                //list.Add(new TargetPlatformListItem("WPF", VenturaPlatform.WPF, AdoSupport.YesUserCanChoose));
                //list.Add(new TargetPlatformListItem("WinForms", VenturaPlatform.WinForms, AdoSupport.YesUserCanChoose));
                //list.Add(new TargetPlatformListItem("UWP (Windows 10 App)", VenturaPlatform.UWP, AdoSupport.NoAdo));
                //list.Add(new TargetPlatformListItem("Xamarin.Android", VenturaPlatform.Android, AdoSupport.NoAdo));
                //list.Add(new TargetPlatformListItem("Xamarin.iOS", VenturaPlatform.iOS, AdoSupport.NoAdo));

                return list;
            }
        }

        public static TargetPlatformListItem FindItem(string targetplatform_as_string)
        {
            List<TargetPlatformListItem> list = GetList;

            return list.Find(a => a.DataString == targetplatform_as_string);
        }

        //public static ParameterTypeListItem FindItem(VenturaSqlDbType venturasqldbtype)
        //{
        //    if (_staticlist == null)
        //        _staticlist = new ParameterTypeList().GetList();

        //    return _staticlist.Find(a => a.VenturaSqlDbType == venturasqldbtype);
        //}

    }

    public class TargetPlatformListItem
    {
        private string _fullname;
        private VenturaPlatform _venturaplatform;
        private AdoSupport _ado_support;

        public TargetPlatformListItem(string fullname, VenturaPlatform venturaplatform, AdoSupport ado_support)
        {
            _fullname = fullname;
            _venturaplatform = venturaplatform;
            _ado_support = ado_support;
        }

        public string FullName
        {
            get { return _fullname; }
        }

        public string DataString
        {
            get
            {
                return _venturaplatform.ToString();
            }
        }

        public VenturaPlatform VenturaPlatform
        {
            get { return _venturaplatform; }
        }

        public AdoSupport AdoSupport
        {
            get { return _ado_support; }
        }

    }

    public enum AdoSupport
    {
        NoAdo,
        YesUserCanChoose,
        Obligated
    }
}
