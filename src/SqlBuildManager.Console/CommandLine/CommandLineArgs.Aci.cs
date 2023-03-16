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
        public Aci AciArgs { get; set; } = new Aci();
        
        public string AciName
        {
            set
            {
                AciArgs.AciName = value;
                this.DirectPropertyChangeTracker.Add("Aci.AciName");
            }
        }
        public string AciResourceGroup
        {
            set
            {
                AciArgs.ResourceGroup = value; 
                this.DirectPropertyChangeTracker.Add("Aci.ResourceGroup");
            }
        }
        public int ContainerCount
        {
            set
            {
                AciArgs.ContainerCount = value; 
                this.DirectPropertyChangeTracker.Add("Aci.ContainerCount");
            }

        }

        public string IdentityName { set { IdentityArgs.IdentityName = value; } }
        public class Aci : ArgsBase
        {
            public string AciName { get; set; } = string.Empty;

            public string ResourceGroup { get; set; } = string.Empty;

            public string SubscriptionId { get; set; } = string.Empty;
            [JsonIgnore]
            public int ContainerCount { get; set; }
          
        }
    }
}
