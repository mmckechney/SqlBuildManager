using System;
using System.Collections.Generic;
using System.Text;
using SqlSync.Connection;
using System.IO;
using Microsoft.Extensions.Logging;
using System.Reflection;
namespace SqlSync.SqlBuild.AdHocQuery
{
    [Serializable()]
    public class QueryResultData
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private QueryResultData()
        {
        }
        public QueryResultData(string server, string database)
        {
            this.Server = server;
            this.Database = database;
        }

        public override string ToString()
        {
            return this.Server + "." + this.Database;
        }

        public string Server
        {
            get;
            set;
        }
        public string Database
        {
            get;
            set;
        }

        private int rowCount = 0;
        public int RowCount
        {
            get { return rowCount; }
            set { rowCount = value; }
        }

        private ColumnDefinition columnDefinition = new ColumnDefinition();

        public ColumnDefinition ColumnDefinition
        {
            get { return columnDefinition; }
            set { columnDefinition = value; }
        }

        private List<QueryRowItem> queryAppendData = new List<QueryRowItem>();
        public List<QueryRowItem> QueryAppendData
        {
            get { return queryAppendData; }
            set { queryAppendData = value; }
        }


        private List<Result> results = new List<Result>();
        public List<Result> Results
        {
            get { return results; }
            set { results = value; }
        }
        public string GetRowValuesCsvString()
        {
            try
            {
                StringBuilder sb = new System.Text.StringBuilder();
                int count = 1;
                foreach (Result r in this.results)
                {
                    sb.Append("\"" + this.Server + "\",\"" + this.Database + "\",\"" + this.rowCount.ToString() + "\",\"" + count.ToString() + "\",");
                    foreach (QueryRowItem item in queryAppendData)
                        sb.Append("\"" + item.Value + "\",");
                    sb.Append(r.GetCsvString() + "\r\n");
                    count++;
                }
                if (sb.Length == 0) // add in a zero record
                {
                    sb.Append("\"" + this.Server + "\",\"" + this.Database + "\",\"" + this.rowCount.ToString() + "\",\"" + count.ToString() + "\",");
                }
                return sb.ToString();
            }
            catch (OutOfMemoryException omExe)
            {
                log.LogError(omExe, "Error in GetRowValuesCsvString");
                return string.Empty;
            }

        }

        //public Stream GetRowValuesCsvStream()
        //{
        //    MemoryStream ms = new MemoryStream();
        //    using (StreamWriter sw = new StreamWriter(ms))
        //    {
        //        int count = 1;
        //        foreach (Result r in this.results)
        //        {

        //            sw.Write("\"" + this.Server + "\",\"" + this.Database + "\",\"" + this.rowCount.ToString() + "\",\"" + count.ToString() + "\",");
        //            foreach (QueryRowItem item in queryAppendData)
        //                sw.Write("\"" + item.Value + "\",");
        //            sw.Write(r.GetCsvString() + "\r\n");
        //            count++;
        //        }
        //    }
        //    return ms;
        //}

      

        public string GetColumnsCsvString()
        {
            try
            {
                StringBuilder sb = new System.Text.StringBuilder();
                sb.Append("\"Server\",\"Database\",\"RowCount\",\"Row #\",");
                foreach (QueryRowItem item in queryAppendData)
                    sb.Append("\"" + item.ColumnName + "\",");
                sb.Append(this.columnDefinition.GetCsvString());
                return sb.ToString();
            }
            catch (OutOfMemoryException omExe)
            {
                log.LogError(omExe, "Error in GetColumnsCsvString");
                return string.Empty;
            }
        }
    }

    [Serializable]
    public class Result : ResultsDictionary
    {
        public Result()
            : base("Row")
        {
        }

        public Result Copy()
        {
            Result dic = new Result();
            foreach (string key in this.Keys)
            {
                dic.Add(key, this[key]);
            }
            return dic;
         }

        public string GetCsvString()
        {
            StringBuilder sb = new System.Text.StringBuilder();
            foreach (string value in this.Values)
            {
                sb.Append("\"" + value + "\",");
            }
            if(sb.Length > 0)
                sb.Length = sb.Length - 1;
            return sb.ToString();
        }
    }

    [Serializable]
    public class ColumnDefinition : ResultsDictionary
    {
        public ColumnDefinition()
            : base("Definition")
        {
        }

        public string GetCsvString()
        {
            StringBuilder sb = new System.Text.StringBuilder();
            foreach(string columnName in this.Keys)
            {
                sb.Append("\"" + columnName + "\",");
            }
            if (sb.Length > 0)
                sb.Length = sb.Length - 1;
            return sb.ToString();
        }

       
       
    }
   
}
