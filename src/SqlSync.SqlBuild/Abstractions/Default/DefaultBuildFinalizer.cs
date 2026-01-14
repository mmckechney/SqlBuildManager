using SqlSync.SqlBuild.Models;
using System;
using System.ComponentModel;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
using SqlBuildManager.Interfaces.Console;

namespace SqlSync.SqlBuild
{
    internal sealed class DefaultBuildFinalizer : IBuildFinalizer
    {
        public Build PerformRunScriptFinalization(
            IBuildFinalizerContext context,
            bool buildFailure, 
            Build myBuild, 
            SqlSyncBuildDataModel buildDataModel, 
            ref DoWorkEventArgs workEventArgs)
        {
            if (context == null) throw new ArgumentNullException(nameof(context));

            DateTime end = DateTime.Now;
            myBuild = myBuild with { BuildEnd = end };

            if (buildFailure)
            {
                context.SetErrorOccurred(true);
                if (context.IsTransactional)
                    myBuild = myBuild with { FinalStatus = BuildItemStatus.RolledBack.ToString() };
                else
                    myBuild = myBuild with { FinalStatus = BuildItemStatus.FailedNoTransaction.ToString() };

                if (!context.IsTransactional)
                {
                    context.RecordCommittedScripts(context.CommittedScripts, buildDataModel, out var updatedModel);
                    buildDataModel = updatedModel;
                    context.LogCommittedScriptsToDatabase(context.CommittedScripts, null);
                }
            }
            else
            {
                if (!context.IsTrialBuild)
                {
                    if (context.IsTransactional)
                    {
                        context.BgWorker.ReportProgress(0, new GeneralStatusEventArgs("Attempting to Commit Build"));
                        bool commitSuccess = context.CommitBuild();
                        if (commitSuccess)
                            context.BgWorker.ReportProgress(0, new GeneralStatusEventArgs("Commit Successful"));
                    }
                    myBuild = myBuild with { FinalStatus = BuildItemStatus.Committed.ToString() };
                    context.RecordCommittedScripts(context.CommittedScripts, buildDataModel, out var updatedModel);
                    buildDataModel = updatedModel;
                    context.LogCommittedScriptsToDatabase(context.CommittedScripts, null);
                    context.RaiseBuildCommittedEvent(context, RunnerReturn.BuildCommitted);
                }
                else
                {
                    if (context.IsTransactional)
                    {
                        context.RollbackBuild();
                        myBuild = myBuild with { FinalStatus = BuildItemStatus.TrialRolledBack.ToString() };
                        context.RaiseBuildSuccessTrialRolledBackEvent(context);
                    }
                    else
                    {
                        myBuild = myBuild with { FinalStatus = BuildItemStatus.Committed.ToString() };
                    }
                }
            }

            if (buildFailure)
            {
                context.RaiseBuildErrorRollBackEvent(context);
            }

            context.SaveBuildDataSet(true);

            if (buildFailure)
            {
                if (workEventArgs.Cancel)
                {
                    if (context.IsTransactional)
                    {
                        context.BgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Failed and Rolled Back"));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK;
                    }
                    else
                    {
                        context.BgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Failed. No Transaction Set."));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_CANCELLED_NO_TRANSACTION;

                    }
                }
                else
                {
                    if (context.IsTransactional)
                    {
                        context.BgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Failed and Rolled Back"));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK;
                    }
                    else
                    {
                        context.BgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Failed. No Transaction Set."));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_FAILED_NO_TRANSACTION;
                    }
                }
            }
            else
            {
                if (context.RunScriptOnly)
                {
                    context.BgWorker.ReportProgress(100, new GeneralStatusEventArgs("Script Generation Complete"));
                    workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.SCRIPT_GENERATION_COMPLETE;
                }
                else
                {
                    if (context.IsTrialBuild == false)
                    {
                        context.BgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Committed"));
                        workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_COMMITTED;
                    }
                    else
                    {
                        if (context.IsTransactional)
                        {
                            context.BgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Successful. Rolled back for Trial Build"));
                            workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL;
                        }
                        else
                        {
                            context.BgWorker.ReportProgress(100, new GeneralStatusEventArgs("Build Successful. Committed with no transaction"));
                            workEventArgs.Result = SqlSync.SqlBuild.BuildResultStatus.BUILD_COMMITTED;

                        }
                    }
                }
            }

            return myBuild;
        }
    }
}
