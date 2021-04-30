using System.Collections.Generic;
using VenturaSQL;

namespace VenturaSQLStudio.Pages
{
    abstract class SnippetCreatorBase
    {
        protected const string CRLF = "\r\n";
        protected const string TAB = "\t";
        protected const string QUOTE = "\"";

        // Parameters
        public string SyntaxHighlighting { get; set; } = "C#";
        public List<VenturaColumn> SelectedColumns { get; set; }
        public List<UDCItem> Selected_UDC_Columns { get; set; }
        public string RecordsetVariable { get; set; }
        public string ViewmodelVariable { get; set; }

        // Type names derived from Recordset and eventual multiple resultsets.
        public string RecordsetTypename;
        public string ResultsetTypename;
        public string RecordTypeName;

        // Switches
        public bool UsesParameter_RecordsetVariable = false;
        public bool UsesParameter_ViewmodelVariable = false;
        //public bool UsesParameter_IncludeUDCs = false;

        public abstract string CreateCode();



    }
}
