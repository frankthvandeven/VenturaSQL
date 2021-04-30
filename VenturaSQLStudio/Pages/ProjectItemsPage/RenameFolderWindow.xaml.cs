using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;


namespace VenturaSQLStudio.Pages.ProjectItemsPage
{
    /// <summary>
    /// Interaction logic for RenameFolderWindow.xaml
    /// </summary>
    public partial class RenameFolderWindow : VenturaWindow
    {
        private Project _project;
        private FolderItem _folderitem;

        public RenameFolderWindow(Project project, FolderItem folderitem)
        {
            _project = project;
            _folderitem = folderitem;

            InitializeComponent();

            txtName.Text = _folderitem.Foldername;
            txtName.SelectAll();
            txtName.Focus();
        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            txtName.Text = txtName.Text.Trim();

            if (txtName.Text.Length == 0)
            {
                MessageBox.Show(this, @"Enter a valid folder name.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Information);
                txtName.Focus();
                return;
            }

            if (IsFoldernameValid(txtName.Text) == false)
            {
                MessageBox.Show(this, @"A folder name can't contain any of the following characters: \ / : * ? "" < > |", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtName.Focus();
                return;
            }

            if ((_folderitem as ITreeViewItem).Parent.ChildItemExists(txtName.Text, _folderitem) == true)
            {
                MessageBox.Show(this, $"A folder with the name '{txtName.Text}' already exists in the selected folder. Enter a unique name.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtName.Focus();
                return;
            }

            _folderitem.Foldername = txtName.Text;

            DialogResult = true;

        }

        private bool IsFoldernameValid(string foldername)
        {
            char[] reserved = Path.GetInvalidFileNameChars();

            foreach (char c in reserved)
            {
                if (foldername.Contains(c))
                    return false;
            }

            return true;
        }
    }
}
