using Microsoft.Azure.Management.Sql.Fluent.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SqlBuildManager.Console.Kubernetes.Yaml
{
    internal class AzureIdentityYaml
    {
        public AzureIdentityYaml(string identityName, string resourceId, string clientId)
        {
            Name = identityName;
            spec.resourceID = resourceId;
            spec.clientID = clientId;
            metadata = new Dictionary<string, string>
            {
                 { "name", Name },
                 { "namespace", KubernetesManager.SbmNamespace }
            };
        }

        [YamlIgnore()]
        public static string Name { get; private set; } = "idname";
        [YamlIgnore()]
        public static string Kind { get { return "AzureIdentity"; } }
       
        [YamlMember(Order = 1)]
        public string apiVersion { get { return "aadpodidentity.k8s.io/v1"; } }

        [YamlMember(Order = 2)]
        public string kind { get { return Kind; } }
        [YamlMember(Order = 3)]
        public Dictionary<string, string> metadata;
        [YamlMember(Order = 4)]
        public IdentitySpec spec = new IdentitySpec();
    }
    internal class IdentitySpec
    {
        public string type { get { return "0"; } }
        public string resourceID { get; internal set; }
        public string clientID { get; internal set; }

    }
}
