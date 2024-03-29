﻿using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace SqlBuildManager.Console.Kubernetes.Yaml
{
    public class SecretYaml
    {
        public SecretYaml(string secretName)
        {
            Name = secretName;
            metadata = new Dictionary<string, string>
            {
             { "name", secretName },
             { "namespace", KubernetesManager.SbmNamespace }
            };
        }
        [YamlIgnore()]
        public static string Name { get; set; } = "connection-secrets";
        [YamlIgnore()]
        public static string Kind { get { return "Secret"; } }

        [YamlMember(Order = 1)]
        public string apiVersion = "v1";

        [YamlMember(Order = 2)]
        public string kind { get { return Kind; } }
        [YamlMember(Order = 3)]
        public Dictionary<string, string> metadata;
        [YamlMember(Order = 4)]
        public string type = "Opaque";
        [YamlMember(Order = 5)]
        public SecretsData data = new SecretsData();

    }
    public class SecretsData
    {
        public string EventHubConnectionString { get; set; } = null;
        public string ServiceBusTopicConnectionString { get; set; } = null;
        public string UserName { get; set; } = null;
        public string Password { get; set; } = null;
        public string StorageAccountKey { get; set; } = null;
    }
}
