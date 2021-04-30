using System;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using VenturaSQLStudio.Progress;
using System.Windows.Input;
using System.Windows.Documents;
using System.Windows.Media;
using ICSharpCode.AvalonEdit;
using ICSharpCode.AvalonEdit.Search;
using VenturaSQLStudio.Ado;
using System.Windows.Threading;
using VenturaSQL;

namespace VenturaSQLStudio.Pages.RecordsetEditorPage
{
    public partial class SqlScriptEditorControl : UserControl
    {
        // Two-Way databinding for AvalonEdit's Text property:
        // http://stackoverflow.com/questions/18964176/two-way-binding-to-avalonedit-document-text-using-mvvm

        // Using AvalonEdit:
        // http://www.codeproject.com/Articles/42490/Using-AvalonEdit-WPF-Text-Editor

        private Project _project;

        private RecordsetItem _recordsetitem;
        private DispatcherTimer _dispatcherTimer = new DispatcherTimer();

        public SqlScriptEditorControl(Project project, RecordsetItem item)
        {
            _project = project;
            _recordsetitem = item;

            this.DataContext = item;

            InitializeComponent();

            item.Parameters.CollectionChanged += Parameters_CollectionChanged;

            AvalonEditControl.Options.EnableHyperlinks = false;

            SearchPanel.Install(AvalonEditControl);

            listView.ItemsSource = item.Parameters;
            listView.SelectionChanged += ListView_SelectionChanged;

            cbResultsets.SelectionChanged += CbResultsets_SelectionChanged;

            cbResultsets.ItemsSource = item.Resultsets;

            // Handy for debugging: get names of embedded resources
            //string[] resourceNames = System.Reflection.Assembly.GetExecutingAssembly().GetManifestResourceNames();

            // Load the Sql syntax highlighting definition from a resource file
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("VenturaSQLStudio.AvalonSql.xshd"))
            using (var reader = new System.Xml.XmlTextReader(stream))
                AvalonEditControl.SyntaxHighlighting = ICSharpCode.AvalonEdit.Highlighting.Xshd.HighlightingLoader.Load(reader, ICSharpCode.AvalonEdit.Highlighting.HighlightingManager.Instance);


            AvalonEditControl.Text = _recordsetitem.SqlScript; /* this will trigger the AvalonEditControl_TextChanged event */

            char parameter_prefix = MainWindow.ViewModel.CurrentProject.ParameterPrefix;
            menuItemInsertRowOffset.Header = $"Insert input parameter {parameter_prefix}RowOffset";
            menuItemInsertRowLimit.Header = $"Insert input parameter {parameter_prefix}RowLimit";
            menuItemInsertDesignMode.Header = $"Insert input parameter {parameter_prefix}DesignMode";

            _dispatcherTimer.Tick += _dispatcherTimer_Tick;
            _dispatcherTimer.Interval = new TimeSpan(0, 0, 0, 5, 0);

            // If there are resultset definitions, select the first one.
            if (item.Resultsets.Count > 0)
                cbResultsets.SelectedItem = item.Resultsets[0];

            UpdateMethodCallSample();

            if (_recordsetitem.IsInAutoCreateFolder)
                tbAutoCreateFolderTip.Visibility = Visibility.Visible;
            else
                tbAutoCreateFolderTip.Visibility = Visibility.Collapsed;

            _recordsetitem.PropertyChanged += _recordsetitem_PropertyChanged;
        }

