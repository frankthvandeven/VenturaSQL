using System.ComponentModel;
using System.Windows.Controls;

namespace VenturaSQLStudio {
    public class Tab : ViewModelBase
    {
        private string _uniqueid;
        private string _header;
        private UserControl _content;
        private object _datacontext;
        private ContextMenu _contextmenu;
        private bool _showclosebutton;
        private RecordsetItem _recordset_item;

        public Tab(string unique_id, string header, UserControl content, object datacontext, bool showclosebutton)
        {
            _uniqueid = unique_id;
            _header = header;
            _content = content;
            _datacontext = datacontext;
            _contextmenu = null;
            _showclosebutton = showclosebutton;

            // If the datacontext is a RecordsetItem we listen for property changes.
            _recordset_item = datacontext as RecordsetItem;

            if (_recordset_item != null)
                _recordset_item.PropertyChanged += Recordset_item_PropertyChanged;
        }

        private void Recordset_item_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "ClassName")
                this.Header = _recordset_item.ClassName;
        }

        public bool ShowCloseButton
        {
            get { return _showclosebutton; }
        }

        public string UniqueID
        {
            get { return _uniqueid; }
            //set
            //{
            //    if (_uniqueid == value)
            //        return;

            //    _uniqueid = value;

            //    NotifyPropertyChanged("UniqueID");
            //}
        }

        public string Header
        {
            get { return _header; }
            private set
            {
                if (_header == value)
                    return;

                _header = value;

                NotifyPropertyChanged("Header");
            }
        }

        public UserControl Content
        {
            get { return _content; }
            //set
            //{
            //    if (_content == value)
            //        return;
                
            //    _content = value;

            //    NotifyPropertyChanged("Content");
            //}
        }

        public object DataContext
        {
            get { return _datacontext; }
            //set
            //{
            //    if (_datacontext == value)
            //        return;

            //    _datacontext = value;

            //    NotifyPropertyChanged("DataContext");
            //}
        }

        public ContextMenu ContextMenu
        {
            get { return _contextmenu; }
            set
            {
                if (_contextmenu == value)
                    return;

                _contextmenu = value;

                NotifyPropertyChanged("ContextMenu");
            }
        }

    }

    public enum TabMenu
    {
        CannotClose,
        CloseAble
    }
}
