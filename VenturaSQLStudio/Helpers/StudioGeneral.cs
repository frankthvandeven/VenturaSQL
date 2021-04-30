using System;
using System.Data;
using System.IO;
using System.Security.Principal;
using System.Net;
using System.Net.Sockets;
using System.DirectoryServices;
using System.Linq;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text;
using System.Data.Common;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Diagnostics;

namespace VenturaSQLStudio
{
    /// <summary>
    /// General Ventura functions wrapped in a central static class.
    /// </summary>
    public static class StudioGeneral
    {
        public static void Sort<TSource, TKey>(this Collection<TSource> source, Func<TSource, TKey> keySelector)
        {
            List<TSource> sortedList = source.OrderBy(keySelector).ToList();
            source.Clear();
            foreach (var sortedItem in sortedList)
                source.Add(sortedItem);
        }

        [DllImport("shlwapi.dll", EntryPoint = "PathRelativePathTo")]
        private static extern bool PathRelativePathTo(StringBuilder lpszDst,
            string from, UInt32 attrFrom,
            string to, UInt32 attrTo);

        /// <summary>
        /// Returns the relative file path based on the base_path parameter.
        /// Note that base_path must be a path and not a file.
        /// </summary>
        /// <param name="base_path">For example c:\Ventura\Samples</param>
        /// <param name="make_relative">For example c:\Ventura\Samples\Server\project.txt</param>
        /// <returns>.\Server\project.txt</returns>
        public static string GetRelativePath(string base_path, string make_relative)
        {
            StringBuilder builder = new StringBuilder(1024);
            bool result = PathRelativePathTo(builder, base_path, 0x10, make_relative, 0);
            return builder.ToString();
        }

        public static string GetAbsolutePath(string base_path, string relative_file_path)
        {
            return Path.GetFullPath(Path.Combine(base_path, relative_file_path));
        }

        public static string MakeSureStringEndsWith(string text, string endword)
        {
            if (text == null)
                throw new NoNullAllowedException();

            if (text.ToLower().EndsWith(endword.ToLower()) == false)
                text = text + endword;
            else
                // The end word is already correct, but character casing might not be
                text = text.Substring(0, text.Length - (endword.Length)) + endword;

            return text;
        }

        public static string MakeSureStringStartsWith(string text, string startword)
        {
            if (text == null)
                throw new NoNullAllowedException();

            if (text.ToLower().StartsWith(startword.ToLower()) == false)
                text = startword + text;
            else
                // The start word is already correct, but character casing might not be
                text = startword + text.Substring(startword.Length);

            return text;
        }


        /// <summary>
        /// Removes the last 9 characters from a string.
        /// That is the word "Recordset"
        /// </summary>
        public static string NewStripLast9(string text)
        {
            return text.Substring(0, text.Length - 9);
        }

        public static string GetLocalIPAddress()
        {
            IPHostEntry host = Dns.GetHostEntry(Dns.GetHostName());

            foreach (var ip in host.AddressList)
                if (ip.AddressFamily == AddressFamily.InterNetwork)
                    return ip.ToString();

            return "unknown";
        }

        /// <summary>
        /// Returns true if the current process is running with Administrator rights.
        /// </summary>
        public static bool ProcessRunningAsAdmin()
        {
            WindowsIdentity identity = WindowsIdentity.GetCurrent();
            WindowsPrincipal principal = new WindowsPrincipal(identity);
            return principal.IsInRole(WindowsBuiltInRole.Administrator);
        }

        /// <summary>
        /// Return a 32 character hash in hexadecimal format.
        /// </summary>
        internal static string GetMachineHash()
        {
            string path = $"WinNT://{Environment.MachineName},Computer";

            byte[] sid_bytes = (byte[])new DirectoryEntry(path).Children.Cast<DirectoryEntry>().First().InvokeGet("objectSID");

            SecurityIdentifier sid = new SecurityIdentifier(sid_bytes, 0);
            SecurityIdentifier acc_sid = sid.AccountDomainSid;

            // Turn the machine sid into a byte array called arr.
            byte[] arr = new byte[acc_sid.BinaryLength];

            acc_sid.GetBinaryForm(arr, 0);

            // Calculate the hash for the sid byte array.
            MD5CryptoServiceProvider md5_csp = new MD5CryptoServiceProvider();

            byte[] hash_array = md5_csp.ComputeHash(arr);

            // Convert the byte array hash to a 32 character string.
            return ToHexadecimalString(hash_array);
        }

