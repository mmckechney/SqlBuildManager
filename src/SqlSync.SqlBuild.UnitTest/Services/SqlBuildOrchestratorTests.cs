using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.Connection;
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
            var helper = new SqlBuildHelper(new ConnectionData("srv", "db"), createScriptRunLogFile: false);
            var runnerCalls = 0;
            var queue = new Queue<BuildModels.Build>();
            queue.Enqueue(new BuildModels.Build("n", "t", DateTime.UtcNow, null, "srv", BuildItemStatus.FailedDueToScriptTimeout, Guid.NewGuid().ToString(), "u"));
            queue.Enqueue(new BuildModels.Build("n", "t", DateTime.UtcNow, null, "srv", BuildItemStatus.Committed, Guid.NewGuid().ToString(), "u"));

            var fakeFactory = new FakeRunnerFactory(ctx => new FakeRunner(ctx, queue, () => runnerCalls++));

            var orchestrator = new SqlBuildOrchestrator(
                helper, 
                helper, 
                helper.RetryPolicy, 
                helper, 
                MockFactory.CreateMockConnectionsService().Object, 
                MockFactory.CreateMockSqlLoggingService().Object,
                fakeFactory);
            
            var runData = new SqlBuildRunDataModel(
                buildDataModel: SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel(),
                buildType: "type",
                server: "srv",
                buildDescription: "desc",
                startIndex: 0,
                projectFileName: "proj",
                isTrial: false,
                runItemIndexes: Array.Empty<double>(),
                runScriptOnly: false,
                buildFileName: "file",
                logToDatabaseName: string.Empty,
                isTransactional: true,
                platinumDacPacFileName: string.Empty,
                targetDatabaseOverrides: null,
                forceCustomDacpac: false,
                buildRevision: null,
                defaultScriptTimeout: 30,
                allowObjectDelete: false);

            var prep = new BuildPreparationResult(
                FilteredScripts: new List<BuildModels.Script> { new BuildModels.Script("one.sql", 1, null, null, null, null, "1", "db", false, true, null, null, null, null, null) },
                Build: new BuildModels.Build("n", "t", DateTime.UtcNow, null, "srv", null, Guid.NewGuid().ToString(), "u"),
                BuildPackageHash: "hash");

            var doa = new DoWorkEventArgs(null);
            var result = await orchestrator.ExecuteAsync(runData, prep, "srv", false, null, allowableTimeoutRetries: 2, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(BuildItemStatus.CommittedWithTimeoutRetries, result.FinalStatus);
            Assert.AreEqual(2, runnerCalls);
        }

        private sealed class FakeRunnerFactory : IRunnerFactory
        {
            private readonly Func<ISqlBuildRunnerContext, SqlBuildRunner> _factory;
            public FakeRunnerFactory(Func<ISqlBuildRunnerContext, SqlBuildRunner> factory) => _factory = factory;

            public SqlBuildRunner Create(IConnectionsService connectionsService, ISqlBuildRunnerContext context, IBuildFinalizerContext finalizerContext, ISqlCommandExecutor executor = null, ITransactionManager transactionManager = null)
            {
                return _factory(context);
            }
        }

        private sealed class FakeRunner : SqlBuildRunner
        {
            private readonly Queue<BuildModels.Build> _queue;
            private readonly Action _onCall;
            public FakeRunner(ISqlBuildRunnerContext ctx, Queue<BuildModels.Build> queue, Action onCall) : base(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object) { _queue = queue; _onCall = onCall; }

            public override async Task<BuildModels.Build> RunAsync(
                IList<BuildModels.Script> scripts,
                BuildModels.Build myBuild,
                string serverName,
                bool isMultiDbRun,
                ScriptBatchCollection scriptBatchColl,
                BuildModels.SqlSyncBuildDataModel buildDataModel,
                CancellationToken cancellationToken = default)
            {
                _onCall();
                await Task.Yield();
                return _queue.Dequeue();
            }
        }
    }
}
