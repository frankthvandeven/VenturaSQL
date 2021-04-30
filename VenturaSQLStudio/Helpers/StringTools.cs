using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Helpers
{
    public static class StringTools
    {
        // Word wrappers:
        // https://stackoverflow.com/questions/3961278/word-wrap-a-string-in-multiple-lines

        /// <summary>
        /// Splits a string into lines based on CRLFs in the string.
        /// </summary>
        public static List<string> SplitLines(string input)
        {
            List<string> list = new List<string>();

            StringReader reader = new StringReader(input);

            while (true)
            {
                string line = reader.ReadLine();

                if (line == null)
                    break;

                list.Add(line);
            }

            return list;
        }

        public static List<string> WordWrap(string text, int maxLineLength)
        {
            var list = new List<string>();
            int currentIndex;
            var lastWrap = 0;
            var whitespace = new[] { ' ', '\r', '\n', '\t' };

            do
            {
                currentIndex = lastWrap + maxLineLength > text.Length ? text.Length : (text.LastIndexOfAny(new[] { ' ', ',', '.', '?', '!', ':', ';', '-', '\n', '\r', '\t' }, Math.Min(text.Length - 1, lastWrap + maxLineLength)) + 1);

                if (currentIndex <= lastWrap)
                    currentIndex = Math.Min(lastWrap + maxLineLength, text.Length);

                list.Add(text.Substring(lastWrap, currentIndex - lastWrap).Trim(whitespace));

                lastWrap = currentIndex;
            }
            while (currentIndex < text.Length);

            return list;
        }

        internal static bool AllLinesAreEmpty(string text)
        {
            StringReader strReader = new StringReader(text);
            int characters = 0;

            while (true)
            {
                string line = strReader.ReadLine();

                if (line == null) break;

                characters += line.Trim().Length;
            }

            return (characters == 0);
        }

        /// <summary>
        /// Remove all CRLF characters, and multiple-spaces are replaced with a single space.
        /// Used on InnerText property of XMLNode.
        /// </summary>
        /// <returns></returns>
        internal static string StripCRLFAndSpaces(string text)
        {
            text = text.Replace("\r", " ").Replace("\n", " ");

            while (text.Contains("  "))
                text = text.Replace("  ", " ");

            return text.Trim();
        }



    }
}
