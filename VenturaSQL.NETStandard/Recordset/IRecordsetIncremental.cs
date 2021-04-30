using System;
using System.Threading.Tasks;

namespace VenturaSQL
{
    public interface IRecordsetIncremental
    {

        Connector IncrementalConnector
        {
            get;
            set;
        }

        int IncrementalOffset
        {
            get;
            set;
        }

        int LastExecCount
        {
            get;
            set;
        }

        int LastExecStartIndex
        {
            get;
            set;
        }

        bool HasMoreRows
        {
            get;
            set;
        }

        void ExecSqlIncremental();
        void ExecSqlNextPage();

        Task ExecSqlIncrementalAsync();
        Task ExecSqlNextPageAsync();

        int RowLimit
        {
            get;
            set;
        }
    }

} // end of assembly
