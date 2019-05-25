using System;
using System.Collections.Generic;
using System.Text;

namespace SqlSync.SqlBuild.MultiDb
{
    [Serializable()]
    public class MultiDbQueryConfig 
    {
        private string sourceServer = string.Empty;

        public string SourceServer
        {
            get { return sourceServer; }
            set { sourceServer = value; }
        }
        private string database = string.Empty;

        public string Database
        {
            get { return database; }
            set { database = value; }
        }
        private string query = string.Empty;

        public string Query
        {
            get { return query; }
            set { query = value; }
        }

        public MultiDbQueryConfig(string sourceServer, string database, string query) :this()
        {
            this.sourceServer = sourceServer;
            this.database = database;
            this.query = query;
        }
        public MultiDbQueryConfig()        {
        
        }
    }
}
