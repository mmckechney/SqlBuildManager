using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// Final coverage tests for SqlBuildRunner - focusing on edge cases without database execution
    /// </summary>
    [TestClass]
    public class SqlBuildRunnerFinalCoverageTests
    {
        /// <summary>
        /// Helper to create a Script with common defaults
        /// </summary>
        private static Script CreateScript(string fileName, string? database = null, bool allowMultipleRuns = true, int scriptTimeout = 30, string? scriptId = null)
        {
            return new Script(
                fileName: fileName,
                buildOrder: 1,
                description: null,
                rollBackOnError: true,
                causesBuildFailure: true,
                dateAdded: null,
                scriptId: scriptId ?? Guid.NewGuid().ToString(),
                database: database ?? "TestDb",
                stripTransactionText: false,
                allowMultipleRuns: allowMultipleRuns,
                addedBy: null,
                scriptTimeOut: scriptTimeout,
                dateModified: null,
                modifiedBy: null,
                tag: null);
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new SqlBuildRunner(
                    new DefaultConnectionsService(),
                    null,
                    new Mock<IBuildFinalizerContext>().Object));
        }

        [TestMethod]
        public void Constructor_WithNullFinalizerContext_ThrowsArgumentNullException()
        {
            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                new SqlBuildRunner(
                    new DefaultConnectionsService(),
                    new FakeRunnerContext(),
                    null));
        }

        [TestMethod]
        public void Constructor_WithDefaultServices_CreatesInstance()
        {
            // Act
            var runner = new SqlBuildRunner(
                new DefaultConnectionsService(),
                new FakeRunnerContext(),
                new Mock<IBuildFinalizerContext>().Object);

            // Assert
            Assert.IsNotNull(runner);
        }

        #endregion

        #region ShouldSkipDueToCommittedScripts Extended Tests

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_WithMultipleCommittedScripts_FindsMatch()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(new DefaultConnectionsService(), ctx, new Mock<IBuildFinalizerContext>().Object);
            
            var model = new SqlSyncBuildDataModel(
                new List<SqlSyncBuildProject>(),
                new List<Script>(),
                new List<Build>(),
                new List<ScriptRun>(),
                new List<CommittedScript>
                {
                    new CommittedScript("script-1", null, null, null, null, null),
                    new CommittedScript("script-2", null, null, null, null, null),
                    new CommittedScript("script-3", null, null, null, null, null)
                });

            var result = runner.ShouldSkipDueToCommittedScripts("script-2", model);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ShouldSkipDueToCommittedScripts_WithEmptyScriptId_ReturnsFalse()
        {
            var ctx = new FakeRunnerContext();
            var runner = new SqlBuildRunner(new DefaultConnectionsService(), ctx, new Mock<IBuildFinalizerContext>().Object);
            
            var model = new SqlSyncBuildDataModel(
                new List<SqlSyncBuildProject>(),
                new List<Script>(),
                new List<Build>(),
                new List<ScriptRun>(),
                new List<CommittedScript>
                {
                    new CommittedScript("script-1", null, null, null, null, null)
                });

            var result = runner.ShouldSkipDueToCommittedScripts("", model);

            Assert.IsFalse(result);
        }

        #endregion

        #region LoadBatchScripts Extended Tests

        [TestMethod]
        public async Task LoadBatchScriptsAsync_WithNullBatchContentsInCollection_ReadsFromContext()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "FROM CONTEXT" } };
            var runner = new SqlBuildRunner(new DefaultConnectionsService(), ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection
            {
                new ScriptBatch("file.sql", null, "script-id")
            };

            var result = await runner.LoadBatchScriptsAsync("script-id", "file.sql", stripTransaction: false, scriptBatchColl: coll, default);

            CollectionAssert.AreEqual(new[] { "FROM CONTEXT" }, result);
        }

        [TestMethod]
        public async Task LoadBatchScriptsAsync_WithMultipleBatchesInCollection_FindsCorrectOne()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "WRONG" } };
            var runner = new SqlBuildRunner(new DefaultConnectionsService(), ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection
            {
                new ScriptBatch("file1.sql", new[] { "BATCH 1" }, "script-1"),
                new ScriptBatch("file2.sql", new[] { "BATCH 2" }, "script-2"),
                new ScriptBatch("file3.sql", new[] { "BATCH 3" }, "script-3")
            };

            var result = await runner.LoadBatchScriptsAsync("script-2", "file2.sql", stripTransaction: false, scriptBatchColl: coll, default);

            CollectionAssert.AreEqual(new[] { "BATCH 2" }, result);
        }

        #endregion

        #region LoadBatchScriptsAsync Extended Tests

        [TestMethod]
        public async Task LoadBatchScriptsAsync_WithNullCollection_ReadsFromContext()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "ASYNC CONTEXT" } };
            var runner = new SqlBuildRunner(new DefaultConnectionsService(), ctx, new Mock<IBuildFinalizerContext>().Object);

            var result = await runner.LoadBatchScriptsAsync("script-id", "file.sql", stripTransaction: false, scriptBatchColl: null, System.Threading.CancellationToken.None);

            CollectionAssert.AreEqual(new[] { "ASYNC CONTEXT" }, result);
        }

        [TestMethod]
        public async Task LoadBatchScriptsAsync_WithValidCollection_ReturnsFromCollection()
        {
            var ctx = new FakeRunnerContext { ReadBatchReturn = new[] { "WRONG" } };
            var runner = new SqlBuildRunner(new DefaultConnectionsService(), ctx, new Mock<IBuildFinalizerContext>().Object);
            var coll = new ScriptBatchCollection
            {
                new ScriptBatch("file.sql", new[] { "CORRECT ASYNC" }, "script-id")
            };

            var result = await runner.LoadBatchScriptsAsync("script-id", "file.sql", stripTransaction: false, scriptBatchColl: coll, System.Threading.CancellationToken.None);

            CollectionAssert.AreEqual(new[] { "CORRECT ASYNC" }, result);
        }

        #endregion

        #region Script Creation Tests

        [TestMethod]
        public void CreateScript_Helper_SetsProperDefaults()
        {
            // Act
            var script = CreateScript("test.sql");

            // Assert
            Assert.AreEqual("test.sql", script.FileName);
            Assert.AreEqual("TestDb", script.Database);
            Assert.IsTrue(script.AllowMultipleRuns);
            Assert.AreEqual(30, script.ScriptTimeOut);
            Assert.IsFalse(string.IsNullOrEmpty(script.ScriptId));
        }

        [TestMethod]
        public void CreateScript_Helper_AllowsCustomization()
        {
            // Arrange
            var customId = Guid.NewGuid().ToString();

            // Act
            var script = CreateScript("custom.sql", "CustomDb", allowMultipleRuns: false, scriptTimeout: 60, scriptId: customId);

            // Assert
            Assert.AreEqual("custom.sql", script.FileName);
            Assert.AreEqual("CustomDb", script.Database);
            Assert.IsFalse(script.AllowMultipleRuns);
            Assert.AreEqual(60, script.ScriptTimeOut);
            Assert.AreEqual(customId, script.ScriptId);
        }

        #endregion
    }
}
