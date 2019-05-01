using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
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
        public Remote RemoteArgs { get; set; } = new Remote();
        [JsonIgnore]
        public DacPac DacPacArgs { get; set; } = new DacPac();
        [JsonIgnore]
        public Synchronize SynchronizeArgs { get; set; } = new Synchronize();
        [JsonIgnore]
        public AutoScripting AutoScriptingArgs { get; set; } = new AutoScripting();
        [JsonIgnore]
        public StoredProcTesting StoredProcTestingArgs { get; set; } = new StoredProcTesting();



        [JsonIgnore]
        public virtual string BuildFileName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual bool OverrideDesignated { get; set; } = false;
        [JsonIgnore]
        public virtual string MultiDbRunConfigFileName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string ManualOverRideSets { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string Server { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string LogFileName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string Database { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string ScriptLogFileName { get; set; } = string.Empty;

        public virtual string RootLoggingPath { get; set; } = string.Empty;

        public virtual bool LogAsText { get; set; } = true;
        [JsonIgnore]
        public virtual bool Trial { get; set; } = false;
        [JsonIgnore]
        public virtual string ScriptSrcDir { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string LogToDatabaseName { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual string Description { get; set; } = string.Empty;
        [JsonIgnore]
        public virtual bool Transactional { get; set; } = true;
        [JsonIgnore]
        public virtual int AllowableTimeoutRetries { get; set; } = 0;
        [JsonIgnore]
        public bool ContinueOnFailure { get; set; }
        [JsonIgnore]
        public string Directory { get; set; }
        [JsonIgnore]
        public string BuildRevision { get; set; }
        [JsonIgnore]
        public string OutputSbm { get; set; }
        [JsonIgnore]
        public string SettingsFile { get; set; }

        [Serializable]
        public class Remote
        {
            public virtual string RemoteServers { get; set; } = string.Empty;
            public virtual string DistributionType { get; set; } = string.Empty;
            public bool TestConnectivity { get; set; } = false;
            public string RemoteDbErrorList { get; set; }
            public string RemoteErrorDetail { get; set; }
            public bool AzureRemoteStatus { get; set; } = false;
        }
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
                if (property.PropertyType == typeof(CommandLineArgs.Remote) ||
                    property.PropertyType == typeof(CommandLineArgs.Authentication) ||
                    property.PropertyType == typeof(CommandLineArgs.Batch) ||
                    property.PropertyType == typeof(CommandLineArgs.AutoScripting) ||
                    property.PropertyType == typeof(CommandLineArgs.DacPac) ||
                    property.PropertyType == typeof(CommandLineArgs.StoredProcTesting) ||
                    property.PropertyType == typeof(CommandLineArgs.Synchronize)
                    )
                {
                    if (property.GetValue(obj) != null)
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
                    else if (property.PropertyType == typeof(bool))
                    {
                        if (bool.Parse(property.GetValue(obj).ToString()) == true)
                        {
                            sb.Append("/" + property.Name + "=true ");
                        }
                    }
                    else if (property.PropertyType == typeof(string))
                    {
                        if (property.GetValue(obj) != null && !string.IsNullOrWhiteSpace(property.GetValue(obj).ToString()))
                        {
                            sb.Append("/" + property.Name + "=\"" + property.GetValue(obj).ToString() + "\" ");
                        }
                    }
                    else if (property.GetValue(obj) != null)
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
