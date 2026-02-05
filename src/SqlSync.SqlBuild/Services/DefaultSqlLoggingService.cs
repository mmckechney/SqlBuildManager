using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.SqlLogging;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using sqlLog = SqlSync.SqlBuild.SqlLogging;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class DefaultSqlLoggingService : ISqlLoggingService
    {
        private string sqlInfoMessage = string.Empty;
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private readonly IConnectionsService connectionsService;
        private readonly IProgressReporter progressReporter;

        // Static cache for verified logging tables (server:database combinations that have been confirmed)
        private static readonly HashSet<string> _verifiedLoggingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _cacheLock = new object();

        public DefaultSqlLoggingService(IConnectionsService connectionsService, IProgressReporter progressReporter) 
        {
            this.connectionsService = connectionsService;
            this.progressReporter = progressReporter;
        }

        private static string GetCacheKey(string serverName, string databaseName) => $"{serverName}:{databaseName}";

        private static bool IsLoggingTableVerified(string serverName, string databaseName)
        {
            lock (_cacheLock)
            {
                return _verifiedLoggingTables.Contains(GetCacheKey(serverName, databaseName));
            }
        }

        private static void MarkLoggingTableVerified(string serverName, string databaseName)
        {
            lock (_cacheLock)
            {
                _verifiedLoggingTables.Add(GetCacheKey(serverName, databaseName));
            }
        }

        /// <summary>
        /// Clears the static cache of verified logging tables. Useful for testing.
        /// </summary>
        internal static void ClearLoggingTableCache()
        {
            lock (_cacheLock)
            {
                _verifiedLoggingTables.Clear();
            }
        }

        /// <summary>
        /// Ensures that the SqlBuild_Logging table exists and that it is setup properly. Self-heals if it is not.
        /// </summary>
        public async Task<string> EnsureLogTablePresence(Dictionary<string, BuildConnectData> connectDictionary, string logToDatabaseName)
        {
            // Check connections that haven't been confirmed yet
            var unconfirmedLogTable = connectionsService.Connections
                .Where(c => !c.Value.HasLoggingTable)
                .ToDictionary();

            if(unconfirmedLogTable.Count == 0)
            {
                return "";
            }
            //Self healing: add the table if needed
            sqlInfoMessage = string.Empty;
            SqlCommand createTableCmd = new SqlCommand(Properties.Resources.LoggingTable);
            SqlCommand createCommitIndex = new SqlCommand(Properties.Resources.LoggingTableCommitCheckIndex);
            Dictionary<string, BuildConnectData>.KeyCollection keys = unconfirmedLogTable.Keys;
            foreach (string key in keys)
            {
                var connData = (BuildConnectData)connectDictionary[key];
                var serverName = connData.Connection.DataSource;
                var databaseName = connData.Connection.Database;

                // Check static cache first - if already verified in this session, skip
                if (IsLoggingTableVerified(serverName, databaseName))
                {
                    connData.HasLoggingTable = true;
                    continue;
                }

                if(await LogTableExists(connData.Connection))
                {
                    connData.HasLoggingTable = true;
                    MarkLoggingTableVerified(serverName, databaseName);
                    continue;
                }
                try
                {

                    createTableCmd.Connection = connData.Connection;
                    createTableCmd.Connection.InfoMessage += new SqlInfoMessageEventHandler(Connection_InfoMessage);

                    //If there is an alternate target for logging, check to see if this connection is for that database, if not, skip it.
                    if (logToDatabaseName.Length > 0 && !createTableCmd.Connection.Database.Equals(logToDatabaseName, StringComparison.CurrentCultureIgnoreCase))
                        continue;

                    if (createTableCmd.Connection.State == ConnectionState.Closed)
                        createTableCmd.Connection.Open();

                    if (connData.Transaction != null)
                        createTableCmd.Transaction = connData.Transaction;

                    await createTableCmd.ExecuteNonQueryAsync();
                    log.LogDebug($"EnsureLogTablePresence Table Sql Messages for {createTableCmd.Connection.DataSource}.{createTableCmd.Connection.Database}:\r\n{sqlInfoMessage}");

                    //Ensure the indexes are there
                    createCommitIndex.Connection = connData.Connection;
                    createCommitIndex.Connection.InfoMessage += new SqlInfoMessageEventHandler(Connection_InfoMessage);

                    //If there is an alternate target for logging, check to see if this connection is for that database, if not, skip it.
                    if (logToDatabaseName.Length > 0 && !createCommitIndex.Connection.Database.Equals(logToDatabaseName, StringComparison.CurrentCultureIgnoreCase))
                        continue;


                    if (createCommitIndex.Connection.State == ConnectionState.Closed)
                        createCommitIndex.Connection.Open();

                    if (connData.Transaction != null)
                        createCommitIndex.Transaction = connData.Transaction;

                    await createCommitIndex.ExecuteNonQueryAsync();
                    log.LogDebug($"EnsureLogTablePresence Index Sql Messages for {createCommitIndex.Connection.DataSource}.{createCommitIndex.Connection.Database}:\r\n{sqlInfoMessage}");

                    //If we made it this far, the logging table exists. Set it to true so it doesn't go through here again.
                    connData.HasLoggingTable = true;
                    MarkLoggingTableVerified(serverName, databaseName);
                }
                catch (Exception e)
                {
                    log.LogError(e, $"Error ensuring log table presence/indexes for {createTableCmd.Connection.DataSource}.{createTableCmd.Connection.Database}");
                }
                finally
                {
                    createTableCmd.Connection.InfoMessage -= new SqlInfoMessageEventHandler(Connection_InfoMessage);
                    createCommitIndex.Connection.InfoMessage -= new SqlInfoMessageEventHandler(Connection_InfoMessage);
                }
            }
            
            log.LogDebug($"sqlInfoMessage value: {sqlInfoMessage}");
            log.LogDebug("Exiting EnsureLogPresence method");
            return sqlInfoMessage;
        }
        /// <summary>
        /// Checks to see that the logging table exists or not.
        /// </summary>
        /// <param name="conn">Connection object to the target database</param>
        /// <returns></returns>
        public async Task<bool> LogTableExists(SqlConnection conn)
        {
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT 1 FROM sys.objects WITH (NOLOCK) WHERE name = 'SqlBuild_Logging' AND type = 'U'", conn);
                if (cmd.Connection.State == ConnectionState.Closed)
                    cmd.Connection.Open();

                object result = await cmd.ExecuteScalarAsync();
                if (result == null || result == System.DBNull.Value)
                    return false;
                else
                    return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        /// <summary>
        /// Adds the list of scripts that were committed with the run to the SqlBuild_Logging table
        /// </summary>
        /// <param name="committedScripts">List of CommittedScript objects</param>
        /// <param name="multiDbRunData">The MultiDbRun data for the run</param>
        /// <returns>True if the commit was successful</returns>
        public async Task<bool> LogCommittedScriptsToDatabase(List<sqlLog.CommittedScript> committedScripts, ISqlBuildRunnerProperties runnerProperties, MultiDbData multiDbRunData)
        {
            bool returnValue = true;
            //If using an alternate database to log the commits to, we need to initiate the connection objects 
            //so that the EnsureLogTablePresence method catches them and creates the tables as needed.
            if (runnerProperties.LogToDatabaseName.Length > 0 && committedScripts.Count > 0)
            {
                List<string> servers = new List<string>();
                for (int i = 0; i < committedScripts.Count; i++)
                    if (!servers.Contains(committedScripts[i].ServerName))
                        servers.Add(committedScripts[i].ServerName);

                for (int i = 0; i < servers.Count; i++)
                {
                    BuildConnectData tmp = connectionsService.GetOrAddBuildConnectionDataClass(runnerProperties.ConnectionData, servers[i], runnerProperties.LogToDatabaseName, runnerProperties.IsTransactional);
                }
            }

           await EnsureLogTablePresence(connectionsService.Connections, runnerProperties.LogToDatabaseName);

            // Use local UTC time instead of server getdate() to avoid extra connection overhead
            DateTime commitDate = DateTime.UtcNow;

            // Build a dictionary for fast script lookup by ScriptId
            var scriptLookup = runnerProperties.BuildDataModel.Script?
                .Where(s => !string.IsNullOrEmpty(s.ScriptId))
                .ToDictionary(s => s.ScriptId, StringComparer.OrdinalIgnoreCase)
                ?? new Dictionary<string, Script>();

            // Common parameters that are the same for all scripts
            string buildFileName = !String.IsNullOrEmpty(runnerProperties.BuildFileName) 
                ? runnerProperties.BuildFileName 
                : Path.GetFileName(runnerProperties.ProjectFileName);
            string userId = System.Environment.UserName;
            string runWithVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();
            string buildProjectHash = runnerProperties.BuildPackageHash;
            string buildRequestedBy = string.IsNullOrEmpty(runnerProperties.BuildRequestedBy) 
                ? System.Environment.UserDomainName + "\\" + System.Environment.UserName 
                : runnerProperties.BuildRequestedBy;
            string description = runnerProperties.BuildDescription ?? string.Empty;

            // Group scripts by their target connection (server:database)
            var scriptsByConnection = new Dictionary<string, List<(sqlLog.CommittedScript script, Script row)>>(StringComparer.OrdinalIgnoreCase);
            
            foreach (var script in committedScripts)
            {
                if (!scriptLookup.TryGetValue(script.ScriptId.ToString(), out var row))
                    continue;

                string targetDb = runnerProperties.LogToDatabaseName.Length > 0 
                    ? runnerProperties.LogToDatabaseName 
                    : script.DatabaseTarget;
                string connectionKey = $"{script.ServerName}:{targetDb}";

                if (!scriptsByConnection.ContainsKey(connectionKey))
                    scriptsByConnection[connectionKey] = new List<(sqlLog.CommittedScript, Script)>();
                
                scriptsByConnection[connectionKey].Add((script, row));
            }

            // Process each connection's batch of scripts
            foreach (var kvp in scriptsByConnection)
            {
                var scriptsForConnection = kvp.Value;
                if (scriptsForConnection.Count == 0) continue;

                var firstScript = scriptsForConnection[0].script;
                string targetDb = runnerProperties.LogToDatabaseName.Length > 0 
                    ? runnerProperties.LogToDatabaseName 
                    : firstScript.DatabaseTarget;

                BuildConnectData tmpConnDat = connectionsService.GetOrAddBuildConnectionDataClass(
                    runnerProperties.ConnectionData, firstScript.ServerName, targetDb, runnerProperties.IsTransactional);

                log.LogInformation($"Batch logging {scriptsForConnection.Count} script(s) to {tmpConnDat.ServerName}:{tmpConnDat.DatabaseName}");

                try
                {
                    await ExecuteBatchInsertAsync(
                        tmpConnDat, 
                        scriptsForConnection, 
                        buildFileName, 
                        userId, 
                        commitDate, 
                        runWithVersion, 
                        buildProjectHash, 
                        buildRequestedBy, 
                        description);
                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"Batch insert failed for {tmpConnDat.ServerName}:{tmpConnDat.DatabaseName}, falling back to individual inserts");
                    // Fallback to individual inserts if batch fails
                    returnValue = await FallbackIndividualInsertsAsync(
                        tmpConnDat, 
                        scriptsForConnection, 
                        buildFileName, 
                        userId, 
                        commitDate, 
                        runWithVersion, 
                        buildProjectHash, 
                        buildRequestedBy, 
                        description);
                }
            }

            return returnValue;
        }

        private async Task ExecuteBatchInsertAsync(
            BuildConnectData connData,
            List<(sqlLog.CommittedScript script, Script row)> scripts,
            string buildFileName,
            string userId,
            DateTime commitDate,
            string runWithVersion,
            string buildProjectHash,
            string buildRequestedBy,
            string description)
        {
            if (connData.Connection.State == ConnectionState.Closed)
                await connData.Connection.OpenAsync();

            // Build multi-row INSERT statement
            var sql = new StringBuilder();
            sql.AppendLine("INSERT INTO SqlBuild_Logging([BuildFileName],[ScriptFileName],[ScriptId],[ScriptFileHash],[CommitDate],[Sequence],[UserId],[AllowScriptBlock],[ScriptText],[Tag],[TargetDatabase],[RunWithVersion],[BuildProjectHash],[BuildRequestedBy],[ScriptRunStart],[ScriptRunEnd],[Description]) VALUES");

            var cmd = new SqlCommand();
            cmd.Connection = connData.Connection;
            if (connData.Transaction != null)
                cmd.Transaction = connData.Transaction;

            for (int i = 0; i < scripts.Count; i++)
            {
                var (script, row) = scripts[i];
                
                if (i > 0) sql.Append(",");
                sql.AppendLine($"(@BuildFileName{i},@ScriptFileName{i},@ScriptId{i},@ScriptFileHash{i},@CommitDate{i},@Sequence{i},@UserId{i},1,@ScriptText{i},@Tag{i},@TargetDatabase{i},@RunWithVersion{i},@BuildProjectHash{i},@BuildRequestedBy{i},@ScriptRunStart{i},@ScriptRunEnd{i},@Description{i})");

                cmd.Parameters.AddWithValue($"@BuildFileName{i}", buildFileName);
                cmd.Parameters.AddWithValue($"@ScriptFileName{i}", row.FileName);
                cmd.Parameters.AddWithValue($"@ScriptId{i}", script.ScriptId);
                cmd.Parameters.AddWithValue($"@ScriptFileHash{i}", script.FileHash);
                cmd.Parameters.AddWithValue($"@CommitDate{i}", commitDate);
                cmd.Parameters.AddWithValue($"@Sequence{i}", script.Sequence);
                cmd.Parameters.AddWithValue($"@UserId{i}", userId);
                cmd.Parameters.AddWithValue($"@ScriptText{i}", script.ScriptText);
                cmd.Parameters.AddWithValue($"@Tag{i}", script.Tag ?? "");
                cmd.Parameters.AddWithValue($"@TargetDatabase{i}", script.DatabaseTarget);
                cmd.Parameters.AddWithValue($"@RunWithVersion{i}", runWithVersion);
                cmd.Parameters.AddWithValue($"@BuildProjectHash{i}", buildProjectHash);
                cmd.Parameters.AddWithValue($"@BuildRequestedBy{i}", buildRequestedBy);
                cmd.Parameters.AddWithValue($"@ScriptRunStart{i}", script.RunStart);
                cmd.Parameters.AddWithValue($"@ScriptRunEnd{i}", script.RunEnd);
                cmd.Parameters.AddWithValue($"@Description{i}", description);
            }

            cmd.CommandText = sql.ToString();
            await cmd.ExecuteNonQueryAsync();
        }

        private async Task<bool> FallbackIndividualInsertsAsync(
            BuildConnectData connData,
            List<(sqlLog.CommittedScript script, Script row)> scripts,
            string buildFileName,
            string userId,
            DateTime commitDate,
            string runWithVersion,
            string buildProjectHash,
            string buildRequestedBy,
            string description)
        {
            bool success = true;
            
            if (connData.Connection.State == ConnectionState.Closed)
                await connData.Connection.OpenAsync();

            var cmd = new SqlCommand(Properties.Resources.LogScript);
            cmd.Connection = connData.Connection;
            if (connData.Transaction != null)
                cmd.Transaction = connData.Transaction;

            cmd.Parameters.AddWithValue("@BuildFileName", buildFileName);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@CommitDate", commitDate);
            cmd.Parameters.AddWithValue("@RunWithVersion", runWithVersion);
            cmd.Parameters.AddWithValue("@BuildProjectHash", buildProjectHash);
            cmd.Parameters.AddWithValue("@BuildRequestedBy", buildRequestedBy);
            cmd.Parameters.AddWithValue("@Description", description);
            cmd.Parameters.Add("@ScriptFileName", SqlDbType.VarChar);
            cmd.Parameters.Add("@ScriptId", SqlDbType.UniqueIdentifier);
            cmd.Parameters.Add("@ScriptFileHash", SqlDbType.VarChar);
            cmd.Parameters.Add("@Sequence", SqlDbType.Int);
            cmd.Parameters.Add("@ScriptText", SqlDbType.Text);
            cmd.Parameters.Add("@Tag", SqlDbType.VarChar);
            cmd.Parameters.Add("@TargetDatabase", SqlDbType.VarChar);
            cmd.Parameters.Add("@ScriptRunStart", SqlDbType.DateTime);
            cmd.Parameters.Add("@ScriptRunEnd", SqlDbType.DateTime);

            foreach (var (script, row) in scripts)
            {
                try
                {
                    cmd.Parameters["@ScriptFileName"].Value = row.FileName;
                    cmd.Parameters["@ScriptId"].Value = script.ScriptId;
                    cmd.Parameters["@ScriptFileHash"].Value = script.FileHash;
                    cmd.Parameters["@Sequence"].Value = script.Sequence;
                    cmd.Parameters["@ScriptText"].Value = script.ScriptText;
                    cmd.Parameters["@Tag"].Value = script.Tag ?? "";
                    cmd.Parameters["@TargetDatabase"].Value = script.DatabaseTarget;
                    cmd.Parameters["@ScriptRunStart"].Value = script.RunStart;
                    cmd.Parameters["@ScriptRunEnd"].Value = script.RunEnd;

                    await cmd.ExecuteNonQueryAsync();
                }
                catch (Exception ex)
                {
                    log.LogError(ex, $"Unable to log script {row.FileName}");
                    success = false;
                }
            }

            return success;
        }

        private void Connection_InfoMessage(object sender, SqlInfoMessageEventArgs e)
        {
            StringBuilder messages = new StringBuilder();
            foreach (SqlError err in e.Errors)
            {
                messages.Append(err.Message + "\r\n");
            }

            sqlInfoMessage += messages.ToString();
        }
    }
}
