using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Pages
{
    class SnippetViewmodelToRecordset : SnippetCreatorBase
    {
        public SnippetViewmodelToRecordset()
        {
            UsesParameter_RecordsetVariable = true;
            UsesParameter_ViewmodelVariable = true;
            //UsesParameter_IncludeUDCs = true;
        }

        public override string CreateCode()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in this.SelectedColumns)
                sb.AppendLine( $"{RecordsetVariable}.{item.PropertyName()} = {ViewmodelVariable}.{item.PropertyName()};");

            foreach (var item in this.Selected_UDC_Columns)
                sb.AppendLine($"{RecordsetVariable}.{item.PropertyName} = {ViewmodelVariable}.{item.PropertyName};");



            return sb.ToString();
        }


    }
}
