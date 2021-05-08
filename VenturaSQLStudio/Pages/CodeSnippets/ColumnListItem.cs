using VenturaSQL;

namespace VenturaSQLStudio.Pages
{
    public class ColumnListItem : ViewModelBase
    {
        private bool _include = true;
        private string _name;
        private VenturaSqlColumn _schema_column;
        private UDCItem _udc_column;

        public ColumnListItem(string name, VenturaSqlColumn schema_column, UDCItem udc_column)
        {
            _name = name;
            _schema_column = schema_column;
            _udc_column = udc_column;

            if (udc_column != null)
                _include = false;

        }

        public bool Include
        {
            get { return _include; }
            set
            {
                if (_include == value)
                    return;

                _include = value;

                NotifyPropertyChanged("Include");
            }
        }

        public string Name
        {
            get
            {
                if (_udc_column != null)
                    return _name + " (UDC)";

                return _name;
            }
        }

        public VenturaSqlColumn SchemaColumn
        {
            get { return _schema_column; }
        }

        public UDCItem UDC_Column
        {
            get { return _udc_column; }
        }

    }
}
