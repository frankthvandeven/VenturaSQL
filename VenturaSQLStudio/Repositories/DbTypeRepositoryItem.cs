using System;
using System.Data;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VenturaSQLStudio {
    public class DbTypeRepositoryItem : ViewModelBase
    {
        private string _id;
        private ParameterGroup _group;
        private DbType? _dbtype;
        private Type _framework_type;
        private string _title;
        private string _description;
        private DbType? _based_on;

        private bool _set_length; // as an int32
        private bool _set_precision; // as a byte
        private bool _set_scale; // as a byte

        public DbTypeRepositoryItem(string id)
        {
            _id = id;
            _dbtype = null;
            _framework_type = null;
            _based_on = null;

            _set_precision = false;
            _set_scale = false;
            _set_length = false;
        }

        public string Id
        {
            get { return _id; }
        }
        
        public ParameterGroup Group
        {
            get { return _group; }
            set
            {
                if (_group == value)
                    return;

                _group = value;

                NotifyPropertyChanged("Group");
            }
        }

        public DbType? DbType
        {
            get { return _dbtype; }
            set
            {
                if (_dbtype == value)
                    return;

                _dbtype = value;

                NotifyPropertyChanged("DbType");
            }
        }

        public Type FrameworkType
        {
            get { return _framework_type; }
            set
            {
                if (_framework_type == value)
                    return;

                _framework_type = value;

                NotifyPropertyChanged("FrameworkType");
            }
        }

        public string Title
        {
            get { return _title; }
            set
            {
                if (_title == value)
                    return;

                _title = value;

                NotifyPropertyChanged("Title");
            }
        }

        public string Description
        {
            get { return _description; }
            set
            {
                if (_description == value)
                    return;

                _description = value;

                NotifyPropertyChanged("Description");
            }
        }

        public DbType? BasedOn
        {
            get { return _based_on; }
            set
            {
                if (_based_on == value)
                    return;

                _based_on = value;

                NotifyPropertyChanged("BasedOn");
            }
        }

        public bool SetLength
        {
            get { return _set_length; }
            set
            {
                if (_set_length == value)
                    return;

                _set_length = value;

                NotifyPropertyChanged("SetLength");
            }
        }

        public bool SetPrecision
        {
            get { return _set_precision; }
            set
            {
                if (_set_precision == value)
                    return;

                _set_precision = value;

                NotifyPropertyChanged("SetPrecision");
            }
        }

        public bool SetScale
        {
            get { return _set_scale; }
            set
            {
                if (_set_scale == value)
                    return;

                _set_scale = value;

                NotifyPropertyChanged("SetScale");
            }
        }

    }

    public enum ParameterGroup
    {
        ByTask,
        ByCodeType,
        ByDbType,
        BySqlDbType
    }


}

