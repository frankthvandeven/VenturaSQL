using System.Collections.Generic;
using System.Text;

namespace VenturaSQLStudio.Helpers
{
    internal class SmartSplit
    {
        private const string CRLF = "\r\n";

        public string FirstLinePrefix { get; set; }
        public string OtherLinePrefix { get; set; }

        public string LinesToSplit { get; set; }

        public void ExecSplit(StringBuilder sb)
        {
            List<string> lines = StringTools.WordWrap(LinesToSplit, 120);

            for (int i = 0; i < lines.Count; i++)
            {
                if (i == 0)
                    sb.Append(FirstLinePrefix + lines[i] + CRLF);
                else
                    sb.Append(OtherLinePrefix + lines[i] + CRLF);
            }

        }

    }

}
