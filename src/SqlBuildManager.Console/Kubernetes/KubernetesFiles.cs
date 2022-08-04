using Microsoft.Azure.Batch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Policy;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Kubernetes
{
    public class KubernetesFiles
    {
        public string RuntimeConfigMapFile { get; set; }
        public string SecretsFile { get; set; }
        public string SecretsProviderFile { get; set; }
        public string AzureIdentityFileName { get;set; }
        public string AzureBindingFileName { get; set; }
        public string JobFileName { get; set; }
    }
}
