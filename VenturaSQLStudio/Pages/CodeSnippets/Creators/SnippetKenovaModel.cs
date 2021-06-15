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

            sb.AppendLine(TAB + $"[ViewModel]");
            sb.AppendLine(TAB + $"public partial class {class_name} : ModelTypedBase<{class_name}>");
            sb.AppendLine(TAB + "{");

            sb.AppendLine(TAB + TAB + $"public {this.RecordsetTypename} Recordset = new();");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + "public bool CreateNew = false;");
            sb.AppendLine();

            foreach (var column in this.SelectedColumns)
                sb.AppendLine(TAB + TAB + $"private {column.ShortTypeNameForColumnProperty()} __{column.PropertyName()};");

            foreach (var column in this.Selected_UDC_Columns)
                sb.AppendLine(TAB + TAB + $"private {column.ShortTypeName} __{column.PropertyName};");

            sb.AppendLine();

            sb.AppendLine(TAB + TAB + $"public {class_name}()");
            sb.AppendLine(TAB + TAB + "{");

            foreach (var column in this.SelectedColumns)
                sb.AppendLine(TAB + TAB + TAB + $"Register(m => m.{column.PropertyName()});");

            foreach (var column in this.Selected_UDC_Columns)
                sb.AppendLine(TAB + TAB + TAB + $"Register(m => m.{column.PropertyName});");

            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine();


            sb.AppendLine(TAB + TAB + $"protected override async Task ValidateEventAsync()");
            sb.AppendLine(TAB + TAB + "{");

            foreach (var column in this.SelectedColumns)
            {
                sb.AppendLine();
                sb.AppendLine(TAB + TAB + TAB + $"if (e.IsMember(m => m.{column.PropertyName()}))");
                sb.AppendLine(TAB + TAB + TAB + "{");
                sb.AppendLine(TAB + TAB + TAB + TAB + "await Task.CompletedTask;");
                sb.AppendLine(TAB + TAB + TAB + TAB + $"e.RemarkText = $\"The value of {column.PropertyName()} was changed to {{this.{column.PropertyName()}}}\";");
                sb.AppendLine(TAB + TAB + TAB + TAB + "e.IsValid = true;");
                sb.AppendLine(TAB + TAB + TAB + TAB + "return;");
                sb.AppendLine(TAB + TAB + TAB + "}");
            }

            foreach (var column in this.Selected_UDC_Columns)
            {
                sb.AppendLine();
                sb.AppendLine(TAB + TAB + TAB + $"if (e.IsMember(m => m.{column.PropertyName}))");
                sb.AppendLine(TAB + TAB + TAB + "{");
                sb.AppendLine(TAB + TAB + TAB + TAB + "await Task.CompletedTask;");
                sb.AppendLine(TAB + TAB + TAB + TAB + $"e.RemarkText = $\"The value of {column.PropertyName} was changed to {{this.{column.PropertyName}}}\";");
                sb.AppendLine(TAB + TAB + TAB + TAB + "e.IsValid = true;");
                sb.AppendLine(TAB + TAB + TAB + TAB + "return;");
                sb.AppendLine(TAB + TAB + TAB + "}");
            }

            sb.AppendLine();
            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine();

            //sb.AppendLine();
            //sb.AppendLine(TAB + TAB + $"public string SearchParam;");
            //sb.AppendLine();
            //sb.AppendLine(TAB + TAB + "public async Task SearchExecTask()");
            //sb.AppendLine(TAB + TAB + "{");
            //sb.AppendLine(TAB + TAB + TAB + "Recordset.RowLimit = 100;");
            //sb.AppendLine(TAB + TAB + TAB + "await Recordset.ExecSqlAsync(this.SearchParam);");
            //sb.AppendLine(TAB + TAB + "}");
            //sb.AppendLine();
            //sb.AppendLine(TAB + TAB + $"public {this.RecordTypeName} LastRecord;");
            //sb.AppendLine();

            sb.AppendLine(TAB + TAB + "public async Task LoadTask()");
            sb.AppendLine(TAB + TAB + "{");

            sb.AppendLine(TAB + TAB + TAB + "if (this.CreateNew)");
            sb.AppendLine(TAB + TAB + TAB + "{");

            // Default values like empty string or zero

            foreach (var column in this.SelectedColumns)
            {
                string code = SnippetViewmodelDefaults.DefaultCode(column.ColumnType, column.IsNullable);

                sb.AppendLine(TAB + TAB + TAB + TAB + $"this.{column.PropertyName()} = {code}");
            }

            foreach (var column in this.Selected_UDC_Columns)
            {
                sb.AppendLine(TAB + TAB + TAB + TAB + $"this.{column.PropertyName} = null;");
            }

            sb.AppendLine(TAB + TAB + TAB + "}");
            sb.AppendLine(TAB + TAB + TAB + "else");
            sb.AppendLine(TAB + TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + TAB + "// edit");

            sb.AppendLine(TAB + TAB + TAB + TAB + "await Recordset.ExecSqlAsync(/* this... prikey parameters here */);");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + TAB + "if (Recordset.RecordCount == 0)");
            sb.AppendLine(TAB + TAB + TAB + TAB + TAB + "throw new Exception($\"Record {\"details here\"} not found in database.\");");
            sb.AppendLine();
            foreach (var column in this.SelectedColumns)
                sb.AppendLine(TAB + TAB + TAB + TAB + $"this.{column.PropertyName()} = Recordset.{column.PropertyName()};");

            foreach (var column in this.Selected_UDC_Columns)
                sb.AppendLine(TAB + TAB + TAB + TAB + $"this.{column.PropertyName} = Recordset.{column.PropertyName};");

            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + "}");

            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine();

            sb.AppendLine(TAB + TAB + "public async Task SaveTask()");
            sb.AppendLine(TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + "bool valid = await this.ValidateAllAsync();");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + $"if (!valid)");
            sb.AppendLine(TAB + TAB + TAB + TAB + "throw new Exception(\"Correct input.\");");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + "if (this.CreateNew)");
            sb.AppendLine(TAB + TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + TAB + "Recordset.Append();");

            foreach (var column in this.SelectedColumns)
            {
                if (column.IsKey == true)
                {
                    if (column.IsAutoIncrement)
                        sb.AppendLine(TAB + TAB + TAB + TAB + $"// skipped primary key column Recordset.{column.PropertyName()} as it is AutoIncrement");
                    else
                        sb.AppendLine(TAB + TAB + TAB + TAB + $"Recordset.{column.PropertyName()} = this.{column.PropertyName()};");
                }
            }

            sb.AppendLine(TAB + TAB + TAB + "}");
            sb.AppendLine(TAB + TAB + TAB + "else");
            sb.AppendLine(TAB + TAB + TAB + "{");
            sb.AppendLine(TAB + TAB + TAB + TAB + "await Recordset.ExecSqlAsync(/* this... prikey parameters here */);");
            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + TAB + "if (Recordset.RecordCount == 0)");
            sb.AppendLine(TAB + TAB + TAB + TAB + TAB + "throw new Exception($\"Record {\"details here\"} not found in database.\");");
            sb.AppendLine(TAB + TAB + TAB + "}");

            sb.AppendLine();

            foreach (var column in this.SelectedColumns)
            {
                if (column.IsKey == false)
                {
                    if (column.IsAutoIncrement)
                        sb.AppendLine(TAB + TAB + TAB + $"// skipped column Recordset.{column.PropertyName()} as it is AutoIncrement");
                    else
                        sb.AppendLine(TAB + TAB + TAB + $"Recordset.{column.PropertyName()} = this.{column.PropertyName()};");
                }
            }

            foreach (var column in this.Selected_UDC_Columns)
                sb.AppendLine(TAB + TAB + TAB + $"Recordset.{column.PropertyName} = this.{column.PropertyName};");

            sb.AppendLine();
            sb.AppendLine(TAB + TAB + TAB + "await Recordset.SaveChangesAsync();");

            sb.AppendLine(TAB + TAB + "}");
            sb.AppendLine();

            sb.AppendLine(TAB + "}");
            sb.AppendLine("}");

            return sb.ToString();
        }

    }
}
