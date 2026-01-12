using System;
using System.Collections.Generic;
using System.ComponentModel;
using Microsoft.Data.SqlClient;
using System.Linq;
using System.Text;
using Microsoft.Extensions.Logging;
using static SqlSync.SqlBuild.SqlBuildHelper;
using BuildModels = SqlSync.SqlBuild.Models;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;

namespace SqlSync.SqlBuild
{
    internal interface ISqlBuildRunnerContext
    {
        ILogger Log { get; }
        BackgroundWorker BgWorker { get; }
        bool IsTransactional { get; }
        bool IsTrialBuild { get; }
        bool RunScriptOnly { get; }
        string BuildPackageHash { get; }
        string ProjectFilePath { get; }
        List<LoggingCommittedScript> CommittedScripts { get; }
        bool ErrorOccured { get; set; }
        string SqlInfoMessage { get; set; }
        int DefaultScriptTimeout { get; }

        BuildConnectData GetConnectionDataClass(string serverName, string databaseName);
        string GetTargetDatabase(string defaultDatabase);
        string[] ReadBatchFromScriptFile(string path, bool stripTransaction, bool useRegex);
        string PerformScriptTokenReplacement(string script);
        void AddScriptRunToHistory(BuildModels.ScriptRun run, BuildModels.Build myBuild);
        void RollbackBuild();
        void SaveBuildDataSet(bool fireSavedEvent);
        BuildModels.Build PerformRunScriptFinalization(bool buildFailure, BuildModels.Build myBuild, BuildModels.SqlSyncBuildDataModel buildDataModel, ref DoWorkEventArgs workEventArgs);
        void PublishScriptLog(bool isError, ScriptLogEventArgs args);
    }

