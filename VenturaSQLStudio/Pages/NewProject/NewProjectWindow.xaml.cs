using Microsoft.Win32;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Xml;

namespace VenturaSQLStudio.Pages
{
    /// <summary>
    /// Interaction logic for NewProjectWindow.xaml
    /// </summary>
    public partial class NewProjectWindow : VenturaWindow
    {
        private string _selectedprojectfilename;
        private string _selected_template_folder = null;

        private const string FILTER = "Visual Studio solution (*.sln)|*.sln";

        public ViewModelClass ViewModel { get; } = new ViewModelClass();

        public NewProjectWindow()
        {
            this.DataContext = this;

            InitializeComponent();

            FillTemplateList();

            comboTemplates.ItemsSource = ViewModel.Templates;
            comboTemplates.SelectedIndex = 0;

            cbCreateDirectory.IsChecked = true;
            //cbCreateDirectory.IsEnabled = false;

            txtFilename.Text = this.FindNewFileName("Ventura", MainWindow.ViewModel.DefaultProjectsFolder);
            txtFilename.SelectAll();
            txtFilename.Focus();

            txtLocation.Text = MainWindow.ViewModel.DefaultProjectsFolder;
        }

        private void FillTemplateList()
        {
            TemplateItem tpi = new TemplateItem
            {
                Index = "a", // for sorting the list
                Title = "Create an empty project.",
                Description = "Start with an empty project file.",
                Folder = null
            };
            ViewModel.Templates.Add(tpi);

            string folder = MainWindow.ViewModel.GetTemplatesFolder();

            if (!Directory.Exists(folder))
            {
                ViewModel.Templates[0].Description += $" Templates not available. Template folder {folder} does not exist.";
                return;
            }

            foreach (string directory in Directory.EnumerateDirectories(folder))
            {
                string filename = Path.Combine(directory, "Template.xml");

                if (File.Exists(filename))
                {
                    tpi = new TemplateItem();
                    tpi.Folder = directory;
                    ReadTemplateInfo(filename, tpi);

                    ViewModel.Templates.Add(tpi);
                }
            }

            if (ViewModel.Templates.Count == 1)
            {
                ViewModel.Templates[0].Description += $" Templates not available. No templates were found in template folder {folder}";
                return;
            }

            // Sort the list with templates on the index property.
            ViewModel.Templates.Sort(a => a.Index);
        }

        private void ReadTemplateInfo(string filename, TemplateItem item)
        {
            XmlDocument doc = new XmlDocument();

            doc.Load(filename);

            XmlNode root = doc.DocumentElement;
            XmlAttribute index_attribute = doc.DocumentElement.GetAttributeNode("index");
            XmlNode title_node = root.SelectSingleNode("title");
            XmlNode description_node = root.SelectSingleNode("description");

            if (index_attribute == null)
                item.Index = "z";
            else
                item.Index = index_attribute.Value;

            item.Title = stripInnerText(title_node);
            item.Description = stripInnerText(description_node);
        }

        private string stripInnerText(XmlNode node)
        {
            string text = node.InnerText;

            text = text.Replace("\r", " ").Replace("\n", " ");

            while (text.Contains("  "))
                text = text.Replace("  ", " ");

            return text.Trim();
        }

        /// <summary>
        /// Returns a filename that doesn't exist in the folder.
        /// </summary>
        private string FindNewFileName(string basename, string folder)
        {
            for (int i = 1; i < 9999; i++)
            {
                string fullfolder_path = Path.Combine(folder, $"{basename}{i}");
                string fullfile_path = Path.Combine(folder, $"{basename}{i}.venproj");
                if (File.Exists(fullfile_path) == false && Directory.Exists(fullfolder_path) == false)
                    return $"{basename}{i}";
            }

            return basename;
        }

