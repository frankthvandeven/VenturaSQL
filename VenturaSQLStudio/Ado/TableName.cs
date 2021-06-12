using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.Ado
{
    public class TableName
    {
        private string _base_servername;
        private string _base_catalogname;
        private string _base_schemaname;
        private string _base_tablename;


        public TableName(string base_servername, string base_catalogname, string base_schemaname, string base_tablename)
        {
            if (base_servername == null)
                throw new ArgumentNullException("base_servername");

            if (base_catalogname == null)
                throw new ArgumentNullException("base_catalogname");

            if (base_schemaname == null)
                throw new ArgumentNullException("base_schemaname");

            if (base_tablename == null)
                throw new ArgumentNullException("base_tablename");

            if (base_tablename.Trim() == "")
                throw new ArgumentException("base_tablename should not be an empty string.");

            _base_servername = base_servername;
            _base_catalogname = base_catalogname;
            _base_schemaname = base_schemaname;
            _base_tablename = base_tablename;
        }

        public string BaseServerName
        {
            get { return _base_servername; }
        }

        public string BaseCatalogName
        {
            get { return _base_catalogname; }
        }

        public string BaseSchemaName
        {
            get { return _base_schemaname; }
        }

        public string BaseTableName
        {
            get { return _base_tablename; }
        }

        /// <summary>
        /// Table name according to project settings. For use in generated SQL statements.
        /// </summary>
        public string ScriptTableName
        {
            get
            {
                Project project = MainWindow.ViewModel.CurrentProject;
                AdvancedSettings advanced = project.AdvancedSettings;

                string prefix = project.QuotePrefix;
                string suffix = project.QuoteSuffix;

                StringBuilder sb = new StringBuilder();

                if (_base_servername != "" && advanced.IncludeServerName)
                {
                    if (_base_servername.Contains(' '))
                    {
                        sb.Append(prefix);
                        sb.Append(_base_servername);
                        sb.Append(suffix);
                    }
                    else
                    {
                        sb.Append(_base_servername);
                    }

                    sb.Append(".");
                }

                if (_base_catalogname != "" && advanced.IncludeCatalogName)
                {
                    if (_base_catalogname.Contains(' '))
                    {
                        sb.Append(prefix);
                        sb.Append(_base_catalogname);
                        sb.Append(suffix);
                    }
                    else
                    {
                        sb.Append(_base_catalogname);
                    }

                    sb.Append(".");
                }

                if (_base_schemaname != "" && advanced.IncludeSchemaName)
                {
                    if (_base_schemaname.Contains(' '))
                    {
                        sb.Append(prefix);
                        sb.Append(_base_schemaname);
                        sb.Append(suffix);
                    }
                    else
                    {
                        sb.Append(_base_schemaname);
                    }

                    sb.Append(".");
                }

                if (_base_tablename.Contains(' '))
                {
                    sb.Append(prefix);
                    sb.Append(_base_tablename);
                    sb.Append(suffix);
                }
                else
                {
                    sb.Append(_base_tablename);
                }

                return sb.ToString();
            }
        }

        public string CSharpTableName
        {
            get
            {
                return ScriptTableName.Replace("\"", "\\\"");
            }
        }




        public static bool operator ==(TableName left, TableName right)
        {
            return EqualHelper(left, right);
        }

        public static bool operator !=(TableName left, TableName right)
        {
            return !EqualHelper(left, right);
        }

        private static bool EqualHelper(TableName left, TableName right)
        {
            if ((object)left == (object)right)
                return true;

            if (left is null || right is null)
                return (left is null && right is null);

            if (left.BaseServerName != right.BaseServerName)
                return false;

            if (left.BaseCatalogName != right.BaseCatalogName)
                return false;

            if (left.BaseSchemaName != right.BaseSchemaName)
                return false;

            if (left.BaseTableName != right.BaseTableName)
                return false;

            return true;
        }

        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is TableName))
                return false;

            TableName table_name = (TableName)obj;

            return TableName.EqualHelper(this, table_name);
        }

        public override int GetHashCode()
        {
            return _base_servername.GetHashCode() ^ _base_catalogname.GetHashCode() ^ _base_schemaname.GetHashCode() ^ _base_tablename.GetHashCode();
        }

    }
}
