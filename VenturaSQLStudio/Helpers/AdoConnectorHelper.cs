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


            var providerHelper = MainWindow.ViewModel.ProviderRepository.FirstOrDefault(z => z.ProviderInvariantName == provider_invariant_name);

            if (providerHelper == null)
                throw new ArgumentOutOfRangeException("provider_invariant_name", $"Provider {provider_invariant_name} not found in repository.");

            if( providerHelper.Factory == null)
                throw new Exception($"Provider DLLs for {provider_invariant_name} are not installed.");

            return new AdoConnector(providerHelper.Factory, connection_string);
        }

    }
}
