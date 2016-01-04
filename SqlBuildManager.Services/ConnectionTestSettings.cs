using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
using SqlSync.Connection;

namespace SqlBuildManager.Services
{
    [DataContract]
    public class ConnectionTestSettings
    {

        
        //public System.Collections.ObjectModel.KeyedCollection<string, System.Collections.ObjectModel.Collection<string>> TargetServers
        [DataMember]
        public Dictionary<string,List<string>> TargetServers
        {
            get;
            set;
        }

        private AuthenticationType authenticationType = SqlSync.Connection.AuthenticationType.UserNamePassword;
        [DataMember(IsRequired = false)]
        public AuthenticationType AuthenticationType
        {
            get { return authenticationType; }
            set { authenticationType = value; }
        }

        private string dbUserName = string.Empty;
        [DataMember(IsRequired = false)]
        public string DbUserName
        {
            get { return dbUserName; }
            set { dbUserName = value; }
        }

        private string dbPassword = string.Empty;
        [DataMember(IsRequired = false)]
        public string DbPassword
        {
            get { return dbPassword; }
            set { dbPassword = value; }
        }


        public override string ToString()
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder();
            if(TargetServers != null)
                sb.AppendLine("Target Server Count: " + TargetServers.Count.ToString());
            else
                sb.AppendLine("Target Server Count: null");
      
            return sb.ToString();
        }
    }
}
