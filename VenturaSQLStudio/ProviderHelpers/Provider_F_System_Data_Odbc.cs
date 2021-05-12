using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.ProviderHelpers
{
    [ProviderInvariantName("System.Data.Odbc")]
    public class Provider_System_Data_Odbc : ProviderHelper
    {

        public Provider_System_Data_Odbc()
        {
            Name = "Odbc Data Provider";
            Description = ".Net Framework Data Provider for Odbc.";
            Company = "Microsoft";
            Factory = System.Data.Odbc.OdbcFactory.Instance;
            FactoryAsString = "System.Data.Odbc.OdbcFactory.Instance";
        }

    }
}
