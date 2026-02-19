using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Text;
using Microsoft.Extensions.Logging;
using static SqlSync.SqlBuild.SqlBuildHelper;
using BuildModels = SqlSync.SqlBuild.Models;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;

namespace SqlSync.SqlBuild
{

    internal sealed record SqlExecutionResult(bool Success, string Results, bool TimeoutDetected = false);

    /// <summary>
    /// Extracted runner responsible for executing build scripts. All stateful dependencies are provided via context.
    /// </summary>
    internal class SqlBuildRunner
    {
        private readonly ISqlBuildRunnerContext _ctx;
        private readonly ISqlCommandExecutor _executor;
        private readonly ISqlBuildFileHelper _fileHelper;
        private readonly IConnectionsService _connectionsService;
        private readonly ISqlLoggingService _sqlLoggingService;
        private readonly IBuildFinalizer _buildFinalizer;
        private readonly IProgressReporter _progressReporter;
        private readonly IBuildFinalizerContext _finalizerContext;
        private readonly ITransactionManager _transactionManager;


        public SqlBuildRunner(IConnectionsService connectionsService, ISqlBuildRunnerContext ctx, IBuildFinalizerContext finalizerContext, ISqlCommandExecutor executor = null, ISqlBuildFileHelper fileHelper = null, IBuildFinalizer buildFinalizer = null, ISqlLoggingService sqlLoggingService = null, IProgressReporter progressReporter = null, ITransactionManager transactionManager = null)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _executor = executor ?? new SqlCommandExecutor(ctx.Log);
            _fileHelper = fileHelper ?? new DefaultSqlBuildFileHelper();
            _progressReporter = progressReporter ?? new DefaultProgressReporter();
            _sqlLoggingService = sqlLoggingService ?? new DefaultSqlLoggingService(connectionsService, _progressReporter);
            _buildFinalizer = buildFinalizer ?? new DefaultBuildFinalizer(_sqlLoggingService, _progressReporter);
            _connectionsService = connectionsService ?? new DefaultConnectionsService();
            _finalizerContext = finalizerContext ?? throw new ArgumentNullException(nameof(finalizerContext));
            _transactionManager = transactionManager ?? new SqlServerTransactionManager();


        }

