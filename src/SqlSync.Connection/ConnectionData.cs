
namespace SqlSync.Connection
{
    using System;


    [Serializable()]
    public partial class ConnectionData
    {

        private string _SQLServerName = string.Empty;

        private string _DatabaseName = string.Empty;

        private string _Password = string.Empty;

        private string _UserId = string.Empty;

        private string _StartingDirectory = string.Empty;

        private AuthenticationType authType = AuthenticationType.Password;

        private int _ScriptTimeout = 20;

        public ConnectionData()
        {

        }

        public ConnectionData(string serverName, string databaseName) : this()
        {
            _SQLServerName = serverName;
            _DatabaseName = databaseName;
            authType = AuthenticationType.Windows;
        }

        public virtual string SQLServerName
        {
            get
            {
                return _SQLServerName;
            }
            set
            {
                _SQLServerName = value;
            }
        }

        public virtual string DatabaseName
        {
            get
            {
                return _DatabaseName;
            }
            set
            {
                _DatabaseName = value;
            }
        }

        public virtual string Password
        {
            get
            {
                return _Password;
            }
            set
            {
                _Password = value;
            }
        }

        public virtual string UserId
        {
            get
            {
                return _UserId;
            }
            set
            {
                _UserId = value;
            }
        }

        public virtual string StartingDirectory
        {
            get
            {
                return _StartingDirectory;
            }
            set
            {
                _StartingDirectory = value;
            }
        }

        public virtual AuthenticationType AuthenticationType
        {
            get
            {
                return authType;
            }
            set
            {
                authType = value;
            }
        }

        public virtual int ScriptTimeout
        {
            get
            {
                return _ScriptTimeout;
            }
            set
            {
                _ScriptTimeout = value;
            }
        }

        public virtual ConnectionData Fill(ConnectionData dataClass)
        {
            try
            {
                SQLServerName = dataClass.SQLServerName;
                DatabaseName = dataClass.DatabaseName;
                Password = dataClass.Password;
                UserId = dataClass.UserId;
                StartingDirectory = dataClass.StartingDirectory;
                authType = dataClass.authType;
                ScriptTimeout = dataClass.ScriptTimeout;
                return this;
            }
            catch (System.Exception ex)
            {
                throw new System.ApplicationException("Error in the Auto-Generated: ConnectionData.Fill(ConnectionData) Method", ex);
            }
        }


    }
}
