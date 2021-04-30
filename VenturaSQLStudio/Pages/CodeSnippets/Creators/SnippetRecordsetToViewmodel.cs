using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Pages
{
    class SnippetRecordsetToViewmodel : SnippetCreatorBase
    {
        public SnippetRecordsetToViewmodel()
        {
            UsesParameter_RecordsetVariable = true;
            UsesParameter_ViewmodelVariable = true;
            //UsesParameter_IncludeUDCs = true;
        }

        public override string CreateCode()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in this.SelectedColumns)
                sb.AppendLine( $"{ViewmodelVariable}.{item.PropertyName()} = {RecordsetVariable}.{item.PropertyName()};");

            foreach (var item in this.Selected_UDC_Columns)
                sb.AppendLine($"{ViewmodelVariable}.{item.PropertyName} = {RecordsetVariable}.{item.PropertyName};");

            return sb.ToString();
        }


    }
}
