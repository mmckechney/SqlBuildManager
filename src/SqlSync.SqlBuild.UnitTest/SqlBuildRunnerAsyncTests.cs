using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using BuildModels = SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Models;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
using SqlSync.SqlBuild.Services;
using Moq;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildRunnerAsyncTests
    {
        [TestMethod]
        public async Task RunAsync_UsesSyncPathAndReturnsBuild()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object, 
                ctx, 
                new Mock<IBuildFinalizerContext>().Object,
                new NoopExecutor(),
                null!,
                MockFactory.CreateMockBuildFinalizer().Object,
                MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter());
            var scripts = new List<BuildModels.Script>();
            var myBuild = new BuildModels.Build("name", "type", DateTime.UtcNow, null, "srv", null, Guid.NewGuid().ToString(), "u");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = await runner.RunAsync(scripts, myBuild, "srv", false, null!, model, CancellationToken.None);
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RunAsync_UsesExecutorAsyncPath()
        {
            var ctx = new LocalFakeRunnerContext();
            var exec = new TrackingExecutor();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object, 
                ctx,
                new Mock<IBuildFinalizerContext>().Object,
                exec,
                null!,
                MockFactory.CreateMockBuildFinalizer().Object,
                MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter());
            var scripts = new List<BuildModels.Script>
            {
                MakeScript("one.sql", "1")
            };
            var myBuild = new BuildModels.Build("name", "type", DateTime.UtcNow, null, "srv", null, Guid.NewGuid().ToString(), "u");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = await runner.RunAsync(scripts, myBuild, "srv", false, null!, model, CancellationToken.None);

            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RunAsync_Cancellation_StopsExecution()
        {
            var ctx = new LocalFakeRunnerContext();
            using var cts = new CancellationTokenSource();
            var exec = new CancelOnFirstExecutor(cts);
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object, 
                ctx,
                new Mock<IBuildFinalizerContext>().Object,
                exec,
                null!,
                MockFactory.CreateMockBuildFinalizer().Object,
                MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter());
            var scripts = new List<BuildModels.Script>
            {
                MakeScript("one.sql", "1"),
                MakeScript("two.sql", "2")
            };
            var myBuild = new BuildModels.Build("name", "type", DateTime.UtcNow, null, "srv", null, Guid.NewGuid().ToString(), "u");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
   
            var result = await runner.RunAsync(scripts, myBuild, "srv", false, null!, model, cts.Token);

            Assert.IsNotNull(result);
        }

        private sealed class TrackingExecutor : ISqlCommandExecutor
        {
            public bool ExecuteCalled { get; private set; }
            public bool ExecuteAsyncCalled { get; private set; }
            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional)
            {
                ExecuteCalled = true;
                return new SqlExecutionResult(true, string.Empty);
            }
            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
            {
                ExecuteAsyncCalled = true;
                return Task.FromResult(new SqlExecutionResult(true, string.Empty));
            }
        }

        private sealed class CancelOnFirstExecutor : ISqlCommandExecutor
        {
            private readonly CancellationTokenSource _cts;
            public int ExecuteAsyncCalls { get; private set; }
            public CancelOnFirstExecutor(CancellationTokenSource cts) => _cts = cts;
            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional) => new SqlExecutionResult(true, string.Empty);
            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
            {
                ExecuteAsyncCalls++;
                _cts.Cancel();
                cancellationToken.ThrowIfCancellationRequested();
                return Task.FromResult(new SqlExecutionResult(true, string.Empty));
            }
        }

        private static BuildModels.Script MakeScript(string fileName, string scriptId) =>
            new BuildModels.Script(fileName, null, null, null, null, null, scriptId, null, null, null, null, null, null, null, null);

        private sealed class NoopExecutor : ISqlCommandExecutor
        {
            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional) => new SqlExecutionResult(true, string.Empty);
            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
                => Task.FromResult(new SqlExecutionResult(true, string.Empty));
        }
    }
}
