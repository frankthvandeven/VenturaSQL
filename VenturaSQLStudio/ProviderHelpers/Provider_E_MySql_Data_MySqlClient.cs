using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.ProviderHelpers
{
    [ProviderInvariantName("MySql.Data.MySqlClient")]
    public class Provider_MySql_Data_MySqlClient : ProviderHelper
    {

        public Provider_MySql_Data_MySqlClient()
        {
            Name = "MySQL Data Provider";
            Description = ".Net Framework Data Provider for MySQL.";
            Company = "Oracle Corporation";
            ProductImage = GetProductImageFromFilename("mysql.png");
            Link = "https://dev.mysql.com/downloads/connector/net";
            Factory = MySql.Data.MySqlClient.MySqlClientFactory.Instance;
            FactoryAsString = "MySql.Data.MySqlClient.MySqlClientFactory.Instance";
        }


    }
}
