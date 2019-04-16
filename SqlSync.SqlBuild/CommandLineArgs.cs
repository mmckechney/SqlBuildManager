using System.Linq;
using System.Text;

namespace SqlSync.SqlBuild
{


    public partial class CommandLineArgs
    {

        public CommandLineArgs()
        {

        }
        public Batch BatchArgs { get; set; } = new Batch();
        public Authentication AuthenticationArgs { get; set; } = new Authentication();
        public Remote RemoteArgs { get; set; } = new Remote();
        public Synchronize SynchronizeArgs { get; set; } = new Synchronize();
        public DacPac DacPacArgs { get; set; } = new DacPac();
        public AutoScripting AutoScriptingArgs { get; set; } = new AutoScripting();
        public StoredProcTesting StoredProcTestingArgs { get; set; } = new StoredProcTesting();
        public ActionType Action { get; set; }



        public virtual string BuildFileName { get; set; } = string.Empty;

        public virtual bool OverrideDesignated { get; set; } = false;

        public virtual string MultiDbRunConfigFileName { get; set; } = string.Empty;

        public virtual string ManualOverRideSets { get; set; } = string.Empty;

        public virtual string Server { get; set; } = string.Empty;

        public virtual string[] RawArguments { get; set; } = new string[0];

        public virtual string LogFileName { get; set; } = string.Empty;

        public virtual string Database { get; set; } = string.Empty;

        public virtual string ScriptLogFileName { get; set; } = string.Empty;

        public virtual string RootLoggingPath { get; set; } = string.Empty;

        public virtual bool LogAsText { get; set; } = true;

        public virtual bool Trial { get; set; } = false;

        public virtual string ScriptSrcDir { get; set; } = string.Empty;

        public virtual string LogToDatabaseName { get; set; } = string.Empty;

        public virtual string Description { get; set; } = string.Empty;

        public virtual bool Transactional { get; set; } = true;

        public virtual int AllowableTimeoutRetries { get; set; } = 0;

        public bool ContinueOnFailure { get; set; }

        public string PackageName { get; set; }

        public string Directory { get; set; }

        public string BuildRevision { get; set; }

        public string OutputSbm { get; set; }

        public class Remote
        {
            public virtual string RemoteServers { get; set; } = string.Empty;
            public virtual string DistributionType { get; set; } = string.Empty;
            public bool? TestConnectivity { get; set; } = null;
            public string RemoteDbErrorList { get; set; }
            public string RemoteErrorDetail { get; set; }
            public bool? AzureRemoteStatus { get; set; } = null;
        }
        public class Authentication
        {
            public virtual string UserName { get; set; } = string.Empty;
            public virtual string Password { get; set; } = string.Empty;
            public SqlSync.Connection.AuthenticationType AuthenticationType { get; set; } = Connection.AuthenticationType.UserNamePassword;
            public bool SavedCreds { get; set; } = false;

        }
        public class Batch
        {
            public bool DeleteBatchPool { get; set; } = true;
            public int BatchNodeCount { get; set; } = 10;
            public string BatchAccountName { get; set; } = null;
            public string BatchAccountKey { get; set; } = null;
            public string BatchAccountUrl { get; set; } = null;
            public string StorageAccountName { get; set; } = null;
            public string StorageAccountKey { get; set; } = null;
            public string BatchVmSize { get; set; } = null;
            public string OutputContainerSasUrl { get; set; }
        }
        public class Synchronize
        {
            public virtual string GoldDatabase { get; set; }
            public virtual string GoldServer { get; set; }
        }
        public class DacPac
        {
            public virtual string PlatinumDacpac { get; set; }
            public virtual string TargetDacpac { get; set; }
            public string PlatinumDbSource { get; set; }
            public string PlatinumServerSource { get; set; }
            public bool ForceCustomDacPac { get; set; }
        }
        public class AutoScripting
        {
            public virtual bool AutoScriptDesignated { get; set; } = false;
            public virtual string AutoScriptFileName { get; set; } = string.Empty;
        }
        public class StoredProcTesting
        {
            public virtual string SpTestFile { get; set; } = string.Empty;
            public virtual bool SprocTestDesignated { get; set; } = false;
        }

        public enum ActionType
        {
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
            Encrypt,
            Error
        }
        
        public override string ToString()
        {
            var props = this.GetType().GetProperties();
            StringBuilder sb = new StringBuilder();
            var lst = props.OrderBy(p => p.Name).ToList();
            lst.ForEach(p => sb.AppendLine("/" + p.Name + "=\"" + p.GetValue(this, null) + "\""));
            return sb.ToString();
        }

    }
      
}
