using Microsoft.Azure.Batch;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using SqlBuildManager.Console.ContainerApp.Internal;
using System.Collections.Generic;
using System.Net.Http.Headers;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using static SqlBuildManager.Console.Kubernetes.Yaml.Containers;

namespace SqlBuildManager.Console.Kubernetes.Yaml
{
    public class JobYaml
    {
        public JobYaml(string k8jobname, string k8ConfigMapName, string k8SecretsName, string k8SecretsProviderName, string registry, string image, string tag, int podCount, bool hasKeyVault, bool useManagedIdentity, string serviceAccountName)
        {

            if (!string.IsNullOrWhiteSpace(k8jobname))
            {
                JobYaml.k8jobname = k8jobname;
            }


            if (!registry.EndsWith("azurecr.io"))
            {
                registry += ".azurecr.io";
            }

            spec = new JobSpec(k8jobname, k8ConfigMapName, k8SecretsName, serviceAccountName, podCount);
            spec.template.spec.containers[0].image = $"{registry}/{image}:{tag}";

            if (useManagedIdentity || hasKeyVault) //remove secrets volume and mount
            {
                spec.template.spec.volumes = new List<Dictionary<string, object>> { spec.template.spec.volumes[0] }.ToArray();
                spec.template.spec.containers[0].volumeMounts = new Mounts[] { spec.template.spec.containers[0].volumeMounts[0] };
            }

            metadata = new JobMetaData(k8jobname);

        }
        [YamlIgnore()]
        public static string k8jobname { get; private set; } = "sqlbuildmanager";
        [YamlIgnore()]
        public static string Kind { get { return "Job"; } }


        [YamlMember(Order = 1)]
        public string apiVersion { get { return "batch/v1"; } }
        [YamlMember(Order = 2)]
        public string kind { get { return Kind; } }


        [YamlMember(Order = 3)]
        public JobMetaData metadata;
        [YamlMember(Order = 4)]
        public JobSpec spec;
   
    }
   
    public class JobMetaData
    {
        public JobMetaData(string jobName)
        {
            name = jobName;
            this.labels = new JobLabels(jobName);
        }
        [YamlMember(Order = 1)]
        public string name { get; set; }
        [YamlMember(Order = 2)]
        public string @namespace { get; set; } = KubernetesManager.SbmNamespace;
        [YamlMember(Order = 3)]
        public JobLabels labels;
    }
    public class JobLabels
    {
        public JobLabels(string jobName)
        {
            jobgroup = jobName;
        }
        public string jobgroup { get; set; }

        [YamlMember(Alias = "azure.workload.identity/use", ScalarStyle =ScalarStyle.SingleQuoted)]
        public bool workloadIdentity { get; set; } = true;

    }
    public class JobSpec
    {
        private int podCount;
        public JobSpec(string k8jobname, string k8ConfigMapName, string k8SecretsName, string serviceAccountName, int podCount)
        {
            template = new JobTemplate(k8jobname, k8ConfigMapName, k8SecretsName, serviceAccountName);
            this.podCount = podCount;
        }

        [YamlMember(Order = 1)]
        public string parallelism { get { return $"{podCount}"; } }
        [YamlMember(Order = 2)]
        public JobTemplate template;
    }
    public class JobTemplate
    {

        public JobTemplate(string k8jobname, string k8ConfigMapName, string k8SecretsName, string serviceAccountName)
        {
            metadata = new JobTemplateMetaData(k8jobname);
            spec = new ContainerSpec(k8jobname, k8ConfigMapName, k8SecretsName, serviceAccountName);
        }
        public JobTemplateMetaData metadata;
        public ContainerSpec spec;

    }
    public class JobTemplateMetaData
    {
        public JobTemplateMetaData(string jobName)
        {
            labels = new JobLabels(jobName);
        }
        public JobLabels labels;
    }


    public class ContainerSpec
    {
        private string _serviceAccountName;
        public ContainerSpec(string k8jobname, string k8ConfigMapName, string k8SecretsName, string serviceAccountName)
        {
            volumes = new Dictionary<string, object>[]
            {
                new  Dictionary<string, object>
                {
                    {"name", "runtime" },
                    {"configMap", new Dictionary<string,string>
                        {
                            { "name", k8ConfigMapName}
                        }
                    }
                },
                new  Dictionary<string, object>
                {
                    {"name", "sbm" },
                    {"secret", new Dictionary<string,string>
                        {
                            { "secretName", k8SecretsName}
                        }
                    }
                }
            };
            containers = new Containers[1] { new Containers(k8jobname) };
            _serviceAccountName = serviceAccountName;
        }
        [YamlMember(Order = 1)]
        public Dictionary<string, string> nodeSelector = new Dictionary<string, string>
        {
            { "kubernetes.io/os","linux"}
        };

        [YamlMember(Order = 2)]
        public string serviceAccountName { get => _serviceAccountName; }
        [YamlMember(Order = 3)]
        public Containers[] containers;

        [YamlMember(Order = 4)]
        public string restartPolicy { get => "Never"; }

        [YamlMember(Order = 5)]
        public Dictionary<string, object>[] volumes;
    }
    public class Containers
    {
        private static string k8jobname = string.Empty;
        internal Containers(string k8jobname)
        {
            Containers.k8jobname = k8jobname;
        }
        [YamlMember(Order = 1)]
        public string name { get => k8jobname; }
        [YamlMember(Order = 2)]
        public string image { get; internal set; }
        [YamlMember(Order = 3)]
        public string imagePullPolicy { get => "Always"; }

        [YamlMember(Order = 5)]
        public Dictionary<string, object> resources = new Dictionary<string, object>
        {
            {"limits",  new Dictionary<string, string>
                {
                    {"memory" , "512M" },
                    {"cpu" , "500m" }
                }
            }
        };
        [YamlMember(Order = 6)]
        public List<string> command = new List<string> { "dotnet", "sbm.dll", "k8s", "worker" };
        [YamlMember(Order = 7)]
        public Mounts[] volumeMounts = new Mounts[]
            {
                new Mounts(){ name = "runtime", mountPath = "/etc/runtime" , readOnly = "true"},
                new Mounts(){ name = "sbm",mountPath = "/etc/sbm", readOnly = "true" }
            };


        public class Mounts
        {
            public string name { get; internal set; }
            public string mountPath { get; internal set; }
            public string readOnly { get; internal set; }
        }
    }
}


/*
 apiVersion: batch/v1
kind: Job
metadata:
  name: sqlbuildmanager
  labels:
    jobgroup: sqlbuildmanager
spec:
  parallelism: 2
  template:
    metadata:
      labels:
        jobgroup: sqlbuildmanager
        aadpodidbinding: azure-pod-identity-binding-selector
    spec:
      containers:
      - name: sqlbuildmanager
        image: sbm003containerregistry.azurecr.io/sqlbuildmanager:latest-vNext
        imagePullPolicy: Always
        resources: 
          limits:
            memory: "512M"
            cpu: "500m"
        command:
          - dotnet
          - sbm.dll
          - k8s
          - worker
        volumeMounts :
          - name: sbm
            mountPath: "/etc/sbm"
            readOnly: true
          - name: runtime
            mountPath: "/etc/runtime"
            readOnly: true
      restartPolicy: OnFailure
      volumes:
      - name: runtime
        configMap:
          name: runtime-properties
      - name: sbm
        csi:
          driver: secrets-store.csi.k8s.io
          readOnly: true
          volumeAttributes:
            secretProviderClass: azure-kvname 
*/