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


        private AuthenticationType authenticationType = SqlSync.Connection.AuthenticationType.Password;
        public AuthenticationType AuthenticationType
        {
            get { return authenticationType; }
            set { authenticationType = value; }
        }

        private string dbUserName = string.Empty;
        public string DbUserName
        {
            get { return dbUserName; }
            set { dbUserName = value; }
        }

        private string dbPassword = string.Empty;
        public string DbPassword
        {
            get { return dbPassword; }
            set { dbPassword = value; }
        }
        private string successful = string.Empty;
        public bool Successful
        {
            get;
            set;
        }

    }
}
