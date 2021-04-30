using System.Text;

namespace VenturaSQLStudio
{
    public class AdvancedSettings : ViewModelBase
    {
        private Project _owningproject;

        private bool _include_server_name;
        private bool _include_catalog_name;
        private bool _include_schema_name;

        private string _column_server;
        private string _column_catalog;
        private string _column_schema;
        private string _column_table;
        private string _column_type;

        private const bool DEFAULT_INCLUDE_SERVER = false;
        private const bool DEFAULT_INCLUDE_CATALOG = false;
        private const bool DEFAULT_INCLUDE_SCHEMA = true;

        private const string DEFAULT_SERVER = "";  // ""
        private const string DEFAULT_CATALOG = ""; // "TABLE_CATALOG";
        private const string DEFAULT_SCHEMA = "";  // "TABLE_SCHEMA";
        private const string DEFAULT_TABLE = "";   // "TABLE_NAME";
        private const string DEFAULT_TYPE = "";    // "TABLE_TYPE";

        public AdvancedSettings(Project project)
        {
            _owningproject = project;

            _include_server_name = DEFAULT_INCLUDE_SERVER;
            _include_catalog_name = DEFAULT_INCLUDE_CATALOG;
            _include_schema_name = DEFAULT_INCLUDE_SCHEMA;

            _column_server = DEFAULT_SERVER;
            _column_catalog = DEFAULT_CATALOG;
            _column_schema = DEFAULT_SCHEMA;
            _column_table = DEFAULT_TABLE;
            _column_type = DEFAULT_TYPE;
        }

        public void ResetToDefault()
        {
            this.IncludeServerName = DEFAULT_INCLUDE_SERVER;
            this.IncludeCatalogName = DEFAULT_INCLUDE_CATALOG;
            this.IncludeSchemaName = DEFAULT_INCLUDE_SCHEMA;

            this.ColumnServer = DEFAULT_SERVER;
            this.ColumnCatalog = DEFAULT_CATALOG;
            this.ColumnSchema = DEFAULT_SCHEMA;
            this.ColumnTable = DEFAULT_TABLE;
            this.ColumnType = DEFAULT_TYPE;
        }

        public bool IsSetToDefaultValues()
        {
            if (_include_server_name != DEFAULT_INCLUDE_SERVER)
                return false;

            if (_include_catalog_name != DEFAULT_INCLUDE_CATALOG)
                return false;

            if (_include_schema_name != DEFAULT_INCLUDE_SCHEMA)
                return false;

            if (_column_server != DEFAULT_SERVER)
                return false;

            if (_column_catalog != DEFAULT_CATALOG)
                return false;

            if (_column_schema != DEFAULT_SCHEMA)
                return false;

            if (_column_table != DEFAULT_TABLE)
                return false;

            if (_column_type != DEFAULT_TYPE)
                return false;

            return true;
        }

        /// <summary>
        /// Include in fully-qualified-table-name?
        /// </summary>
        public bool IncludeServerName
        {
            get { return _include_server_name; }
            set
            {
                if (_include_server_name == value)
                    return;

                _include_server_name = value;

                NotifyPropertyChanged("IncludeServerName");
                NotifyPropertyChanged("SampleFullyQualifiedTableName");

                _owningproject?.SetModified();
            }
        }

        /// <summary>
        /// Include in fully-qualified-table-name?
        /// </summary>
        public bool IncludeCatalogName
        {
            get { return _include_catalog_name; }
            set
            {
                if (_include_catalog_name == value)
                    return;

                _include_catalog_name = value;

                NotifyPropertyChanged("IncludeCatalogName");
                NotifyPropertyChanged("SampleFullyQualifiedTableName");

                _owningproject?.SetModified();
            }
        }

        /// <summary>
        /// Include in fully-qualified-table-name?
        /// </summary>
        public bool IncludeSchemaName
        {
            get { return _include_schema_name; }
            set
            {
                if (_include_schema_name == value)
                    return;

                _include_schema_name = value;

                NotifyPropertyChanged("IncludeSchemaName");
                NotifyPropertyChanged("SampleFullyQualifiedTableName");

                _owningproject?.SetModified();
            }
        }

        public string ColumnServer
        {
            get { return _column_server; }
            set
            {
                value = value.Trim();

                if (_column_server == value)
                    return;

                _column_server = value;

                NotifyPropertyChanged("ColumnServer");

                _owningproject?.SetModified();
            }
        }

        public string ColumnCatalog
        {
            get { return _column_catalog; }
            set
            {
                value = value.Trim();

                if (_column_catalog == value)
                    return;

                _column_catalog = value;

                NotifyPropertyChanged("ColumnCatalog");

                _owningproject?.SetModified();
            }
        }

        public string ColumnSchema
        {
            get { return _column_schema; }
            set
            {
                value = value.Trim();

                if (_column_schema == value)
                    return;

                _column_schema = value;

                NotifyPropertyChanged("ColumnSchema");

                _owningproject?.SetModified();
            }
        }

        public string ColumnTable
        {
            get { return _column_table; }
            set
            {
                value = value.Trim();

                if (_column_table == value)
                    return;

                _column_table = value;

                NotifyPropertyChanged("ColumnTable");

                _owningproject?.SetModified();
            }
        }

        public string ColumnType
        {
            get { return _column_type; }
            set
            {
                value = value.Trim();

                if (_column_type == value)
                    return;

                _column_type = value;

                NotifyPropertyChanged("ColumnType");

                _owningproject?.SetModified();
            }
        }

        public string SampleFullyQualifiedTableName
        {
            get
            {
                Project project = MainWindow.ViewModel.CurrentProject;
                string prefix = project.QuotePrefix;
                string suffix = project.QuoteSuffix;

                StringBuilder sb = new StringBuilder();

                if (_include_server_name == true)
                {
                    sb.Append(prefix);
                    sb.Append("SQLPROD55");
                    sb.Append(suffix);

                    sb.Append(".");
                }

                if (_include_catalog_name == true)
                {
                    sb.Append(prefix);
                    sb.Append("AdventureWorksDatabase");
                    sb.Append(suffix);

                    sb.Append(".");
                }

                if (_include_schema_name == true)
                {
                    sb.Append(prefix);
                    sb.Append("Sales");
                    sb.Append(suffix);

                    sb.Append(".");
                }

                sb.Append(prefix);
                sb.Append("Customer");
                sb.Append(suffix);

                return sb.ToString();
            }
        }

    }
}
