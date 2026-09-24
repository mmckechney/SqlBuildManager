using Microsoft.VisualStudio.TestTools.UnitTesting;
using Microsoft.Data.SqlClient;
using Moq;
using Moq.Protected;
using SqlBuildManager.Connection;
using SqlBuildManager.SqlBuild.Models;
using SqlBuildManager.SqlBuild.MultiDb;
using SqlBuildManager.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class DefaultSqlLoggingServiceTests
    {
        private Mock<IConnectionsService> _mockConnectionsService = null!;
        private Mock<IProgressReporter> _mockProgressReporter = null!;

        [TestInitialize]
        public void Setup()
        {
            _mockConnectionsService = MockFactory.CreateMockConnectionsService();
            _mockProgressReporter = new Mock<IProgressReporter>();
        }

        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Act
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            // Assert
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public async Task EnsureLogTablePresence_WithEmptyDictionary_ReturnsEmptyMessage()
        {
            // Arrange
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            var emptyConnections = new Dictionary<string, BuildConnectData>();

            // Act
            var result = await service.EnsureLogTablePresence(emptyConnections, string.Empty);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public async Task EnsureLogTablePresence_WithLogToDatabase_SkipsNonMatchingDatabases()
        {
            // Arrange
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            var connections = new Dictionary<string, BuildConnectData>();
            // No connections that match "SpecificLogDb"

            // Act
            var result = await service.EnsureLogTablePresence(connections, "SpecificLogDb");

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task EnsureLogTablePresence_WithNullDictionary_StillSucceedsIfConnectionsEmpty()
        {
            // Arrange
            // Note: With the caching optimization, we now check connectionsService.Connections first
            // If that returns an empty dictionary, the null parameter is never accessed
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            // Act - no exception expected now since we check Connections first
            var result = await service.EnsureLogTablePresence(null!, string.Empty);

            // Assert - should return empty string since there are no unconfirmed connections
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public async Task EnsureLogTablePresence_WithEmptyLogToDatabaseName_ProcessesAllConnections()
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
        [DataRow(true, "Audit")]
        [DataRow(false, "Audit")]
        [DataRow(true, "")]
        [DataRow(false, "")]
        public async Task LogCommittedScripts_UsesAutocommitAfterBuild(bool transactionalBuild, string loggingDatabase)
        {
            var connection = new Mock<DbConnection>();
            connection.SetupGet(c => c.State).Returns(ConnectionState.Open);
            using var parameterOwner = new SqlCommand();
            var command = new Mock<DbCommand>();
            command.Protected().Setup<DbParameterCollection>("DbParameterCollection")
                .Returns(parameterOwner.Parameters);
            command.Protected().Setup<DbParameter>("CreateDbParameter").Returns(() => new SqlParameter());
            command.Setup(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>())).ReturnsAsync(1);
            connection.Protected().Setup<DbCommand>("CreateDbCommand").Returns(command.Object);

            var authentication = new ConnectionData();
            string targetDatabase = string.IsNullOrEmpty(loggingDatabase) ? "Target" : loggingDatabase;
            var connectionData = new BuildConnectData
            {
                Connection = connection.Object,
                ServerName = "Server",
                DatabaseName = targetDatabase,
                HasLoggingTable = true
            };
            _mockConnectionsService.Object.Connections.Add("Server:" + targetDatabase, connectionData);
            _mockConnectionsService.Setup(c => c.GetOrAddBuildConnectionDataClass(
                authentication, "Server", targetDatabase, It.IsAny<bool>())).Returns(connectionData);

            var scriptId = Guid.NewGuid();
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model.Script.Add(new Script { ScriptId = scriptId.ToString(), FileName = "insert.sql" });
            var runner = new Mock<ISqlBuildRunnerProperties>();
            runner.SetupGet(r => r.ConnectionData).Returns(authentication);
            runner.SetupGet(r => r.LogToDatabaseName).Returns(loggingDatabase);
            runner.SetupGet(r => r.IsTransactional).Returns(transactionalBuild);
            runner.SetupGet(r => r.BuildDataModel).Returns(model);
            runner.SetupGet(r => r.BuildFileName).Returns("build.sbm");

            var service = new DefaultSqlLoggingService(_mockConnectionsService.Object, _mockProgressReporter.Object);
            var scripts = new List<SqlLogging.CommittedScript>
            {
                new SqlLogging.CommittedScript(scriptId, "hash", 1, "SELECT 1", "", "Server", "Target")
            };

            Assert.IsTrue(await service.LogCommittedScriptsToDatabase(scripts, runner.Object, new MultiDbData()));
            _mockConnectionsService.Verify(c => c.GetOrAddBuildConnectionDataClass(
                authentication, "Server", targetDatabase, false),
                Times.Exactly(string.IsNullOrEmpty(loggingDatabase) ? 1 : 2));
            _mockConnectionsService.Verify(c => c.GetOrAddBuildConnectionDataClass(
                It.IsAny<ConnectionData>(), It.IsAny<string>(), It.IsAny<string>(), true), Times.Never);
            command.Verify(c => c.ExecuteNonQueryAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
