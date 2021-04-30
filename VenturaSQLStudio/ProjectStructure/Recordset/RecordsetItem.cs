using System.Collections.ObjectModel;
using System.Text;
using System.Windows;

namespace VenturaSQLStudio
{
    public class RecordsetItem : ViewModelBase, ITreeViewItem
    {
        // For support of the ITreeViewItem interface
        private FolderItem _parent;

        private bool _isExpanded;
        private bool _isSelected;

        private string _classname = "";
        private bool _enabled = true;

        private string _class_summary = "";
        private string _sql_script = "";

        private ResultsetCollection _resultsets;

        private bool _implement_databinding;
        private bool _rowload_incremental;

        private ObservableCollection<OutputProjectSelectionItem> _selected_vs_projects = new ObservableCollection<OutputProjectSelectionItem>();

        public ParameterCollection Parameters;
        public UDCCollection UserDefinedColumns;

        private Project _owningproject;

        public RecordsetItem(Project owningproject, FolderItem parent, string classname)
        {
            _owningproject = owningproject;
            _parent = parent;
            _classname = classname;

            _implement_databinding = true;
            _rowload_incremental = false;

            _selected_vs_projects.CollectionChanged += _selected_vs_projects_CollectionChanged;

            this.Parameters = new ParameterCollection(owningproject);
            this.UserDefinedColumns = new UDCCollection(owningproject);

            for (int i = 0; i < owningproject.VisualStudioProjects.Count; i++)
                _selected_vs_projects.Add(new OutputProjectSelectionItem(owningproject, i, true));

            _resultsets = new ResultsetCollection(owningproject);
        }

        private void _selected_vs_projects_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _owningproject?.SetModified();
        }

        public string ClassName
        {
            get { return _classname; }
            set
            {
                if (_classname == value)
                    return;

                _classname = value;

                NotifyPropertyChanged("ClassName");
                NotifyPropertyChanged("HeaderText");

                _owningproject?.SetModified();
            }
        }

        /// <summary>
        /// Displayed in the top of the recordset editor Tab page.
        /// </summary>
        public string HeaderText
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                sb.Append(_classname);

                if (this.IsInAutoCreateFolder)
                {
                    sb.Append("  (autocreate)");
                }

                return sb.ToString();
            }
        }

        /// <summary>
        /// Enabled here means "include in code generation"
        /// </summary>
        public bool Enabled
        {
            get { return _enabled; }
            set
            {
                if (_enabled == value)
                    return;

                _enabled = value;

                NotifyPropertyChanged("Enabled");
                NotifyPropertyChanged("TextDecorationForTreeview");

                _owningproject?.SetModified();
            }
        }

        public bool RowloadIncremental
        {
            get { return _rowload_incremental; }
            set
            {
                if (_rowload_incremental == value)
                    return;

                _rowload_incremental = value;

                NotifyPropertyChanged("RowloadIncremental");

                _owningproject?.SetModified();
            }
        }

        public TextDecorationCollection TextDecorationForTreeview
        {
            get
            {
                if (_enabled == true)
                    return null;
                else
                    return TextDecorations.Strikethrough;
            }
        }

        public bool ImplementDatabinding
        {
            get { return _implement_databinding; }
            set
            {
                if (_implement_databinding == value)
                    return;

                _implement_databinding = value;
                NotifyPropertyChanged("ImplementDatabinding");
                NotifyPropertyChanged("HighPerformance");

                _owningproject?.SetModified();
            }
        }

        /// <summary>
        /// For RadioButton
        /// </summary>
        public bool HighPerformance
        {
            get { return !_implement_databinding; }
            set { this.ImplementDatabinding = !value; }
        }

        public string ClassSummary
        {
            get { return _class_summary; }
            set
            {
                if (_class_summary == value)
                    return;

                _class_summary = value;

                NotifyPropertyChanged("ClassSummary");

                _owningproject?.SetModified();
            }
        }

        public string SqlScript
        {
            get { return _sql_script; }
            set
            {
                if (_sql_script == value)
                    return;

                _sql_script = value;

                NotifyPropertyChanged("SqlScript");

                _owningproject?.SetModified();
            }
        }

        /// <summary>
        /// Does not contain resultsets itself, but settings for each resultset.
        /// </summary>
        public ResultsetCollection Resultsets
        {
            get { return _resultsets; }
            set { _resultsets = value; }
        }

        public ObservableCollection<OutputProjectSelectionItem> OutputProjects
        {
            get { return _selected_vs_projects; }
        }

        FolderItem ITreeViewItem.Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        TreeViewModelKind ITreeViewItem.ItemKind
        {
            get { return TreeViewModelKind.RecordsetItem; }
        }

        string ITreeViewItem.Name
        {
            get { return _classname; }
        }

        public RecordsetItem Clone()
        {
            RecordsetItem temp = new RecordsetItem(_owningproject, null, null);
            temp.ClassName = this.ClassName;
            temp.Enabled = this.Enabled;
            temp.ClassSummary = this.ClassSummary;
            temp.SqlScript = this.SqlScript;
            temp.Parameters = this.Parameters.Clone();
            temp.UserDefinedColumns = this.UserDefinedColumns.Clone();
            temp.ImplementDatabinding = this.ImplementDatabinding;
            temp.RowloadIncremental = this.RowloadIncremental;

            for (int i = 0; i < temp.OutputProjects.Count; i++)
            {
                temp.OutputProjects[i] = this.OutputProjects[i];
            }

            temp.Resultsets = this.Resultsets.Clone();
            return temp;
        }

        /// <summary>
        /// Gets/sets whether the TreeViewExItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded == value)
                    return;

                _isExpanded = value;

                NotifyPropertyChanged("IsExpanded");

                //_owningproject?.SetModified();
            }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewExItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value == _isSelected)
                    return;

                _isSelected = value;

                NotifyPropertyChanged("IsSelected");

                // Modified notification disabled as a simple mouse-click-select
                // would set the project to modified.
                //_owningproject?.SetModified();
            }
        }

        // Call this method if any of the advanced settings for the provider were modified.
        // This includes: parameter prefix and table name settings.
        public void ProviderRelatedSettingsWereModified()
        {
            char prefix = MainWindow.ViewModel.CurrentProject.ParameterPrefix;

            foreach (ParameterItem item in Parameters)
                item.Name = prefix + item.Name.Substring(1);

            // Touch the referenced tables list display name for each resultset.
            foreach (ResultsetItem resultset_item in _resultsets)
            {
                foreach (ReferencedTableItem rti in resultset_item.ReferencedTablesList)
                    rti.NotifyPropertyChanged("DisplayString");
            }

        }

        public void FolderStructureWasModified()
        {
            this.NotifyPropertyChanged("FolderStructureWasModified");
            this.NotifyPropertyChanged("IsInAutoCreateFolder");
            this.NotifyPropertyChanged("HeaderText");
        }

        public bool IsInAutoCreateFolder
        {
            get
            {
                var item_path = ((ITreeViewItem)this).Parent.CalculatePath();
                var acf = _owningproject.AutoCreateSettings.Folder;

                if (item_path == acf || item_path.StartsWith(acf + "\\"))
                    return true;

                return false;
            }
        }

    }
}
