using System;
using System.Text;

namespace VenturaSQLStudio.Pages
{
    class SnippetViewmodelDefaults : SnippetCreatorBase
    {
        public SnippetViewmodelDefaults()
        {
            UsesParameter_ViewmodelVariable = true;
        }

        public override string CreateCode()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var column in this.SelectedColumns)
            {
                string code = DefaultCode(column.ColumnType, column.IsNullable);

                sb.AppendLine($"{ViewmodelVariable}.{column.PropertyName()} = {code}");
            }

            foreach (var column in this.Selected_UDC_Columns)
            {
                sb.AppendLine($"{ViewmodelVariable}.{column.PropertyName} = null;");
            }

            return sb.ToString();
        }

        public static string DefaultCode(Type column_type, bool isnullable)
        {
            if (isnullable == true)
                return "null;";

            if (column_type == typeof(Boolean))
                return "false;";
            else if (column_type == typeof(Byte))
                return "0;";
            else if (column_type == typeof(DateTime))
                return "new DateTime(1900, 1, 1);"; // Don't go lower than 1900
            else if (column_type == typeof(Decimal))
                return "0.0m;";
            else if (column_type == typeof(Single))
                return "0.0f;";
            else if (column_type == typeof(Double))
                return "0.0d;";
            else if (column_type == typeof(Int16))
                return "0;";
            else if (column_type == typeof(Int32))
                return "0;";
            else if (column_type == typeof(Int64))
                return "0;";
            else if (column_type == typeof(String))
                return "\"\";";
            else if (column_type == typeof(Guid))
                return "Guid.Empty;";
            else if (column_type == typeof(byte[]))
                return "new byte[0];";
            else if (column_type == typeof(Object))
                return "???"; // Weird, but at least not null.
            else if (column_type == typeof(TimeSpan))
                return "new TimeSpan(0);";
            else if (column_type == typeof(DateTimeOffset))
                return "new DateTimeOffset();";
            else
                return $"??? // don't know the default for type '{column_type.FullName}'";

        }

    }
}
