using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Kubernetes
{
    public class SecretYaml
    {
        public string apiVersion = "v1";
        public string kind = "Secret";
        public Dictionary<string, string> metadata = new Dictionary<string, string>
            {
             { "name", "connection-secrets" }
            };
        public string type = "Opaque";
        public SecretsData data = new Kubernetes.SecretsData();

    }
    public class SecretsData
    {
        public string EventHubConnectionString { get; set; } = null;
        public string ServiceBusTopicConnectionString { get; set; } = null;
        public string UserName { get; set; } = null;
        public string Password { get; set; } = null;
        public string StorageAccountName { get; set; } = null;
        public string StorageAccountKey { get; set; } = null;
    }
}
