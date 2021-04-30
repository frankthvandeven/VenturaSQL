using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.ComponentModel;
using System.Xml;
using System.Reflection;
using System.Collections.ObjectModel;
using System.Collections.Specialized;

namespace VenturaSQLStudio
{

    public class UDCCollection : ObservableCollection<UDCItem>
    {
        private Project _owningproject;

        public UDCCollection(Project owningproject)
        {
            _owningproject = owningproject;
        }

        public UDCCollection Clone()
        {
            UDCCollection temp = new UDCCollection(_owningproject);

            foreach (UDCItem item in this.Items)
                temp.Add(item.Clone());

            return temp;
        }

        protected override void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            _owningproject?.SetModified();

            base.OnCollectionChanged(e);
        }
    }


}
