using SqlSync.Connection;
using System.Collections.Generic;
namespace SqlSync.SqlBuild.MultiDb
{
    class MultiDbDataSorter : IComparer<KeyValuePair<string, List<DatabaseOverride>>>
    {
        public int Compare(KeyValuePair<string, List<DatabaseOverride>> x, KeyValuePair<string, List<DatabaseOverride>> y)
        {

            bool haveString = false;
            double dblX = -999, dblY = -999;
            string strX = string.Empty, strY = string.Empty;
            if (!double.TryParse(x.Key, out dblX))
            {
                strX = x.Key;
                haveString = true;
            }

            if (!double.TryParse(y.Key, out dblY))
            {
                strY = y.Key;
                haveString = true;
            }

            if (haveString)
            {
                if (strX.Length > 0 && strY.Length > 0)
                    return strX.CompareTo(strY);
                else if (strX.Length > 0)
                    return -1;
                else if (strY.Length > 0)
                    return 1;
                else
                    return 0;
            }
            else
            {
                if (dblX < dblY)
                    return -1;
                else if (dblX > dblY)
                    return 1;
                else
                    return 0;
            }
        }

        public MultiDbDataSorter()
        {

        }
    }
}

