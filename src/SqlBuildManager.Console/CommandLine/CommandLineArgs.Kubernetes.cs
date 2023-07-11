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
        public int PodCount
        {
            set
            {
                KubernetesArgs.PodCount = value;
                this.DirectPropertyChangeTracker.Add("Kubernetes.PodCount");

            }
        }
        public class Kubernetes :ArgsBase
        {
            [DefaultValue(10)]
            public int PodCount { get; set; } = 10;
            [JsonIgnore]
            [DefaultValue(false)]
            public bool RunningInKubernetes = false;
        }
    }
}
