using System.Text;
using System.Windows;
using System.Data;
using System.Windows.Input;
using System.Data.Common;
using System.IO;
using System.Windows.Controls;
using System.Windows.Media;
using System.Collections.ObjectModel;
using System.ComponentModel;
using VenturaSQLStudio.Progress;
using VenturaSQL;
using System;

namespace VenturaSQLStudio.Pages
{
    public partial class AdvancedWindow : Window, INotifyPropertyChanged
    {
        public AdvancedSettings ViewModel { get; }

        private Project _project;

        private ObservableCollection<ListItem> _column_list = new ObservableCollection<ListItem>();

        private bool _advanced_settings_modified = false;

        public enum OpenWithTab
        {
            Welcome,
            QualifyTableName,
            ListAllTables
        }


        public AdvancedWindow(Project project, OpenWithTab tab_to_open = OpenWithTab.Welcome)
        {
            _project = project;

            ViewModel = _project.AdvancedSettings;

            _project.AdvancedSettings.PropertyChanged += AdvancedSettings_PropertyChanged;

            this.DataContext = this;

            this.Width = 900;
            this.Height = 500;

            InitializeComponent();

            sectionWelcome.Visibility = Visibility.Collapsed;
            sectionQualifyTableName.Visibility = Visibility.Collapsed;
            sectionListAllTables.Visibility = Visibility.Collapsed;

            if(tab_to_open == OpenWithTab.Welcome)
            {
                sectionWelcome.Visibility = Visibility.Visible;
                MarkButtonAsSelected(btnWelcome);
            }
            else if (tab_to_open == OpenWithTab.QualifyTableName)
            {
                sectionQualifyTableName.Visibility = Visibility.Visible;
                MarkButtonAsSelected(btnQualify);
            }
            else if (tab_to_open == OpenWithTab.ListAllTables)
            {
                sectionListAllTables.Visibility = Visibility.Visible;
                MarkButtonAsSelected(btnTableList);
            }

            this.ContentRendered += Window_ContentRendered;
            this.Closed += AdvancedWindow_Closed;
        }

        private void AdvancedSettings_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            _advanced_settings_modified = true;
        }

        private void Window_ContentRendered(object sender, System.EventArgs e)
        {
            this.ContentRendered -= Window_ContentRendered; // just in case the event repeats

            ProgressDialogResult result = ProgressDialog.Execute(this, "Retrieving List of Tables...", (bw, we) =>
            {
                CollectListOfTables();
            });

            if (result.Error != null)
            {
                MessageBox.Show("Connecting to database and retrieving list of all tables failed.\n\n" + result.Error.Message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Close();
                return;
            }

        }

        private void AdvancedWindow_Closed(object sender, System.EventArgs e)
        {
            // Update the loaded Recordsets. 
            if (_advanced_settings_modified)
                _project.ProviderRelatedSettingsWereModified();
        }

        // This is like 'change tab'
        private void btnWelcome_Click(object sender, RoutedEventArgs e)
        {
            MarkButtonAsSelected(btnWelcome);
            sectionWelcome.Visibility = Visibility.Visible;
            sectionQualifyTableName.Visibility = Visibility.Collapsed;
            sectionListAllTables.Visibility = Visibility.Collapsed;
        }


        private void btnQualify_Click(object sender, RoutedEventArgs e)
        {
            MarkButtonAsSelected(btnQualify);
            sectionWelcome.Visibility = Visibility.Collapsed;
            sectionQualifyTableName.Visibility = Visibility.Visible;
            sectionListAllTables.Visibility = Visibility.Collapsed;
        }

        // This is like 'change tab'
        private void btnTableList_Click(object sender, RoutedEventArgs e)
        {
            MarkButtonAsSelected(btnTableList);
            sectionWelcome.Visibility = Visibility.Collapsed;
            sectionQualifyTableName.Visibility = Visibility.Collapsed;
            sectionListAllTables.Visibility = Visibility.Visible;
        }

        private void MarkButtonAsSelected(Button selectbutton)
        {
            for (int i = 0; i < panelWrap.Children.Count; i++)
            {
                Button button = panelWrap.Children[i] as Button;

                if (button.Equals(selectbutton))
                    button.Background = Brushes.SkyBlue;
                else
                    button.ClearValue(Button.BackgroundProperty);
            }
        }

        private void btnDefault_Click(object sender, RoutedEventArgs e)
        {
            string message = "Reset 'Qualify Table Name' and 'List All Tables' values to default?";

            MessageBoxResult result = MessageBox.Show(message, "VenturaSQL Studio", MessageBoxButton.OKCancel, MessageBoxImage.Exclamation, MessageBoxResult.OK);

            if (result != MessageBoxResult.OK)
                return;

            ViewModel.ResetToDefault();
        }

        private void CollectListOfTables()
        {
            AdoConnector connector = AdoConnectorHelper.Create(_project.ProviderInvariantName, _project.MacroConnectionString);

            DataTable data_table;

            using (DbConnection connection = connector.OpenConnection())
            {
                data_table = connection.GetSchemaExtension("Tables");
            }

            //using (MemoryStream ms = new MemoryStream())
            //{
            //    data_table.WriteXml(ms);
            //    //ms.Position = 0;
            //    string readable = Encoding.ASCII.GetString(ms.ToArray());
            //}

            Action action = () =>
            {

                lvTables.ItemsSource = data_table.DefaultView;

                _column_list.Clear();

                ListItem li = new ListItem();
                li.Display = "(map automatically)";
                li.Data = "";
                _column_list.Add(li);

                for (int i = 0; i < data_table.Columns.Count; i++)
                {
                    li = new ListItem();
                    li.Display = data_table.Columns[i].ColumnName;
                    li.Data = data_table.Columns[i].ColumnName;
                    _column_list.Add(li);
                }

                NotifyPropertyChanged("ColumnList");
            };

            Application.Current.Dispatcher.Invoke(action);

        }

        private void lvTables_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            TextBlock tb = new TextBlock();
            tb.Text = e.PropertyName;

            // We want the unfiltered name in the header. Otherwise the underscores will be gone.
            e.Column.Header = tb;
        }

        public ObservableCollection<ListItem> ColumnList
        {
            get { return _column_list; }
        }

        private void CreateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CreateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            // This makes sure the changed data in a focussed control is committed to the ViewModel
            FocusManager.SetFocusedElement(this, null);

            DialogResult = true;
        }

        private void Error(string message)
        {
            MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(string property_name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property_name));
        }

        public class ListItem
        {
            public string Display { get; set; }
            public string Data { get; set; }
        }

    }
}