using System;
using System.Collections;
using System.Windows.Forms;
namespace SqlSync
{
    public class ColumnSorter : IComparer
    {
        private int currentColumn = 0;
        private int lastColumn = 0;
        private Type expectedType = null;
        public bool IncludeNonSchemaSortOption
        {
            get;
            set;
        }
        private bool sortBySchemaPrefix = true;
        public int CurrentColumn
        {
            set
            {
                if (currentColumn != lastColumn)
                {
                    expectedType = null;
                    sort = SortOrder.Ascending;
                }
                else if (currentColumn == lastColumn)
                {
                    //If never include schema split (i.e. we will only do a full string sorting...
                    if (!IncludeNonSchemaSortOption)
                    {
                        if (sort == SortOrder.Ascending)
                            sort = SortOrder.Descending;
                        else
                            sort = SortOrder.Ascending;
                    }
                    else // Desired sequence: full string Asc, full string Desc, object name asc, object name desc
                    {
                        if (sort == SortOrder.Ascending)
                        {
                            if (sortBySchemaPrefix)
                                sort = SortOrder.Descending;
                            else
                            {
                                sortBySchemaPrefix = false;
                                sort = SortOrder.Descending;
                            }
                        }
                        else
                        {
                            if (sortBySchemaPrefix)
                            {
                                sortBySchemaPrefix = false;
                                sort = SortOrder.Ascending;
                            }
                            else
                            {
                                sortBySchemaPrefix = true;
                                sort = SortOrder.Ascending;
                            }
                        }
                    }
                }
                else if (sort == SortOrder.Ascending)
                    sort = SortOrder.Descending;
                else
                    sort = SortOrder.Ascending;


                lastColumn = currentColumn;
                currentColumn = value;

            }
            get
            {
                return currentColumn;
            }

        }
        private SortOrder sort = SortOrder.Ascending;

        public SortOrder Sort
        {
            get { return sort; }
            set { sort = value; }
        }

        public int Compare(object x, object y)
        {
            int returnVal = -1;
            string strX = ((ListViewItem)x).SubItems[currentColumn].Text;
            string strY = ((ListViewItem)y).SubItems[currentColumn].Text;
            if (!sortBySchemaPrefix)
            {
                if (strX.IndexOf('.') > -1)
                    strX = strX.Split('.')[1];

                if (strY.IndexOf('.') > -1)
                    strY = strY.Split('.')[1];
            }

            if (strX.Length == 0 && strY.Length == 0 && currentColumn == 0)
            {
                strX = ((ListViewItem)x).ImageIndex.ToString();
                strY = ((ListViewItem)y).ImageIndex.ToString();
            }
            if (expectedType == null)
            {
                try
                {
                    DateTime.Parse(strX);
                    DateTime.Parse(strY);
                    expectedType = typeof(DateTime);
                }
                catch
                {
                    try
                    {
                        Decimal.Parse((strX == string.Empty) ? "-1000." : strX);
                        Decimal.Parse((strY == string.Empty) ? "-1000." : strY);
                        expectedType = typeof(Decimal);
                    }
                    catch
                    {
                        expectedType = typeof(string);
                    }
                }
            }


            switch (expectedType.ToString())
            {
                case "System.DateTime":
                    returnVal = CompareDates(strX, strY);
                    break;
                case "System.Decimal":
                    returnVal = CompareNumbers(strX, strY);
                    break;
                default:
                    returnVal = string.Compare(strX, strY);
                    break;
            }


            if (sort == SortOrder.Descending)
                returnVal *= -1;

            return returnVal;

        }

        private int CompareDates(string x, string y)
        {
            try
            {
                // Parse the two objects passed as a parameter as a DateTime.
                System.DateTime firstDate = DateTime.Parse(x);
                System.DateTime secondDate = DateTime.Parse(y);
                // Compare the two dates.
                return DateTime.Compare(firstDate, secondDate);
            }
            catch
            {
                return String.Compare(x, y);
            }

        }
        private int CompareNumbers(string x, string y)
        {
            try
            {
                Decimal first = Decimal.Parse(x);
                Decimal second = Decimal.Parse(y);
                if (first < second)
                    return -1;
                else if (first == second)
                    return 0;
                else if (first > second)
                    return 1;
            }
            catch
            {
                return String.Compare(x, y);
            }
            return -1;
        }

        public ColumnSorter()
        {

        }

    }

}

