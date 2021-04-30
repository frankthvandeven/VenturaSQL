using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Windows;
using System.Windows.Controls;
using VenturaSQLStudio.Ado;
using VenturaSQLStudio.Progress;

namespace VenturaSQLStudio.Pages
{
    /// <summary>
    /// Interaction logic for RawSchemaPage.xaml
    /// </summary>
    public partial class RawSchemaPage : UserControl
    {
        private RecordsetItem _recordsetitem;
        private QueryInfo _queryinfo;

        // The list for the combobox.
        private List<string> _resultsetnames = new List<string>();

        public RawSchemaPage(RecordsetItem recordsetitem)
        {
            _recordsetitem = recordsetitem;

            InitializeComponent();
        }

        private bool _isloaded = false;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isloaded == true)
                return;

            _isloaded = true;

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) => DoWork();

            ProgressDialogResult result1 = ProgressDialog.Execute(Application.Current.MainWindow, "Retrieving raw schema...", action);

            if (result1.Error != null)
            {
                MessageBox.Show("Retrieving raw schema failed. " + result1.Error.Message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);

                MainWindow window = (MainWindow)Application.Current.MainWindow;
                window.CloseTabContainingPage(this);

                return;
            }

            // Fill the list for the dropdown combobox.
            for (int i = 0; i < _queryinfo.ResultSets.Count; i++)
                _resultsetnames.Add($"Resultset {i + 1}");

            cbResultsets.ItemsSource = _resultsetnames;
            cbResultsets.SelectionChanged += CbResultsets_SelectionChanged;
            cbResultsets.SelectedIndex = 0;

            if (_queryinfo.ResultSets.Count < 2)
            {
                // One resultset.
                textblockInfo.Text = "The raw schema as returned by ADO.NET for the resultset of the executed SQL script.";
                textblockInfo.Visibility = Visibility.Visible;
                stackpanelSelectResultset.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Two or more resultsets.
                textblockInfo.Visibility = Visibility.Collapsed;
                stackpanelSelectResultset.Visibility = Visibility.Visible;
            }

        }

        // Called when another item in the list of resultsets is selected.
        private void CbResultsets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ResultSetInfo info = _queryinfo.ResultSets[cbResultsets.SelectedIndex];

            DataTable table = info.AdoSchemaTable;

            dataGrid.ItemsSource = table.DefaultView;
        }

        /// <summary>
        /// Running on a separate thread... don't try to update the GUI from within this function.
        /// </summary>
        private void DoWork()
        {
            _queryinfo = QueryInfo.CreateInstance(_recordsetitem);
        }

    }
}
