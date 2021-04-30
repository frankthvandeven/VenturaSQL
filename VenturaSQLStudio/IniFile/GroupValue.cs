using System;
using System.Collections.Generic;
using System.Text;

namespace VenturaSQLStudio
{
    public class GroupValue
    {

        private string _valuename;
        private string _valuedata;


        public GroupValue(string valuename, string valuedata)
        {
            _valuename = valuename;
            _valuedata = valuedata;
        }

        public string ValueName
        {
            get
            {
                return _valuename;
            }
            set
            {
                _valuename = value.ToLower();
            }
        }

        public string ValueData
        {
            get
            {
                return _valuedata;
            }
            set
            {
                _valuedata = value;
            }

        }

    }
}
