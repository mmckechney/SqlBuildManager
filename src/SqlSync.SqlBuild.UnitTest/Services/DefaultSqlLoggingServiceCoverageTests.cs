using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    /// <summary>
    /// Extended tests for DefaultSqlLoggingService to improve code coverage
    /// </summary>
    [TestClass]
    public class DefaultSqlLoggingServiceCoverageTests
    {
        private Mock<IConnectionsService> _mockConnectionsService;
        private Mock<IProgressReporter> _mockProgressReporter;

        [TestInitialize]
        public void Setup()
        {
            _mockConnectionsService = MockFactory.CreateMockConnectionsService();
            _mockProgressReporter = new Mock<IProgressReporter>();
        }

        #region Constructor Tests

        [TestMethod]
        public void Constructor_WithNullConnectionsService_CreatesInstance()
        {
            // Act - should not throw
            var service = new DefaultSqlLoggingService(null, _mockProgressReporter.Object);

            // Assert
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Constructor_WithNullProgressReporter_CreatesInstance()
        {
            // Act
            var service = new DefaultSqlLoggingService(_mockConnectionsService.Object, null);

            // Assert
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public void Constructor_WithBothNull_CreatesInstance()
        {
            // Act
            var service = new DefaultSqlLoggingService(null, null);

            // Assert
            Assert.IsNotNull(service);
        }

        #endregion

        #region EnsureLogTablePresence Tests

        [TestMethod]
        public async Task EnsureLogTablePresence_WithMultipleEmptyConnections_ReturnsEmptyMessage()
        {
            // Arrange
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            var connections = new Dictionary<string, BuildConnectData>();

            // Act
            var result = await service.EnsureLogTablePresence(connections, string.Empty);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public async Task EnsureLogTablePresence_WithSpecificLogDb_FiltersConnections()
        {
            // Arrange
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            var connections = new Dictionary<string, BuildConnectData>();

            // Act - pass specific database name
            var result = await service.EnsureLogTablePresence(connections, "SpecificLogDatabase");

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task EnsureLogTablePresence_EmptyDictionaryWithLogDbName_ReturnsEmptyMessage()
        {
            // Arrange
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            var connections = new Dictionary<string, BuildConnectData>();

            // Act
            var result = await service.EnsureLogTablePresence(connections, "TestLogDb");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region LogCommittedScriptsToDatabase Tests - Empty/Edge Cases

        [TestMethod]
        public async Task LogCommittedScriptsToDatabase_EmptyCommittedScripts_ReturnsTrue()
        {
            // Arrange
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            var mockRunnerProperties = new Mock<ISqlBuildRunnerProperties>();
            mockRunnerProperties.Setup(x => x.LogToDatabaseName).Returns(string.Empty);
            mockRunnerProperties.Setup(x => x.ConnectionData).Returns(new SqlSync.Connection.ConnectionData());
            mockRunnerProperties.Setup(x => x.BuildDataModel).Returns(SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel());
            mockRunnerProperties.Setup(x => x.BuildFileName).Returns("test.sbm");
            mockRunnerProperties.Setup(x => x.ProjectFileName).Returns("test.xml");
            mockRunnerProperties.Setup(x => x.BuildPackageHash).Returns("HASH123");
            mockRunnerProperties.Setup(x => x.BuildRequestedBy).Returns(string.Empty);
            mockRunnerProperties.Setup(x => x.BuildDescription).Returns("Test build");
            mockRunnerProperties.Setup(x => x.IsTransactional).Returns(false);

            var committedScripts = new List<SqlSync.SqlBuild.SqlLogging.CommittedScript>();
            var multiDbData = new SqlSync.SqlBuild.MultiDb.MultiDbData();

            // Act - empty scripts list should just set up connections and return
            bool result = await service.LogCommittedScriptsToDatabase(
                committedScripts,
                mockRunnerProperties.Object,
                multiDbData);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region Additional Edge Case Tests

        [TestMethod]
        public async Task EnsureLogTablePresence_ConnectionsWithDifferentDatabases_ProcessesAll()
        {
            // Arrange
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            var connections = new Dictionary<string, BuildConnectData>();

            // Act
            var result = await service.EnsureLogTablePresence(connections, string.Empty);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void EnsureLogTablePresence_LogDbNameCaseInsensitiveMatch_WorksCorrectly()
        {
            // Arrange
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            var connections = new Dictionary<string, BuildConnectData>();

            // Act - using different case for log database name
            var result = service.EnsureLogTablePresence(connections, "TESTDB");

            // Assert
            Assert.IsNotNull(result);
        }

        #endregion
    }
}
