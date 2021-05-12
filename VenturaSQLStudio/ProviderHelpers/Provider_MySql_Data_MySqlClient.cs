using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.ProviderHelpers
{
    [ProviderInvariantName("MySql.Data.MySqlClient")]
    class Provider_MySql_Data_MySqlClient : ProviderHelperBase
    {

        Provider_MySql_Data_MySqlClient()
        {
            ProviderInvariantName = ""; // Attribute.GetCustomAttribute(this.GetType()).;
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
