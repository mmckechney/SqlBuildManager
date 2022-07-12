using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace SqlBuildManager.Console.Kubernetes.Yaml
{
    public class RuntimeYaml
    {
        public string apiVersion = "v1";
        public string kind = "ConfigMap";
        public Dictionary<string, string> metadata = new Dictionary<string, string>
            {
             { "name", "runtime-properties" }
            };

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

    }
}
