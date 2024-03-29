﻿using System.Collections.Generic;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SqlBuildManager.Console.Kubernetes.Yaml
{
    public class ConfigmapYaml
    {
        public ConfigmapYaml(string k8ConfigMapName)
        {
            ConfigmapYaml.k8ConfigMapName = k8ConfigMapName;
            metadata = new Dictionary<string, string>
            {
                { "name", k8ConfigMapName },
                { "namespace", KubernetesManager.SbmNamespace }
            };
        }
        [YamlIgnore()]
        public static string k8ConfigMapName { get; private set; } = "runtime-properties";
        [YamlIgnore()]
        public static string Kind { get { return "ConfigMap"; } }

        [YamlMember(Order = 1)]
        public string apiVersion = "v1";

        [YamlMember(Order = 2)]
        public string kind { get { return Kind; } }

        [YamlMember(Order = 3)]
        public Dictionary<string, string> metadata;

        [YamlMember(Order = 4)]
        public RuntimeData data = new RuntimeData();

    }
    public class RuntimeData
    {
        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string DacpacName { get; set; } = null;

        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string PackageName { get; set; } = null;

        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string JobName { get; set; } = null;

        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string AllowObjectDelete { get; set; } = null;

        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string Concurrency { get; set; } = null;

        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string ConcurrencyType { get; set; } = null;
        public string AuthType { get; set; } = null;
        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string ServiceBusTopicConnectionString { get; set; } = null;

        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string EventHubConnectionString { get; set; } = null;
        public string StorageAccountName { get; set; } = null;

        public string KeyVaultName { get; set; } = null;
        public string QueryFile { get; set; } = null;
        public string OutputFile { get; set; } = null;

        [YamlMember(ScalarStyle = ScalarStyle.SingleQuoted)]
        public string EventHubLogging { get; set; } = null;

    }
}
