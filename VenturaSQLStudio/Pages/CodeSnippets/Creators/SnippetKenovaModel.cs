using System.Text;
using System.Windows.Media.TextFormatting;

namespace VenturaSQLStudio.Pages
{
    class SnippetKenovaModel : SnippetCreatorBase
    {
        public SnippetKenovaModel()
        {

            //UsesParameter_IncludeUDCs = true;
        }

        public override string CreateCode()
        {
            string class_name = this.ResultsetTypename;

            if (class_name.EndsWith("Recordset"))
                class_name = class_name.Substring(0, class_name.Length - 9);

            class_name = class_name + "Model";

            StringBuilder sb = new StringBuilder();

            sb.AppendLine("using Kenova;"); 
            sb.AppendLine("using System.Threading.Tasks;");
            sb.AppendLine();

            sb.AppendLine("namespace YOUR.NAMESPACE.HERE");
            sb.AppendLine("{");

            sb.AppendLine(TAB + $"public class {class_name} : ModelBase<{class_name}>");
            sb.AppendLine(TAB + "{");

            sb.AppendLine(TAB + TAB + "public enum ModelMode { Edit, New }");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + "public ModelMode Mode;");
            sb.AppendLine();

            foreach (var column in this.SelectedColumns)
                sb.AppendLine(TAB + TAB + $"public {column.ShortTypeNameForColumnProperty()} {column.PropertyName()};");

            foreach (var column in this.Selected_UDC_Columns)
                sb.AppendLine(TAB + TAB + $"public {column.ShortTypeName} {column.PropertyName};");

            sb.AppendLine();

            sb.AppendLine(TAB + TAB + $"public {class_name}()");
            sb.AppendLine(TAB + TAB + "{");

            foreach (var column in this.SelectedColumns)
                sb.AppendLine(TAB + TAB + TAB + $"Register(m => m.{column.PropertyName()});");

            foreach (var column in this.Selected_UDC_Columns)
                sb.AppendLine(TAB + TAB + TAB + $"Register(m => m.{column.PropertyName});");

            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine();


            sb.AppendLine(TAB + TAB + $"public override async Task ValidateEventAsync(ValidateEventArgs<{class_name}> e)");
            sb.AppendLine(TAB + TAB + "{");

            int count = 0;

            foreach (var column in this.SelectedColumns)
            {
                sb.AppendLine(TAB + TAB + TAB + $"{(count == 0 ? "" : "else ")}if (e.IsMember(m => m.{column.PropertyName()}))");
                sb.AppendLine(TAB + TAB + TAB + "{");
                sb.AppendLine(TAB + TAB + TAB + TAB + "await Task.CompletedTask;");
                sb.AppendLine(TAB + TAB + TAB + TAB + $"Console.WriteLine($\"The value of {column.PropertyName()} was changed to {{this.{column.PropertyName()}}}\");");
                sb.AppendLine(TAB + TAB + TAB + "}");
                count++;
            }

            foreach (var column in this.Selected_UDC_Columns)
            {
                sb.AppendLine(TAB + TAB + TAB + $"{(count == 0 ? "" : "else ")}if (e.IsMember(m => m.{column.PropertyName}))");
                sb.AppendLine(TAB + TAB + TAB + "{");
                sb.AppendLine(TAB + TAB + TAB + TAB + "await Task.CompletedTask;");
                sb.AppendLine(TAB + TAB + TAB + TAB + $"Console.WriteLine($\"The value of {column.PropertyName} was changed to {{this.{column.PropertyName}}}\");");
                sb.AppendLine(TAB + TAB + TAB + "}");
                count++;
            }

            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + "base.ValidateEvent(e);");
            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine();

            sb.AppendLine(TAB + TAB + $"public {this.RecordsetTypename} Recordset {{ get; private set; }} = new {this.RecordsetTypename}();");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + $"public string SearchParam;");
            sb.AppendLine();

            sb.AppendLine(TAB + TAB + "public async Task SearchExecTask()");
            sb.AppendLine(TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + "Recordset.RowLimit = 100;");
            sb.AppendLine(TAB + TAB + TAB + "await Recordset.ExecSqlAsync(this.SearchParam);");
            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine();

            sb.AppendLine(TAB + TAB + $"public {this.RecordTypeName} LastRecord;");
            sb.AppendLine();

            sb.AppendLine(TAB + TAB + "public async Task LoadExecTask()");
            sb.AppendLine(TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + "LastRecord = null;");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + $"var rs = new {this.RecordsetTypename}();");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + "await rs.ExecSqlAsync(/* parameters here */);");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + "if (rs.RecordCount == 0)");
            sb.AppendLine(TAB + TAB + TAB + TAB + "throw new Exception(\"Record [details here] not found in database.\");");
            sb.AppendLine();
            foreach (var column in this.SelectedColumns)
                sb.AppendLine(TAB + TAB + TAB + $"this.{column.PropertyName()} = rs.{column.PropertyName()};");

            foreach (var column in this.Selected_UDC_Columns)
                sb.AppendLine(TAB + TAB + TAB + $"this.{column.PropertyName} = rs.{column.PropertyName};");

            sb.AppendLine();

            sb.AppendLine(TAB + TAB + TAB + "LastRecord = rs.CurrentRecord;");
            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine();

            sb.AppendLine(TAB + TAB + "public async Task SaveExecTask()");
            sb.AppendLine(TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + "LastRecord = null;");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + $"var rs = new {this.RecordsetTypename}();");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + "if (this.Mode == ModelMode.Edit)");
            sb.AppendLine(TAB + TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + TAB + "await rs.ExecSqlAsync(/* parameters here */);");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + TAB + "if (rs.RecordCount == 0)");
            sb.AppendLine(TAB + TAB + TAB + TAB + TAB + "throw new Exception(\"Record [details here] not found in database.\");");
            sb.AppendLine(TAB + TAB + TAB + "}");
            sb.AppendLine(TAB + TAB + TAB + "else");
            sb.AppendLine(TAB + TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + TAB + "rs.Append();");
            sb.AppendLine(TAB + TAB + TAB + "}");
            sb.AppendLine();

            foreach (var column in this.SelectedColumns)
                sb.AppendLine(TAB + TAB + TAB + $"rs.{column.PropertyName()} = this.{column.PropertyName()};");

            foreach (var column in this.Selected_UDC_Columns)
                sb.AppendLine(TAB + TAB + TAB + $"rs.{column.PropertyName} = this.{column.PropertyName};");

            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + "await rs.SaveChangesAsync();");

            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + "LastRecord = rs.CurrentRecord;");

            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine();

            sb.AppendLine(TAB + "}");
            sb.AppendLine("}");

            return sb.ToString();
        }

    }
}
