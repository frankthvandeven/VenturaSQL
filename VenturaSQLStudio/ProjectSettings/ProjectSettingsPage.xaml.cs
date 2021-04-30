using System;
using System.Data.Common;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using VenturaSQL;
using VenturaSQLStudio.Progress;

namespace VenturaSQLStudio.Pages
{
    /// <summary>
    /// Interaction logic for ProjectSettingsPage.xaml
    /// </summary>
    public partial class ProjectSettingsPage : UserControl
    {

        private Project _project;

        public ProjectSettingsPage(Project project)
        {
            _project = project;

            this.DataContext = project;

            InitializeComponent();

            MainWindow.ViewModel.PropertyChanged += MainViewModel_PropertyChanged;

            this.icVisualStudioProjects.ItemsSource = project.VisualStudioProjects;

            RefreshRemark();
        }

        private void MainViewModel_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != "FileName")
                return;

            RefreshRemark();
        }

        private void RefreshRemark()
        {
            runPojectFolder.Text = Path.GetDirectoryName(MainWindow.ViewModel.FileName);
        }

        private void buttonTestConnectString_Click(object sender, RoutedEventArgs e)
        {
            string connectstring = textboxConnectionString.Text;

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Testing Sql connection...", (bw, we) =>
            {
                AdoConnector connector = AdoConnectorHelper.Create(MainWindow.ViewModel.CurrentProject.ProviderInvariantName, MainWindow.ViewModel.CurrentProject.MacroConnectionString);
                DbConnection connection = connector.OpenConnection();
                connection.Close();
            }
            );

            if (result.Error == null)
                MessageBox.Show("The connect string is OK.", "Testing Sql connection", MessageBoxButton.OK, MessageBoxImage.Exclamation);
            else
                MessageBox.Show(result.Error.Message, "The connect string failed.", MessageBoxButton.OK, MessageBoxImage.Error);

        }

        private void buttonClipboard_Click(object sender, RoutedEventArgs e)
        {
            string connectstring = textboxConnectionString.Text;

            Clipboard.SetText(connectstring);
        }

        private void buttonChangeProvider_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = (MainWindow)Application.Current.MainWindow;

            window.DoOpenSelectProvider();
        }

        private void buttonAdvancedSettings_Click(object sender, RoutedEventArgs e)
        {
            MainWindow window = (MainWindow)Application.Current.MainWindow;

            window.DoOpenAdvancedProviderSettings();
        }

        private void buttonAutoCreateSettings_Click(object sender, RoutedEventArgs e)
        {
            AutoCreateSettingsWindow window = new AutoCreateSettingsWindow(_project);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        private void buttonConnectorCodeClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(textboxConnectorCode.Text);
        }
    }
}