        private void _recordsetitem_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "RowloadIncremental")
            {
                UpdateIncrementalTip();
            }
            else if (e.PropertyName == "FolderStructureWasModified")
            {
                if (_recordsetitem.IsInAutoCreateFolder)
                    tbAutoCreateFolderTip.Visibility = Visibility.Visible;
                else
                    tbAutoCreateFolderTip.Visibility = Visibility.Collapsed;
            }

        }

        private void _dispatcherTimer_Tick(object sender, EventArgs e)
        {
            // Not used at the moment
        }

        private void UpdateIncrementalTip()
        {
            string script = AvalonEditControl.Text;

            var pref = MainWindow.ViewModel.CurrentProject.ParameterPrefix;
            string par_offset = $"{pref}RowOffset";
            string par_limit = $"{pref}RowLimit";
            bool contains_offset = script.IndexOf($"{pref}RowOffset", System.StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool contains_limit = script.IndexOf($"{pref}RowLimit", System.StringComparison.InvariantCultureIgnoreCase) >= 0;
            bool params_detected = (contains_offset == true && contains_limit == true);

            if (_recordsetitem.RowloadIncremental == true && params_detected == false)
            {
                tbIncrementalTip.Visibility = Visibility.Visible;
                tbIncrementalTip.Text = $"Use the built-in {pref}RowOffset and {pref}RowLimit parameters to implement incremental row loading. " +
                    "For example: SELECT * FROM CUSTOMERS ORDER BY CustID LIMIT @RowLimit OFFSET @RowOffset";
            }
            else
            {
                tbIncrementalTip.Visibility = Visibility.Collapsed;
            }

        }


        private void ListView_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateMethodCallSample();
        }

        private void Parameters_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            UpdateMethodCallSample();
        }

        private void UpdateMethodCallSample()
        {
            ParameterCollection items = _recordsetitem.Parameters;

            InlineCollection lines = tbParameters.Inlines;
            lines.Clear();

            Run run = new Run("C#: ");
            run.FontWeight = FontWeights.Bold;
            lines.Add(run);

            lines.Add("Recordset.ExecSql(");

            int count = 0;

            foreach (var item in _recordsetitem.Parameters)
            {
                if (item.Input == true)
                {
                    if (count > 0)
                        lines.Add(", ");

                    TypeTools.TryConvertToCSharpTypeName(item.FullTypename, out string csharp_name);

                    run = new Run(csharp_name + " " + item.Name.Substring(1));

                    if (item == listView.SelectedItem)
                    {
                        run.Foreground = Brushes.Red;
                        run.FontWeight = FontWeights.Bold;
                    }

                    lines.Add(run);

                    count++;
                }
            }

            lines.Add((");"));
        }


        private void ResetResultsetAndTableComboboxes()
        {
            ResultsetCollection resultsets = _recordsetitem.Resultsets;

            if (resultsets.Count == 0)
            {
                // Disable the comboboxes, wich have empty lists.
                cbResultsets.IsEnabled = false;
                cbTables.IsEnabled = false;
            }
            else
            {
                // Enable the comboboxes.
                cbResultsets.IsEnabled = true;
                cbTables.IsEnabled = true;

                if (cbResultsets.SelectedIndex == -1)
                    cbResultsets.SelectedIndex = 0;
            }

        }

        private void CbResultsets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            cbTables.DataContext = cbResultsets.SelectedItem;
        }

        private void UserControl_Loaded(object sender, RoutedEventArgs e)
        {
            ResetResultsetAndTableComboboxes();
            AvalonEditControl.Focus();
        }

        private bool avalonhasfocus = false;

        private void AvalonEditControl_GotFocus(object sender, RoutedEventArgs e)
        {
            _dispatcherTimer.Start();
            avalonhasfocus = true;
        }

        private void AvalonEditControl_TextChanged(object sender, EventArgs e)
        {
            UpdateIncrementalTip();

            // We manually set the Project to modified, even though the SQL script in the project
            // has not been updated yet. It will be updated upon EditControl.Unfocus.

            if (avalonhasfocus == true)
                _project.SetModified();
        }

        private void AvalonEditControl_LostFocus(object sender, RoutedEventArgs e)
        {
            avalonhasfocus = false;
            _recordsetitem.SqlScript = AvalonEditControl.Text;
            _dispatcherTimer.Stop();
        }

        public void InsertText(string text)
        {
            TextEditor edit = AvalonEditControl;

            edit.Document.Insert(edit.TextArea.Caret.Offset, text);
            //edit.CaretOffset += text.Length

            AvalonEditControl.Focus();
        }

        private void buttonNew_Click(object sender, RoutedEventArgs e)
        {
            EditParameterWindow window = new EditParameterWindow(EditParameterWindow.Mode.New, _project, _recordsetitem, null);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();

            UpdateMethodCallSample();
        }

        private void ParameterListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            EditParameterWindow window = new EditParameterWindow(EditParameterWindow.Mode.Edit, _project, _recordsetitem, (ParameterItem)listView.SelectedItem);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();

            UpdateMethodCallSample();
        }

        private void buttonCollect_Click(object sender, RoutedEventArgs e)
        {
            //MessageBox.Show(_recordsetitem.SqlScript);

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                QueryInfo q = QueryInfo.CreateInstance(_recordsetitem);

                // Align the resultset definition list and fill the table lists
                _recordsetitem.Resultsets.SetResultsetsLength(q);
            };

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Executing SQL script...", action);

            if (result.Error != null)
            {
                string text = "Refreshing table list failed.\n\n" +
                    "Refreshing the table list means connecting to the database, executing the SQL script " +
                    "and retrieving the referenced table names from the resultset schema.\n\n" +
                    result.Error.Message;

                MessageBox.Show(App.Current.MainWindow, text, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            ResetResultsetAndTableComboboxes();
        }

        private void MoveUpCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (listView == null)
                return;

            if (listView.SelectedIndex > 0)
                e.CanExecute = true;
        }

        private void MoveUpCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var parameters = _recordsetitem.Parameters;
            var selectedIndex = listView.SelectedIndex;

            var itemToMoveUp = parameters[selectedIndex];
            parameters.RemoveAt(selectedIndex);
            parameters.Insert(selectedIndex - 1, itemToMoveUp);
            listView.SelectedIndex = selectedIndex - 1;
        }

        private void MoveDown_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (listView == null)
                return;

            if (listView.SelectedIndex == -1)
                return;

            if ((listView.SelectedIndex + 1) < _recordsetitem.Parameters.Count)
                e.CanExecute = true;
        }

        private void MoveDownCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var parameters = _recordsetitem.Parameters;
            var selectedIndex = listView.SelectedIndex;

            var itemToMoveDown = parameters[selectedIndex];
            parameters.RemoveAt(selectedIndex);
            parameters.Insert(selectedIndex + 1, itemToMoveDown);
            listView.SelectedIndex = selectedIndex + 1;
        }

        private void InsertParameterNameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (listView == null)
                return;

            if (listView.SelectedItem != null)
                e.CanExecute = true;
        }

        private void InsertParameterNameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ParameterItem item = (ParameterItem)listView.SelectedItem;

            string text = item.Name;

            InsertText(text);
        }

        private void DeleteParameterCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (listView != null && listView.SelectedItems.Count > 0)
                e.CanExecute = true;
        }

        private void DeleteParameterCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int count = listView.SelectedItems.Count;

            ParameterItem[] list = new ParameterItem[count];

            for (int i = 0; i < count; i++)
                list[i] = (ParameterItem)listView.SelectedItems[i];

            foreach (ParameterItem item in list)
                _recordsetitem.Parameters.Remove(item);
        }

        private void InsertRowOffset_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void InsertRowOffset_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            char parameter_prefix = MainWindow.ViewModel.CurrentProject.ParameterPrefix;
            InsertText($"{parameter_prefix}RowOffset");
        }

        private void InsertRowLimit_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void InsertRowLimit_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            char parameter_prefix = MainWindow.ViewModel.CurrentProject.ParameterPrefix;
            InsertText($"{parameter_prefix}RowLimit");
        }

        private void InsertDesignMode_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void InsertDesignMode_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            char parameter_prefix = MainWindow.ViewModel.CurrentProject.ParameterPrefix;
            InsertText($"{parameter_prefix}DesignMode");
        }
    }
}