        public virtual async Task<BuildModels.Build> RunAsync(
            IList<BuildModels.Script> scripts,
            BuildModels.Build myBuild,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            BuildModels.SqlSyncBuildDataModel buildDataModel,
            CancellationToken cancellationToken = default)
        {
            var progress = _ctx.ProgressReporter ?? new DefaultProgressReporter();
            var log = _ctx.Log;
            var committedScripts = _ctx.CommittedScripts;

            log.LogInformation("Proceeding with Build");
            log.LogDebug($"Processing with build for build Package hash = {_ctx.BuildPackageHash}");

            int overallIndex = 0;
            int runSequence = 0;
            bool buildFailure = false;
            bool failureDueToScriptTimeout = false;
            var dbTargets = new List<string>();

            try
            {
                ValidateScriptsInput(scripts);

                for (int i = 0; i < scripts.Count; i++)
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    var script = scripts[i];
                    var scriptId = script.ScriptId ?? Guid.NewGuid().ToString();
                    var fileName = script.FileName ?? string.Empty;
                    var stripTransaction = script.StripTransactionText ?? false;
                    var rollBackOnError = script.RollBackOnError ?? true;
                    var causesBuildFailure = script.CausesBuildFailure ?? true;
                    var allowMultipleRuns = script.AllowMultipleRuns ?? true;
                    var scriptTimeout = script.ScriptTimeOut ?? _ctx.DefaultScriptTimeout;
                    var targetDatabase = _ctx.GetTargetDatabase(script.Database ?? string.Empty);
                    dbTargets.Add(targetDatabase);

                    var batchScripts = await LoadBatchScriptsAsync(scriptId, fileName, stripTransaction, scriptBatchColl, cancellationToken).ConfigureAwait(false);

                    if (!allowMultipleRuns && ShouldSkipDueToCommittedScripts(scriptId, buildDataModel))
                    {
                        log.LogInformation($"Skipping pre-run script {fileName}");
                        progress.ReportProgress(0, new ScriptRunStatusEventArgs("Skipped Pre-Run script", TimeSpan.Zero));
                        continue;
                    }

                    BuildConnectData cData;
                    var scriptRunRowId = Guid.NewGuid();
                    BuildModels.ScriptRun currentRun = null;
                    try
                    {
                        cData = _connectionsService.GetOrAddBuildConnectionDataClass(_ctx.ConnectionData, serverName, targetDatabase, _ctx.IsTransactional);
                        currentRun = new BuildModels.ScriptRun(
                            fileHash: null,
                            results: string.Empty,
                            runStart: DateTime.Now,
                            runEnd: null,
                            success: false,
                            fileName: fileName,
                            runOrder: i + 1,
                            database: targetDatabase,
                            scriptRunId: scriptRunRowId.ToString(),
                            buildId: myBuild.BuildId);
                    }
                    catch (Exception e)
                    {
                        log.LogError(e, "Error establishing connection data");
                        _ctx.ErrorOccured = true;
                        buildFailure = true;
                        progress.ReportProgress(0, new ScriptRunStatusEventArgs($"ERROR connecting to {serverName}.{targetDatabase}", TimeSpan.Zero));
                        if (currentRun != null)
                        {
                            currentRun.Success = false;
                            currentRun.Results = "Connection failed";
                            _ctx.AddScriptRunToHistory(currentRun, myBuild);
                        }
                        if (buildFailure) break;
                        continue;
                    }

                    // hash & commit staging
                    string textHash = _fileHelper.GetSHA1Hash(batchScripts);
                    currentRun.FileHash = textHash;
                    var scriptText = _fileHelper.JoinBatchedScripts(batchScripts);
                    var tmpCommitted = new LoggingCommittedScript(new Guid(scriptId), currentRun.FileHash, runSequence++, scriptText, script.Tag, cData.ServerName, cData.DatabaseName);

                    var savePointName = scriptId.Replace("-", "");
                    TryCreateSavePoint(cData, savePointName, serverName, targetDatabase, fileName);

                    var start = DateTime.Now;
                    for (int x = 0; x < batchScripts.Length; x++)
                    {
                        if (cancellationToken.IsCancellationRequested)
                        {
                            log.LogInformation("Encountered cancellation pending directive. Breaking out of build");
                            break;
                        }

                        batchScripts[x] = _ctx.PerformScriptTokenReplacement(batchScripts[x]);
                        overallIndex++;

                        try
                        {
                            if (_ctx.RunScriptOnly)
                            {
                                _ctx.PublishScriptLog(false, new ScriptLogEventArgs(overallIndex, batchScripts[x], targetDatabase, fileName, "Scripted"));
                                continue;
                            }

                            var execResult = await _executor.ExecuteAsync(batchScripts[x], scriptTimeout, cData, _ctx.IsTransactional, cancellationToken).ConfigureAwait(false);
                            failureDueToScriptTimeout = failureDueToScriptTimeout || execResult.TimeoutDetected;
                            currentRun.Success = true;
                            currentRun.Results = (currentRun.Results ?? string.Empty) + execResult.Results;
                            _ctx.PublishScriptLog(false, new ScriptLogEventArgs(overallIndex, batchScripts[x], targetDatabase, fileName, currentRun.Results + _ctx.SqlInfoMessage));
                        }
                        catch (SqlException e)
                        {
                            var (handledBuildFailure, timeoutDetected) = HandleSqlException(e, fileName, batchScripts[x], targetDatabase, savePointName, start, rollBackOnError, causesBuildFailure, cData, ref currentRun);
                            failureDueToScriptTimeout = failureDueToScriptTimeout || timeoutDetected;
                            buildFailure = buildFailure || handledBuildFailure;
                        }

                        if (buildFailure) { _ctx.ErrorOccured = true; break; }
                        if (rollBackOnError && currentRun.Success == false) break;
                    }

                    if (cancellationToken.IsCancellationRequested)
                    {
                        progress.ReportProgress(0, new ScriptRunStatusEventArgs("Build Cancelled", TimeSpan.Zero));
                        buildFailure = true;
                    }
                    else if (currentRun.Success == true)
                    {
                        var span = DateTime.Now - currentRun.RunStart!.Value;
                        tmpCommitted.RunStart = currentRun.RunStart ?? DateTime.MinValue;
                        tmpCommitted.RunEnd = DateTime.Now;
                        committedScripts.Add(tmpCommitted);
                        progress.ReportProgress(0, new ScriptRunStatusEventArgs($"Script Successful against {currentRun.Database}", span));
                    }

                    currentRun.RunEnd = DateTime.Now;
                    _ctx.AddScriptRunToHistory(currentRun, myBuild);
                    if (buildFailure) { _ctx.ErrorOccured = true; break; }
                    if (_ctx.RunScriptOnly)
                        progress.ReportProgress(0, new ScriptRunStatusEventArgs("Scripted", TimeSpan.Zero));
                }
            }
            catch (OperationCanceledException)
            {
                buildFailure = true;
                progress.ReportProgress(0, new ScriptRunStatusEventArgs("Build Cancelled", TimeSpan.Zero));
            }
            catch (Exception e)
            {
                log.LogError(e, "General build failure");
                _ctx.ErrorOccured = true;
                buildFailure = true;
                progress.ReportProgress(100, e);
            }
            finally
            {
                await _buildFinalizer.SaveBuildDataModelAsync(_ctx, false).ConfigureAwait(false);
                WriteFinalScriptLog(dbTargets, buildFailure, isTransactional: _ctx.IsTransactional, isTrialBuild: _ctx.IsTrialBuild);
                if (buildFailure)
                {
                    progress.ReportProgress(100, new ScriptRunStatusEventArgs("Build Failed", TimeSpan.Zero));
                    log.LogDebug("Build failed");
                }
            }

