using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using BuildModels = SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class SqlBuildOrchestratorTests
    {
        [TestMethod]
        public async Task ExecuteAsync_RetriesOnTimeoutAndSucceeds()
        {
            var helper = new SqlBuildHelper(new ConnectionData("srv", "db"), createScriptRunLogFile: false)
            {
                bgWorker = new BackgroundWorker { WorkerReportsProgress = true, WorkerSupportsCancellation = true }
            };

            var runnerCalls = 0;
            var queue = new Queue<BuildModels.Build>();
            queue.Enqueue(new BuildModels.Build("n", "t", DateTime.UtcNow, null, "srv", BuildItemStatus.FailedDueToScriptTimeout, Guid.NewGuid().ToString(), "u"));
            queue.Enqueue(new BuildModels.Build("n", "t", DateTime.UtcNow, null, "srv", BuildItemStatus.Committed, Guid.NewGuid().ToString(), "u"));

            var originalFactory = SqlBuildHelper.SqlBuildRunnerFactory;
            try
            {
                SqlBuildHelper.SqlBuildRunnerFactory = (connSvc, ctx, finalizerContext, exec) => new FakeRunner(ctx, queue, () => runnerCalls++);

                var orchestrator = new SqlBuildOrchestrator(helper, MockFactory.CreateMockConnectionsService().Object, MockFactory.CreateMockSqlLoggingService().Object);
                var runData = new SqlBuildRunDataModel(
                    BuildDataModel: SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel(),
                    BuildType: "type",
                    Server: "srv",
                    BuildDescription: "desc",
                    StartIndex: 0,
                    ProjectFileName: "proj",
                    IsTrial: false,
                    RunItemIndexes: Array.Empty<double>(),
                    RunScriptOnly: false,
                    BuildFileName: "file",
                    LogToDatabaseName: string.Empty,
                    IsTransactional: true,
                    PlatinumDacPacFileName: string.Empty,
                    TargetDatabaseOverrides: null,
                    ForceCustomDacpac: false,
                    BuildRevision: null,
                    DefaultScriptTimeout: 30,
                    AllowObjectDelete: false);

                var prep = new SqlBuildHelper.BuildPreparationResult(
                    FilteredScripts: new List<BuildModels.Script> { new BuildModels.Script("one.sql", 1, null, null, null, null, "1", "db", false, true, null, null, null, null, null) },
                    Build: new BuildModels.Build("n", "t", DateTime.UtcNow, null, "srv", null, Guid.NewGuid().ToString(), "u"),
                    BuildPackageHash: "hash");

                var doa = new DoWorkEventArgs(null);
                var result = await orchestrator.ExecuteAsync(runData, prep, helper.bgWorker, doa, "srv", false, null, allowableTimeoutRetries: 2, CancellationToken.None);

                Assert.IsNotNull(result);
                Assert.AreEqual(BuildItemStatus.CommittedWithTimeoutRetries, result.FinalStatus);
                Assert.AreEqual(2, runnerCalls);
            }
            finally
            {
                SqlBuildHelper.SqlBuildRunnerFactory = originalFactory;
            }
        }

        private sealed class FakeRunner : SqlBuildRunner
        {
            private readonly Queue<BuildModels.Build> _queue;
            private readonly Action _onCall;
            public FakeRunner(ISqlBuildRunnerContext ctx, Queue<BuildModels.Build> queue, Action onCall) : base(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object) { _queue = queue; _onCall = onCall; }

            public override async Task<BuildModels.Build> RunAsync(
                IReadOnlyList<BuildModels.Script> scripts,
                BuildModels.Build myBuild,
                string serverName,
                bool isMultiDbRun,
                ScriptBatchCollection scriptBatchColl,
                BuildModels.SqlSyncBuildDataModel buildDataModel,
                DoWorkEventArgs workEventArgs,
                CancellationToken cancellationToken = default)
            {
                _onCall();
                await Task.Yield();
                return _queue.Dequeue();
            }
        }
    }
}
