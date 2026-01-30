using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.SqlLogging;
using System;
using System.Collections.Generic;
using System.Linq;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
using CommittedScript = SqlSync.SqlBuild.Models.CommittedScript;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class DefaultBuildFinalizerTests
    {
        private Mock<ISqlLoggingService> _mockSqlLoggingService;
        private Mock<IProgressReporter> _mockProgressReporter;
        private DefaultBuildFinalizer _finalizer;

        [TestInitialize]
        public void TestInitialize()
        {
            _mockSqlLoggingService = new Mock<ISqlLoggingService>();
            _mockProgressReporter = new Mock<IProgressReporter>();
            _finalizer = new DefaultBuildFinalizer(_mockSqlLoggingService.Object, _mockProgressReporter.Object);
        }

        #region CommitBuild Tests

        [TestMethod]
        public void CommitBuild_NonTransactional_ReturnsTrue()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();

            // Act
            var result = _finalizer.CommitBuild(mockConnectionsService.Object, isTransactional: false);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CommitBuild_TransactionalWithEmptyConnections_ReturnsTrue()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();
            mockConnectionsService.Setup(x => x.Connections)
                .Returns(new Dictionary<string, BuildConnectData>());

            // Act
            var result = _finalizer.CommitBuild(mockConnectionsService.Object, isTransactional: true);

            // Assert
            Assert.IsTrue(result);
        }

        // Note: Tests that mock SqlTransaction/SqlConnection directly are not possible because they are sealed classes.
        // The CommitBuild and RollbackBuild methods with actual transactions would require integration tests.

        #endregion

        #region RollbackBuild Tests

        [TestMethod]
        public void RollbackBuild_NonTransactional_ReturnsFalse()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();

            // Act
            var result = _finalizer.RollbackBuild(mockConnectionsService.Object, isTransactional: false);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void RollbackBuild_TransactionalWithEmptyConnections_ReturnsTrue()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();
            mockConnectionsService.Setup(x => x.Connections)
                .Returns(new Dictionary<string, BuildConnectData>());

            // Act
            var result = _finalizer.RollbackBuild(mockConnectionsService.Object, isTransactional: true);

            // Assert
            Assert.IsTrue(result);
        }

        // Note: Tests that mock SqlTransaction/SqlConnection directly are not possible because they are sealed classes.
        // RollbackBuild and CommitBuild methods with actual transactions would require integration tests.

        #endregion

        #region RecordCommittedScripts Tests

        [TestMethod]
        public void RecordCommittedScripts_WithNullCommittedScripts_ReturnsModelUnchanged()
        {
            // Arrange
            var buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = _finalizer.RecordCommittedScripts(null, buildDataModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.CommittedScript.Count);
        }

        [TestMethod]
        public void RecordCommittedScripts_WithEmptyCommittedScripts_ReturnsModelUnchanged()
        {
            // Arrange
            var committedScripts = new List<LoggingCommittedScript>();
            var buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = _finalizer.RecordCommittedScripts(committedScripts, buildDataModel);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.CommittedScript.Count);
        }

        [TestMethod]
        public void RecordCommittedScripts_WithValidScripts_AddsToModel()
        {
            // Arrange
            var scriptId = Guid.NewGuid();
            var committedScripts = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(
                    scriptId: scriptId,
                    fileHash: "HASH123",
                    sequence: 1,
                    scriptText: "SELECT 1",
                    tag: "v1.0",
                    serverName: "TestServer",
                    databaseTarget: "TestDB")
            };

            var buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var initialCount = buildDataModel.CommittedScript.Count;

            // Act
            var result = _finalizer.RecordCommittedScripts(committedScripts, buildDataModel);

            // Assert
            Assert.AreEqual(initialCount + 1, result.CommittedScript.Count);
            Assert.AreEqual(scriptId.ToString(), result.CommittedScript[initialCount].ScriptId);
            Assert.AreEqual("TestServer", result.CommittedScript[initialCount].ServerName);
            Assert.AreEqual("HASH123", result.CommittedScript[initialCount].ScriptHash);
        }

        [TestMethod]
        public void RecordCommittedScripts_WithMultipleScripts_AddsAllToModel()
        {
            // Arrange
            var committedScripts = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(Guid.NewGuid(), "HASH1", 1, "SELECT 1", "tag1", "Server1", "Db1"),
                new LoggingCommittedScript(Guid.NewGuid(), "HASH2", 2, "SELECT 2", "tag2", "Server2", "Db2"),
                new LoggingCommittedScript(Guid.NewGuid(), "HASH3", 3, "SELECT 3", "tag3", "Server3", "Db3")
            };

            var buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = _finalizer.RecordCommittedScripts(committedScripts, buildDataModel);

            // Assert
            Assert.AreEqual(3, result.CommittedScript.Count);
        }

        #endregion

        #region CalculateFinalStatus Tests

        [TestMethod]
        public void CalculateFinalStatus_AllCommitted_ReturnsCommitted()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.BUILD_COMMITTED
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_COMMITTED, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_AllTrialRolledBack_ReturnsTrialRolledBack()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL,
                BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_AnyFailedAndRolledBack_ReturnsFailedAndRolledBack()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK,
                BuildResultStatus.BUILD_COMMITTED
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_AnyFailedNoTransaction_ReturnsFailedNoTransaction()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.BUILD_FAILED_NO_TRANSACTION
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_NO_TRANSACTION, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_EmptyList_ReturnsBuildSuccessfulRolledBackForTrial()
        {
            // Arrange
            var results = new List<BuildResultStatus>();

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            // Note: Empty list returns BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL because 
            // All() returns true for empty collections
            Assert.AreEqual(BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_SingleCommitted_ReturnsCommitted()
        {
            // Arrange
            var results = new List<BuildResultStatus> { BuildResultStatus.BUILD_COMMITTED };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_COMMITTED, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_FailedAndRolledBackTakesPrecedence_OverFailedNoTransaction()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_FAILED_NO_TRANSACTION,
                BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_MixedSuccessAndTrial_ReturnsUnknown()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.UNKNOWN, result);
        }

        #endregion

        #region RecordCommittedScripts Edge Cases

        [TestMethod]
        public void RecordCommittedScripts_WithNullBuildDataModel_ThrowsNullReferenceException()
        {
            // Arrange
            var committedScripts = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(Guid.NewGuid(), "HASH1", 1, "SELECT 1", "tag1", "Server1", "Db1")
            };

            // Act & Assert - There's a bug in the implementation where it uses buildDataModel instead of model
            Assert.ThrowsException<NullReferenceException>(() => 
                _finalizer.RecordCommittedScripts(committedScripts, null));
        }

        [TestMethod]
        public void RecordCommittedScripts_PreservesExistingCommittedScripts()
        {
            // Arrange
            var existingScript = new CommittedScript(
                Guid.NewGuid().ToString(),
                "ExistingServer",
                DateTime.Now,
                true,
                "EXISTINGHASH",
                0);
            var buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            buildDataModel.CommittedScript = new List<CommittedScript> { existingScript };

            var newScripts = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(Guid.NewGuid(), "NEWHASH", 1, "SELECT 1", "tag1", "NewServer", "NewDb")
            };

            // Act
            var result = _finalizer.RecordCommittedScripts(newScripts, buildDataModel);

            // Assert
            Assert.AreEqual(2, result.CommittedScript.Count);
            Assert.AreEqual("ExistingServer", result.CommittedScript[0].ServerName);
            Assert.AreEqual("NewServer", result.CommittedScript[1].ServerName);
        }

        [TestMethod]
        public void RecordCommittedScripts_SetsCommitDateToNow()
        {
            // Arrange
            var before = DateTime.Now;
            var committedScripts = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(Guid.NewGuid(), "HASH1", 1, "SELECT 1", "tag1", "Server1", "Db1")
            };
            var buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = _finalizer.RecordCommittedScripts(committedScripts, buildDataModel);
            var after = DateTime.Now;

            // Assert
            Assert.IsTrue(result.CommittedScript[0].CommittedDate >= before);
            Assert.IsTrue(result.CommittedScript[0].CommittedDate <= after);
        }

        [TestMethod]
        public void RecordCommittedScripts_SetsAllowScriptBlockTrueForAll()
        {
            // Arrange
            var committedScripts = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(Guid.NewGuid(), "HASH1", 1, "SELECT 1", "tag1", "Server1", "Db1"),
                new LoggingCommittedScript(Guid.NewGuid(), "HASH2", 2, "SELECT 2", "tag2", "Server2", "Db2")
            };
            var buildDataModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = _finalizer.RecordCommittedScripts(committedScripts, buildDataModel);

            // Assert
            Assert.IsTrue(result.CommittedScript.All(cs => cs.AllowScriptBlock == true));
        }

        #endregion

        #region CalculateFinalStatus Edge Cases

        [TestMethod]
        public void CalculateFinalStatus_ScriptGenerationComplete_ReturnsSameStatus()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.SCRIPT_GENERATION_COMPLETE,
                BuildResultStatus.SCRIPT_GENERATION_COMPLETE
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.SCRIPT_GENERATION_COMPLETE, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_AllUnknown_ReturnsUnknown()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.UNKNOWN,
                BuildResultStatus.UNKNOWN
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.UNKNOWN, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_FailedAndRolledBackWithTrialRolledBack_ReturnsFailedAndRolledBack()
        {
            // Arrange - failed should take precedence
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL,
                BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK,
                BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_NullList_ThrowsOrHandlesGracefully()
        {
            // Arrange
            List<BuildResultStatus> results = null;

            // Act & Assert - should throw NullReferenceException since .All() is called
            Assert.ThrowsException<NullReferenceException>(() => _finalizer.CalculateFinalStatus(results));
        }

        [TestMethod]
        public void CalculateFinalStatus_SingleFailed_ReturnsFailed()
        {
            // Arrange
            var results = new List<BuildResultStatus> { BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_SingleTrialRolledBack_ReturnsTrialRolledBack()
        {
            // Arrange
            var results = new List<BuildResultStatus> { BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_LargeListAllCommitted_ReturnsCommitted()
        {
            // Arrange
            var results = Enumerable.Repeat(BuildResultStatus.BUILD_COMMITTED, 100).ToList();

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_COMMITTED, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_MixedWithOneFailedNoTransaction_ReturnsFailedNoTransaction()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.BUILD_FAILED_NO_TRANSACTION
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_NO_TRANSACTION, result);
        }

        #endregion

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullSqlLoggingService_CreatesInstance()
        {
            // Arrange & Act - testing that constructor doesn't throw
            var finalizer = new DefaultBuildFinalizer(null, _mockProgressReporter.Object);

            // Assert
            Assert.IsNotNull(finalizer);
        }

        [TestMethod]
        public void Constructor_WithNullProgressReporter_CreatesInstance()
        {
            // Arrange & Act
            var finalizer = new DefaultBuildFinalizer(_mockSqlLoggingService.Object, null);

            // Assert
            Assert.IsNotNull(finalizer);
        }

        #endregion
    }
}
