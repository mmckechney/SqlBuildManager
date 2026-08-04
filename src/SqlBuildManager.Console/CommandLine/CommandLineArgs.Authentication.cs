using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel;

namespace SqlBuildManager.Console.CommandLine
{
    public  partial class CommandLineArgs
    {
        
        
        [JsonIgnore]
        public virtual string UserName
        {
            set
            {
                if (AuthenticationArgs == null) AuthenticationArgs = new Authentication();
                AuthenticationArgs.UserName = value;
                this.DirectPropertyChangeTracker.Add("Authentication.UserName");
            }
        }
        [JsonIgnore]
        public virtual string Password
        {
            set
            {
                if (AuthenticationArgs == null) AuthenticationArgs = new Authentication();
                AuthenticationArgs.Password = value;
                this.DirectPropertyChangeTracker.Add("Authentication.Password");
            }
        }

        [JsonIgnore]
        public virtual SqlBuildManager.Connection.AuthenticationType AuthenticationType
        {
            set 
            { 
                AuthenticationArgs.AuthenticationType = value;
                this.DirectPropertyChangeTracker.Add("Authentication.AuthenticationType");
            }
        }

        [JsonIgnore]
        public virtual SqlBuildManager.Connection.DatabasePlatform DatabasePlatform
        {
            set
            {
                AuthenticationArgs.DatabasePlatform = value;
                this.DirectPropertyChangeTracker.Add("Authentication.DatabasePlatform");
            }
        }

        [JsonIgnore]
        public virtual bool TrustServerCertificate
        {
            set
            {
                if (AuthenticationArgs == null) AuthenticationArgs = new Authentication();
                AuthenticationArgs.TrustServerCertificate = value;
                this.DirectPropertyChangeTracker.Add("Authentication.TrustServerCertificate");
            }
        }

        [Serializable]
        public class Authentication : ArgsBase
        {
            public virtual string UserName { get; set; } = string.Empty;
            public virtual string Password { get; set; } = string.Empty;

            [JsonConverter(typeof(JsonStringEnumConverter))]
            [DefaultValue(SqlBuildManager.Connection.AuthenticationType.Password)]
            public SqlBuildManager.Connection.AuthenticationType AuthenticationType { get; set; } = SqlBuildManager.Connection.AuthenticationType.Password;

            [JsonConverter(typeof(JsonStringEnumConverter))]
            [DefaultValue(SqlBuildManager.Connection.DatabasePlatform.SqlServer)]
            public SqlBuildManager.Connection.DatabasePlatform DatabasePlatform { get; set; } = SqlBuildManager.Connection.DatabasePlatform.SqlServer;

            [DefaultValue(false)]
            public bool TrustServerCertificate { get; set; } = false;
        }
    }
}
