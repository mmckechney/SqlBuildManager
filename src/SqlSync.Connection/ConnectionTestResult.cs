using System;

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
        } = string.Empty;
        private string databaseName = string.Empty;
        public string DatabaseName
        {
            get;
            set;
        } = string.Empty;


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

        private string managedIdentityClientId = string.Empty;
        public string ManagedIdentityClientId
        {
            get { return managedIdentityClientId; }
            set { managedIdentityClientId = value; }
        }
        private string successful = string.Empty;
        public bool Successful
        {
            get;
            set;
        }

    }
}
