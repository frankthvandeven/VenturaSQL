using System;
using System.Collections.ObjectModel;
using System.Data;
using System.Data.Common;
using System.IO;
using System.Linq;
using System.Text;
using VenturaSQL;
using VenturaSQLStudio.Ado;

namespace VenturaSQLStudio
{
    public class TableList : ObservableCollection<TableListItem>
    {
        private bool _all_catalog_names_equal = false;
        private bool _all_schema_names_equal = false;

        public void CollectListOfTables(Project project)
        {
            AdoConnector connector = AdoConnectorHelper.Create(project.ProviderInvariantName, project.MacroConnectionString);

            DataTable data_table;

            using (DbConnection connection = connector.OpenConnection())
            {
                data_table = connection.GetSchemaExtension("Tables");
            }

#if EXCLUDE_THIS
            using (MemoryStream ms = new MemoryStream())
            {
                data_table.WriteXml(ms);
                string readable = Encoding.ASCII.GetString(ms.ToArray());
            }
#endif

            this.Clear();
            //this.ClearItems();

            foreach (DataRow row in data_table.Rows)
            {
                string server_name = row.RowValue("TABLE_SERVER", "");
                string catalog_name = row.RowValue("TABLE_CATALOG", "");
                string schema_name = row.RowValue("TABLE_SCHEMA", "");
                string table_name = row.RowValue("TABLE_NAME", "");
                string table_type = row.RowValue("TABLE_TYPE", "");

                if (server_name == "")
                    server_name = row.RowValue(project.AdvancedSettings.ColumnServer, "");

                if (catalog_name == "")
                    catalog_name = row.RowValue(project.AdvancedSettings.ColumnCatalog, "");

                if (schema_name == "")
                    schema_name = row.RowValue(project.AdvancedSettings.ColumnSchema, "");

                if (table_name == "")
                    table_name = row.RowValue(project.AdvancedSettings.ColumnTable, "");

                if (table_type == "")
                    table_type = row.RowValue(project.AdvancedSettings.ColumnType, "");

                if (table_name == "") // It is impossible not to find a table name.
                    ThrowMappingException(project, data_table);

                if (table_type == "" || table_type.ToLower().Contains("table"))
                {
                    TableName tn = new TableName(server_name, catalog_name, schema_name, table_name);
                    this.Add(new TableListItem(this, tn));
                }
            }

            // Test if all catalog names are the same.
            var grouped_catalognames = from x in this
                                       group x by x.PreliminaryTableName.BaseCatalogName;

            if (grouped_catalognames.Count() == 1)
                _all_catalog_names_equal = true;

            // Test if all schema names are the same.
            var grouped_schemanames = from x in this
                                      group x by x.PreliminaryTableName.BaseCatalogName + "." + x.PreliminaryTableName.BaseSchemaName;

            if (grouped_schemanames.Count() == 1)
                _all_schema_names_equal = true;


            // Set the Selected (excluded) property.
            foreach(TableName excluded in project.AutoCreateSettings.ExcludedTablenames)
            {
                TableListItem found = this.FirstOrDefault(a => a.PreliminaryTableName == excluded);

                if (found != null)
                    found.Exclude = true;
            }

            this.Sort(z => z.DisplayTableName);
        }

        private void ThrowMappingException(Project project, DataTable data_table)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"A table list of {data_table.Rows.Count} items was returned by ADO.NET.");
            sb.AppendLine();
            sb.AppendLine("Unable to find the table name.");
            sb.AppendLine();
            sb.AppendLine($"The selected provider \"{project.ProviderInvariantName}\" needs custom column mappings.");
            sb.AppendLine();
            sb.AppendLine("Open the Advanced Provider Settings window to change the mappings.");

            string message = sb.ToString();

            throw new Exception(message);
        }


        public bool AllCatalogNamesEqual
        {
            get { return _all_catalog_names_equal; }
        }

        public bool AllSchemaNamesEqual
        {
            get { return _all_schema_names_equal; }
        }

    }
}
