using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using VenturaSQLStudio.ProviderHelpers;

namespace VenturaSQLStudio
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private Project _currentproject;
        private string _filename;

        public string _statusbartext;

        private string _default_projects_folder;

        //private bool _autoloadproject = false;
        //private string _autoloadprojectfilename = "";
        private Version _do_not_notify_version = null;

        private bool _projectitems_checkbox_visible;

        private MostRecentlyUsedList _most_recently_used_list;

        private ObservableCollection<Tab> _tabs;

        public event PropertyChangedEventHandler PropertyChanged;

        private List<ProviderHelperBase> _providerHelpers = new List<ProviderHelperBase>();
        

        public MainViewModel()
        {
            _tabs = new ObservableCollection<Tab>();
            _currentproject = null;
            _filename = null;
            _most_recently_used_list = new MostRecentlyUsedList(30);

            _default_projects_folder = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "VenturaSQLProjects"); // SpecialFolder.UserProfile or MyDocuments


            // Build the list of providerhelpers

            IEnumerable<Type> types = from type in Assembly.GetExecutingAssembly().GetTypes()
                                      where type.IsDefined(typeof(ProviderInvariantNameAttribute), false)
                                      select type;

            foreach(var type in types)
            {
                _providerHelpers.Add((ProviderHelperBase)Activator.CreateInstance(type));
            }



        }

        public List<ProviderHelperBase> ProviderHelpers
        {
            get { return _providerHelpers; }
        }

        public ObservableCollection<Tab> Tabs
        {
            get { return _tabs; }
        }

        public string WindowTitle
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                if (_currentproject != null)
                {
                    sb.Append(Path.GetFileNameWithoutExtension(_filename));

                    if (_currentproject.IsModified)
                        sb.Append(" (Modified)");

                    sb.Append(" - ");
                }

                sb.Append($"VenturaSQL Studio {this.VenturaSqlVersionString}");

                return sb.ToString(); // "VenturaSQL Studio"
            }
        }

        public string DefaultProjectsFolder
        {
            get { return _default_projects_folder; }
        }

        /// <summary>
        /// The name of the VenturaSQL Studio project file.
        /// </summary>
        public string FileName
        {
            get { return _filename; }
            set
            {
                if (_filename == value)
                    return;

                _filename = value;

                OnPropertyChanged("FileName");
                OnPropertyChanged("WindowTitle");
            }
        }


        public Project CurrentProject
        {
            get
            {
                return _currentproject;
            }
            set
            {
                if (_currentproject == value)
                    return;

                if (_currentproject != null)
                    _currentproject.PropertyChanged -= CurrentProject_PropertyChanged;

                _currentproject = value;

                if (_currentproject != null)
                    _currentproject.PropertyChanged += CurrentProject_PropertyChanged;

                OnPropertyChanged("CurrentProject");
                OnPropertyChanged("WindowTitle");
            }
        }

        private void CurrentProject_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == "IsModified")
                OnPropertyChanged("WindowTitle");
        }

        public string StatusBarText
        {
            get { return _statusbartext; }
            set
            {
                if (_statusbartext == value)
                    return;

                _statusbartext = value;

                OnPropertyChanged("StatusBarText");
            }
        }


        public Version DoNotNotifyVersion
        {
            get { return _do_not_notify_version; }
            set
            {
                if (_do_not_notify_version == value)
                    return;

                _do_not_notify_version = value;

                OnPropertyChanged("DoNotNotifyVersion");
            }
        }

        public string GetTemplatesFolder()
        {
            string exe_path = Path.GetDirectoryName(Environment.GetCommandLineArgs()[0]);
            string template_folder = Path.Combine(exe_path, "..\\Templates");

            //#if DEBUG
            //            template_folder = @"C:\Active\VenturaSQL\BuildSystem\Templates";
            //#endif

            // Convert to absolute path.
            template_folder = Path.GetFullPath(template_folder);

            return template_folder;
        }


        ///// <summary>
        ///// This is not project data, but part of the program settings.
        ///// </summary>
        //public bool AutoLoadProject
        //{
        //    get { return _autoloadproject; }
        //    set
        //    {
        //        _autoloadproject = value;
        //        OnPropertyChanged("AutoLoadProject");
        //    }
        //}

        ///// <summary>
        ///// This is not project data, but part of the program settings.
        ///// </summary>
        //public string AutoLoadProjectFilename
        //{
        //    get { return _autoloadprojectfilename; }
        //    set
        //    {
        //        _autoloadprojectfilename = value;
        //        OnPropertyChanged("AutoLoadProjectFilename");
        //    }
        //}

        public Version VenturaSqlVersion
        {
            get { return Assembly.GetExecutingAssembly().GetName().Version; }
        }

        public string VenturaSqlVersionString
        {
            get { return VenturaSqlVersion.ToString(3); }
        }

        public bool ProjectItemsCheckBoxVisible
        {
            get { return _projectitems_checkbox_visible; }
            set
            {
                if (_projectitems_checkbox_visible == value)
                    return;

                _projectitems_checkbox_visible = value;

                OnPropertyChanged("ProjectItemsCheckBoxVisible");
                OnPropertyChanged("CheckBoxVisibility");
            }
        }

        /// <summary>
        /// Used in XAML data binding for the projectitems treeview.
        /// </summary>
        public Visibility CheckBoxVisibility
        {
            get
            {
                if (_projectitems_checkbox_visible == true)
                    return Visibility.Visible;
                else
                    return Visibility.Collapsed;
            }
        }

        public MostRecentlyUsedList MRU
        {
            get { return _most_recently_used_list; }
        }

        // Create the OnPropertyChanged method to raise the event
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }

    }

}