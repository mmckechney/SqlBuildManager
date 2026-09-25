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
            fixture.Transaction.Protected().Setup("Dispose", ItExpr.Is<bool>(disposing => disposing)).Throws(new InvalidOperationException("Dispose failed"));
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
        [DataRow(0, false)]
        [DataRow(1, false)]
        [DataRow(2, false)]
        [DataRow(0, true)]
        [DataRow(1, true)]
        [DataRow(2, true)]
        public void Rollback_AlwaysDisposesAndClearsTransaction(int rollbackFailure, bool disposeFailure)
        {
            var fixture = CreateFixture(true, false);
            var data = fixture.Connections.Object.Connections["server.db"];
            if (rollbackFailure != 0)
                fixture.Transaction.Setup(t => t.Rollback()).Throws(new InvalidOperationException(
                    rollbackFailure == 1 ? "This SqlTransaction has completed; it is no longer usable." : "Unexpected rollback failure"));
            if (disposeFailure)
                fixture.Transaction.Protected().Setup("Dispose", ItExpr.Is<bool>(disposing => disposing)).Throws(new InvalidOperationException("Dispose failed"));

            var success = fixture.Finalizer.RollbackBuild(fixture.Connections.Object, true);

            Assert.AreEqual(rollbackFailure != 2 && !disposeFailure, success);
            Assert.IsNull(data.Transaction);
            fixture.Transaction.Verify(t => t.Rollback(), Times.Once);
            fixture.Transaction.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            fixture.Connection.Verify(c => c.Close(), Times.Once);
            fixture.Finalizer.RollbackBuild(fixture.Connections.Object, true);
            fixture.Transaction.Verify(t => t.Rollback(), Times.Once, "Finalization must not retry a cleared transaction.");
        }

        [TestMethod]
        [DataRow(false, false)]
        [DataRow(false, true)]
        [DataRow(true, false)]
        [DataRow(true, true)]
        public void Commit_AlwaysDisposesAndClearsTransaction(bool commitFailure, bool disposeFailure)
        {
            var fixture = CreateFixture(true, false);
            var data = fixture.Connections.Object.Connections["server.db"];
            if (commitFailure)
            {
                fixture.Transaction.Setup(t => t.Commit()).Throws(new InvalidOperationException("Original commit failure"));
                fixture.Transaction.Setup(t => t.Rollback()).Throws(new InvalidOperationException("This SqlTransaction has completed; it is no longer usable."));
            }
            if (disposeFailure)
                fixture.Transaction.Protected().Setup("Dispose", ItExpr.Is<bool>(disposing => disposing)).Throws(new InvalidOperationException("Dispose failed"));

            Assert.AreEqual(!commitFailure && !disposeFailure, fixture.Finalizer.CommitBuild(fixture.Connections.Object, true));
            Assert.IsNull(data.Transaction);
            fixture.Transaction.Verify(t => t.Commit(), Times.Once);
            fixture.Transaction.Verify(t => t.Rollback(), commitFailure ? Times.Once() : Times.Never());
            fixture.Transaction.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            fixture.Connection.Verify(c => c.Close(), Times.Once);
        }

        [TestMethod]
        public void CommitFailure_RollsBackAndClearsRemainingTransactions()
        {
            var fixture = CreateFixture(true, false);
            var remaining = new Mock<DbTransaction>();
            var remainingConnection = new Mock<DbConnection>();
            var data = new BuildConnectData { Transaction = remaining.Object, Connection = remainingConnection.Object };
            fixture.Connections.Object.Connections.Add("server.other", data);
            fixture.Transaction.Setup(t => t.Commit()).Throws(new InvalidOperationException("Original commit failure"));
            fixture.Transaction.Setup(t => t.Rollback()).Throws(new InvalidOperationException("This SqlTransaction has completed; it is no longer usable."));
            remaining.Setup(t => t.Rollback()).Throws(new InvalidOperationException("This SqlTransaction has completed; it is no longer usable."));

            Assert.IsFalse(fixture.Finalizer.CommitBuild(fixture.Connections.Object, true));

            Assert.IsNull(fixture.Connections.Object.Connections["server.db"].Transaction);
            Assert.IsNull(data.Transaction);
            remaining.Verify(t => t.Commit(), Times.Never);
            remaining.Verify(t => t.Rollback(), Times.Once);
            remaining.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            remainingConnection.Verify(c => c.Close(), Times.Once);
        }

        [TestMethod]
        public void Rollback_UsesInjectedProviderTransactionManager()
        {
            var fixture = CreateFixture(true, false);
            var completed = new InvalidOperationException("Provider transaction completed");
            fixture.Transaction.Setup(t => t.Rollback()).Throws(completed);
            var manager = new Mock<ITransactionManager>();
            manager.Setup(m => m.IsTransactionZombied(completed)).Returns(true);
            var finalizer = new DefaultBuildFinalizer(MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter(), manager.Object);

            Assert.IsTrue(finalizer.RollbackBuild(fixture.Connections.Object, true));

            manager.Verify(m => m.IsTransactionZombied(completed), Times.Once);
            Assert.IsNull(fixture.Connections.Object.Connections["server.db"].Transaction);
            fixture.Transaction.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        }

        [TestMethod]
        [DataRow(true)]
        [DataRow(false)]
        public async Task RunnerCleanup_IgnoresOnlyKnownCompletedTransaction(bool completed)
        {
            var fixture = CreateFixture(true, false);
            var data = fixture.Connections.Object.Connections["server.db"];
            fixture.Transaction.Setup(t => t.Rollback()).Throws(new InvalidOperationException(completed
                ? "This SqlTransaction has completed; it is no longer usable." : "Unexpected rollback failure"));
            var runner = new SqlBuildRunner(fixture.Connections.Object, fixture.Context.Object,
                new Mock<IBuildFinalizerContext>().Object, MockFactory.CreateMockSqlExecutor().Object,
                buildFinalizer: MockFactory.CreateMockBuildFinalizer().Object);

            if (completed)
            {
                var result = await runner.RunAsync(fixture.Model.Script, fixture.Build, "server", false, fixture.Batches, fixture.Model);
                Assert.AreEqual(BuildItemStatus.Committed, result.FinalStatus);
            }
            else
            {
                var error = await Assert.ThrowsAsync<AggregateException>(() =>
                    runner.RunAsync(fixture.Model.Script, fixture.Build, "server", false, fixture.Batches, fixture.Model));
                StringAssert.Contains(error.InnerException!.Message, "Unexpected rollback failure");
            }
            Assert.IsNull(data.Transaction);
            fixture.Transaction.Verify(t => t.Rollback(), Times.Once);
            fixture.Transaction.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            fixture.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        }

        [TestMethod]
        public async Task ScriptFailure_RemainsReportedWhenTransactionIsCompletedAndConnectionDisposalFails()
        {
            var fixture = CreateFixture(true, false);
            fixture.Model.Script[0].CausesBuildFailure = true;
            fixture.Model.Script[0].RollBackOnError = true;
            fixture.Transaction.Setup(t => t.Rollback()).Throws(new InvalidOperationException("This SqlTransaction has completed; it is no longer usable."));
            fixture.Connection.Protected().Setup("Dispose", ItExpr.Is<bool>(disposing => disposing)).Throws(new InvalidOperationException("Secondary disposal failure"));
            var executor = MockFactory.CreateMockSqlExecutor();
            executor.Setup(e => e.ExecuteAsync(It.IsAny<string>(), It.IsAny<int>(), It.IsAny<BuildConnectData>(), true, It.IsAny<CancellationToken>()))
                .ThrowsAsync(new TestDbException("Original script failure"));
            var runner = new SqlBuildRunner(fixture.Connections.Object, fixture.Context.Object,
                new Mock<IBuildFinalizerContext>().Object, executor.Object,
                buildFinalizer: fixture.Finalizer, transactionManager: new Mock<ITransactionManager>().Object);

            var result = await runner.RunAsync(fixture.Model.Script, fixture.Build, "server", false, fixture.Batches, fixture.Model);

            Assert.AreEqual(BuildItemStatus.RolledBack, result.FinalStatus);
            var saved = await SqlSyncBuildDataXmlSerializer.LoadAsync(fixture.Context.Object.ProjectFileName);
            Assert.IsFalse(saved.ScriptRun[0].Success);
            StringAssert.Contains(saved.ScriptRun[0].Results!, "Original script failure");
            fixture.Transaction.Verify(t => t.Rollback(), Times.Once, "Runner cleanup must not retry finalizer rollback.");
            fixture.Transaction.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
            fixture.Connection.Protected().Verify("Dispose", Times.Once(), ItExpr.IsAny<bool>());
        }

        private sealed class TestDbException(string message) : DbException(message);

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
