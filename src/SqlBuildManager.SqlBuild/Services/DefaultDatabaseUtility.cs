using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Connection;

using SqlBuildManager.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

#nullable enable

namespace SqlBuildManager.SqlBuild.Services
{
    internal class DefaultDatabaseUtility :IDatabaseUtility
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);
        private readonly ISqlLoggingService sqlLoggingService;
        private readonly IConnectionsService connectionsService;
        private readonly IProgressReporter progressReporter;
        private readonly ISqlBuildFileHelper fileHelper;

        private readonly ISqlResourceProvider resourceProvider;

        public DefaultDatabaseUtility(IConnectionsService connectionsService, ISqlLoggingService sqlLoggingService, IProgressReporter progressReporter, ISqlBuildFileHelper fileHelper, ISqlResourceProvider? resourceProvider = null) 
        {
            this.connectionsService = connectionsService;
            this.sqlLoggingService = sqlLoggingService;
            this.progressReporter = progressReporter;
            this.fileHelper = fileHelper ?? new DefaultSqlBuildFileHelper();
            this.resourceProvider = resourceProvider ?? new SqlServerResourceProvider();
        }
        /// <summary>
        /// Checks to see if the specified script has a block against running more than once. If so, returns some data about it
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="cData">The ConnectionData object for the target database</param>
        /// <param name="databaseName">The name of the database that needs to be checked</param>
        /// <param name="scriptHash">out string for the hash of the script</param>
        /// <param name="scriptTextHash">out string for the hash of the parsed script</param>
        /// <param name="commitDate">out DateTime for the commit date that is blocking the re-run</param>
        /// <returns>True if there is a script block in place</returns>
        public bool HasBlockingSqlLog(System.Guid scriptId, ConnectionData cData, string databaseName, out string scriptHash, out string scriptTextHash, out DateTime commitDate)
        {


            bool hasBlock = false;
            scriptHash = string.Empty;
            scriptTextHash = string.Empty;
            commitDate = DateTime.MinValue;

            if (string.IsNullOrWhiteSpace(databaseName))
            {
                return false;
            }

            var conn = SqlBuildManager.Connection.ConnectionHelper.GetDbConnection(new ConnectionData() { DatabaseName = databaseName, SQLServerName = cData.SQLServerName, UserId = cData.UserId, Password = cData.Password, AuthenticationType = cData.AuthenticationType, ScriptTimeout = 2, ManagedIdentityClientId = cData.ManagedIdentityClientId, DatabasePlatform = cData.DatabasePlatform });
            DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = resourceProvider.GetHasBlockingSqlLogQuery();
            var param = cmd.CreateParameter();
            param.ParameterName = "@ScriptId";
            param.Value = scriptId;
            cmd.Parameters.Add(param);
            try
            {
                cmd.Connection!.Open();
                int i = 0;
                using (DbDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        if (i == 0)
                        {
                            scriptHash = (reader[1] == DBNull.Value) ? string.Empty : reader[1].ToString() ?? string.Empty;
                            commitDate = (reader[2] == DBNull.Value) ? DateTime.MinValue : DateTime.Parse(reader[2].ToString()!);
                            scriptTextHash = (reader[3] == DBNull.Value) ? string.Empty : fileHelper.GetSHA1Hash(reader[3].ToString()!);
                            i++;
                        }

                        if (Convert.ToBoolean(reader[0]))
                        {
                            hasBlock = true;
                            break;
                        }
                    }
                    reader.Close();
                }
                return hasBlock;
            }
            catch (DbException)
            {
                //swallow the exception
                return false;
            }
            catch (Exception exe)
            {
                log.LogWarning(exe, $"Unable to check for blocking SQL for script {scriptId.ToString()} on database {cmd.Connection!.DataSource}.{cmd.Connection.Database}");
                return false;
            }
            finally
            {
                cmd.Connection!.Close();
            }
        }

        /// <summary>Maximum number of script ID parameters per batch query — safe under SQL Server's 2100-param limit.</summary>
        private const int BatchMaxParams = 500;

        /// <summary>
        /// PERF-003: Fetches blocking SQL log status for a batch of script IDs in one query per database.
        /// Each script GUID maps to a <see cref="SqlBuildManager.SqlBuild.Models.SqlLogStatus"/> with the newest-row hash/date
        /// and an any-row AllowScriptBlock flag, matching the semantics of <see cref="HasBlockingSqlLog"/>.
        /// </summary>
        public IReadOnlyDictionary<Guid, SqlBuildManager.SqlBuild.Models.SqlLogStatus> GetBatchBlockingSqlLog(
            IReadOnlyList<Guid> scriptIds, ConnectionData cData, string databaseName)
        {
            var result = new Dictionary<Guid, SqlBuildManager.SqlBuild.Models.SqlLogStatus>();

            if (scriptIds == null || scriptIds.Count == 0 || string.IsNullOrWhiteSpace(databaseName))
                return result;

            var targetData = new ConnectionData()
            {
                DatabaseName = databaseName,
                SQLServerName = cData.SQLServerName,
                UserId = cData.UserId,
                Password = cData.Password,
                AuthenticationType = cData.AuthenticationType,
                ScriptTimeout = 2,
                ManagedIdentityClientId = cData.ManagedIdentityClientId,
                DatabasePlatform = cData.DatabasePlatform
            };

            for (int chunkStart = 0; chunkStart < scriptIds.Count; chunkStart += BatchMaxParams)
            {
                int chunkSize = Math.Min(BatchMaxParams, scriptIds.Count - chunkStart);
                var chunk = new List<Guid>(chunkSize);
                for (int i = chunkStart; i < chunkStart + chunkSize; i++)
                    chunk.Add(scriptIds[i]);
                QueryBatchChunk(chunk, targetData, result);
            }

            return result;
        }

        private void QueryBatchChunk(
            IList<Guid> chunk, ConnectionData targetData,
            Dictionary<Guid, SqlBuildManager.SqlBuild.Models.SqlLogStatus> result)
        {
            using var conn = SqlBuildManager.Connection.ConnectionHelper.GetDbConnection(targetData);
            using DbCommand cmd = conn.CreateCommand();
            cmd.CommandText = resourceProvider.GetBatchHasBlockingSqlLogQuery(chunk.Count);
            for (int i = 0; i < chunk.Count; i++)
            {
                var p = cmd.CreateParameter();
                p.ParameterName = $"@p{i}";
                p.Value = chunk[i];
                cmd.Parameters.Add(p);
            }

            // Per-script tracking: first row seen = newest (ORDER BY CommitDate DESC), any row can set hasBlock
            var firstRows = new Dictionary<Guid, (string scriptHash, string scriptTextHash, DateTime commitDate)>();
            var hasBlockSet = new HashSet<Guid>();

            cmd.Connection!.Open();
            using DbDataReader reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                if (reader[0] == DBNull.Value) continue;
                if (!Guid.TryParse(reader[0].ToString(), out Guid scriptId)) continue;

                bool allowBlock = reader[1] != DBNull.Value && Convert.ToBoolean(reader[1]);
                if (allowBlock) hasBlockSet.Add(scriptId);

                if (!firstRows.ContainsKey(scriptId))
                {
                    string scriptHash = reader[2] == DBNull.Value ? string.Empty : reader[2].ToString() ?? string.Empty;
                    DateTime commitDate = reader[3] == DBNull.Value
                        ? DateTime.MinValue
                        : DateTime.Parse(reader[3].ToString()!);
                    string scriptTextHash = reader[4] == DBNull.Value
                        ? string.Empty
                        : fileHelper.GetSHA1Hash(reader[4].ToString()!);
                    firstRows[scriptId] = (scriptHash, scriptTextHash, commitDate);
                }
            }

            foreach (var kvp in firstRows)
            {
                result[kvp.Key] = new SqlBuildManager.SqlBuild.Models.SqlLogStatus(
                    HasBlock: hasBlockSet.Contains(kvp.Key),
                    ScriptHash: kvp.Value.scriptHash,
                    ScriptTextHash: kvp.Value.scriptTextHash,
                    CommitDate: kvp.Value.commitDate);
            }
        }
        /// <summary>
        /// Quick check to see if the specicified script has a block against it.
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="connData">The BuildConnectData for the target database</param>
        /// <returns>True if there is a block</returns>
        public bool GetBlockingSqlLog(System.Guid scriptId, ref BuildConnectData connData)
        {
            try
            {
                DbCommand cmd = connData.Connection.CreateCommand();
                cmd.CommandText = resourceProvider.GetBlockingScriptLogQuery();
                cmd.Transaction = connData.Transaction;
                var param = cmd.CreateParameter();
                param.ParameterName = "@ScriptId";
                param.Value = scriptId;
                cmd.Parameters.Add(param);
                object? has = cmd.ExecuteScalar();
                if (has == null || has == DBNull.Value)
                    return false;
                else
                    return true;
            }
            catch // most likely get here because the table doesn't exist?
            {
                return false;
            }
        }

        /// <summary>
        /// Returns the build run log for the specified script
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="connData">The ConnectionData object for the target database</param>
        /// <returns>ScriptRunLog table containing the history</returns>
        public IReadOnlyList<SqlBuildManager.SqlBuild.Models.ScriptRunLogEntry> GetScriptRunLog(System.Guid scriptId, ConnectionData connData)
        {
            try
            {
                using var conn = SqlBuildManager.Connection.ConnectionHelper.GetDbConnection(connData);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = resourceProvider.GetScriptRunLogQuery();
                var param = cmd.CreateParameter();
                param.ParameterName = "@ScriptId";
                param.Value = scriptId;
                cmd.Parameters.Add(param);
                var list = new List<SqlBuildManager.SqlBuild.Models.ScriptRunLogEntry>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(ReadScriptRunLogEntry(reader));
                }
                return list;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unable to retrieve script run log for {scriptId.ToString()} on database {connData.SQLServerName}.{connData.DatabaseName}");
                throw new ApplicationException("Error retrieving Script Run Log", e);
            }
        }

        /// <summary>
        /// Returns the build run log for the specified script
        /// </summary>
        /// <param name="scriptId">Guid for the script in question</param>
        /// <param name="connData">The ConnectionData object for the target database</param>
        /// <returns>ScriptRunLog table containing the history</returns>
        public IReadOnlyList<SqlBuildManager.SqlBuild.Models.ScriptRunLogEntry> GetObjectRunHistoryLog(string objectFileName, ConnectionData connData)
        {
            try
            {
                using var conn = SqlBuildManager.Connection.ConnectionHelper.GetDbConnection(connData);
                conn.Open();
                using var cmd = conn.CreateCommand();
                cmd.CommandText = resourceProvider.GetObjectRunHistoryQuery();
                var param = cmd.CreateParameter();
                param.ParameterName = "@ScriptFileName";
                param.Value = objectFileName;
                cmd.Parameters.Add(param);
                var list = new List<SqlBuildManager.SqlBuild.Models.ScriptRunLogEntry>();
                using var reader = cmd.ExecuteReader();
                while (reader.Read())
                {
                    list.Add(ReadScriptRunLogEntry(reader));
                }
                return list;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Unable to retrieve object history for {objectFileName} on database {connData.SQLServerName}.{connData.DatabaseName}");
                throw new ApplicationException("Error retrieving Script Run Log", e);
            }
        }
        public SqlBuildManager.SqlBuild.Models.ScriptRunLogEntry ReadScriptRunLogEntry(IDataRecord reader)
        {
            Guid? TryGuid(string name)
            {
                try { var val = reader[name]; if (val == DBNull.Value) return null; return Guid.Parse(val.ToString() ?? string.Empty); } catch { return null; }
            }

            bool? TryBool(string name)
            {
                try { var val = reader[name]; if (val == DBNull.Value) return null; return Convert.ToBoolean(val, CultureInfo.InvariantCulture); } catch { return null; }
            }

            int? TryInt(string name)
            {
                try { var val = reader[name]; if (val == DBNull.Value) return null; return Convert.ToInt32(val, CultureInfo.InvariantCulture); } catch { return null; }
            }

            DateTime? TryDate(string name)
            {
                try { var val = reader[name]; if (val == DBNull.Value) return null; return Convert.ToDateTime(val, CultureInfo.InvariantCulture); } catch { return null; }
            }

            string? TryString(string name)
            {
                try { var val = reader[name]; if (val == DBNull.Value) return null; return val.ToString(); } catch { return null; }
            }

            return new SqlBuildManager.SqlBuild.Models.ScriptRunLogEntry(
                BuildFileName: TryString("BuildFileName"),
                ScriptFileName: TryString("ScriptFileName"),
                ScriptId: TryGuid("ScriptId"),
                ScriptFileHash: TryString("ScriptFileHash"),
                CommitDate: TryDate("CommitDate"),
                Sequence: TryInt("Sequence"),
                UserId: TryString("UserId"),
                AllowScriptBlock: TryBool("AllowScriptBlock"),
                AllowBlockUpdateId: TryString("AllowBlockUpdateId"),
                ScriptText: TryString("ScriptText"),
                Tag: TryString("Tag"));
        }

        public SqlSyncBuildDataModel ClearScriptBlocks(ClearScriptData scrData, ConnectionData connData, IProgressReporter progressReporter, ISqlBuildRunnerProperties runnerProperties)//, DoWorkEventArgs e)
        {
            throw new NotImplementedException();

            //string projectFileName = scrData.ProjectFileName;
            //var model = scrData.BuildDataModel;
            //if (model == null)
            //    throw new ArgumentException("ClearScriptData must provide BuildDataModel", nameof(scrData));
            //string buildFileName = scrData.BuildZipFileName;
            //string[] selectedScriptIds = scrData.SelectedScriptIds;

            //progressReporter.ReportProgress(0, new GeneralStatusEventArgs("Clearing Script Blocks"));

            //sqlLoggingService.EnsureLogTablePresence(connectionsService.Connections, runnerProperties.LogToDataBaseName);

            //SqlCommand cmd = new SqlCommand("UPDATE SqlBuild_Logging SET AllowScriptBlock = 0, AllowBlockUpdateId = @UserId WHERE ScriptId = @ScriptId AND AllowScriptBlock = 1");
            //cmd.Parameters.Add("@ScriptId", SqlDbType.UniqueIdentifier);
            //cmd.Parameters.AddWithValue("@UserId", System.Environment.UserName);
            //var scriptsById = model.Script
            //    .Where(s => s.ScriptId != null)
            //    .ToDictionary(s => s.ScriptId!, s => s, StringComparer.OrdinalIgnoreCase);
            //var updatedCommitted = model.CommittedScript.ToList();
            //for (int i = 0; i < selectedScriptIds.Length; i++)
            //{
            //    var id = selectedScriptIds[i];
            //    if (!scriptsById.TryGetValue(id, out var script))
            //        continue;

            //    progressReporter.ReportProgress(0, new GeneralStatusEventArgs("Clearing " + (script.FileName ?? id)));

            //    model = ClearAllowScriptBlocks(model, connData.SQLServerName, selectedScriptIds);

            //    //Update Sql server log
            //    string targetDatabase = GetTargetDatabase(script.Database ?? string.Empty);
            //    BuildConnectData cData = connectionsService.GetBuildConnectionDataClass(connData.SQLServerName, targetDatabase);
            //    sqlLoggingService.EnsureLogTablePresence(connectionsService.Connections, runnerProperties.LogToDataBaseName);
            //    cmd.Connection = cData.Connection;
            //    cmd.Transaction = cData.Transaction;
            //    cmd.Parameters["@ScriptId"].Value = new System.Guid(id);
            //    cmd.ExecuteNonQuery();
            //}


            //CommitBuild();
            //SaveBuildDataSet(true);

            //progressReporter.ReportProgress(100, new GeneralStatusEventArgs("Selected Script Blocks Cleared"));
            //return model;
        }
        public SqlSyncBuildDataModel ClearAllowScriptBlocks(SqlSyncBuildDataModel model, string serverName, IReadOnlyList<string> selectedScriptIds)
        {
            var updatedCommitted = model.CommittedScript.ToList();
            var idSet = new HashSet<string>(selectedScriptIds, StringComparer.OrdinalIgnoreCase);
            for (var j = 0; j < updatedCommitted.Count; j++)
            {
                var cs = updatedCommitted[j];
                if (cs.ScriptId != null && idSet.Contains(cs.ScriptId) && string.Equals(cs.ServerName, serverName, StringComparison.OrdinalIgnoreCase))
                {
                    updatedCommitted[j] = new CommittedScript(
                        scriptId: cs.ScriptId,
                        serverName: cs.ServerName,
                        committedDate: cs.CommittedDate,
                        allowScriptBlock: false,
                        scriptHash: cs.ScriptHash,
                        sqlSyncBuildProjectId: cs.SqlSyncBuildProjectId);
                }
            }
            return new SqlSyncBuildDataModel(
                sqlSyncBuildProject: model.SqlSyncBuildProject,
                script: model.Script,
                build: model.Build,
                scriptRun: model.ScriptRun,
                committedScript: updatedCommitted);
        }

    }
   

    }
