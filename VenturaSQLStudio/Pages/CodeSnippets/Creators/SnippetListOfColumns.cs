using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Pages
{
    class SnippetListOfColumns : SnippetCreatorBase
    {
        public SnippetListOfColumns()
        {
            //UsesParameter_IncludeUDCs = true;
        }

        public override string CreateCode()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in this.SelectedColumns)
                sb.AppendLine(item.PropertyName());

            foreach (var item in this.Selected_UDC_Columns)
                sb.AppendLine(item.PropertyName);

            return sb.ToString();
        }


    }
}