            (myBuild, buildDataModel, _) = await _buildFinalizer.PerformRunScriptFinalizationAsync(_ctx, _connectionsService, _finalizerContext, buildFailure, myBuild).ConfigureAwait(false);
            
            // If build failed due to a timeout, set the status to allow retry mechanism to work
            if (buildFailure && failureDueToScriptTimeout)
            {
                myBuild.FinalStatus = BuildItemStatus.FailedDueToScriptTimeout;
            }
            
            return myBuild;
        }

        internal bool ShouldSkipDueToCommittedScripts(string scriptId, BuildModels.SqlSyncBuildDataModel buildDataModel)
        {
            var csList = buildDataModel?.CommittedScript ?? new List<BuildModels.CommittedScript>();
            return csList.Any(cs => string.Equals(cs.ScriptId, scriptId, StringComparison.OrdinalIgnoreCase));
        }

        internal async Task<string[]> LoadBatchScriptsAsync(string scriptId, string fileName, bool stripTransaction, ScriptBatchCollection scriptBatchColl, CancellationToken cancellationToken)
        {
            string[] batchScripts = null;
            if (scriptBatchColl != null)
            {
                var b = scriptBatchColl.GetScriptBatch(scriptId);
                if (b != null)
                    batchScripts = b.ScriptBatchContents;
            }
            if (batchScripts == null || batchScripts.Length == 0)
            {
                batchScripts = await _ctx.ReadBatchFromScriptFileAsync(System.IO.Path.Combine(_ctx.ProjectFilePath, fileName), stripTransaction, false, cancellationToken).ConfigureAwait(false);
            }
            return batchScripts;
        }

        private void ValidateScriptsInput(IList<BuildModels.Script> scripts)
        {
            if (scripts == null || scripts.Count == 0)
            {
                _ctx.Log.LogError("No scripts selected for execution.");
                throw new ApplicationException("No scripts selected for execution.");
            }
        }

        private static BuildModels.ScriptRun BuildScriptRunFailure(string fileName, int runOrder, string targetDatabase, Guid scriptRunRowId, string buildId, string message)
        {
            return new BuildModels.ScriptRun(
                fileHash: null,
                results: "Database connection failed\r\n" + message,
                fileName: fileName,
                runOrder: runOrder,
                runStart: DateTime.Now,
                runEnd: DateTime.Now,
                success: false,
                database: targetDatabase,
                scriptRunId: scriptRunRowId.ToString(),
                buildId: buildId);
        }

        private void TryCreateSavePoint(BuildConnectData cData, string savePointName, string serverName, string targetDatabase, string fileName)
        {
            try
            {
                if (_ctx.IsTransactional)
                    _transactionManager.CreateSavePoint(cData.Transaction, savePointName);
            }
            catch (Exception mye)
            {
                _ctx.Log.LogWarning(mye, $"Error creating Transaction save point on {serverName}.{targetDatabase} for script {fileName}");
            }
        }

        private (bool buildFailure, bool timeoutDetected) HandleSqlException(SqlException e, string fileName, string batchScript, string targetDatabase, string savePointName, DateTime start, bool rollBackOnError, bool causesBuildFailure, BuildConnectData cData, ref BuildModels.ScriptRun currentRun)
        {
            var log = _ctx.Log;
            var progress = _ctx.ProgressReporter ?? new DefaultProgressReporter();
            var logMsg = new StringBuilder($"Script File: {fileName}{Environment.NewLine}");
            bool timeoutDetected = false;
            foreach (SqlError error in e.Errors)
            {
                logMsg.Append($"Line Number: {error.LineNumber}{Environment.NewLine}");
                logMsg.Append($"Error Message: {error.Message}{Environment.NewLine}");
                logMsg.Append($"Offending Script:{Environment.NewLine}{batchScript}");
                logMsg.Append("----------------");
                log.LogError($"Error running script in: {fileName}");
                log.LogError(error.Message);

                // Check for timeout error number (-2) or timeout message
                if (error.Number == -2 || error.Message.Trim().ToLower().Contains("timeout expired."))
                {
                    timeoutDetected = true;
                }
            }
            if (!timeoutDetected && e.Message.Trim().ToLower().Contains("timeout expired."))
            {
                log.LogWarning($"Encountered a Timeout exception for script: \"{batchScript}\"");
                timeoutDetected = true;
            }
            if (timeoutDetected)
            {
                log.LogWarning($"Encountered a Timeout exception for script: \"{batchScript}\"");
            }
            currentRun.Success = false;
            currentRun.Results = (currentRun.Results ?? string.Empty) + logMsg.ToString();
            log.LogDebug(e, logMsg.ToString());
            _ctx.PublishScriptLog(true, new ScriptLogEventArgs(0, batchScript, targetDatabase, fileName, logMsg.ToString() + _ctx.SqlInfoMessage));

            bool buildFailure = false;
            bool zombiedTransaction = false;
            if (rollBackOnError)
            {
                try
                {
                    var span = DateTime.Now - start;
                    if (_ctx.IsTransactional)
                    {
                        _transactionManager.RollbackToSavePoint(cData.Transaction, savePointName);
                        log.LogWarning($"Script Rolled Back for {fileName}");
                    }
                    else
                        log.LogError($"Script Error. No Rollback Available for {fileName}");
                }
                catch (SqlException sqle)
                {
                    logMsg.Clear();
                    foreach (SqlError err in sqle.Errors)
                        logMsg.Append(err.Message + "\r\n");
                    currentRun.Results = (currentRun.Results ?? string.Empty) + logMsg.ToString();
                    if (_ctx.IsTransactional)
                    {
                        _buildFinalizer.RollbackBuild(_connectionsService, _ctx.IsTransactional);
                        log.LogWarning($"Build Rolled Back");
                    }
                    else
                        log.LogError($"Error. No Rollback Available.");
                    buildFailure = true;
                }
                catch (InvalidOperationException invalExe)
                {
                    logMsg.Clear(); logMsg.Append(invalExe.Message + Environment.NewLine);
                    currentRun.Results = (currentRun.Results ?? string.Empty) + logMsg.ToString();
                    if (_ctx.IsTransactional && !_transactionManager.IsTransactionZombied(invalExe))
                    {
                        _buildFinalizer.RollbackBuild(_connectionsService, _ctx.IsTransactional);
                        log.LogWarning($"Build Rolled Back");
                    }
                    else
                    {
                        log.LogError($"Error. No Rollback Available.");
                        zombiedTransaction = true;
                    }
                    buildFailure = true;
                }
            }
            else
            {
                if (_ctx.IsTransactional)
                    log.LogError($"Error, Save Point Not Rolled Back");
                else
                    log.LogError($"Error. No Rollback Available.");
            }

            if (causesBuildFailure || buildFailure)
            {
                if (_ctx.IsTransactional && !zombiedTransaction)
                {
                    _buildFinalizer.RollbackBuild(_connectionsService, _ctx.IsTransactional);
                    log.LogWarning($"Build Rolled Back");
                }
                else
                    log.LogError("Error. No Rollback Available.");
                buildFailure = true;
            }

            return (buildFailure, timeoutDetected);
        }

        private void WriteFinalScriptLog(List<string> dbTargets, bool buildFailure, bool isTransactional, bool isTrialBuild)
        {
            var database = dbTargets.Count > 0 ? dbTargets.Distinct().Aggregate((a, b) => a + ", " + b) : "UNKNOWN";
            if (isTransactional)
            {
                if (!buildFailure)
                {
                    if (isTrialBuild)
                        _ctx.PublishScriptLog(false, new ScriptLogEventArgs(-10000, "ROLLBACK TRANSACTION", database, "-- Trial Run: Complete Transaction with a rollback--", "Scripted"));
                    else
                        _ctx.PublishScriptLog(false, new ScriptLogEventArgs(-10000, "COMMIT TRANSACTION", database, "-- Complete Transaction --", "Scripted"));
                }
                else
                    _ctx.PublishScriptLog(false, new ScriptLogEventArgs(-10000, "ROLLBACK TRANSACTION", database, "-- ERROR: Rollback Transaction --", "Scripted"));
            }
            else
            {
                if (!buildFailure)
                    _ctx.PublishScriptLog(false, new ScriptLogEventArgs(-10000, string.Empty, database, "-- Completed: No Transaction Set --", "Scripted"));
                else
                    _ctx.PublishScriptLog(false, new ScriptLogEventArgs(-10000, string.Empty, database, "-- ERROR: No Transaction Set --", "Scripted"));
            }
        }

        internal sealed class SqlCommandExecutor : ISqlCommandExecutor
        {
            private readonly ILogger _log;
            public SqlCommandExecutor(ILogger log) => _log = log;

            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional)
            {
                var cmd = cData.Connection.CreateCommand();
                cmd.CommandText = sql;
                if (isTransactional)
                    cmd.Transaction = cData.Transaction;
                cmd.CommandTimeout = timeoutSeconds;
                var sb = new StringBuilder();
                try
                {
                    using (var reader = cmd.ExecuteReader())
                    {
                        bool more = true;
                        while (more)
                        {
                            sb.Append("Records Affected: " + reader.RecordsAffected + "\r\n");
                            while (reader.Read())
                            {
                                for (int j = 0; j < reader.FieldCount; j++)
                                {
                                    sb.Append(reader[j]);
                                    if (j < reader.FieldCount - 1)
                                        sb.Append("|");
                                    else
                                        sb.Append("\r\n");
                                }
                            }
                            more = reader.NextResult();
                        }
                    }
                    return new SqlExecutionResult(true, sb.ToString(), TimeoutDetected: false);
                }
                catch (SqlException e) when (e.Message.Trim().ToLower().Contains("timeout expired."))
                {
                    // propagate; caller handles
                    _log.LogWarning($"Timeout expired executing sql: {sql}");
                    throw;
                }
            }

            public async Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
            {
                var cmd = cData.Connection.CreateCommand();
                cmd.CommandText = sql;
                if (isTransactional)
                    cmd.Transaction = cData.Transaction;
                cmd.CommandTimeout = timeoutSeconds;
                var sb = new StringBuilder();
                try
                {
                    using (var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false))
                    {
                        bool more = true;
                        while (more)
                        {
                            sb.Append("Records Affected: " + reader.RecordsAffected + "\r\n");
                            while (await reader.ReadAsync(cancellationToken).ConfigureAwait(false))
                            {
                                for (int j = 0; j < reader.FieldCount; j++)
                                {
                                    sb.Append(reader[j]);
                                    if (j < reader.FieldCount - 1)
                                        sb.Append("|");
                                    else
                                        sb.Append("\r\n");
                                }
                            }
                            more = await reader.NextResultAsync(cancellationToken).ConfigureAwait(false);
                        }
                    }
                    return new SqlExecutionResult(true, sb.ToString(), TimeoutDetected: false);
                }
                catch (SqlException e) when (e.Message.Trim().ToLower().Contains("timeout expired."))
                {
                    // propagate; caller handles
                    _log.LogWarning($"Timeout expired executing sql: {sql}");
                    throw;
                }
            }
        }
    }
}
