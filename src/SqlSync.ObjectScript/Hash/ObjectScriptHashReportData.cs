using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.ObjectScript.Hash
{
    public class ObjectScriptHashReportData
    {
        public void ResetComparisonValues()
        {
            foreach (ObjectScriptHashData data in this.databaseData)
            {
                data.ResetComparisonValues();
            }
        }
        public ObjectScriptHashReportData()
        {
            this.databaseData = new List<ObjectScriptHashData>();
        }
        List<ObjectScriptHashData> databaseData;

        public List<ObjectScriptHashData> DatabaseData
        {
            get { return databaseData; }
            set { databaseData = value; }
        }
        string baseLineServer;

        public string BaseLineServer
        {
            get { return baseLineServer; }
            set { baseLineServer = value; }
        }
        string baseLineDatabase;

        public string BaseLineDatabase
        {
            get { return baseLineDatabase; }
            set { baseLineDatabase = value; }
        }
        DateTime processTime;

        public DateTime ProcessTime
        {
            get { return processTime; }
            set { processTime = value; }
        }

    }
}
