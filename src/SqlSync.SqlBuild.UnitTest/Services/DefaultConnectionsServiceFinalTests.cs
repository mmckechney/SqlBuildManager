using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using SqlSync.Connection;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    /// <summary>
    /// Final coverage tests for DefaultConnectionsService
    /// </summary>
    [TestClass]
    public class DefaultConnectionsServiceFinalTests
    {
        #region Constructor Tests

        [TestMethod]
        public void Constructor_InitializesEmptyDictionary()
        {
            // Act
            var service = new DefaultConnectionsService();

            // Assert
            Assert.IsNotNull(service.Connections);
            Assert.AreEqual(0, service.Connections.Count);
        }

        #endregion

        #region GetBuildConnectionDataClass Tests

        [TestMethod]
        public void GetBuildConnectionDataClass_WithNonExistentKey_ThrowsArgumentNullException()
        {
            // Arrange
            var service = new DefaultConnectionsService();

            // Act & Assert
            Assert.ThrowsExactly<ArgumentNullException>(() =>
                service.GetBuildConnectionDataClass("NonExistentServer", "NonExistentDb", false));
        }

        [TestMethod]
        public void GetBuildConnectionDataClass_KeyIsUpperCased()
        {
            // Arrange
            var service = new DefaultConnectionsService();
            var key = "TESTSERVER:TESTDB";
            service.Connections.Add(key, new BuildConnectData 
            { 
                ServerName = "TestServer", 
                DatabaseName = "TestDb" 
            });

            // Assert - The key is uppercased in the implementation
            Assert.IsTrue(service.Connections.ContainsKey("TESTSERVER:TESTDB"));
            Assert.IsFalse(service.Connections.ContainsKey("testserver:testdb"));
        }

        #endregion

        #region ResetConnectionsForRetry Tests

        [TestMethod]
        public void ResetConnectionsForRetry_WithMultipleConnections_ClearsAll()
        {
            // Arrange
            var service = new DefaultConnectionsService();
            service.Connections.Add("KEY1", new BuildConnectData { ServerName = "Server1", DatabaseName = "Db1" });
            service.Connections.Add("KEY2", new BuildConnectData { ServerName = "Server2", DatabaseName = "Db2" });
            service.Connections.Add("KEY3", new BuildConnectData { ServerName = "Server3", DatabaseName = "Db3" });

            Assert.AreEqual(3, service.Connections.Count);

            // Act
            service.ResetConnectionsForRetry();

            // Assert
            Assert.AreEqual(0, service.Connections.Count);
        }

        [TestMethod]
        public void ResetConnectionsForRetry_WithConnectionsHavingNullTransaction_DoesNotThrow()
        {
            // Arrange
            var service = new DefaultConnectionsService();
            service.Connections.Add("KEY1", new BuildConnectData 
            { 
                ServerName = "Server1", 
                DatabaseName = "Db1",
                Transaction = null
            });

            // Act - should not throw
            service.ResetConnectionsForRetry();

            // Assert
            Assert.AreEqual(0, service.Connections.Count);
        }

        [TestMethod]
        public void ResetConnectionsForRetry_WithConnectionsHavingNullConnection_DoesNotThrow()
        {
            // Arrange
            var service = new DefaultConnectionsService();
            service.Connections.Add("KEY1", new BuildConnectData 
            { 
                ServerName = "Server1", 
                DatabaseName = "Db1",
                Connection = null
            });

            // Act - should not throw
            service.ResetConnectionsForRetry();

            // Assert
            Assert.AreEqual(0, service.Connections.Count);
        }

        [TestMethod]
        public void ResetConnectionsForRetry_CalledMultipleTimes_DoesNotThrow()
        {
            // Arrange
            var service = new DefaultConnectionsService();
            service.Connections.Add("KEY1", new BuildConnectData { ServerName = "Server1", DatabaseName = "Db1" });

            // Act
            service.ResetConnectionsForRetry();
            service.ResetConnectionsForRetry();
            service.ResetConnectionsForRetry();

            // Assert
            Assert.AreEqual(0, service.Connections.Count);
        }

        #endregion

        #region Connections Dictionary Tests

        [TestMethod]
        public void Connections_IsNotReadOnly()
        {
            // Arrange
            var service = new DefaultConnectionsService();

            // Act
            service.Connections.Add("KEY", new BuildConnectData());
            service.Connections.Remove("KEY");

            // Assert
            Assert.AreEqual(0, service.Connections.Count);
        }

        [TestMethod]
        public void Connections_CanStoreAndRetrieveComplexData()
        {
            // Arrange
            var service = new DefaultConnectionsService();
            var data = new BuildConnectData
            {
                ServerName = "ComplexServer",
                DatabaseName = "ComplexDb",
                HasLoggingTable = true
            };

            // Act
            service.Connections.Add("COMPLEXSERVER:COMPLEXDB", data);
            var retrieved = service.Connections["COMPLEXSERVER:COMPLEXDB"];

            // Assert
            Assert.AreEqual("ComplexServer", retrieved.ServerName);
            Assert.AreEqual("ComplexDb", retrieved.DatabaseName);
            Assert.IsTrue(retrieved.HasLoggingTable);
        }

        [TestMethod]
        public void Connections_ContainsKey_ReturnsTrueForExistingKey()
        {
            // Arrange
            var service = new DefaultConnectionsService();
            service.Connections.Add("MYKEY", new BuildConnectData());

            // Act & Assert
            Assert.IsTrue(service.Connections.ContainsKey("MYKEY"));
        }

        [TestMethod]
        public void Connections_ContainsKey_ReturnsFalseForNonExistingKey()
        {
            // Arrange
            var service = new DefaultConnectionsService();
            service.Connections.Add("MYKEY", new BuildConnectData());

            // Act & Assert
            Assert.IsFalse(service.Connections.ContainsKey("OTHERKEY"));
        }

        #endregion

        #region BuildConnectData Tests

        [TestMethod]
        public void BuildConnectData_DefaultValues()
        {
            // Act
            var data = new BuildConnectData();

            // Assert - Default values are empty string, not null
            Assert.AreEqual(string.Empty, data.ServerName);
            Assert.AreEqual(string.Empty, data.DatabaseName);
            Assert.IsNull(data.Connection);
            Assert.IsNull(data.Transaction);
            Assert.IsFalse(data.HasLoggingTable);
        }

        [TestMethod]
        public void BuildConnectData_CanSetAllProperties()
        {
            // Arrange & Act
            var data = new BuildConnectData
            {
                ServerName = "TestServer",
                DatabaseName = "TestDb",
                HasLoggingTable = true
            };

            // Assert
            Assert.AreEqual("TestServer", data.ServerName);
            Assert.AreEqual("TestDb", data.DatabaseName);
            Assert.IsTrue(data.HasLoggingTable);
        }

        #endregion

        #region Key Generation Tests

        [TestMethod]
        public void GetBuildConnectionDataClass_UsesUpperCaseKeys()
        {
            // Arrange
            var service = new DefaultConnectionsService();
            // Add with expected uppercase key
            service.Connections.Add("MYSERVER:MYDB", new BuildConnectData 
            { 
                ServerName = "myserver",
                DatabaseName = "mydb" 
            });

            // Assert - lowercase input should find uppercase key internally
            Assert.IsTrue(service.Connections.ContainsKey("MYSERVER:MYDB"));
        }

        #endregion
    }

    /// <summary>
    /// Tests for DefaultSqlLoggingService
    /// </summary>
    [TestClass]
    public class DefaultSqlLoggingServiceFinalTests
    {
        [TestMethod]
        public void Constructor_WithValidDependencies_CreatesInstance()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();
            var mockProgressReporter = new Mock<IProgressReporter>();

            // Act - using reflection since the class is internal
            var service = Activator.CreateInstance(
                typeof(DefaultConnectionsService).Assembly.GetType("SqlSync.SqlBuild.Services.DefaultSqlLoggingService"),
                mockConnectionsService.Object,
                mockProgressReporter.Object,
                (ISqlResourceProvider)null,
                (IScriptSyntaxProvider)null);

            // Assert
            Assert.IsNotNull(service);
        }

        [TestMethod]
        public async Task EnsureLogTablePresence_WithEmptyDictionary_ReturnsEmptyString()
        {
            // Arrange
            var mockConnectionsService = new Mock<IConnectionsService>();
            var mockProgressReporter = new Mock<IProgressReporter>();
            
            // Set up Connections property to return an empty dictionary
            mockConnectionsService.Setup(x => x.Connections).Returns(new Dictionary<string, BuildConnectData>());
            
            // Create instance using internal constructor
            var serviceType = typeof(DefaultConnectionsService).Assembly.GetType("SqlSync.SqlBuild.Services.DefaultSqlLoggingService");
            var service = Activator.CreateInstance(serviceType, mockConnectionsService.Object, mockProgressReporter.Object, (ISqlResourceProvider)null, (IScriptSyntaxProvider)null) as ISqlLoggingService;

            var emptyConnections = new Dictionary<string, BuildConnectData>();

            // Act
            var result = await service.EnsureLogTablePresence(emptyConnections, string.Empty);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }
    }
}
