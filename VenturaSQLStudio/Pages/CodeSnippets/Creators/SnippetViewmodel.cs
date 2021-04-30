using System.Text;

namespace VenturaSQLStudio.Pages
{
    class SnippetViewmodel : SnippetCreatorBase
    {
        public SnippetViewmodel()
        {
            
            //UsesParameter_IncludeUDCs = true;
        }

        public override string CreateCode()
        {
            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using VenturaSQL;");
            sb.AppendLine();

            sb.AppendLine("namespace YOUR.NAMESPACE.HERE");
            sb.AppendLine("{");

            sb.AppendLine(TAB + $"public class {this.ResultsetTypename}Viewmodel : ViewmodelBase");
            sb.AppendLine(TAB + "{");

            sb.AppendLine(TAB + TAB + "// begin: Support for 'ModelMode'. Remove the whole block if not needed.");
            sb.AppendLine(TAB + TAB + "public enum ModeKind { New, Edit }");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + "public ModeKind ModelMode { get; }");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + $"public {this.ResultsetTypename}Viewmodel(ModeKind mode)");
            sb.AppendLine(TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + "ModelMode = mode;");
            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine(TAB + TAB + "// end: Support for 'ModelMode'. Remove the whole block if not needed.");
            sb.AppendLine();

            foreach (var column in this.SelectedColumns)
            {
                string variable_name = $"_{column.PropertyName().ToLower()}";

                sb.AppendLine(TAB + TAB + $"private {column.ShortTypeNameForColumnProperty()} {variable_name};");
                sb.AppendLine();
                AddProperty(sb, column.ShortTypeNameForColumnProperty(), column.PropertyName(), variable_name);
            }

            foreach (var column in this.Selected_UDC_Columns)
            {
                string variable_name = $"_{column.PropertyName.ToLower()}";

                sb.AppendLine(TAB + TAB + $"private {column.ShortTypeName} {variable_name};");
                sb.AppendLine();
                AddProperty(sb, column.ShortTypeName, column.PropertyName, variable_name);
            }

            sb.AppendLine(TAB + "}");
            sb.AppendLine("}");

            return sb.ToString();
        }

        private void AddProperty(StringBuilder sb, string typename, string propertyname, string privatevarname)
        {
            sb.AppendLine(TAB + TAB + $"public {typename} {propertyname}");
            sb.AppendLine(TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + $"get {{ return {privatevarname}; }}");
            sb.AppendLine(TAB + TAB + TAB + "set");
            sb.AppendLine(TAB + TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + TAB + $"if ({privatevarname} == value) return;");
            sb.AppendLine(TAB + TAB + TAB + TAB + $"{privatevarname} = value;");
            sb.AppendLine(TAB + TAB + TAB + TAB + $"NotifyPropertyChanged(nameof({propertyname}));");
            sb.AppendLine(TAB + TAB + TAB + "}");
            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine();
        }


    }
}
