using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace VenturaSQLStudio.ProviderHelpers
{
    [ProviderInvariantName("Npgsql")]
    public class Provider_Npgsql : ProviderHelper
    {

        public Provider_Npgsql()
        {
            Name = "PostgreSQL Data Provider for .NET";
            Description = ".Net Framework Data Provider for PostgreSQL.";
            Company = "Npgsql Development Team";
            ProductImage = GetProductImageFromFilename("PostgreSQL.png");
            Link = "https://www.npgsql.org/";
            Factory = Npgsql.NpgsqlFactory.Instance;
            FactoryAsString = "Npgsql.NpgsqlFactory.Instance";

        }
    }
}