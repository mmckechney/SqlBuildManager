using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Text.Json.Serialization;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineArgs
    {
        public string VnetName
        {
            set
            {
                NetworkArgs.VnetName = value;
                this.DirectPropertyChangeTracker.Add("Network.VnetName");
            }
        }
        public string SubnetName
        {
            set
            {
                NetworkArgs.SubnetName = value;
                this.DirectPropertyChangeTracker.Add("Network.SubnetName");
            }
        }
        public string VnetResourceGroup
        {
            set
            {
                NetworkArgs.ResourceGroup = value;
                this.DirectPropertyChangeTracker.Add("Network.ResourceGroup");
            }
        }
        public class Network :ArgsBase
        {
            public string VnetName { get; set; } = string.Empty;
            public string SubnetName { get; set; } = string.Empty;
            public string ResourceGroup { get; set; } = string.Empty;
        }
    }
}
