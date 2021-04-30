using System;
using System.Collections.Generic;
using System.Text;

namespace VenturaSQLStudio.Pages
{
    class SnippetForm : SnippetCreatorBase
    {

        public SnippetForm()
        {
            SyntaxHighlighting = "XML";
            UsesParameter_ViewmodelVariable = true;
        }

        public override string CreateCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"<Ventura:Form StretchRows=\"False\">");

            foreach (var column in this.SelectedColumns)
                WriteFormField(sb, column.PropertyName(), column.ColumnType, column.IsNullable);

            foreach (var column in this.Selected_UDC_Columns)
            {
                Type type = Type.GetType(column.FullTypename);
                WriteFormField(sb, column.PropertyName, type, true);
            }

            sb.AppendLine("</Ventura:Form>");

            return sb.ToString();
        }

        private void WriteFormField(StringBuilder sb, string propertyname, Type type, bool nullable)
        {
            string control_type = "TextBox";
            string value_attribute = "Text";
            string converter_insert = "";

            var attributes = new Dictionary<string, string>();

            if (type == typeof(Boolean))
            {
                control_type = "CheckBox";
                value_attribute = "IsChecked";
            }
            else if (type == typeof(DateTime))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputDate";
                else
                    control_type = "Ventura:InputDateNullable";

            }
            else if (type == typeof(String))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputString";
                else
                    control_type = "Ventura:InputStringNullable";

            }

            // SIGNED INTEGERS

            else if (type == typeof(SByte))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputSByte";
                else
                    control_type = "Ventura:InputSByteNullable";

                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", SByte.MaxValue.ToString());
            }
            else if (type == typeof(Int16))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputInt16";
                else
                    control_type = "Ventura:InputInt16Nullable";

                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", Int16.MaxValue.ToString());
            }
            else if (type == typeof(Int32))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputInt32";
                else
                    control_type = "Ventura:InputInt32Nullable";

                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", Int32.MaxValue.ToString());
            }
            else if (type == typeof(Int64))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputInt64";
                else
                    control_type = "Ventura:InputInt64Nullable";

                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", Int64.MaxValue.ToString());
            }

            // UNSIGNED INTEGERS

            else if (type == typeof(Byte))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputByte64";
                else
                    control_type = "Ventura:InputByteNullable";

                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", byte.MaxValue.ToString());
            }
            else if (type == typeof(UInt16))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputUInt16";
                else
                    control_type = "Ventura:InputUInt16Nullable";

                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", UInt16.MaxValue.ToString());
            }
            else if (type == typeof(UInt32))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputUInt32";
                else
                    control_type = "Ventura:InputUInt32Nullable";

                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", UInt32.MaxValue.ToString());
            }
            else if (type == typeof(UInt64))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputUInt64";
                else
                    control_type = "Ventura:InputUInt64Nullable";

                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", UInt64.MaxValue.ToString());
            }

            // FLOATING POINT
            else if (type == typeof(Single))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputSingle";
                else
                    control_type = "Ventura:InputSingleNullable";

                attributes.Add("Mask", "99999999.99");
                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", Single.MaxValue.ToString());
            }
            else if (type == typeof(Double))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputDouble";
                else
                    control_type = "Ventura:InputDoubleNullable";

                attributes.Add("Mask", "99999999.99");
                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", Double.MaxValue.ToString());
            }

            // DECIMAL
            else if (type == typeof(Decimal))
            {
                value_attribute = "Value";

                if (nullable == false)
                    control_type = "Ventura:InputDecimal";
                else
                    control_type = "Ventura:InputDecimalNullable";

                attributes.Add("Mask", "99999999.99");
                attributes.Add("MinValue", "0");
                attributes.Add("MaxValue", decimal.MaxValue.ToString());
            }

            // DON'T KNOW HOW TO HANDLE

            else
            {
                converter_insert = $", Converter={{StaticResource CREATE_A_CONVERTER_FOR_{type.FullName}{(nullable ? "_NULLABLE" : "")}}}";
            }

            sb.AppendLine(TAB + $"<Ventura:FormField FieldWidth=\"200\" Header=\"{propertyname}\">");

            string mode_insert = ", Mode=TwoWay";

            sb.Append(TAB + TAB + $"<{control_type} {value_attribute}=\"{{x:Bind {ViewmodelVariable}.{propertyname}{converter_insert}{mode_insert}}}\"");

            foreach (var keyval in attributes)
                sb.Append($" {keyval.Key}=\"{keyval.Value}\"");

            sb.AppendLine(" />");

            sb.AppendLine(TAB + "</Ventura:FormField>");
        }

    }
}
