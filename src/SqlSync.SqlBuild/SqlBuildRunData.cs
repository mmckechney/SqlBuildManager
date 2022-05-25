using System;
using System.Collections.Generic;
using System.Text;
using SqlSync.Connection;
namespace SqlSync.SqlBuild
{
    public class SqlBuildRunData
    {
        public SqlSyncBuildData BuildData { get; set; } = null;
        public string BuildType { get; set; } = string.Empty;
        public string Server { get; set; } = string.Empty;
        public string BuildDescription { get; set; } = string.Empty;
        public double StartIndex { get; set; } = 0;
        public string ProjectFileName { get; set; }
        public bool IsTrial { get; set; } = false;
        public double[] RunItemIndexes { get; set; } = new double[0];
        public bool RunScriptOnly { get; set; } = false;
        public string BuildFileName { get; set; }
        public string LogToDatabaseName { get; set; } = string.Empty;
        public bool IsTransactional { get; set; } = true;
        public string PlatinumDacPacFileName { get; set; } = string.Empty;
        public List<DatabaseOverride> TargetDatabaseOverrides { get; set; }
        public bool ForceCustomDacpac { get; set; }
        public string BuildRevision { get; set; }
        public int DefaultScriptTimeout { get; set; } = 500;
        public bool AllowObjectDelete { get; set; } = false;

    }
}
