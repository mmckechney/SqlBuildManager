using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSync.Connection
{
    [Serializable]
    public class ConnectionTestResult
    {
        private string serverName = string.Empty;
        public string ServerName
        {
            get;
            set;
        }
        private string databaseName = string.Empty;
        public string DatabaseName
        {
            get;
            set;
        }
        private string successful = string.Empty;
        public bool Successful
        {
            get;
            set;
        }

    }
}
