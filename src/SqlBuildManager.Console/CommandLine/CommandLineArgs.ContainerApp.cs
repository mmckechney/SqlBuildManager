using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineArgs
    {
        public string EnvironmentName
        {
            set
            {
                ContainerAppArgs.EnvironmentName = value;
                this.DirectPropertyChangeTracker.Add("ContainerApp.EnvironmentName");
            }
        }
        public string Location
        {
            set
            {
                ContainerAppArgs.Location = value;
                this.DirectPropertyChangeTracker.Add("ContainerApp.Location");
            }
        }
        public string ResourceGroup
        {
            set
            {
                ContainerAppArgs.ResourceGroup = value;
                this.DirectPropertyChangeTracker.Add("ContainerApp.ResourceGroup");
            }
        }
        public bool EnvironmentVariablesOnly
        {
            set
            {
                ContainerAppArgs.EnvironmentVariablesOnly = value;
                this.DirectPropertyChangeTracker.Add("ContainerApp.EnvironmentVariablesOnly");
            }
        }
        public int MaxContainers
        {
            set
            {
                ContainerAppArgs.MaxContainerCount = value;
                this.DirectPropertyChangeTracker.Add("ContainerApp.MaxContainerCount");
            }
        }
        public class ContainerApp : ArgsBase
        {
            public string EnvironmentName { get; set; } = string.Empty;
            public string SubscriptionId { get; set; } = string.Empty;
            public string ResourceGroup { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public int MaxContainerCount { get; set; } = 10;
            [JsonIgnore]
            [DefaultValue(false)]
            public bool RunningAsContainerApp { get; set; } = false;
            [JsonIgnore]
            [DefaultValue(false)]
            public bool EnvironmentVariablesOnly { get; set; } = false;
        }
    }
}
