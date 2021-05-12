using System;
using System.Text;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Linq;
using System.Windows.Media;
using System.IO;
using VenturaSQL;
using VenturaSQLStudio.ProviderHelpers;

namespace VenturaSQLStudio {
    public class Project : ViewModelBase
    {
        private string _provider_invariant_name;
        private ProviderHelper _provider_helper;
        private string _connection_string;

        private AdvancedSettings _advanced_settings;
        private AutoCreateSettings _autocreate_settings;

        private List<VisualStudioProjectItem> _visualStudioProjects = new List<VisualStudioProjectItem>();

        private RootItem _folderstructure;

        private bool _modified;

        private const string DEFAULT_PROVIDER_VARIANT_NAME = "System.Data.SqlClient";

        public Project()
        {
            _folderstructure = new RootItem(this);

            _advanced_settings = new AdvancedSettings(this);
            _autocreate_settings = new AutoCreateSettings(this);

            // On this line the number of Visual Studio output projects is set:
            const int NUMBER_OF_PROJECTS = 6;

            for (int i = 1; i <= NUMBER_OF_PROJECTS; i++)
                _visualStudioProjects.Add(new VisualStudioProjectItem(this, i));

            // Note: Collections should be cleared and not reinstantiated. This is necessary for data binding.           

            _provider_invariant_name = DEFAULT_PROVIDER_VARIANT_NAME;
            _provider_helper = MainWindow.ViewModel.ProviderRepository.FirstOrDefault(z => z.ProviderInvariantName == DEFAULT_PROVIDER_VARIANT_NAME);

            if (_provider_helper == null)
                throw new Exception($"ProviderHelper for {DEFAULT_PROVIDER_VARIANT_NAME} not found. Should not happen.");

            _connection_string = "Server=(local);Initial Catalog=YourDatabaseNameHere;Integrated Security=SSPI;Max Pool Size=250;Connect Timeout=30;";

            _modified = false;
        }

        public bool IsModified
        {
            get { return _modified; }
            //set
            //{
            //    if (_modified == value)
            //        return;

            //    _modified = value;

            //    NotifyPropertyChanged("IsModified");
            //}
        }

        /// <summary>
        /// Mark the project as modified.
        /// </summary>
        public void SetModified()
        {
            if (_modified == true)
                return;

            _modified = true;

            NotifyPropertyChanged("IsModified");
        }

        /// <summary>
        /// Mark the project as unmodified.
        /// </summary>
        public void ResetModified()
        {
            if (_modified == false)
                return;

            _modified = false;

            NotifyPropertyChanged("IsModified");

        }

        public AdvancedSettings AdvancedSettings
        {
            get { return _advanced_settings; }
        }

        public AutoCreateSettings AutoCreateSettings
        {
            get { return _autocreate_settings; }
        }

        public RootItem FolderStructure
        {
            get { return _folderstructure; }
        }

        public List<VisualStudioProjectItem> VisualStudioProjects
        {
            get
            {
                return _visualStudioProjects;
            }
        }

        public string ProviderInvariantName
        {
            get { return _provider_invariant_name; }
            set
            {
                if (_provider_invariant_name == value)
                    return;

                if (value == null)
                    throw new ArgumentNullException(nameof(ProviderInvariantName));

                _provider_invariant_name = value;
                _provider_helper = MainWindow.ViewModel.ProviderRepository.FirstOrDefault(z => z.ProviderInvariantName == _provider_invariant_name);

                // If a provider variant name is not in the ProviderRepository, then we cannot use it.

                if( _provider_helper == null )
                {
                    // Fall back to default provider.
                    _provider_invariant_name = DEFAULT_PROVIDER_VARIANT_NAME;
                    _provider_helper = MainWindow.ViewModel.ProviderRepository.FirstOrDefault(z => z.ProviderInvariantName == DEFAULT_PROVIDER_VARIANT_NAME);
                }

                NotifyPropertyChanged("ProviderInvariantName");
                NotifyPropertyChanged("ProviderHelper");
                NotifyPropertyChanged("ProviderInfoImage");
                NotifyPropertyChanged("ProviderInfoName");
                NotifyPropertyChanged("ProviderInfoDescription");
                NotifyPropertyChanged("ParameterPrefix");
                NotifyPropertyChanged("QuotePrefix");
                NotifyPropertyChanged("QuoteSuffix");
                NotifyPropertyChanged("ConnectorCode");

                this.SetModified();
            }
        }

