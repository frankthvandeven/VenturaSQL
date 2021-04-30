using System.Collections.Generic;

namespace VenturaSQLStudio
{

    public static class ProviderRepository
    {
        private static List<ProviderInfo> _provider_list = new();

        static ProviderRepository()
        {
            List<ProviderInfo> list = VendorList();
            
            // First add the installed providers. This is for grouping in the ProviderPage dialog.
            foreach (var item in list)
                if (item.Factory != null)
                    _provider_list.Add(item);

            // Second add the providers that are not installed
            foreach (var item in list)
                if (item.Factory == null)
                    _provider_list.Add(item);

        }

        public static List<ProviderInfo> List
        {
            get { return _provider_list; }
        }

        private static List<ProviderInfo> VendorList()
        {
            List<ProviderInfo> list = new();

            list.Add(new ProviderInfo("System.Data.SqlClient")
            {
                Name = "SqlClient Data Provider",
                Description = ".Net Framework Data Provider for SqlServer.",
                Company = "Microsoft",
                ProductImage = ProviderInfo.GetProductImageFromFilename("sql_server.png"),
                Factory = System.Data.SqlClient.SqlClientFactory.Instance,
                FactoryAsString = "System.Data.SqlClient.SqlClientFactory.Instance"

            });

            list.Add(new ProviderInfo("Npgsql")
            {
                Name = "PostgreSQL Data Provider for .NET",
                Description = ".Net Framework Data Provider for PostgreSQL.",
                Company = "Npgsql Development Team",
                ProductImage = ProviderInfo.GetProductImageFromFilename("PostgreSQL.png"),
                Link = "https://www.npgsql.org/",
                Factory = Npgsql.NpgsqlFactory.Instance,
                FactoryAsString = "Npgsql.NpgsqlFactory.Instance"

            });

            list.Add(new ProviderInfo("System.Data.SQLite")
            {
                Name = "SQLite Data Provider",
                Description = ".NET Framework Data Provider for SQLite.",
                Company = null,
                ProductImage = ProviderInfo.GetProductImageFromFilename("sqlite.png"),
                Link = "https://system.data.sqlite.org",
                Factory = System.Data.SQLite.SQLiteFactory.Instance,
                FactoryAsString = "System.Data.SQLite.SQLiteFactory.Instance"
            });

            // https://github.com/aspnet/Microsoft.Data.Sqlite/wiki/Data-Type-Mappings

            list.Add(new ProviderInfo("Microsoft.Data.Sqlite")
            {
                Name = "Microsoft SQLite Data Provider",
                Description = "SQLite implementation of the System.Data.Common provider model.",
                Company = "Microsoft",
                ProductImage = ProviderInfo.GetProductImageFromFilename("sqlite.png"),
                Link = "https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite",
                Factory = Microsoft.Data.Sqlite.SqliteFactory.Instance,
                FactoryAsString = "Microsoft.Data.Sqlite.SqliteFactory.Instance"

            });

            list.Add(new ProviderInfo("MySql.Data.MySqlClient")
            {
                Name = "MySQL Data Provider",
                Description = ".Net Framework Data Provider for MySQL.",
                Company = "Oracle Corporation",
                ProductImage = ProviderInfo.GetProductImageFromFilename("mysql.png"),
                Link = "https://dev.mysql.com/downloads/connector/net",
                Factory = MySql.Data.MySqlClient.MySqlClientFactory.Instance,
                FactoryAsString = "MySql.Data.MySqlClient.MySqlClientFactory.Instance"

            }); ;

            list.Add(new ProviderInfo("System.Data.Odbc")
            {
                Name = "Odbc Data Provider",
                Description = ".Net Framework Data Provider for Odbc.",
                Company = "Microsoft",
                Factory = System.Data.Odbc.OdbcFactory.Instance,
                FactoryAsString = "System.Data.Odbc.OdbcFactory.Instance"

            }); ;

            list.Add(new ProviderInfo("System.Data.OleDb")
            {
                Name = "OleDb Data Provider",
                Description = ".Net Framework Data Provider for OleDb.",
                Company = "Microsoft",
                Factory = System.Data.OleDb.OleDbFactory.Instance,
                FactoryAsString = "System.Data.OleDb.OleDbFactory.Instance"

            }); ; ;

            #region NOT-INSTALLED

            list.Add(new ProviderInfo("System.Data.OracleClient")
            {
                Name = "OracleClient Data Provider",
                Description = ".Net Framework Data Provider for Oracle.",
                Company = "Microsoft",
                ProductImage = ProviderInfo.GetProductImageFromFilename("oracle.png")

            });

            list.Add(new ProviderInfo("IBM.Data.DB2.dll")
            {
                Name = "IBM DB2 Data Provider for .NET",
                Description = ".Net Framework Data Provider for DB2.",
                Company = "IBM Corporation",
                ProductImage = ProviderInfo.GetProductImageFromFilename("db2.png"),
                Link = "https://www.ibm.com/support/knowledgecenter/en/SSEPGG_11.1.0/com.ibm.swg.im.dbclient.adonet.doc/doc/c0010960.html"

            });

            list.Add(new ProviderInfo("MariaDB546426") // This provider will never be found. As intented.
            {
                Name = "MariaDB uses the 'MySQL Data Provider'",
                Description = "Install the 'MySQL Data Provider' for connecting to MariaDB.",
                ProductImage = ProviderInfo.GetProductImageFromFilename("mariadb.png"),
                Link = "https://mariadb.com/kb/en/library/adonet/"

            });

            list.Add(new ProviderInfo("Devart.Data.SQLite")
            {
                Name = "dotConnect for SQLite",
                Description = "Devart dotConnect for SQLite.",
                Company = "Devart",
                ProductImage = ProviderInfo.GetProductImageFromFilename("sqlite.png"),
                Link = "https://www.devart.com/dotconnect/sqlite/download.html"

            });

            list.Add(new ProviderInfo("System.Data.CData.Exchange")
            {
                Name = "CData ADO.NET Provider for Exchange",
                Description = "CData ADO.NET Provider for Exchange.",
                Company = "CData Software",
                ProductImage = ProviderInfo.GetProductImageFromFilename("exchange.png"),
                Link = "https://www.cdata.com/ado/"

            });

            list.Add(new ProviderInfo("System.Data.CData.Facebook")
            {
                Name = "CData ADO.NET Provider for Facebook",
                Description = "CData ADO.NET Provider for Facebook.",
                Company = "CData Software",
                ProductImage = ProviderInfo.GetProductImageFromFilename("facebook.png"),
                Link = "https://www.cdata.com/ado/"

            });

            list.Add(new ProviderInfo("System.Data.CData.Twitter")
            {
                Name = "CData ADO.NET Provider for Twitter",
                Description = "CData ADO.NET Provider for Twitter.",
                Company = "CData Software",
                ProductImage = ProviderInfo.GetProductImageFromFilename("twitter.png"),
                Link = "https://www.cdata.com/ado/"

            });

            #endregion

            return list;
        }

    }
}
