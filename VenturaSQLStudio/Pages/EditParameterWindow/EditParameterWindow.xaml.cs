using System;
using System.Globalization;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Linq;
using System.Data;
using System.Windows.Input;
using VenturaSQL;

// One way of setting the design time data context:
// d:DataContext="{d:DesignInstance vm:TechnicianSelectionViewModel}"
//
// OR
//
//     <Window.DataContext>
//        <MainViewModel x:Name="ViewModel" //>
//    </Window.DataContext>


namespace VenturaSQLStudio.Pages
{
    public partial class EditParameterWindow : Window
    {
        public ViewModelClass ViewModel { get; } = new ViewModelClass();

        public enum Mode
        {
            New,
            Edit
        }

        private Mode _mode;
        private Project _project;
        private string _parameter_prefix;

        private RecordsetItem _recordset_item;
        private ParameterItem _edit_parameter_item;

        private DbTypeRepository _list;

        public EditParameterWindow(Mode mode, Project project, RecordsetItem recordset_item, ParameterItem edit_parameter_item)
        {
            _mode = mode;
            _project = project;
            _parameter_prefix = project.ParameterPrefix.ToString();
            _recordset_item = recordset_item;
            _edit_parameter_item = edit_parameter_item;

            this.DataContext = this;

            this.Width = Application.Current.MainWindow.Width - 40;
            this.Height = Application.Current.MainWindow.Height - 40;

            InitializeComponent();
                       
            _list = new DbTypeRepository();

            lvTasks.ItemsSource = _list;

            CollectionView view = (CollectionView)CollectionViewSource.GetDefaultView(lvTasks.ItemsSource);
            PropertyGroupDescription groupDescription = new PropertyGroupDescription("Group", new EditParameterTasknameConverter());
            view.GroupDescriptions.Add(groupDescription);

            textblockInfo.Text = "Select a parameter type from the list.";

            RunPrefix.Text = _parameter_prefix; // TextBlock>>Run

            this.Loaded += EditParameterWindow_Loaded;
        }

        private void EditParameterWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (_mode == Mode.Edit)
            {
                Title = $"Edit Parameter {_edit_parameter_item.Name}";

                ViewModel.Name = _edit_parameter_item.Name;
                ViewModel.FullTypename = _edit_parameter_item.FullTypename;

                ViewModel.Input = _edit_parameter_item.Input;
                ViewModel.Output = _edit_parameter_item.Output;

                ViewModel.SetDbType = _edit_parameter_item.SetDbType;
                ViewModel.SetLength = _edit_parameter_item.SetLength;
                ViewModel.SetPrecision = _edit_parameter_item.SetPrecision;
                ViewModel.SetScale = _edit_parameter_item.SetScale;

                ViewModel.DbTypeString = _edit_parameter_item.DbTypeString;
                ViewModel.Length = _edit_parameter_item.Length;
                ViewModel.Precision = _edit_parameter_item.Precision;
                ViewModel.Scale = _edit_parameter_item.Scale;

                ViewModel.DesignValue = _edit_parameter_item.DesignValue;
            }
            else
            {
                Title = "New Parameter";
                int ordinal = _recordset_item.Parameters.Count + 1;

                ViewModel.Name = _parameter_prefix + "P" + ordinal.ToString();
                ViewModel.FullTypename = "System.Int32";

                ViewModel.Input = true;
                ViewModel.Output = false;

                ViewModel.SetDbType = false;
                ViewModel.SetLength = false;
                ViewModel.SetPrecision = false;
                ViewModel.SetScale = false;

                ViewModel.DbTypeString = "DbType.Int32";
                ViewModel.Length = 0;
                ViewModel.Precision = 0;
                ViewModel.Scale = 0;

                ViewModel.DesignValue = "null";
            }

            tbParamName.Focus();
        }

        private void tbParamName_GotFocus(object sender, RoutedEventArgs e)
        {
            if (tbParamName.Text.Length >= 2) // Do not include parameter prefix in the selection
                tbParamName.Select(1, tbParamName.Text.Length - 1);
        }

