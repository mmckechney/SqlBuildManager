using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.CodeTable;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest.MultiDb
{
    /// <summary>
    /// Comprehensive tests for MultiDbData and related classes
    /// </summary>
    [TestClass]
    public class MultiDbDataFinalTests
    {
        #region MultiDbData Tests

        [TestMethod]
        public void MultiDbData_DefaultConstructor_SetsDefaultValues()
        {
            // Act
            var data = new MultiDbData();

            // Assert
            Assert.AreEqual(0, data.AllowableTimeoutRetries);
            Assert.IsTrue(data.IsTransactional);
            Assert.IsTrue(data.RunAsTrial);
            Assert.AreEqual(string.Empty, data.BuildFileName);
            Assert.AreEqual(string.Empty, data.ProjectFileName);
            Assert.IsNull(data.BuildData);
            Assert.AreEqual(string.Empty, data.MultiRunId);
            Assert.AreEqual(string.Empty, data.UserName);
            Assert.AreEqual(string.Empty, data.Password);
            Assert.AreEqual(AuthenticationType.Password, data.AuthenticationType);
            Assert.AreEqual(string.Empty, data.BuildRevision);
        }

        [TestMethod]
        public void MultiDbData_CanSetAllProperties()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var data = new MultiDbData
            {
                AllowableTimeoutRetries = 3,
                IsTransactional = false,
                RunAsTrial = false,
                BuildFileName = "test.sbm",
                ProjectFileName = "test.xml",
                BuildData = model,
                MultiRunId = "RUN123",
                UserName = "testuser",
                Password = "testpass",
                AuthenticationType = AuthenticationType.AzureADIntegrated,
                BuildRevision = "1.0.0"
            };

            // Assert
            Assert.AreEqual(3, data.AllowableTimeoutRetries);
            Assert.IsFalse(data.IsTransactional);
            Assert.IsFalse(data.RunAsTrial);
            Assert.AreEqual("test.sbm", data.BuildFileName);
            Assert.AreEqual("test.xml", data.ProjectFileName);
            Assert.IsNotNull(data.BuildData);
            Assert.AreEqual("RUN123", data.MultiRunId);
            Assert.AreEqual("testuser", data.UserName);
            Assert.AreEqual("testpass", data.Password);
            Assert.AreEqual(AuthenticationType.AzureADIntegrated, data.AuthenticationType);
            Assert.AreEqual("1.0.0", data.BuildRevision);
        }

        [TestMethod]
        public void MultiDbData_BuildDescription_CanBeSetAndGet()
        {
            // Arrange
            var data = new MultiDbData();

            // Act
            data.BuildDescription = "Test build description";

            // Assert
            Assert.AreEqual("Test build description", data.BuildDescription);
        }

        [TestMethod]
        public void MultiDbData_InheritsFromList()
        {
            // Arrange
            var data = new MultiDbData();

            // Act
            data.Add(new ServerData { ServerName = "Server1" });
            data.Add(new ServerData { ServerName = "Server2" });

            // Assert
            Assert.AreEqual(2, data.Count);
            Assert.IsInstanceOfType(data, typeof(List<ServerData>));
        }

        #endregion

        #region ServerData Tests

        [TestMethod]
        public void ServerData_DefaultConstructor_SetsDefaults()
        {
            // Act
            var data = new ServerData();

            // Assert
            Assert.AreEqual(string.Empty, data.ServerName);
            Assert.IsNotNull(data.Overrides);
            Assert.IsNull(data.SequenceId);
        }

        [TestMethod]
        public void ServerData_CanSetProperties()
        {
            // Act
            var data = new ServerData
            {
                ServerName = "TestServer",
                SequenceId = 42,
                Overrides = new DbOverrides()
            };

            // Assert
            Assert.AreEqual("TestServer", data.ServerName);
            Assert.AreEqual(42, data.SequenceId);
            Assert.IsNotNull(data.Overrides);
        }

        [TestMethod]
        public void ServerData_Equals_WithSameServerAndOverrides_ReturnsTrue()
        {
            // Arrange
            var data1 = new ServerData
            {
                ServerName = "Server1",
                Overrides = new DbOverrides()
            };
            var data2 = new ServerData
            {
                ServerName = "Server1",
                Overrides = new DbOverrides()
            };

            // Act
            var result = data1.Equals(data2);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ServerData_Equals_WithDifferentServerName_ReturnsFalse()
        {
            // Arrange
            var data1 = new ServerData { ServerName = "Server1" };
            var data2 = new ServerData { ServerName = "Server2" };

            // Act
            var result = data1.Equals(data2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ServerData_Equals_WithDifferentOverrides_ReturnsFalse()
        {
            // Arrange
            var data1 = new ServerData
            {
                ServerName = "Server1",
                Overrides = new DbOverrides(new DatabaseOverride("source", "Db1", "Db2"))
            };
            var data2 = new ServerData
            {
                ServerName = "Server1",
                Overrides = new DbOverrides(new DatabaseOverride("source", "Db1", "Db3"))
            };

            // Act
            var result = data1.Equals(data2);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region DbOverrides Tests

        [TestMethod]
        public void DbOverrides_DefaultConstructor_CreatesEmptyList()
        {
            // Act
            var overrides = new DbOverrides();

            // Assert
            Assert.AreEqual(0, overrides.Count);
        }

        [TestMethod]
        public void DbOverrides_ParamsConstructor_AddsOverrides()
        {
            // Arrange
            var ovr1 = new DatabaseOverride("source", "Default1", "Override1");
            var ovr2 = new DatabaseOverride("source", "Default2", "Override2");

            // Act
            var overrides = new DbOverrides(ovr1, ovr2);

            // Assert
            Assert.AreEqual(2, overrides.Count);
        }

        [TestMethod]
        public void DbOverrides_GetQueryRowData_WithEmptyList_ReturnsEmptyList()
        {
            // Arrange
            var overrides = new DbOverrides();

            // Act
            var result = overrides.GetQueryRowData("default", "override");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void DbOverrides_GetQueryRowData_WithNoMatchingOverride_ReturnsEmptyList()
        {
            // Arrange
            var overrides = new DbOverrides(new DatabaseOverride("source", "Default1", "Override1"));

            // Act
            var result = overrides.GetQueryRowData("nonexistent", "override");

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void DbOverrides_GetOverrideDatabaseNameList_ReturnsUniqueNames()
        {
            // Arrange
            var overrides = new DbOverrides(
                new DatabaseOverride("source", "Default1", "Override1"),
                new DatabaseOverride("source", "Default2", "Override1"),
                new DatabaseOverride("source", "Default3", "Override2")
            );

            // Act
            var result = overrides.GetOverrideDatabaseNameList();

            // Assert
            Assert.AreEqual(2, result.Count);
            Assert.IsTrue(result.Contains("Override1"));
            Assert.IsTrue(result.Contains("Override2"));
        }

        [TestMethod]
        public void DbOverrides_GetOverrideDatabaseNameList_IsSorted()
        {
            // Arrange
            var overrides = new DbOverrides(
                new DatabaseOverride("source", "Default1", "Zdb"),
                new DatabaseOverride("source", "Default2", "Adb"),
                new DatabaseOverride("source", "Default3", "Mdb")
            );

            // Act
            var result = overrides.GetOverrideDatabaseNameList();

            // Assert
            Assert.AreEqual("Adb", result[0]);
            Assert.AreEqual("Mdb", result[1]);
            Assert.AreEqual("Zdb", result[2]);
        }

        [TestMethod]
        public void DbOverrides_Equals_WithSameContent_ReturnsTrue()
        {
            // Arrange
            var overrides1 = new DbOverrides(new DatabaseOverride("source", "Default1", "Override1"));
            var overrides2 = new DbOverrides(new DatabaseOverride("source", "Default1", "Override1"));

            // Act
            var result = overrides1.Equals(overrides2);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DbOverrides_Equals_WithDifferentCount_ReturnsFalse()
        {
            // Arrange
            var overrides1 = new DbOverrides(new DatabaseOverride("source", "Default1", "Override1"));
            var overrides2 = new DbOverrides(
                new DatabaseOverride("source", "Default1", "Override1"),
                new DatabaseOverride("source", "Default2", "Override2")
            );

            // Act
            var result = overrides1.Equals(overrides2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DbOverrides_Equals_WithDifferentDefaultDbTarget_ReturnsFalse()
        {
            // Arrange
            var overrides1 = new DbOverrides(new DatabaseOverride("source", "Default1", "Override1"));
            var overrides2 = new DbOverrides(new DatabaseOverride("source", "Default2", "Override1"));

            // Act
            var result = overrides1.Equals(overrides2);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DbOverrides_Equals_WithDifferentOverrideDbTarget_ReturnsFalse()
        {
            // Arrange
            var overrides1 = new DbOverrides(new DatabaseOverride("source", "Default1", "Override1"));
            var overrides2 = new DbOverrides(new DatabaseOverride("source", "Default1", "Override2"));

            // Act
            var result = overrides1.Equals(overrides2);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }

    /// <summary>
    /// Tests for ScriptUpdates class
    /// </summary>
    [TestClass]
    public class ScriptUpdatesTests
    {
        [TestMethod]
        public void ScriptUpdates_CanSetAllProperties()
        {
            // Act
            var updates = new ScriptUpdates
            {
                Query = "SELECT * FROM Table",
                ShortFileName = "script.sql",
                SourceTable = "SourceTable",
                SourceDatabase = "SourceDb",
                SourceServer = "SourceServer",
                KeyCheckColumns = "Id,Name"
            };

            // Assert
            Assert.AreEqual("SELECT * FROM Table", updates.Query);
            Assert.AreEqual("script.sql", updates.ShortFileName);
            Assert.AreEqual("SourceTable", updates.SourceTable);
            Assert.AreEqual("SourceDb", updates.SourceDatabase);
            Assert.AreEqual("SourceServer", updates.SourceServer);
            Assert.AreEqual("Id,Name", updates.KeyCheckColumns);
        }

        [TestMethod]
        public void ScriptUpdates_DefaultValues_AreNull()
        {
            // Act
            var updates = new ScriptUpdates();

            // Assert
            Assert.IsNull(updates.Query);
            Assert.IsNull(updates.ShortFileName);
            Assert.IsNull(updates.SourceTable);
            Assert.IsNull(updates.SourceDatabase);
            Assert.IsNull(updates.SourceServer);
            Assert.IsNull(updates.KeyCheckColumns);
        }
    }

    /// <summary>
    /// Tests for QueryRowItem
    /// </summary>
    [TestClass]
    public class QueryRowItemTests
    {
        [TestMethod]
        public void QueryRowItem_CanSetAllProperties()
        {
            // Act
            var item = new QueryRowItem
            {
                ColumnName = "TestColumn",
                Value = "TestValue"
            };

            // Assert
            Assert.AreEqual("TestColumn", item.ColumnName);
            Assert.AreEqual("TestValue", item.Value);
        }

        [TestMethod]
        public void QueryRowItem_DefaultValues()
        {
            // Act
            var item = new QueryRowItem();

            // Assert
            Assert.IsNull(item.ColumnName);
            Assert.IsNull(item.Value);
        }
    }
}
