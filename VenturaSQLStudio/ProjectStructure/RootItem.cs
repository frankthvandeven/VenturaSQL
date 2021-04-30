using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace VenturaSQLStudio {

    public class RootItem : FolderItem, ITreeViewItem, IEnumerable<RootItem>
    {
        private Project _owningproject;

        public RootItem(Project owningproject) : base(owningproject, null, "") // Root of C# project
        {
            _owningproject = owningproject;
        }

        public List<FolderListItem> GetFolderList() /* method could be moved to FolderItem, so it can also start in middle of a tree instead of root */
        {
            List<FolderListItem> folderlist = new List<FolderListItem>();

            folderlist.Add(new FolderListItem("", this));

            IterateChildren(folderlist, this, null);

            return folderlist;
        }

        private void IterateChildren(List<FolderListItem> pathlist, FolderItem currentitem, string startpath) /* method could be moved to FolderItem, so it can also start in middle of a tree instead of root */
        {
            foreach (FolderItem child in currentitem.Children.Where(child => child.ItemKind == TreeViewModelKind.FolderItem))
            {
                string newpath;

                if (startpath == null)
                    newpath = child.Foldername;
                else
                    newpath = startpath + @"\" + child.Foldername;

                pathlist.Add(new FolderListItem(newpath, child));

                IterateChildren(pathlist, child, newpath);
            }
        }

        /// <summary>
        /// Unselects all children of the current FolderItem.
        /// The IsSelected of this FolderItem does not change.
        /// </summary>
        public void UnselectAll()
        {
            UnselectAllIterator(this);
        }

        private void UnselectAllIterator(FolderItem folder_item)
        {
            if (this.IsSelected == true)
                this.IsSelected = false;

            foreach (ITreeViewItem childitem in folder_item.Children)
            {
                if (childitem.IsSelected == true)
                    childitem.IsSelected = false;

                FolderItem child_folder_item = childitem as FolderItem;

                if (child_folder_item != null)
                    UnselectAllIterator(child_folder_item);
            }
        }

        /// <summary>
        /// Will always return a FolderItem. If the item does not exist, the tree structure will be created.
        /// </summary>
        public FolderItem FetchOrCreateFolderItem(string folderpath) /* method could be moved to FolderItem, so it can also start in middle of a tree instead of root */
        {
            string[] parts = folderpath.Split(new[] { '\\' }, StringSplitOptions.RemoveEmptyEntries);

            // We start scanning the object tree from the root level.
            FolderItem currentitem = this;

            foreach (string path_part in parts)
            {
                FolderItem childfound = null;

                foreach (FolderItem child in currentitem.Children.Where(child => child.ItemKind == TreeViewModelKind.FolderItem))
                {
                    if (child.Foldername == path_part)
                    {
                        childfound = child;
                        break;
                    }
                }

                if (childfound != null)
                    currentitem = childfound;
                else
                {
                    FolderItem newitem = new FolderItem(_owningproject, currentitem, path_part);
                    currentitem.Children.Add(newitem);
                    currentitem = newitem;
                }

            } // end of parts loop

            return currentitem;
        } // end of method

        public void AddRecordsetItem(string outputfolder, RecordsetItem recordsetitem)
        {
            FolderItem folderitem = FetchOrCreateFolderItem(outputfolder);

            ITreeViewItem item = recordsetitem;

            item.Parent = folderitem;

            folderitem.Children.Add(item);
        }

        IEnumerator<RootItem> IEnumerable<RootItem>.GetEnumerator()
        {
            // How did I come up with this?
            yield return this; // it enumerates a "collection" only containing one item. This is for XAML TreeView databinding
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            yield return this; // it enumerates a "collection" only containing one item. This is for XAML TreeView databinding
        }

        TreeViewModelKind ITreeViewItem.ItemKind
        {
            get { return TreeViewModelKind.RootItem; }
        }

        public class FolderListItem
        {
            private string _outputfolder;
            private FolderItem _folderitem;

            public FolderListItem(string outputfolder, FolderItem folderitem)
            {
                _outputfolder = outputfolder;
                _folderitem = folderitem;
            }

            public string OutputFolder
            {
                get { return _outputfolder; }
            }

            public FolderItem FolderItem
            {
                get { return _folderitem; }
            }

        }

    }
}
