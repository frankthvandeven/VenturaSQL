using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Pages
{
    class SnippetListOfQuotedColumns : SnippetCreatorBase
    {
        public SnippetListOfQuotedColumns()
        {
            //UsesParameter_IncludeUDCs = true;
        }

        public override string CreateCode()
        {
            StringBuilder sb = new StringBuilder();

            foreach (var item in this.SelectedColumns)
                sb.AppendLine( QUOTE + item.PropertyName() + QUOTE);

            foreach (var item in this.Selected_UDC_Columns)
                sb.AppendLine(QUOTE + item.PropertyName + QUOTE);

            return sb.ToString();
        }


    }
}