        private void btnOk_Click(object sender, RoutedEventArgs e)
        {
            // This makes sure the changed data in a focussed control is committed to the ViewModel
            FocusManager.SetFocusedElement(this, null);

            ViewModel.Name = ViewModel.Name.Trim();

            if (ViewModel.Name.Length < 2)
            {
                Error("Enter a parameter name of at least 2 characters.");
                tbParamName.Focus();
                return;
            }

            if (ViewModel.Name.Contains(" ") == true)
            {
                Error("Parameter name can not contain space characters.");
                tbParamName.Focus();
                return;
            }

            if (ViewModel.Name.StartsWith(_parameter_prefix) == false)
            {
                Error($"Parameter name must start with prefix '{_parameter_prefix}'.");
                tbParamName.Focus();
                return;
            }

            if( ViewModel.Name.Substring(1).ToUpper() == "DesignMode".ToUpper() )
            {
                Error($"'{_parameter_prefix}DesignMode' is a reserved parameter name.");
                tbParamName.Focus();
                return;
            }

            if (ViewModel.Name.Substring(1).ToUpper() == "RowOffset".ToUpper())
            {
                Error($"'{_parameter_prefix}RowOffset' is a reserved parameter name.");
                tbParamName.Focus();
                return;
            }

            if (ViewModel.Name.Substring(1).ToUpper() == "RowLimit".ToUpper())
            {
                Error($"'{_parameter_prefix}RowLimit' is a reserved parameter name.");
                tbParamName.Focus();
                return;
            }

            string csharp_name = ViewModel.Name.Substring(1);
            bool valididentifier = System.CodeDom.Compiler.CodeGenerator.IsValidLanguageIndependentIdentifier(csharp_name);

            if (valididentifier == false)
            {
                Error($"'{csharp_name}' is not a valid C# identifier.");
                tbParamName.Focus();
                return;
            }

            ParameterItem found = _recordset_item.Parameters.FirstOrDefault(z => z.Name.ToLower() == ViewModel.Name.ToLower());

            if (found != null && found.Equals(_edit_parameter_item) == false)
            {
                Error($"Parameter name {ViewModel.Name} already exists.");
                tbParamName.Focus();
                return;
            }


            if (ViewModel.Input == false && ViewModel.Output == false)
            {
                Error($"Neither Input nor Output selected.");
                return;
            }

            ParameterItem item;

            if (_mode == Mode.New)
                item = new ParameterItem(_project);
            else
                item = _edit_parameter_item;

            item.Name = ViewModel.Name;
            item.FullTypename = ViewModel.FullTypename;

            item.Input = ViewModel.Input;
            item.Output = ViewModel.Output;

            item.SetDbType = ViewModel.SetDbType;
            item.SetLength = ViewModel.SetLength;
            item.SetPrecision = ViewModel.SetPrecision;
            item.SetScale = ViewModel.SetScale;

            item.DbTypeString = ViewModel.DbTypeString;
            item.Length = ViewModel.Length;
            item.Scale = ViewModel.Scale;
            item.Precision = ViewModel.Precision;

            item.DesignValue = ViewModel.DesignValue.Trim();

            if (_mode == Mode.New)
                _recordset_item.Parameters.Add(item);

            DialogResult = true;
        }

