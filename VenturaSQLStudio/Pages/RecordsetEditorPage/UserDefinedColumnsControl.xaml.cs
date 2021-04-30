using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace VenturaSQLStudio.Pages.RecordsetEditorPage
{
    /// <summary>
    /// Interaction logic for UserDefinedColumnsControl.xaml
    /// </summary>
    public partial class UserDefinedColumnsControl : UserControl
    {
        private Project _project;
        private RecordsetItem _item;
        
        public UserDefinedColumnsControl(Project project, RecordsetItem item)
        {
            _project = project;
            _item = item;

            this.DataContext = item;

            InitializeComponent();

            btnMoveUp.Command = new UserDefinedColumnsControl.MoveUpCommand(listView, _item.UserDefinedColumns);
            btnMoveDown.Command = new UserDefinedColumnsControl.MoveDownCommand(listView, _item.UserDefinedColumns);

            listView.ItemsSource = item.UserDefinedColumns;

            textblockInfo.Text = "User defined columns can be used to store data in a row temporarily. " +
                "The data is not transmitted to the server. The columns are generated as nullable.";

        }

        private void CommandNew_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            _item.UserDefinedColumns.Add(new UDCItem(_project) { ColumnName = "", FullTypename = "System.String" });

            // Focus the new row
            listView.SelectedIndex = _item.UserDefinedColumns.Count - 1;
        }

        private void CommandNew_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {

            foreach (UDCItem colitem in _item.UserDefinedColumns)
            {
                if (colitem.ColumnName.Trim().Length == 0)
                {
                    e.CanExecute = false;
                    return;
                }
            }

            e.CanExecute = true;
        }

        private void CommandDelete_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            int selectedindex = listView.SelectedIndex;

            _item.UserDefinedColumns.Remove((UDCItem)listView.SelectedItem);

            int itemcount = _item.UserDefinedColumns.Count;
            int newindex;

            if (selectedindex >= itemcount)
                newindex = itemcount - 1;
            else
                newindex = selectedindex;

            listView.SelectedIndex = newindex;
        }

        private void CommandDelete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (listView == null)
                return;

            e.CanExecute = (listView.SelectedItem != null);
        }

        private void TextBox_Loaded(object sender, RoutedEventArgs e)
        {
            TextBox textbox = sender as TextBox;

            if (textbox == null)
                return;

            textbox.Focus();
        }

        public class MoveUpCommand : ICommand
        {
            private ListView _listview;
            ObservableCollection<UDCItem> _collection;

            public MoveUpCommand(ListView listview, ObservableCollection<UDCItem> collection)
            {
                _listview = listview;
                _collection = collection;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter)
            {
                if (_listview.SelectedIndex == -1)
                    return false;

                return _listview.SelectedIndex > 0;
            }

            public void Execute(object parameter)
            {
                var selectedIndex = _listview.SelectedIndex;

                var itemToMoveUp = _collection[selectedIndex];
                _collection.RemoveAt(selectedIndex);
                _collection.Insert(selectedIndex - 1, itemToMoveUp);
                _listview.SelectedIndex = selectedIndex - 1;
            }
        }

        public class MoveDownCommand : ICommand
        {
            private ListView _listview;
            ObservableCollection<UDCItem> _collection;

            public MoveDownCommand(ListView listview, ObservableCollection<UDCItem> collection)
            {
                _listview = listview;
                _collection = collection;
            }

            public event EventHandler CanExecuteChanged
            {
                add { CommandManager.RequerySuggested += value; }
                remove { CommandManager.RequerySuggested -= value; }
            }

            public bool CanExecute(object parameter)
            {
                if (_listview.SelectedIndex == -1)
                    return false;

                return (_listview.SelectedIndex + 1) < _collection.Count;
            }

            public void Execute(object parameter)
            {
                var selectedIndex = _listview.SelectedIndex;

                var itemToMoveDown = _collection[selectedIndex];
                _collection.RemoveAt(selectedIndex);
                _collection.Insert(selectedIndex + 1, itemToMoveDown);
                _listview.SelectedIndex = selectedIndex + 1;
            }
        }

        private void TextBox_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Return)
            {

                //e.Handled = true;
            }
        }
    }
}
