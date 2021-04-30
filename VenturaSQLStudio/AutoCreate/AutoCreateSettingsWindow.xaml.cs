using System.Windows;
using System.Linq;
using System.Windows.Input;
using System.IO;
using System.Collections.ObjectModel;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio.Pages
{
    public partial class AutoCreateSettingsWindow : Window
    {
        public ViewModelClass ViewModel { get; } = new ViewModelClass();

        Project _project;

        public AutoCreateSettingsWindow(Project project)
        {
            _project = project;

            this.DataContext = this;

            InitializeComponent();

            this.Width = 800;
            this.Height = 600;

            this.MinWidth = 800;
            this.MinHeight = 600;

            this.Loaded += AutoCreateWindow_Loaded;
        }

        private void AutoCreateWindow_Loaded(object sender, RoutedEventArgs e)
        {
            ViewModel.Enabled = _project.AutoCreateSettings.Enabled;
            ViewModel.Folder = _project.AutoCreateSettings.Folder;
            ViewModel.CreateGetAll = _project.AutoCreateSettings.CreateGetAll;
            ViewModel.CreateIncremental = _project.AutoCreateSettings.CreateIncremental;

            ViewModel.List.CollectListOfTables(_project); // The data binding to lvTables is not there. Why?

            lvTables.ItemsSource = ViewModel.List; // This fixes the binding problem.

            tbFolderName.Focus();

        }

        private void OkCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            //if (ViewModel.List.Count > 0)
            e.CanExecute = true;
        }

        private void OkCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // This makes sure the changed data in a focussed control is committed to the ViewModel
            FocusManager.SetFocusedElement(this, null);

            ViewModel.Folder = ViewModel.Folder.Trim();

            if (ViewModel.Folder.Length == 0)
            {
                Error("Enter a valid folder name.");
                tbFolderName.Focus();
                return;
            }

            if (IsFoldernameValid(ViewModel.Folder) == false)
            {
                Error(@"A folder name can't contain any of the following characters: \ / : * ? "" < > |");
                tbFolderName.Focus();
                return;
            }

            _project.AutoCreateSettings.Enabled = ViewModel.Enabled;
            _project.AutoCreateSettings.Folder = ViewModel.Folder;
            _project.AutoCreateSettings.CreateGetAll = ViewModel.CreateGetAll;
            _project.AutoCreateSettings.CreateIncremental = ViewModel.CreateIncremental;

            ObservableCollection<TableName> excluded = _project.AutoCreateSettings.ExcludedTablenames;

            // Add items from the table list to the project's excluded list.
            foreach (TableListItem item in ViewModel.List)
            {
                if (item.Exclude == true)
                {
                    bool found = excluded.Any(a => a == item.PreliminaryTableName);

                    if (found == false)
                        excluded.Add(item.PreliminaryTableName);

                }
            }

            // Remove items from the project's excluded list if the items is not present and selected in the table list.
            for (int x = excluded.Count - 1; x >= 0; x--)
            {
                TableName item = excluded[x];

                bool found = ViewModel.List.Any(a => a.Exclude == true && a.PreliminaryTableName == item);

                if (found == false)
                    excluded.RemoveAt(x);
            }

            // Update the "auto create tip" in all open recordset tabs for the
            // auto create folder name may have changed.
            _project.FolderStructureWasModified();

            DialogResult = true;
        }

        private bool IsFoldernameValid(string foldername)
        {
            char[] reserved = Path.GetInvalidFileNameChars();

            foreach (char c in reserved)
            {
                if (foldername.Contains(c))
                    return false;
            }

            return true;
        }

        private void btnExcludeAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ViewModel.List)
                item.Exclude = true;
        }

        private void btnExcludeNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var item in ViewModel.List)
                item.Exclude = false;
        }

        private void btnAdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = (MainWindow)Application.Current.MainWindow;
            window.DoOpenAdvancedProviderSettings(AdvancedWindow.OpenWithTab.ListAllTables);

            ViewModel.List.CollectListOfTables(_project);

            lvTables.ItemsSource = ViewModel.List;
        }

        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            ViewModel.Folder = "VenturaAutoCreate";
        }

        private void Error(string message)
        {
            MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
        }


        public class ViewModelClass : ViewModelBase
        {
            private bool _enabled;
            private string _folder = "";
            private bool _create_get_all;
            private bool _create_incremental;

            private TableList _list = new TableList();

            public bool Enabled
            {
                get { return _enabled; }
                set
                {
                    if (_enabled == value)
                        return;

                    _enabled = value;

                    NotifyPropertyChanged("Enabled");
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
                }
            }

            public TableList List
            {
                get
                {
                    return _list;
                }
            }

        } // end of viewmodel

    }
}
