using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace VenturaSQLStudio.Helpers
{
    public static class StringTools
    {
        static readonly IDictionary<string, string> m_replaceDict = new Dictionary<string, string>();

        const string ms_regexEscapes = @"[\a\b\f\n\r\t\v\\""]";

        static StringTools()
        {
            m_replaceDict.Add("\a", @"\a");
            m_replaceDict.Add("\b", @"\b");
            m_replaceDict.Add("\f", @"\f");
            m_replaceDict.Add("\n", @"\n");
            m_replaceDict.Add("\r", @"\r");
            m_replaceDict.Add("\t", @"\t");
            m_replaceDict.Add("\v", @"\v");

            m_replaceDict.Add("\\", @"\\");
            m_replaceDict.Add("\0", @"\0");

            //The parser gets fooled by the verbatim version of the string to replace - @"\"""
            m_replaceDict.Add("\"", "\\\"");
        }

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

        public static string EscapedCSharpStringLiteral(string input)
        {
            return Regex.Replace(input, ms_regexEscapes, match);
        }

        //public static string CharLiteral(char c)
        //{
        //    return c == '\'' ? @"'\''" : string.Format("'{0}'", c);
        //}

        private static string match(Match m)
        {
            string match = m.ToString();
            if (m_replaceDict.ContainsKey(match))
            {
                return m_replaceDict[match];
            }

            throw new NotSupportedException();
        }

    }
}
