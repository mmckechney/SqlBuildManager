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

        #region Nested Object properties
        public Authentication AuthenticationArgs { get; set; } = new Authentication();
        public Batch BatchArgs { get; set; } = new Batch();
        public Connections ConnectionArgs { get; set; } = new Connections();
        public Identity IdentityArgs { get; set; } = new Identity();
        public Aci AciArgs { get; set; } = new Aci();
        [JsonIgnore]
        public DacPac DacPacArgs { get; set; } = new DacPac();
        [JsonIgnore]
        public Synchronize SynchronizeArgs { get; set; } = new Synchronize();
        [JsonIgnore]
        public AutoScripting AutoScriptingArgs { get; set; } = new AutoScripting();
        [JsonIgnore]
        public StoredProcTesting StoredProcTestingArgs { get; set; } = new StoredProcTesting();

        #endregion
        private string settingsFile = string.Empty;
        [JsonIgnore]
        public string SettingsFile
        {
            get
            {
                return this.settingsFile;
            }
            internal set
            {
                if (File.Exists(value))
                {
                    CommandLineArgs cmdLine = JsonConvert.DeserializeObject<CommandLineArgs>(File.ReadAllText(value));
                    this.BatchArgs = cmdLine.BatchArgs;
                    this.AuthenticationArgs = cmdLine.AuthenticationArgs;
                    this.ConnectionArgs = cmdLine.ConnectionArgs;
                    this.IdentityArgs = cmdLine.IdentityArgs;
                    this.AciArgs = cmdLine.AciArgs;

                    this.RootLoggingPath = cmdLine.RootLoggingPath;
                    this.DefaultScriptTimeout = cmdLine.DefaultScriptTimeout;
                    this.TimeoutRetryCount = cmdLine.TimeoutRetryCount;
                }
                this.settingsFile = value;
            }
        }
        [JsonIgnore]
        public FileInfo FileInfoSettingsFile
        {
            set
            {
                this.SettingsFile = value.FullName;
            }
        }
        public string Override {
            set
            {
                this.OverrideDesignated = true;
                var fileExt = new List<string> { ".multidb", ".cfg", ".multidbq", ".sql" };
                fileExt.ForEach(ex =>
                {
                    if(value.ToLower().Trim().EndsWith(ex))
                    {
                        this.MultiDbRunConfigFileName = value;
                    }
                });
                if(this.MultiDbRunConfigFileName == string.Empty)
                {
                    this.ManualOverRideSets = value;
                }
            }
        }
        [JsonIgnore]
        public string SettingsFileKey { get; set; }
        [JsonIgnore]
        public virtual string Server { get; set; } = string.Empty;
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
                if(Path.GetExtension(value) == string.Empty)
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
                BatchArgs.BatchJobName = value.ToLower();
            }
        }
        [JsonIgnore]
        public virtual int Concurrency { get; set; } = 10;
        [JsonIgnore]
        public virtual ConcurrencyType ConcurrencyType { get; set; } = ConcurrencyType.Count;

        [JsonIgnore]
        public bool Silent { get; set; }
        [JsonIgnore]
        public FileInfo QueryFile { get; set; }
        [JsonIgnore]
        public FileInfo OutputFile { get; set; }

        [JsonIgnore]
        public bool WhatIf { get; set; } = false;

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
            set { AuthenticationArgs.UserName = value; }
        }
        [JsonIgnore]
        public virtual string Password
        {
            set { AuthenticationArgs.Password = value; }
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

            [JsonIgnore]
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
        public virtual string BatchAccountName
        {
            set { ConnectionArgs.BatchAccountName = value; }
        }
        [JsonIgnore]
        public virtual string BatchAccountKey
        {
            set { ConnectionArgs.BatchAccountKey = value; }
        }
        [JsonIgnore]
        public virtual string BatchAccountUrl
        {
            set { ConnectionArgs.BatchAccountUrl = value; }
        }
        [JsonIgnore]
        public virtual string StorageAccountName
        {
            set
            {
                ConnectionArgs.StorageAccountName = value;
            }
        }
        [JsonIgnore]
        public virtual string StorageAccountKey
        {
            set
            {
                ConnectionArgs.StorageAccountKey = value;
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
        public virtual string EventHubConnection
        {
            set
            {
                ConnectionArgs.EventHubConnectionString = value;
            }
        }
        [JsonIgnore]
        public virtual string ServiceBusTopicConnection
        {
            set
            {
                ConnectionArgs.ServiceBusTopicConnectionString = value;
            }
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
        [Serializable]
        public class Batch
        {
 
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
            public virtual string PlatinumDacpac { get; set; }
            public virtual string TargetDacpac { get; set; }
            public string PlatinumDbSource { get; set; }
            public string PlatinumServerSource { get; set; }
            public bool ForceCustomDacPac { get; set; }
        }
        #endregion


        public string KeyVaultName { set { this.ConnectionArgs.KeyVaultName = value; } }
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

        public string ClientId { set { this.IdentityArgs.ClientId = value; } }
        public string PrincipalId { set { this.IdentityArgs.PrincipalId = value; } }
        public string ResourceId { set { this.IdentityArgs.ResourceId = value; } }
        public string ResourceGroup { set { this.IdentityArgs.ResourceGroup = value; } }
        public string SubscriptionId { set { this.IdentityArgs.SubscriptionId = value;} }

        public class Identity
        {
            public string ClientId { get; set; } = string.Empty;
            public string PrincipalId { get; set; } = string.Empty;
            public string ResourceId { get; set; } = string.Empty;
            public string ResourceGroup { get; set; } = string.Empty;
            public string SubscriptionId { get; set; } = string.Empty;
        }

        
        public string AciName { set { this.AciArgs.AciName = value; } }
        public string IdentityResourceGroup { set { this.AciArgs.IdentityResourceGroup = value; } }
        public int ContainerCount { set { this.AciArgs.ContainerCount = value; } }
        public string IdentityName { set { this.AciArgs.IdentityName = value; } }
        public class Aci
        {
            public string AciName { get; set; } = string.Empty;
            public string IdentityResourceGroup { get; set; } = string.Empty;
            public int ContainerCount{ get; set; } = 10;
            public string IdentityName { get; set; } = string.Empty;
        }

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
            return (CommandLineArgs) this.MemberwiseClone();
        }
    }
      
}
