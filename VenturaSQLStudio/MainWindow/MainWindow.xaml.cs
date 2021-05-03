using Microsoft.Win32;
using System;
using System.ComponentModel;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media.Animation;
using VenturaSQLStudio.Helpers;
using VenturaSQLStudio.Pages;
using VenturaSQLStudio.Progress;
using VenturaSQLStudio.ProjectActions;

namespace VenturaSQLStudio
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private static MainViewModel _main_viewmodel = new MainViewModel();

        public static MainViewModel ViewModel
        {
            get { return _main_viewmodel; }
        }

        private IniFile _inifile;

        public MainWindow()
        {
            //SqlServerTypes.Utilities.LoadNativeAssemblies(AppDomain.CurrentDomain.BaseDirectory);

            this.DataContext = _main_viewmodel;

            // This prevents the right-click CONTEXT MENU from flickering:
            App.Current.Resources.Add(SystemParameters.MenuPopupAnimationKey, PopupAnimation.None);

            InitializeComponent();

            //Style = (Style)FindResource(typeof(Window));

            // This is the per-user-local-store data folder for program data:
            // (you can also use Environment.SpecialFolder.ApplicationData for Roaming application data. Only for small amounts of data)
            string iniFileFolder = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
            string iniFilePath = Path.Combine(iniFileFolder, @"VenturaSQLStudio", "settings.ini");

            // Open the .ini file for as long as this program is running
            _inifile = new IniFile(iniFilePath);

            ViewModel.MRU.ReadFromIniFile(_inifile);

            Group inigroup = _inifile["Settings"];

            if (double.TryParse(inigroup.Get("explorer_column_width", "250"), out double width))
            {
                _explorer_column.Width = new GridLength(Math.Max(width, 250));
            }

            inigroup = _inifile["Notifications"];
            Version v = null;
            Version.TryParse(inigroup.Get("donotnotifyversion", "0.0.0"), out v);
            ViewModel.DoNotNotifyVersion = v;

            // Ini settings to Project Viewmodel
            //Group inigroup = _inifile["AutoLoad"];

            //_mainmodel.AutoLoadProject = inigroup.Get("enabled", "no") == "yes" ? true : false;
            //_mainmodel.AutoLoadProjectFilename = inigroup.Get("projectfilename", "");

            _checkboxDoNotNotifyAgain.Checked += _checkboxDoNotNotifyAgain_Checked;
            _checkboxDoNotNotifyAgain.Unchecked += _checkboxDoNotNotifyAgain_Unchecked;

            this.AddTab(() => new StartPage(), "Start Page", "STARTPAGE", this.DataContext, TabMenu.CannotClose);
            //pageStartPage.SetSource(this);

            AddRecordsetCommandBinding.CanExecute += _project_items_control.AddRecordsetCommand_CanExecute;
            AddRecordsetCommandBinding.Executed += _project_items_control.AddRecordsetCommand_Executed;

            AutoCreateCommandBinding.CanExecute += _project_items_control.AutoCreateCommand_CanExecute;
            AutoCreateCommandBinding.Executed += _project_items_control.AutoCreateCommand_Executed;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (App.StartUpProject != null)
                if (File.Exists(App.StartUpProject))
                    DoOpen(App.StartUpProject);

            //if (_mainmodel.AutoLoadProject && File.Exists(_mainmodel.AutoLoadProjectFilename))
            //{
            //    DoLoadFilename(_mainmodel.AutoLoadProjectFilename);
            //}

            Action action = () =>
            {
                // make sure the default folder exists
                if (Directory.Exists(ViewModel.DefaultProjectsFolder) == false)
                {
                    // Ignore any Exception
                    try { Directory.CreateDirectory(ViewModel.DefaultProjectsFolder); }
                    catch { }
                }

#if LICENSE_MANAGER
                Central.Refresh();
#endif

                //if (Central.RefreshResult != RefreshResult.ValidLicenseKey || (Central.RefreshResult == RefreshResult.ValidLicenseKey && Central.Unlocked() == false))
                //{
                //    MessageBox.Show("Your Ventura license is not valid and needs attention.\n\nThe license page will be opened.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Stop);
                //    this.AddTab(() => new LicensePage(), "License", "LICENSE ", this.DataContext, TabMenu.CloseAble);
                //}

                var check = new CheckForUpdate();
                check.CheckForUpdateEvent += Check_CheckForUpdateEvent;
                
                _ = check.RunAsync(); // Async method not awaited is intentional.

            }; // end of Action block

            Application.Current.Dispatcher.Invoke(action);

            //DispatcherTimer timer = new DispatcherTimer();
            //timer.Interval = TimeSpan.FromSeconds(1);
            //timer.Tick += DispatcherTimer_Tick;
            //timer.Start();
        }

        //private void DispatcherTimer_Tick(object sender, EventArgs e)
        //{
        //    CommandManager.InvalidateRequerySuggested();
        //}

        private Version _latest_version_reported_by_server = null;

        private void Check_CheckForUpdateEvent(Version latest_version)
        {
            _latest_version_reported_by_server = latest_version;

            Version current = _main_viewmodel.VenturaVersion;
            current = new Version(current.Major, current.Minor, current.Build); // Remove the fourth part, the revision number, from the current version number

            if (latest_version > current)
            {
                if (ViewModel.DoNotNotifyVersion == latest_version)
                    return;

                Action action = () =>
                {
                    _textblockMessage.Text = $"A new VenturaSQL version is available: {latest_version}";
                    _checkboxDoNotNotifyAgain.ToolTip = $"If checked, you won't be reminded again that version {latest_version} is available";

                    Storyboard sb = Resources["sbShowUpdateAvailablePanel"] as Storyboard;
                    sb.Begin(_new_update_available_panel);

                }; // end of Action block

                Application.Current.Dispatcher.Invoke(action);
            }

        }

        private void _checkboxDoNotNotifyAgain_Checked(object sender, RoutedEventArgs e)
        {
            if (_latest_version_reported_by_server == null)
                return;

            ViewModel.DoNotNotifyVersion = _latest_version_reported_by_server;
        }

        private void _checkboxDoNotNotifyAgain_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewModel.DoNotNotifyVersion = null;
        }


        private void button_DownloadUpdate_Click(object sender, RoutedEventArgs e)
        {
            StudioGeneral.StartBrowser("https://site.sysdev.nl/venturasql");
        }

        private void button_CloseUpdatePanel_Click(object sender, RoutedEventArgs e)
        {
            Storyboard sb = Resources["sbHideUpdateAvailablePanel"] as Storyboard;
            sb.Begin(_new_update_available_panel);
        }

        /// <summary>
        /// Opens a tab and puts a page control on it.
        /// </summary>
        /// <param name="func">A function returning the Page to create after the tab is created.</param>
        /// <param name="tabtitle"></param>
        /// <param name="uniqueid">If a tab with this id is already open, it will be focussed, and no new tab/page will be created. 'null' means not unique</param>
        public Tab AddTab(Func<UserControl> func, string tabtitle, string uniqueid, object datacontext, TabMenu context_menu_style)
        {
            if (uniqueid != null)
                foreach (Tab existing_tab in ViewModel.Tabs)
                    if (existing_tab.UniqueID == uniqueid)
                    {
                        tabControl.SelectedItem = existing_tab;
                        return existing_tab;
                    }

            // Instantiate the new page object.
            UserControl page = func();

            bool showclosebutton = context_menu_style == TabMenu.CloseAble ? true : false;

            Tab new_tab = new Tab(uniqueid, tabtitle, page, datacontext, showclosebutton);

            ViewModel.Tabs.Add(new_tab);
            tabControl.SelectedItem = new_tab;

            ContextMenu context_menu = null;
            MenuItem menu_item;

            // Set up the correct context menu
            if (context_menu_style == TabMenu.CloseAble)
            {
                context_menu = new ContextMenu();

                menu_item = new MenuItem();
                menu_item.Header = "Close Item";
                menu_item.Click += delegate { ViewModel.Tabs.Remove(new_tab); };
                context_menu.Items.Add(menu_item);

                menu_item = new MenuItem();
                menu_item.Header = "Close All";
                menu_item.Click += delegate { CloseAllTabs(); };
                context_menu.Items.Add(menu_item);

                menu_item = new MenuItem();
                menu_item.Header = "Close All But This";
                menu_item.Click += delegate { CloseAllButThis(new_tab); };
                context_menu.Items.Add(menu_item);
            }
            else if (context_menu_style == TabMenu.CannotClose)
            {
                context_menu = new ContextMenu();

                menu_item = new MenuItem();
                menu_item.Header = "This tab remains open.";
                menu_item.IsEnabled = false;
                context_menu.Items.Add(menu_item);
            }

            new_tab.ContextMenu = context_menu;

            //Binding myBinding = new Binding();
            //myBinding.Source = datacontext;
            //myBinding.Path = new PropertyPath("TabItemTitle");
            //myBinding.Mode = BindingMode.OneWay;
            //myBinding.UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged;
            //BindingOperations.SetBinding((DependencyObject)tabitem.Header, ContentControl.ContentProperty, myBinding);

            //tabitem.Focus();

            return new_tab;
        }

        public void CloseTabContainingPage(UserControl page)
        {
            foreach (Tab tab in ViewModel.Tabs)
                if (tab.Content.Equals(page))
                {
                    ViewModel.Tabs.Remove(tab);
                    return;
                }
        }

        public void CloseTab(TabItem tabitem)
        {
            tabControl.Items.Remove(tabitem);
        }

        public void CloseTab(string uniqueId)
        {
            var tabs = ViewModel.Tabs;

            foreach (Tab tab in tabs)
            {
                if (tab.UniqueID != null && tab.UniqueID == uniqueId)
                {
                    tabs.Remove(tab);
                    return;
                }
            }
        }

        public void CloseAllTabs()
        {
            var tabs = ViewModel.Tabs;

            const int NUMBER_OF_FIXEDTABS = 1;

            for (int x = tabs.Count; x > NUMBER_OF_FIXEDTABS; x--)
                tabs.RemoveAt(x - 1);
        }

        public void CloseAllButThis(Tab tab_to_skip)
        {
            var tabs = ViewModel.Tabs;

            const int NUMBER_OF_FIXEDTABS = 1;

            for (int x = tabs.Count; x > NUMBER_OF_FIXEDTABS; x--)
            {
                if (tabs[x - 1].Equals(tab_to_skip) == false)
                    tabs.RemoveAt(x - 1);

            }
        }

