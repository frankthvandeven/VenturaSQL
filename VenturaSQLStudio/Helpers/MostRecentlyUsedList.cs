using System.Collections.ObjectModel;
using System.IO;

namespace VenturaSQLStudio
{
    public class MostRecentlyUsedList
    {
        private int _maxsize;
        private ObservableCollection<MostRecentlyUsedListItem> _collection;

        /// <summary>
        /// The minumum value for maxsize is 5.
        /// </summary>
        public MostRecentlyUsedList(int maxsize)
        {
            if (maxsize < 5)
                maxsize = 5;

            _maxsize = maxsize;
            _collection = new ObservableCollection<MostRecentlyUsedListItem>();
        }

        public ObservableCollection<MostRecentlyUsedListItem> Collection
        {
            get { return _collection; }
        }

        public void AddFile(string full_file_path)
        {
            MostRecentlyUsedListItem item = null;

            for (int i = 0; i < _collection.Count; i++)
            {
                if (_collection[i].FullFilePath == full_file_path)
                {
                    item = _collection[i];
                    _collection.RemoveAt(i);
                    break;
                }
            }

            if (item == null)
            {
                item = new MostRecentlyUsedListItem();
                item.FullFilePath = full_file_path;
                item.Pinned = false;
            }

            _collection.Insert(0, item);

            TrimList();


        }

        private void TrimList()
        {
            while (_collection.Count > _maxsize)
                _collection.RemoveAt(_collection.Count - 1);
        }

        public void ReadFromIniFile(IniFile ini_file)
        {
            _collection.Clear();

            Group ini_group = ini_file["MRU"];

            foreach (GroupValue group_value in ini_group)
            {
                string[] data = group_value.ValueData.Split(',');

                MostRecentlyUsedListItem item = new MostRecentlyUsedListItem();
                item.FullFilePath = data[0];

                if (data.Length >= 2)
                    item.Pinned = (data[1].ToLower() == "pinned" ? true : false);

                _collection.Add(item);
            }

        }

        public void Remove(MostRecentlyUsedListItem item)
        {
            _collection.Remove(item);
        }

        public void WriteToIniFile(IniFile ini_file)
        {
            Group ini_group = ini_file["MRU"];

            ini_group.Clear();

            int i = 0;

            foreach (MostRecentlyUsedListItem mru_item in _collection)
            {
                i++;

                string pinned_text = mru_item.Pinned ? ",pinned" : "";

                ini_group.Set($"{i}", $"{mru_item.FullFilePath}{pinned_text}");
            }
        }

        public void Clear()
        {
            _collection.Clear();
        }

    } // class

    public class MostRecentlyUsedListItem
    {

        private string _full_file_path;

        public string FullFilePath
        {
            get { return _full_file_path; }
            set { _full_file_path = value; }
        }

        public bool Pinned { get; set; }

        public string FileNameWithoutExtension
        {
            get { return Path.GetFileNameWithoutExtension(_full_file_path); }
        }

        public string GetDirectoryName
        {
            get { return Path.GetDirectoryName(_full_file_path); }
        }

    }
}
