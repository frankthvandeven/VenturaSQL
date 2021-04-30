using System;
using System.ComponentModel;

namespace VenturaSQLStudio {

    public class VisualStudioProjectItem : ViewModelBase
    {
        private int _projectindex; // starts with 1 and not zero.

        private bool _projectenabled;
        private string _outputprojectfilename;
        private string _targetplatform;
        private bool _generateDirectAdoConnectionCode;
        private bool _checkboxenabled;

        private Project _owningproject;

        public VisualStudioProjectItem(Project owningproject, int projectindex)
        {
            _owningproject = owningproject;

            _projectindex = projectindex;

            _projectenabled = false;
            _outputprojectfilename = "";
            _generateDirectAdoConnectionCode = true;
            _checkboxenabled = false;
            _targetplatform = "NETStandard";
        }

        public bool ProjectEnabled
        {
            get { return _projectenabled; }
            set
            {
                if (_projectenabled == value)
                    return;

                _projectenabled = value;
                NotifyPropertyChanged("ProjectEnabled");
                NotifyPropertyChanged("TargetPlatform");
                NotifyPropertyChanged("GenerateDirectAdoConnectionCode");
                NotifyPropertyChanged("CheckBoxEnabled");
                NotifyPropertyChanged("ProjectFileInfo");

                _owningproject?.SetModified();
            }
        }

        /// <summary>
        /// The Visual Studio project filenames including path.
        /// </summary>
        public string OutputProjectFilename
        {
            get { return _outputprojectfilename; }
            set
            {
                if (_outputprojectfilename == value)
                    return;

                _outputprojectfilename = value;
                NotifyPropertyChanged("OutputProjectFilename");
                NotifyPropertyChanged("ProjectFileInfo");

                _owningproject?.SetModified();
            }
        }

        public string TargetPlatform
        {
            get { return _targetplatform; }
            set
            {
                // When the loaded value equals the default value, the notification event would not be sent. That's why we disabled the following 2 lines.
                //if (_targetplatform == value)
                //    return;

                _targetplatform = value;

                TargetPlatformListItem item = TargetPlatformList.FindItem(_targetplatform);

                if (item != null)
                {
                    if (item.AdoSupport == AdoSupport.NoAdo)
                    {
                        _generateDirectAdoConnectionCode = false;
                        _checkboxenabled = false;
                    }
                    else if (item.AdoSupport == AdoSupport.Obligated)
                    {
                        _generateDirectAdoConnectionCode = true;
                        _checkboxenabled = false;
                    }
                    if (item.AdoSupport == AdoSupport.YesUserCanChoose)
                    {
                        _generateDirectAdoConnectionCode = true;
                        _checkboxenabled = true;
                    }
                }

                NotifyPropertyChanged("TargetPlatform");
                NotifyPropertyChanged("ProjectFileInfo");
                NotifyPropertyChanged("GenerateDirectAdoConnectionCode");
                NotifyPropertyChanged("CheckBoxEnabled");

                _owningproject?.SetModified();
            }
        }

        // Support direct Ado connection (suppress sql script etc..)
        public bool GenerateDirectAdoConnectionCode
        {
            get { return _generateDirectAdoConnectionCode; }
            set
            {
                if (_generateDirectAdoConnectionCode == value)
                    return;

                _generateDirectAdoConnectionCode = value;
                NotifyPropertyChanged("GenerateDirectAdoConnectionCode");
                NotifyPropertyChanged("ProjectFileInfo");

                _owningproject?.SetModified();
            }
        }

        // Enabled status for Checkbox GenerateDirectAdoConnectionCode
        public bool CheckBoxEnabled
        {
            get
            {
                if (_projectenabled == false)
                    return false;

                return _checkboxenabled;
            }
        }

        // INFO PROPERTIES

        public int ProjectIndex
        {
            get
            {
                return _projectindex;
            }
        }

        public string HeaderLabelText
        {
            get { return $"Visual Studio C# project {_projectindex}"; }
        }

        /// <summary>
        /// Used in the Recordset's settings page.
        /// </summary>
        public string ProjectFileInfo
        {
            get
            {
                string temp = _outputprojectfilename.Trim();
                return $"Project {_projectindex}" + (temp.Length == 0 ? "" : " (" + temp + ")");
            }
        }

    }
}
