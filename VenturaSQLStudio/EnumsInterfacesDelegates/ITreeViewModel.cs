
namespace VenturaSQLStudio
{
    public interface ITreeViewItem
    {
        FolderItem Parent
        {
            get;
            set;
        }

        TreeViewModelKind ItemKind
        {
            get;
        }

        string Name
        {
            get;
        }

        bool IsSelected
        {
            get;
            set;
        }

        bool IsExpanded
        {
            get;
            set;
        }

    }
}
