using System.IO;
using System.Linq;
using System.Windows;


namespace VenturaSQLStudio.Pages.ProjectItemsPage
{
    /// <summary>
    /// Interaction logic for CreateFolderWindow.xaml
    /// </summary>
    public partial class CreateFolderWindow : VenturaWindow
    {
        private FolderItem _folderitem;

        public CreateFolderWindow(FolderItem folderitem)
        {
            _folderitem = folderitem;

            InitializeComponent();

            int x = 1;

            while( true)
            {
                string new_name = $"NewFolder{x}";
                x++;

                if( folderitem.ChildItemExists(new_name) == false)
                {
                    txtFolderName.Text = new_name;
                    break;
                }

            }

            txtFolderName.SelectAll();
            txtFolderName.Focus();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            txtFolderName.Text = txtFolderName.Text.Trim();

            if (txtFolderName.Text.Length == 0)
            {
                MessageBox.Show(this, @"Enter a valid folder name.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Information);
                txtFolderName.Focus();
                return;
            }

            if (IsFoldernameValid(txtFolderName.Text) == false)
            {
                MessageBox.Show(this, @"A folder name can't contain any of the following characters: \ / : * ? "" < > |", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtFolderName.Focus();
                return;
            }

            if (_folderitem.ChildItemExists(txtFolderName.Text) == true)
            {
                MessageBox.Show(this, $"A folder with the name '{txtFolderName.Text}' already exists in the selected folder. Enter a unique name.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtFolderName.Focus();
                return;
            }


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
