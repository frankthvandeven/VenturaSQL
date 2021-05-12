using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.ProviderHelpers
{
    [ProviderInvariantName("Microsoft.Data.Sqlite")]
    class Provider_Microsoft_Data_Sqlite : ProviderHelperBase
    {
        Provider_Microsoft_Data_Sqlite()
        {
            Name = "Microsoft SQLite Data Provider";
            Description = "SQLite implementation of the System.Data.Common provider model.";
            Company = "Microsoft";
            ProductImage = GetProductImageFromFilename("sqlite.png");
            Link = "https://docs.microsoft.com/en-us/dotnet/standard/data/sqlite";
            Factory = Microsoft.Data.Sqlite.SqliteFactory.Instance;
            FactoryAsString = "Microsoft.Data.Sqlite.SqliteFactory.Instance";

        }
    }
}