        /// <summary>
        /// Return a 32 character hash in hexadecimal format.
        /// </summary>
        private static string ToHexadecimalString(byte[] bytes)
        {
            string hexString = "";
            for (int i = 0; i < bytes.Length; i++)
            {
                hexString += bytes[i].ToString("X2");
            }
            return hexString;
        }

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
        /// If the DblDataReader parameter is null, nothing happens. If datareader.Close() throws an Exception, it will be ignored.
        /// </summary>
        public static void SmartClose(DbDataReader datareader)
        {
            if (datareader != null)
            {
                try
                { datareader.Close(); }
                catch { }
            }

        }

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

        public static void SmartClose(DbTransaction transaction, bool rollback)
        {
            if (transaction == null)
                return;

            try
            {
                if (rollback == true)
                    transaction.Rollback();
                else
                    transaction.Commit();
            }
            catch { }

        } // end of function SmartClose

        /// <summary>
        /// For debugging purposes.
        /// </summary>
        public static void ClickSound()
        {
            //new SoundPlayer(@"C:\Active\Ventura\Projects\VenturaSQL Studio\Assets\button_select1.wav").Play();
        }

        public static void StartBrowser(string url)
        {
            url = url.Replace("&", "^&");
            
            Process.Start(new ProcessStartInfo("cmd", $"/c start {url}") { CreateNoWindow = true });
        }

        public static string TimeSpanAsWords(DateTime reference)
        {
            reference = reference.ToUniversalTime();

            DateTime utc_now = DateTime.UtcNow;

            if (reference == utc_now)
                return "now";
            else if (reference < utc_now)
            {
                TimeSpan time_since = utc_now.Subtract(reference);

                if (time_since.TotalMilliseconds < 1)
                    return "not yet";
                if (time_since.TotalMinutes < 1)
                    return "just now";
                if (time_since.TotalMinutes < 2)
                    return "1 minute ago";
                if (time_since.TotalMinutes < 60)
                    return string.Format("{0} minutes ago", time_since.Minutes);
                if (time_since.TotalMinutes < 120)
                    return "1 hour ago";
                if (time_since.TotalHours < 24)
                    return string.Format("{0} hours ago", time_since.Hours);
                if (time_since.TotalDays == 1)
                    return "yesterday";
                if (time_since.TotalDays < 7)
                    return string.Format("{0} days ago", time_since.Days);
                if (time_since.TotalDays < 14)
                    return "last week";
                if (time_since.TotalDays < 21)
                    return "2 weeks ago";
                if (time_since.TotalDays < 28)
                    return "3 weeks ago";
                if (time_since.TotalDays < 60)
                    return "last month";
                if (time_since.TotalDays < 365)
                    return string.Format("{0} months ago", Math.Round(time_since.TotalDays / 30));
                if (time_since.TotalDays < 730)
                    return "last year";

                return string.Format("{0} years ago", Math.Round(time_since.TotalDays / 365));
            }
            else if (reference > utc_now)
            {
                TimeSpan time_before = reference.Subtract(utc_now);

                if (time_before.TotalMilliseconds < 1)
                    return "not yet";
                if (time_before.TotalMinutes < 1)
                    return "right now";
                if (time_before.TotalMinutes < 2)
                    return "in 1 minute";
                if (time_before.TotalMinutes < 60)
                    return string.Format("in {0} minutes", time_before.Minutes);
                if (time_before.TotalMinutes < 120)
                    return "in 1 hour";
                if (time_before.TotalHours < 24)
                    return string.Format("in {0} hours", time_before.Hours);
                if (time_before.TotalDays == 1)
                    return "tomorrow";
                if (time_before.TotalDays < 7)
                    return string.Format("in {0} days", time_before.Days);
                if (time_before.TotalDays < 14)
                    return "next week";
                if (time_before.TotalDays < 21)
                    return "in 2 weeks";
                if (time_before.TotalDays < 28)
                    return "in 3 weeks";
                if (time_before.TotalDays < 60)
                    return "next month";
                if (time_before.TotalDays < 365)
                    return string.Format("in {0} months", Math.Round(time_before.TotalDays / 30));
                if (time_before.TotalDays < 730)
                    return "next year";

                return string.Format("in {0} years", Math.Round(time_before.TotalDays / 365));
            }

            return "error finding timespan words";
        }

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