#region Command related methods
        private void CommonCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommonNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void CommandSave_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel.CurrentProject != null)
                e.CanExecute = true;
        }

        private void CommandSaveAs_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel.CurrentProject != null)
                e.CanExecute = true;
        }

        private void CommandGenerate_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel.CurrentProject != null)
                e.CanExecute = true;
        }

        private void CommandSettings_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel.CurrentProject != null)
                e.CanExecute = true;
        }

        private void AdvancedSettingsCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel.CurrentProject != null)
                e.CanExecute = true;
        }

        private void SelectProviderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel.CurrentProject != null)
                e.CanExecute = true;
        }

        private void CommandClose_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel.CurrentProject != null)
                e.CanExecute = true;
        }

        private void CommandExploreFolder_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ViewModel.CurrentProject != null)
                e.CanExecute = true;
        }

        private void CommandNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoNew();
        }

        private void CommandOpen_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoOpen();
        }

        private void CommandSave_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoSave();
        }

        private void CommandSaveAs_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoSaveAs();
        }

        private void CommandClose_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoClose();
        }

        private void CommandGenerate_Executed(object sender, ExecutedRoutedEventArgs e)
        {

#if LICENSE_MANAGER
            if (Central.RefreshResult != RefreshResult.ValidLicenseKey || (Central.RefreshResult == RefreshResult.ValidLicenseKey && Central.Unlocked() == false))
            {
                MessageBox.Show("Code generation is locked.\n\nYour Ventura license is not activated or invalid and needs attention.\n\nThe license page will be selected.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Stop);
                this.AddTab(() => new LicensePage(), "License", "LICENSE ", this.DataContext, TabMenu.CloseAble);
                return;
            }
#endif

            // This makes sure the changed data in a focussed control is committed to the ViewModel
            FocusManager.SetFocusedElement(this, null);

            Tab generate_tab = this.AddTab(() => new GeneratePage(ViewModel.CurrentProject), "Generate", "GENERATECODE", this.DataContext, TabMenu.CloseAble);

            GeneratePage generate_page = (GeneratePage)generate_tab.Content;

            generate_page.DoGenerate();
        }

        private void CommandSettings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoOpenProjectSettings();
        }

        private void AdvancedSettings_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoOpenAdvancedProviderSettings();
        }

        private void SelectProviderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            DoOpenSelectProvider();
        }

        public void DoOpenProjectSettings()
        {
            this.AddTab(() => new ProjectSettingsPage(ViewModel.CurrentProject), "Project Settings", "PROJECTSETTINGS", this.DataContext, TabMenu.CloseAble);
        }

        public void DoOpenAdvancedProviderSettings(AdvancedWindow.OpenWithTab tab_to_select = AdvancedWindow.OpenWithTab.Welcome)
        {
            AdvancedWindow window = new AdvancedWindow(ViewModel.CurrentProject, tab_to_select);
            window.Owner = Application.Current.MainWindow;
            window.ShowDialog();
        }

        public void DoOpenSelectProvider()
        {
            this.AddTab(() => new ProviderPage(), "Select ADO.NET Provider", "PROVIDER ", this.DataContext, TabMenu.CloseAble);
        }

        private void CommandExploreFolder_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            //string root_folder = Path.GetDirectoryName(ViewModel.FileName);
            OpenFileExplorerWindow.Exec(ViewModel.FileName);
        }

