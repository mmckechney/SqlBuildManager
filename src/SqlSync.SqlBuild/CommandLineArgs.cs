using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Transactions;
using Newtonsoft.Json;
namespace SqlSync.SqlBuild
{

    [Serializable]
    public partial class CommandLineArgs 
    {

        public CommandLineArgs()
        {

        }
        [JsonIgnore]
        public ActionType Action { get; set; }
        public Authentication AuthenticationArgs { get; set; } = new Authentication();
        public Batch BatchArgs { get; set; } = new Batch();
        [JsonIgnore]
        public DacPac DacPacArgs { get; set; } = new DacPac();
        [JsonIgnore]
        public Synchronize SynchronizeArgs { get; set; } = new Synchronize();
        [JsonIgnore]
        public AutoScripting AutoScriptingArgs { get; set; } = new AutoScripting();
        [JsonIgnore]
        public StoredProcTesting StoredProcTestingArgs { get; set; } = new StoredProcTesting();

        private string settingsFile = string.Empty;
        [JsonIgnore]
        public string SettingsFile
        {
            get
            {
                return this.settingsFile;
            }
            set
            {
                if (File.Exists(value))
                {

                    CommandLineArgs cmdLine = JsonConvert.DeserializeObject<CommandLineArgs>(File.ReadAllText(value));
                    cmdLine = Cryptography.DecryptSensitiveFields(cmdLine);
                    this.BatchArgs = cmdLine.BatchArgs;
                    this.AuthenticationArgs = cmdLine.AuthenticationArgs;
                    this.RootLoggingPath = cmdLine.RootLoggingPath;
                    this.DefaultScriptTimeout = cmdLine.DefaultScriptTimeout;
                    this.TimeoutRetryCount = cmdLine.TimeoutRetryCount;
                }
                this.settingsFile = value;
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
        public virtual string Server { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string Database { get; set; } = string.Empty;
        public virtual string RootLoggingPath { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual bool Trial { get; set; } = false;
        [JsonIgnore]
        public virtual string ScriptSrcDir { get; set; } = string.Empty;
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
        public virtual string GoldServer
        {
            set { SynchronizeArgs.GoldServer = value; }
        }
        [JsonIgnore]
        public virtual string GoldDatabase
        {
            set { SynchronizeArgs.GoldDatabase = value; }
        }
        [JsonIgnore]
        public bool ContinueOnFailure { get; set; }
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
        [JsonIgnore]
        public string BuildRevision { get; set; }
        [JsonIgnore]
        public string OutputSbm { get; set; }
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
            set { BatchArgs.BatchJobName = value; }
        }
        [JsonIgnore]
        public virtual string BatchAccountName
        {
            set { BatchArgs.BatchAccountName = value; }
        }
        [JsonIgnore]
        public virtual string BatchAccountKey
        {
            set { BatchArgs.BatchAccountKey = value; }
        }
        [JsonIgnore]
        public virtual string BatchAccountUrl
        {
            set { BatchArgs.BatchAccountUrl = value; }
        }
        [JsonIgnore]
        public virtual string StorageAccountName
        {
            set { BatchArgs.StorageAccountName = value; }
        }
        [JsonIgnore]
        public virtual string StorageAccountKey
        {
            set { BatchArgs.StorageAccountKey = value; }
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
            set { BatchArgs.EventHubConnectionString = value; }
        }
        [JsonIgnore]
        public virtual bool PollBatchPoolStatus
        {
            set { BatchArgs.PollBatchPoolStatus = value; }
        }
        public virtual int DefaultScriptTimeout { get; set; } = 500;



        #region Not direct commandline options
        [JsonIgnore]
        public virtual bool OverrideDesignated { get; set; } = false;
        [JsonIgnore]
        public virtual string MultiDbRunConfigFileName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string ManualOverRideSets { get; set; } = string.Empty;
        #endregion


        [Serializable]
        public class Authentication
        {
            public virtual string UserName { get; set; } = string.Empty;
            public virtual string Password { get; set; } = string.Empty;

            [JsonIgnore]
            public SqlSync.Connection.AuthenticationType AuthenticationType { get; set; } = Connection.AuthenticationType.Password;
        }

        [Serializable]
        public class Batch
        {
 
            public int BatchNodeCount { get; set; } = 10;
            public string BatchAccountName { get; set; } = null;
            public string BatchAccountKey { get; set; } = null;
            public string BatchAccountUrl { get; set; } = null;
            public string StorageAccountName { get; set; } = null;
            public string StorageAccountKey { get; set; } = null;
            public string BatchVmSize { get; set; } = null;
            [JsonIgnore]
            public string OutputContainerSasUrl { get; set; }
            public bool DeleteBatchPool { get; set; } = false;
            public bool DeleteBatchJob { get; set; } = true;
            [JsonIgnore]
            public string BatchJobName { get; set; } = null;
            public bool PollBatchPoolStatus { get; set; } = true;
            public string BatchPoolName { get; set; } = null;
            public string EventHubConnectionString { get; set; } = string.Empty;
        }

        [Serializable]
        public class Synchronize
        {
            public virtual string GoldDatabase { get; set; }
            public virtual string GoldServer { get; set; }
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

        [Serializable]
        public enum ActionType
        {
            Error,
            Build,
            Threaded,
            Remote,
            Batch,
            Package,
            PolicyCheck,
            GetHash,
            CreateBackout,
            GetDifference,
            Synchronize,
            ScriptExtract,
            SaveSettings,
            BatchPreStage, 
            BatchCleanUp
        }
        
        public override string ToString()
        {
            return this.ToStringExtension();
        }
    }

    public static class CommandLineExtensions
    {
        public static string ToStringExtension(this object obj)
        {
            StringBuilder sb = new StringBuilder();
            foreach (System.Reflection.PropertyInfo property in obj.GetType().GetProperties())
            {
                if (property.PropertyType == typeof(CommandLineArgs.Authentication) ||
                    property.PropertyType == typeof(CommandLineArgs.Batch) ||
                    property.PropertyType == typeof(CommandLineArgs.AutoScripting) ||
                    property.PropertyType == typeof(CommandLineArgs.DacPac) ||
                    property.PropertyType == typeof(CommandLineArgs.StoredProcTesting) ||
                    property.PropertyType == typeof(CommandLineArgs.Synchronize)
                    )
                {
                    if (property.CanRead &&  property.GetValue(obj) != null)
                    {
                        sb.Append(property.GetValue(obj).ToStringExtension());
                    }
                }
                else
                {
                    if (property.Name == "MultiDbRunConfigFileName") //special case
                    {
                        if (property.GetValue(obj) != null && !string.IsNullOrWhiteSpace(property.GetValue(obj).ToString()))
                        {
                            sb.Append("/Override=\"" + property.GetValue(obj).ToString() + "\" ");
                        }
                    }
                    else if (property.Name == "ManualOverRideSets") //special case
                    {
                        if (property.GetValue(obj) != null && !string.IsNullOrWhiteSpace(property.GetValue(obj).ToString()))
                        {
                            sb.Append("/Override=\"" + property.GetValue(obj).ToString() + "\" ");
                        }
                    }
                    else if (property.Name == "BuildFileName") //special case where commandline arg != propertyname
                    {
                        if (property.GetValue(obj) != null && !string.IsNullOrWhiteSpace(property.GetValue(obj).ToString()))
                        {
                            sb.Append("/PackageName=\"" + property.GetValue(obj).ToString() + "\" ");
                        }
                    }
                    else if (property.Name == "OverrideDesignated") //special case
                    {
                        //do nothing
                    }
                    else if (property.PropertyType == typeof(bool) && property.CanRead)
                    {
                        if (bool.Parse(property.GetValue(obj).ToString()) == true)
                        {
                            sb.Append("/" + property.Name + "=true ");
                        }
                    }
                    else if (property.PropertyType == typeof(string) && property.CanRead)
                    {
                        if (property.GetValue(obj) != null && !string.IsNullOrWhiteSpace(property.GetValue(obj).ToString()))
                        {
                            sb.Append("/" + property.Name + "=\"" + property.GetValue(obj).ToString() + "\" ");
                        }
                    }
                    else if (property.CanRead && property.GetValue(obj) != null)
                    {
                        double num;
                        if (double.TryParse(property.GetValue(obj).ToString(), NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture, out num))
                        {
                            sb.Append("/" + property.Name + "=" + property.GetValue(obj).ToString() + " ");
                        }
                        else
                        {
                            sb.Append("/" + property.Name + "=\"" + property.GetValue(obj).ToString() + "\" ");
                        }
                    }
                }

            }
            return sb.ToString();
        }
    }
      
}
