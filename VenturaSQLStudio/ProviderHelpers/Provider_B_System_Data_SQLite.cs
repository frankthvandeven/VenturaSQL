using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.ProviderHelpers
{
    [ProviderInvariantName("System.Data.SQLite")]
    public class Provider_System_Data_SQLite : ProviderHelper 
    {

        public Provider_System_Data_SQLite()
        {
            Name = "SQLite Data Provider";
            Description = ".NET Framework Data Provider for SQLite.";
            Company = null;
            ProductImage = GetProductImageFromFilename("sqlite.png");
            Link = "https://system.data.sqlite.org";
            Factory = System.Data.SQLite.SQLiteFactory.Instance;
            FactoryAsString = "System.Data.SQLite.SQLiteFactory.Instance";

        }


    }
}
