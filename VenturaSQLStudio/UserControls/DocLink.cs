using System.Diagnostics;
using System.Windows.Documents;

namespace VenturaSQLStudio
{
    public class DocLink : Hyperlink
    {
        private string _page_name = null;
        private const string _url = "https://docs.sysdev.nl/{0}.html";

        public DocLink()
        {

        }

        /// <summary>
        /// The page name in the online documentation without the .html extension.
        /// </summary>
        public string PageName
        {
            get { return _page_name; }
            set
            {
                _page_name = value;
                ToolTip = "Opens page " + string.Format(_url, value) + " in the browser.";
            }
        }


        protected override void OnClick()
        {
            if (_page_name != null)
                StudioGeneral.StartBrowser(string.Format(_url, _page_name));

            base.OnClick();
        }



    }


}
