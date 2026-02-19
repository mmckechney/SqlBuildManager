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
using System.Data.Common;
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
        private readonly ISqlResourceProvider resourceProvider;
        private readonly IScriptSyntaxProvider syntaxProvider;

        // Static cache for verified logging tables (server:database combinations that have been confirmed)
        private static readonly HashSet<string> _verifiedLoggingTables = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        private static readonly object _cacheLock = new object();

        public DefaultSqlLoggingService(IConnectionsService connectionsService, IProgressReporter progressReporter, ISqlResourceProvider resourceProvider = null, IScriptSyntaxProvider syntaxProvider = null) 
        {
            this.connectionsService = connectionsService;
            this.progressReporter = progressReporter;
            this.resourceProvider = resourceProvider ?? new SqlServerResourceProvider();
            this.syntaxProvider = syntaxProvider ?? new SqlServerSyntaxProvider();
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
            DbCommand createTableCmd = null;
            DbCommand createCommitIndex = null;
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
                    createTableCmd = connData.Connection.CreateCommand();
                    createTableCmd.CommandText = resourceProvider.LoggingTableDdl;
                    if (createTableCmd.Connection is SqlConnection sqlConn1)
                        sqlConn1.InfoMessage += new SqlInfoMessageEventHandler(Connection_InfoMessage);

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
                    createCommitIndex = connData.Connection.CreateCommand();
                    createCommitIndex.CommandText = resourceProvider.LoggingTableCommitCheckIndex;
                    if (createCommitIndex.Connection is SqlConnection sqlConn2)
                        sqlConn2.InfoMessage += new SqlInfoMessageEventHandler(Connection_InfoMessage);

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
                    if (createTableCmd?.Connection is SqlConnection sqlConnA)
                        sqlConnA.InfoMessage -= new SqlInfoMessageEventHandler(Connection_InfoMessage);
                    if (createCommitIndex?.Connection is SqlConnection sqlConnB)
                        sqlConnB.InfoMessage -= new SqlInfoMessageEventHandler(Connection_InfoMessage);
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
        public async Task<bool> LogTableExists(DbConnection conn)
        {
            try
            {
                DbCommand cmd = conn.CreateCommand();
                cmd.CommandText = resourceProvider.CheckTableExistsQuery("SqlBuild_Logging");
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

        private static void AddParameter(DbCommand cmd, string name, object value)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            p.Value = value ?? DBNull.Value;
            cmd.Parameters.Add(p);
        }

        private static DbParameter AddParameter(DbCommand cmd, string name)
        {
            var p = cmd.CreateParameter();
            p.ParameterName = name;
            cmd.Parameters.Add(p);
            return p;
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

            // Build multi-row INSERT statement using lowercase unquoted column names (compatible with both SQL Server and PostgreSQL)
            var tableName = resourceProvider is PostgresResourceProvider ? "sqlbuild_logging" : "SqlBuild_Logging";
            var sql = new StringBuilder();
            sql.AppendLine($"INSERT INTO {tableName}(BuildFileName,ScriptFileName,ScriptId,ScriptFileHash,CommitDate,Sequence,UserId,AllowScriptBlock,ScriptText,Tag,TargetDatabase,RunWithVersion,BuildProjectHash,BuildRequestedBy,ScriptRunStart,ScriptRunEnd,Description) VALUES");

            var cmd = connData.Connection.CreateCommand();
            if (connData.Transaction != null)
                cmd.Transaction = connData.Transaction;

            for (int i = 0; i < scripts.Count; i++)
            {
                var (script, row) = scripts[i];
                
                if (i > 0) sql.Append(",");
                sql.AppendLine($"(@BuildFileName{i},@ScriptFileName{i},@ScriptId{i},@ScriptFileHash{i},@CommitDate{i},@Sequence{i},@UserId{i},{syntaxProvider.BooleanTrueLiteral},@ScriptText{i},@Tag{i},@TargetDatabase{i},@RunWithVersion{i},@BuildProjectHash{i},@BuildRequestedBy{i},@ScriptRunStart{i},@ScriptRunEnd{i},@Description{i})");

                AddParameter(cmd, $"@BuildFileName{i}", buildFileName);
                AddParameter(cmd, $"@ScriptFileName{i}", row.FileName);
                AddParameter(cmd, $"@ScriptId{i}", script.ScriptId);
                AddParameter(cmd, $"@ScriptFileHash{i}", script.FileHash);
                AddParameter(cmd, $"@CommitDate{i}", commitDate);
                AddParameter(cmd, $"@Sequence{i}", script.Sequence);
                AddParameter(cmd, $"@UserId{i}", userId);
                AddParameter(cmd, $"@ScriptText{i}", script.ScriptText);
                AddParameter(cmd, $"@Tag{i}", script.Tag ?? "");
                AddParameter(cmd, $"@TargetDatabase{i}", script.DatabaseTarget);
                AddParameter(cmd, $"@RunWithVersion{i}", runWithVersion);
                AddParameter(cmd, $"@BuildProjectHash{i}", buildProjectHash);
                AddParameter(cmd, $"@BuildRequestedBy{i}", buildRequestedBy);
                AddParameter(cmd, $"@ScriptRunStart{i}", script.RunStart);
                AddParameter(cmd, $"@ScriptRunEnd{i}", script.RunEnd);
                AddParameter(cmd, $"@Description{i}", description);
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

            var cmd = connData.Connection.CreateCommand();
            cmd.CommandText = resourceProvider.LogScriptInsert;
            if (connData.Transaction != null)
                cmd.Transaction = connData.Transaction;

            AddParameter(cmd, "@BuildFileName", buildFileName);
            AddParameter(cmd, "@UserId", userId);
            AddParameter(cmd, "@CommitDate", commitDate);
            AddParameter(cmd, "@RunWithVersion", runWithVersion);
            AddParameter(cmd, "@BuildProjectHash", buildProjectHash);
            AddParameter(cmd, "@BuildRequestedBy", buildRequestedBy);
            AddParameter(cmd, "@Description", description);
            AddParameter(cmd, "@ScriptFileName");
            AddParameter(cmd, "@ScriptId");
            AddParameter(cmd, "@ScriptFileHash");
            AddParameter(cmd, "@Sequence");
            AddParameter(cmd, "@ScriptText");
            AddParameter(cmd, "@Tag");
            AddParameter(cmd, "@TargetDatabase");
            AddParameter(cmd, "@ScriptRunStart");
            AddParameter(cmd, "@ScriptRunEnd");

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