        public ProviderHelper ProviderHelper
        {
            get { return _provider_helper; }
        }


        #region Extended provider information (readonly)

        public ImageSource ProviderInfoImage
        {
            get
            {
                // The information comes from the repository, and NOT from the provider DLL.
                if (_provider_helper == null)
                    return ProviderHelper.GetProductImageFromFilename("default_not_installed.png");
                else
                    return _provider_helper.ProductImage;
            }
        }

        public string ProviderInfoName
        {
            get
            {
                if (_provider_helper == null)
                    return _provider_invariant_name;
                else
                    return _provider_helper.Name;
            }
        }

        public string ProviderInfoDescription
        {
            get
            {
                if (_provider_helper == null)
                    return "The provider is not installed or registered.";
                else
                    return _provider_helper.Description;
            }
        }

        #endregion

        public string ConnectionString
        {
            get { return _connection_string; }
            set
            {
                if (_connection_string == value)
                    return;

                _connection_string = value;

                NotifyPropertyChanged("ConnectionString");
                NotifyPropertyChanged("ConnectorCode");
                NotifyPropertyChanged("MacroConnectionString");

                this.SetModified();
            }
        }

        public string ConnectorCode
        {
            get
            {
                StringBuilder sb = new();

                sb.Append("var connector = new AdoConnector(");

                if (_provider_helper != null && !string.IsNullOrEmpty(_provider_helper.FactoryAsString))
                    sb.Append(_provider_helper.FactoryAsString);
                else
                    sb.Append('?');

                sb.Append(", \"");
                sb.Append(this.MacroConnectionString.Replace("\"","\\\""));
                sb.Append("\");");

                return sb.ToString();
            }
        }

        /// <summary>
        /// This is the connection string with the macro {pf} filled in.
        /// {pf} is the project file folder. For example: "c:\users\fvv\projects"
        /// </summary>
        public string MacroConnectionString
        {
            get
            {
                string output = _connection_string;

                output = output.Replace("{pf}", Path.GetDirectoryName(MainWindow.ViewModel.FileName));

                return output;
            }
        }

        /// <summary>
        /// Parameter Prefix. For SQL Server this is '@'
        /// </summary>
        public char ParameterPrefix
        {
            get
            {
                AdoConnector temp = AdoConnectorHelper.Create(_provider_invariant_name);

                return temp.ParameterPrefix;
            }
        }

        /// <summary>
        /// Quote Prefix for column and table names. For SQL Server this is '['
        /// </summary>
        public string QuotePrefix
        {
            get
            {
                AdoConnector temp = AdoConnectorHelper.Create(_provider_invariant_name);

                return temp.QuotePrefix;
            }
        }

        /// <summary>
        /// Quote Prefix for column and table names. For SQL Server this is ']'
        /// </summary>
        public string QuoteSuffix
        {
            get
            {
                AdoConnector temp = AdoConnectorHelper.Create(_provider_invariant_name);

                return temp.QuoteSuffix;
            }
        }

        // Call this method if any of the advanced settings for the provider were modified.
        // This includes: parameter prefix and table name settings
        public void ProviderRelatedSettingsWereModified()
        {
            List<ITreeViewItem> list = _folderstructure.AllProjectItemsInThisFolderAndSubfolders();

            for (int i = 0; i < list.Count; i++)
            {
                RecordsetItem rsi = list[i] as RecordsetItem;

                if (rsi != null)
                    rsi.ProviderRelatedSettingsWereModified();
            }


        }

        public void FolderStructureWasModified()
        {
            List<ITreeViewItem> list = _folderstructure.AllProjectItemsInThisFolderAndSubfolders();

            for (int i = 0; i < list.Count; i++)
            {
                RecordsetItem rsi = list[i] as RecordsetItem;

                if (rsi != null)
                    rsi.FolderStructureWasModified();
            }
        }

    }
}
