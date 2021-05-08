using System;
using System.ComponentModel;
using VenturaSQL;

namespace VenturaSQLStudio
{
    public class UDCItem : ViewModelBase
    {
        private string _columnname = "";
        private string _fulltypename = "";

        private Project _owningproject;

        public UDCItem(Project owningproject)
        {
            _owningproject = owningproject;
        }

        public string ColumnName
        {
            get { return _columnname; }
            set
            {
                if (_columnname == value)
                    return;

                _columnname = value;

                _owningproject?.SetModified();

                NotifyPropertyChanged("ColumnName");
            }
        }

        public string FullTypename
        {
            get { return _fulltypename; }
            set
            {
                if (_fulltypename == value)
                    return;

                _fulltypename = value;

                _owningproject?.SetModified();

                NotifyPropertyChanged("FullTypename");
            }
        }

        /// <summary>
        /// For C# code generator.
        /// For example "DateTime?", "int?", "string"
        /// </summary>
        public string ShortTypeName
        {
            get
            {
                VenturaSqlColumn c = new VenturaSqlColumn(_columnname, _fulltypename, true);
                return c.ShortTypeNameForColumnProperty();
            }
        }

        /// <summary>
        /// For C# code generator.
        /// For example "_udc__customer_id"
        /// </summary>
        public string PrivateVariableName
        {
            get
            {
                return "_udc__" + TemplateHelper.ConvertToValidIdentifier(_columnname);
            }
        }

        /// <summary>
        /// For C# code generator.
        /// For example "customer_id"
        /// </summary>
        public string PropertyName
        {
            get
            {
                return TemplateHelper.ConvertToValidIdentifier(_columnname);
            }
        }


        public UDCItem Clone()
        {
            UDCItem temp = new UDCItem(_owningproject);
            temp.ColumnName = this.ColumnName;
            temp.FullTypename = this.FullTypename;
            return temp;
        }

    }
}
