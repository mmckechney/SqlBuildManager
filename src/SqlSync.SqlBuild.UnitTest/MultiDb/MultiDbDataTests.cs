using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlSync.SqlBuild.UnitTest.MultiDb
{
    /// <summary>
    /// Tests for MultiDbData and related classes
    /// </summary>
    [TestClass]
    public class MultiDbDataTests
    {
        #region MultiDbData Tests

        [TestMethod]
        public void MultiDbData_Constructor_SetsDefaults()
        {
            var data = new MultiDbData();

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
        public void MultiDbData_IsListOfServerData()
        {
            var data = new MultiDbData();

            Assert.IsInstanceOfType(data, typeof(List<ServerData>));
        }

        [TestMethod]
        public void MultiDbData_CanAddServerData()
        {
            var data = new MultiDbData();
            var server = new ServerData { ServerName = "TestServer" };

            data.Add(server);

            Assert.AreEqual(1, data.Count);
            Assert.AreEqual("TestServer", data[0].ServerName);
        }

        [TestMethod]
        public void MultiDbData_CanSetAllProperties()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var data = new MultiDbData
            {
                AllowableTimeoutRetries = 5,
                IsTransactional = false,
                RunAsTrial = false,
                BuildFileName = "build.sbm",
                ProjectFileName = "project.xml",
                BuildData = model,
                MultiRunId = "run-123",
                UserName = "admin",
                Password = "secret",
                AuthenticationType = AuthenticationType.AzureADPassword,
                BuildRevision = "v1.0.0"
            };

            Assert.AreEqual(5, data.AllowableTimeoutRetries);
            Assert.IsFalse(data.IsTransactional);
            Assert.IsFalse(data.RunAsTrial);
            Assert.AreEqual("build.sbm", data.BuildFileName);
            Assert.AreEqual("project.xml", data.ProjectFileName);
            Assert.IsNotNull(data.BuildData);
            Assert.AreEqual("run-123", data.MultiRunId);
            Assert.AreEqual("admin", data.UserName);
            Assert.AreEqual("secret", data.Password);
            Assert.AreEqual(AuthenticationType.AzureADPassword, data.AuthenticationType);
            Assert.AreEqual("v1.0.0", data.BuildRevision);
        }

        #endregion

        #region ServerData Tests

        [TestMethod]
        public void ServerData_Constructor_SetsDefaults()
        {
            var server = new ServerData();

            Assert.AreEqual(string.Empty, server.ServerName);
            Assert.IsNotNull(server.Overrides);
            Assert.AreEqual(0, server.Overrides.Count);
            Assert.IsNull(server.SequenceId);
        }

        [TestMethod]
        public void ServerData_CanSetProperties()
        {
            var server = new ServerData
            {
                ServerName = "MyServer",
                SequenceId = 42,
                Overrides = new DbOverrides
                {
                    new DatabaseOverride("Server1", "DefaultDb", "OverrideDb")
                }
            };

            Assert.AreEqual("MyServer", server.ServerName);
            Assert.AreEqual(42, server.SequenceId);
            Assert.AreEqual(1, server.Overrides.Count);
        }

        [TestMethod]
        public void ServerData_Equals_ReturnsTrueForSameData()
        {
            var server1 = new ServerData
            {
                ServerName = "Server1",
                Overrides = new DbOverrides
                {
                    new DatabaseOverride("Server1", "Default", "Override")
                }
            };
            var server2 = new ServerData
            {
                ServerName = "Server1",
                Overrides = new DbOverrides
                {
                    new DatabaseOverride("Server1", "Default", "Override")
                }
            };

            var result = server1.Equals(server2);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ServerData_Equals_ReturnsFalseForDifferentServerName()
        {
            var server1 = new ServerData { ServerName = "Server1" };
            var server2 = new ServerData { ServerName = "Server2" };

            var result = server1.Equals(server2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ServerData_Equals_ReturnsFalseForDifferentOverrides()
        {
            var server1 = new ServerData
            {
                ServerName = "Server1",
                Overrides = new DbOverrides
                {
                    new DatabaseOverride("Server1", "Default", "Override1")
                }
            };
            var server2 = new ServerData
            {
                ServerName = "Server1",
                Overrides = new DbOverrides
                {
                    new DatabaseOverride("Server1", "Default", "Override2")
                }
            };

            var result = server1.Equals(server2);

            Assert.IsFalse(result);
        }

        #endregion

        #region DbOverrides Tests

        [TestMethod]
        public void DbOverrides_Constructor_CreatesEmptyList()
        {
            var overrides = new DbOverrides();

            Assert.AreEqual(0, overrides.Count);
        }

        [TestMethod]
        public void DbOverrides_ConstructorWithParams_AddsItems()
        {
            var ovr1 = new DatabaseOverride("Server1", "Default1", "Override1");
            var ovr2 = new DatabaseOverride("Server2", "Default2", "Override2");

            var overrides = new DbOverrides(ovr1, ovr2);

            Assert.AreEqual(2, overrides.Count);
        }

        [TestMethod]
        public void DbOverrides_GetQueryRowData_WithEmptyList_ReturnsEmptyList()
        {
            var overrides = new DbOverrides();

            var result = overrides.GetQueryRowData("default", "override");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void DbOverrides_GetQueryRowData_WithNoMatch_ReturnsEmptyList()
        {
            var overrides = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default1", "Override1")
            };

            var result = overrides.GetQueryRowData("nonexistent", "nonexistent");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void DbOverrides_GetQueryRowData_IsCaseInsensitive()
        {
            var queryRow = new List<QueryRowItem> { new QueryRowItem { ColumnName = "Col1" } };
            var overrides = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default1", "Override1") { QueryRowData = queryRow }
            };

            var result = overrides.GetQueryRowData("DEFAULT1", "OVERRIDE1");

            Assert.AreEqual(1, result.Count);
        }

        [TestMethod]
        public void DbOverrides_GetOverrideDatabaseNameList_ReturnsDistinctSortedList()
        {
            var overrides = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default1", "Override1"),
                new DatabaseOverride("Server1", "Default2", "Override2"),
                new DatabaseOverride("Server1", "Default3", "Override1"), // Duplicate override
                new DatabaseOverride("Server1", "Default4", "Override3")
            };

            var result = overrides.GetOverrideDatabaseNameList();

            Assert.AreEqual(3, result.Count);
            Assert.AreEqual("Override1", result[0]);
            Assert.AreEqual("Override2", result[1]);
            Assert.AreEqual("Override3", result[2]);
        }

        [TestMethod]
        public void DbOverrides_GetOverrideDatabaseNameList_WithEmpty_ReturnsEmptyList()
        {
            var overrides = new DbOverrides();

            var result = overrides.GetOverrideDatabaseNameList();

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void DbOverrides_Equals_ReturnsTrueForSameData()
        {
            var ovr1 = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default1", "Override1")
            };
            var ovr2 = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default1", "Override1")
            };

            var result = ovr1.Equals(ovr2);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void DbOverrides_Equals_ReturnsFalseForDifferentCount()
        {
            var ovr1 = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default1", "Override1")
            };
            var ovr2 = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default1", "Override1"),
                new DatabaseOverride("Server1", "Default2", "Override2")
            };

            var result = ovr1.Equals(ovr2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DbOverrides_Equals_ReturnsFalseForDifferentDefaultDb()
        {
            var ovr1 = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default1", "Override1")
            };
            var ovr2 = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default2", "Override1")
            };

            var result = ovr1.Equals(ovr2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DbOverrides_Equals_ReturnsFalseForDifferentOverrideDb()
        {
            var ovr1 = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default1", "Override1")
            };
            var ovr2 = new DbOverrides
            {
                new DatabaseOverride("Server1", "Default1", "Override2")
            };

            var result = ovr1.Equals(ovr2);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void DbOverrides_Equals_WithSingleItem_ComparesCorrectly()
        {
            var ovr1 = new DbOverrides
            {
                new DatabaseOverride("Server1", "A", "X")
            };
            var ovr2 = new DbOverrides
            {
                new DatabaseOverride("Server1", "A", "X")
            };

            // The Equals method may call Sort() which can throw if DatabaseOverride doesn't implement IComparable
            // If there's only one item, Sort() won't compare anything
            var result = ovr1.Equals(ovr2);

            Assert.IsTrue(result);
        }

        #endregion

        #region DatabaseOverride Tests

        [TestMethod]
        public void DatabaseOverride_Constructor_SetsProperties()
        {
            var ovr = new DatabaseOverride("Server1", "DefaultDb", "OverrideDb");

            Assert.AreEqual("Server1", ovr.Server);
            Assert.AreEqual("DefaultDb", ovr.DefaultDbTarget);
            Assert.AreEqual("OverrideDb", ovr.OverrideDbTarget);
        }

        [TestMethod]
        public void DatabaseOverride_QueryRowData_DefaultsToEmptyList()
        {
            var ovr = new DatabaseOverride("Server1", "DefaultDb", "OverrideDb");

            Assert.IsNotNull(ovr.QueryRowData);
        }

        #endregion
    }
}
