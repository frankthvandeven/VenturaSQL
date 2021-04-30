using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using VenturaSQLStudio.Pages.ProjectItemsPage;
using VenturaSQLStudio.AutoCreate;

// Sorting a list on multiple columns:
// samples = samples.OrderBy(a => a.SortOrder).ThenBy(a => a.Description).ToList();

// populate treeview from path string:
// http://stackoverflow.com/questions/1155977/populate-treeview-from-a-list-of-path

namespace VenturaSQLStudio {
    public partial class ProjectItemsControl : UserControl
    {
        public ProjectItemsControl()
        {
            InitializeComponent();
        }

        //private werd public voor Obfuscator!
        public void ProjectItemsTreeview_ItemDoubleClicked(object sender, RecordsetItem e)
        {
            if (e is RecordsetItem)
                OpenEditRecordsetTab(e);
        }

        private void ToggleCheckboxCommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        private void ToggleCheckboxCommandBinding_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MainWindow.ViewModel.ProjectItemsCheckBoxVisible = !MainWindow.ViewModel.ProjectItemsCheckBoxVisible;
        }

        private void DeleteCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ProjectItemsTreeview == null)
                return;

            if (ProjectItemsTreeview.SelectedRootItemsCount > 0)
                return;

            if (ProjectItemsTreeview.SelectedFolderItemsCount > 0 || ProjectItemsTreeview.SelectedRecordsetItemsCount > 0)
                e.CanExecute = true;
        }

        private void DeleteCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            string message = "The selected items will be deleted permanently.";

            if (ProjectItemsTreeview.SelectedNodes.Count == 1)
            {
                string name = ProjectItemsTreeview.SelectedNodes[0].Name;

                if (ProjectItemsTreeview.SelectedFolderItemsCount == 1)
                    message = $"'{name}' and all its contents will be deleted permanently.";

                if (ProjectItemsTreeview.SelectedRecordsetItemsCount == 1)
                    message = $"'{name}' will be deleted permanently.";
            }

            MessageBoxResult result = MessageBox.Show(message, "VenturaSQL Studio", MessageBoxButton.OKCancel, MessageBoxImage.Warning, MessageBoxResult.OK, MessageBoxOptions.None);

            if (result != MessageBoxResult.OK)
                return;

            // Copy all items to an array as the SelectedNodes collection will change due
            // to unselecting items while looping

            var selected_nodes = ProjectItemsTreeview.SelectedNodes;

            ITreeViewItem[] selected_array = new ITreeViewItem[selected_nodes.Count];

            selected_nodes.CopyTo(selected_array, 0);

            // Loop all selected folders and unselect any file or folder below

            foreach (ITreeViewItem item in selected_array)
            {
                FolderItem folder_item = item as FolderItem;

                if (folder_item != null)
                    folder_item.UnselectAllChildren();
            }

            // Copy the array again

            selected_array = new ITreeViewItem[selected_nodes.Count];

            selected_nodes.CopyTo(selected_array, 0);

            MainWindow window = (MainWindow)Application.Current.MainWindow;

            foreach (ITreeViewItem tvi in selected_array)
            {
                if (tvi is RecordsetItem)
                {
                    string uniqueid = $"RS {tvi.GetHashCode()}";
                    window.CloseTab(uniqueid);
                    tvi.Parent.Children.Remove(tvi);
                }

                if (tvi is FolderItem)
                {
                    List<ITreeViewItem> projectitemlist = (tvi as FolderItem).AllProjectItemsInThisFolderAndSubfolders();

                    foreach (ITreeViewItem singleprojectitem in projectitemlist)
                        window.CloseTab($"RS {singleprojectitem.GetHashCode()}");

                    tvi.Parent.Children.Remove(tvi);
                }

            }

            // clear the selection
            this.Project.FolderStructure.UnselectAll();

            // Calling Focus() solves the grayed out toolbar buttons problem.
            ProjectItemsTreeview.Focus();
        }

        private void RenameCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ProjectItemsTreeview == null)
                return;

            if (ProjectItemsTreeview.SelectedNodes.Count != 1)
                return;

            if (ProjectItemsTreeview.SelectedFolderItemsCount == 1 || ProjectItemsTreeview.SelectedRecordsetItemsCount == 1)
                e.CanExecute = true;

        }

        private void RenameCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ITreeViewItem tvm = ProjectItemsTreeview.SelectedNodes[0] as ITreeViewItem;

            if (tvm is FolderItem)
            {
                RenameFolderWindow window = new RenameFolderWindow(this.Project, tvm as FolderItem);
                window.ShowDialog();
            }
            else if (tvm is RecordsetItem)
            {
                RenameItemWindow window = new RenameItemWindow(this.Project, tvm as ITreeViewItem);
                window.ShowDialog();
            }
        }

        private void CopyNameClipboardCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ProjectItemsTreeview == null)
                return;

            if (ProjectItemsTreeview.SelectedNodes.Count != 1)
                return;

            if (ProjectItemsTreeview.SelectedRecordsetItemsCount != 1)
                return;

            e.CanExecute = true;
        }

        private void CopyNameClipboardCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ITreeViewItem selected_item = (ITreeViewItem)ProjectItemsTreeview.SelectedNodes[0];

            Clipboard.SetText(selected_item.Name);
        }

        // DP

        public Project Project
        {
            get { return (Project)GetValue(ProjectProperty); }
            set
            {
                SetValue(ProjectProperty, value);
            }
        }

        public static readonly DependencyProperty ProjectProperty =
            DependencyProperty.Register("Project", typeof(Project), typeof(ProjectItemsControl), new PropertyMetadata(null, new PropertyChangedCallback(OnProjectValueChanged)));

        private static void OnProjectValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProjectItemsControl self = (ProjectItemsControl)d;

            Project project = (Project)e.NewValue;

        }

        private void EditCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ProjectItemsTreeview == null)
                return;

            if (ProjectItemsTreeview.SelectedRecordsetItemsCount > 0)
                e.CanExecute = true;
        }

        private void EditCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var items = ProjectItemsTreeview.GetAllSelectedItems<RecordsetItem>();

            foreach (RecordsetItem recordsetitem in items)
                OpenEditRecordsetTab(recordsetitem);
        }

        public void OpenEditRecordsetTab(RecordsetItem recordsetitem)
        {
            MainWindow window = (MainWindow)Application.Current.MainWindow;

            string uniqueid = $"RS {recordsetitem.GetHashCode()}";
            window.AddTab(() => new RecordsetEditorPage(this.Project, recordsetitem), recordsetitem.ClassName, uniqueid, recordsetitem, TabMenu.CloseAble);
        }

        public void AddRecordsetCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ProjectItemsTreeview == null)
                return;

            if (ProjectItemsTreeview.SelectedNodes.Count != 1)
                return;

            e.CanExecute = true;
        }

        public void AddRecordsetCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ITreeViewItem selected_item = (ITreeViewItem)ProjectItemsTreeview.SelectedNodes[0];

            FolderItem folder;

            if (selected_item is RecordsetItem)
            {
                folder = selected_item.Parent;
            }
            else
            {
                folder = (FolderItem)selected_item;
            }

            // Do not allow the Root folder.
            if (folder is RootItem)
            {
                MessageBox.Show("You cannot add recordsets to the Root folder.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            // Do not allow the Auto Create Recordsets folder.
            string path = folder.CalculatePath();

            if (path.ToLower() == MainWindow.ViewModel.CurrentProject.AutoCreateSettings.Folder.ToLower())
            {
                MessageBox.Show("You cannot add recordsets to the Auto Create Recordsets folder.\n\nThe folder is emptied when running Auto Create.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            NewItemWindow window = new NewItemWindow(folder);

            bool? result = window.ShowDialog();

            if (result == false)
                return;

            string newclassname = window.txtClassName.Text;

            folder.IsExpanded = true;

            RecordsetItem newrecordsetitem = new RecordsetItem(this.Project, folder, newclassname);
            folder.Children.Add(newrecordsetitem);

            // clear the selection
            this.Project.FolderStructure.UnselectAll();

            // Set the new item as selected.
            newrecordsetitem.IsSelected = true;

            OpenEditRecordsetTab(newrecordsetitem);
        }

        private void ProjectItemsTreeview_GetContextMenuForRecordsetItem(object sender, UserControls.GetContextMenuForRecordsetItemEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem() { Command = DeleteCommandBinding.Command, Header = "Delete" });
            contextMenu.Items.Add(new MenuItem() { Command = RenameCommandBinding.Command, Header = "Rename" });
            contextMenu.Items.Add(new Separator());
            contextMenu.Items.Add(new MenuItem() { Command = CopyNameClipboardCommandBinding.Command, Header = "Copy name to Clipboard" });

            e.ContextMenu = contextMenu;
        }

        private void ProjectItemsTreeview_GetContextMenuForFolderItem(object sender, UserControls.GetContextMenuForFolderItemEventArgs e)
        {
            ICommand arsc = (ICommand)Application.Current.Resources["AddRecordsetCommand"];


            ContextMenu contextMenu = new ContextMenu();
            
            contextMenu.Items.Add(new MenuItem() { Command = arsc, Header = "Add Recordset" });
            contextMenu.Items.Add(new MenuItem() { Command = NewFolderCommandBinding.Command, Header = "New Folder" });
            contextMenu.Items.Add(new MenuItem() { Command = RenameCommandBinding.Command, Header = "Rename" });
            contextMenu.Items.Add(new MenuItem() { Command = DeleteCommandBinding.Command, Header = "Delete" });

            e.ContextMenu = contextMenu;
        }

        private void ProjectItemsTreeview_GetContextMenuForRootItem(object sender, UserControls.GetContextMenuForRootItemEventArgs e)
        {
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.Items.Add(new MenuItem() { Command = NewFolderCommandBinding.Command, Header = "New Folder" });

            e.ContextMenu = contextMenu;
        }

        private void NewFolderCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ProjectItemsTreeview == null)
                return;

            if (ProjectItemsTreeview.SelectedNodes.Count != 1)
                return;

            e.CanExecute = true;
        }

        private void NewFolderCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            ITreeViewItem selected_item = (ITreeViewItem)ProjectItemsTreeview.SelectedNodes[0];

            FolderItem folder;

            if (selected_item is RecordsetItem)
            {
                folder = selected_item.Parent;
            }
            else
            {
                folder = (FolderItem)selected_item;
            }

            CreateFolderWindow window = new CreateFolderWindow(folder);

            bool? result = window.ShowDialog();

            if (result == false)
                return;

            string newfoldername = window.txtFolderName.Text;

            FolderItem newfolder = new FolderItem(this.Project, folder, newfoldername);

            folder.Children.Add(newfolder);

            folder.IsExpanded = true;

            // clear the selection
            this.Project.FolderStructure.UnselectAll();

            // Set the new folder as selected.
            newfolder.IsSelected = true;
        }

        private void MoveCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ProjectItemsTreeview == null)
                return;

            if (ProjectItemsTreeview.SelectedFolderItemsCount > 0 || ProjectItemsTreeview.SelectedRecordsetItemsCount > 0)
                e.CanExecute = true;
        }

        private void MoveCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            MoveItemWindow window = new MoveItemWindow(this.Project, ProjectItemsTreeview.SelectedNodes);
            window.ShowDialog();
        }

        private void CopyCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ProjectItemsTreeview == null)
                return;

            if (ProjectItemsTreeview.SelectedFolderItemsCount > 0 || ProjectItemsTreeview.SelectedRecordsetItemsCount > 0)
                e.CanExecute = true;
        }

        private void CopyCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            CopyItemWindow window = new CopyItemWindow(this.Project, ProjectItemsTreeview.SelectedNodes);
            window.ShowDialog();
        }

        public void AutoCreateCommand_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (ProjectItemsTreeview == null)
                return;

            if (MainWindow.ViewModel.CurrentProject == null)
                return;

            e.CanExecute = true;
        }

        public void AutoCreateCommand_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (MainWindow.ViewModel.CurrentProject.AutoCreateSettings.Enabled == false)
            {
                MessageBox.Show("Auto Create Recordsets is disabled.\n\nYou need to enable the feature in the Project Settings page.", "VenturaSQL Studio", MessageBoxButton.OK, MessageBoxImage.Exclamation);
                return;
            }

            RunAutoCreate run = new RunAutoCreate(MainWindow.ViewModel.CurrentProject);

            run.Exec(true);
        }

    }
}
