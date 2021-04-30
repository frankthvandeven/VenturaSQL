using System.Text;

namespace VenturaSQLStudio.Pages
{
    class SnippetHyperGrid : SnippetCreatorBase
    {

        public SnippetHyperGrid()
        {
            SyntaxHighlighting = "XML";
            //UsesParameter_IncludeUDCs = true;
        }

        public override string CreateCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine($"<Ventura:HyperGrid x:Name=\"{this.ResultsetTypename}Grid\">");
            sb.AppendLine(TAB + "<Ventura:HyperGrid.Header>");

            foreach (var column in this.SelectedColumns)
                sb.AppendLine(TAB + TAB + $"<Ventura:HeaderDefinition Caption=\"{column.PropertyName()}\" />");

            foreach (var column in this.Selected_UDC_Columns)
                sb.AppendLine(TAB + TAB + $"<Ventura:HeaderDefinition Caption=\"{column.PropertyName}\" />");

            sb.AppendLine(TAB + "</Ventura:HyperGrid.Header>");

            sb.AppendLine(TAB + "<Ventura:HyperGrid.ItemTemplate>");
            sb.AppendLine(TAB + TAB + $"<DataTemplate x:DataType=\"Recordsets:{this.RecordTypeName}\">");
            sb.AppendLine(TAB + TAB + TAB + "<Ventura:HyperGridPanel>");

            foreach (var column in this.SelectedColumns)
            {
                sb.AppendLine(TAB + TAB + TAB + TAB + $"<TextBlock Width=\"200\" Text=\"{{x:Bind {column.PropertyName()}, Mode=OneWay}}\" TextTrimming=\"CharacterEllipsis\" />");
            }

            foreach (var column in this.Selected_UDC_Columns)
            {
                sb.AppendLine(TAB + TAB + TAB + TAB + $"<TextBlock Width=\"200\" Text=\"{{x:Bind {column.PropertyName}, Mode=OneWay}}\" TextTrimming=\"CharacterEllipsis\" />");
            }

            sb.AppendLine(TAB + TAB + TAB + "</Ventura:HyperGridPanel>");
            sb.AppendLine(TAB + TAB + "</DataTemplate>");
            sb.AppendLine(TAB + "</Ventura:HyperGrid.ItemTemplate>");
            sb.AppendLine("</Ventura:HyperGrid>");

            return sb.ToString();
        }


    }
}
