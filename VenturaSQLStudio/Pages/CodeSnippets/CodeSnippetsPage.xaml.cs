using ICSharpCode.AvalonEdit.Highlighting;
using ICSharpCode.AvalonEdit.Search;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using VenturaSQLStudio.Ado;
using VenturaSQL;
using VenturaSQLStudio.Progress;

namespace VenturaSQLStudio.Pages
{


    public partial class CodeSnippetsPage : UserControl
    {
        private RecordsetItem _recordsetitem;
        private QueryInfo _queryinfo;
        private ResultSetInfo _resultset_info;
        private ResultsetItem _resultset_item;
        private VenturaSchema _schema;

        // The list for the combobox.
        private List<string> _resultsetnames = new List<string>();
        private List<SnippetListItem> _snippet_list = new List<SnippetListItem>();

        private string _recordset_typename;
        private string _resultset_typename;
        private string _record_typename;

        private ObservableCollection<ColumnListItem> _columns = new ObservableCollection<ColumnListItem>();

        public CodeSnippetsPage(RecordsetItem recordsetitem)
        {
            _recordsetitem = recordsetitem.Clone();

            InitializeComponent();

            AvalonEditControl.Options.EnableHyperlinks = false;

            SearchPanel.Install(AvalonEditControl);
        }

        private bool _isloaded = false;
        
        private void FillSnippetList()
        {
            _snippet_list.Add(new SnippetListItem
            {
                Title = "ColumnCollection (C#) and HyperGrid definition (Razor) for Kenova (Blazor WebAssembly).",
                DoCreate = () => new SnippetHyperGridWebassembly()
            });

            _snippet_list.Add(new SnippetListItem
            {
                Title = "Model class for Kenova (Blazor WebAssembly).",
                DoCreate = () => new SnippetKenovaModel()
            }); ;

            _snippet_list.Add(new SnippetListItem
            {
                Title = "Viewmodel class.",
                DoCreate = () => new SnippetViewmodel()
            });

            _snippet_list.Add(new SnippetListItem
            {
                Title = "Initialize Viewmodel properties with default values.",
                DoCreate = () => new SnippetViewmodelDefaults()
            });

            _snippet_list.Add(new SnippetListItem
            {
                Title = "Recordset to Viewmodel.",
                DoCreate = () => new SnippetRecordsetToViewmodel()
            });

            _snippet_list.Add(new SnippetListItem
            {
                Title = "Viewmodel to Recordset.",
                DoCreate = () => new SnippetViewmodelToRecordset()
            });

            _snippet_list.Add(new SnippetListItem
            {
                Title = "XAML definition for VenturaSQL's UWP HyperGrid.",
                DoCreate = () => new SnippetHyperGrid()
            });

            _snippet_list.Add(new SnippetListItem
            {
                Title = "XAML definition for VenturaSQL's UWP Form.",
                DoCreate = () => new SnippetForm()
            });

            _snippet_list.Add(new SnippetListItem
            {
                Title = "List of columns.",
                DoCreate = () => new SnippetListOfColumns()
            });

            _snippet_list.Add(new SnippetListItem
            {
                Title = "List of columns enclosed in quotes.",
                DoCreate = () => new SnippetListOfQuotedColumns()
            });

        }


        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if (_isloaded == true)
                return;

            _isloaded = true;

            Action<BackgroundWorker, DoWorkEventArgs> action = (bw, we) => DoWork();

            ProgressDialogResult result1 = ProgressDialog.Execute(Application.Current.MainWindow, "Retrieving raw schema...", action);

            if (result1.Error != null)
            {
                MessageBox.Show("Retrieving raw schema failed. " + result1.Error.Message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);

                MainWindow window = (MainWindow)Application.Current.MainWindow;
                window.CloseTabContainingPage(this);

                return;
            }

            FillSnippetList();

            comboSnippets.ItemsSource = _snippet_list;
            comboSnippets.SelectedIndex = 0;

            int index = 0;

            // Fill the list for the dropdown combobox.
            foreach (ResultSetInfo info in _queryinfo.ResultSets)
            {
                ResultsetItem item = _recordsetitem.Resultsets[index];

                StringBuilder sb = new StringBuilder();

                sb.Append($"Resultset {index + 1}");

                if (item.UpdateableTableName == null)
                    sb.Append(" (read-only)");
                else
                    sb.Append(" " + item.UpdateableTableName.ScriptTableName);

                _resultsetnames.Add(sb.ToString());
                index++;
            }

