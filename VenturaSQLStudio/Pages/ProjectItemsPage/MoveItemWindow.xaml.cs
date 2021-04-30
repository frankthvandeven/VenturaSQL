using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace VenturaSQLStudio.Pages.ProjectItemsPage
{

    public partial class MoveItemWindow : VenturaWindow
    {
        private Project _project;
        private RootItem _clonedrootitem;

        private ObservableCollection<ITreeViewItem> _selected_nodes;

        public MoveItemWindow(Project project, ObservableCollection<ITreeViewItem> selected_nodes)
        {
            _project = project;
            _selected_nodes = selected_nodes;

            InitializeComponent();

            _clonedrootitem = CloneTree();

            ProjectFoldersTreeview.TreeViewControl.SelectionMode = SelectionMode.Single;

            ProjectFoldersTreeview.SelectedNodes.CollectionChanged += SelectedNodes_CollectionChanged;

            ProjectFoldersTreeview.RootItem = _clonedrootitem;

            ProjectFoldersTreeview.Focus();
        }

        private void SelectedNodes_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            if (ProjectFoldersTreeview.SelectedNodes.Count == 0)
                btnMove.IsEnabled = false;
            else
                btnMove.IsEnabled = true;
        }

        private void btnMove_Click(object sender, RoutedEventArgs e)
        {
            FolderItem cloned_target_folder = (FolderItem)ProjectFoldersTreeview.SelectedNodes[0];
            string path = cloned_target_folder.CalculatePath();
            FolderItem target_folder = _project.FolderStructure.FetchOrCreateFolderItem(path);

            // Do not allow the Auto Create Recordsets folder.
            if (path.ToLower() == MainWindow.ViewModel.CurrentProject.AutoCreateSettings.Folder.ToLower())
            {
                string message = $"You cannot select folder {target_folder.Foldername} as the destination folder.\n\nThe folder is set as the Auto Create Recordsets folder.\n\nThe folder is emptied when running Auto Create.";

                MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                ProjectFoldersTreeview.Focus();
                return;
            }

            // Validate folders
            foreach (ITreeViewItem tvi in _selected_nodes)
            {
                FolderItem folder_item = tvi as FolderItem;

                if (folder_item != null)
                {
                    if (target_folder.Children.Contains(folder_item) == true || target_folder.Equals(folder_item))
                    {
                        string message = $"Cannot move folder '{folder_item.Foldername}'. The destination folder is the same as the source folder.";

                        MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                        ProjectFoldersTreeview.Focus();
                        return;
                    }

                    if (folder_item.ContainsChildFolder(target_folder))
                    {
                        string message = $"Cannot move folder '{folder_item.Foldername}'. The destination folder is a subfolder of the source folder.";

                        MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                        ProjectFoldersTreeview.Focus();
                        return;
                    }

                    if (target_folder.ChildItemExists(folder_item.Foldername) == true)
                    {
                        string message = $"Cannot move folder '{folder_item.Foldername}'. The destination folder already contains a folder with the same name.";

                        MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                        ProjectFoldersTreeview.Focus();
                        return;
                    }
                }

            }

            int rs_count = 0;

            // Validate recordsets
            foreach (ITreeViewItem tvi in _selected_nodes)
            {
                RecordsetItem recordset_item = tvi as RecordsetItem;

                if (recordset_item != null)
                {
                    rs_count++;

                    if (target_folder.Children.Contains(recordset_item) == true)
                    {
                        string message = $"Cannot move Recordset '{recordset_item.ClassName}'. The destination folder is the same as the source folder.";

                        MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                        ProjectFoldersTreeview.Focus();
                        return;
                    }

                    if (target_folder.ChildItemExists(recordset_item.ClassName) == true)
                    {
                        string message = $"Cannot move Recordset '{recordset_item.ClassName}'. The destination folder already contains a Recordset with the same name.";

                        MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                        ProjectFoldersTreeview.Focus();
                        return;
                    }
                }
            }

            // Do not allow the Root folder.
            if (target_folder is RootItem && rs_count > 0)
            {
                string message = $"It is not allowed to move recordsets to the Root folder.";

                MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                ProjectFoldersTreeview.Focus();
                return;
            }

            // Validation completed

            target_folder.ExpandBubbleUp();

            // Copy all items to an array as the SelectedNodes collection will change due
            // to unselecting items while looping

            ITreeViewItem[] selected_array = new ITreeViewItem[_selected_nodes.Count];

            _selected_nodes.CopyTo(selected_array, 0);

            // Loop all selected folders and unselect any file or folder below

            foreach (ITreeViewItem item in selected_array)
            {
                FolderItem folder_item = item as FolderItem;

                if (folder_item != null)
                    folder_item.UnselectAllChildren();
            }

            // Copy the array again

            selected_array = new ITreeViewItem[_selected_nodes.Count];

            _selected_nodes.CopyTo(selected_array, 0);

            // clear the selection
            _project.FolderStructure.UnselectAll();

            // Move the selected items one by one by unhooking them, and re-attaching...

            foreach (ITreeViewItem tvi in selected_array)
            {
                bool result = tvi.Parent.Children.Remove(tvi); // unhook

                if (result == false)
                    MessageBox.Show("No child object to remove. Should not happen.");

                // Re-parent
                tvi.Parent = target_folder; // re-parent

                RecordsetItem recordset_item = tvi as RecordsetItem;

                if (recordset_item != null)
                    target_folder.Children.Add(recordset_item); // re-attach

                FolderItem folder_item = tvi as FolderItem;

                if (folder_item != null)
                    target_folder.Children.Add(folder_item);

            }

            //target_folder.IsSelected = true;

            _project.FolderStructureWasModified();

            DialogResult = true;
        }

        private RootItem CloneTree()
        {
            RootItem clonedrootitem = new RootItem(null);
            clonedrootitem.IsExpanded = true;

            List<RootItem.FolderListItem> folderlist = _project.FolderStructure.GetFolderList();

            foreach (RootItem.FolderListItem listitem in folderlist)
            {
                FolderItem clonedfolder = clonedrootitem.FetchOrCreateFolderItem(listitem.OutputFolder);
                clonedfolder.IsExpanded = true;
            }

            return clonedrootitem;
        }

    }
}
