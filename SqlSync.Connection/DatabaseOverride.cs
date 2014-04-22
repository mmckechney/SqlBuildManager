using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Xml.Serialization;
using System.Runtime.Serialization;
namespace SqlSync.Connection
{
    public class DatabaseOverride
    {
        private string defaultDbTarget = string.Empty;

        public string DefaultDbTarget
        {
            get { return defaultDbTarget; }
            set { defaultDbTarget = value; }
        }
        private string overrideDbTarget = string.Empty;

        public string OverrideDbTarget
        {
            get
            {
                if (this.overrideDbTarget.Trim().Length == 0)
                    return defaultDbTarget;
                else
                    return overrideDbTarget;
            }
            set { overrideDbTarget = value; }
        }

        public DatabaseOverride()
        {
        }
        public DatabaseOverride(string defaultDbTarget, string overrideDbTarget)
        {
            this.defaultDbTarget = defaultDbTarget;
            this.overrideDbTarget = overrideDbTarget;
        }
        public override string ToString()
        {
            return this.defaultDbTarget + ";"+ this.overrideDbTarget;
        }
        private List<QueryRowItem> queryRowData = new List<QueryRowItem>();

        public List<QueryRowItem> QueryRowData
        {
            get { return queryRowData; }
            set { this.queryRowData = value; }
        }
        public void AppendedQueryRowData(object[] dataArray, int startIndex, DataColumnCollection cols )
        {
            for (int i = startIndex; i < dataArray.Length; i++)
            {
                queryRowData.Add(new QueryRowItem(cols[i].ColumnName, dataArray[i].ToString()));
            }
        }
    }

    [Serializable]
    public struct QueryRowItem
    {
        private string columnName;

        [XmlAttribute()]
        public string ColumnName
        {
            get { return columnName; }
            set { this.columnName = value; }
        }
        private string value;

        [XmlAttribute()]
        public string Value
        {
            get { return this.value; }
            set { this.value = value; }

        }
        public QueryRowItem(string columnName, string value)
        {
            this.columnName = columnName.TrimEnd();
            this.value = value.TrimEnd();
        }
      
    }

    // Create a node sorter that implements the IComparer interface.
    public class DatabaseOverrideSorter : System.Collections.IComparer
    {
        public int Compare(object x, object y)
        {

            return string.Compare(((DatabaseOverride)x).OverrideDbTarget, ((DatabaseOverride)y).OverrideDbTarget);
        }
    }
}
