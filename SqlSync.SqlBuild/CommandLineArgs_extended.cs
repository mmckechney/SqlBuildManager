using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SqlSync.SqlBuild
{
    public partial class CommandLineArgs
    {
        [System.Runtime.Serialization.DataMember()]
        public virtual bool GetDifference { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public virtual bool Synchronize { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public virtual string GoldDatabase { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public virtual string GoldServer { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public virtual string PlatinumDacpac { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public virtual string TargetDacpac { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public bool ContinueOnFailure { get; set; }

        [System.Runtime.Serialization.DataMember()]
        public bool ForceCustomDacPac { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string Action { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string PackageName
        { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string Directory { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string PlatinumDbSource { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string PlatinumServerSource { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string BuildRevision { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string RemoteDbErrorList { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string RemoteErrorDetail { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public string OutputSbm { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public bool SavedCreds { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public SqlSync.Connection.AuthenticationType AuthenticationType { get; set; }
        [System.Runtime.Serialization.DataMember()]
        public bool TestConnectivity { get; set; }
        public string OutputContainerSasUrl { get; set; }

        #region Needed for Azure batch processing 
        public bool DeleteBatchPool { get; set; } = true;
        public int BatchNodeCount { get; set; } = 10;
        public string BatchAccountName { get; set; } = null;
        public string BatchAccountKey { get; set; } = null;
        public string BatchAccountUrl { get; set; } = null;
        public string StorageAccountName { get; set; } = null;
        public string StorageAccountKey { get; set; } = null;
        public string BatchVmSize { get; set; } = null;
        #endregion 

        public override string ToString()
        {
            var props = this.GetType().GetProperties();
            StringBuilder sb = new StringBuilder();
            var lst = props.OrderBy(p => p.Name).ToList();
            lst.ForEach(p => sb.AppendLine("/"+ p.Name + "=\"" + p.GetValue(this, null) + "\""));
            return sb.ToString();
        }

    }
}
