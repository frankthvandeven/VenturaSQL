using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio
{
    public class ExecuteSqlScriptException : Exception
    {
        /// <summary>
        /// Exception message includes (part of the) SQL script.
        /// </summary>
        public ExecuteSqlScriptException(string sql_script, Exception inner_exception) : base(FillMessage(sql_script, inner_exception), inner_exception)
        {

        }

        private static string FillMessage(string sql_script, Exception inner_exception)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine("Executing SQL script failed:");

            var processed_list = SplitLines(sql_script);

            foreach (string line in processed_list)
            {
                sb.AppendLine(line);
            }

            sb.AppendLine();
            sb.AppendLine(inner_exception.GetType().FullName);
            sb.AppendLine();
            sb.Append(inner_exception.Message);

            return sb.ToString();
        }

        private static List<string> SplitLines(string input)
        {
            List<string> list = new List<string>();

            StringReader reader = new StringReader(input);

            int line_count = 0;

            while (true)
            {
                string line = reader.ReadLine();

                if (line == null)
                    break;

                if (line_count == 5) // we already have 5 lines but there is more...
                {
                    list.Add("(...)");
                    break;
                }

                line = line.Trim();

                if (line.Length > 100)
                    line = line.Substring(0, 97) + "...";

                if (line.Length > 0)
                {
                    list.Add(line);
                    line_count++;
                }
            }

            return list;
        }

    }
}
