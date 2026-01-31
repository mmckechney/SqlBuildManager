using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.Connection;
using SqlBuildManager.Interfaces.Console;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
using CommittedScript = SqlSync.SqlBuild.Models.CommittedScript;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    /// <summary>
    /// Final coverage tests for DefaultBuildFinalizer - testing SaveBuildDataModel and PerformRunScriptFinalization
    /// </summary>
    [TestClass]
    public class DefaultBuildFinalizerFinalTests
    {
        private Mock<ISqlLoggingService> _mockSqlLoggingService;
        private Mock<IProgressReporter> _mockProgressReporter;
        private DefaultBuildFinalizer _finalizer;
        private string _testDir;

        [TestInitialize]
        public void Setup()
        {
            _mockSqlLoggingService = new Mock<ISqlLoggingService>();
            _mockProgressReporter = new Mock<IProgressReporter>();
            _finalizer = new DefaultBuildFinalizer(_mockSqlLoggingService.Object, _mockProgressReporter.Object);
            
            _testDir = Path.Combine(Path.GetTempPath(), $"FinalizerTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (Directory.Exists(_testDir))
                    Directory.Delete(_testDir, true);
            }
            catch { }
        }

        #region SaveBuildDataModel Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SaveBuildDataModel_WithNullProjectFileName_ThrowsArgumentException()
        {
            // Arrange
            var mockContext = new Mock<ISqlBuildRunnerProperties>();
            mockContext.Setup(x => x.ProjectFileName).Returns((string)null);
            mockContext.Setup(x => x.BuildDataModel).Returns(SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel());

            // Act
            await _finalizer.SaveBuildDataModel(mockContext.Object, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SaveBuildDataModel_WithEmptyProjectFileName_ThrowsArgumentException()
        {
            // Arrange
            var mockContext = new Mock<ISqlBuildRunnerProperties>();
            mockContext.Setup(x => x.ProjectFileName).Returns(string.Empty);
            mockContext.Setup(x => x.BuildDataModel).Returns(SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel());

            // Act
            await _finalizer.SaveBuildDataModel(mockContext.Object, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SaveBuildDataModel_WithNullBuildHistoryXmlFile_ThrowsArgumentException()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "test.xml");
            string buildFileName = Path.Combine(_testDir, "test.sbm");
            
            var mockContext = new Mock<ISqlBuildRunnerProperties>();
            mockContext.Setup(x => x.ProjectFileName).Returns(projFileName);
            mockContext.Setup(x => x.BuildFileName).Returns(buildFileName);
            mockContext.Setup(x => x.BuildHistoryXmlFile).Returns((string)null);
            mockContext.Setup(x => x.BuildDataModel).Returns(SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel());

            // Act
            await _finalizer.SaveBuildDataModel(mockContext.Object, true);
        }

        [TestMethod]
        [ExpectedException(typeof(ArgumentException))]
        public async Task SaveBuildDataModel_WithEmptyBuildHistoryXmlFile_ThrowsArgumentException()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "test.xml");
            string buildFileName = Path.Combine(_testDir, "test.sbm");
            
            var mockContext = new Mock<ISqlBuildRunnerProperties>();
            mockContext.Setup(x => x.ProjectFileName).Returns(projFileName);
            mockContext.Setup(x => x.BuildFileName).Returns(buildFileName);
            mockContext.Setup(x => x.BuildHistoryXmlFile).Returns(string.Empty);
            mockContext.Setup(x => x.BuildDataModel).Returns(SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel());

            // Act
            await _finalizer.SaveBuildDataModel(mockContext.Object, true);
        }

        #endregion

        #region PerformRunScriptFinalization Tests

        [TestMethod]
        [ExpectedException(typeof(ArgumentNullException))]
        public async Task PerformRunScriptFinalization_WithNullContext_ThrowsArgumentNullException()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();
            var mockFinalizerContext = new Mock<IBuildFinalizerContext>();
            var build = new Build("Test", null, DateTime.Now, null, null, null, null, null);

            // Act
            await _finalizer.PerformRunScriptFinalizationAsync(null, mockConnectionsService.Object, mockFinalizerContext.Object, false, build);
        }

        [TestMethod]
        public async Task PerformRunScriptFinalization_WithBuildFailure_Transactional_ReturnsRolledBackStatus()
        {
            // Arrange
            var context = CreateMockContext(isTransactional: true, isTrialBuild: false);
            var mockConnectionsService = CreateMockConnectionsService();
            var mockFinalizerContext = new Mock<IBuildFinalizerContext>();
            var build = new Build("Test", "Full", DateTime.Now, null, "Server", BuildItemStatus.Pending, "BUILD1", "User");

            // Act
            var (updatedBuild, _, result) = await _finalizer.PerformRunScriptFinalizationAsync(
                context.Object, mockConnectionsService.Object, mockFinalizerContext.Object, buildFailure: true, build);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_AND_ROLLED_BACK, result);
            Assert.AreEqual(BuildItemStatus.RolledBack, updatedBuild.FinalStatus);
        }

        [TestMethod]
        public async Task PerformRunScriptFinalization_WithBuildFailure_NonTransactional_ReturnsFailedNoTransaction()
        {
            // Arrange
            var context = CreateMockContext(isTransactional: false, isTrialBuild: false);
            var mockConnectionsService = CreateMockConnectionsService();
            var mockFinalizerContext = new Mock<IBuildFinalizerContext>();
            var build = new Build("Test", "Full", DateTime.Now, null, "Server", BuildItemStatus.Pending, "BUILD1", "User");

            // Act
            var (updatedBuild, _, result) = await _finalizer.PerformRunScriptFinalizationAsync(
                context.Object, mockConnectionsService.Object, mockFinalizerContext.Object, buildFailure: true, build);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_FAILED_NO_TRANSACTION, result);
            Assert.AreEqual(BuildItemStatus.FailedNoTransaction, updatedBuild.FinalStatus);
        }

        [TestMethod]
        public async Task PerformRunScriptFinalization_WithSuccess_ReturnsCommitted()
        {
            // Arrange
            var context = CreateMockContext(isTransactional: true, isTrialBuild: false);
            var mockConnectionsService = CreateMockConnectionsService();
            var mockFinalizerContext = new Mock<IBuildFinalizerContext>();
            var build = new Build("Test", "Full", DateTime.Now, null, "Server", BuildItemStatus.Pending, "BUILD1", "User");

            // Act
            var (updatedBuild, _, result) = await _finalizer.PerformRunScriptFinalizationAsync(
                context.Object, mockConnectionsService.Object, mockFinalizerContext.Object, buildFailure: false, build);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_COMMITTED, result);
            Assert.AreEqual(BuildItemStatus.Committed, updatedBuild.FinalStatus);
        }

        [TestMethod]
        public async Task PerformRunScriptFinalization_TrialBuild_Transactional_ReturnsRolledBackForTrial()
        {
            // Arrange
            var context = CreateMockContext(isTransactional: true, isTrialBuild: true);
            var mockConnectionsService = CreateMockConnectionsService();
            var mockFinalizerContext = new Mock<IBuildFinalizerContext>();
            var build = new Build("Test", "Full", DateTime.Now, null, "Server", BuildItemStatus.Pending, "BUILD1", "User");

            // Act
            var (updatedBuild, _, result) = await _finalizer.PerformRunScriptFinalizationAsync(
                context.Object, mockConnectionsService.Object, mockFinalizerContext.Object, buildFailure: false, build);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_SUCCESSFUL_ROLLED_BACK_FOR_TRIAL, result);
            Assert.AreEqual(BuildItemStatus.TrialRolledBack, updatedBuild.FinalStatus);
        }

        [TestMethod]
        public async Task PerformRunScriptFinalization_TrialBuild_NonTransactional_ReturnsCommitted()
        {
            // Arrange
            var context = CreateMockContext(isTransactional: false, isTrialBuild: true);
            var mockConnectionsService = CreateMockConnectionsService();
            var mockFinalizerContext = new Mock<IBuildFinalizerContext>();
            var build = new Build("Test", "Full", DateTime.Now, null, "Server", BuildItemStatus.Pending, "BUILD1", "User");

            // Act
            var (updatedBuild, _, result) = await _finalizer.PerformRunScriptFinalizationAsync(
                context.Object, mockConnectionsService.Object, mockFinalizerContext.Object, buildFailure: false, build);

            // Assert
            Assert.AreEqual(BuildResultStatus.BUILD_COMMITTED, result);
            Assert.AreEqual(BuildItemStatus.Committed, updatedBuild.FinalStatus);
        }

        [TestMethod]
        public async Task PerformRunScriptFinalization_RunScriptOnly_ReturnsScriptGenerationComplete()
        {
            // Arrange
            var context = CreateMockContext(isTransactional: true, isTrialBuild: false, runScriptOnly: true);
            var mockConnectionsService = CreateMockConnectionsService();
            var mockFinalizerContext = new Mock<IBuildFinalizerContext>();
            var build = new Build("Test", "Full", DateTime.Now, null, "Server", BuildItemStatus.Pending, "BUILD1", "User");

            // Act
            var (_, _, result) = await _finalizer.PerformRunScriptFinalizationAsync(
                context.Object, mockConnectionsService.Object, mockFinalizerContext.Object, buildFailure: false, build);

            // Assert
            Assert.AreEqual(BuildResultStatus.SCRIPT_GENERATION_COMPLETE, result);
        }

        [TestMethod]
        public async Task PerformRunScriptFinalization_RaisesBuildCommittedEvent_OnSuccess()
        {
            // Arrange
            var context = CreateMockContext(isTransactional: true, isTrialBuild: false);
            var mockConnectionsService = CreateMockConnectionsService();
            var mockFinalizerContext = new Mock<IBuildFinalizerContext>();
            var build = new Build("Test", "Full", DateTime.Now, null, "Server", BuildItemStatus.Pending, "BUILD1", "User");

            // Act
            await _finalizer.PerformRunScriptFinalizationAsync(
                context.Object, mockConnectionsService.Object, mockFinalizerContext.Object, buildFailure: false, build);

            // Assert
            mockFinalizerContext.Verify(x => x.RaiseBuildCommittedEvent(It.IsAny<ISqlBuildRunnerProperties>(), RunnerReturn.BuildCommitted), Times.Once);
        }

        [TestMethod]
        public async Task PerformRunScriptFinalization_RaisesBuildErrorRollBackEvent_OnFailure()
        {
            // Arrange
            var context = CreateMockContext(isTransactional: true, isTrialBuild: false);
            var mockConnectionsService = CreateMockConnectionsService();
            var mockFinalizerContext = new Mock<IBuildFinalizerContext>();
            var build = new Build("Test", "Full", DateTime.Now, null, "Server", BuildItemStatus.Pending, "BUILD1", "User");

            // Act
            await _finalizer.PerformRunScriptFinalizationAsync(
                context.Object, mockConnectionsService.Object, mockFinalizerContext.Object, buildFailure: true, build);

            // Assert
            mockFinalizerContext.Verify(x => x.RaiseBuildErrorRollBackEvent(It.IsAny<ISqlBuildRunnerProperties>()), Times.Once);
        }

        [TestMethod]
        public async Task PerformRunScriptFinalization_ClearsConnections()
        {
            // Arrange
            var context = CreateMockContext(isTransactional: true, isTrialBuild: false);
            var mockConnectionsService = CreateMockConnectionsService();
            var mockFinalizerContext = new Mock<IBuildFinalizerContext>();
            var build = new Build("Test", "Full", DateTime.Now, null, "Server", BuildItemStatus.Pending, "BUILD1", "User");

            // Act
            await _finalizer.PerformRunScriptFinalizationAsync(
                context.Object, mockConnectionsService.Object, mockFinalizerContext.Object, buildFailure: false, build);

            // Assert - Verify connections were cleared (by checking it was accessed)
            mockConnectionsService.Verify(x => x.Connections, Times.AtLeastOnce);
        }

        #endregion

        #region CommitBuild Edge Cases

        [TestMethod]
        public void CommitBuild_WithEmptyConnections_ReturnsTrue()
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

        #endregion

        #region RollbackBuild Edge Cases

        [TestMethod]
        public void RollbackBuild_WithEmptyConnections_ReturnsTrue()
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

        #endregion

        #region RecordCommittedScripts Edge Cases

        [TestMethod]
        public void RecordCommittedScripts_WithMultipleScripts_RecordsAll()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var scripts = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(Guid.NewGuid(), "HASH1", 1, "SELECT 1", "tag1", "Server1", "Db1"),
                new LoggingCommittedScript(Guid.NewGuid(), "HASH2", 2, "SELECT 2", "tag2", "Server2", "Db2"),
                new LoggingCommittedScript(Guid.NewGuid(), "HASH3", 3, "SELECT 3", "tag3", "Server3", "Db3")
            };

            // Act
            var result = _finalizer.RecordCommittedScripts(scripts, model);

            // Assert
            Assert.AreEqual(3, result.CommittedScript.Count);
        }

        [TestMethod]
        public void RecordCommittedScripts_SetsAllowScriptBlockToTrue()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var scripts = new List<LoggingCommittedScript>
            {
                new LoggingCommittedScript(Guid.NewGuid(), "HASH1", 1, "SELECT 1", "tag1", "Server1", "Db1")
            };

            // Act
            var result = _finalizer.RecordCommittedScripts(scripts, model);

            // Assert
            Assert.IsTrue(result.CommittedScript[0].AllowScriptBlock);
        }

        #endregion

        #region Helper Methods

        private Mock<ISqlBuildRunnerProperties> CreateMockContext(bool isTransactional, bool isTrialBuild, bool runScriptOnly = false)
        {
            string projFileName = Path.Combine(_testDir, "project.xml");
            string buildFileName = Path.Combine(_testDir, "build.sbm");
            string historyFile = Path.Combine(_testDir, "history.xml");

            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var mock = new Mock<ISqlBuildRunnerProperties>();
            mock.Setup(x => x.IsTransactional).Returns(isTransactional);
            mock.Setup(x => x.IsTrialBuild).Returns(isTrialBuild);
            mock.Setup(x => x.RunScriptOnly).Returns(runScriptOnly);
            mock.Setup(x => x.ProjectFileName).Returns(projFileName);
            mock.Setup(x => x.BuildFileName).Returns(buildFileName);
            mock.Setup(x => x.BuildHistoryXmlFile).Returns(historyFile);
            mock.Setup(x => x.BuildDataModel).Returns(model);
            mock.Setup(x => x.BuildPackageHash).Returns("TESTHASH");
            mock.Setup(x => x.CommittedScripts).Returns(new List<LoggingCommittedScript>());
            mock.Setup(x => x.MultiDbRunData).Returns(new MultiDbData());
            mock.Setup(x => x.ConnectionData).Returns(new ConnectionData());

            return mock;
        }

        private Mock<IConnectionsService> CreateMockConnectionsService()
        {
            var connections = new Dictionary<string, BuildConnectData>();
            var mock = new Mock<IConnectionsService>();
            mock.Setup(x => x.Connections).Returns(connections);
            return mock;
        }

        #endregion
    }
}
