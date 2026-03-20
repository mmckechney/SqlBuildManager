using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class DefaultDatabaseUtilityTests
    {
        private Mock<IConnectionsService> _mockConnectionsService = null!;
        private Mock<ISqlLoggingService> _mockLoggingService = null!;
        private Mock<IProgressReporter> _mockProgressReporter = null!;
        private Mock<ISqlBuildFileHelper> _mockFileHelper = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockConnectionsService = MockFactory.CreateMockConnectionsService();
            _mockLoggingService = MockFactory.CreateMockSqlLoggingService();
            _mockProgressReporter = new Mock<IProgressReporter>();
            _mockFileHelper = MockFactory.CreateMockFileHelper();
        }

        [TestMethod]
        public void Constructor_WithAllDependencies_CreatesInstance()
        {
            // Act
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            // Assert
            Assert.IsNotNull(utility);
        }

        [TestMethod]
        public void Constructor_WithNullFileHelper_CreatesInstance()
        {
            // Act - should not throw because a default file helper is used
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                null!);

            // Assert
            Assert.IsNotNull(utility);
        }

        #region ClearAllowScriptBlocks Tests

        [TestMethod]
        public void ClearAllowScriptBlocks_WithEmptyScriptIds_ReturnsUnchangedModel()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string>());

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(model.CommittedScript.Count, result.CommittedScript.Count);
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_WithMatchingScriptId_SetsAllowScriptBlockToFalse()
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
                committedScript: new List<CommittedScript> { committedScript });

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { scriptId });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CommittedScript.Count);
            Assert.IsFalse(result.CommittedScript[0].AllowScriptBlock);
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_WithNonMatchingServer_DoesNotClearBlock()
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
                serverName: "OtherServer",
                committedDate: DateTime.UtcNow,
                allowScriptBlock: true,
                scriptHash: "ABC123",
                sqlSyncBuildProjectId: 1);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript> { committedScript });

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { scriptId });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CommittedScript.Count);
            Assert.IsTrue(result.CommittedScript[0].AllowScriptBlock);
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_WithNonMatchingScriptId_DoesNotClearBlock()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = Guid.NewGuid().ToString();
            var differentScriptId = Guid.NewGuid().ToString();
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
                committedScript: new List<CommittedScript> { committedScript });

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { differentScriptId });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CommittedScript.Count);
            Assert.IsTrue(result.CommittedScript[0].AllowScriptBlock);
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_WithCaseInsensitiveScriptId_ClearsBlock()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = Guid.NewGuid().ToString().ToLower();
            var committedScript = new CommittedScript(
                scriptId: scriptId.ToUpper(),
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
                committedScript: new List<CommittedScript> { committedScript });

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { scriptId });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.CommittedScript[0].AllowScriptBlock);
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_WithCaseInsensitiveServerName_ClearsBlock()
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
                scriptHash: "ABC123",
                sqlSyncBuildProjectId: 1);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript> { committedScript });

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "testserver", new List<string> { scriptId });

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(result.CommittedScript[0].AllowScriptBlock);
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_WithMultipleScripts_ClearsOnlyMatchingScripts()
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

            var committedScripts = new List<CommittedScript>
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
                committedScript: committedScripts);

            // Act - only clear scriptId1
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { scriptId1 });

            // Assert
            Assert.AreEqual(3, result.CommittedScript.Count);
            Assert.IsFalse(result.CommittedScript[0].AllowScriptBlock); // scriptId1 cleared
            Assert.IsTrue(result.CommittedScript[1].AllowScriptBlock);  // scriptId2 not in list
            Assert.IsTrue(result.CommittedScript[2].AllowScriptBlock);  // scriptId3 different server
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_PreservesOtherModelProperties()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var project = new SqlSyncBuildProject(1, "TestProject", true);
            var script = new Script("test.sql", 1.0, "Test", true, true, DateTime.UtcNow, Guid.NewGuid().ToString(), "TestDB", false, false, "user", 30, null, null, null);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject> { project },
                script: new List<Script> { script },
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>());

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string>());

            // Assert
            Assert.AreEqual(1, result.SqlSyncBuildProject.Count);
            Assert.AreEqual("TestProject", result.SqlSyncBuildProject[0].ProjectName);
            Assert.AreEqual(1, result.Script.Count);
            Assert.AreEqual("test.sql", result.Script[0].FileName);
        }

        #endregion

        #region ReadScriptRunLogEntry Tests

        [TestMethod]
        public void ReadScriptRunLogEntry_WithValidReader_ReturnsPopulatedEntry()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = Guid.NewGuid();
            var commitDate = DateTime.UtcNow;

            var mockReader = new Mock<IDataRecord>();
            mockReader.Setup(r => r["BuildFileName"]).Returns("build.sbm");
            mockReader.Setup(r => r["ScriptFileName"]).Returns("script.sql");
            mockReader.Setup(r => r["ScriptId"]).Returns(scriptId.ToString());
            mockReader.Setup(r => r["ScriptFileHash"]).Returns("HASH123");
            mockReader.Setup(r => r["CommitDate"]).Returns(commitDate);
            mockReader.Setup(r => r["Sequence"]).Returns(1);
            mockReader.Setup(r => r["UserId"]).Returns("testuser");
            mockReader.Setup(r => r["AllowScriptBlock"]).Returns(true);
            mockReader.Setup(r => r["AllowBlockUpdateId"]).Returns("updateuser");
            mockReader.Setup(r => r["ScriptText"]).Returns("SELECT 1");
            mockReader.Setup(r => r["Tag"]).Returns("v1.0");

            // Act
            var entry = utility.ReadScriptRunLogEntry(mockReader.Object);

            // Assert
            Assert.IsNotNull(entry);
            Assert.AreEqual("build.sbm", entry.BuildFileName);
            Assert.AreEqual("script.sql", entry.ScriptFileName);
            Assert.AreEqual(scriptId, entry.ScriptId);
            Assert.AreEqual("HASH123", entry.ScriptFileHash);
            Assert.AreEqual(commitDate, entry.CommitDate);
            Assert.AreEqual(1, entry.Sequence);
            Assert.AreEqual("testuser", entry.UserId);
            Assert.AreEqual(true, entry.AllowScriptBlock);
            Assert.AreEqual("updateuser", entry.AllowBlockUpdateId);
            Assert.AreEqual("SELECT 1", entry.ScriptText);
            Assert.AreEqual("v1.0", entry.Tag);
        }

        [TestMethod]
        public void ReadScriptRunLogEntry_WithDbNullValues_ReturnsEntryWithNulls()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var mockReader = new Mock<IDataRecord>();
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
            var entry = utility.ReadScriptRunLogEntry(mockReader.Object);

            // Assert
            Assert.IsNotNull(entry);
            Assert.IsNull(entry.BuildFileName);
            Assert.IsNull(entry.ScriptFileName);
            Assert.IsNull(entry.ScriptId);
            Assert.IsNull(entry.ScriptFileHash);
            Assert.IsNull(entry.CommitDate);
            Assert.IsNull(entry.Sequence);
            Assert.IsNull(entry.UserId);
            Assert.IsNull(entry.AllowScriptBlock);
            Assert.IsNull(entry.AllowBlockUpdateId);
            Assert.IsNull(entry.ScriptText);
            Assert.IsNull(entry.Tag);
        }

        [TestMethod]
        public void ReadScriptRunLogEntry_WithMissingColumns_ReturnsEntryWithNulls()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var mockReader = new Mock<IDataRecord>();
            mockReader.Setup(r => r[It.IsAny<string>()]).Throws<IndexOutOfRangeException>();

            // Act
            var entry = utility.ReadScriptRunLogEntry(mockReader.Object);

            // Assert
            Assert.IsNotNull(entry);
            Assert.IsNull(entry.BuildFileName);
            Assert.IsNull(entry.ScriptId);
        }

        [TestMethod]
        public void ReadScriptRunLogEntry_WithPartialData_ReturnsEntryWithMixedValues()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var mockReader = new Mock<IDataRecord>();
            mockReader.Setup(r => r["BuildFileName"]).Returns("build.sbm");
            mockReader.Setup(r => r["ScriptFileName"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["ScriptId"]).Returns(Guid.NewGuid().ToString());
            mockReader.Setup(r => r["ScriptFileHash"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["CommitDate"]).Returns(DateTime.UtcNow);
            mockReader.Setup(r => r["Sequence"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["UserId"]).Returns("testuser");
            mockReader.Setup(r => r["AllowScriptBlock"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["AllowBlockUpdateId"]).Returns(DBNull.Value);
            mockReader.Setup(r => r["ScriptText"]).Returns("SELECT 1");
            mockReader.Setup(r => r["Tag"]).Returns(DBNull.Value);

            // Act
            var entry = utility.ReadScriptRunLogEntry(mockReader.Object);

            // Assert
            Assert.IsNotNull(entry);
            Assert.AreEqual("build.sbm", entry.BuildFileName);
            Assert.IsNull(entry.ScriptFileName);
            Assert.IsNotNull(entry.ScriptId);
            Assert.IsNull(entry.ScriptFileHash);
            Assert.IsNotNull(entry.CommitDate);
            Assert.IsNull(entry.Sequence);
            Assert.AreEqual("testuser", entry.UserId);
        }

        [TestMethod]
        public void ReadScriptRunLogEntry_WithInvalidGuid_ReturnsNullGuid()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var mockReader = new Mock<IDataRecord>();
            mockReader.Setup(r => r["BuildFileName"]).Returns("build.sbm");
            mockReader.Setup(r => r["ScriptFileName"]).Returns("script.sql");
            mockReader.Setup(r => r["ScriptId"]).Returns("not-a-valid-guid");
            mockReader.Setup(r => r["ScriptFileHash"]).Returns("HASH");
            mockReader.Setup(r => r["CommitDate"]).Returns(DateTime.UtcNow);
            mockReader.Setup(r => r["Sequence"]).Returns(1);
            mockReader.Setup(r => r["UserId"]).Returns("user");
            mockReader.Setup(r => r["AllowScriptBlock"]).Returns(false);
            mockReader.Setup(r => r["AllowBlockUpdateId"]).Returns("");
            mockReader.Setup(r => r["ScriptText"]).Returns("");
            mockReader.Setup(r => r["Tag"]).Returns("");

            // Act
            var entry = utility.ReadScriptRunLogEntry(mockReader.Object);

            // Assert
            Assert.IsNotNull(entry);
            Assert.IsNull(entry.ScriptId); // Invalid GUID should result in null
        }

        [TestMethod]
        public void ReadScriptRunLogEntry_WithInvalidDate_ReturnsNullDate()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var mockReader = new Mock<IDataRecord>();
            mockReader.Setup(r => r["BuildFileName"]).Returns("build.sbm");
            mockReader.Setup(r => r["ScriptFileName"]).Returns("script.sql");
            mockReader.Setup(r => r["ScriptId"]).Returns(Guid.NewGuid().ToString());
            mockReader.Setup(r => r["ScriptFileHash"]).Returns("HASH");
            mockReader.Setup(r => r["CommitDate"]).Returns("not-a-valid-date");
            mockReader.Setup(r => r["Sequence"]).Returns(1);
            mockReader.Setup(r => r["UserId"]).Returns("user");
            mockReader.Setup(r => r["AllowScriptBlock"]).Returns(false);
            mockReader.Setup(r => r["AllowBlockUpdateId"]).Returns("");
            mockReader.Setup(r => r["ScriptText"]).Returns("");
            mockReader.Setup(r => r["Tag"]).Returns("");

            // Act
            var entry = utility.ReadScriptRunLogEntry(mockReader.Object);

            // Assert
            Assert.IsNotNull(entry);
            Assert.IsNull(entry.CommitDate); // Invalid date should result in null
        }

        [TestMethod]
        public void ReadScriptRunLogEntry_WithInvalidInt_ReturnsNullInt()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var mockReader = new Mock<IDataRecord>();
            mockReader.Setup(r => r["BuildFileName"]).Returns("build.sbm");
            mockReader.Setup(r => r["ScriptFileName"]).Returns("script.sql");
            mockReader.Setup(r => r["ScriptId"]).Returns(Guid.NewGuid().ToString());
            mockReader.Setup(r => r["ScriptFileHash"]).Returns("HASH");
            mockReader.Setup(r => r["CommitDate"]).Returns(DateTime.UtcNow);
            mockReader.Setup(r => r["Sequence"]).Returns("not-a-number");
            mockReader.Setup(r => r["UserId"]).Returns("user");
            mockReader.Setup(r => r["AllowScriptBlock"]).Returns(false);
            mockReader.Setup(r => r["AllowBlockUpdateId"]).Returns("");
            mockReader.Setup(r => r["ScriptText"]).Returns("");
            mockReader.Setup(r => r["Tag"]).Returns("");

            // Act
            var entry = utility.ReadScriptRunLogEntry(mockReader.Object);

            // Assert
            Assert.IsNotNull(entry);
            Assert.IsNull(entry.Sequence); // Invalid int should result in null
        }

        [TestMethod]
        public void ReadScriptRunLogEntry_WithBooleanFalse_ReturnsCorrectValue()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var mockReader = new Mock<IDataRecord>();
            mockReader.Setup(r => r["BuildFileName"]).Returns("build.sbm");
            mockReader.Setup(r => r["ScriptFileName"]).Returns("script.sql");
            mockReader.Setup(r => r["ScriptId"]).Returns(Guid.NewGuid().ToString());
            mockReader.Setup(r => r["ScriptFileHash"]).Returns("HASH");
            mockReader.Setup(r => r["CommitDate"]).Returns(DateTime.UtcNow);
            mockReader.Setup(r => r["Sequence"]).Returns(1);
            mockReader.Setup(r => r["UserId"]).Returns("user");
            mockReader.Setup(r => r["AllowScriptBlock"]).Returns(false);
            mockReader.Setup(r => r["AllowBlockUpdateId"]).Returns("");
            mockReader.Setup(r => r["ScriptText"]).Returns("");
            mockReader.Setup(r => r["Tag"]).Returns("");

            // Act
            var entry = utility.ReadScriptRunLogEntry(mockReader.Object);

            // Assert
            Assert.IsNotNull(entry);
            Assert.AreEqual(false, entry.AllowScriptBlock);
        }

        #endregion

        #region ClearAllowScriptBlocks Additional Edge Case Tests

        [TestMethod]
        public void ClearAllowScriptBlocks_WithNullScriptId_DoesNotThrow()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var committedScript = new CommittedScript(
                scriptId: null,
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
                committedScript: new List<CommittedScript> { committedScript });

            // Act - should not throw
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { Guid.NewGuid().ToString() });

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.CommittedScript.Count);
            Assert.IsTrue(result.CommittedScript[0].AllowScriptBlock); // Should remain true since scriptId is null
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_PreservesCommittedScriptMetadata()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scriptId = Guid.NewGuid().ToString();
            var commitDate = DateTime.UtcNow;
            var committedScript = new CommittedScript(
                scriptId: scriptId,
                serverName: "TestServer",
                committedDate: commitDate,
                allowScriptBlock: true,
                scriptHash: "SPECIAL_HASH_123",
                sqlSyncBuildProjectId: 42);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript> { committedScript });

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", new List<string> { scriptId });

            // Assert
            Assert.AreEqual(scriptId, result.CommittedScript[0].ScriptId);
            Assert.AreEqual("TestServer", result.CommittedScript[0].ServerName);
            Assert.AreEqual(commitDate, result.CommittedScript[0].CommittedDate);
            Assert.AreEqual("SPECIAL_HASH_123", result.CommittedScript[0].ScriptHash);
            Assert.AreEqual(42, result.CommittedScript[0].SqlSyncBuildProjectId);
            Assert.IsFalse(result.CommittedScript[0].AllowScriptBlock); // Only this should change
        }

        [TestMethod]
        public void ClearAllowScriptBlocks_LargeScriptIdList_HandlesEfficiently()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var committedScripts = new List<CommittedScript>();
            var scriptIdsToClean = new List<string>();

            // Create 100 committed scripts
            for (int i = 0; i < 100; i++)
            {
                var id = Guid.NewGuid().ToString();
                committedScripts.Add(new CommittedScript(id, "TestServer", DateTime.UtcNow, true, $"HASH{i}", i));
                if (i % 2 == 0) // Clear every other script
                {
                    scriptIdsToClean.Add(id);
                }
            }

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: committedScripts);

            // Act
            var result = utility.ClearAllowScriptBlocks(model, "TestServer", scriptIdsToClean);

            // Assert
            Assert.AreEqual(100, result.CommittedScript.Count);
            int clearedCount = result.CommittedScript.Count(cs => cs.AllowScriptBlock == false);
            Assert.AreEqual(50, clearedCount); // Half should be cleared
        }

        #endregion

        #region ClearScriptBlocks Tests

        [TestMethod]
        public void ClearScriptBlocks_ThrowsNotImplementedException()
        {
            // Arrange
            var utility = new DefaultDatabaseUtility(
                _mockConnectionsService.Object,
                _mockLoggingService.Object,
                _mockProgressReporter.Object,
                _mockFileHelper.Object);

            var scrData = new ClearScriptData(
                selectedScriptIds: new string[] { "id1" },
                buildDataModel: SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel(),
                projectFileName: "project.sbx",
                buildZipFileName: "build.sbm");

            var connData = new SqlSync.Connection.ConnectionData();
            var mockProgress = new Mock<IProgressReporter>();
            var mockProps = new Mock<ISqlBuildRunnerProperties>();

            // Act & Assert
            Assert.ThrowsExactly<NotImplementedException>(() =>
                utility.ClearScriptBlocks(scrData, connData, mockProgress.Object, mockProps.Object));
        }

        #endregion
    }
}
