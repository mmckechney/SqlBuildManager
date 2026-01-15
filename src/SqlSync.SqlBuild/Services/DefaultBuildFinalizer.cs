using Microsoft.Extensions.Logging;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
                var projectId = model.SqlSyncBuildProject.Count > 0 ? model.SqlSyncBuildProject[0].SqlSyncBuildProject_Id : 0;
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
                var updatedModel = model with { CommittedScript = list };
                return updatedModel;
            }

            return model;
        }

        public void SaveBuildDataModel(ISqlBuildRunnerProperties context, bool fireSavedEvent)
        {
            progressReporter.ReportProgress(0, new GeneralStatusEventArgs("Saving Build File Updates"));

            if (context.ProjectFileName == null || context.ProjectFileName.Length == 0)
            {
                string message = "The \"projectFileName\" field value is null or empty. Unable to save the DataSet.";
                progressReporter.ReportProgress(0, new GeneralStatusEventArgs(message));
                throw new ArgumentException(message);
            }

            SqlBuildFileHelper.SaveSqlBuildProjectFile(context.BuildDataModel, context.ProjectFileName, context.BuildFileName, includeHistoryAndLogs: true);


            if (context.BuildHistoryXmlFile == null || context.BuildHistoryXmlFile.Length == 0)
            {
                string message = "The \"buildHistoryXmlFile\" field value is null or empty. Unable to save the build history DataSet.";
                progressReporter.ReportProgress(0, new GeneralStatusEventArgs(message));
                throw new ArgumentException(message);
            }

            SqlSyncBuildDataXmlSerializer.Save(context.BuildHistoryXmlFile, context.BuildDataModel);

            if (fireSavedEvent)
                progressReporter.ReportProgress(0, new ScriptRunProjectFileSavedEventArgs(true));
        }
        public (List<Build> buildRecords, SqlSyncBuildDataModel updatedModel, bool errorOccurred) PerformRunScriptFinalization(ISqlBuildRunnerProperties context, IConnectionsService connectionsService, bool buildFailure, List<Build> myBuild, SqlSyncBuildDataModel buildDataModel,  ref DoWorkEventArgs workEventArgs)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            var finalizationErrorOccurred = false;
            var updatedDataModel = buildDataModel;
            DateTime end = DateTime.Now;
            for(int i=0;i<myBuild.Count; i++)
            {
                if (myBuild[i].BuildId == context.BuildPackageHash)
                {
                    var buildToUpdate = myBuild[i];
                    buildToUpdate = buildToUpdate with { BuildEnd = end };
                    myBuild[i] = buildToUpdate;
                }
            }

            if (buildFailure)
            {
                finalizationErrorOccurred = true;
                for (int i = 0; i < myBuild.Count; i++)
                {
                    var buildToUpdate = myBuild[i];
                    if (context.IsTransactional)
                        buildToUpdate = buildToUpdate with { FinalStatus = BuildItemStatus.RolledBack.ToString() };
                    else
                        buildToUpdate = buildToUpdate with { FinalStatus = BuildItemStatus.FailedNoTransaction.ToString() };
                    myBuild[i] = buildToUpdate;
                }

                if (!context.IsTransactional)
                {
                    updatedDataModel = RecordCommittedScripts(context.CommittedScripts, buildDataModel);
                    sqlLoggingService.LogCommittedScriptsToDatabase(context.CommittedScripts, context,context.MultiDbRunData);
                }
            }
            else
            {
                if (!context.IsTrialBuild)
                {
                    if (context.IsTransactional)
                    {
                        progressReporter.ReportProgress(0, new GeneralStatusEventArgs("Attempting to Commit Build"));
                        bool commitSuccess = CommitBuild(connectionsService, context.IsTransactional);
                        if (commitSuccess)
                            progressReporter.ReportProgress(0, new GeneralStatusEventArgs("Commit Successful"));
                    }

                    for (int i = 0; i < myBuild.Count; i++)
                    {
                        var buildToUpdate = myBuild[i];
                        buildToUpdate = buildToUpdate with { FinalStatus = BuildItemStatus.Committed.ToString() };
                        myBuild[i] = buildToUpdate;
                    }
                       
                    updatedDataModel = RecordCommittedScripts(context.CommittedScripts, buildDataModel);
                    sqlLoggingService.LogCommittedScriptsToDatabase(context.CommittedScripts, context, context.MultiDbRunData);
                    context.RaiseBuildCommittedEvent(context, RunnerReturn.BuildCommitted);
                }
                else
                {
                    if (context.IsTransactional)
                    {
                        RollbackBuild(connectionsService, context.IsTransactional);

                        for (int i = 0; i < myBuild.Count; i++)
                        {
                            var buildToUpdate = myBuild[i];
                            buildToUpdate = buildToUpdate with { FinalStatus = BuildItemStatus.TrialRolledBack.ToString() };
                            myBuild[i] = buildToUpdate;
                        }
                        context.RaiseBuildSuccessTrialRolledBackEvent(context);
                    }
                    else
                    {
                        for (int i = 0; i < myBuild.Count; i++)
                        {
                            var buildToUpdate = myBuild[i];
                            buildToUpdate = buildToUpdate with { FinalStatus = BuildItemStatus.Committed.ToString() };
                            myBuild[i] = buildToUpdate;
                        }
                    }
                }
            }

            if (buildFailure)
            {
                context.RaiseBuildErrorRollBackEvent(context);
            }

            SaveBuildDataModel(context, true);

            if (buildFailure)
            {
                if (workEventArgs.Cancel)
                {
                    if (context.IsTransactional)
                    {
                        progressReporter.ReportProgress(100, new GeneralStatusEventArgs("Build Failed and Rolled Back"));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK;
                    }
                    else
                    {
                        progressReporter.ReportProgress(100, new GeneralStatusEventArgs("Build Failed. No Transaction Set."));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_CANCELLED_NO_TRANSACTION;

                    }
                }
                else
                {
                    if (context.IsTransactional)
                    {
                        progressReporter.ReportProgress(100, new GeneralStatusEventArgs("Build Failed and Rolled Back"));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK;
                    }
                    else
                    {
                        progressReporter.ReportProgress(100, new GeneralStatusEventArgs("Build Failed. No Transaction Set."));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_FAILED_NO_TRANSACTION;
                    }
                }
            }
            else
            {
                if (context.RunScriptOnly)
                {
                    progressReporter.ReportProgress(100, new GeneralStatusEventArgs("Script Generation Complete"));
                    workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.SCRIPT_GENERATION_COMPLETE;
                }
                else
                {
                    if (context.IsTrialBuild == false)
                    {
                        progressReporter.ReportProgress(100, new GeneralStatusEventArgs("Build Committed"));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_COMMITTED;
                    }
                    else
                    {
                        if (context.IsTransactional)
                        {
                            progressReporter.ReportProgress(100, new GeneralStatusEventArgs("Build Successful. Rolled back for Trial Build"));
                            workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL;
                        }
                        else
                        {
                            progressReporter.ReportProgress(100, new GeneralStatusEventArgs("Build Successful. Committed with no transaction"));
                            workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_COMMITTED;

                        }
                    }
                }
            }

            return (myBuild, updatedDataModel, finalizationErrorOccurred) ;
        }
    }
}
