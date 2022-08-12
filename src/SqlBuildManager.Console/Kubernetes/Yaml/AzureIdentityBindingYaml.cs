using Microsoft.SqlServer.Management.Sdk.Sfc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace SqlBuildManager.Console.Kubernetes.Yaml
{
    internal class AzureIdentityBindingYaml
    {
        public AzureIdentityBindingYaml(string identityName)
        {
            spec.azureIdentity = identityName;
        }
        
        [YamlIgnore()]
        public static string Name { get; private set; } = "azure-pod-identity-binding";
        [YamlIgnore()] 
        public static string Kind { get { return "AzureIdentityBinding"; } }
       
        [YamlMember(Order = 1)]
        public string apiVersion { get { return "aadpodidentity.k8s.io/v1"; } }
        [YamlMember(Order = 2)]
        public string kind { get { return Kind; } }
        [YamlMember(Order = 3)]
        public Dictionary<string, string> metadata = new Dictionary<string, string>
            {
             { "name", Name },
             { "namespace", KubernetesManager.SbmNamespace }
            };
        [YamlMember(Order = 4)]
        public BindingSpec spec = new BindingSpec();
    }
    internal class BindingSpec
    {
        public string azureIdentity { get; internal set; }
        public string selector { get { return "azure-pod-identity-binding-selector"; } }
    }
}
