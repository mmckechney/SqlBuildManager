using System;
using System.Collections.Generic;

namespace SqlSync.ObjectScript.Hash
{
    public class ObjectScriptHashReportData
    {
        public void ResetComparisonValues()
        {
            foreach (ObjectScriptHashData data in databaseData)
            {
                data.ResetComparisonValues();
            }
        }
        public ObjectScriptHashReportData()
        {
            databaseData = new List<ObjectScriptHashData>();
        }
        List<ObjectScriptHashData> databaseData;

        public List<ObjectScriptHashData> DatabaseData
        {
            get { return databaseData; }
            set { databaseData = value; }
        }
        string baseLineServer = string.Empty;

        public string BaseLineServer
        {
            get { return baseLineServer; }
            set { baseLineServer = value; }
        }
        string baseLineDatabase = string.Empty;

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
