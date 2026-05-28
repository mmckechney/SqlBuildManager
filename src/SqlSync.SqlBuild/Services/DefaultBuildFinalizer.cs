using Microsoft.Build.Execution;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;

namespace SqlSync.SqlBuild.Services
{
    public sealed class DefaultBuildFinalizer : IBuildFinalizer
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);
        private readonly IProgressReporter progressReporter;
        private readonly ISqlLoggingService sqlLoggingService;

        public DefaultBuildFinalizer(ISqlLoggingService sqlLoggingService, IProgressReporter progressReporter)
        {
            this.sqlLoggingService = sqlLoggingService;
            this.progressReporter = progressReporter;
        }

        public bool CommitBuild(IConnectionsService connectionsService, bool isTransactional)
        {
            //If we're not in a transaction, they've already committed.
            if (!isTransactional)
                return true;

            Dictionary<string, BuildConnectData>.KeyCollection keys = connectionsService.Connections.Keys;
            bool success = true;
            bool continueCommitting = true;
            foreach (string key in keys)
            {
                var connData = (BuildConnectData)connectionsService.Connections[key];
                if (continueCommitting)
                {
                    try
                    {
                        log.LogInformation($"Committing transaction for {key}");
                        connData.Transaction?.Commit();
                        connData.Transaction?.Dispose();
                        connData.Transaction = null!;
                        log.LogInformation($"Commit Successful for {key}");
                    }
                    catch (Exception e)
                    {
                        log.LogError(e, $"Error in CommitBuild Transaction.Commit() for database '{key}'");
                        progressReporter.ReportProgress(100, new CommitFailureEventArgs(e.Message));
                        success = false;
                        continueCommitting = false;
                        TryRollbackTransaction(connData, key);
                    }
                }
                else
                {
                    success = !TryRollbackTransaction(connData, key) ? false : success;
                }

                if (!TryCloseConnection(connData, key, reportCommitFailure: true))
                {
                    success = false;
                }
            }

            return success;
        }

        public bool RollbackBuild(IConnectionsService connectionsService, bool isTransactional)
        {
            //If we're not in a transaction, we can't roll back...
            if (!isTransactional)
            {
                log.LogWarning("Build is non-transactional. Unable to rollback");
                return false;
            }

            Dictionary<string, BuildConnectData>.KeyCollection keys = connectionsService.Connections.Keys;
            bool success = true;
            foreach (string key in keys)
            {
                var connData = (BuildConnectData)connectionsService.Connections[key];
                if (!TryRollbackTransaction(connData, key))
                {
                    success = false;
                }
                if (!TryCloseConnection(connData, key, reportCommitFailure: false))
                {
                    success = false;
                }
            }

            return success;
        }

        private bool TryRollbackTransaction(BuildConnectData connData, string key)
        {
            if (connData.Transaction == null)
            {
                return true;
            }

            try
            {
                log.LogInformation($"Rolling back transaction for {key}");
                connData.Transaction.Rollback();
                connData.Transaction.Dispose();
                connData.Transaction = null!;
                return true;
            }
            catch (Exception e)
            {
                log.LogError(e, $"Error rolling back transaction for database '{key}'");
                return false;
            }
        }

        private bool TryCloseConnection(BuildConnectData connData, string key, bool reportCommitFailure)
        {
            try
            {
                log.LogDebug($"Closing connection for {key}");
                connData.Connection?.Close();
                return true;
            }
            catch (Exception e)
            {
                if (reportCommitFailure)
                {
                    log.LogWarning(e, $"Error in CommitBuild Connection.Close() for database '{key}'");
                    progressReporter.ReportProgress(100, new CommitFailureEventArgs(e.Message));
                }
                else
                {
                    log.LogError(e, $"Error in RollbackBuild Connection.Close() for database '{key}'");
                }
                return false;
            }
        }

        public SqlSyncBuildDataModel RecordCommittedScripts(List<LoggingCommittedScript> committedScripts, SqlSyncBuildDataModel buildDataModel)
        {
            var model = buildDataModel ?? SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Fast POCO path
            if (committedScripts != null)
            {
                var list = new List<SqlSync.SqlBuild.Models.CommittedScript>(model.CommittedScript);
                var projectId = model.SqlSyncBuildProject.Count > 0 ? model.SqlSyncBuildProject[0].SqlSyncBuildProjectId : 0;
                foreach (var cs in committedScripts)
                {
                    list.Add(new SqlSync.SqlBuild.Models.CommittedScript(
                        cs.ScriptId.ToString(),
                        cs.ServerName,
                        DateTime.Now,
                        true,
                        cs.FileHash,
                        projectId));
                }
                buildDataModel!.CommittedScript = list;
                return buildDataModel;
            }

            return model;
        }

        public async Task SaveBuildDataModelAsync(ISqlBuildRunnerProperties context, bool fireSavedEvent)
        {
            log.LogInformation("Saving Build File Updates");

            if (context.ProjectFileName == null || context.ProjectFileName.Length == 0)
            {
                string message = "The \"projectFileName\" field value is null or empty. Unable to save the DataSet.";
                log.LogError(message);
                throw new ArgumentException(message);
            }

            var modelToSave = MergeBuildHistory(context.BuildDataModel, context.BuildHistoryModel);
            context.BuildDataModel = modelToSave;

            await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(modelToSave, context.ProjectFileName, context.BuildFileName, includeHistoryAndLogs: true).ConfigureAwait(false);


            if (context.BuildHistoryXmlFile == null || context.BuildHistoryXmlFile.Length == 0)
            {
                string message = "The \"buildHistoryXmlFile\" field value is null or empty. Unable to save the build history DataSet.";
                log.LogError(message); 
                throw new ArgumentException(message);
            }

            await SqlSyncBuildDataXmlSerializer.SaveAsync(context.BuildHistoryXmlFile, modelToSave).ConfigureAwait(false);

            log.LogInformation("Build Data saved successfully.");
        }

        private static SqlSyncBuildDataModel MergeBuildHistory(SqlSyncBuildDataModel buildDataModel, SqlSyncBuildDataModel buildHistoryModel)
        {
            var model = buildDataModel ?? SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            if (buildHistoryModel == null)
            {
                return model;
            }

            var builds = new List<Build>(model.Build ?? new List<Build>());
            foreach (var build in buildHistoryModel.Build ?? new List<Build>())
            {
                if (!builds.Any(existing => SameBuild(existing, build)))
                {
                    builds.Add(build);
                }
            }

            var scriptRuns = new List<ScriptRun>(model.ScriptRun ?? new List<ScriptRun>());
            foreach (var scriptRun in buildHistoryModel.ScriptRun ?? new List<ScriptRun>())
            {
                if (!scriptRuns.Any(existing => SameScriptRun(existing, scriptRun)))
                {
                    scriptRuns.Add(scriptRun);
                }
            }

            return new SqlSyncBuildDataModel(
                sqlSyncBuildProject: model.SqlSyncBuildProject,
                script: model.Script,
                build: builds,
                scriptRun: scriptRuns,
                committedScript: model.CommittedScript);
        }

        private static bool SameBuild(Build left, Build right)
        {
            if (!string.IsNullOrWhiteSpace(left.BuildId) && !string.IsNullOrWhiteSpace(right.BuildId))
            {
                return string.Equals(left.BuildId, right.BuildId, StringComparison.OrdinalIgnoreCase);
            }

            return ReferenceEquals(left, right);
        }

        private static bool SameScriptRun(ScriptRun left, ScriptRun right)
        {
            if (!string.IsNullOrWhiteSpace(left.ScriptRunId) && !string.IsNullOrWhiteSpace(right.ScriptRunId))
            {
                return string.Equals(left.ScriptRunId, right.ScriptRunId, StringComparison.OrdinalIgnoreCase);
            }

            return string.Equals(left.BuildId, right.BuildId, StringComparison.OrdinalIgnoreCase)
                && string.Equals(left.FileName, right.FileName, StringComparison.OrdinalIgnoreCase)
                && left.RunOrder == right.RunOrder;
        }
        public async Task<(Build updatedBuild, SqlSyncBuildDataModel updatedModel, BuildResultStatus buildResult)> PerformRunScriptFinalizationAsync(ISqlBuildRunnerProperties context, IConnectionsService connectionsService, IBuildFinalizerContext finalizerContext, bool buildFailure, Build myBuild)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var updatedDataModel = context.BuildDataModel;
            BuildResultStatus finalBuildResult;
            DateTime end = DateTime.Now;
            myBuild.BuildId = context.BuildPackageHash;
            myBuild.BuildEnd = end;
       

            if (buildFailure)
            {
                if (context.IsTransactional)
                {
                    var rollbackSuccess = RollbackBuild(connectionsService, context.IsTransactional);
                    myBuild.FinalStatus = BuildItemStatus.RolledBack;
                    if (!rollbackSuccess)
                    {
                        myBuild.FinalStatus = BuildItemStatus.PendingRollBack;
                    }
                }
                else
                    myBuild.FinalStatus = BuildItemStatus.FailedNoTransaction;
               


                if (!context.IsTransactional)
                {
                    updatedDataModel = RecordCommittedScripts(context.CommittedScripts, updatedDataModel);
                    await sqlLoggingService.LogCommittedScriptsToDatabase(context.CommittedScripts, context, context.MultiDbRunData).ConfigureAwait(false);
                }
            }
            else
            {
                if (!context.IsTrialBuild)
                {
                    bool commitSuccess = true;
                    if (context.IsTransactional)
                    {
                        log.LogInformation("Attempting to Commit Build");
                        commitSuccess = CommitBuild(connectionsService, context.IsTransactional);
                        //    if (commitSuccess)
                        //        log.LogInformation("Commit Successful");  -- Not needed since each connection will show a commit or not
                    }
                    if (commitSuccess)
                    {
                        myBuild.FinalStatus = BuildItemStatus.Committed;
                        updatedDataModel = RecordCommittedScripts(context.CommittedScripts, updatedDataModel);
                        await sqlLoggingService.LogCommittedScriptsToDatabase(context.CommittedScripts, context, context.MultiDbRunData).ConfigureAwait(false);
                        finalizerContext.RaiseBuildCommittedEvent(context, RunnerReturn.BuildCommitted);
                    }
                    else
                    {
                        myBuild.FinalStatus = BuildItemStatus.PendingRollBack;
                        //updatedDataModel = RecordCommittedScripts(context.CommittedScripts, updatedDataModel);
                        //await sqlLoggingService.LogCommittedScriptsToDatabase(context.CommittedScripts, context, context.MultiDbRunData).ConfigureAwait(false);
                        finalizerContext.RaiseBuildErrorRollBackEvent(context);
                    }
                }
                else
                {
                    if (context.IsTransactional)
                    {
                       
                        RollbackBuild(connectionsService, context.IsTransactional);
                        myBuild.FinalStatus = BuildItemStatus.TrialRolledBack;
                        finalizerContext.RaiseBuildSuccessTrialRolledBackEvent(context);
                    }
                    else
                    {
                        myBuild.FinalStatus = BuildItemStatus.Committed;
                    }
                }
            }
            

            if (buildFailure)
            {
                finalizerContext.RaiseBuildErrorRollBackEvent(context);
            }

            await SaveBuildDataModelAsync(context, true);

            if (buildFailure)
            {

                if (context.IsTransactional)
                {
                    log.LogInformation("Build Failed and Rolled Back");
                    finalBuildResult = SqlSync.SqlBuild.BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK;
                }
                else
                {
                    log.LogInformation("Build Failed. No Transaction Set.");
                    finalBuildResult = SqlSync.SqlBuild.BuildResultStatus.BUILD_FAILED_NO_TRANSACTION;
                }

            }
            else
            {
                if (context.RunScriptOnly)
                {
                    log.LogInformation("Script Generation Complete");
                    finalBuildResult = SqlSync.SqlBuild.BuildResultStatus.SCRIPT_GENERATION_COMPLETE;
                }
                else if (myBuild.FinalStatus == BuildItemStatus.RolledBack || myBuild.FinalStatus == BuildItemStatus.PendingRollBack)
                {
                    log.LogWarning($"Build was not committed. Final status is {myBuild.FinalStatus}");
                    finalBuildResult = ConvertBuildItemStatusToResultStatus(myBuild.FinalStatus, context.IsTransactional, context.IsTrialBuild);
                }
                else
                {
                    if (context.IsTrialBuild == false)
                    {
                        log.LogInformation("Build Committed");
                        finalBuildResult = SqlSync.SqlBuild.BuildResultStatus.BUILD_COMMITTED;
                    }
                    else
                    {
                        if (context.IsTransactional)
                        {
                            log.LogInformation("Build Successful. Rolled back for Trial Build");
                            finalBuildResult = SqlSync.SqlBuild.BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL;
                        }
                        else
                        {
                            log.LogInformation("Build Successful. Committed with no transaction");
                            finalBuildResult = SqlSync.SqlBuild.BuildResultStatus.BUILD_COMMITTED;

                        }
                    }
                }
            }
            connectionsService.Connections.Clear();
            return (myBuild, updatedDataModel, finalBuildResult);
        }

        public BuildResultStatus CalculateFinalStatus(IList<BuildResultStatus> buildResults)
        {

            // If all of the results are the same, return that status
            if (buildResults.Count > 0 && buildResults.All(result => result == buildResults[0]))
            {
                return buildResults[0];
            }
              
            // If any build result is failed, the overall status is failed
            if (buildResults.Any(result => result == BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK))
            {
                return BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK;
            }

            if (buildResults.Any(result => result == BuildResultStatus.BUILD_FAILED_NO_TRANSACTION))
            {
                return BuildResultStatus.BUILD_FAILED_NO_TRANSACTION;
            }

            if (buildResults.All(result => result == BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL))
            {
                return BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL;
            }

            // If all builds are successful, the overall status is successful
            if (buildResults.All(result => result == BuildResultStatus.BUILD_COMMITTED))
            {
                return BuildResultStatus.BUILD_COMMITTED;
            }

            return BuildResultStatus.UNKNOWN;
        }

        public BuildResultStatus ConvertBuildItemStatusToResultStatus(BuildItemStatus? itemStatus, bool isTransactional, bool isTrialBuild)
        {
            return itemStatus switch
            {
                BuildItemStatus.Committed or BuildItemStatus.CommittedWithTimeoutRetries or BuildItemStatus.CommittedWithCustomDacpac => 
                    BuildResultStatus.BUILD_COMMITTED,
                BuildItemStatus.RolledBack or BuildItemStatus.RolledBackAfterRetries => 
                    BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK,
                BuildItemStatus.TrialRolledBack => 
                    BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL,
                BuildItemStatus.FailedNoTransaction => 
                    BuildResultStatus.BUILD_FAILED_NO_TRANSACTION,
                BuildItemStatus.PendingRollBack => 
                    isTransactional ? BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK : BuildResultStatus.BUILD_FAILED_NO_TRANSACTION,
                BuildItemStatus.AlreadyInSync => 
                    BuildResultStatus.BUILD_COMMITTED,
                _ => BuildResultStatus.UNKNOWN
            };
        }
    }
}
