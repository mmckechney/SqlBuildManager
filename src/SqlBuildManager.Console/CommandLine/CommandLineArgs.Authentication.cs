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
        public Authentication AuthenticationArgs { get; set; } = new Authentication();
        
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
        public virtual SqlSync.Connection.AuthenticationType AuthenticationType
        {
            set 
            { 
                AuthenticationArgs.AuthenticationType = value;
                this.DirectPropertyChangeTracker.Add("Authentication.AuthenticationType");
            }
        }
        [Serializable]
        public class Authentication : ArgsBase
        {
            public virtual string UserName { get; set; } = string.Empty;
            public virtual string Password { get; set; } = string.Empty;

            [JsonConverter(typeof(JsonStringEnumConverter))]
            [DefaultValue(SqlSync.Connection.AuthenticationType.Password)]
            public SqlSync.Connection.AuthenticationType AuthenticationType { get; set; } = SqlSync.Connection.AuthenticationType.Password;
        }
    }
}
