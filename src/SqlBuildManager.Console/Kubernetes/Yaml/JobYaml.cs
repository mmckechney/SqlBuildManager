using Microsoft.Azure.Management.Compute.Fluent.VirtualMachine.Definition;
using Microsoft.Azure.Management.Graph.RBAC.Fluent.Models;
using Microsoft.SqlServer.Management.Sdk.Sfc;
using Microsoft.SqlServer.Management.Smo;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ContainerApp.Internal;
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
        public JobYaml(string k8jobname, string k8ConfigMapName, string k8SecretsName, string k8SecretsProviderName, string registry, string image, string tag, bool hasKeyVault, bool useManagedIdentity)
        {

            if (!string.IsNullOrWhiteSpace(k8jobname))
            {
                JobYaml.k8jobname = k8jobname;
            }


            if (!registry.EndsWith("azurecr.io"))
            {
                registry += ".azurecr.io";
            }

            spec = new JobSpec(k8jobname, k8ConfigMapName, k8SecretsName);
            spec.template.spec.containers[0].image = $"{registry}/{image}:{tag}";
            if (hasKeyVault) //replace volume with CSI driver
            {
                spec.template.spec.volumes = new List<Dictionary<string, object>> { spec.template.spec.volumes[0], new SecretsConfigs(k8SecretsProviderName).csi }.ToArray();
            }

            if (useManagedIdentity) //remove secrets volume and mount
            {
                spec.template.spec.volumes = new List<Dictionary<string, object>> { spec.template.spec.volumes[0] }.ToArray();
                spec.template.spec.containers[0].volumeMounts = new Mounts[] { spec.template.spec.containers[0].volumeMounts[0] };
            }

            metadata = new Dictionary<string, object>  {
                {"name", k8jobname },
                { "namespace", KubernetesManager.SbmNamespace },
                {"labels",  new Dictionary<string, string>
                    {
                        {"jobgroup" , k8jobname }
                    }
                }
            };


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
        public Dictionary<string, object> metadata;
        [YamlMember(Order = 4)]
        public JobSpec spec;
    }
    internal class JobSpec
    {

        public JobSpec(string k8jobname, string k8ConfigMapName, string k8SecretsName)
        {
            template = new JobTemplate(k8jobname, k8ConfigMapName, k8SecretsName);
        }
 
        [YamlMember(Order = 1)]
        public string parallelism { get { return "2"; } }
        [YamlMember(Order = 2)]
        public JobTemplate template;
    }
    internal class JobTemplate
    {
        
        public JobTemplate(string k8jobname, string k8ConfigMapName, string k8SecretsName)
        {
            metadata =  new Dictionary<string, object>
            {
                {"labels",  new Dictionary<string, string>
                    {
                        {"jobgroup" , k8jobname },
                        {"aadpodidbinding" , "azure-pod-identity-binding-selector" }
                    }
                }
            };
            spec = new ContainerSpec(k8jobname, k8ConfigMapName, k8SecretsName);
        }
        public Dictionary<string, object> metadata;
        public ContainerSpec spec;

    }

    internal class ContainerSpec
    {

        public ContainerSpec(string k8jobname, string k8ConfigMapName, string k8SecretsName)
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
        }
        [YamlMember(Order = 1)]
        public Dictionary<string, string> nodeSelector = new Dictionary<string, string>
        {
            { "kubernetes.io/os","linux"}
        };

        [YamlMember(Order = 2)]
        public Containers[] containers;

        [YamlMember(Order = 3)]
        public string restartPolicy { get { return "OnFailure"; } }

        [YamlMember(Order = 4)]
        public Dictionary<string, object>[] volumes;
    }
    internal class Containers
    {
        private static string k8jobname = string.Empty;
        internal Containers(string k8jobname)
        {
            Containers.k8jobname = k8jobname;
        }
        [YamlMember(Order = 1)]
        public string name { get { return k8jobname; } }
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
        public List<string> command = new List<string> { "dotnet", "sbm.dll", "k8s", "worker" };
        [YamlMember(Order = 7)]
        public Mounts[] volumeMounts = new Mounts[]
            {
                new Mounts(){ name = "runtime", mountPath = "/etc/runtime" , readOnly = "true"},
                new Mounts(){ name = "sbm",mountPath = "/etc/sbm", readOnly = "true" }
            };


        internal class Mounts
        {
            public string name { get; internal set; }
            public string mountPath { get; internal set; }
            public string readOnly { get; internal set; }
        }

        internal class SecretsConfigs
        {

            public SecretsConfigs(string k8SecretsProviderName)
            {

                csi = new Dictionary<string, object>
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
                                    { "secretProviderClass",k8SecretsProviderName }
                                }
                            }
                        }
                    }
                };
            }
            public Dictionary<string, object> csi;

           
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