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
    /// <summary>
    /// Extended tests for DefaultBuildFinalizer to improve code coverage
    /// </summary>
    [TestClass]
    public class DefaultBuildFinalizerCoverageTests
    {
        private Mock<ISqlLoggingService> _mockSqlLoggingService;
        private Mock<IProgressReporter> _mockProgressReporter;
        private DefaultBuildFinalizer _finalizer;

        [TestInitialize]
        public void Setup()
        {
            _mockSqlLoggingService = new Mock<ISqlLoggingService>();
            _mockProgressReporter = new Mock<IProgressReporter>();
            _finalizer = new DefaultBuildFinalizer(_mockSqlLoggingService.Object, _mockProgressReporter.Object);
        }

        #region RecordCommittedScripts Extended Tests

        [TestMethod]
        public void RecordCommittedScripts_WithEmptyProjectList_UsesZeroProjectId()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>(),
                codeReview: new List<CodeReview>());

            var committedScripts = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(Guid.NewGuid(), "HASH1", 1, "SELECT 1", "tag1", "Server1", "Db1")
            };

            // Act
            var result = _finalizer.RecordCommittedScripts(committedScripts, model);

            // Assert
            Assert.AreEqual(1, result.CommittedScript.Count);
            Assert.AreEqual(0, result.CommittedScript[0].SqlSyncBuildProjectId);
        }

        [TestMethod]
        public void RecordCommittedScripts_WithExistingProjectId_UsesCorrectProjectId()
        {
            // Arrange
            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject> 
                { 
                    new SqlSyncBuildProject(42, "TestProject", false) 
                },
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript>(),
                codeReview: new List<CodeReview>());

            var committedScripts = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(Guid.NewGuid(), "HASH1", 1, "SELECT 1", "tag1", "Server1", "Db1")
            };

            // Act
            var result = _finalizer.RecordCommittedScripts(committedScripts, model);

            // Assert
            Assert.AreEqual(1, result.CommittedScript.Count);
            Assert.AreEqual(42, result.CommittedScript[0].SqlSyncBuildProjectId);
        }

        [TestMethod]
        public void RecordCommittedScripts_PreservesAllExistingProperties()
        {
            // Arrange
            var existingScript = new CommittedScript(
                scriptId: Guid.NewGuid().ToString(),
                serverName: "OldServer",
                committedDate: new DateTime(2020, 1, 1),
                allowScriptBlock: false,
                scriptHash: "OLDHASH",
                sqlSyncBuildProjectId: 99);

            var model = new SqlSyncBuildDataModel(
                sqlSyncBuildProject: new List<SqlSyncBuildProject>(),
                script: new List<Script>(),
                build: new List<Build>(),
                scriptRun: new List<ScriptRun>(),
                committedScript: new List<CommittedScript> { existingScript },
                codeReview: new List<CodeReview>());

            var newScript = new LoggingCommittedScript(Guid.NewGuid(), "NEWHASH", 1, "SELECT 1", "tag", "NewServer", "NewDb");

            // Act
            var result = _finalizer.RecordCommittedScripts(new List<LoggingCommittedScript> { newScript }, model);

            // Assert
            Assert.AreEqual(2, result.CommittedScript.Count);
            
            // Verify old script is unchanged
            var oldResult = result.CommittedScript[0];
            Assert.AreEqual("OldServer", oldResult.ServerName);
            Assert.AreEqual("OLDHASH", oldResult.ScriptHash);
            Assert.AreEqual(99, oldResult.SqlSyncBuildProjectId);
            Assert.AreEqual(new DateTime(2020, 1, 1), oldResult.CommittedDate);
            Assert.IsFalse(oldResult.AllowScriptBlock);
        }

        [TestMethod]
        public void RecordCommittedScripts_WithLargeNumberOfScripts_HandlesAllCorrectly()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var committedScripts = Enumerable.Range(1, 100)
                .Select(i => new LoggingCommittedScript(
                    Guid.NewGuid(), $"HASH{i}", i, $"SELECT {i}", $"tag{i}", $"Server{i}", $"Db{i}"))
                .ToList();

            // Act
            var result = _finalizer.RecordCommittedScripts(committedScripts, model);

            // Assert
            Assert.AreEqual(100, result.CommittedScript.Count);
        }

        #endregion

        #region CalculateFinalStatus Extended Edge Cases

        [TestMethod]
        public void CalculateFinalStatus_MixedStatusWithNoFailures_ReturnsUnknown()
        {
            // Arrange - multiple different non-failure statuses
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.SCRIPT_GENERATION_COMPLETE,
                BuildResultStatus.BUILD_COMMITTED
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.UNKNOWN, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_AllScriptGenerationComplete_ReturnsScriptGenerationComplete()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.SCRIPT_GENERATION_COMPLETE,
                BuildResultStatus.SCRIPT_GENERATION_COMPLETE,
                BuildResultStatus.SCRIPT_GENERATION_COMPLETE
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.SCRIPT_GENERATION_COMPLETE, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_SingleFailedNoTransaction_ReturnsFailedNoTransaction()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_FAILED_NO_TRANSACTION
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_NO_TRANSACTION, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_AllCancelledAndRolledBack_ReturnsCancelledRolledBack()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK,
                BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_FailedRolledBackTakesPrecedenceOverCancelled()
        {
            // Arrange
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK,
                BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK,
                BuildResultStatus.BUILD_COMMITTED
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK, result);
        }

        #endregion

        #region CommitBuild Extended Tests

        [TestMethod]
        public void CommitBuild_WithNullConnections_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();
            mockConnectionsService.Setup(x => x.Connections).Returns((Dictionary<string, BuildConnectData>)null);

            // Act & Assert
            Assert.ThrowsException<NullReferenceException>(() =>
                _finalizer.CommitBuild(mockConnectionsService.Object, isTransactional: true));
        }

        [TestMethod]
        public void CommitBuild_TransactionalFalse_DoesNotAccessConnections()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();

            // Act
            var result = _finalizer.CommitBuild(mockConnectionsService.Object, isTransactional: false);

            // Assert
            Assert.IsTrue(result);
            mockConnectionsService.Verify(x => x.Connections, Times.Never);
        }

        #endregion

        #region RollbackBuild Extended Tests

        [TestMethod]
        public void RollbackBuild_TransactionalFalse_LogsWarningAndReturnsFalse()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();

            // Act
            var result = _finalizer.RollbackBuild(mockConnectionsService.Object, isTransactional: false);

            // Assert
            Assert.IsFalse(result);
            // No connections should be accessed
            mockConnectionsService.Verify(x => x.Connections, Times.Never);
        }

        [TestMethod]
        public void RollbackBuild_WithNullConnections_ThrowsNullReferenceException()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();
            mockConnectionsService.Setup(x => x.Connections).Returns((Dictionary<string, BuildConnectData>)null);

            // Act & Assert
            Assert.ThrowsException<NullReferenceException>(() =>
                _finalizer.RollbackBuild(mockConnectionsService.Object, isTransactional: true));
        }

        #endregion

        #region Build Object Tests

        [TestMethod]
        public void Build_Constructor_SetsAllPropertiesCorrectly()
        {
            // Arrange
            var buildStart = DateTime.Now;
            var buildEnd = buildStart.AddMinutes(5);

            // Act
            var build = new Build(
                name: "TestBuild",
                buildType: "FullBuild",
                buildStart: buildStart,
                buildEnd: buildEnd,
                serverName: "TestServer",
                finalStatus: BuildItemStatus.Committed,
                buildId: "BUILD123",
                userId: "TestUser");

            // Assert
            Assert.AreEqual("TestBuild", build.Name);
            Assert.AreEqual("FullBuild", build.BuildType);
            Assert.AreEqual(buildStart, build.BuildStart);
            Assert.AreEqual(buildEnd, build.BuildEnd);
            Assert.AreEqual("TestServer", build.ServerName);
            Assert.AreEqual(BuildItemStatus.Committed, build.FinalStatus);
            Assert.AreEqual("BUILD123", build.BuildId);
            Assert.AreEqual("TestUser", build.UserId);
        }

        #endregion

        #region Progress Reporter Tests

        [TestMethod]
        public void CommitBuild_WithProgressReporter_DoesNotReportOnSuccess()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();
            mockConnectionsService.Setup(x => x.Connections)
                .Returns(new Dictionary<string, BuildConnectData>());

            // Act
            _finalizer.CommitBuild(mockConnectionsService.Object, isTransactional: true);

            // Assert - progress should not be reported for successful empty commits
            _mockProgressReporter.Verify(
                x => x.ReportProgress(It.IsAny<int>(), It.IsAny<object>()), 
                Times.Never);
        }

        #endregion

        #region Null Safety Tests

        [TestMethod]
        public void Constructor_BothNull_CreatesInstance()
        {
            // Act
            var finalizer = new DefaultBuildFinalizer(null, null);

            // Assert
            Assert.IsNotNull(finalizer);
        }

        [TestMethod]
        public void RecordCommittedScripts_BothNull_ThrowsNullReferenceException()
        {
            // Act & Assert
            Assert.ThrowsException<NullReferenceException>(() =>
                _finalizer.RecordCommittedScripts(new List<LoggingCommittedScript>(), null));
        }

        #endregion

        #region CalculateFinalStatus Order of Precedence Tests

        [TestMethod]
        public void CalculateFinalStatus_FailedRolledBack_AlwaysWins()
        {
            // Arrange - all possible statuses with failed rolled back
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL,
                BuildResultStatus.SCRIPT_GENERATION_COMPLETE,
                BuildResultStatus.BUILD_FAILED_NO_TRANSACTION,
                BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK,
                BuildResultStatus.BUILD_CANCELLED_AND_ROLLED_BACK,
                BuildResultStatus.UNKNOWN
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK, result);
        }

        [TestMethod]
        public void CalculateFinalStatus_FailedNoTransaction_WinsOverNonFailures()
        {
            // Arrange - various statuses without rolled back failure
            var results = new List<BuildResultStatus>
            {
                BuildResultStatus.BUILD_COMMITTED,
                BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL,
                BuildResultStatus.BUILD_FAILED_NO_TRANSACTION,
                BuildResultStatus.SCRIPT_GENERATION_COMPLETE
            };

            // Act
            var result = _finalizer.CalculateFinalStatus(results);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_NO_TRANSACTION, result);
        }

        #endregion
    }
}
