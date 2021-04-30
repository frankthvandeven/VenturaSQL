using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace VenturaSQLStudio
{
    public class Group : IEnumerable<GroupValue>
    {
        private string _name;
        private List<GroupValue> _valueslist = new List<GroupValue>();

        public Group(string name)
        {
            _name = name;
        }

        public string Name
        {
            get { return _name; }
        }

        public void Clear()
        {
            _valueslist.Clear();
        }

        public void Set(string valuename, string valuedata)
        {
            valuename = valuename.ToLower();

            foreach (GroupValue setting in _valueslist)
            {
                if (setting.ValueName == valuename)
                {
                    setting.ValueData = valuedata;
                    return;
                }
            }

            _valueslist.Add( new GroupValue(valuename, valuedata) );
        }

        public void Set(string valuename, Int32 valuedata)
        {
            Set(valuename, Convert.ToString(valuedata));
        }

        public string Get(string valuename, string defaultvalue)
        {
            valuename = valuename.ToLower();

            foreach (GroupValue setting in _valueslist)
            {
                if (setting.ValueName == valuename)
                    return setting.ValueData;
            }

            _valueslist.Add( new GroupValue(valuename, defaultvalue) );

            return defaultvalue;

        }

        public Int32 Get(string valuename, Int32 defaultvalue)
        {
            try
            {
                return Convert.ToInt32(Get(valuename, Convert.ToString(defaultvalue)));
            }
            catch
            {
                return defaultvalue;
            }
        }

        /// <summary>
        /// Extracts a list with settings starting with selected string of characters.
        /// </summary>
        /// <param name="startswith">The string to check for.</param>
        /// <returns>A list with settings.</returns>
        public List<GroupValue> Extract(string startswith)
        {
            startswith = startswith.ToLower();
            string dotend = startswith + ".";

            List<GroupValue> s = new List<GroupValue>();

            foreach (GroupValue setting in _valueslist)
            {
                if (setting.ValueName == startswith || setting.ValueName.StartsWith(dotend))
                    s.Add(setting);
            }

            return s;

        }


        public IEnumerator<GroupValue> GetEnumerator()
        {
            return _valueslist.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _valueslist.GetEnumerator();
        }
    
    }





}
