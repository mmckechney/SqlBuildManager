using SqlSync.Connection;
using SqlSync.SqlBuild.Abstractions;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    public interface IDatabaseUtility
    {
      
        public bool HasBlockingSqlLog(System.Guid scriptId, ConnectionData cData, string databaseName, out string scriptHash, out string scriptTextHash, out DateTime commitDate);
        public bool GetBlockingSqlLog(System.Guid scriptId, ref BuildConnectData connData);
        public IReadOnlyList<SqlSync.SqlBuild.Models.ScriptRunLogEntry> GetScriptRunLog(System.Guid scriptId, ConnectionData connData);
        public IReadOnlyList<SqlSync.SqlBuild.Models.ScriptRunLogEntry> GetObjectRunHistoryLog(string objectFileName, ConnectionData connData);
        public SqlSync.SqlBuild.Models.ScriptRunLogEntry ReadScriptRunLogEntry(IDataRecord reader);
        public SqlSyncBuildDataModel ClearScriptBlocks(ClearScriptData scrData, ConnectionData connData, IProgressReporter progressReporter, ISqlBuildRunnerProperties runnerProperties);

        public SqlSyncBuildDataModel ClearAllowScriptBlocks(SqlSyncBuildDataModel model, string serverName, IReadOnlyList<string> selectedScriptIds);
    }
}
