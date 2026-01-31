using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class DefaultSqlLoggingServiceTests
    {
        private Mock<IConnectionsService> _mockConnectionsService;
        private Mock<IProgressReporter> _mockProgressReporter;

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
        public async Task EnsureLogTablePresence_WithNullDictionary_ThrowsNullReferenceException()
        {
            // Arrange
            var service = new DefaultSqlLoggingService(
                _mockConnectionsService.Object,
                _mockProgressReporter.Object);

            // Act & Assert
            Assert.ThrowsException<NullReferenceException>(async () => 
                await service.EnsureLogTablePresence(null, string.Empty));
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
    }
}
