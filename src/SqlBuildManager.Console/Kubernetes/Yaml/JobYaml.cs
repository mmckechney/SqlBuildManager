using Microsoft.Azure.Management.Compute.Fluent.VirtualMachine.Definition;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using YamlDotNet.Serialization;
using static SqlBuildManager.Console.Kubernetes.Yaml.Containers;

namespace SqlBuildManager.Console.Kubernetes.Yaml
{
    internal class JobYaml
    {
        public JobYaml(string registry, string image, string tag, bool hasSecrets)
        {
            if (!registry.EndsWith("azurecr.io")) registry += ".azurecr.io";
            spec.template.spec.containers[0].image = $"{registry}/{image}:{tag}";
            if(hasSecrets)
            {
                spec.template.spec.volumes = new List<Dictionary<string, object>> { spec.template.spec.volumes[0], SecretsConfigs.csi }.ToArray();
                spec.template.spec.containers[0].volumeMounts = new Mounts[] { spec.template.spec.containers[0].volumeMounts[0], SecretsConfigs.sbm };
            }
        }
        [YamlMember(Order = 1)]
        public string apiVersion { get { return "batch/v1"; } }
        [YamlMember(Order = 2)]
        public string kind { get { return "Job"; } }
        [YamlMember(Order = 3)]
        public Dictionary<string, object> metadata = new Dictionary<string, object>
        {
            {"name","sqlbuildmanager" },
            {"labels",  new Dictionary<string, string>
                {
                    {"jobgroup" , "sqlbuildmanager" }
                }
            }
        };
        [YamlMember(Order = 4)]
        public JobSpec spec = new JobSpec();
    }
    internal class JobSpec
    {
        [YamlMember(Order = 1)]
        public string parallelism { get { return "2"; } }
        [YamlMember(Order = 2)]
        public JobTemplate template = new JobTemplate();
    }
    internal class JobTemplate
    {
        public Dictionary<string, object> metadata = new Dictionary<string, object>
        {
            {"labels",  new Dictionary<string, string>
                {
                    {"jobgroup" , "sqlbuildmanager" },
                    {"aadpodidbinding" , "azure-pod-identity-binding-selector" }
                }
            }
        };
        public ContainerSpec spec = new ContainerSpec();

    }

    internal class ContainerSpec
    {
        [YamlMember(Order = 1)]
        public string restartPolicy { get { return "OnFailure"; } }
        [YamlMember(Order = 2)]
        public Containers[] containers = new Containers[1] {new Containers()};
        [YamlMember(Order = 3)]
        public Dictionary<string, object>[] volumes = new Dictionary<string, object>[]
        {
            new  Dictionary<string, object>
            {
                {"name", "runtime" },
                {"configMap", new Dictionary<string,string>
                    {
                        { "name", "runtime-properties"}
                    }
                }
            }
        };
    }
    internal class Containers
    {
        [YamlMember(Order = 1)]
        public string name { get { return "sqlbuildmanager"; } }
        [YamlMember(Order = 2)]
        public string image { get; internal set; }
        [YamlMember(Order = 3)]
        public string imagePullPolicy { get { return "Always"; } }
        
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
        public string[] command = new string[] { "dotnet", "sbm.dll", "k8s", "worker" };
        [YamlMember(Order = 7)]
        public Mounts[] volumeMounts = new Mounts[]
            {
                new Mounts(){ name = "runtime", mountPath = "/etc/runtime" , readOnly = "true"}
            };
       
        internal class Mounts
        {
            public string name { get; internal set; }
            public string mountPath { get; internal set; }
            public string readOnly { get; internal set; }
        }

        internal class SecretsConfigs
        {
            public static Dictionary<string, object> csi = new Dictionary<string, object>
            {
                { "name", "sbm" },
                {
                    "csi",
                    new Dictionary<string, object>
                    {
                        { "driver", "secrets-store.csi.k8s.io" },
                        { "readOnly", "true" },
                        {
                            "volumeAttributes",
                            new Dictionary<string, string>
                            {
                                { "secretProviderClass", "azure-kvname" }
                            }
                        }
                    }
                }
            };

            public static Mounts sbm = new Mounts()
            {
                name = "sbm",
                mountPath = "/etc/sbm",
                readOnly = "true"
            };
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