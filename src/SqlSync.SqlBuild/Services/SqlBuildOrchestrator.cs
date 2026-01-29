using Microsoft.Identity.Client;
using SqlSync.SqlBuild.Models;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Services
{
    internal sealed class SqlBuildOrchestrator : ISqlBuildOrchestrator
    {
        private readonly IConnectionsService connectionsService;
        private readonly IBuildRetryPolicy _retryPolicy;
        private readonly ISqlLoggingService _sqlLoggingService;
        private readonly ISqlBuildRunnerContext _ctx;
        private readonly ISqlBuildRunnerProperties _props;
        private readonly IBuildRetryPolicy retryPolicy;
        private readonly IBuildFinalizerContext _finalizerCtx;
        public SqlBuildOrchestrator(ISqlBuildRunnerContext ctx, ISqlBuildRunnerProperties props, IBuildRetryPolicy retryPolicy, IBuildFinalizerContext finalizerCtx, IConnectionsService connectionsService, ISqlLoggingService sqlLoggingService)
        {
            _ctx = ctx;
            _props = props;
            _retryPolicy = retryPolicy; 
            this.connectionsService = connectionsService;
            _sqlLoggingService = sqlLoggingService;
            _finalizerCtx = finalizerCtx;
        }

        public Build Execute(
            SqlBuildRunDataModel runData,
            SqlBuildHelper.BuildPreparationResult prep,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl,
            int allowableTimeoutRetries)
        {
            _props.ErrorOccured = false;

            if (prep.FilteredScripts == null || prep.FilteredScripts.Count == 0)
            {
                return prep.Build;
            }

            Build buildResultsModel = null;
            int buildRetries = 0;
            var runner = SqlBuildHelper.SqlBuildRunnerFactory(connectionsService, _ctx, _finalizerCtx, null);

            while (buildRetries <= allowableTimeoutRetries &&
                (buildResultsModel == null || buildResultsModel.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout))
            {
                if (buildRetries > 0)
                {
                    _ctx.PublishScriptLog(false, new ScriptLogEventArgs(0, "", "", "", "Resetting transaction for retry attempt", true));
                    connectionsService.ResetConnectionsForRetry();
                    _props.CommittedScripts.Clear();
                }

                buildResultsModel = runner.Run(prep.FilteredScripts, prep.Build, serverName, isMultiDbRun, scriptBatchColl, runData.BuildDataModel!);

                if (buildRetries > 0 && buildResultsModel.FinalStatus == BuildItemStatus.Committed)
                    buildResultsModel.FinalStatus = BuildItemStatus.CommittedWithTimeoutRetries;

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
            _props.ErrorOccured = false;

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
            var runner = SqlBuildHelper.SqlBuildRunnerFactory(connectionsService, _ctx, _finalizerCtx, null);

            while (buildRetries <= allowableTimeoutRetries &&
                (buildResultsModel == null || buildResultsModel.FinalStatus == BuildItemStatus.FailedDueToScriptTimeout ))
            {
                cancellationToken.ThrowIfCancellationRequested();

                if (buildRetries > 0)
                {
                    _ctx.PublishScriptLog(false, new ScriptLogEventArgs(0, "", "", "", "Resetting transaction for retry attempt", true));
                    connectionsService.ResetConnectionsForRetry();
                    _props.CommittedScripts.Clear();
                }

                buildResultsModel = await runner.RunAsync(prep.FilteredScripts, prep.Build, serverName, isMultiDbRun, scriptBatchColl, runData.BuildDataModel!, cancellationToken).ConfigureAwait(false);



                if (buildRetries > 0 && buildResultsModel.FinalStatus == BuildItemStatus.Committed)
                    buildResultsModel.FinalStatus = BuildItemStatus.CommittedWithTimeoutRetries;

                if (!_retryPolicy.ShouldRetry(buildResultsModel, buildRetries))
                    break;

                buildRetries++;
            }

            return buildResultsModel;
        }
    }
}
