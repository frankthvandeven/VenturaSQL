using System;
using System.IO;
//using System.Security.Principal;

using System.Data.Common;

namespace VenturaSQL
{
    /// <summary>
    /// General Ventura functions
    /// </summary>
    public static class General
    {

        /// <summary>
        /// Returns true if the current process is running with Administrator rights.
        /// </summary>
        //public static bool ProcessRunningAsAdmin()
        //{
        //    WindowsIdentity identity = WindowsIdentity.GetCurrent();
        //    WindowsPrincipal principal = new WindowsPrincipal(identity);
        //    return principal.IsInRole(WindowsBuiltInRole.Administrator);
        //}

        public static DateTime ToDateTime(object dt)
        {
            if (dt == DBNull.Value)
                return new DateTime(0);

            return Convert.ToDateTime(dt);
        }

        public static int ToInt32(object dt)
        {
            if (dt == DBNull.Value)
                return 0;

            return Convert.ToInt32(dt);
        }

        public static string ToString(object dt)
        {
            if (dt == DBNull.Value)
                return "";

            return Convert.ToString(dt);
        }

        public static bool ToBoolean(object dt)
        {
            if (dt == DBNull.Value)
                return false;

            return Convert.ToBoolean(dt);
        }


        /// <summary>
        /// If the DbConnection parameter is null, nothing happens. If connection.Close() throws an Exception, it will be ignored.
        /// </summary>
        public static void SmartClose(DbConnection connection)
        {
            if (connection != null)
            {
                try
                { connection.Close(); }
                catch { }
            }

        } // end of function SmartClose

        /// <summary>
        /// If the DbDataReader parameter is null, nothing happens. If datareader.Close() throws an Exception, it will be ignored.
        /// </summary>
        public static void SmartClose(DbDataReader datareader)
        {
            if (datareader != null)
            {
                try
                { datareader.Close(); }
                catch { }
            }

        } // end of function SmartClose

        /// <summary>
        /// If the FileStream parameter is null, nothing happens. If filestream.Close() throws an Exception, it will be ignored.
        /// </summary>
        public static void SmartClose(FileStream filestream)
        {
            if (filestream != null)
            {
                try
                { filestream.Close(); }
                catch { }
            }

        } // end of function SmartClose

        /// <summary>
        /// If the Stream parameter is null, nothing happens. If stream.Close() throws an Exception, it will be ignored.
        /// </summary>
        public static void SmartClose(Stream stream)
        {
            if (stream != null)
            {
                try
                { stream.Close(); }
                catch { }
            }

        } // end of function SmartClose

    } // end of class

    public class TNTimer
    {
        private long startTime;
        private long endTime;
        private TimeSpan timeTaken;

        public TNTimer()
        {
            this.Start();
        }

        public void Start()
        {
            startTime = DateTime.Now.Ticks;
        }

        public void Stop()
        {
            endTime = DateTime.Now.Ticks;
            timeTaken = new TimeSpan(endTime - startTime);
        }

        public string TimeTakenString()
        {
            return timeTaken.ToString();
        }

    }
}
