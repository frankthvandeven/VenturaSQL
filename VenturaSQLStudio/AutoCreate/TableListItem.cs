using System.Text;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio
{
    public class TableListItem : ViewModelBase
    {
        private TableList _parent;
        private bool _exclude = false;

        private TableName _tablename;

        public TableListItem(TableList parent, TableName tablename)
        {
            _parent = parent;
            _tablename = tablename;
        }

        public bool Exclude
        {
            get { return _exclude; }
            set
            {
                if (_exclude == value)
                    return;

                _exclude = value;

                NotifyPropertyChanged("Exclude");
            }
        }

        public string DisplayTableName
        {
            get
            {
                string catalog = _tablename.BaseCatalogName;
                string schema = _tablename.BaseSchemaName;

                StringBuilder sb = new StringBuilder();

                if (catalog != "")
                    if (_parent.AllCatalogNamesEqual == false)
                    {
                        sb.Append(catalog);
                        sb.Append(".");
                    }

                if (schema != "")
                    if (_parent.AllSchemaNamesEqual == false)
                    {
                        sb.Append(schema);
                        sb.Append(".");
                    }

                sb.Append(_tablename.BaseTableName);

                return sb.ToString();
            }
        }

        /// <summary>
        /// It is preliminary as GetSchema("Tables") can not be expected
        /// to return full server + catalog + schema + table names.
        /// </summary>
        public TableName PreliminaryTableName
        {
            get
            {
                return _tablename;
            }
        }

    }
}
