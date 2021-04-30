using System.Text;
using System.Windows;

namespace VenturaSQLStudio.Pages.ProjectItemsPage
{
    /// <summary>
    /// Interaction logic for NewItemWindow.xaml
    /// </summary>
    public partial class NewItemWindow : VenturaWindow
    {
        private FolderItem _folderitem;

        public NewItemWindow(FolderItem folderitem)
        {
            _folderitem = folderitem;

            string base_name;

            if (folderitem is RootItem)
                base_name = "Root";
            else
                base_name = folderitem.Foldername;

            InitializeComponent();

            for (int i = 1; i < 999; i++)
            {
                string suggested_name = $"{base_name}{i}Recordset";
                if (_folderitem.ChildItemExists(suggested_name) == false)
                {
                    txtClassName.Text = suggested_name;
                    break;
                }
            }

            txtClassName.SelectAll();
            txtClassName.Focus();
        }

        private void btnNew_Click(object sender, RoutedEventArgs e)
        {
            txtClassName.Text = txtClassName.Text.Trim();

            bool valididentifier = System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(txtClassName.Text);

            if (txtClassName.Text.Length == 0 || valididentifier == false)
            {
                MessageBox.Show(this, @"Enter a valid class name.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtClassName.Focus();
                return;
            }

            // Begin: Since version 1.0.42 the name includes the word "Recordset" at the end.
            txtClassName.Text = StudioGeneral.MakeSureStringEndsWith(txtClassName.Text,"Recordset");

            if (txtClassName.Text.ToLower() == "recordset")
            {
                MessageBox.Show(this, @"The word Recordset must be preceded by one or more characters.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtClassName.Focus();
                return;
            }
            // End: Since version 1.0.42 the name includes the word "Recordset" at the end.

            if (_folderitem.ChildItemExists(txtClassName.Text) == true)
            {
                MessageBox.Show(this, $"An item with the name '{txtClassName.Text}' already exists in the selected folder. Enter a unique name.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtClassName.Focus();
                return;
            }


            DialogResult = true;

        }

        private string ReplaceAtIndex(string text, int index, char c)
        {
            var stringBuilder = new StringBuilder(text);
            stringBuilder[index] = c;
            return stringBuilder.ToString();
        }
    }
}
