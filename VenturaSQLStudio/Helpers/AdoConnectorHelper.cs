using System;
using System.Data.Common;
using System.Linq;
using VenturaSQL;

namespace VenturaSQLStudio
{
    public static class AdoConnectorHelper
    {
        /// <summary>
        /// Creates a Connector based on the provider invariant name.
        /// For example 'System.Data.SqlClient'
        /// </summary>
        public static AdoConnector Create(string provider_invariant_name, string connection_string = null)
        {
            if (string.IsNullOrEmpty(provider_invariant_name))
                throw new ArgumentOutOfRangeException("provider_invariant_name", "Empty or null string not allowed.");


            var provider_info = ProviderRepository.List.FirstOrDefault(z => z.ProviderInvariantName == provider_invariant_name);

            if (provider_info == null)
                throw new ArgumentOutOfRangeException("provider_invariant_name", $"Provider {provider_invariant_name} not found in repository.");

            if( provider_info.Factory == null)
                throw new Exception($"Provider DLLs for {provider_invariant_name} are not installed.");

            return new AdoConnector(provider_info.Factory, connection_string);
        }

    }
}
