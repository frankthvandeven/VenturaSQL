using ICSharpCode.AvalonEdit.Search;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VenturaSQLStudio.AutoCreate;
using VenturaSQLStudio.Progress;
using VenturaSQLStudio.ProjectActions;
using VenturaSQLStudio.Validation;
using VenturaSQLStudio.Validation.Validators;

namespace VenturaSQLStudio.Pages
{
    /// <summary>
    /// Interaction logic for GeneratePage.xaml
    /// </summary>
    public partial class GeneratePage : UserControl
    {
        private Project _project;
        private GridLength _original1;
        private GridLength _original2;
        private double _original3;

        public GeneratePage(Project project)
        {
            _project = project;

            this.DataContext = _project;

            InitializeComponent();

            AvalonEditControl.Options.EnableHyperlinks = false;

            SearchPanel.Install(AvalonEditControl);

            _original1 = splitterColumn.Height;
            _original2 = listviewColumn.Height;
            _original3 = listviewColumn.MinHeight;

            HideListview();

            
        }

        private void HideListview()
        {
            splitterColumn.Height = new GridLength(0);
            listviewColumn.MinHeight = 0;
            listviewColumn.Height = new GridLength(0);
        }

        private void UnHideListview()
        {
            splitterColumn.Height = _original1;
            listviewColumn.Height = _original2;
            listviewColumn.MinHeight = _original3;
        }

        public void DoGenerate()
        {
            DateTime timestamp = DateTime.Now;

            HideListview();

            AvalonEditControl.Clear();
            listView.ItemsSource = null;
            btnSaveAs.IsEnabled = false;

            AppendTextToEditor($"Generator started on {timestamp.ToLongDateString()} at {timestamp.ToLongTimeString()}.");

            if (_project.AutoCreateSettings.Enabled == true)
            {
                AppendTextToEditor("Running Auto Create recordsets.");

                RunAutoCreate run = new RunAutoCreate(_project);
                bool success = run.Exec(false);

                if (success == false)
                {
                    AppendTextToEditor("Auto Create recordsets failed.");
                    return;
                }

                AppendTextToEditor("Auto Create recordsets complete.");
            }

            ValidationEngine engine = new ValidationEngine();

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                engine.AddValidator(new ProjectSettingsValidator(_project));
                engine.AddValidator(new ProjectSqlConnectionValidator(_project));

                List<ITreeViewItem> projectitemlist = _project.FolderStructure.AllProjectItemsInThisFolderAndSubfolders();

                foreach (ITreeViewItem projectitem in projectitemlist)
                {
                    RecordsetItem recordsetitem = projectitem as RecordsetItem;

                    if (recordsetitem != null && recordsetitem.Enabled == true)
                        engine.AddValidator(new RecordsetValidator(_project, recordsetitem, RecordsetValidator.RecordsetValidationMode.Full));
                }

                bool validated = false;

                AppendTextToEditor($"Validating project.");

                validated = engine.Exec();

                if (validated == true)
                {
                    AppendTextToEditor("Validation success.");
                    AppendTextToEditor("Starting code generation.");

                    GenerateCode generatecode = new GenerateCode(_project);
                    generatecode.LogOutputEvent += Generatecode_LogOutputEvent;

                    generatecode.Exec(timestamp);

                    if (generatecode.Exception == null)
                    {
                        AppendTextToEditor("");

                        if (generatecode.FilesWritten > 0)
                            AppendTextToEditor($"Code generation succeeded, {generatecode.FilesWritten} Recordset source code (.cs) files were written and no problems were encountered while doing so.");
                        else
                            AppendTextToEditor("Nothing to do.");
                    }

                }
                else
                {
                    AppendTextToEditor($"Validation failed with {engine.ErrorCount} error(s) and {engine.WarningCount} warning(s).");
                    AppendTextToEditor("Code generation cannot start. Resolve validation errors first.");
                }
            };

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Generating code...", action);

            if (result.Error != null)
                MessageBox.Show("Generating failed. " + result.Error.Message);

            listView.ItemsSource = engine.ValidationMessages;

            if (engine.ValidationMessages.Count > 0)
                UnHideListview();

            btnSaveAs.IsEnabled = true;

        }

        private const string CRLF = "\r\n";
        private const string TAB = "\t";
        private const string QUOTE = "\"";

        private void Generatecode_LogOutputEvent(string text)
        {
            AppendTextToEditor(text);
        }

        private void AppendTextToEditor(string text)
        {
            Action action = () =>
            {
                if (AvalonEditControl.Text.Length > 0)
                    AvalonEditControl.AppendText(CRLF);

                AvalonEditControl.AppendText(text);

                // Move the caret to the end.
                AvalonEditControl.CaretOffset = AvalonEditControl.Text.Length;
                // Make sure the caret is visible.
                AvalonEditControl.TextArea.Caret.Show();
                // By bringing the caret into view we bring the last line of the text into view.
                AvalonEditControl.TextArea.Caret.BringCaretToView();
            };

            Dispatcher.Invoke(action);
        }

        private void ListViewItem_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            ListViewItem lvi = (ListViewItem)sender;
            ValidationListItem val = (ValidationListItem)lvi.DataContext;

            if( val.Group == "Recordset")
            {
                MainWindow window = (MainWindow)Application.Current.MainWindow;
                window._project_items_control.OpenEditRecordsetTab((RecordsetItem)val.RefersToObject);
            }

        }


        private void listView_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            GridView gridView = listView.View as GridView;

            var actualWidth = listView.ActualWidth - SystemParameters.VerticalScrollBarWidth;

            for (Int32 i = 0; i < (gridView.Columns.Count - 1); i++)
            {
                actualWidth = actualWidth - gridView.Columns[i].ActualWidth;
            }

            if (actualWidth < 200)
                actualWidth = 200;

            gridView.Columns[gridView.Columns.Count - 1].Width = actualWidth;
        }

        private void btnClearAll_Click(object sender, RoutedEventArgs e)
        {
            HideListview();

            AvalonEditControl.Clear();

            // Make sure the caret is visible.
            AvalonEditControl.TextArea.Caret.Show();

            listView.ItemsSource = null;

            btnSaveAs.IsEnabled = false;
        }

        private void btnSaveAs_Click(object sender, RoutedEventArgs e)
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.Filter = "Text Files (*.txt)|*.txt";
            bool? result = dialog.ShowDialog(App.Current.MainWindow);

            if (result == null)
                MessageBox.Show("null");

            if (result == true)
                File.WriteAllText(dialog.FileName, AvalonEditControl.Text);
        }


    }
}
