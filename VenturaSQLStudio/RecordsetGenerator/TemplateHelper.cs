using System.Globalization;
using System.Text;

namespace VenturaSQLStudio {
    internal static class TemplateHelper
    {

        private static string[] _reservedwordlist = { "Append", "ExecAsync","SaveChangesAsync","NewRecord",
        "SetExecSqlParams","Hash", "HashString", "UpdateableTablename", "CollectionChanged",
        "PropertyChanged", "Move", "ClearItems", "InsertItem", "RemoveItem", "MoveItem",
        "SetItem", "OnCollectionChanged", "_parameterschema", "_sqlscript",
         "CRLF", "CurrentRecord", "this", "Clear", "RecordCount", "RecordNumber",
        "MoveFirst", "MoveLast", "MoveNext", "MovePrevious", "RowOffset", "RowLimit", "ParameterSchema",
        "ParameterValues", "SqlScript", "Sort", "_schema", "Schema" };

        internal static string ConvertToValidIdentifier(string value)
        {
            StringBuilder sb = new StringBuilder(256);

            if (value.Length == 0)
                return "";

            for (int i = 0; i < value.Length; i++)
            {
                char chr = value[i];

                switch (char.GetUnicodeCategory(chr))
                {
                    case UnicodeCategory.UppercaseLetter:
                    case UnicodeCategory.LowercaseLetter:
                    case UnicodeCategory.TitlecaseLetter:
                    case UnicodeCategory.ModifierLetter:
                    case UnicodeCategory.OtherLetter:
                    case UnicodeCategory.LetterNumber:
                        sb.Append(chr);
                        break;
                    case UnicodeCategory.NonSpacingMark:
                    case UnicodeCategory.SpacingCombiningMark:
                    case UnicodeCategory.DecimalDigitNumber:
                    case UnicodeCategory.ConnectorPunctuation:
                        if (i == 0 && chr != '\u005F') // underscore
                            sb.Append('_');
                        else
                            sb.Append(chr);

                        break;
                    case UnicodeCategory.EnclosingMark:
                    case UnicodeCategory.OtherNumber:
                    case UnicodeCategory.SpaceSeparator:
                    case UnicodeCategory.LineSeparator:
                    case UnicodeCategory.ParagraphSeparator:
                    case UnicodeCategory.Control:
                    case UnicodeCategory.Format:
                    case UnicodeCategory.Surrogate:
                    case UnicodeCategory.PrivateUse:
                        sb.Append('_');
                        break;
                    default:
                        sb.Append('_');
                        break;
                }
            }
            return sb.ToString();
        } // end of method

    } // end of class
}
