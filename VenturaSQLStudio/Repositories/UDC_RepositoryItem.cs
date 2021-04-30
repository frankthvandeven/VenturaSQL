using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using VenturaSQL;

namespace VenturaSQLStudio
{
    public class UDC_RepositoryItem
    {
        private string _framework_name;
        private Type _type;
        private string _full_type_name;

        public UDC_RepositoryItem(string framework_name, Type type)
        {
            _framework_name = framework_name;
            _type = type;

            _full_type_name = TypeTools.FullTypename(type);
        }

        public string FullTypename // was DataString
        {
            get { return _full_type_name; }
        }

        public string DisplayString
        {
            get { return _framework_name; }
        }

    }
}
