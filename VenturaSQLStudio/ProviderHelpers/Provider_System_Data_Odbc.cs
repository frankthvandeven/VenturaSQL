using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.ProviderHelpers
{
    [ProviderInvariantName("System.Data.Odbc")]
    class Provider_System_Data_Odbc : ProviderHelperBase
    {

        Provider_System_Data_Odbc()
        {
            ProviderInvariantName = ""; // Attribute.GetCustomAttribute(this.GetType()).;
            Name = "Odbc Data Provider";
            Description = ".Net Framework Data Provider for Odbc.";
            Company = "Microsoft";
            Factory = System.Data.Odbc.OdbcFactory.Instance;
            FactoryAsString = "System.Data.Odbc.OdbcFactory.Instance";
        }

    }
}