        private void btnBrowse_Click(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog cofd = new CommonOpenFileDialog();
            cofd.DefaultDirectory = MainWindow.ViewModel.DefaultProjectsFolder;
            cofd.IsFolderPicker = true;

            string initial = txtLocation.Text.Trim();

            if (initial.Length > 0 && Directory.Exists(initial))
                cofd.InitialDirectory = initial;

            CommonFileDialogResult result = cofd.ShowDialog(this);

            if (result == CommonFileDialogResult.Ok)
                txtLocation.Text = cofd.FileName;

        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            string filename = txtFilename.Text.Trim();
            string location = txtLocation.Text.Trim();

            if (filename.Length == 0)
            {
                MessageBox.Show(this, @"Enter a file name.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtFilename.Focus();
                return;
            }

            if (IsFilenameValid(filename) == false)
            {
                MessageBox.Show(this, @"A file name can't contain any of the following characters: \ / : * ? "" < > |", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtFilename.Focus();
                return;
            }

            if (location.Length == 0)
            {
                MessageBox.Show(this, @"Select a location.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtLocation.Focus();
                return;
            }

            if (Directory.Exists(location) == false)
            {
                MessageBox.Show(this, $"Location {location} does not exist.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtLocation.Focus();
                return;
            }

            bool create_directory = cbCreateDirectory.IsChecked.Value;

            if (create_directory == true && Directory.Exists(Path.Combine(location, filename)))
            {
                MessageBox.Show(this, $"Directory {filename} already exists in location {location}.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtFilename.Focus();
                return;
            }

            if (create_directory)
                location = Path.Combine(location, filename);

            _selectedprojectfilename = Path.Combine(location, filename);

            if (_selectedprojectfilename.ToLower().EndsWith(".venproj") == false)
                _selectedprojectfilename = _selectedprojectfilename + ".venproj";

            //if (create_directory == false)
            if (File.Exists(_selectedprojectfilename) == true)
            {
                MessageBox.Show(this, $"Project file {_selectedprojectfilename} already exists.\nChoose a different project name.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                txtLocation.Focus();
                return;
            }

            if (comboTemplates.SelectedIndex == 0)
                _selected_template_folder = null;
            else
                _selected_template_folder = ViewModel.Templates[comboTemplates.SelectedIndex].Folder;

            DialogResult = true;
        }

        /// <summary>
        /// The filename for the project to be created. The folder might not exist yet.
        /// </summary>
        public string SelectedProjectFilename
        {
            get { return _selectedprojectfilename; }
        }

        /// <summary>
        /// The folder for the selected template.
        /// </summary>
        public string SelectedTemplateFolder
        {
            get { return _selected_template_folder; }
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Select a Visual Studio solution file";

            dialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
            dialog.Filter = FILTER;

            if (dialog.ShowDialog(this) == true)
            {
                txtFilename.Text = Path.GetFileNameWithoutExtension(dialog.FileName);
                txtLocation.Text = Path.GetDirectoryName(dialog.FileName);

                cbCreateDirectory.IsChecked = false;
                comboTemplates.SelectedIndex = 0; // empty project
                txtFilename.SelectAll();
                txtFilename.Focus();
            }

            e.Handled = true;
        }

        private void comboTemplates_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (comboTemplates.SelectedIndex == 0)
            {
                cbCreateDirectory.IsChecked = false;
                cbCreateDirectory.IsEnabled = true;
            }
            else
            {
                cbCreateDirectory.IsChecked = true;
                cbCreateDirectory.IsEnabled = false;
            }

        }

        private void cbCreateDirectory_Checked(object sender, RoutedEventArgs e)
        {
            labelFilename.Text = "File name and directory name:";
        }

        private void cbCreateDirectory_Unchecked(object sender, RoutedEventArgs e)
        {
            labelFilename.Text = "File name:";
        }

        private bool IsFilenameValid(string filename)
        {
            char[] reserved = Path.GetInvalidFileNameChars();

            foreach (char c in reserved)
            {
                if (filename.Contains(c))
                    return false;
            }

            return true;
        }

        public class ViewModelClass : ViewModelBase
        {
            private string _filename = "";
            private string _location = "";
            private ObservableCollection<TemplateItem> _templates = new ObservableCollection<TemplateItem>();

            public string FileName
            {
                get { return _filename; }
                set
                {
                    if (_filename == value)
                        return;

                    _filename = value;

                    NotifyPropertyChanged("FileName");
                }
            }

            public string Location
            {
                get { return _location; }
                set
                {
                    if (_location == value)
                        return;

                    _location = value;

                    NotifyPropertyChanged("Location");
                }
            }

            public ObservableCollection<TemplateItem> Templates
            {
                get { return _templates; }
            }

        }

        public class TemplateItem
        {
            public string Index { get; set; }
            public string Title { get; set; }
            public string Description { get; set; }
            public string Folder { get; set; }
        }

    }
}