#endregion

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            // This makes sure the changed data in a focussed control is committed to the ViewModel
            FocusManager.SetFocusedElement(this, null);

            bool cancelled = DoClose();

            if (cancelled)
            {
                e.Cancel = true;
                return;
            }

            // Write to the .INI file
            ViewModel.MRU.WriteToIniFile(_inifile);

            // Settings
            Group inigroup = _inifile["Settings"];

            inigroup.Set("explorer_column_width", _explorer_column.Width.ToString());

            // The 'do not notify' setting
            inigroup = _inifile["Notifications"];

            if (ViewModel.DoNotNotifyVersion == null)
                inigroup.Set("donotnotifyversion", "");
            else
                inigroup.Set("donotnotifyversion", ViewModel.DoNotNotifyVersion.ToString(3));

            // Project ViewModel settings to IniFile
            //Group inigroup = _inifile["AutoLoad"];
            //inigroup.Set("enabled", _mainmodel.AutoLoadProject ? "yes" : "no");
            //inigroup.Set("projectfilename", _mainmodel.AutoLoadProjectFilename);

            // This is the last thing to be done before terminating process:
            _inifile.Save();
        }

        internal void DoNew()
        {
            NewProjectWindow window = new NewProjectWindow();

            bool? dialogresult = window.ShowDialog();

            if (dialogresult == false)
                return;

            string newproject_filename = window.SelectedProjectFilename;
            string newproject_folder = Path.GetDirectoryName(window.SelectedProjectFilename);

            bool cancelled = DoClose(); // Close the open project (if there is one open).

            if (cancelled == true)
                return;

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                if (Directory.Exists(newproject_folder) == false)
                    Directory.CreateDirectory(newproject_folder);

                if (window.SelectedTemplateFolder == null) // No template selected. Create a project file from scratch.
                {
                    // Write an empty project to disk.
                    Project temp_project = new Project();

                    // Create the "VenturaRecordsets" folder in the new project's folder structure.
                    FolderItem fi = temp_project.FolderStructure.FetchOrCreateFolderItem("VenturaRecordsets");
                    fi.IsSelected = true; // Select the folder
                    fi.ExpandBubbleUp(); // Make sure all parent folders are expanded

                    SaveToFile savetofile = new SaveToFile();
                    savetofile.Save(temp_project, newproject_filename);
                }
                else
                {
                    string[] projectfiles_found = Directory.GetFiles(window.SelectedTemplateFolder, "*.venproj");

                    if (projectfiles_found.Length == 0)
                        throw new FileNotFoundException($"Template folder {window.SelectedTemplateFolder} does not contain any .venproj files.");

                    if (projectfiles_found.Length > 1)
                        throw new FileNotFoundException($"Template folder {window.SelectedTemplateFolder} contains multiple .venproj files, confused.");

                    string project_filename_in_template = Path.GetFileName(projectfiles_found[0]);
                    string file_to_rename = Path.Combine(newproject_folder, project_filename_in_template);
                    string file_to_delete = Path.Combine(newproject_folder, "template.xml");

                    FolderCopy.Exec(window.SelectedTemplateFolder, newproject_folder);
                    File.Move(file_to_rename, newproject_filename); // Rename to desired project filename.
                    File.Delete(file_to_delete); // Delete the template.xml that was copied by FolderCopy
                }
            };

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Creating project...", action);

            if (result.Error != null)
            {
                MessageBox.Show("Creating project file failed. " + result.Error.Message);
                return;
            }

            ViewModel.MRU.AddFile(newproject_filename);

            ViewModel.StatusBarText = "Project created";

            DoOpen(newproject_filename);
        }

        private const string FILTER = "Project Files (*.venproj)|*.venproj";

        public void DoOpen(string filename_to_open = null)
        {
            if (filename_to_open == null)
            {
                OpenFileDialog dialog = new OpenFileDialog();
                dialog.InitialDirectory = MainWindow.ViewModel.DefaultProjectsFolder;
                //dialog.FileName = textboxProject1.Text;

                dialog.Filter = FILTER;

                if (dialog.ShowDialog(this) == false)
                    return;

                filename_to_open = dialog.FileName;
            }

            bool cancelled = DoClose();

            if (cancelled == true)
                return;

            //_mainmodel.AutoLoadProjectFilename = filename_to_open;

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                LoadFromFile loader = new LoadFromFile();

                // Throwing an Exception during Load() is totally Ok. The error
                // message will be displayed and the load will not happen.
                ViewModel.CurrentProject = loader.Load(filename_to_open);
            };

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Loading project...", action);

            if (result.Error != null)
                MessageBox.Show("Opening file failed. " + result.Error.Message);
            else
            {
                ViewModel.FileName = filename_to_open;
                ViewModel.MRU.AddFile(filename_to_open);
            }

            // This is handled by {Binding ...} now. this.ProjectItemsControl.Project = ViewModel.CurrentProject;

        }

        /// <summary>
        /// Returns true if closing was cancelled.
        /// </summary>
        internal bool DoClose()
        {
            // This makes sure the changed data in a focussed control is committed to the ViewModel
            FocusManager.SetFocusedElement(this, null);

            if (ViewModel.CurrentProject == null) // we are already closed
                return false;

            if (ViewModel.CurrentProject.IsModified)
            {
                bool cancelled = AskUserWhatToDoWithUnsavedChanges();
                if (cancelled == true)
                    return true;
            }

            CloseAllTabs();

            ViewModel.CurrentProject = null;
            ViewModel.FileName = null;
            return false;
        }

        internal void DoSave()
        {
            // This makes sure the changed data in a focussed control is committed to the ViewModel
            FocusManager.SetFocusedElement(this, null);

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                SaveToFile savetofile = new SaveToFile();
                savetofile.Save(ViewModel.CurrentProject, ViewModel.FileName);
            };

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Saving project...", action);

            if (result.Error != null)
            {
                MessageBox.Show("Saving file failed. " + result.Error.Message);
                return;
            }
            ViewModel.StatusBarText = "Project saved";
        }

        internal void DoSaveAs()
        {
            // This makes sure the changed data in a focussed control is committed to the ViewModel
            FocusManager.SetFocusedElement(this, null);

            string filename = AskSaveAsFileName();

            if (filename == "")
                return;

            // At this point we are guaranteed to have a filename
            //_mainmodel.AutoLoadProjectFilename = filename;

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                SaveToFile savetofile = new SaveToFile();
                savetofile.Save(ViewModel.CurrentProject, filename);
            };

            ProgressDialogResult result = ProgressDialog.Execute(Application.Current.MainWindow, "Saving project...", action);

            if (result.Error != null)
            {
                MessageBox.Show("Saving file failed. " + result.Error.Message);
                return;
            }
            else
            {
                // Update the filename
                ViewModel.FileName = filename;
                ViewModel.MRU.AddFile(filename);
            }

            ViewModel.StatusBarText = "Project saved";

        }

        /// <summary>
        /// Asks the user whether changes should be saved. If Yes this method will save the changes.
        /// 
        /// If the user clicks Cancel, this method will return true.
        /// 
        /// Only call this method after detecting _project.IsModified is set to true.
        /// </summary>
        private bool AskUserWhatToDoWithUnsavedChanges()
        {

            if (ViewModel.CurrentProject.IsModified == false)
                return false;

            MessageBoxResult result = MessageBox.Show($"Do you want to save changes to {Path.GetFileName(ViewModel.FileName)}?", "VenturaSQL Studio", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning, MessageBoxResult.Yes, MessageBoxOptions.None);

            if (result == MessageBoxResult.Cancel)
                return true;

            if (result == MessageBoxResult.No)
                return false;

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) =>
            {
                SaveToFile savetofile = new SaveToFile();
                savetofile.Save(ViewModel.CurrentProject, ViewModel.FileName);
            };

            ProgressDialogResult result1 = ProgressDialog.Execute(Application.Current.MainWindow, "Saving project...", action);

            if (result1.Error != null)
            {
                MessageBox.Show("Saving file failed. " + result1.Error.Message);
                return true;
            }

            ViewModel.MRU.AddFile(ViewModel.FileName);

            ViewModel.StatusBarText = "Project saved";

            return false;
        }

        /// <summary>
        /// Returns an empty string if no filename was selected.
        /// </summary>
        private string AskSaveAsFileName()
        {
            SaveFileDialog dialog = new SaveFileDialog();
            dialog.InitialDirectory = ViewModel.DefaultProjectsFolder;
            dialog.Filter = FILTER;
            bool? result = dialog.ShowDialog(this);

            if (result == null)
                MessageBox.Show("null");

            if (result == true)
                return dialog.FileName;
            else
                return "";

        }

        private void MenuExit_Click(object sender, RoutedEventArgs e)
        {
            //MainWindow window = (MainWindow)Application.Current.MainWindow;
            this.Close();
        }

        private void MenuAboutClick(object sender, RoutedEventArgs e)
        {
            this.AddTab(() => new AboutPage(), "About", "ABOUT ", this.DataContext, TabMenu.CloseAble);
        }

        private void MenuOnlineDocumentationClick(object sender, RoutedEventArgs e)
        {
            StudioGeneral.StartBrowser("https://docs.sysdev.nl/");
        }

        private void MenuGettingStartedClick(object sender, RoutedEventArgs e)
        {
            StudioGeneral.StartBrowser("https://docs.sysdev.nl/GettingStarted.html");
        }

        private void MenuOptions_Click(object sender, RoutedEventArgs e)
        {
            this.AddTab(() => new OptionsPage(ViewModel), "Options", "OPTIONS ", this.DataContext, TabMenu.CloseAble);
        }

        private void MenuLicenseClick(object sender, RoutedEventArgs e)
        {
            //this.AddTab(() => new LicensePage(), "License", "LICENSE ", this.DataContext, TabMenu.CloseAble);
        }

        private void MenuCheckForUpdatesClick(object sender, RoutedEventArgs e)
        {

        }

        private void TabCloseButton_Click(object sender, RoutedEventArgs e)
        {
            var control = (Button)sender;
            var tab_item = (Tab)control.DataContext;

            ViewModel.Tabs.Remove(tab_item);
        }


        
    }
}
