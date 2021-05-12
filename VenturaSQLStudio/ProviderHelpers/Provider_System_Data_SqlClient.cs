using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.ProviderHelpers
{
    [ProviderInvariantName("System.Data.SqlClient")]
    class Provider_System_Data_SqlClient : ProviderHelperBase
    {

        Provider_System_Data_SqlClient()
        {
            ProviderInvariantName = ""; // Attribute.GetCustomAttribute(this.GetType()).;
            Name = "SqlClient Data Provider";
            Description = ".Net Framework Data Provider for SqlServer.";
            Company = "Microsoft";
            ProductImage = GetProductImageFromFilename("sql_server.png");
            Factory = System.Data.SqlClient.SqlClientFactory.Instance;
            FactoryAsString = "System.Data.SqlClient.SqlClientFactory.Instance";
        }


    }
}