        private void Error(string message)
        {
            MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void btnSelect_Click(object sender, RoutedEventArgs e)
        {
            DbTypeRepositoryItem item = (DbTypeRepositoryItem)lvTasks.SelectedItem;

            if (item.Group != ParameterGroup.ByDbType)
            {
                DbType? based_on = item.BasedOn;
                if (based_on == null)
                {
                    Error("The selected item does not have the BasedOn property set. The list needs to be fixed.");
                    return;
                }

                // Any item that is not in group ByDbType must be 'BasedOn' an item of the ByDbType group!
                item = _list.First(z => z.Group == ParameterGroup.ByDbType && z.DbType == based_on);
            }

            ViewModel.DbTypeString = "DbType." + item.DbType.ToString();
            ViewModel.FullTypename = item.FrameworkType.FullName;
        }

        public class ViewModelClass : ViewModelBase
        {
            private string _name = "";

            private string _dbtype_string = "";
            private string _fulltypename = "";

            private bool _input = true;
            private bool _output = false;

            private string _designvalue = "";

            private bool _set_dbtype = false;
            private bool _set_length = false;
            private bool _set_precision = false;
            private bool _set_scale = false;

            private int _length = 0;
            private byte _precision = 0;
            private byte _scale = 0;

            public string Name
            {
                get { return _name; }
                set
                {
                    if (_name == value)
                        return;

                    _name = value;

                    NotifyPropertyChanged("Name");
                }
            }

            public string DbTypeString
            {
                get { return _dbtype_string; }
                set
                {
                    if (_dbtype_string == value)
                        return;

                    _dbtype_string = value;

                    NotifyPropertyChanged("DbTypeString");
                    NotifyPropertyChanged("ShortDbTypeString");
                }
            }

            public string ShortDbTypeString
            {
                get
                {
                    int index = _dbtype_string.LastIndexOf('.');

                    string txt = _dbtype_string.Substring(index + 1);

                    return txt;
                }
            }


            public string FullTypename
            {
                get { return _fulltypename; }
                set
                {
                    if (_fulltypename == value)
                        return;

                    _fulltypename = value;

                    NotifyPropertyChanged("FullTypename");
                    NotifyPropertyChanged("FullTypenameInfo");
                }
            }

            public string FullTypenameInfo
            {
                get
                {
                    StringBuilder sb = new StringBuilder();

                    int index = _fulltypename.LastIndexOf('.');

                    sb.Append(_fulltypename.Substring(index + 1));

                    if (TypeTools.TryConvertToCSharpTypeName(_fulltypename, out string csharp_name))
                        sb.Append(" (" + csharp_name + ")");

                    return sb.ToString();
                }
            }

            public bool Input
            {
                get { return _input; }
                set
                {
                    if (_input == value)
                        return;

                    _input = value;

                    NotifyPropertyChanged("Input");
                }
            }

            public bool Output
            {
                get { return _output; }
                set
                {
                    if (_output == value)
                        return;

                    _output = value;

                    NotifyPropertyChanged("Output");
                }
            }

            public string DesignValue
            {
                get { return _designvalue; }
                set
                {
                    if (_designvalue == value)
                        return;

                    _designvalue = value;

                    NotifyPropertyChanged("DesignValue");
                }
            }

            public bool SetDbType
            {
                get { return _set_dbtype; }
                set
                {
                    if (_set_dbtype == value)
                        return;

                    _set_dbtype = value;

                    NotifyPropertyChanged("SetDbType");
                }
            }

            public bool SetLength
            {
                get { return _set_length; }
                set
                {
                    if (_set_length == value)
                        return;

                    _set_length = value;

                    NotifyPropertyChanged("SetLength");
                }
            }

            public bool SetPrecision
            {
                get { return _set_precision; }
                set
                {
                    if (_set_precision == value)
                        return;

                    _set_precision = value;

                    NotifyPropertyChanged("SetPrecision");
                }
            }
            public bool SetScale
            {
                get { return _set_scale; }
                set
                {
                    if (_set_scale == value)
                        return;

                    _set_scale = value;

                    NotifyPropertyChanged("SetScale");
                }
            }


            public int Length
            {
                get { return _length; }
                set
                {
                    if (_length == value)
                        return;

                    _length = value;

                    NotifyPropertyChanged("Length");
                }
            }

            public byte Precision
            {
                get { return _precision; }
                set
                {
                    if (_precision == value)
                        return;

                    _precision = value;

                    NotifyPropertyChanged("Precision");
                }
            }

            public byte Scale
            {
                get { return _scale; }
                set
                {
                    if (_scale == value)
                        return;

                    _scale = value;

                    NotifyPropertyChanged("Scale");
                }
            }

        } // end of viewmodel

    }

    public class EditParameterTasknameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            ParameterGroup status = (ParameterGroup)value;

            if (status == ParameterGroup.ByTask)
                return "Common";
            else if (status == ParameterGroup.ByCodeType)
                return "By .NET Framework Type";
            else if (status == ParameterGroup.ByDbType)
                return "By ADO.NET generic DbType";
            if (status == ParameterGroup.BySqlDbType)
                return "By SQL Server's SqlDbType";

            return "unknown";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

}




