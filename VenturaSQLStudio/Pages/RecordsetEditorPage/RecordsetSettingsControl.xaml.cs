using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using VenturaSQLStudio.Ado;
using VenturaSQLStudio.Progress;

namespace VenturaSQLStudio.Pages.RecordsetEditorPage
{
    /// <summary>
    /// Interaction logic for RecordsetSettingsControl.xaml
    /// </summary>
    public partial class RecordsetSettingsControl : UserControl
    {
        private RecordsetItem _recordset_item;

        public RecordsetSettingsControl(RecordsetItem recordset_item)
        {
            _recordset_item = recordset_item;
                       
            InitializeComponent();

            this.DataContext = _recordset_item;

            this.icOutputProjects.ItemsSource = _recordset_item.OutputProjects;

            _recordset_item.Resultsets.CollectionChanged += Resultsets_CollectionChanged;

            ResetVisibility();
        }

        private void Resultsets_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            ResetVisibility();
        }

        private void ResetVisibility()
        {

            if (_recordset_item.Resultsets.Count == 0)
            {
                sectionNoDefinitions.Visibility = Visibility.Visible;
                sectionMultipleDefinitions.Visibility = Visibility.Collapsed;
            }
            else
            {
                sectionNoDefinitions.Visibility = Visibility.Collapsed;
                sectionMultipleDefinitions.Visibility = Visibility.Visible;
            }
        }

        private void TextBoxResultsetName_LostFocus(object sender, RoutedEventArgs e)
        {
            TextBox textbox = (TextBox)sender;
            ResultsetItem resultset_item = (ResultsetItem)textbox.DataContext;

            string origvalue = textbox.Text;

            // Trim the leading and trailing spaces.
            string newvalue = origvalue.Trim();

            // Remove spaces
            newvalue = newvalue.Replace(" ", "");

            if (newvalue.ToLower().StartsWith("resultset") && (newvalue.Length > "resultset".Length))
                newvalue = StudioGeneral.MakeSureStringStartsWith(newvalue, "Resultset"); // make sure it is the correct case
            else
            {
                if (newvalue.Length == 0 || newvalue.ToLower().Contains("resultset")) // if the name contains the word "resultset" the name is a mess. Replace with default.
                    newvalue = resultset_item.GetDefaultName();
                else
                    newvalue = "Resultset" + newvalue;
            }

            if (origvalue != newvalue)
                textbox.Text = newvalue;
        }

        private void CleanupCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (_recordset_item.Resultsets.DisabledCount() > 0)
                e.CanExecute = true;
        }

        private void CleanupCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            _recordset_item.Resultsets.RealDeleteDisabled();
        }

        private void DefaultCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            if (_recordset_item.Resultsets.Count > 0)
                e.CanExecute = true;
        }

        private void DefaultCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            _recordset_item.Resultsets.ResetResultsetNames();
        }

        private void CollectCommand_CanExecute(object sender, System.Windows.Input.CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CollectCommand_Executed(object sender, System.Windows.Input.ExecutedRoutedEventArgs e)
        {
            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                QueryInfo q = QueryInfo.CreateInstance(_recordset_item);

                // Align the resultset definition list and fill the table lists
                _recordset_item.Resultsets.SetResultsetsLength(q);
            };

            ProgressDialogResult result1 = ProgressDialog.Execute(Application.Current.MainWindow, "Executing SQL script...", action);

            if (result1.Error != null)
            {
                MessageBox.Show("Retrieving resultset info failed. " + result1.Error.Message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

        }
    }

}
