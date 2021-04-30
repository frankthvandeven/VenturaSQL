using Microsoft.Win32;
using System.IO;
using System.Windows;
using System.Windows.Controls;

namespace VenturaSQLStudio.UserControls
{
    /// <summary>
    /// Used in ProjectSettingsPage.xaml for setting Visual Studio output projects.
    /// </summary>
    public partial class VisualStudioProject : UserControl
    {
        private const string FILTER = "CSharp projects (*.csproj)|*.csproj";

        public VisualStudioProject()
        {
            InitializeComponent();
        }

        private void buttonSelectProject_Click(object sender, RoutedEventArgs e)
        {
            string base_path = Path.GetDirectoryName(MainWindow.ViewModel.FileName);

            string relative_file_path = textboxProject.Text.Trim();

            OpenFileDialog dialog = new OpenFileDialog();

            dialog.InitialDirectory = base_path;

            if (relative_file_path.Length > 0)
            {
                string absolute_path = StudioGeneral.GetAbsolutePath(base_path, relative_file_path);

                dialog.InitialDirectory = Path.GetDirectoryName(absolute_path);
                dialog.FileName = Path.GetFileName(absolute_path);
            }

            dialog.Filter = FILTER;

            if (dialog.ShowDialog(App.Current.MainWindow) == true)
            {
                textboxProject.Text = StudioGeneral.GetRelativePath(base_path, dialog.FileName);
                textboxProject.Focus();
            }
        }
    }
}