            cbResultsets.ItemsSource = _resultsetnames;
            cbResultsets.SelectionChanged += CbResultsets_SelectionChanged;
            cbResultsets.SelectedIndex = 0;

            if (_queryinfo.ResultSets.Count < 2)
            {
                // One resultset.
                stackpanelResultsets.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Two or more resultsets.
                stackpanelResultsets.Visibility = Visibility.Visible;
            }

            AvalonEditControl.Text = "// Code snippets will be displayed here.";
        }

        // Called when another item in the list of resultsets is selected.
        private void CbResultsets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int index = cbResultsets.SelectedIndex;

            _resultset_info = _queryinfo.ResultSets[index];
            _resultset_item = _recordsetitem.Resultsets[index];

            ColumnArrayBuilder builder = new ColumnArrayBuilder();
            builder.Add(_resultset_info, _resultset_item.UpdateableTableName);

            _schema = new VenturaSchema(builder);

            lvColumns.ItemsSource = null;

            _columns.Clear();

            for (int x = 0; x < _schema.Count; x++)
            {
                VenturaColumn schema_column = _schema[x];
                _columns.Add(new ColumnListItem(schema_column.ColumnName, schema_column, null));
            }

            foreach (var udc_item in _recordsetitem.UserDefinedColumns)
            {
                _columns.Add(new ColumnListItem(udc_item.ColumnName, null, udc_item));
            }

            lvColumns.ItemsSource = _columns;

            if (_queryinfo.ResultSets.Count == 1) /* Generate a Recordset with Exec/Param/SqlScript/SaveChanges inside it */
            {
                _recordset_typename = _recordsetitem.ClassName;
                _resultset_typename = _recordsetitem.ClassName;
                _record_typename = $"{StudioGeneral.NewStripLast9(_recordsetitem.ClassName)}Record";
            }
            else
            {
                string resultsetname = "Multi" + _recordsetitem.Resultsets[index].ResultsetName;
                _recordset_typename = _recordsetitem.ClassName;
                _resultset_typename = resultsetname;
                _record_typename = resultsetname + "Record";
            }


