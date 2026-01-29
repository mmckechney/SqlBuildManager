using Microsoft.Build.Execution;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;

namespace SqlSync.SqlBuild.Services
{
    public sealed class DefaultBuildFinalizer : IBuildFinalizer
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
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
            foreach (string key in keys)
            {
                try
                {
                    log.LogInformation($"Committing transaction for {key}");
                    ((BuildConnectData)connectionsService.Connections[key]).Transaction.Commit();
                    log.LogInformation($"Commit Successful for {key}");
                }
                catch (Exception e)
                {
                    log.LogError(e, $"Error in CommitBuild Transaction.Commit() for database '{key}'");
                    progressReporter.ReportProgress(100, new CommitFailureEventArgs(e.Message));
                    success = false;
                }
                try
                {
                    log.LogDebug($"Closing connection for {key}");
                    ((BuildConnectData)connectionsService.Connections[key]).Connection.Close();
                }
                catch (Exception e)
                {
                    log.LogWarning(e, $"Error in CommitBuild Connection.Close() for database '{e}'");
                    progressReporter.ReportProgress(100, new CommitFailureEventArgs(e.Message));
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
            foreach (string key in keys)
            {
                try
                {
                    log.LogInformation($"Rolling back transaction for {key}");
                    ((BuildConnectData)connectionsService.Connections[key]).Transaction.Rollback();
                }
                catch (Exception e)
                {
                    log.LogError($"Error in RollbackBuild Transaction.Rollback() for database '{key}'. {e.Message}");
                }
                try
                {
                    log.LogDebug($"Closing connection for {key}");
                    ((BuildConnectData)connectionsService.Connections[key]).Connection.Close();
                }
                catch (Exception e)
                {
                    log.LogError($"Error in RollbackBuild Connection.Close() for database '{key}'. {e.Message}");
                }
            }

            return true;
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
                buildDataModel.CommittedScript = list;
                return buildDataModel;
            }

            return model;
        }

        public void SaveBuildDataModel(ISqlBuildRunnerProperties context, bool fireSavedEvent)
        {
            log.LogInformation("Saving Build File Updates");

            if (context.ProjectFileName == null || context.ProjectFileName.Length == 0)
            {
                string message = "The \"projectFileName\" field value is null or empty. Unable to save the DataSet.";
                log.LogError(message);
                throw new ArgumentException(message);
            }

            SqlBuildFileHelper.SaveSqlBuildProjectFile(context.BuildDataModel, context.ProjectFileName, context.BuildFileName, includeHistoryAndLogs: true);


            if (context.BuildHistoryXmlFile == null || context.BuildHistoryXmlFile.Length == 0)
            {
                string message = "The \"buildHistoryXmlFile\" field value is null or empty. Unable to save the build history DataSet.";
                log.LogError(message); 
                throw new ArgumentException(message);
            }

            SqlSyncBuildDataXmlSerializer.Save(context.BuildHistoryXmlFile, context.BuildDataModel);

            log.LogInformation("Build Data saved successfully.");
        }
        public (Build updatedBuild, SqlSyncBuildDataModel updatedModel, BuildResultStatus buildResult) PerformRunScriptFinalization(ISqlBuildRunnerProperties context, IConnectionsService connectionsService, IBuildFinalizerContext finalizerContext, bool buildFailure, Build myBuild)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var finalizationErrorOccurred = false;
            var updatedDataModel = context.BuildDataModel;
            BuildResultStatus finalBuildResult;
            DateTime end = DateTime.Now;
            myBuild = new Build(
                name: myBuild.Name,
                buildType: myBuild.BuildType,
                buildStart: myBuild.BuildStart,
                buildEnd: end,
                serverName: myBuild.ServerName,
                finalStatus: myBuild.FinalStatus,
                buildId: context.BuildPackageHash,
                userId: myBuild.UserId);

            if (buildFailure)
            {
                finalizationErrorOccurred = true;
                if (context.IsTransactional)
                    myBuild = new Build(
                        name: myBuild.Name,
                        buildType: myBuild.BuildType,
                        buildStart: myBuild.BuildStart,
                        buildEnd: myBuild.BuildEnd,
                        serverName: myBuild.ServerName,
                        finalStatus: BuildItemStatus.RolledBack,
                        buildId: myBuild.BuildId,
                        userId: myBuild.UserId);
                else
                    myBuild = new Build(
                        name: myBuild.Name,
                        buildType: myBuild.BuildType,
                        buildStart: myBuild.BuildStart,
                        buildEnd: myBuild.BuildEnd,
                        serverName: myBuild.ServerName,
                        finalStatus: BuildItemStatus.FailedNoTransaction,
                        buildId: myBuild.BuildId,
                        userId: myBuild.UserId);


                if (!context.IsTransactional)
                {
                    updatedDataModel = RecordCommittedScripts(context.CommittedScripts, updatedDataModel);
                    sqlLoggingService.LogCommittedScriptsToDatabase(context.CommittedScripts, context, context.MultiDbRunData);
                }
            }
            else
            {
                if (!context.IsTrialBuild)
                {
                    if (context.IsTransactional)
                    {
                        log.LogInformation("Attempting to Commit Build");
                        bool commitSuccess = CommitBuild(connectionsService, context.IsTransactional);
                        if (commitSuccess)
                            log.LogInformation("Commit Successful");
                    }

                    myBuild = new Build(
                        name: myBuild.Name,
                        buildType: myBuild.BuildType,
                        buildStart: myBuild.BuildStart,
                        buildEnd: myBuild.BuildEnd,
                        serverName: myBuild.ServerName,
                        finalStatus: BuildItemStatus.Committed,
                        buildId: myBuild.BuildId,
                        userId: myBuild.UserId);

                    updatedDataModel = RecordCommittedScripts(context.CommittedScripts, updatedDataModel);
                    sqlLoggingService.LogCommittedScriptsToDatabase(context.CommittedScripts, context, context.MultiDbRunData);
                    //TODO: figure out eventing here
                    finalizerContext.RaiseBuildCommittedEvent(context, RunnerReturn.BuildCommitted);
                }
                else
                {
                    if (context.IsTransactional)
                    {
                        RollbackBuild(connectionsService, context.IsTransactional);
                        myBuild = new Build(
                            name: myBuild.Name,
                            buildType: myBuild.BuildType,
                            buildStart: myBuild.BuildStart,
                            buildEnd: myBuild.BuildEnd,
                            serverName: myBuild.ServerName,
                            finalStatus: BuildItemStatus.TrialRolledBack,
                            buildId: myBuild.BuildId,
                            userId: myBuild.UserId);
                        finalizerContext.RaiseBuildSuccessTrialRolledBackEvent(context);
                    }
                    else
                    {
                        myBuild = new Build(
                            name: myBuild.Name,
                            buildType: myBuild.BuildType,
                            buildStart: myBuild.BuildStart,
                            buildEnd: myBuild.BuildEnd,
                            serverName: myBuild.ServerName,
                            finalStatus: BuildItemStatus.Committed,
                            buildId: myBuild.BuildId,
                            userId: myBuild.UserId);
                    }
                }
            }
            

            if (buildFailure)
            {
                finalizerContext.RaiseBuildErrorRollBackEvent(context);
            }

            SaveBuildDataModel(context, true);

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
    }
}
