using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.ProviderHelpers
{
    [ProviderInvariantName("System.Data.OleDb")]
    public class Provider_System_Data_OleDb : ProviderHelper
    {

        public Provider_System_Data_OleDb()
        {
            Name = "OleDb Data Provider";
            Description = ".Net Framework Data Provider for OleDb.";
            Company = "Microsoft";
            Factory = System.Data.OleDb.OleDbFactory.Instance;
            FactoryAsString = "System.Data.OleDb.OleDbFactory.Instance";
        }

    }
}