    internal interface ISqlCommandExecutor
    {
        SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional);
    }

    internal sealed record SqlExecutionResult(bool Success, string Results, bool TimeoutDetected = false);

    /// <summary>
    /// Extracted runner responsible for executing build scripts. All stateful dependencies are provided via context.
    /// </summary>
    internal sealed class SqlBuildRunner
    {
        private readonly ISqlBuildRunnerContext _ctx;
        private readonly ISqlCommandExecutor _executor;

        public SqlBuildRunner(ISqlBuildRunnerContext ctx, ISqlCommandExecutor executor = null)
        {
            _ctx = ctx ?? throw new ArgumentNullException(nameof(ctx));
            _executor = executor ?? new SqlCommandExecutor(ctx.Log);
        }

        internal BuildModels.Build Run(
            IReadOnlyList<BuildModels.Script> scripts,
            BuildModels.Build myBuild,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            BuildModels.SqlSyncBuildDataModel buildDataModel,
            ref DoWorkEventArgs workEventArgs)
        {
            var bgWorker = _ctx.BgWorker;
            var log = _ctx.Log;
            var committedScripts = _ctx.CommittedScripts;

            bgWorker.ReportProgress(0, new GeneralStatusEventArgs("Proceeding with Build"));
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

                    var batchScripts = LoadBatchScripts(scriptId, fileName, stripTransaction, scriptBatchColl);

                    if (!allowMultipleRuns && ShouldSkipDueToCommittedScripts(scriptId, buildDataModel))
                    {
                        log.LogInformation($"Skipping pre-run script {fileName}");
                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Skipped Pre-Run script", TimeSpan.Zero));
                        continue;
                    }

                    BuildConnectData cData;
                    var scriptRunRowId = Guid.NewGuid();
                    try
                    {
                        cData = _ctx.GetConnectionDataClass(serverName, targetDatabase);
                    }
                    catch (Exception e)
                    {
                        log.LogError(e, $"Database connection to {serverName}.{targetDatabase} failed");
                        var currentRunFail = BuildScriptRunFailure(fileName, i + 1, targetDatabase, scriptRunRowId, myBuild.Build_Id, e.Message);
                        _ctx.AddScriptRunToHistory(currentRunFail, myBuild);
                        _ctx.PublishScriptLog(true, new ScriptLogEventArgs(overallIndex, "Database connection failed", targetDatabase, fileName, e.Message + "\r\n" + _ctx.SqlInfoMessage));
                        _ctx.RollbackBuild();
                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Build Rolled Back", TimeSpan.Zero));
                        buildFailure = true;
                        break;
                    }

                    var currentRun = new BuildModels.ScriptRun(
                        FileHash: null,
                        Results: string.Empty,
                        FileName: fileName,
                        RunOrder: i + 1,
                        RunStart: DateTime.Now,
                        RunEnd: null,
                        Success: null,
                        Database: targetDatabase,
                        ScriptRunId: scriptRunRowId.ToString(),
                        Build_Id: myBuild.Build_Id);

                    // hash & commit staging
                    SqlBuildFileHelper.GetSHA1Hash(batchScripts, out var textHash);
                    currentRun = currentRun with { FileHash = textHash };
                    var scriptText = SqlBuildFileHelper.JoinBatchedScripts(batchScripts);
                    var tmpCommitted = new LoggingCommittedScript(new Guid(scriptId), currentRun.FileHash, runSequence++, scriptText, script.Tag, cData.ServerName, cData.DatabaseName);

                    var savePointName = scriptId.Replace("-", "");
                    TryCreateSavePoint(cData, savePointName, serverName, targetDatabase, fileName);

                    var start = DateTime.Now;
                    for (int x = 0; x < batchScripts.Length; x++)
                    {
                        if (bgWorker.CancellationPending)
                        {
                            log.LogInformation("Encountered cancellation pending directive. Breaking out of build");
                            workEventArgs.Cancel = true;
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

                            var execResult = _executor.Execute(batchScripts[x], scriptTimeout, cData, _ctx.IsTransactional);
                            failureDueToScriptTimeout = failureDueToScriptTimeout || execResult.TimeoutDetected;
                            currentRun = currentRun with { Success = true, Results = (currentRun.Results ?? string.Empty) + execResult.Results };
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

                    if (workEventArgs.Cancel)
                    {
                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Build Cancelled", TimeSpan.Zero));
                        buildFailure = true;
                    }
                    else if (currentRun.Success == true)
                    {
                        var span = DateTime.Now - currentRun.RunStart!.Value;
                        tmpCommitted.RunStart = currentRun.RunStart ?? DateTime.MinValue;
                        tmpCommitted.RunEnd = DateTime.Now;
                        committedScripts.Add(tmpCommitted);
                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs($"Script Successful against {currentRun.Database}", span));
                    }

                    currentRun = currentRun with { RunEnd = DateTime.Now };
                    _ctx.AddScriptRunToHistory(currentRun, myBuild);
                    if (buildFailure) { _ctx.ErrorOccured = true; break; }
                    if (_ctx.RunScriptOnly)
                        bgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Scripted", TimeSpan.Zero));
                }
            }
            catch (Exception e)
            {
                log.LogError(e, "General build failure");
                _ctx.ErrorOccured = true;
                buildFailure = true;
                bgWorker.ReportProgress(100, e);
            }
            finally
            {
                _ctx.SaveBuildDataSet(false);
                WriteFinalScriptLog(dbTargets, buildFailure, isTransactional: _ctx.IsTransactional, isTrialBuild: _ctx.IsTrialBuild);
            }

            // finalize
            if (buildFailure)
            {
                log.LogError("Build failure. Check execution logs for details");
                if (!isMultiDbRun)
                {
                    myBuild = _ctx.PerformRunScriptFinalization(buildFailure, myBuild, buildDataModel, ref workEventArgs);
                }
                else
                {
                    myBuild = myBuild with { FinalStatus = _ctx.IsTransactional ? BuildItemStatus.PendingRollBack.ToString() : BuildItemStatus.FailedNoTransaction.ToString() };
                }
                if (_ctx.IsTransactional && failureDueToScriptTimeout)
                {
                    myBuild = myBuild with { FinalStatus = BuildItemStatus.FailedDueToScriptTimeout.ToString() };
                }
            }
            else
            {
                if (isMultiDbRun)
                    myBuild = myBuild with { FinalStatus = BuildItemStatus.Pending.ToString() };
                else
                    myBuild = _ctx.PerformRunScriptFinalization(buildFailure, myBuild, buildDataModel, ref workEventArgs);
                log.LogDebug("Build Successful!");
            }
            return myBuild;
        }

        internal bool ShouldSkipDueToCommittedScripts(string scriptId, BuildModels.SqlSyncBuildDataModel buildDataModel)
        {
            var csList = buildDataModel?.CommittedScript ?? Array.Empty<BuildModels.CommittedScript>();
            return csList.Any(cs => string.Equals(cs.ScriptId, scriptId, StringComparison.OrdinalIgnoreCase));
        }

        internal string[] LoadBatchScripts(string scriptId, string fileName, bool stripTransaction, ScriptBatchCollection scriptBatchColl)
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
                batchScripts = _ctx.ReadBatchFromScriptFile(System.IO.Path.Combine(_ctx.ProjectFilePath, fileName), stripTransaction, false);
            }
            return batchScripts;
        }

        private void ValidateScriptsInput(IReadOnlyList<BuildModels.Script> scripts)
        {
            if (scripts == null || scripts.Count == 0)
            {
                _ctx.Log.LogError("No scripts selected for execution.");
                throw new ApplicationException("No scripts selected for execution.");
            }
        }

        private static BuildModels.ScriptRun BuildScriptRunFailure(string fileName, int runOrder, string targetDatabase, Guid scriptRunRowId, int buildId, string message)
        {
            return new BuildModels.ScriptRun(
                FileHash: null,
                Results: "Database connection failed\r\n" + message,
                FileName: fileName,
                RunOrder: runOrder,
                RunStart: DateTime.Now,
                RunEnd: DateTime.Now,
                Success: false,
                Database: targetDatabase,
                ScriptRunId: scriptRunRowId.ToString(),
                Build_Id: buildId);
        }

        private void TryCreateSavePoint(BuildConnectData cData, string savePointName, string serverName, string targetDatabase, string fileName)
        {
            try
            {
                if (_ctx.IsTransactional)
                    cData.Transaction.Save(savePointName);
            }
            catch (Exception mye)
            {
                _ctx.Log.LogWarning(mye, $"Error creating Transaction save point on {serverName}.{targetDatabase} for script {fileName}");
            }
        }

        private (bool buildFailure, bool timeoutDetected) HandleSqlException(SqlException e, string fileName, string batchScript, string targetDatabase, string savePointName, DateTime start, bool rollBackOnError, bool causesBuildFailure, BuildConnectData cData, ref BuildModels.ScriptRun currentRun)
        {
            var log = _ctx.Log;
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
            }
            if (e.Message.Trim().ToLower().Contains("timeout expired."))
            {
                log.LogWarning($"Encountered a Timeout exception for script: \"{batchScript}\"");
                timeoutDetected = true;
            }
            currentRun = currentRun with { Success = false, Results = (currentRun.Results ?? string.Empty) + logMsg.ToString() };
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
                        cData.Transaction.Rollback(savePointName);
                        _ctx.BgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Script Rolled Back", span));
                    }
                    else
                        _ctx.BgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Script Error. No Rollback Available.", span));
                }
                catch (SqlException sqle)
                {
                    logMsg.Clear();
                    foreach (SqlError err in sqle.Errors)
                        logMsg.Append(err.Message + "\r\n");
                    currentRun = currentRun with { Results = (currentRun.Results ?? string.Empty) + logMsg.ToString() };
                    if (_ctx.IsTransactional)
                    {
                        _ctx.RollbackBuild();
                        _ctx.BgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Build Rolled Back", TimeSpan.Zero));
                    }
                    else
                        _ctx.BgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Error. No Rollback Available.", TimeSpan.Zero));
                    buildFailure = true;
                }
                catch (InvalidOperationException invalExe)
                {
                    logMsg.Clear(); logMsg.Append(invalExe.Message + Environment.NewLine);
                    currentRun = currentRun with { Results = (currentRun.Results ?? string.Empty) + logMsg.ToString() };
                    if (_ctx.IsTransactional && !invalExe.Message.Contains("no longer usable"))
                    {
                        _ctx.RollbackBuild();
                        _ctx.BgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Build Rolled Back", TimeSpan.Zero));
                    }
                    else
                    {
                        _ctx.BgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Error. No Rollback Available.", TimeSpan.Zero));
                        zombiedTransaction = true;
                    }
                    buildFailure = true;
                }
            }
            else
            {
                if (_ctx.IsTransactional)
                    _ctx.BgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Error, Save Point Not Rolled Back", TimeSpan.Zero));
                else
                    _ctx.BgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Error. No Rollback Available.", TimeSpan.Zero));
            }

            if (causesBuildFailure || buildFailure)
            {
                if (_ctx.IsTransactional && !zombiedTransaction)
                {
                    _ctx.RollbackBuild();
                    _ctx.BgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Build Rolled Back", TimeSpan.Zero));
                }
                else
                    _ctx.BgWorker.ReportProgress(0, new ScriptRunStatusEventArgs("Error. No Rollback Available.", TimeSpan.Zero));
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
                var cmd = isTransactional ? new SqlCommand(sql, cData.Connection, cData.Transaction) : new SqlCommand(sql, cData.Connection);
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
        }
    }
}
