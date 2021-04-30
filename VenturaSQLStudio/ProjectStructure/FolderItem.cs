using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace VenturaSQLStudio {
    /// <summary>
    /// Base class for all ViewModel classes displayed by TreeViewExItems.  
    /// This acts as an adapter between a raw data object and a TreeViewExItem.
    /// </summary>
    public class FolderItem : ViewModelBase, ITreeViewItem
    {
        readonly ObservableCollection<ITreeViewItem> _children;
        private FolderItem _parent;

        bool _isExpanded = false;
        bool _isSelected = false;

        private string _foldername;

        private Project _owningproject;

        public FolderItem(Project owningproject, FolderItem parent, string foldername)
        {
            _owningproject = owningproject;
            _parent = parent;
            _foldername = foldername;

            _children = new ObservableCollection<ITreeViewItem>();

            _children.CollectionChanged += _children_CollectionChanged;
        }

        private void _children_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            //if (e.Action == System.Collections.Specialized.NotifyCollectionChangedAction.Reset)
            //    return;

            _owningproject?.SetModified();
        }

        public string Foldername
        {
            get { return _foldername; }
            set
            {
                if (_foldername == value)
                    return;

                _foldername = value;
                NotifyPropertyChanged("FolderName");

                _owningproject?.SetModified();
            }
        }

        /// <summary>
        /// Returns the logical child items of this object.
        /// </summary>
        public ObservableCollection<ITreeViewItem> Children
        {
            get { return _children; }
        }

        /// <summary>
        /// Gets/sets whether the TreeViewExItem 
        /// associated with this object is expanded.
        /// </summary>
        public bool IsExpanded
        {
            get { return _isExpanded; }
            set
            {
                if (_isExpanded == value)
                    return;

                _isExpanded = value;
                NotifyPropertyChanged("IsExpanded");
                NotifyPropertyChanged("FolderImage");

                // Modified notification disabled as a simple mouse-click-expand/collapse
                // would set the project to modified.

                //_owningproject?.SetModified();
            }
        }

        /// <summary>
        /// Expands this folder and all its parent folders.
        /// </summary>
        public void ExpandBubbleUp()
        {
            if (_isExpanded == false)
            {
                _isExpanded = true;
                NotifyPropertyChanged("IsExpanded");
                //_owningproject?.SetModified();
            }

            // Expand all the way up to the root.
            if (_parent != null)
                _parent.ExpandBubbleUp();

        }

        /// <summary>
        /// Gets/sets whether the TreeViewExItem 
        /// associated with this object is selected.
        /// </summary>
        public bool IsSelected
        {
            get { return _isSelected; }
            set
            {
                if (value != _isSelected)
                {
                    _isSelected = value;
                    NotifyPropertyChanged("IsSelected");

                    // Modified notification disabled as a simple mouse-click-select
                    // would set the project to modified.

                    //_owningproject?.SetModified();
                }
            }
        }

        /// <summary>
        /// Calculates the full path of the FolderItem.
        /// For example: Ventura\ProjectItems
        public string CalculatePath()
        {
            StringBuilder sb = new StringBuilder();

            CalculatePath(sb);

            return sb.ToString();
        }

        /// <summary>
        /// Calculates the full path of the FolderItem. Better call CalculatePath() without parameter.
        /// </summary>
        internal void CalculatePath(StringBuilder sb)
        {
            if (_parent != null)
                _parent.CalculatePath(sb);

            if (_foldername == "")
                return;

            if (sb.Length > 0)
                sb.Append("\\");

            sb.Append(_foldername);
        }


        /// <summary>
        /// Returns true when a child item with the specified name exists.
        /// The check is not case-sensitive.
        /// </summary>
        public bool ChildItemExists(string name, ITreeViewItem ignore = null)
        {
            bool found = false;

            foreach (ITreeViewItem tvi in this.Children)
            {
                bool skip = false;

                if (ignore != null)
                    if (ignore.Equals(tvi) == true)
                        skip = true;

                if (skip == false)
                {
                    FolderItem folder_item = tvi as FolderItem;

                    if (folder_item != null)
                    {
                        if (folder_item.Foldername.ToLower() == name.ToLower())
                        {
                            found = true;
                            break;
                        }
                    }

                    RecordsetItem recordset_item = tvi as RecordsetItem;

                    if (recordset_item != null)
                    {
                        if (recordset_item.ClassName.ToLower() == name.ToLower())
                        {
                            found = true;
                            break;
                        }

                    }

                }


            }

            return found;
        }

        public List<ITreeViewItem> AllProjectItemsInThisFolderAndSubfolders()
        {
            List<ITreeViewItem> itemlist = new List<ITreeViewItem>();

            IterateChildren2(itemlist, this);

            return itemlist;
        }

        private void IterateChildren2(List<ITreeViewItem> itemlist, FolderItem currentfolderitem)
        {
            foreach (ITreeViewItem childitem in currentfolderitem.Children)
            {
                if ((childitem is FolderItem) == false)
                    itemlist.Add(childitem);
                else
                    IterateChildren2(itemlist, (FolderItem)childitem);
            }
        }

        /// <summary>
        /// Unselects all children of the current FolderItem.
        /// The IsSelected of this FolderItem does not change.
        /// </summary>
        public void UnselectAllChildren()
        {
            UnselectAllChildrenIterator(this);
        }

        private void UnselectAllChildrenIterator(FolderItem folder_item)
        {
            foreach (ITreeViewItem childitem in folder_item.Children)
            {
                if (childitem.IsSelected == true)
                    childitem.IsSelected = false;

                FolderItem child_folder_item = childitem as FolderItem;

                if (child_folder_item != null)
                    UnselectAllChildrenIterator(child_folder_item);
            }
        }

        public bool ContainsChildFolder(FolderItem compare_item)
        {
            return ContainsChildFolderIterator(this, compare_item);
        }

        private bool ContainsChildFolderIterator(FolderItem folder_item, FolderItem compare_item)
        {
            foreach (ITreeViewItem childitem in folder_item.Children)
            {
                FolderItem child_folder_item = childitem as FolderItem;

                if (child_folder_item != null)
                {
                    if (child_folder_item.Equals(compare_item))
                        return true;

                    bool result = ContainsChildFolderIterator(child_folder_item, compare_item);

                    if (result == true)
                        return true;
                }
            }

            return false;
        }

        public ImageSource FolderImage
        {
            get
            {
                if (_isExpanded == false)
                    return GetProductImageFromFilename("Folder.png");
                else
                    return GetProductImageFromFilename("FolderOpen.png");

            }
        }

        #region Static method

        /// <summary>
        /// Filename like 'default_installed.png'.
        /// </summary>
        public static ImageSource GetProductImageFromFilename(string filename)
        {
            //Uri u = new Uri($"/Pages/ProviderPage/ProductImages/{filename}", UriKind.Relative);

            string uriString = $@"pack://application:,,,/VenturaSQLStudio;component/Assets/{filename}";

            Uri u = new Uri(uriString, UriKind.RelativeOrAbsolute);

            BitmapImage bmi = new BitmapImage(u);

            // prevents error 'Must create DependencySource on same Thread as the DependencyObject'
            if (bmi.CanFreeze == true)
                bmi.Freeze();

            return bmi;
        }
        #endregion

        FolderItem ITreeViewItem.Parent
        {
            get { return _parent; }
            set { _parent = value; }
        }

        TreeViewModelKind ITreeViewItem.ItemKind
        {
            get { return TreeViewModelKind.FolderItem; }
        }

        string ITreeViewItem.Name
        {
            get { return _foldername; }
        }

    }
}