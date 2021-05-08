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

                list.Add(new TargetPlatformListItem(".NET Standard", VenturaSqlPlatform.NETStandard, AdoSupport.YesUserCanChoose));

                // Long ago...
                //list.Add(new TargetPlatformListItem("Asp.Net", VenturaSqlPlatform.AspNet, AdoSupport.Obligated));
                //list.Add(new TargetPlatformListItem("WPF", VenturaSqlPlatform.WPF, AdoSupport.YesUserCanChoose));
                //list.Add(new TargetPlatformListItem("WinForms", VenturaSqlPlatform.WinForms, AdoSupport.YesUserCanChoose));
                //list.Add(new TargetPlatformListItem("UWP (Windows 10 App)", VenturaSqlPlatform.UWP, AdoSupport.NoAdo));
                //list.Add(new TargetPlatformListItem("Xamarin.Android", VenturaSqlPlatform.Android, AdoSupport.NoAdo));
                //list.Add(new TargetPlatformListItem("Xamarin.iOS", VenturaSqlPlatform.iOS, AdoSupport.NoAdo));

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
        private VenturaSqlPlatform _venturasqlplatform;
        private AdoSupport _ado_support;

        public TargetPlatformListItem(string fullname, VenturaSqlPlatform venturasqlplatform, AdoSupport ado_support)
        {
            _fullname = fullname;
            _venturasqlplatform = venturasqlplatform;
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
                return _venturasqlplatform.ToString();
            }
        }

        public VenturaSqlPlatform VenturaSqlPlatform
        {
            get { return _venturasqlplatform; }
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
