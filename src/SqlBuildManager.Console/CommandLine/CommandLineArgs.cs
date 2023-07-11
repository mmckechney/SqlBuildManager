using Microsoft.Azure.Batch;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using System;
using System.Collections.Generic;
using System.IO;
using System.ComponentModel;
using SqlBuildManager.Logging.Threaded;
using System.Linq;

namespace SqlBuildManager.Console.CommandLine
{

    [Serializable]
    public partial class CommandLineArgs //: System.ICloneable
    {
        public Aci AciArgs { get; set; }
        public Authentication AuthenticationArgs { get; set; }
        [JsonIgnore]
        public AutoScripting AutoScriptingArgs { get; set; }
        public Batch BatchArgs { get; set; }
        public Connections ConnectionArgs { get; set; }
        public ContainerApp ContainerAppArgs { get; set; }
        public ContainerRegistry ContainerRegistryArgs { get; set; }
        [JsonIgnore]
        public DacPac DacPacArgs { get; set; }
        public EventHub EventHubArgs { get; set; }
        public Identity IdentityArgs { get; set; }
        public Kubernetes KubernetesArgs { get; set; }
        public Network NetworkArgs { get; set; }
        [JsonIgnore]
        public StoredProcTesting StoredProcTestingArgs { get; set; }
        [JsonIgnore]
        public Synchronize SynchronizeArgs { get; set; }
        public CommandLineArgs()
        {
            this.AciArgs = new Aci();
            this.AuthenticationArgs = new Authentication();
            this.AutoScriptingArgs = new AutoScripting();
            this.BatchArgs = new Batch();
            this.ConnectionArgs = new Connections();
            this.ContainerAppArgs = new ContainerApp();
            this.ContainerRegistryArgs = new ContainerRegistry();
            this.DacPacArgs = new DacPac();
            this.EventHubArgs = new EventHub();
            this.IdentityArgs = new Identity();
            this.KubernetesArgs = new Kubernetes();
            this.NetworkArgs = new Network();
            this.StoredProcTestingArgs = new StoredProcTesting();
            this.SynchronizeArgs = new Synchronize();
        }

        [JsonIgnore]
        public virtual LogLevel LogLevel { get; set; } = LogLevel.Information;
        
        #region State Flags
        [JsonIgnore]
        [DefaultValue(false)]
        public bool Decrypted { get; set; } = false;
        [JsonIgnore]
        [DefaultValue(false)]
        public bool KeyVaultSecretsRetrieved { get; set; } = false;
        [JsonIgnore]
        [DefaultValue(false)]
        public virtual bool RunningAsContainer { get; set; } = false;
        #endregion

        #region Settings File Properties 
        private string settingsFile = string.Empty;
        [JsonIgnore]
        public string SettingsFileKey { get; set; }
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
                    CommandLineArgs cmdLine = JsonSerializer.Deserialize<CommandLineArgs>(File.ReadAllText(value));
                    this.SetValues(cmdLine);
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
        #endregion

        #region Build Runtime Configuration Properties

        [JsonIgnore]
        public virtual string BuildFileName { get; set; } = string.Empty;
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
                if (value != null)
                {
                    jobName = value.ToLower();
                    BatchJobName = value.ToLower();
                }
            }
        }

        [DefaultValue(10)]
        public virtual int Concurrency { get; set; } = 10;
        [JsonConverter(typeof(JsonStringEnumConverter))]
        [DefaultValue(ConcurrencyType.Count)]
        public virtual ConcurrencyType ConcurrencyType { get; set; } = ConcurrencyType.Count;
        [JsonIgnore]
        [DefaultValue(false)]
        public virtual bool Trial { get; set; } = false;
        [JsonIgnore]
        [DefaultValue(true)]
        public virtual bool Transactional { get; set; } = true;
        public virtual int TimeoutRetryCount { get; set; } = 0;
        [DefaultValue(500)]
        public virtual int DefaultScriptTimeout { get; set; } = 500;
        public virtual string RootLoggingPath { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string Description { get; set; } = string.Empty;
        [JsonIgnore]
        public string BuildRevision { get; set; }
        [JsonIgnore]
        public virtual string LogToDatabaseName { get; set; } = string.Empty;

        #endregion

        #region Override and MultiDbConfig Properties (incl. from script)
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
        [DefaultValue(false)]
        public virtual bool OverrideDesignated { get; set; } = false;
        [JsonIgnore]
        public virtual string MultiDbRunConfigFileName { get; internal set; } = string.Empty;
        [JsonIgnore]
        public virtual string ManualOverRideSets { get; set; } = string.Empty;

        /// <summary>
        /// Script file name for generating override cfg file
        /// </summary>
        [JsonIgnore]
        public virtual FileInfo ScriptFile { get; set; }
        /// <summary>
        /// Script text  generating override cfg file
        /// </summary>
        [JsonIgnore]
        public virtual string ScriptText { get; set; }

        #endregion

        [JsonIgnore]
        public virtual string Server { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string Database { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string ScriptSrcDir { get; set; } = string.Empty;
        [JsonIgnore]
        public string Directory { get; set; } = string.Empty;
        [JsonIgnore]
        public bool ContinueOnFailure { get; set; }
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
        [JsonIgnore]
        public bool Silent { get; set; }
        [JsonIgnore]
        public FileInfo QueryFile { get; set; }
        [JsonIgnore]
        public FileInfo OutputFile { get; set; }
        
        public List<string> DirectPropertyChangeTracker = new();  
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
