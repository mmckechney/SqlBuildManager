using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using SqlBuildManager.Connection;
using SqlBuildManager.SqlBuild.Models;
using SqlBuildManager.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Data.Common;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildRunnerPersistenceTests
    {
        private string directory = null!;

        [TestInitialize]
        public void Initialize()
        {
            directory = Path.Combine(Path.GetTempPath(), "sbm-persistence-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
        }

        [TestCleanup]
        public void Cleanup() => Directory.Delete(directory, true);

        [TestMethod]
        [DataRow(true, false)]
        [DataRow(true, true)]
        [DataRow(false, false)]
        [DataRow(false, true)]
        public async Task PreCommitSaveFailure_FinalizesAndDisposes_PreservingOriginalError(bool transactional, bool failFinalSave)
        {
            var fixture = CreateFixture(transactional, failFinalSave);
            var original = new IOException("Injected pre-commit persistence failure");
            var finalizer = new Mock<IBuildFinalizer>();
            finalizer.Setup(f => f.SaveBuildDataModelAsync(fixture.Context.Object, false)).ThrowsAsync(original);
            finalizer.Setup(f => f.PerformRunScriptFinalizationAsync(
                    fixture.Context.Object, fixture.Connections.Object, It.IsAny<IBuildFinalizerContext>(), true, fixture.Build))
                .Returns((ISqlBuildRunnerProperties ctx, IConnectionsService connections, IBuildFinalizerContext events, bool failed, Build build) =>
                    fixture.Finalizer.PerformRunScriptFinalizationAsync(ctx, connections, events, failed, build));
            var runner = new SqlBuildRunner(fixture.Connections.Object, fixture.Context.Object,
                new Mock<IBuildFinalizerContext>().Object, MockFactory.CreateMockSqlExecutor().Object,
                buildFinalizer: finalizer.Object, transactionManager: new Mock<ITransactionManager>().Object);

            var actual = await Assert.ThrowsAsync<IOException>(() =>
                runner.RunAsync(fixture.Model.Script, fixture.Build, "server", false, fixture.Batches, fixture.Model));

            Assert.AreSame(original, actual);
            Assert.AreEqual(transactional ? BuildItemStatus.RolledBack : BuildItemStatus.FailedNoTransaction, fixture.Build.FinalStatus);
            fixture.Transaction.Verify(t => t.Commit(), Times.Never);
            fixture.Transaction.Verify(t => t.Rollback(), transactional ? Times.Once() : Times.Never());
            fixture.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            if (transactional)
                fixture.Transaction.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            Assert.AreEqual(0, fixture.Connections.Object.Connections.Count);
        }

        [TestMethod]
        public async Task PostCommitSaveFailure_DoesNotRollbackOrChangeCommittedOutcome()
        {
            var fixture = CreateFixture(true, true);
            var finalizer = new Mock<IBuildFinalizer>();
            finalizer.Setup(f => f.SaveBuildDataModelAsync(fixture.Context.Object, false)).Returns(Task.CompletedTask);
            finalizer.Setup(f => f.PerformRunScriptFinalizationAsync(
                    fixture.Context.Object, fixture.Connections.Object, It.IsAny<IBuildFinalizerContext>(), false, fixture.Build))
                .Returns((ISqlBuildRunnerProperties ctx, IConnectionsService connections, IBuildFinalizerContext events, bool failed, Build build) =>
                    fixture.Finalizer.PerformRunScriptFinalizationAsync(ctx, connections, events, failed, build));
            var runner = new SqlBuildRunner(fixture.Connections.Object, fixture.Context.Object,
                new Mock<IBuildFinalizerContext>().Object, MockFactory.CreateMockSqlExecutor().Object,
                buildFinalizer: finalizer.Object, transactionManager: new Mock<ITransactionManager>().Object);

            await Assert.ThrowsAsync<IOException>(() =>
                runner.RunAsync(fixture.Model.Script, fixture.Build, "server", false, fixture.Batches, fixture.Model));

            Assert.AreEqual(BuildItemStatus.Committed, fixture.Build.FinalStatus);
            fixture.Transaction.Verify(t => t.Commit(), Times.Once);
            fixture.Transaction.Verify(t => t.Rollback(), Times.Never);
            fixture.Transaction.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            fixture.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        }

        [TestMethod]
        public async Task CleanupFailure_DoesNotMaskPersistenceError_AndStillDisposesConnection()
        {
            var fixture = CreateFixture(true, false);
            var original = new IOException("Original persistence failure");
            var finalizer = new Mock<IBuildFinalizer>();
            finalizer.Setup(f => f.SaveBuildDataModelAsync(It.IsAny<ISqlBuildRunnerProperties>(), false)).ThrowsAsync(original);
            finalizer.Setup(f => f.PerformRunScriptFinalizationAsync(It.IsAny<ISqlBuildRunnerProperties>(),
                    It.IsAny<IConnectionsService>(), It.IsAny<IBuildFinalizerContext>(), true, It.IsAny<Build>()))
                .ThrowsAsync(new IOException("Secondary finalization failure"));
            fixture.Transaction.Setup(t => t.Rollback()).Throws(new InvalidOperationException("Rollback failed"));
            fixture.Transaction.Protected().Setup("Dispose", ItExpr.IsAny<bool>()).Throws(new InvalidOperationException("Dispose failed"));
            var runner = new SqlBuildRunner(fixture.Connections.Object, fixture.Context.Object,
                new Mock<IBuildFinalizerContext>().Object, MockFactory.CreateMockSqlExecutor().Object,
                buildFinalizer: finalizer.Object, transactionManager: new Mock<ITransactionManager>().Object);

            var actual = await Assert.ThrowsAsync<IOException>(() =>
                runner.RunAsync(fixture.Model.Script, fixture.Build, "server", false, fixture.Batches, fixture.Model));

            Assert.AreSame(original, actual);
            fixture.Transaction.Verify(t => t.Rollback(), Times.Once);
            fixture.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        }

        [TestMethod]
        public async Task SuccessfulFinalization_PreservesBuildIdAndPersistedScriptHistory()
        {
            var fixture = CreateFixture(true, false);
            var buildId = fixture.Build.BuildId;
            var runner = new SqlBuildRunner(fixture.Connections.Object, fixture.Context.Object,
                new Mock<IBuildFinalizerContext>().Object, MockFactory.CreateMockSqlExecutor().Object,
                buildFinalizer: fixture.Finalizer, transactionManager: new Mock<ITransactionManager>().Object);

            await runner.RunAsync(fixture.Model.Script, fixture.Build, "server", false, fixture.Batches, fixture.Model);

            var saved = await SqlSyncBuildDataXmlSerializer.LoadAsync(fixture.Context.Object.ProjectFileName);
            Assert.AreEqual(buildId, fixture.Build.BuildId);
            Assert.AreEqual(1, saved.ScriptRun.Count);
            Assert.AreEqual(buildId, saved.ScriptRun[0].BuildId);
            Assert.AreEqual(BuildItemStatus.Committed, saved.Build[0].FinalStatus);
            fixture.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        }

        [TestMethod]
        public async Task CachedBatchTokens_RemainUnchangedAcrossTargets()
        {
            var first = CreateFixture(false, false);
            var second = CreateFixture(false, false);
            var shared = new ScriptBatchCollection
            {
                new ScriptBatch("script.sql", new[] { "SELECT '{{target}}';" }, first.Model.Script[0].ScriptId!)
            };
            second.Model.Script[0].ScriptId = first.Model.Script[0].ScriptId;
            first.Context.Setup(c => c.PerformScriptTokenReplacement(It.IsAny<string>())).Returns((string sql) => sql.Replace("{{target}}", "first"));
            second.Context.Setup(c => c.PerformScriptTokenReplacement(It.IsAny<string>())).Returns((string sql) => sql.Replace("{{target}}", "second"));
            var firstExecutor = MockFactory.CreateMockSqlExecutor();
            var secondExecutor = MockFactory.CreateMockSqlExecutor();
            var firstRunner = new SqlBuildRunner(first.Connections.Object, first.Context.Object,
                new Mock<IBuildFinalizerContext>().Object, firstExecutor.Object, buildFinalizer: MockFactory.CreateMockBuildFinalizer().Object);
            var secondRunner = new SqlBuildRunner(second.Connections.Object, second.Context.Object,
                new Mock<IBuildFinalizerContext>().Object, secondExecutor.Object, buildFinalizer: MockFactory.CreateMockBuildFinalizer().Object);

            await firstRunner.RunAsync(first.Model.Script, first.Build, "server", false, shared, first.Model);
            await secondRunner.RunAsync(second.Model.Script, second.Build, "server", false, shared, second.Model);

            Assert.AreEqual("SELECT '{{target}}';", shared[0].ScriptBatchContents[0]);
            firstExecutor.Verify(e => e.ExecuteAsync("SELECT 'first';", It.IsAny<int>(), It.IsAny<BuildConnectData>(), false, It.IsAny<CancellationToken>()), Times.Once);
            secondExecutor.Verify(e => e.ExecuteAsync("SELECT 'second';", It.IsAny<int>(), It.IsAny<BuildConnectData>(), false, It.IsAny<CancellationToken>()), Times.Once);
        }

        private Fixture CreateFixture(bool transactional, bool failFinalSave)
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model.Script.Add(new Script { FileName = "script.sql", ScriptId = Guid.NewGuid().ToString(), Database = "db", BuildOrder = 1 });
            var build = new Build { BuildId = Guid.NewGuid().ToString() };
            var history = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            history.Build.Add(build);
            var context = MockFactory.CreateMockRunnerContext();
            context.SetupProperty(c => c.BuildDataModel, model);
            context.SetupProperty(c => c.BuildHistoryModel, history);
            context.Setup(c => c.AddScriptRunToHistory(It.IsAny<ScriptRun>(), It.IsAny<Build>()))
                .Callback((ScriptRun run, Build _) => history.ScriptRun.Add(run));
            context.Setup(c => c.ProjectFilePath).Returns(directory);
            context.Setup(c => c.ProjectFileName).Returns(Path.Combine(directory, "project.xml"));
            context.Setup(c => c.BuildFileName).Returns(string.Empty);
            context.Setup(c => c.IsTransactional).Returns(transactional);
            var blocker = Path.Combine(directory, "not-a-directory");
            if (failFinalSave)
                File.WriteAllText(blocker, "blocker");
            context.Setup(c => c.BuildHistoryXmlFile).Returns(Path.Combine(failFinalSave ? blocker : directory, "history.xml"));
            var connection = new Mock<DbConnection>();
            var transaction = new Mock<DbTransaction>();
            var data = new BuildConnectData { Connection = connection.Object, Transaction = transactional ? transaction.Object : null!, ServerName = "server", DatabaseName = "db" };
            var connections = MockFactory.CreateMockConnectionsService();
            connections.Setup(c => c.Connections).Returns(new Dictionary<string, BuildConnectData> { ["server.db"] = data });
            connections.Setup(c => c.GetOrAddBuildConnectionDataClass(It.IsAny<ConnectionData>(), It.IsAny<string>(), It.IsAny<string>(), It.IsAny<bool>())).Returns(data);
            var logging = MockFactory.CreateMockSqlLoggingService();
            logging.Setup(l => l.LogCommittedScriptsToDatabase(It.IsAny<List<SqlLogging.CommittedScript>>(),
                It.IsAny<ISqlBuildRunnerProperties>(), It.IsAny<SqlBuildManager.SqlBuild.MultiDb.MultiDbData>())).ReturnsAsync(true);
            return new Fixture(context, connections, connection, transaction,
                new DefaultBuildFinalizer(logging.Object, new NoopProgressReporter()), model, build,
                new ScriptBatchCollection { new ScriptBatch("script.sql", new[] { "SELECT 1;" }, model.Script[0].ScriptId!) });
        }

        private sealed record Fixture(
            Mock<ISqlBuildRunnerContext> Context, Mock<IConnectionsService> Connections,
            Mock<DbConnection> Connection, Mock<DbTransaction> Transaction, DefaultBuildFinalizer Finalizer,
            SqlSyncBuildDataModel Model, Build Build, ScriptBatchCollection Batches);
    }
}
