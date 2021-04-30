using System.Collections.Generic;
using System.Windows;
using VenturaSQLStudio.ProjectActions;
using System.Linq;
using System.Collections.ObjectModel;
using System.Windows.Controls;

namespace VenturaSQLStudio.Pages.ProjectItemsPage
{

    public partial class CopyItemWindow : VenturaWindow
    {
        private Project _project;
        private RootItem _clonedrootitem;

        private ObservableCollection<ITreeViewItem> _selected_nodes;

        public CopyItemWindow(Project project, ObservableCollection<ITreeViewItem> selected_nodes)
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
                btnCopy.IsEnabled = false;
            else
                btnCopy.IsEnabled = true;
        }

        private void btnCopy_Click(object sender, RoutedEventArgs e)
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

            int rs_count = 0;

            // Validate folders
            foreach (ITreeViewItem tvi in _selected_nodes)
            {
                FolderItem folder_item = tvi as FolderItem;

                if (folder_item != null)
                {
                    if (folder_item.ContainsChildFolder(target_folder))
                    {
                        string message = $"Cannot copy '{folder_item.Foldername}'. The destination folder is a subfolder of the source folder.";

                        MessageBox.Show(this, message, "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Error);
                        ProjectFoldersTreeview.Focus();
                        return;
                    }
                }

                if (tvi is RecordsetItem)
                    rs_count++;
            }

            // Do not allow the Root folder.
            if (target_folder is RootItem && rs_count > 0)
            {
                string message = $"It is not allowed to copy recordsets to the Root folder.";

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

            // We didn't implement overwrite functionality, just to keep it simple.

            foreach (ITreeViewItem tvi in selected_array)
            {
                RecordsetItem recordset_item = tvi as RecordsetItem;

                if (recordset_item != null)
                {
                    string new_name = recordset_item.ClassName;

                    while (target_folder.ChildItemExists(new_name) == true)
                        new_name = "CopyOf" + new_name;

                    RecordsetItem new_rs = recordset_item.Clone();

                    new_rs.ClassName = new_name;
                    (new_rs as ITreeViewItem).Parent = target_folder;
                    new_rs.IsSelected = true;

                    target_folder.Children.Add(new_rs);
                }

                FolderItem folder_item = tvi as FolderItem;

                if (folder_item != null)
                {
                    string new_name = folder_item.Foldername;

                    while (target_folder.ChildItemExists(new_name) == true)
                        new_name = "CopyOf" + new_name;

                    FolderItem new_folder = new FolderItem(_project, target_folder, new_name);
                    new_folder.IsSelected = true;
                    new_folder.IsExpanded = true;

                    target_folder.Children.Add(new_folder);

                    CopyFolderStructure(folder_item, new_folder);
                }

            }

            _project.FolderStructureWasModified();

            DialogResult = true;

        }

        /// <summary>
        /// Runs recursive 
        /// </summary>
        private void CopyFolderStructure(FolderItem source_folder, FolderItem destination_folder)
        {
            foreach (ITreeViewItem tvi in source_folder.Children)
            {
                FolderItem folder_item = tvi as FolderItem;

                if (folder_item != null)
                {
                    FolderItem new_folder = new FolderItem(_project, destination_folder, folder_item.Foldername);
                    new_folder.IsSelected = true;
                    new_folder.IsExpanded = true;

                    destination_folder.Children.Add(new_folder);

                    CopyFolderStructure(folder_item, new_folder);
                }

                RecordsetItem recordset_item = tvi as RecordsetItem;

                if (recordset_item != null)
                {
                    string new_name = recordset_item.ClassName;

                    RecordsetItem new_rs = recordset_item.Clone();
                    new_rs.ClassName = new_name;
                    (new_rs as ITreeViewItem).Parent = destination_folder;
                    new_rs.IsSelected = true;

                    destination_folder.Children.Add(new_rs);
                }

            }
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
