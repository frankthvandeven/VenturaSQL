using System;

namespace VenturaSQLStudio
{
    /// <summary>
    /// Contains the ADO.NET Provider Invariant Name. This normally matches the Namespace
    /// of the provider's assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, Inherited = false)]
    public sealed class ProviderInvariantNameAttribute : Attribute
    {
        private string _provider_invariant_name;

        /// <summary>
        /// Specify the release date and time in UTC format.
        /// </summary>
        public ProviderInvariantNameAttribute(string provider_invariant_name)
        {
            _provider_invariant_name = provider_invariant_name;
        }

        public string ProviderInvariantName
        {
            get { return _provider_invariant_name; }
        }

    }
}