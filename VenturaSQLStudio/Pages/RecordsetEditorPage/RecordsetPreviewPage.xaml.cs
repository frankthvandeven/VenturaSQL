using ICSharpCode.AvalonEdit.Search;
using System;
using System.ComponentModel;
using System.Data.Common;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using VenturaSQL;
using VenturaSQLStudio.Ado;
using VenturaSQLStudio.Progress;

namespace VenturaSQLStudio.Pages
{

    /// <summary>
    /// Interaction logic for RecordsetPreviewPage.xaml
    /// </summary>
    public partial class RecordsetPreviewPage : UserControl
    {
        private RecordsetItem _recordsetitem;
        private string _generatedcode_YesDirectSql;
        private string _generatedcode_NoDirectSql;

        private const string YESDIRECTSQLINFO = @"This version of the Recordset can connect with ADO.NET directly, " +
            "and can also retrieve and update data through the ASP.NET middle-tier server. " +
            "For safety reasons, you would not deploy this Recordset outside the trusted environment " +
            "of your organisation, for it contains SQL script.";

        private const string NODIRECTSQLINFO = @"In this version of the Recordset the SQL script is " +
            "left out. This Recordset needs the ASP.NET middle-tier to retrieve and update data. " +
            "This Recordset can safely be deployed anywhere.";

        public RecordsetPreviewPage(RecordsetItem recordsetitem)
        {
            _recordsetitem = recordsetitem;

            InitializeComponent();

            AvalonEditControl.Options.EnableHyperlinks = false;

            SearchPanel.Install(AvalonEditControl);

        }

        private bool _isloaded = false;

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isloaded == true)
                return;

            _isloaded = true;

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) => DoWork();

            ProgressDialogResult result1 = ProgressDialog.Execute(Application.Current.MainWindow, $"Generating recordset...", action);

            if (result1.Error != null)
            {
                MessageBox.Show("Generating recordset failed. " + result1.Error.Message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);

                MainWindow window = (MainWindow)Application.Current.MainWindow;
                window.CloseTabContainingPage(this);

                return;
            }

            AvalonEditControl.Text = _generatedcode_YesDirectSql;
            textblockInfo.Text = YESDIRECTSQLINFO;
            MarkButtonAsSelected((Button)panelWrap.Children[0]);
        }

        /// <summary>
        /// Running on a separate thread... don't try to update the GUI from within this function.
        /// </summary>
        private void DoWork()
        {
            AdoConnector connector = AdoConnectorHelper.Create(MainWindow.ViewModel.CurrentProject.ProviderInvariantName, MainWindow.ViewModel.CurrentProject.MacroConnectionString);

            using (DbConnection connection = connector.OpenConnection())
            { 
                MasterTemplate template = new MasterTemplate(connection, _recordsetitem, DateTime.Now);

                template.PreGenerate();

                _generatedcode_YesDirectSql = template.GenerateCSharp("PreviewGenerate", true, VenturaPlatform.NETStandard);
                _generatedcode_NoDirectSql = template.GenerateCSharp("PreviewGenerateWithoutADO", false, VenturaPlatform.NETStandard);

                template.PostGenerate();
            }
        }

        private void btnYesLocalSqlCode_Click(object sender, RoutedEventArgs e)
        {
            MarkButtonAsSelected((Button)sender);

            //AvalonEditControl.Text = null;
            AvalonEditControl.Text = _generatedcode_YesDirectSql;
            textblockInfo.Text = YESDIRECTSQLINFO;
        }

        private void btnNoLocalSqlCode_Click(object sender, RoutedEventArgs e)
        {
            MarkButtonAsSelected((Button)sender);

            //AvalonEditControl.Text = null;
            AvalonEditControl.Text = _generatedcode_NoDirectSql;
            textblockInfo.Text = NODIRECTSQLINFO;
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
    }
}
