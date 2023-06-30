using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;
using System.ComponentModel;
namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineArgs
    {
        public EventHub EventHubArgs { get; set; } = new EventHub();
        
        [JsonIgnore]
        public string EventHubResourceGroup
        {
            set
            {
                EventHubArgs.ResourceGroup = value; 
                this.DirectPropertyChangeTracker.Add("EventHub.ResourceGroup");
            }
        }

        [JsonIgnore]
        public string EventHubSubscriptionId
        {
            set
            {
                EventHubArgs.SubscriptionId = value;
                this.DirectPropertyChangeTracker.Add("EventHub.SubscriptionId");
            }
        }
  
        public class EventHub : ArgsBase
        {
            public string ResourceGroup { get; set; } = string.Empty;

            public string SubscriptionId { get; set; } = string.Empty;
          
        }
    }
}
