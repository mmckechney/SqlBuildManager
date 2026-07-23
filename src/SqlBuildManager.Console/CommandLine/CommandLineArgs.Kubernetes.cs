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
            [DefaultValue(ExecutionOptions.DefaultKubernetesPodCount)]
            public int PodCount { get; set; } = ExecutionOptions.DefaultKubernetesPodCount;
            [JsonIgnore]
            [DefaultValue(false)]
            public bool RunningInKubernetes = false;
        }
    }
}
