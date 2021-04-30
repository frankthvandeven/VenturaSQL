using System.Text.RegularExpressions;
using System.Windows;

namespace VenturaSQLStudio.Pages.ProjectItemsPage
{
    /// <summary>
    /// Interaction logic for RenameItemWindow.xaml
    /// </summary>
    public partial class RenameItemWindow : VenturaWindow
    {
        private Project _project;
        private ITreeViewItem _projectitem;

        public RenameItemWindow(Project project, ITreeViewItem projectitem)
        {
            _project = project;
            _projectitem = projectitem;

            InitializeComponent();

            txtName.Text = _projectitem.Name;
            txtName.SelectAll();
            txtName.Focus();

        }

        private void btnRename_Click(object sender, RoutedEventArgs e)
        {
            txtName.Text = txtName.Text.Trim();

            bool valididentifier = System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(txtName.Text);

            if (txtName.Text.Length == 0 || valididentifier == false)
            {
                MessageBox.Show(this, @"Enter a valid class name.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtName.Focus();
                return;
            }

            // Begin: Since version 1.0.42 the name includes the word "Recordset" at the end.
            txtName.Text = StudioGeneral.MakeSureStringEndsWith(txtName.Text, "Recordset");

            if (txtName.Text.ToLower() == "recordset")
            {
                MessageBox.Show(this, @"The word Recordset must be preceded by one or more characters.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtName.Focus();
                return;
            }
            // End: Since version 1.0.42 the name includes the word "Recordset" at the end.


            if (_projectitem.Parent.ChildItemExists(txtName.Text, _projectitem ) == true)
            {
                MessageBox.Show(this, $"An item with the name '{txtName.Text}' already exists in the selected folder. Enter a unique name.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtName.Focus();
                return;
            }

            if( _projectitem is RecordsetItem )
                (_projectitem as RecordsetItem).ClassName = txtName.Text;
            //else if (_projectitem is ViewmodelItem)
            //    (_projectitem as ViewmodelItem).ClassName = txtName.Text;
            //else if (_projectitem is DialogItem)
            //    (_projectitem as DialogItem).ClassName = txtName.Text;

            DialogResult = true;

        }

    }
}
