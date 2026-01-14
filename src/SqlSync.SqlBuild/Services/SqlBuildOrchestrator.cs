using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class SqlBuildOrchestrator : ISqlBuildOrchestrator
    {
        private readonly SqlBuildHelper _helper;
        private readonly IBuildRetryPolicy _retryPolicy;

        public SqlBuildOrchestrator(SqlBuildHelper helper)
        {
            _helper = helper;
            _retryPolicy = helper.RetryPolicy;
        }

        public Build Execute(
            SqlBuildRunDataModel runData,
            SqlBuildHelper.BuildPreparationResult prep,
            BackgroundWorker bgWorker,
            DoWorkEventArgs workEventArgs,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            int allowableTimeoutRetries)
        {
            _helper.bgWorker = bgWorker;
            _helper.ErrorOccured = false;

            if (prep.FilteredScripts == null || prep.FilteredScripts.Count == 0)
            {
                workEventArgs.Cancel = true;
                return prep.Build;
            }

            Build buildResultsModel = null;
            int buildRetries = 0;
            var runner = SqlBuildHelper.SqlBuildRunnerFactory(_helper, null);

            while (buildRetries <= allowableTimeoutRetries &&
                (buildResultsModel == null || buildResultsModel.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout.ToString()))
            {
                if (buildRetries > 0)
                {
                    ((ISqlBuildRunnerContext)_helper).PublishScriptLog(false, new ScriptLogEventArgs(0, "", "", "", "Resetting transaction for retry attempt", true));
                    _helper.ResetConnectionsForRetry();
                }

                buildResultsModel = runner.Run(prep.FilteredScripts, prep.Build, serverName, isMultiDbRun, scriptBatchColl, runData.BuildDataModel!, ref workEventArgs);

                if (buildRetries > 0 && buildResultsModel.FinalStatus == BuildItemStatus.Committed.ToString())
                    buildResultsModel = buildResultsModel with { FinalStatus = BuildItemStatus.CommittedWithTimeoutRetries.ToString() };

                if (!_retryPolicy.ShouldRetry(buildResultsModel, buildRetries))
                    break;

                buildRetries++;
            }

            return buildResultsModel;
        }

        public Task<Build> ExecuteAsync(
            SqlBuildRunDataModel runData,
            SqlBuildHelper.BuildPreparationResult prep,
            BackgroundWorker bgWorker,
            DoWorkEventArgs workEventArgs,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            int allowableTimeoutRetries,
            CancellationToken cancellationToken = default)
        {
            return Task.Run(() => Execute(runData, prep, bgWorker, workEventArgs, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries), cancellationToken);
        }
    }
}
