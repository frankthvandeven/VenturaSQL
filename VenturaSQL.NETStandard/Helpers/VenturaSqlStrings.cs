using System;
using System.Collections.Generic;
using System.Text;

namespace VenturaSQL
{
    public static class VenturaSqlStrings
    {
        //public static readonly string GET_VALUE_NOT_INITIALIZED_MSG = "New record's '{0}' column cannot not be read, as it contains no value yet.";

        public static readonly string VALUE_NOT_SET_MSG = "Record {0}, column {1} primary key value not set. Value must to be set before calling Recordset.SaveChanges().";

        public static readonly string SET_NULL_MSG = "Column does not allow a null value.";

        public static readonly string CURRENT_RECORD_NOT_SET = "No CurrentRecord selected.";

        public static readonly string UNKNOWN_COLUMN_NAME = "Unknown column name {0}";

    }
}
