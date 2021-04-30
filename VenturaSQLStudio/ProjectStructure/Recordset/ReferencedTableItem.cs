using System.ComponentModel;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio {

    public class ReferencedTableItem : ViewModelBase
    {
        private TableName _table_name = null;
        private bool _invalid = false;

        public string DisplayString
        {
            get
            {
                if (_table_name == null)
                    return "(read-only)";

                if (_invalid)
                    return _table_name.ScriptTableName + " (invalid)";
                else
                    return _table_name.ScriptTableName;
            }
        }

        public TableName DataObject
        {
            get { return _table_name; }
            set
            {
                if (_table_name == value)
                    return;

                _table_name = value;

                NotifyPropertyChanged("DataObject");
            }
        }

        public bool Invalid
        {
            get { return _invalid; }
            set
            {
                if (_invalid == value)
                    return;

                _invalid = value;

                NotifyPropertyChanged("Invalid");
                NotifyPropertyChanged("DisplayString");
            }
        }

        public ReferencedTableItem Clone()
        {
            ReferencedTableItem temp = new ReferencedTableItem();
            temp.DataObject = _table_name;
            temp.Invalid = _invalid;
            return temp;
        }

    }
}