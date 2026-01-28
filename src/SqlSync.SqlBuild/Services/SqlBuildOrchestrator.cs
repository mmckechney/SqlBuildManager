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
        private readonly IConnectionsService connectionsService;
        private readonly IBuildRetryPolicy _retryPolicy;
        private readonly ISqlLoggingService _sqlLoggingService;

        public SqlBuildOrchestrator(SqlBuildHelper helper, IConnectionsService connectionsService, ISqlLoggingService sqlLoggingService)
        {
            _helper = helper;
            _retryPolicy = helper.RetryPolicy;
            this.connectionsService = connectionsService;
            _sqlLoggingService = sqlLoggingService;
        }

        public Build Execute(
            SqlBuildRunDataModel runData,
            SqlBuildHelper.BuildPreparationResult prep,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            int allowableTimeoutRetries)
        {
            _helper.ErrorOccured = false;

            if (prep.FilteredScripts == null || prep.FilteredScripts.Count == 0)
            {
                return prep.Build;
            }

            Build buildResultsModel = null;
            int buildRetries = 0;
            var runner = SqlBuildHelper.SqlBuildRunnerFactory(connectionsService, _helper,_helper, null);

            while (buildRetries <= allowableTimeoutRetries &&
                (buildResultsModel == null || buildResultsModel.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout))
            {
                if (buildRetries > 0)
                {
                    ((ISqlBuildRunnerContext)_helper).PublishScriptLog(false, new ScriptLogEventArgs(0, "", "", "", "Resetting transaction for retry attempt", true));
                    connectionsService.ResetConnectionsForRetry();
                    _helper.CommittedScripts.Clear();
                }

                buildResultsModel = runner.Run(prep.FilteredScripts, prep.Build, serverName, isMultiDbRun, scriptBatchColl, runData.BuildDataModel!);

                if (buildRetries > 0 && buildResultsModel.FinalStatus == BuildItemStatus.Committed)
                    buildResultsModel = buildResultsModel with { FinalStatus = BuildItemStatus.CommittedWithTimeoutRetries };

                if (!_retryPolicy.ShouldRetry(buildResultsModel, buildRetries))
                    break;

                buildRetries++;
            }

            return buildResultsModel;
        }

        public Task<Build> ExecuteAsync(
            SqlBuildRunDataModel runData,
            SqlBuildHelper.BuildPreparationResult prep,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            int allowableTimeoutRetries,
            CancellationToken cancellationToken = default)
        {
            _helper.ErrorOccured = false;

            if (prep.FilteredScripts == null || prep.FilteredScripts.Count == 0)
            {
                return Task.FromResult(prep.Build);
            }

            return ExecuteAsyncCore(runData, prep, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries, cancellationToken);
        }

        private async Task<Build> ExecuteAsyncCore(
            SqlBuildRunDataModel runData,
            SqlBuildHelper.BuildPreparationResult prep,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            int allowableTimeoutRetries,
            CancellationToken cancellationToken)
        {
            Build buildResultsModel = null;
            int buildRetries = 0;
            var runner = SqlBuildHelper.SqlBuildRunnerFactory(connectionsService, _helper, _helper,null);

            while (buildRetries <= allowableTimeoutRetries &&
                (buildResultsModel == null || buildResultsModel.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout ))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (buildRetries > 0)
                {
                    ((ISqlBuildRunnerContext)_helper).PublishScriptLog(false, new ScriptLogEventArgs(0, "", "", "", "Resetting transaction for retry attempt", true));
                    connectionsService.ResetConnectionsForRetry();
                    _helper.CommittedScripts.Clear();
                }

                buildResultsModel = await runner.RunAsync(prep.FilteredScripts, prep.Build, serverName, isMultiDbRun, scriptBatchColl, runData.BuildDataModel!, cancellationToken).ConfigureAwait(false);



                if (buildRetries > 0 && buildResultsModel.FinalStatus == BuildItemStatus.Committed)
                    buildResultsModel = buildResultsModel with { FinalStatus = BuildItemStatus.CommittedWithTimeoutRetries };

                if (!_retryPolicy.ShouldRetry(buildResultsModel, buildRetries))
                    break;

                buildRetries++;
            }

            return buildResultsModel;
        }
    }
}
