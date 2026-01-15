using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    internal class DefaultDatabaseUtility :IDatabaseUtility
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly ISqlLoggingService sqlLoggingService;
        private readonly IConnectionsService connectionsService;
        private readonly IProgressReporter progressReporter;
        private readonly ISqlBuildFileHelper fileHelper;

        public DefaultDatabaseUtility(IConnectionsService connectionsService, ISqlLoggingService sqlLoggingService, IProgressReporter progressReporter, ISqlBuildFileHelper fileHelper) 
        {
            this.connectionsService = connectionsService;
            this.sqlLoggingService = sqlLoggingService;
            this.progressReporter = progressReporter;
            this.fileHelper = fileHelper ?? new DefaultSqlBuildFileHelper();
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

            SqlCommand cmd = new SqlCommand("SELECT AllowScriptBlock,ScriptFileHash,CommitDate,ScriptText FROM SqlBuild_Logging WITH (NOLOCK) WHERE ScriptId = @ScriptId ORDER BY CommitDate DESC");
            cmd.Parameters.AddWithValue("@ScriptId", scriptId);
            cmd.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(databaseName, cData.SQLServerName, cData.UserId, cData.Password, cData.AuthenticationType, 2, cData.ManagedIdentityClientId);
            try
            {
                cmd.Connection.Open();
                int i = 0;
                using (SqlDataReader reader = cmd.ExecuteReader(CommandBehavior.CloseConnection))
                {
                    while (reader.Read())
                    {
                        if (i == 0)
                        {
                            scriptHash = (reader[1] == DBNull.Value) ? string.Empty : reader[1].ToString();
                            commitDate = (reader[2] == DBNull.Value) ? DateTime.MinValue : DateTime.Parse(reader[2].ToString());
                            scriptTextHash = (reader[3] == DBNull.Value) ? string.Empty : fileHelper.GetSHA1Hash(reader[3].ToString());
                            i++;
                        }

                        if (reader.GetSqlBoolean(0) == true)
                        {
                            hasBlock = true;
                            break;
                        }
                    }
                    reader.Close();
                }
                return hasBlock;
            }
            catch (SqlException)
            {
                //swallow the exception
                return false;
            }
            catch (Exception exe)
            {
                log.LogWarning(exe, $"Unable to check for blocking SQL for script {scriptId.ToString()} on database {cmd.Connection.DataSource}.{cmd.Connection.Database}");
                return false;
            }
            finally
            {
                cmd.Connection.Close();
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
                SqlCommand cmd = new SqlCommand("SELECT * FROM SqlBuild_Logging WHERE ScriptId = @ScriptId AND AllowScriptBlock = 1", connData.Connection, connData.Transaction);
                cmd.Parameters.AddWithValue("@ScriptId", scriptId);
                object has = cmd.ExecuteScalar();
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
        public IReadOnlyList<SqlSync.SqlBuild.Models.ScriptRunLogEntry> GetScriptRunLog(System.Guid scriptId, ConnectionData connData)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM SqlBuild_Logging WITH (NOLOCK) WHERE ScriptId = @ScriptId ORDER BY CommitDate DESC");
                cmd.Parameters.AddWithValue("@ScriptId", scriptId);
                cmd.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
                cmd.Connection.Open();
                var list = new List<SqlSync.SqlBuild.Models.ScriptRunLogEntry>();
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
        public IReadOnlyList<SqlSync.SqlBuild.Models.ScriptRunLogEntry> GetObjectRunHistoryLog(string objectFileName, ConnectionData connData)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT * FROM SqlBuild_Logging WITH (NOLOCK) WHERE [ScriptFileName] = @ScriptFileName ORDER BY CommitDate DESC");
                cmd.Parameters.AddWithValue("@ScriptFileName", objectFileName);
                cmd.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(connData);
                cmd.Connection.Open();
                var list = new List<SqlSync.SqlBuild.Models.ScriptRunLogEntry>();
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
        public SqlSync.SqlBuild.Models.ScriptRunLogEntry ReadScriptRunLogEntry(IDataRecord reader)
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

            return new SqlSync.SqlBuild.Models.ScriptRunLogEntry(
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
            string projectFileName = scrData.ProjectFileName;
            var model = scrData.BuildDataModel;
            if (model == null)
                throw new ArgumentException("ClearScriptData must provide BuildDataModel", nameof(scrData));
            string buildFileName = scrData.BuildZipFileName;
            string[] selectedScriptIds = scrData.SelectedScriptIds;

            progressReporter.ReportProgress(0, new GeneralStatusEventArgs("Clearing Script Blocks"));

            sqlLoggingService.EnsureLogTablePresence();

            SqlCommand cmd = new SqlCommand("UPDATE SqlBuild_Logging SET AllowScriptBlock = 0, AllowBlockUpdateId = @UserId WHERE ScriptId = @ScriptId AND AllowScriptBlock = 1");
            cmd.Parameters.Add("@ScriptId", SqlDbType.UniqueIdentifier);
            cmd.Parameters.AddWithValue("@UserId", System.Environment.UserName);
            var scriptsById = model.Script
                .Where(s => s.ScriptId != null)
                .ToDictionary(s => s.ScriptId!, s => s, StringComparer.OrdinalIgnoreCase);
            var updatedCommitted = model.CommittedScript.ToList();
            for (int i = 0; i < selectedScriptIds.Length; i++)
            {
                var id = selectedScriptIds[i];
                if (!scriptsById.TryGetValue(id, out var script))
                    continue;

                progressReporter.ReportProgress(0, new GeneralStatusEventArgs("Clearing " + (script.FileName ?? id)));

                model = ClearAllowScriptBlocks(model, connData.SQLServerName, selectedScriptIds);

                //Update Sql server log
                string targetDatabase = GetTargetDatabase(script.Database ?? string.Empty);
                BuildConnectData cData = connectionsService.GetBuildConnectionDataClass(connData.SQLServerName, targetDatabase);
                sqlLoggingService.EnsureLogTablePresence(connectionsService.Connections, runnerProperties.LogToDataBaseName);
                cmd.Connection = cData.Connection;
                cmd.Transaction = cData.Transaction;
                cmd.Parameters["@ScriptId"].Value = new System.Guid(id);
                cmd.ExecuteNonQuery();
            }


            CommitBuild();
            SaveBuildDataSet(true);

            progressReporter.ReportProgress(100, new GeneralStatusEventArgs("Selected Script Blocks Cleared"));
            return model;
        }


    }
}
