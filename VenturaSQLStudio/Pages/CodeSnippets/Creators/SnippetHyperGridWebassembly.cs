using System.Text;

namespace VenturaSQLStudio.Pages
{
    class SnippetHyperGridWebassembly : SnippetCreatorBase
    {

        public SnippetHyperGridWebassembly()
        {
            SyntaxHighlighting = "C#";
            //UsesParameter_IncludeUDCs = true;
        }

        public override string CreateCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"private {this.ResultsetTypename} rs = new {this.ResultsetTypename}();");
            sb.AppendLine($"private ColumnCollection<{this.RecordTypeName}> columns = new ColumnCollection<{this.RecordTypeName}>();");
            sb.AppendLine($"private List<{this.RecordTypeName}> checkedItemsList = new List<{this.RecordTypeName}>();");
            sb.AppendLine();

            int count = 0;
            string extra_attr = "";

            foreach (var column in this.SelectedColumns)
            {
                count++;
                extra_attr = count == 1 ? ", true" : "";

                sb.AppendLine($"columns.Add(c => c.{column.PropertyName()}, \"{column.PropertyName()}\", 150{extra_attr});");
            }

            foreach (var column in this.Selected_UDC_Columns)
            {
                count++;
                extra_attr = count == 1 ? ", true" : "";

                sb.AppendLine($"columns.Add(c => c.{column.PropertyName}, \"{column.PropertyName}\", 150{extra_attr});");
            }

            sb.AppendLine();

            sb.AppendLine("private void CheckedItemsChanged()");
            sb.AppendLine("{");
            sb.AppendLine("}");

            sb.AppendLine();

            sb.AppendLine("private void GridHyperlinkClicked(HyperlinkEventArgs arg)");
            sb.AppendLine("{");
            sb.AppendLine(TAB + $"var record = arg.Item as {this.RecordTypeName};");
            sb.AppendLine("}");

            sb.AppendLine();
            sb.AppendLine();
            sb.AppendLine();

            sb.AppendLine($"<HyperGrid ItemType=\"{this.RecordTypeName}\" Columns=\"@columns\" Items=\"@rs\"");
            sb.AppendLine(TAB + TAB + "UseMultiCheck=\"true\" CheckedItems=\"@checkedItemsList\"");
            sb.AppendLine(TAB + TAB + "UsePagination=\"true\" PageSize=\"15\"");
            sb.AppendLine(TAB + TAB + "UseHeader=\"true\"");
            sb.AppendLine(TAB + TAB + "CheckedItemsChanged=\"CheckedItemsChanged\"");
            sb.AppendLine(TAB + TAB + "HyperlinkClicked=\"GridHyperlinkClicked\"");
            sb.AppendLine(TAB + TAB + "SelectedItem = \"() => rs.CurrentRecord\" />");
            
            return sb.ToString();
        }


    }
}
