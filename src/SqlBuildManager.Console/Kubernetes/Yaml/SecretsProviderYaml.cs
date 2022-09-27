using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace SqlBuildManager.Console.Kubernetes.Yaml
{
    internal class SecretsProviderYaml
    {
        public SecretsProviderYaml(string providerName, string keyVaultName, string tentantId, string clientId)
        {
            secretsProviderName = providerName; 
            if (!string.IsNullOrWhiteSpace(keyVaultName))
            {
                spec.parameters.keyvaultName = keyVaultName;
            }

            if (!string.IsNullOrWhiteSpace(tentantId))
            {
                spec.parameters.tenantId = tentantId;
            }

            if (!string.IsNullOrWhiteSpace(clientId))
            {
                spec.parameters.userAssignedIdentityID = clientId;
            }
             metadata = new Dictionary<string, string>
            {
                { "name", providerName },
                { "namespace", KubernetesManager.SbmNamespace }
            };
        }
        [YamlIgnore()]
        public static string secretsProviderName { get; private set; } = "azure-kvname";
        [YamlIgnore()]
        public static string Kind { get { return "SecretProviderClass"; } }

        [YamlMember(Order = 1)]
        public string apiVersion { get { return "secrets-store.csi.x-k8s.io/v1"; } }
        [YamlMember(Order = 2)]
        public string kind { get { return Kind; } }

        [YamlMember(Order = 3)]
        public Dictionary<string, string> metadata;
        
        [YamlMember(Order = 4)]
        public SecretsProviderSpec spec = new SecretsProviderSpec();

    }
    internal class SecretsProviderSpec
    {

        [YamlMember(Order = 1)]
        public string provider { get { return "azure"; } }
        [YamlMember(Order = 2)]
        public SecretsProviderParams parameters = new SecretsProviderParams();
    }
    internal class SecretsProviderParams
    {
        [YamlMember(Order = 1, ScalarStyle = YamlDotNet.Core.ScalarStyle.SingleQuoted)]
        public string usePodIdentity { get { return "false"; } }
        [YamlMember(Order = 2, ScalarStyle = YamlDotNet.Core.ScalarStyle.SingleQuoted)]
        public string useVMManagedIdentity { get { return "true"; } }
        [YamlMember(Order = 3)]
        public string userAssignedIdentityID = "";
        [YamlMember(Order = 4)]
        public string keyvaultName = "";
        [YamlMember(Order = 5)]
        public string tenantId = "";
    }



 
    internal class Secrets
    {
        public Secrets(string name)
        {
            objectName = name;
        }
        public string objectName = "";
        public string objectType { get { return "secret"; } }

    }

    internal class Objects
    {
        internal static string definition = @"
    objects:  |
      array:
        - |
          objectName: StorageAccountKey
          objectType: secret 
        - |
          objectName: StorageAccountName
          objectType: secret 
        - |
          objectName: EventHubConnectionString
          objectType: secret  
        - |
          objectName: ServiceBusTopicConnectionString
          objectType: secret
        - |
          objectName: UserName
          objectType: secret
        - |
          objectName: Password
          objectType: secret";
            
    }
}
