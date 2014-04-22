using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Runtime.Serialization;
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