            // What data is available?
            // 1. _resultset_info
            // 2. _resultset_item
            // 3. _schema
            // 4. _columns 
            // a. _recordsetitem
            // b. _queryinfo
        }

        /// <summary>
        /// Running on a separate thread... don't try to update the GUI from within this function.
        /// </summary>
        private void DoWork()
        {
            _queryinfo = QueryInfo.CreateInstance(_recordsetitem);
        }

        private void SelectAll_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in _columns)
                col.Include = true;
        }

        private void SelectNone_Click(object sender, RoutedEventArgs e)
        {
            foreach (var col in _columns)
                col.Include = false;
        }

        private void comboSnippets_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            SnippetListItem list_item = _snippet_list[comboSnippets.SelectedIndex];

            SnippetCreatorBase creator = list_item.DoCreate();

            panelRecordsetVariable.Visibility = creator.UsesParameter_RecordsetVariable ? Visibility.Visible : Visibility.Collapsed;
            panelViewmodelVariable.Visibility = creator.UsesParameter_ViewmodelVariable ? Visibility.Visible : Visibility.Collapsed;
            //checkboxIncludeUDCs.Visibility = creator.UsesParameter_IncludeUDCs ? Visibility.Visible : Visibility.Collapsed;
        }

        private void Create_Click(object sender, RoutedEventArgs e)
        {
            if (panelRecordsetVariable.Visibility == Visibility.Visible && textboxRecordsetVariable.Text.Trim() == "")
            {
                MessageBox.Show("No recordset variable specified.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                textboxRecordsetVariable.Focus();
                return;
            }

            if (panelViewmodelVariable.Visibility == Visibility.Visible && textboxViewmodelVariable.Text.Trim() == "")
            {
                textboxViewmodelVariable.Focus();
                MessageBox.Show("No viewmodel variable specified.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            List<VenturaColumn> selected_database_columns = new List<VenturaColumn>();

            foreach (var item in _columns)
                if (item.Include && item.SchemaColumn != null)
                    selected_database_columns.Add(item.SchemaColumn);

            List<UDCItem> selected_udc_columns = new List<UDCItem>();

            foreach (var item in _columns)
                if (item.Include && item.UDC_Column != null)
                    selected_udc_columns.Add(item.UDC_Column);

            if (selected_database_columns.Count == 0 && selected_udc_columns.Count == 0)
            {
                MessageBox.Show("No columns selected.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            SnippetListItem list_item = _snippet_list[comboSnippets.SelectedIndex];

            SnippetCreatorBase creator = list_item.DoCreate();

            creator.SelectedColumns = selected_database_columns;
            creator.Selected_UDC_Columns = selected_udc_columns;

            creator.RecordsetVariable = textboxRecordsetVariable.Text.Trim();
            creator.ViewmodelVariable = textboxViewmodelVariable.Text.Trim();

            creator.RecordsetTypename = _recordset_typename;
            creator.ResultsetTypename = _resultset_typename;
            creator.RecordTypeName = _record_typename;

            var typeConverter = new HighlightingDefinitionTypeConverter();

            AvalonEditControl.SyntaxHighlighting = (IHighlightingDefinition)typeConverter.ConvertFrom(creator.SyntaxHighlighting);
            AvalonEditControl.Text = creator.CreateCode();
        }

        private void CopyClipboard_Click(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(AvalonEditControl.Text);
        }
    }
}


//sb.Append(PRE + TAB + TAB + TAB + $"schema_array.Add(new VenturaColumn(\"{column.ColumnName}\", typeof({column.ShortTypeNameAsCSharpString()}), {column.IsNullable.ToString().ToLower()})" + CRLF);
//sb.Append(PRE + TAB + TAB + TAB + "{" + CRLF);
//if (column.Updateable == true) list.Add($"Updateable = {column.Updateable.ToString().ToLower()}");
//if (column.DbType != null) list.Add($"DbType = DbType.{column.DbType}");
//if (column.ColumnSize != null) list.Add($"ColumnSize = {column.ColumnSize}");
//if (column.NumericPrecision != null) list.Add($"NumericPrecision = {column.NumericPrecision}");
//if (column.NumericScale != null) list.Add($"NumericScale = {column.NumericScale}");
//if (column.ProviderType != 0 && AdoDirectSwitch) list.Add($"ProviderType = {column.ProviderType}");
//if (column.IsUnique == true) list.Add($"IsUnique = {column.IsUnique.ToString().ToLower()}");
//if (column.IsKey == true) list.Add($"IsKey = {column.IsKey.ToString().ToLower()}");
//if (column.IsAliased == true) list.Add($"IsAliased = {column.IsAliased.ToString().ToLower()}");
//if (column.IsExpression == true) list.Add($"IsExpression = {column.IsExpression.ToString().ToLower()}");
//if (column.IsIdentity == true) list.Add($"IsIdentity = {column.IsIdentity.ToString().ToLower()}");
//if (column.IsAutoIncrement == true) list.Add($"IsAutoIncrement = {column.IsAutoIncrement.ToString().ToLower()}");
//if (column.IsRowGuid == true) list.Add($"IsRowGuid = {column.IsRowGuid.ToString().ToLower()}");
//if (column.IsLong == true) list.Add($"IsLong = {column.IsLong.ToString().ToLower()}");
//if (column.IsReadOnly == true) list.Add($"IsReadOnly = {column.IsReadOnly.ToString().ToLower()}");
//if (column.XmlSchemaCollectionDatabase.Length > 0 && AdoDirectSwitch) list.Add($"XmlSchemaCollectionDatabase = \"{column.XmlSchemaCollectionDatabase}\"");
//if (column.XmlSchemaCollectionOwningSchema.Length > 0 && AdoDirectSwitch) list.Add($"XmlSchemaCollectionOwningSchema = \"{column.XmlSchemaCollectionOwningSchema}\"");
//if (column.XmlSchemaCollectionName.Length > 0 && AdoDirectSwitch) list.Add($"XmlSchemaCollectionName = \"{column.XmlSchemaCollectionName}\"");
//if (column.UdtAssemblyQualifiedName.Length > 0 && AdoDirectSwitch) list.Add($"UdtAssemblyQualifiedName = \"{column.UdtAssemblyQualifiedName}\"");
//if (column.BaseServerName.Length > 0 && AdoDirectSwitch) list.Add($"BaseServerName = \"{column.BaseServerName}\"");
//if (column.BaseCatalogName.Length > 0 && AdoDirectSwitch) list.Add($"BaseCatalogName = \"{column.BaseCatalogName}\"");
//if (column.BaseSchemaName.Length > 0 && AdoDirectSwitch) list.Add($"BaseSchemaName = \"{column.BaseSchemaName}\"");
//if (column.BaseTableName.Length > 0 && AdoDirectSwitch) list.Add($"BaseTableName = \"{column.BaseTableName}\"");
//if (column.BaseColumnName.Length > 0 && AdoDirectSwitch) list.Add($"BaseColumnName = \"{column.BaseColumnName}\"");
