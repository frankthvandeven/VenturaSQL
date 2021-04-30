using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;

namespace VenturaSQLStudio {

    public class IniFile : IEnumerable<Group>
    {
        private string _filename;
        private List<Group> _groupslist;
        private List<String> _commentslist;

        public IniFile(string filename)
        {
            _filename = filename;

            FileStream filestream = null;

            try
            {
                _groupslist = new List<Group>();
                _commentslist = new List<string>();

                // Make sure the folder exists
                string path = Path.GetDirectoryName(filename);
                Directory.CreateDirectory(path);

                filestream = new FileStream(filename, FileMode.OpenOrCreate, FileAccess.Read);

                filestream.Position = 0;

                using (StreamReader sr = new StreamReader(filestream))
                {
                    Group selectedgroup = null;

                    while (true)
                    {
                        string line = sr.ReadLine();

                        if (line == null) break;

                        line = line.Trim();

                        if (line.Length == 0)
                        {
                            if (selectedgroup == null)
                                _commentslist.Add("");
                        }
                        else if (line.StartsWith(";"))
                        {
                            if (selectedgroup == null)
                                _commentslist.Add(line);
                            else
                                _commentslist.Add(string.Format(";({0}) {1}", selectedgroup.Name, line.Substring(1)));
                        }
                        else if (line.StartsWith("["))
                        {
                            string groupname = line.Replace("[", "").Replace("]", "");
                            selectedgroup = new Group(groupname);
                            _groupslist.Add(selectedgroup);
                        }
                        else
                        {
                            int pos = line.IndexOf("=");

                            if (pos != -1)
                            {
                                if (selectedgroup == null)
                                    _commentslist.Add(string.Format("Ignored ungrouped setting: {0}", line));
                                else
                                {
                                    string valuename = line.Substring(0, pos);
                                    string valuedata = line.Substring(pos + 1);
                                    selectedgroup.Set(valuename, valuedata);
                                }
                            }
                        }
                    }
                }
            }
            finally
            {
                if (filestream != null)
                    filestream.Dispose();
            }

        }

        /// <summary>
        /// Clears all values and comments stored in memory.
        /// </summary>
        public void Zap()
        {
            _groupslist.Clear();
            _commentslist.Clear();
        }

        public void Save()
        {
            FileStream filestream = null;

            try
            {
                // Make sure the folder exists
                string path = Path.GetDirectoryName(_filename);
                Directory.CreateDirectory(path);

                filestream = new FileStream(_filename, FileMode.OpenOrCreate, FileAccess.Write);

                filestream.SetLength(0);

                using (StreamWriter sw = new StreamWriter(filestream))
                {
                    bool lastline_empty = false;

                    foreach (String line in _commentslist)
                    {
                        if (line.Length == 0)
                        {
                            sw.WriteLine();
                            lastline_empty = true;
                        }
                        else
                        {
                            lastline_empty = false;
                            if (line.StartsWith(";") == false)
                                sw.Write(";");
                            sw.WriteLine(line);
                        }
                    }

                    if (lastline_empty == false)
                        sw.WriteLine();

                    foreach (Group group in _groupslist)
                    {
                        sw.WriteLine(string.Format("[{0}]", group.Name));

                        foreach (GroupValue groupvalue in group)
                            sw.WriteLine(string.Format("{0}={1}", groupvalue.ValueName, groupvalue.ValueData));

                        sw.WriteLine();
                    }

                } /* StreamWrite.Dispose() will call .Flush() internally. */
            }
            finally
            {
                if (filestream != null)
                    filestream.Dispose();
            }
        }

        /// <summary>
        /// Returns a Group. If the group doesn't exist, the group will be created.
        /// </summary>
        /// <param name="groupname">The name of the group to be retrieved or created.</param>
        /// <returns>Returns the selected group.</returns>
        public Group this[string groupname]
        {
            get
            {
                foreach (Group group in _groupslist)
                {
                    if (group.Name.ToLower() == groupname.ToLower())
                        return group;
                }

                Group newgroup = new Group(groupname);
                _groupslist.Add(newgroup);

                return newgroup;
            }
        }

        public IEnumerator<Group> GetEnumerator()
        {
            return _groupslist.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _groupslist.GetEnumerator();
        }

    } // end of class
}
