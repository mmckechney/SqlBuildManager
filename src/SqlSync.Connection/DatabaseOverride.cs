using System;
using System.Collections.Generic;
using System.Data;
using System.Text.Json.Serialization;
using System.Xml.Serialization;

namespace SqlSync.Connection
{
    public class DatabaseOverride
    {
        public string ConcurrencyTag { get; set; } = string.Empty;
        public string Server { get; set; }
        public string DefaultDbTarget { get; set; } = string.Empty;
        private string overrideDbTarget = string.Empty;
        public string OverrideDbTarget
        {
            get
            {
                if (overrideDbTarget.Trim().Length == 0)
                    return DefaultDbTarget;
                else
                    return overrideDbTarget;
            }
            set { overrideDbTarget = value; }
        }

        public DatabaseOverride()
        {
        }
        public DatabaseOverride(string server, string defaultDbTarget, string overrideDbTarget, string tag = "")
        {
            this.Server = server;
            this.DefaultDbTarget = defaultDbTarget;
            this.overrideDbTarget = overrideDbTarget;
            this.ConcurrencyTag = tag;
        }
        public override string ToString()
        {
            if(this.ConcurrencyTag.Length > 0)
            {
                return DefaultDbTarget + ";" + overrideDbTarget + "#" + ConcurrencyTag;
            }
            else
            {
                return DefaultDbTarget + ";" + overrideDbTarget;
            }
           
        }
        private List<QueryRowItem> queryRowData = new List<QueryRowItem>();

        [JsonIgnore]
        public List<QueryRowItem> QueryRowData
        {
            get { return queryRowData; }
            set { queryRowData = value; }
        }
        public void AppendedQueryRowData(object[] dataArray, int startIndex, DataColumnCollection cols)
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
            set { columnName = value; }
        }
        private string value;

        [XmlAttribute()]
        public string Value
        {
            get { return value; }
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
