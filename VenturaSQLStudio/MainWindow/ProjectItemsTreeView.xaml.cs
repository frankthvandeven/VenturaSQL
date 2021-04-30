using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace VenturaSQLStudio.UserControls
{
    // About this control:
    // - This is a TreeView for displaying the Project's Folder tree.
    // - It displays a RootItem, multiple FolderItem and multiple RecordsetItem items. 
    // - It allows multi-select. The collection of selected items will be updated instantly.
    // - It keeps statistics about the number of FolderItems and RecordsetItem objects that are selected.
    // - It has NO context menu.
    // - This control is used in the Project Explorer and the Move and Copy dialogs.

    // Using the TreeView from a ViewModel. This is the most important example!
    // http://www.codeproject.com/Articles/26288/Simplifying-the-WPF-TreeView-by-Using-the-ViewMode

    // Learn all about the WPF TreeView:
    // http://www.codeproject.com/Articles/124644/Basic-Understanding-of-Tree-View-in-WPF

    // Switch template dynamically to make it editable:
    // http://stackoverflow.com/questions/9580583/editable-treeview-node-using-templates-and-styles

    /// <summary>
    /// Interaction logic for ProjectItemsTreeView.xaml
    /// </summary>
    public partial class ProjectItemsTreeView : UserControl
    {
        public delegate void ItemDoubleClickedEventHandler(object sender, RecordsetItem e);
        public delegate void GetContextMenuForRecordsetItemEventHandler(object sender, GetContextMenuForRecordsetItemEventArgs e);
        public delegate void GetContextMenuForFolderItemEventHandler(object sender, GetContextMenuForFolderItemEventArgs e);
        public delegate void GetContextMenuForRootItemEventHandler(object sender, GetContextMenuForRootItemEventArgs e);

        public event ItemDoubleClickedEventHandler ItemDoubleClicked;
        public event GetContextMenuForRecordsetItemEventHandler GetContextMenuForRecordsetItem;
        public event GetContextMenuForFolderItemEventHandler GetContextMenuForFolderItem;
        public event GetContextMenuForRootItemEventHandler GetContextMenuForRootItem;

        // This (EventHandler<>) will cause a problem after Obfuscation with both Obfuscar and Dotfuscator.
        // The problem starts with the XAML defining the handler. The obfuscators will rename the
        // (private) eventhandler in the C# code, and then XAML cannot find it.
        //
        //public event EventHandler<RecordsetItem> ItemDoubleClicked;
        //public event EventHandler<GetContextMenuForRecordsetItemEventArgs> GetContextMenuForRecordsetItem;
        //public event EventHandler<GetContextMenuForFolderItemEventArgs> GetContextMenuForFolderItem;

        private ObservableCollection<ITreeViewItem> _selectedNodes;

        private int _root_items_count;
        private int _folder_items_count;
        private int _recordset_items_count;

        public ProjectItemsTreeView()
        {
            _selectedNodes = new ObservableCollection<ITreeViewItem>();
            _selectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;

            _root_items_count = 0;
            _folder_items_count = 0;
            _recordset_items_count = 0;

            InitializeComponent();

            TreeViewControl.ItemsSource = null;
        }

        #region Selected Nodes functionality

        public int SelectedRootItemsCount
        {
            get { return _root_items_count; }
        }

        public int SelectedFolderItemsCount
        {
            get { return _folder_items_count; }
        }

        public int SelectedRecordsetItemsCount
        {
            get { return _recordset_items_count; }
        }

        public ObservableCollection<T> GetAllSelectedItems<T>()
        {
            ObservableCollection<T> items = new ObservableCollection<T>();

            foreach (object item in _selectedNodes)
                if (item is T)
                    items.Add((T)item);

            return items;
        }

        private void SelectedNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            _root_items_count = 0;
            _folder_items_count = 0;
            _recordset_items_count = 0;

            foreach (object item in _selectedNodes)
            {
                if (item is RootItem)
                    _root_items_count++;
                else if (item is FolderItem)
                    _folder_items_count++;
                else if (item is RecordsetItem)
                    _recordset_items_count++;
            }

            //NumberOfSelectedNodes.Text = $"ROO {_root_items_count} FOL {_folder_items_count} REC {_recordset_items_count}";
        }

        public ObservableCollection<ITreeViewItem> SelectedNodes
        {
            get { return _selectedNodes; }
        }

        #endregion

        private void TreeViewExItem_MouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            // Begin: Detect repeated call due to bubbling up.

            DependencyObject dep_object = e.OriginalSource as DependencyObject;

            if (dep_object == null)
                return;

            if (sender != this.GetVisualParent(dep_object, typeof(TreeViewExItem)))
                return;

            // ...sender is the node, which was directly doubleclicked

            // End: Detect repeated call due to bubbling up.

            TreeViewExItem tvi = sender as TreeViewExItem;

            RecordsetItem recordset_item = tvi.DataContext as RecordsetItem;

            if (recordset_item == null)
                return;

            if (ItemDoubleClicked != null)
                ItemDoubleClicked(this, recordset_item);
        }

        public new bool Focus()
        {
            return TreeViewControl.Focus();
        }

        #region Dependency Properties

        public RootItem RootItem
        {
            get { return (RootItem)GetValue(RootItemProperty); }
            set { SetValue(RootItemProperty, value); }
        }

        public static readonly DependencyProperty RootItemProperty =
            DependencyProperty.Register("RootItem", typeof(RootItem), typeof(ProjectItemsTreeView), new PropertyMetadata(null, new PropertyChangedCallback(OnRootItemValueChanged)));

        private static void OnRootItemValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProjectItemsTreeView self = (ProjectItemsTreeView)d;

            self._selectedNodes.Clear();

            RootItem rootitem = (RootItem)e.NewValue;

            if (rootitem == null)
                self.TreeViewControl.ItemsSource = null;
            else
                self.TreeViewControl.ItemsSource = rootitem;

        }

        #endregion

        //private TreeViewExItem VisualUpwardSearch(DependencyObject source)
        //{
        //    while (source != null && !(source is TreeViewExItem))
        //        source = VisualTreeHelper.GetParent(source);

        //    return source as TreeViewExItem;
        //}

        private void TreeViewExItem_MouseRightButtonUp(object sender, MouseButtonEventArgs e)
        {
            // Begin: Detect repeated call due to bubbling up.

            DependencyObject dep_object = e.OriginalSource as DependencyObject;

            if (dep_object == null)
                return;

            if (sender != this.GetVisualParent(dep_object, typeof(TreeViewExItem)))
                return;

            // ...sender is the node, which was directly doubleclicked

            // End: Detect repeated call due to bubbling up.

            TreeViewExItem tvi = sender as TreeViewExItem;

            if (tvi == null)
                return;

            this.SelectedNodes.Clear();
            tvi.IsSelected = true;

            ITreeViewItem tvm = tvi.DataContext as ITreeViewItem;

            if (tvm == null)
                return;

            if (tvm.ItemKind == TreeViewModelKind.RecordsetItem && GetContextMenuForRecordsetItem != null)
            {
                var args = new GetContextMenuForRecordsetItemEventArgs(tvi, (RecordsetItem)tvm);
                GetContextMenuForRecordsetItem(this, args);

                tvi.ContextMenu = args.ContextMenu;
            }

            if (tvm.ItemKind == TreeViewModelKind.FolderItem && GetContextMenuForFolderItem != null)
            {
                var args = new GetContextMenuForFolderItemEventArgs(tvi, (FolderItem)tvm);
                GetContextMenuForFolderItem(this, args);

                tvi.ContextMenu = args.ContextMenu;
            }

            if (tvm.ItemKind == TreeViewModelKind.RootItem && GetContextMenuForRootItem != null)
            {
                var args = new GetContextMenuForRootItemEventArgs(tvi, (RootItem)tvm);
                GetContextMenuForRootItem(this, args);

                tvi.ContextMenu = args.ContextMenu;
            }

        }

        private object GetVisualParent(DependencyObject obj, Type expectedType)
        {
            var parent = VisualTreeHelper.GetParent(obj);
            while (parent != null && parent.GetType() != expectedType)
                parent = VisualTreeHelper.GetParent(parent);

            return parent;
        }

    }

    public class GetContextMenuForRecordsetItemEventArgs : EventArgs
    {
        private readonly TreeViewExItem _tvi;
        private readonly RecordsetItem _item;
        private ContextMenu _contextmenu;

        public GetContextMenuForRecordsetItemEventArgs(TreeViewExItem tvi, RecordsetItem item)
        {
            _tvi = tvi;
            _item = item;
            _contextmenu = null;
        }

        public TreeViewExItem TreeViewExItem
        {
            get { return _tvi; }
        }

        public RecordsetItem RecordsetItem
        {
            get { return _item; }
        }

        public ContextMenu ContextMenu
        {
            get { return _contextmenu; }
            set { _contextmenu = value; }
        }
    }

    public class GetContextMenuForFolderItemEventArgs : EventArgs
    {
        private readonly TreeViewExItem _tvi;
        private readonly FolderItem _item;
        private ContextMenu _contextmenu;

        public GetContextMenuForFolderItemEventArgs(TreeViewExItem tvi, FolderItem item)
        {
            _tvi = tvi;
            _item = item;
            _contextmenu = null;
        }

        public TreeViewExItem TreeViewExItem
        {
            get { return _tvi; }
        }

        public FolderItem FolderItem
        {
            get { return _item; }
        }

        public ContextMenu ContextMenu
        {
            get { return _contextmenu; }
            set { _contextmenu = value; }
        }

    }

    public class GetContextMenuForRootItemEventArgs : EventArgs
    {
        private readonly TreeViewExItem _tvi;
        private readonly RootItem _item;
        private ContextMenu _contextmenu;

        public GetContextMenuForRootItemEventArgs(TreeViewExItem tvi, RootItem item)
        {
            _tvi = tvi;
            _item = item;
            _contextmenu = null;
        }

        public TreeViewExItem TreeViewExItem
        {
            get { return _tvi; }
        }

        public RootItem RootItem
        {
            get { return _item; }
        }

        public ContextMenu ContextMenu
        {
            get { return _contextmenu; }
            set { _contextmenu = value; }
        }

    }

}