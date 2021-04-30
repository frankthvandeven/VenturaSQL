using System;

using System.Reflection; // Contains extension method GetTypeInfo()

namespace VenturaSQL
{
    /// <summary>
    /// New general functions for Ventura. Multi platform.
    /// </summary>
    public class NewTools
    {

        public static Version VenturaVersion
        {
            get
            {
                return typeof(NewTools).Assembly.GetName().Version;
                //For UWP return typeof(NewTools).GetTypeInfo().Assembly.GetName().Version;
            }
        }

        /// <summary>
        /// Returns the platform the currently executing Ventura runtime was compiled for.
        /// </summary>
        public static VenturaPlatform ExecutingVenturaPlatform
        {
            get
            {
                return VenturaPlatform.NETStandard;
            }
        }

    }
}