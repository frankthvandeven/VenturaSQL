using System;

namespace VenturaSQL
{
    public interface IRecordsetBase
    {
        byte[] Hash
        {
            get;
        }

        string HashString
        {
            get;
        }

        VenturaPlatform GeneratorTarget
        {
            get;
        }

        Version GeneratorVersion
        {
            get;
        }

        DateTime GeneratorTimestamp
        {
            get;
        }

        string GeneratorProviderInvariantName
        {
            get;
        }

        IResultsetBase[] Resultsets
        {
            get;
        }

        string SqlScript
        {
            get;
        }

        VenturaSchema ParameterSchema
        {
            get;
        }

        object[] InputParameterValues
        {
            get;
        }

        object[] OutputParameterValues
        {
            get;
        }

        int RowLimit
        {
            get;
            set;
        }

    }

} // end of assembly
