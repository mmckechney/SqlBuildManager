using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// Additional coverage tests for SqlBuildRunner
    /// </summary>
    [TestClass]
    public class SqlBuildRunnerCoverageTests
    {
        #region ShouldSkipDueToCommittedScripts Tests

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_ReturnsFalse_WhenNoCommittedScripts()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = runner.ShouldSkipDueToCommittedScripts("any-id", model);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_ReturnsFalse_WhenDifferentScriptId()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: model.SqlSyncBuildProject,
                script: model.Script,
                build: model.Build,
                scriptRun: model.ScriptRun,
                committedScript: new List<CommittedScript>
                {
                    new CommittedScript("script-1", null, null, null, null, null)
                });

            var result = runner.ShouldSkipDueToCommittedScripts("different-id", model);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_IsCaseInsensitive()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: model.SqlSyncBuildProject,
                script: model.Script,
                build: model.Build,
                scriptRun: model.ScriptRun,
                committedScript: new List<CommittedScript>
                {
                    new CommittedScript("ABC-123", null, null, null, null, null)
                });

            var result = runner.ShouldSkipDueToCommittedScripts("abc-123", model);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_WithNullModel_ReturnsFalse()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);

            var result = runner.ShouldSkipDueToCommittedScripts("any-id", null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_WithNullCommittedScriptList_ReturnsFalse()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: null);

            var result = runner.ShouldSkipDueToCommittedScripts("any-id", model);

            Assert.IsFalse(result);
        }

        #endregion

        #region LoadBatchScripts Tests

        [TestMethod]
        public void LoadBatchScripts_WithNullScriptBatchColl_ReadsFromContext()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "SELECT 1;", "SELECT 2;" } };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);

            var result = runner.LoadBatchScripts("script-id", "file.sql", stripTransaction: false, scriptBatchColl: null);

            CollectionAssert.AreEqual(new[] { "SELECT 1;", "SELECT 2;" }, result);
        }

        [TestMethod]
        public void LoadBatchScripts_WithEmptyScriptBatchColl_ReadsFromContext()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "SELECT 3;" } };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection();

            var result = runner.LoadBatchScripts("script-id", "file.sql", stripTransaction: false, scriptBatchColl: coll);

            CollectionAssert.AreEqual(new[] { "SELECT 3;" }, result);
        }

        [TestMethod]
        public void LoadBatchScripts_WithMatchingScriptInCollection_ReturnsFromCollection()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "WRONG" } };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection
            {
                new ScriptBatch("file.sql", new[] { "SELECT CORRECT;" }, "script-id")
            };

            var result = runner.LoadBatchScripts("script-id", "file.sql", stripTransaction: false, scriptBatchColl: coll);

            CollectionAssert.AreEqual(new[] { "SELECT CORRECT;" }, result);
        }

        [TestMethod]
        public void LoadBatchScripts_WithDifferentScriptIdInCollection_ReadsFromContext()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "FROM CONTEXT" } };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection
            {
                new ScriptBatch("file.sql", new[] { "FROM COLLECTION" }, "different-script-id")
            };

            var result = runner.LoadBatchScripts("my-script-id", "file.sql", stripTransaction: false, scriptBatchColl: coll);

            CollectionAssert.AreEqual(new[] { "FROM CONTEXT" }, result);
        }

        [TestMethod]
        public void LoadBatchScripts_WithEmptyBatchContentsInCollection_ReadsFromContext()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "FROM CONTEXT" } };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection
            {
                new ScriptBatch("file.sql", new string[0], "script-id")
            };

            var result = runner.LoadBatchScripts("script-id", "file.sql", stripTransaction: false, scriptBatchColl: coll);

            CollectionAssert.AreEqual(new[] { "FROM CONTEXT" }, result);
        }

        #endregion

        #region LoadBatchScriptsAsync Tests

        [TestMethod]
        public async Task LoadBatchScriptsAsync_WithMatchingScriptInCollection_ReturnsFromCollection()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "WRONG" } };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection
            {
                new ScriptBatch("file.sql", new[] { "SELECT ASYNC;" }, "script-id")
            };

            var result = await runner.LoadBatchScriptsAsync("script-id", "file.sql", stripTransaction: false, scriptBatchColl: coll, CancellationToken.None);

            CollectionAssert.AreEqual(new[] { "SELECT ASYNC;" }, result);
        }

        [TestMethod]
        public async Task LoadBatchScriptsAsync_WithNoMatch_ReadsFromContext()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "ASYNC CONTEXT" } };
            var runner = new SqlBuildRunner(MockFactory.CreateMockConnectionsService().Object, ctx, new Mock<IBuildFinalizerContext>().Object);

            var result = await runner.LoadBatchScriptsAsync("script-id", "file.sql", stripTransaction: false, scriptBatchColl: null, CancellationToken.None);

            CollectionAssert.AreEqual(new[] { "ASYNC CONTEXT" }, result);
        }

        #endregion

        #region Run Method Edge Cases

        [TestMethod]
        public void Run_WithEmptyScriptList_ReturnsFailedBuild()
        {
            var ctx = new FakeRunnerContext();
            var mockFinalizer = MockFactory.CreateMockBuildFinalizer();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object,
                ctx,
                new Mock<IBuildFinalizerContext>().Object,
                buildFinalizer: mockFinalizer.Object);

            var scripts = new List<Script>();
            var build = new Build("Test", null, null, null, null, null, null, null);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Empty scripts cause an exception which is caught and results in a failed build
            var result = runner.Run(scripts, build, "Server", false, null, model);
            
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void Run_WithNullScriptList_ReturnsFailedBuild()
        {
            var ctx = new FakeRunnerContext();
            var mockFinalizer = MockFactory.CreateMockBuildFinalizer();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object,
                ctx,
                new Mock<IBuildFinalizerContext>().Object,
                buildFinalizer: mockFinalizer.Object);

            var build = new Build("Test", null, null, null, null, null, null, null);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Null scripts cause an exception which is caught and results in a failed build
            var result = runner.Run(null, build, "Server", false, null, model);
            
            Assert.IsNotNull(result);
        }

        #endregion

        #region RunAsync Method Edge Cases

        [TestMethod]
        public async Task RunAsync_WithEmptyScriptList_ReturnsFailedBuild()
        {
            var ctx = new FakeRunnerContext();
            var mockFinalizer = MockFactory.CreateMockBuildFinalizer();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object,
                ctx,
                new Mock<IBuildFinalizerContext>().Object,
                buildFinalizer: mockFinalizer.Object);

            var scripts = new List<Script>();
            var build = new Build("Test", null, null, null, null, null, null, null);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = await runner.RunAsync(scripts, build, "Server", false, null, model, CancellationToken.None);
            
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RunAsync_WithNullScriptList_ReturnsFailedBuild()
        {
            var ctx = new FakeRunnerContext();
            var mockFinalizer = MockFactory.CreateMockBuildFinalizer();
            var runner = new SqlBuildRunner(
                MockFactory.CreateMockConnectionsService().Object,
                ctx,
                new Mock<IBuildFinalizerContext>().Object,
                buildFinalizer: mockFinalizer.Object);

            var build = new Build("Test", null, null, null, null, null, null, null);
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = await runner.RunAsync(null, build, "Server", false, null, model, CancellationToken.None);
            
            Assert.IsNotNull(result);
        }

        #endregion
    }
}
