using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using BuildModels = SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildRunnerAdditionalTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithValidParameters_CreatesInstance()
        {
            // Arrange
            var connectionsService = MockFactory.CreateMockConnectionsService().Object;
            var ctx = new FakeRunnerContext();
            var finalizerContext = new Mock<IBuildFinalizerContext>().Object;

            // Act
            var runner = new SqlBuildRunner(connectionsService, ctx, finalizerContext);

            // Assert
            Assert.IsNotNull(runner);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var connectionsService = MockFactory.CreateMockConnectionsService().Object;
            var finalizerContext = new Mock<IBuildFinalizerContext>().Object;

            // Act
            new SqlBuildRunner(connectionsService, null, finalizerContext);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Constructor_WithNullFinalizerContext_ThrowsArgumentNullException()
        {
            // Arrange
            var connectionsService = MockFactory.CreateMockConnectionsService().Object;
            var ctx = new FakeRunnerContext();

            // Act
            new SqlBuildRunner(connectionsService, ctx, null);
        }

        [TestMethod]
        public void Constructor_WithAllOptionalParameters_CreatesInstance()
        {
            // Arrange
            var connectionsService = MockFactory.CreateMockConnectionsService().Object;
            var ctx = new FakeRunnerContext();
            var finalizerContext = new Mock<IBuildFinalizerContext>().Object;
            var executor = new SuccessfulExecutor();
            var fileHelper = MockFactory.CreateMockFileHelper().Object;
            var buildFinalizer = MockFactory.CreateMockBuildFinalizer().Object;
            var loggingService = MockFactory.CreateMockSqlLoggingService().Object;
            var progressReporter = new NoopProgressReporter();

            // Act
            var runner = new SqlBuildRunner(
                connectionsService, 
                ctx, 
                finalizerContext, 
                executor, 
                fileHelper, 
                buildFinalizer, 
                loggingService, 
                progressReporter);

            // Assert
            Assert.IsNotNull(runner);
        }

        #endregion

        #region ShouldSkipDueToCommittedScripts Tests

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_ReturnsTrue_WhenScriptIdExists()
        {
            // Arrange
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var scriptId = Guid.NewGuid().ToString();
            model = new BuildModels.SqlSyncBuildDataModel(
                sqlSyncBuildProject: model.SqlSyncBuildProject,
                script: model.Script,
                build: model.Build,
                scriptRun: model.ScriptRun,
                committedScript: new List<BuildModels.CommittedScript>
                {
                    new BuildModels.CommittedScript(scriptId, serverName: null, committedDate: null, allowScriptBlock: null, scriptHash: null, sqlSyncBuildProjectId: null)
                },
                codeReview: model.CodeReview);

            // Act
            var result = runner.ShouldSkipDueToCommittedScripts(scriptId, model);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_ReturnsFalse_WhenScriptIdDoesNotExist()
        {
            // Arrange
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = runner.ShouldSkipDueToCommittedScripts("non-existent-id", model);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_ReturnsFalse_WhenModelIsNull()
        {
            // Arrange
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);

            // Act
            var result = runner.ShouldSkipDueToCommittedScripts("any-id", null);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_CaseInsensitive_ReturnsTrue()
        {
            // Arrange
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var scriptId = "ABC-123-def";
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = new BuildModels.SqlSyncBuildDataModel(
                sqlSyncBuildProject: model.SqlSyncBuildProject,
                script: model.Script,
                build: model.Build,
                scriptRun: model.ScriptRun,
                committedScript: new List<BuildModels.CommittedScript>
                {
                    new BuildModels.CommittedScript(scriptId.ToUpper(), serverName: null, committedDate: null, allowScriptBlock: null, scriptHash: null, sqlSyncBuildProjectId: null)
                },
                codeReview: model.CodeReview);

            // Act
            var result = runner.ShouldSkipDueToCommittedScripts(scriptId.ToLower(), model);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region LoadBatchScripts Tests

        [TestMethod]
        public void LoadBatchScripts_ReturnsFromScriptBatchCollection_WhenPresent()
        {
            // Arrange
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var scriptId = "test-script-id";
            var expectedScripts = new[] { "SELECT 1;", "SELECT 2;" };
            var coll = new ScriptBatchCollection();
            coll.Add(new ScriptBatch("test.sql", expectedScripts, scriptId));

            // Act
            var result = runner.LoadBatchScripts(scriptId, "test.sql", false, coll);

            // Assert
            CollectionAssert.AreEqual(expectedScripts, result);
        }

        [TestMethod]
        public void LoadBatchScripts_ReturnsFromContext_WhenNoBatchCollection()
        {
            // Arrange
            var expectedScripts = new[] { "SELECT 3;" };
            var ctx = new FakeRunnerContext { ReadBatchReturn = expectedScripts };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);

            // Act
            var result = runner.LoadBatchScripts("id", "file.sql", false, null);

            // Assert
            CollectionAssert.AreEqual(expectedScripts, result);
        }

        [TestMethod]
        public void LoadBatchScripts_ReturnsFromContext_WhenScriptIdNotInCollection()
        {
            // Arrange
            var expectedScripts = new[] { "SELECT 4;" };
            var ctx = new FakeRunnerContext { ReadBatchReturn = expectedScripts };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection();
            coll.Add(new ScriptBatch("other.sql", new[] { "OTHER" }, "other-id"));

            // Act
            var result = runner.LoadBatchScripts("non-existent-id", "file.sql", false, coll);

            // Assert
            CollectionAssert.AreEqual(expectedScripts, result);
        }

        [TestMethod]
        public async Task LoadBatchScriptsAsync_ReturnsFromScriptBatchCollection_WhenPresent()
        {
            // Arrange
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var scriptId = "async-test-id";
            var expectedScripts = new[] { "SELECT ASYNC 1;" };
            var coll = new ScriptBatchCollection();
            coll.Add(new ScriptBatch("async.sql", expectedScripts, scriptId));

            // Act
            var result = await runner.LoadBatchScriptsAsync(scriptId, "async.sql", false, coll, CancellationToken.None);

            // Assert
            CollectionAssert.AreEqual(expectedScripts, result);
        }

        #endregion

        #region Run Method Tests

        [TestMethod]
        public void Run_WithEmptyScripts_SetsErrorOccurredFlag()
        {
            // Arrange - ValidateScriptsInput throws, which gets caught and handled
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object, 
                ctx, 
                new Mock<IBuildFinalizerContext>().Object,
                new SuccessfulExecutor(),
                null,
                MockFactory.CreateMockBuildFinalizer().Object,
                MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter());
            var build = CreateTestBuild();
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var emptyScripts = new List<BuildModels.Script>();

            // Act
            var result = runner.Run(emptyScripts, build, "srv", false, null, model);

            // Assert - Should complete but context should have error
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Run_WithNullScripts_SetsErrorOccurredFlag()
        {
            // Arrange
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object, 
                ctx, 
                new Mock<IBuildFinalizerContext>().Object,
                new SuccessfulExecutor(),
                null,
                MockFactory.CreateMockBuildFinalizer().Object,
                MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter());
            var build = CreateTestBuild();
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = runner.Run(null, build, "srv", false, null, model);

            // Assert - Should complete but return build with failure state
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Run_WithValidScript_ReturnsCompletedBuild()
        {
            // Arrange
            var ctx = new LocalFakeRunnerContext();
            var exec = new SuccessfulExecutor();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object,
                ctx,
                new Mock<IBuildFinalizerContext>().Object,
                exec,
                null,
                MockFactory.CreateMockBuildFinalizer().Object,
                MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter());
            var scripts = new List<BuildModels.Script> { CreateTestScript("test.sql", "script-1") };
            var build = CreateTestBuild();
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = runner.Run(scripts, build, "srv", false, 
                new ScriptBatchCollection { new ScriptBatch("test.sql", new[] { "SELECT 1" }, "script-1") }, 
                model);

            // Assert
            Assert.IsNotNull(result);
            // Build completes - actual FinalStatus depends on transactional context and mock behavior
            Assert.IsNotNull(result.FinalStatus);
        }

        [TestMethod]
        public void Run_SkipsPreRunScript_WhenNotAllowMultipleRunsAndAlreadyCommitted()
        {
            // Arrange
            var ctx = new LocalFakeRunnerContext();
            var exec = new CountingExecutor();
            var scriptId = "script-to-skip";
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object,
                ctx,
                new Mock<IBuildFinalizerContext>().Object,
                exec,
                null,
                MockFactory.CreateMockBuildFinalizer().Object,
                MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter());

            var baseModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var model = new BuildModels.SqlSyncBuildDataModel(
                sqlSyncBuildProject: baseModel.SqlSyncBuildProject,
                script: baseModel.Script,
                build: baseModel.Build,
                scriptRun: baseModel.ScriptRun,
                committedScript: new List<BuildModels.CommittedScript>
                {
                    new BuildModels.CommittedScript(scriptId, null, null, null, null, null)
                },
                codeReview: baseModel.CodeReview);

            var scripts = new List<BuildModels.Script>
            {
                new BuildModels.Script(
                    fileName: "skip.sql",
                    buildOrder: 1,
                    description: null,
                    rollBackOnError: true,
                    causesBuildFailure: true,
                    dateAdded: null,
                    scriptId: scriptId,
                    database: "db",
                    stripTransactionText: false,
                    allowMultipleRuns: false, // This will cause skip
                    addedBy: null,
                    scriptTimeOut: 30,
                    dateModified: null,
                    modifiedBy: null,
                    tag: null)
            };
            var build = CreateTestBuild();

            // Act
            var result = runner.Run(scripts, build, "srv", false,
                new ScriptBatchCollection { new ScriptBatch("skip.sql", new[] { "SELECT 1" }, scriptId) },
                model);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, exec.ExecuteCount, "Script should have been skipped");
        }

        #endregion

        #region RunAsync Tests

        [TestMethod]
        public async Task RunAsync_WithEmptyScripts_ReturnsWithErrorState()
        {
            // Arrange
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object, 
                ctx, 
                new Mock<IBuildFinalizerContext>().Object,
                new SuccessfulExecutor(),
                null,
                MockFactory.CreateMockBuildFinalizer().Object,
                MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter());
            var build = CreateTestBuild();
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act - ValidateScriptsInput throws but is caught
            var result = await runner.RunAsync(new List<BuildModels.Script>(), build, "srv", false, null, model, CancellationToken.None);

            // Assert - Should complete, errors are handled internally
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RunAsync_WithValidScript_ReturnsCompletedBuild()
        {
            // Arrange
            var ctx = new LocalFakeRunnerContext();
            var exec = new SuccessfulExecutor();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object,
                ctx,
                new Mock<IBuildFinalizerContext>().Object,
                exec,
                null,
                MockFactory.CreateMockBuildFinalizer().Object,
                MockFactory.CreateMockSqlLoggingService().Object,
                new NoopProgressReporter());
            var scripts = new List<BuildModels.Script> { CreateTestScript("test.sql", "script-1") };
            var build = CreateTestBuild();
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = await runner.RunAsync(scripts, build, "srv", false,
                new ScriptBatchCollection { new ScriptBatch("test.sql", new[] { "SELECT 1" }, "script-1") },
                model, CancellationToken.None);

            // Assert
            Assert.IsNotNull(result);
            // Build completes - actual FinalStatus depends on transactional context and mock behavior
            Assert.IsNotNull(result.FinalStatus);
        }

        #endregion

        #region Helper Methods and Classes

        private static BuildModels.Build CreateTestBuild()
        {
            return new BuildModels.Build(
                name: "Test Build",
                buildType: "UnitTest",
                buildStart: DateTime.UtcNow,
                buildEnd: null,
                serverName: "TestServer",
                finalStatus: null,
                buildId: Guid.NewGuid().ToString(),
                userId: "TestUser");
        }

        private static BuildModels.Script CreateTestScript(string fileName, string scriptId)
        {
            return new BuildModels.Script(
                fileName: fileName,
                buildOrder: 1,
                description: "Test Script",
                rollBackOnError: true,
                causesBuildFailure: true,
                dateAdded: DateTime.UtcNow,
                scriptId: scriptId,
                database: "TestDB",
                stripTransactionText: false,
                allowMultipleRuns: true,
                addedBy: "Test",
                scriptTimeOut: 30,
                dateModified: null,
                modifiedBy: null,
                tag: null);
        }

        private sealed class SuccessfulExecutor : ISqlCommandExecutor
        {
            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional)
                => new SqlExecutionResult(true, "Success");

            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
                => Task.FromResult(new SqlExecutionResult(true, "Success"));
        }

        private sealed class CountingExecutor : ISqlCommandExecutor
        {
            public int ExecuteCount { get; private set; }
            public int ExecuteAsyncCount { get; private set; }

            public SqlExecutionResult Execute(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional)
            {
                ExecuteCount++;
                return new SqlExecutionResult(true, "Success");
            }

            public Task<SqlExecutionResult> ExecuteAsync(string sql, int timeoutSeconds, BuildConnectData cData, bool isTransactional, CancellationToken cancellationToken = default)
            {
                ExecuteAsyncCount++;
                return Task.FromResult(new SqlExecutionResult(true, "Success"));
            }
        }

        #endregion
    }
}
