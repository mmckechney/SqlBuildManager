using System;
using System.Collections.Generic;
using System.Data.SqlClient;
namespace SqlSync.SprocTest
{
    class SqlParamListSorter : IComparer<List<SqlParameter>>
    {
        public int Compare(List<SqlParameter> x, List<SqlParameter> y)
        {
            if (x == null && y == null)
                return 0;

            if (x != null && y == null)
                return 1;

            if (x == null && y != null)
                return -1;

            if (x.Count > y.Count)
                return 1;

            if (x.Count < y.Count)
                return -1;

            if (x.Count == y.Count)
                return 0;

            throw new ArgumentException("Unable to compare List<StoredProcedure>");
        }
    }
}
