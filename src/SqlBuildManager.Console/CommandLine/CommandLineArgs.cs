using Microsoft.Azure.Batch;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using System;
using System.Collections.Generic;
using System.IO;

namespace SqlBuildManager.Console.CommandLine
{

    [Serializable]
    public partial class CommandLineArgs //: System.ICloneable
    {

        public CommandLineArgs()
        {

        }

        [JsonIgnore]
        public bool Decrypted { get; set; } = false;
        
        #region Nested Argument Object properties
        public Authentication AuthenticationArgs { get; set; } = new Authentication();
        public Batch BatchArgs { get; set; } = new Batch();
        public Connections ConnectionArgs { get; set; } = new Connections();
        public Identity IdentityArgs { get; set; } = new Identity();
        public Aci AciArgs { get; set; } = new Aci();

        public Network NetworkArgs { get; set; } = new Network();
        [JsonIgnore]
        public DacPac DacPacArgs { get; set; } = new DacPac();
        [JsonIgnore]
        public Synchronize SynchronizeArgs { get; set; } = new Synchronize();
        [JsonIgnore]
        public AutoScripting AutoScriptingArgs { get; set; } = new AutoScripting();
        [JsonIgnore]
        public StoredProcTesting StoredProcTestingArgs { get; set; } = new StoredProcTesting();

        public ContainerApp ContainerAppArgs { get; set; } = new ContainerApp();

        public Kubernetes KubernetesArgs { get; set; } = new Kubernetes();

        public ContainerRegistry ContainerRegistryArgs { get; set; } = new ContainerRegistry();

        #endregion
        
        private string settingsFile = string.Empty;
        [JsonIgnore]
        public string SettingsFile
        {
            get
            {
                return settingsFile;
            }
            internal set
            {
                if (File.Exists(value))
                {
                    CommandLineArgs cmdLine = JsonConvert.DeserializeObject<CommandLineArgs>(File.ReadAllText(value));
                    BatchArgs = cmdLine.BatchArgs;
                    AuthenticationArgs = cmdLine.AuthenticationArgs;
                    ConnectionArgs = cmdLine.ConnectionArgs;
                    IdentityArgs = cmdLine.IdentityArgs;
                    AciArgs = cmdLine.AciArgs;
                    ContainerAppArgs = cmdLine.ContainerAppArgs;
                    ContainerRegistryArgs = cmdLine.ContainerRegistryArgs;
                    KubernetesArgs = cmdLine.KubernetesArgs;
                    NetworkArgs = cmdLine.NetworkArgs;
                    RootLoggingPath = cmdLine.RootLoggingPath;
                    DefaultScriptTimeout = cmdLine.DefaultScriptTimeout;
                    TimeoutRetryCount = cmdLine.TimeoutRetryCount;

                }
                settingsFile = value;
            }
        }
        [JsonIgnore]
        public FileInfo FileInfoSettingsFile
        {
            set
            {
                SettingsFile = value.FullName;
            }
        }

        [JsonIgnore]
        public bool AllowObjectDelete { get; set; } = false;
        public string Override
        {
            set
            {
                OverrideDesignated = true;
                var fileExt = new List<string> { ".multidb", ".cfg", ".multidbq", ".sql" };
                fileExt.ForEach(ex =>
                {
                    if (value.ToLower().Trim().EndsWith(ex))
                    {
                        MultiDbRunConfigFileName = value;
                    }
                });
                if (MultiDbRunConfigFileName == string.Empty)
                {
                    ManualOverRideSets = value;
                }
            }
        }
        [JsonIgnore]
        public string SettingsFileKey { get; set; }
        [JsonIgnore]
        public virtual string Server { get; set; } = string.Empty;

        /// <summary>
        /// Script file name for generating override cfg file
        /// </summary>
        [JsonIgnore]
        public virtual FileInfo ScriptFile { get; set; }
        [JsonIgnore]
        public virtual string ScriptText { get; set; }
        [JsonIgnore]
        public virtual string Database { get; set; } = string.Empty;
        public virtual string RootLoggingPath { get; set; } = string.Empty;

