using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using VenturaSQLStudio.Ado;
using VenturaSQLStudio.Pages;
using VenturaSQLStudio.Pages.RecordsetEditorPage;
using VenturaSQLStudio.Progress;
using VenturaSQLStudio.Validation;
using VenturaSQLStudio.Validation.Validators;

namespace VenturaSQLStudio {
    /// <summary>
    /// Interaction logic for RecordsetEditorPage.xaml
    /// </summary>
    public partial class RecordsetEditorPage : UserControl
    {
        private RecordsetItem _recordsetitem;

        //Views
        private SqlScriptEditorControl _sqlScriptEditorControl;
        private RecordsetSettingsControl _recordsetSettingsControl;
        private UserDefinedColumnsControl _userDefinedColumnsControl;

        private Project _project;

        public RecordsetEditorPage(Project project, RecordsetItem recordsetitem)
        {
            _project = project;
            _recordsetitem = recordsetitem;

            // Preload views
            _sqlScriptEditorControl = new SqlScriptEditorControl(project, recordsetitem);
            _recordsetSettingsControl = new RecordsetSettingsControl(recordsetitem);
            _userDefinedColumnsControl = new UserDefinedColumnsControl(project, recordsetitem);

            InitializeComponent();

            this.DataContext = recordsetitem;

            // Select the initial view
            contentView.Content = _sqlScriptEditorControl;

            MarkButtonAsSelected((Button)panelWrap.Children[0]);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
        }

        private void Page_Unloaded(object sender, RoutedEventArgs e)
        {
            //General.ClickSound();
        }

        private bool ValidateMe(RecordsetValidator.RecordsetValidationMode validationmode)
        {
            ValidationEngine engine = new ValidationEngine();
            bool engineresult = false;

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                engine.AddValidator(new ProjectSqlConnectionValidator(MainWindow.ViewModel.CurrentProject));
                engine.AddValidator(new RecordsetValidator(_project ,_recordsetitem, validationmode));

                engineresult = engine.Exec();
            };

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Validating Recordset...", action);

            if (result.Error != null)
                MessageBox.Show("VALIDATION ENGINE FAILURE. " + result.Error.Message);

            if (engineresult == false)
            {
                //System.Media.SystemSounds.Asterisk.Play();
                engine.ShowMessagesWindow();
                return false;
            }

            return true;
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

        private void btnSql_Click(object sender, RoutedEventArgs e)
        {
            MarkButtonAsSelected((Button)sender);

            contentView.Content = _sqlScriptEditorControl;
        }

        private void btnSettings_Click(object sender, RoutedEventArgs e)
        {
            MarkButtonAsSelected((Button)sender);
            contentView.Content = _recordsetSettingsControl;
        }

        private void btnUDC_Click(object sender, RoutedEventArgs e)
        {
            MarkButtonAsSelected((Button)sender);
            contentView.Content = _userDefinedColumnsControl;
        }

        private void btnRunQuery_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateMe(RecordsetValidator.RecordsetValidationMode.SqlScriptNotEmptyOnly) == false)
                return;

            QueryInfo query_info = QueryInfo.CreateInstance(_recordsetitem);

            if (query_info.ResultSets.Count == 0) // The RecordsetValidator updated RecordsetItem.QueryInfo
            {
                MessageBox.Show("The SQL script returns no result sets.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MainWindow window = (MainWindow)Application.Current.MainWindow;

            window.AddTab(() => new RunQueryPage(_recordsetitem.SqlScript, _recordsetitem.Parameters), $"Run - {_recordsetitem.ClassName}", null, this.DataContext, TabMenu.CloseAble);
        }

        private void btnPreviewCode_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateMe(RecordsetValidator.RecordsetValidationMode.Full) == false)
                return;

            MainWindow window = (MainWindow)Application.Current.MainWindow;

            window.AddTab(() => new RecordsetPreviewPage(_recordsetitem), $"Preview {_recordsetitem.ClassName}", null, this.DataContext, TabMenu.CloseAble);
        }

        private void btnRawSchema_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateMe(RecordsetValidator.RecordsetValidationMode.SqlScriptNotEmptyOnly) == false)
                return;

            QueryInfo query_info = QueryInfo.CreateInstance(_recordsetitem);

            if (query_info.ResultSets.Count == 0) 
            {
                MessageBox.Show("The SQL script returns no result sets.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MainWindow window = (MainWindow)Application.Current.MainWindow;

            window.AddTab(() => new RawSchemaPage(_recordsetitem), $"Raw schema {_recordsetitem.ClassName}", null, this.DataContext, TabMenu.CloseAble);
        }

        private void btnCodeSnippets_Click(object sender, RoutedEventArgs e)
        {
            if (ValidateMe(RecordsetValidator.RecordsetValidationMode.Full) == false)
                return;

            QueryInfo query_info = QueryInfo.CreateInstance(_recordsetitem);

            if (query_info.ResultSets.Count == 0)
            {
                MessageBox.Show("The SQL script returns no result sets.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            MainWindow window = (MainWindow)Application.Current.MainWindow;

            window.AddTab(() => new CodeSnippetsPage(_recordsetitem), $"Code Snippets {_recordsetitem.ClassName}", null, this.DataContext, TabMenu.CloseAble);
        }


    }
}
