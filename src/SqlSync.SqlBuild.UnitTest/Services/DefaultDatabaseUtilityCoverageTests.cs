using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    /// <summary>
    /// Extended tests for DefaultDatabaseUtility to improve code coverage
    /// </summary>
    [TestClass]
    public class DefaultDatabaseUtilityCoverageTests
    {
        private Mock<IConnectionsService> _mockConnectionsService;
        private Mock<ISqlLoggingService> _mockLoggingService;
        private Mock<IProgressReporter> _mockProgressReporter;
        private Mock<ISqlBuildFileHelper> _mockFileHelper;

        [TestInitialize]
        public void Setup()
        {
            _mockConnectionsService = MockFactory.CreateMockConnectionsService();
            _mockLoggingService = MockFactory.CreateMockSqlLoggingService();
            _mockProgressReporter = new Mock<IProgressReporter>();
            _mockFileHelper = MockFactory.CreateMockFileHelper();
        }

        #region ClearAllowScriptBlocks Tests

        [TestMethod]
        public void ClearAllowScriptBlocks_NoMatchingServerName_ReturnsUnchangedModel()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = Guid.NewGuid().ToString();
            var committedScript = new CommittedScript(
                scriptId: scriptId,
                serverName: "DifferentServer",
                committedDate: DateTime.UtcNow,
                allowScriptBlock: true,
                scriptHash: "ABC123",
                sqlSyncBuildProjectId: 1);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript> { committedScript },
                codeReview: new List<CodeReview>());

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { scriptId });

            // Assert
            Assert.AreEqual(1, result.CommittedScript.Count);
            Assert.IsTrue(result.CommittedScript[0].AllowScriptBlock); // Should remain unchanged
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_NoMatchingScriptId_ReturnsUnchangedModel()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = Guid.NewGuid().ToString();
            var committedScript = new CommittedScript(
                scriptId: scriptId,
                serverName: "TestServer",
                committedDate: DateTime.UtcNow,
                allowScriptBlock: true,
                scriptHash: "ABC123",
                sqlSyncBuildProjectId: 1);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript> { committedScript },
                codeReview: new List<CodeReview>());

            // Act - passing different script ID
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { Guid.NewGuid().ToString() });

            // Assert
            Assert.AreEqual(1, result.CommittedScript.Count);
            Assert.IsTrue(result.CommittedScript[0].AllowScriptBlock); // Should remain unchanged
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_MatchingServerAndScriptId_SetsAllowScriptBlockToFalse()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = Guid.NewGuid().ToString();
            var committedScript = new CommittedScript(
                scriptId: scriptId,
                serverName: "TestServer",
                committedDate: DateTime.UtcNow,
                allowScriptBlock: true,
                scriptHash: "ABC123",
                sqlSyncBuildProjectId: 1);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript> { committedScript },
                codeReview: new List<CodeReview>());

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { scriptId });

            // Assert
            Assert.AreEqual(1, result.CommittedScript.Count);
            Assert.IsFalse(result.CommittedScript[0].AllowScriptBlock);
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_CaseInsensitiveScriptIdMatch_SetsAllowScriptBlockToFalse()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = "abc123-def456";
            var committedScript = new CommittedScript(
                scriptId: scriptId.ToUpper(),
                serverName: "TestServer",
                committedDate: DateTime.UtcNow,
                allowScriptBlock: true,
                scriptHash: "HASH",
                sqlSyncBuildProjectId: 1);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript> { committedScript },
                codeReview: new List<CodeReview>());

            // Act - using lowercase script ID
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { scriptId.ToLower() });

            // Assert
            Assert.IsFalse(result.CommittedScript[0].AllowScriptBlock);
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_CaseInsensitiveServerNameMatch_SetsAllowScriptBlockToFalse()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = Guid.NewGuid().ToString();
            var committedScript = new CommittedScript(
                scriptId: scriptId,
                serverName: "TESTSERVER",
                committedDate: DateTime.UtcNow,
                allowScriptBlock: true,
                scriptHash: "HASH",
                sqlSyncBuildProjectId: 1);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript> { committedScript },
                codeReview: new List<CodeReview>());

            // Act - using different case for server name
            var result = utility.ClearAllowScriptBlocks(model, "testserver", new List<string> { scriptId });

            // Assert
            Assert.IsFalse(result.CommittedScript[0].AllowScriptBlock);
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_MultipleScripts_OnlyClearsMatchingOnes()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId1 = Guid.NewGuid().ToString();
            var scriptId2 = Guid.NewGuid().ToString();
            var scriptId3 = Guid.NewGuid().ToString();

            var scripts = new List<CommittedScript>
            {
                new CommittedScript(scriptId1, "TestServer", DateTime.UtcNow, true, "HASH1", 1),
                new CommittedScript(scriptId2, "TestServer", DateTime.UtcNow, true, "HASH2", 1),
                new CommittedScript(scriptId3, "OtherServer", DateTime.UtcNow, true, "HASH3", 1)
            };

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: scripts,
                codeReview: new List<CodeReview>());

            // Act - clear only scriptId1
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { scriptId1 });

            // Assert
            var script1Result = result.CommittedScript.First(s => s.ScriptId == scriptId1);
            var script2Result = result.CommittedScript.First(s => s.ScriptId == scriptId2);
            var script3Result = result.CommittedScript.First(s => s.ScriptId == scriptId3);

            Assert.IsFalse(script1Result.AllowScriptBlock); // Should be cleared
            Assert.IsTrue(script2Result.AllowScriptBlock);  // Should remain unchanged (not in list)
            Assert.IsTrue(script3Result.AllowScriptBlock);  // Should remain unchanged (different server)
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_NullScriptId_SkipsNullScript()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scripts = new List<CommittedScript>
            {
                new CommittedScript(null, "TestServer", DateTime.UtcNow, true, "HASH", 1)
            };

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: scripts,
                codeReview: new List<CodeReview>());

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { "someId" });

            // Assert - should not throw and should not modify null script ID entries
            Assert.AreEqual(1, result.CommittedScript.Count);
            Assert.IsTrue(result.CommittedScript[0].AllowScriptBlock);
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_PreservesOtherProperties()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = Guid.NewGuid().ToString();
            var commitDate = new DateTime(2023, 6, 15, 10, 30, 0);
            var committedScript = new CommittedScript(
                scriptId: scriptId,
                serverName: "TestServer",
                committedDate: commitDate,
                allowScriptBlock: true,
                scriptHash: "ORIGINALHASH",
                sqlSyncBuildProjectId: 42);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript> { committedScript },
                codeReview: new List<CodeReview>());

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { scriptId });

            // Assert - all other properties should be preserved
            var resultScript = result.CommittedScript[0];
            Assert.AreEqual(scriptId, resultScript.ScriptId);
            Assert.AreEqual("TestServer", resultScript.ServerName);
            Assert.AreEqual(commitDate, resultScript.CommittedDate);
            Assert.AreEqual("ORIGINALHASH", resultScript.ScriptHash);
            Assert.AreEqual(42, resultScript.SqlSyncBuildProjectId);
            Assert.IsFalse(resultScript.AllowScriptBlock); // Only this should change
        }

        #endregion

        #region ReadScriptRunLogEntry Tests

        [TestMethod]
        public void ReadScriptRunLogEntry_WithNullValues_HandlesGracefully()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            // Create a mock data reader that returns DBNull for all values
            var mockReader = new Mock<System.Data.IDataRecord>();
            mockReader.Setup(r => r["BuildFileName"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["ScriptFileName"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["ScriptId"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["ScriptFileHash"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["CommitDate"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["Sequence"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["UserId"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["AllowScriptBlock"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["AllowBlockUpdateId"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["ScriptText"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["Tag"]).Returns(DBNull.Value);

            // Act
            var result = utility.ReadScriptRunLogEntry(mockReader.Object);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNull(result.BuildFileName);
            Assert.IsNull(result.ScriptFileName);
            Assert.IsNull(result.ScriptId);
            Assert.IsNull(result.ScriptFileHash);
            Assert.IsNull(result.CommitDate);
            Assert.IsNull(result.Sequence);
            Assert.IsNull(result.UserId);
            Assert.IsNull(result.AllowScriptBlock);
            Assert.IsNull(result.AllowBlockUpdateId);
            Assert.IsNull(result.ScriptText);
            Assert.IsNull(result.Tag);
        }

        [TestMethod]
        public void ReadScriptRunLogEntry_WithValidValues_ReturnsPopulatedEntry()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = Guid.NewGuid();
            var commitDate = DateTime.Now;

            var mockReader = new Mock<System.Data.IDataRecord>();
            mockReader.Setup(r => r["BuildFileName"]).Returns("test.sbm");
            mockReader.Setup(r => r["ScriptFileName"]).Returns("script.sql");
            mockReader.Setup(r => r["ScriptId"]).Returns(scriptId.ToString());
            mockReader.Setup(r => r["ScriptFileHash"]).Returns("HASH123");
            mockReader.Setup(r => r["CommitDate"]).Returns(commitDate);
            mockReader.Setup(r => r["Sequence"]).Returns(5);
            mockReader.Setup(r => r["UserId"]).Returns("testuser");
            mockReader.Setup(r => r["AllowScriptBlock"]).Returns(true);
            mockReader.Setup(r => r["AllowBlockUpdateId"]).Returns("blockId");
            mockReader.Setup(r => r["ScriptText"]).Returns("SELECT 1");
            mockReader.Setup(r => r["Tag"]).Returns("v1.0");

            // Act
            var result = utility.ReadScriptRunLogEntry(mockReader.Object);

            // Assert
            Assert.AreEqual("test.sbm", result.BuildFileName);
            Assert.AreEqual("script.sql", result.ScriptFileName);
            Assert.AreEqual(scriptId, result.ScriptId);
            Assert.AreEqual("HASH123", result.ScriptFileHash);
            Assert.AreEqual(commitDate, result.CommitDate);
            Assert.AreEqual(5, result.Sequence);
            Assert.AreEqual("testuser", result.UserId);
            Assert.AreEqual(true, result.AllowScriptBlock);
            Assert.AreEqual("blockId", result.AllowBlockUpdateId);
            Assert.AreEqual("SELECT 1", result.ScriptText);
            Assert.AreEqual("v1.0", result.Tag);
        }

        #endregion

        #region HasBlockingSqlLog Edge Cases

        [TestMethod]
        public void HasBlockingSqlLog_WithEmptyDatabaseName_ReturnsFalse()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var connData = new SqlSync.Connection.ConnectionData();

            // Act
            bool result = utility.HasBlockingSqlLog(
                Guid.NewGuid(),
                connData,
                string.Empty,
                out string scriptHash,
                out string scriptTextHash,
                out DateTime commitDate);

            // Assert
            Assert.IsFalse(result);
            Assert.AreEqual(string.Empty, scriptHash);
            Assert.AreEqual(string.Empty, scriptTextHash);
            Assert.AreEqual(DateTime.MinValue, commitDate);
        }

        [TestMethod]
        public void HasBlockingSqlLog_WithWhitespaceDatabaseName_ReturnsFalse()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var connData = new SqlSync.Connection.ConnectionData();

            // Act
            bool result = utility.HasBlockingSqlLog(
                Guid.NewGuid(),
                connData,
                "   ",
                out string scriptHash,
                out string scriptTextHash,
                out DateTime commitDate);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void HasBlockingSqlLog_WithNullDatabaseName_ReturnsFalse()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var connData = new SqlSync.Connection.ConnectionData();

            // Act
            bool result = utility.HasBlockingSqlLog(
                Guid.NewGuid(),
                connData,
                null,
                out string scriptHash,
                out string scriptTextHash,
                out DateTime commitDate);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }
}
