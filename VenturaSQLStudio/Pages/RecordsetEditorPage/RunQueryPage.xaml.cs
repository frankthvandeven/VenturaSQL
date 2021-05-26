using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using VenturaSQL;
using VenturaSQLStudio.Ado;
using VenturaSQLStudio.Progress;

namespace VenturaSQLStudio.Pages
{

    /// <summary>
    /// Interaction logic for RunQueryPage.xaml
    /// </summary>
    public partial class RunQueryPage : UserControl
    {
        private string _sqlscript;
        private ParameterCollection _parameters;

        private const int MAXROWS = 500;

        // The list for the combobox.
        private List<string> _resultsetnames = new List<string>();

        // The list containing the data for each resultset.
        private List<DataTable> _datatable_list = new List<DataTable>();

        public RunQueryPage(string sqlscript, ParameterCollection parameters)
        {
            _sqlscript = sqlscript;
            _parameters = parameters;

            InitializeComponent();
        }

        private bool _isloaded = false;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isloaded == true)
            {
                return;
            }

            _isloaded = true;

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) => DoWork();

            string header = $"Running Query...";

            ProgressDialogResult result1 = ProgressDialog.Execute(Application.Current.MainWindow, header, action);

            if (result1.Error != null)
            {
                MessageBox.Show("Running query failed. " + result1.Error.Message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);

                MainWindow window = (MainWindow)Application.Current.MainWindow;
                window.CloseTabContainingPage(this);
                return;
            }

            // Hide the combobox if less than 2 items.
            if (_resultsetnames.Count < 2)
                stackpanelSelectResultset.Visibility = Visibility.Collapsed;

            cbResultsets.ItemsSource = _resultsetnames;
            cbResultsets.SelectionChanged += CbResultsets_SelectionChanged;
            cbResultsets.SelectedIndex = 0;
        }

        // Called when another item in the list of resultsets is selected.
        private void CbResultsets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            DataTable table = _datatable_list[cbResultsets.SelectedIndex];

            dataGrid.ItemsSource = table.DefaultView;

            if (table.Rows.Count == MAXROWS)
            {
                textblockInfo.Text = $"The number of rows retrieved has been limited to {MAXROWS}.";
                textblockInfo.Visibility = Visibility.Visible;
            }
            else if (table.Rows.Count == 0)
            {
                textblockInfo.Text = $"The query returned no rows.";
                textblockInfo.Visibility = Visibility.Visible;
            }
            else
                textblockInfo.Visibility = Visibility.Collapsed;

        }

        /// <summary>
        /// Running on a separate thread... don't try to update the GUI from within this function.
        /// </summary>
        private void DoWork()
        {
            AdoConnector connector = AdoConnectorHelper.Create(MainWindow.ViewModel.CurrentProject.ProviderInvariantName, MainWindow.ViewModel.CurrentProject.MacroConnectionString);

            using (DbConnection connection = connector.OpenConnection())
            {
                using (DbTransaction transaction = connection.BeginTransaction())
                {
                    ExecuteReader(connector, connection, transaction);
                    transaction.Rollback();
                }
            }

        }

        private void ExecuteReader(AdoConnector connector, DbConnection connection, DbTransaction transaction)
        {
            DbCommand command = connector.CreateCommand(_sqlscript, connection, transaction);

            // Start with the automatic parameters.

            DbParameter parameter = connector.CreateParameter(connector.ParameterPrefix + "DesignMode", true);
            command.Parameters.Add(parameter);

            parameter = connector.CreateParameter(connector.ParameterPrefix + "RowOffset", 0);
            command.Parameters.Add(parameter);

            parameter = connector.CreateParameter(connector.ParameterPrefix + "RowLimit", 500);
            command.Parameters.Add(parameter);

            foreach (ParameterItem parameter_item in _parameters)
            {
                parameter = parameter_item.CreateDesignValueDbParameter(connector);
            }

            CommandBehavior behavior = CommandBehavior.KeyInfo; // We need both the schema and datarows, so omit the 'CommandBehavior.SchemaOnly' flag!

            // Microsoft.Data.Sqlite does not support regular flags.
            if (command is Microsoft.Data.Sqlite.SqliteCommand)
                behavior = 0;

            // Build the list of resultsets and collect the data.
            // We assume the at least 1 resultset will be returned by the SqlDataReader.
            using (DbDataReader sqldatareader = command.ExecuteReader(behavior))
            {
                int resultsetcount = 0;

                while (true) // Resultset loop
                {
                    DataTable current_table = CreateDataTableFromSchema(sqldatareader);

                    _datatable_list.Add(current_table);
                    _resultsetnames.Add($"Resultset {resultsetcount + 1}");

                    resultsetcount++;

                    int rowCount = 0;

                    while (sqldatareader.Read())
                    {
                        rowCount++;

                        DataRow display_row = current_table.NewRow();
                        for (int i = 0; i < current_table.Columns.Count; i++)
                        {
                            object column_data;

                            try
                            {
                                column_data = sqldatareader[i];
                                //column_data = sqldatareader.GetDataTypeName(i); //TEST
                            }
                            catch
                            {
                                column_data = sqldatareader.GetDataTypeName(i);
                            }

                            if (column_data is DBNull)
                            {
                                display_row[i] = "(null)";
                            }
                            else if (column_data is string)
                            {
                                string temp = (string)column_data;

                                if (temp.Length > 30)
                                    display_row[i] = temp.Substring(0, 30) + "...";
                                else
                                    display_row[i] = temp;
                            }
                            else if (column_data is Byte[])
                            {
                                const int BYTE_LIMIT = 15; // each byte will display as hex
                                byte[] byte_array = AdoConnector.GetBytes(sqldatareader, i, BYTE_LIMIT);

                                if (byte_array.Length == BYTE_LIMIT)
                                    display_row[i] = Bytes2String(byte_array) + "...";
                                else
                                    display_row[i] = Bytes2String(byte_array);

                            }
                            else
                            {
                                display_row[i] = column_data;
                            }


                        }

                        current_table.Rows.Add(display_row);

                        if (rowCount == MAXROWS)
                            break;

                    } // end row loop

                    if (sqldatareader.NextResult() == false)
                        break;
                }

                // Call Close when done reading.
                sqldatareader.Close();
            } // End of Using.
        }


        private DataTable CreateDataTableFromSchema(DbDataReader datareader)
        {
            DataTable schema_table = datareader.GetSchemaTable();

            // After retrieving the Schema with GetSchemaTable, you MUST remove rows where IsHidden is set to true.
            QueryInfoTools.RemoveIsHiddenRowsFromSchemaTable(schema_table);

            DataTable table = new DataTable();

            for (int i = 0; i < schema_table.Rows.Count; i++)
            {
                DataRow schema_column = schema_table.Rows[i];

                DataColumn datacolumn = new DataColumn();

                datacolumn.ColumnName = (string)schema_column["ColumnName"];
                datacolumn.DataType = typeof(object);

                table.Columns.Add(datacolumn);
            }

            return table;
        }


        //0xE6100000010C68E2C226D73F3E406C62DC68DBA657C0 22
        private string Bytes2String(byte[] byte_array)
        {
            StringBuilder sb = new StringBuilder(60);

            sb.Append("0x");

            for (int i = 0; i < byte_array.Length; i++)
                sb.AppendFormat("{0:x}", byte_array[i]);

            return sb.ToString();

        }

    }
}
