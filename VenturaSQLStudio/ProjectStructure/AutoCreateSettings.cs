using System.Collections.ObjectModel;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio {
    public class AutoCreateSettings : ViewModelBase
    {
        private Project _owningproject;

        private bool _enabled = false;
        private string _folder = "VenturaAutoCreate";
        private bool _create_get_all = true;
        private bool _create_incremental = false;

        private ObservableCollection<TableName> _excluded_tablenames;

        public AutoCreateSettings(Project project)
        {
            _owningproject = project;

            _excluded_tablenames = new ObservableCollection<TableName>();

            _excluded_tablenames.CollectionChanged += Excluded_tablenames_CollectionChanged;
        }

        private void Excluded_tablenames_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _owningproject?.SetModified();
        }

        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;

                NotifyPropertyChanged("Enabled");
                NotifyPropertyChanged("AutoCreateInfoForSettingsPage");

                _owningproject?.SetModified();
            }
        }

        public string Folder
        {
            get { return _folder; }
            set
            {
                if (_folder == value)
                    return;

                _folder = value;

                NotifyPropertyChanged("Folder");

                _owningproject?.SetModified();
            }
        }

        public bool CreateGetAll
        {
            get { return _create_get_all; }
            set
            {
                if (_create_get_all == value)
                    return;

                _create_get_all = value;

                NotifyPropertyChanged("CreateGetAll");

                _owningproject?.SetModified();
            }
        }

        public bool CreateIncremental
        {
            get { return _create_incremental; }
            set
            {
                if (_create_incremental == value)
                    return;

                _create_incremental = value;

                NotifyPropertyChanged("CreateIncremental");

                _owningproject?.SetModified();
            }
        }

        public ObservableCollection<TableName> ExcludedTablenames
        {
            get { return _excluded_tablenames; }
        }

        public string AutoCreateInfoForSettingsPage
        {
            get
            {
                if (_enabled == false)
                    return "Auto Create Recordsets is off.";

                return "Auto Create Recordsets is enabled.";
            }
        }


    }
}
