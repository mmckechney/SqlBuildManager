
namespace SqlSync.Connection {
    using System;
    
    
    [Serializable()]
    public partial class ConnectionData {
        
        private string _SQLServerName = string.Empty;
        
        private string _DatabaseName = string.Empty;
        
        private string _Password = string.Empty;
        
        private string _UserId = string.Empty;
        
        private string _StartingDirectory = string.Empty;
        
        private AuthenticationType authType = AuthenticationType.Password;
        
        private int _ScriptTimeout = 20;
        
        public ConnectionData() {
        
        }

        public ConnectionData(string serverName, string databaseName) : this()
        {
            this._SQLServerName = serverName;
            this._DatabaseName = databaseName;
            authType = AuthenticationType.Windows;
        }

        public virtual string SQLServerName {
            get {
                return this._SQLServerName;
            }
            set {
                this._SQLServerName = value;
            }
        }
        
        public virtual string DatabaseName {
            get {
                return this._DatabaseName;
            }
            set {
                this._DatabaseName = value;
            }
        }
        
        public virtual string Password {
            get {
                return this._Password;
            }
            set {
                this._Password = value;
            }
        }
        
        public virtual string UserId {
            get {
                return this._UserId;
            }
            set {
                this._UserId = value;
            }
        }
        
        public virtual string StartingDirectory {
            get {
                return this._StartingDirectory;
            }
            set {
                this._StartingDirectory = value;
            }
        }
        
        public virtual AuthenticationType AuthenticationType
        {
            get
            {
                return this.authType;
            }
            set
            {
                this.authType = value;
            }
        }
        
        public virtual int ScriptTimeout {
            get {
                return this._ScriptTimeout;
            }
            set {
                this._ScriptTimeout = value;
            }
        }
        
        public virtual bool Fill(ConnectionData dataClass) {
            try {
                this.SQLServerName = dataClass.SQLServerName;
                this.DatabaseName = dataClass.DatabaseName;
                this.Password = dataClass.Password;
                this.UserId = dataClass.UserId;
                this.StartingDirectory = dataClass.StartingDirectory;
                this.authType = dataClass.authType;
                this.ScriptTimeout = dataClass.ScriptTimeout;
                return true;
            }
            catch (System.Exception ex) {
                throw new System.ApplicationException("Error in the Auto-Generated: ConnectionData.Fill(ConnectionData) Method", ex);
            }
        }
        
        
    }
}
