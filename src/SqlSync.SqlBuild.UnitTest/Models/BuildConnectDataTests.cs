using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;

namespace SqlSync.SqlBuild.UnitTest.Models
{
    [TestClass]
    public class BuildConnectDataTests
    {
        [TestMethod]
        public void DefaultConstructor_InitializesWithDefaultValues()
        {
            // Act
            var data = new BuildConnectData();

            // Assert
            Assert.IsNotNull(data);
            Assert.IsNull(data.Connection);
            Assert.IsNull(data.Transaction);
            Assert.AreEqual(string.Empty, data.DatabaseName);
            Assert.AreEqual(string.Empty, data.ServerName);
            Assert.IsFalse(data.HasLoggingTable);
        }

        [TestMethod]
        public void DatabaseName_CanBeSetAndRetrieved()
        {
            // Arrange
            var data = new BuildConnectData();

            // Act
            data.DatabaseName = "TestDatabase";

            // Assert
            Assert.AreEqual("TestDatabase", data.DatabaseName);
        }

        [TestMethod]
        public void ServerName_CanBeSetAndRetrieved()
        {
            // Arrange
            var data = new BuildConnectData();

            // Act
            data.ServerName = "SQLSERVER01";

            // Assert
            Assert.AreEqual("SQLSERVER01", data.ServerName);
        }

        [TestMethod]
        public void HasLoggingTable_CanBeSetToTrue()
        {
            // Arrange
            var data = new BuildConnectData();

            // Act
            data.HasLoggingTable = true;

            // Assert
            Assert.IsTrue(data.HasLoggingTable);
        }

        [TestMethod]
        public void HasLoggingTable_DefaultsToFalse()
        {
            // Arrange & Act
            var data = new BuildConnectData();

            // Assert
            Assert.IsFalse(data.HasLoggingTable);
        }

        [TestMethod]
        public void AllProperties_CanBeSetTogether()
        {
            // Arrange & Act
            var data = new BuildConnectData
            {
                DatabaseName = "ProductionDb",
                ServerName = "PROD-SQL",
                HasLoggingTable = true
            };

            // Assert
            Assert.AreEqual("ProductionDb", data.DatabaseName);
            Assert.AreEqual("PROD-SQL", data.ServerName);
            Assert.IsTrue(data.HasLoggingTable);
        }

        [TestMethod]
        public void Connection_CanBeSetToNull()
        {
            // Arrange
            var data = new BuildConnectData();

            // Act
            data.Connection = null!;

            // Assert
            Assert.IsNull(data.Connection);
        }

        [TestMethod]
        public void Transaction_CanBeSetToNull()
        {
            // Arrange
            var data = new BuildConnectData();

            // Act
            data.Transaction = null!;

            // Assert
            Assert.IsNull(data.Transaction);
        }

        [TestMethod]
        public void DatabaseName_EmptyString_IsValid()
        {
            // Arrange
            var data = new BuildConnectData();

            // Act
            data.DatabaseName = string.Empty;

            // Assert
            Assert.AreEqual(string.Empty, data.DatabaseName);
        }

        [TestMethod]
        public void ServerName_EmptyString_IsValid()
        {
            // Arrange
            var data = new BuildConnectData();

            // Act
            data.ServerName = string.Empty;

            // Assert
            Assert.AreEqual(string.Empty, data.ServerName);
        }

        [TestMethod]
        public void DatabaseName_SpecialCharacters_ArePreserved()
        {
            // Arrange
            var data = new BuildConnectData();

            // Act
            data.DatabaseName = "Test-Database_123.Prod";

            // Assert
            Assert.AreEqual("Test-Database_123.Prod", data.DatabaseName);
        }

        [TestMethod]
        public void ServerName_WithInstanceName_IsPreserved()
        {
            // Arrange
            var data = new BuildConnectData();

            // Act
            data.ServerName = "SERVER01\\SQLINSTANCE";

            // Assert
            Assert.AreEqual("SERVER01\\SQLINSTANCE", data.ServerName);
        }

        [TestMethod]
        public void ServerName_WithPort_IsPreserved()
        {
            // Arrange
            var data = new BuildConnectData();

            // Act
            data.ServerName = "SERVER01,1433";

            // Assert
            Assert.AreEqual("SERVER01,1433", data.ServerName);
        }
    }
}
