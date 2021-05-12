using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.ProviderHelpers
{
    [ProviderInvariantName("System.Data.OleDb")]
    class Provider_System_Data_OleDb : ProviderHelperBase
    {

        Provider_System_Data_OleDb()
        {
            ProviderInvariantName = ""; // Attribute.GetCustomAttribute(this.GetType()).;
            Name = "OleDb Data Provider";
            Description = ".Net Framework Data Provider for OleDb.";
            Company = "Microsoft";
            Factory = System.Data.OleDb.OleDbFactory.Instance;
            FactoryAsString = "System.Data.OleDb.OleDbFactory.Instance";
        }

    }
}