#if DO_NOT_COMPILE
        /// <summary>
        /// Scans the SQL script in the TextBox for parameters, and adds the parameters
        /// to the Parameters Grid.
        /// </summary>
        private void buttonParse_Click(object sender, RoutedEventArgs e)
        {
            bool origmodified = _project.IsModified;

            ExtractParametersFromSqlScript extractor = new ExtractParametersFromSqlScript();
            Exception exec_exception = null;

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                try
                {
                    extractor.Exec(_recordsetitem.SqlScript /*AvalonEditControl.Text*/);
                }
                catch (Exception ex)
                {
                    exec_exception = ex;
                }
            };

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Parsing Sql script...", action);

            if (result.Error != null)
            {
                string text = "Parsing SQL script failed.\n\n" +
                    "The error message is:\n" +
                    result.Error.Message;

                MessageBox.Show(App.Current.MainWindow, text, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            else
            {
                textblockParameters.Text = _original_textblockParameters_text;

                if (exec_exception != null)
                    textblockParameters.Text = $"Parser aborted. Might be a quote character {exec_exception.Message}";

                if (extractor.Errors.Count > 0)
                    textblockParameters.Text = extractor.Errors[0];

                List<string> names = extractor.Variables;

                _recordsetitem.Parameters.UpdateCollection(names);
            }

            if (origmodified == true)
                _project.SetModified();
            else
                _project.ResetModified();

            // Autoselect extended properties for first parameter in the parameter list.
            if (parameterPropertiesControl.SelectedParameter == null && _recordsetitem.Parameters.Count > 0)
                parameterPropertiesControl.SelectedParameter = _recordsetitem.Parameters[0];

        }
#endif
