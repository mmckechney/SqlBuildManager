using SqlBuildManager.Connection;
using SqlBuildManager.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.SqlBuild.Services
{
    public interface IDatabaseUtility
    {
      
        public bool HasBlockingSqlLog(System.Guid scriptId, ConnectionData cData, string databaseName, out string scriptHash, out string scriptTextHash, out DateTime commitDate);
        public bool GetBlockingSqlLog(System.Guid scriptId, ref BuildConnectData connData);
        public IReadOnlyList<SqlBuildManager.SqlBuild.Models.ScriptRunLogEntry> GetScriptRunLog(System.Guid scriptId, ConnectionData connData);
        public IReadOnlyList<SqlBuildManager.SqlBuild.Models.ScriptRunLogEntry> GetObjectRunHistoryLog(string objectFileName, ConnectionData connData);
        public SqlBuildManager.SqlBuild.Models.ScriptRunLogEntry ReadScriptRunLogEntry(IDataRecord reader);
        public SqlSyncBuildDataModel ClearScriptBlocks(ClearScriptData scrData, ConnectionData connData, IProgressReporter progressReporter, ISqlBuildRunnerProperties runnerProperties);

        public SqlSyncBuildDataModel ClearAllowScriptBlocks(SqlSyncBuildDataModel model, string serverName, IReadOnlyList<string> selectedScriptIds);

        /// <summary>
        /// Fetches blocking SQL log status for a batch of script IDs with a single query per target database.
        /// Results are keyed by script GUID; missing entries mean the script has no log record (not-run).
        /// </summary>
        public IReadOnlyDictionary<System.Guid, SqlBuildManager.SqlBuild.Models.SqlLogStatus> GetBatchBlockingSqlLog(
            IReadOnlyList<System.Guid> scriptIds, ConnectionData cData, string databaseName);
    }
}