        [JsonIgnore]
        public virtual LogLevel LogLevel { get; set; } = LogLevel.Information;
        [JsonIgnore]
        public virtual bool Trial { get; set; } = false;
        [JsonIgnore]
        public virtual bool RunningAsContainer { get; set; } = false;
        [JsonIgnore]
        public virtual string ScriptSrcDir { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string LogToDatabaseName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string Description { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string BuildFileName { get; set; } = string.Empty;
        [JsonIgnore]
        public string Directory { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual bool Transactional { get; set; } = true;
        public virtual int TimeoutRetryCount { get; set; } = 0;
        [JsonIgnore]
        public bool ContinueOnFailure { get; set; }
        [JsonIgnore]
        public string BuildRevision { get; set; }
        [JsonIgnore]
        public string OutputSbm { get; set; }
        [JsonIgnore]
        public FileInfo[] Scripts { get; set; }
        [JsonIgnore]
        public string DacpacName
        {
            get
            {
                return dacpacName;
            }
            set
            {
                if (Path.GetExtension(value) == string.Empty)
                {
                    dacpacName = value + ".dacpac";
                }
                else
                {
                    dacpacName = value;
                }
            }
        }
        private string dacpacName = string.Empty;
        public virtual int DefaultScriptTimeout { get; set; } = 500;

        private string jobName = string.Empty;
        [JsonIgnore]
        public string JobName
        {
            get
            {
                return jobName;
            }
            set
            {
                jobName = value.ToLower();
                BatchJobName = value.ToLower();
            }
        }
        public virtual int Concurrency { get; set; } = 10;

        [JsonConverter(typeof(StringEnumConverter))]
        public virtual ConcurrencyType ConcurrencyType { get; set; } = ConcurrencyType.Count;

        [JsonIgnore]
        public bool Silent { get; set; }
        [JsonIgnore]
        public FileInfo QueryFile { get; set; }
        [JsonIgnore]
        public FileInfo OutputFile { get; set; }

        [JsonIgnore]
        public bool WhatIf { get; set; } = false;
        [JsonIgnore]
        public virtual string ServiceAccountName
        {
            set
            {
                if (IdentityArgs == null) IdentityArgs = new Identity();
                IdentityArgs.ServiceAccountName = value;
            }
        }

        #region Not direct (inferred) commandline options
        [JsonIgnore]
        public virtual bool OverrideDesignated { get; set; } = false;
        [JsonIgnore]
        public virtual string MultiDbRunConfigFileName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string ManualOverRideSets { get; set; } = string.Empty;
        #endregion

        #region Authentication Nested Class and property setters
        [JsonIgnore]
        public virtual string UserName
        {
            set
            {
                if (AuthenticationArgs == null) AuthenticationArgs = new Authentication();
                AuthenticationArgs.UserName = value;
            }
        }
        [JsonIgnore]
        public virtual string Password
        {
            set
            {
                if (AuthenticationArgs == null) AuthenticationArgs = new Authentication();
                AuthenticationArgs.Password = value;
            }
        }
        [JsonIgnore]
        public virtual SqlSync.Connection.AuthenticationType AuthenticationType
        {
            set { AuthenticationArgs.AuthenticationType = value; }
        }
        [Serializable]
        public class Authentication
        {
            public virtual string UserName { get; set; } = string.Empty;
            public virtual string Password { get; set; } = string.Empty;

            [JsonConverter(typeof(StringEnumConverter))]
            public SqlSync.Connection.AuthenticationType AuthenticationType { get; set; } = SqlSync.Connection.AuthenticationType.Password;
        }
        #endregion

        #region Batch Nested Class and property setters
        [JsonIgnore]
        public virtual string OutputContainerSasUrl
        {
            set { BatchArgs.OutputContainerSasUrl = value; }
        }
        [JsonIgnore]
        public virtual bool DeleteBatchPool
        {
            set { BatchArgs.DeleteBatchPool = value; }
        }
        [JsonIgnore]
        public virtual bool DeleteBatchJob
        {
            set { BatchArgs.DeleteBatchJob = value; }
        }
        [JsonIgnore]
        public virtual int BatchNodeCount
        {
            set { BatchArgs.BatchNodeCount = value; }
        }
        [JsonIgnore]
        public virtual string BatchJobName
        {
            set
            {
                BatchArgs.BatchJobName = value.ToLower();
                jobName = value.ToLower();
            }
        }


        [JsonIgnore]
        public virtual string BatchVmSize
        {
            set { BatchArgs.BatchVmSize = value; }
        }
        [JsonIgnore]
        public virtual string BatchPoolName
        {
            set { BatchArgs.BatchPoolName = value; }
        }

        [JsonIgnore]
        public virtual bool PollBatchPoolStatus
        {
            set { BatchArgs.PollBatchPoolStatus = value; }
        }
        [JsonIgnore]
        public virtual OsType BatchPoolOs
        {
            set { BatchArgs.BatchPoolOs = value; }
        }
        [JsonIgnore]
        public virtual string ApplicationPackage
        {
            set { BatchArgs.ApplicationPackage = value; }
        }
        [JsonIgnore]
        public virtual string BatchResourceGroup
        {
            set { BatchArgs.ResourceGroup = value; }
        }
        [Serializable]
        public class Batch
        {

            public string ResourceGroup { get; set; } = string.Empty;
            public int BatchNodeCount { get; set; } = 10;
            public string BatchVmSize { get; set; } = null;
            [JsonIgnore]
            public string OutputContainerSasUrl { get; set; }
            public bool DeleteBatchPool { get; set; } = false;
            public bool DeleteBatchJob { get; set; } = true;
            [JsonIgnore]
            public string BatchJobName { get; set; } = null;
            public bool PollBatchPoolStatus { get; set; } = true;
            public string BatchPoolName { get; set; } = null;
            [JsonConverter(typeof(StringEnumConverter))]
            public OsType BatchPoolOs { get; set; }
            public string ApplicationPackage { get; set; } = string.Empty;
        }
        #endregion

        #region Synchronize Nested Class and property setters
        [JsonIgnore]
        public virtual string GoldServer
        {
            set { SynchronizeArgs.GoldServer = value; }
        }
        [JsonIgnore]
        public virtual string GoldDatabase
        {
            set { SynchronizeArgs.GoldDatabase = value; }
        }
        [Serializable]
        public class Synchronize
        {
            public virtual string GoldDatabase { get; set; }
            public virtual string GoldServer { get; set; }
        }
        #endregion

        #region DacPac Nested Class and property setters
        [JsonIgnore]
        public virtual string PlatinumDacpac
        {
            set { DacPacArgs.PlatinumDacpac = value; }
        }
        [JsonIgnore]
        public virtual string TargetDacpac
        {
            set { DacPacArgs.TargetDacpac = value; }
        }
        [JsonIgnore]
        public virtual bool ForceCustomDacPac
        {
            set { DacPacArgs.ForceCustomDacPac = value; }
        }
        [JsonIgnore]
        public virtual string PlatinumDbSource
        {
            set { DacPacArgs.PlatinumDbSource = value; }
        }
        [JsonIgnore]
        public virtual string PlatinumServerSource
        {
            set { DacPacArgs.PlatinumServerSource = value; }
        }
        [Serializable]
        public class DacPac
        {
            public virtual string PlatinumDacpac { get; set; } = string.Empty;
            public virtual string TargetDacpac { get; set; }
            public string PlatinumDbSource { get; set; }
            public string PlatinumServerSource { get; set; }
            public bool ForceCustomDacPac { get; set; }
        }
        #endregion

        #region Connection Nested Class and property setters
        [JsonIgnore]
        public string KeyVaultName { set { ConnectionArgs.KeyVaultName = value; } }
        [JsonIgnore]
        public virtual string EventHubConnection { set { ConnectionArgs.EventHubConnectionString = value; } }

        [JsonIgnore]
        public virtual string ServiceBusTopicConnection { set { ConnectionArgs.ServiceBusTopicConnectionString = value; } }

        [JsonIgnore]
        public virtual string StorageAccountName { set { ConnectionArgs.StorageAccountName = value; } }

        [JsonIgnore]
        public virtual string StorageAccountKey { set { ConnectionArgs.StorageAccountKey = value; } }
        [JsonIgnore]
        public virtual string BatchAccountName { set { ConnectionArgs.BatchAccountName = value; } }
   
        [JsonIgnore]
        public virtual string BatchAccountKey { set { ConnectionArgs.BatchAccountKey = value; } }
        [JsonIgnore]
        public virtual string BatchAccountUrl { set { ConnectionArgs.BatchAccountUrl = value; } }

        public class Connections
        {
            public string KeyVaultName { get; set; } = string.Empty;

            public string ServiceBusTopicConnectionString { get; set; } = string.Empty;
            public string EventHubConnectionString { get; set; } = string.Empty;
            public string StorageAccountName { get; set; } = string.Empty;
            public string StorageAccountKey { get; set; } = string.Empty;
            public string BatchAccountName { get; set; } = string.Empty;
            public string BatchAccountKey { get; set; } = string.Empty;
            public string BatchAccountUrl { get; set; } = string.Empty;

        }
        #endregion

        #region Identity Nested Class and property setters
        public string ClientId { set { IdentityArgs.ClientId = value; } }
        public string PrincipalId { set { IdentityArgs.PrincipalId = value; } }
        public string ResourceId { set { IdentityArgs.ResourceId = value; } }
        public string IdentityResourceGroup { set { IdentityArgs.ResourceGroup = value; } }
        public string SubscriptionId { set { IdentityArgs.SubscriptionId = value; ContainerAppArgs.SubscriptionId = value; AciArgs.SubscriptionId = value; } }
        public string TenantId { set { IdentityArgs.TenantId = value; } }

        public class Identity
        {
            private string _resourceid = string.Empty;
            public string IdentityName { get; set; } = string.Empty;
            public string ClientId { get; set; } = string.Empty;
            public string PrincipalId { get; set; } = string.Empty;
            [JsonIgnore]
            public string ResourceId
            {
                get
                {
                    if (_resourceid == null) _resourceid = String.Empty;
                    if (!_resourceid.StartsWith("/subscriptions") && !string.IsNullOrEmpty(IdentityName) && !string.IsNullOrEmpty(ResourceGroup) && !string.IsNullOrEmpty(SubscriptionId))
                    {
                        _resourceid = $"/subscriptions/{SubscriptionId}/resourcegroups/{ResourceGroup}/providers/Microsoft.ManagedIdentity/userAssignedIdentities/{IdentityName}";
                    }
                    return _resourceid;
                }
                set
                {
                    (string sub, string rg, string name) = Arm.ArmHelper.GetSubRgAndIdentityName(value);
                    if (sub != null)
                    {
                        IdentityName = name;
                        SubscriptionId = sub;
                        ResourceGroup = rg;

                    }
                    _resourceid = value;
                }
            }
            public string ResourceGroup { get; set; } = string.Empty;
            public string SubscriptionId { get; set; } = string.Empty;
            public string TenantId { get; set; } = string.Empty;

            public string ServiceAccountName { get; set; } = string.Empty;
        }
        #endregion

        #region ACI Nested Class and property setters
        public string AciName { set { AciArgs.AciName = value; } }
        public string AciResourceGroup { set { AciArgs.ResourceGroup = value; } }
        public int ContainerCount { set { AciArgs.ContainerCount = value; } }

        public string IdentityName { set { IdentityArgs.IdentityName = value; } }
        public class Aci
        {
            public string AciName { get; set; } = string.Empty;
            public string ResourceGroup { get; set; } = string.Empty;
            public string SubscriptionId { get; set; } = string.Empty;
            [JsonIgnore]
            public int ContainerCount { get; set; } = 10;
        }
        #endregion

        #region Networking Nested Class and property setters
        public string VnetName { set { NetworkArgs.VnetName = value; } }
        public string SubnetName { set { NetworkArgs.SubnetName = value; } }
        public string VnetResourceGroup { set { NetworkArgs.ResourceGroup = value; } }
        public class Network
        {
            public string VnetName { get; set; } = string.Empty;
            public string SubnetName { get; set; } = string.Empty;
            public string ResourceGroup { get; set; } = string.Empty;
        }
        #endregion

        #region ContainerApp Nested Class and property setters
        public string EnvironmentName { set { ContainerAppArgs.EnvironmentName = value; } }
        public string Location { set { ContainerAppArgs.Location = value; } }
        public string ResourceGroup { set { ContainerAppArgs.ResourceGroup = value; } }
        public bool EnvironmentVariablesOnly { set { ContainerAppArgs.EnvironmentVariablesOnly = value; } }
        public int MaxContainers { set { ContainerAppArgs.MaxContainerCount = value; } }
        public class ContainerApp
        {
            public string EnvironmentName { get; set; } = string.Empty;
            public string SubscriptionId { get; set; } = string.Empty;
            public string ResourceGroup { get; set; } = string.Empty;
            public string Location { get; set; } = string.Empty;
            public int MaxContainerCount { get; set; } = 10;
            [JsonIgnore]
            public bool RunningAsContainerApp { get; set; } = false;
            [JsonIgnore]
            public bool EnvironmentVariablesOnly { get; set; } = false;
        }
        #endregion

        #region ContainerRegistry Nested Class and property setters
        public string RegistryServer { set { ContainerRegistryArgs.RegistryServer = value; } }
        public string ImageName { set { ContainerRegistryArgs.ImageName = value; } }
        public string ImageTag { set { ContainerRegistryArgs.ImageTag = value; } }
        public string RegistryUserName { set { ContainerRegistryArgs.RegistryUserName = value; } }
        public string RegistryPassword { set { ContainerRegistryArgs.RegistryPassword = value; } }

        public class ContainerRegistry
        {
            public string RegistryServer { get; set; } = string.Empty;

            private string imageName = string.Empty;
            public string ImageName
            {
                set { imageName = value; }
                get
                {
                    if (string.IsNullOrEmpty(imageName))
                        return "sqlbuildmanager";
                    else
                        return imageName;
                }
            }
            public string ImageTag { get; set; } = string.Empty;
            public string RegistryUserName { get; set; } = string.Empty;
            public string RegistryPassword { get; set; } = string.Empty;

        }
        #endregion

        #region Kubernetes Nested Class and property setters
        public int PodCount { set { KubernetesArgs.PodCount = value; } }
        public class Kubernetes
        {
            public int PodCount { get; set; } = 10;
        }
        #endregion
        
        
        [Serializable]
        public class AutoScripting
        {
            public virtual bool AutoScriptDesignated { get; set; } = false;
            public virtual string AutoScriptFileName { get; set; } = string.Empty;
        }

        [Serializable]
        public class StoredProcTesting
        {
            public virtual string SpTestFile { get; set; } = string.Empty;
            public virtual bool SprocTestDesignated { get; set; } = false;
        }


        public override string ToString()
        {
            return this.ToStringExtension(StringType.Basic);
        }
        public string ToBatchString()
        {
            return this.ToStringExtension(StringType.Batch);
        }

        public object Clone()
        {
            return (CommandLineArgs)MemberwiseClone();
        }
    }

}
