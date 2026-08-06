using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlBuildManager.Connection.UnitTest
{
    /// <summary>
    /// Additional unit tests for ConnectionHelper class to improve coverage
    /// </summary>
    [TestClass]
    public class ConnectionHelperAdditionalTest
    {
        private static string appNameString = string.Empty;

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            appNameString = ConnectionHelper.appName;
        }

        #region GetConnectionString Tests for Different Auth Types

        [TestMethod]
        public void GetConnectionString_AzureADIntegrated_ShouldIncludeCorrectAuthMethod()
        {
            var connData = new ConnectionData("myserver", "mydatabase")
            {
                AuthenticationType = AuthenticationType.AzureADIntegrated
            };

            string result = ConnectionHelper.GetConnectionString(connData);

            Assert.Contains("Authentication=ActiveDirectoryIntegrated", result);
            Assert.Contains("Integrated Security=True", result);
            Assert.DoesNotContain("Trust Server Certificate=True", result, "Should not trust server cert by default (secure-by-default)");
        }

        [TestMethod]
        public void GetConnectionString_AzureADIntegrated_ShouldNotIncludeCredentials()
        {
            var connData = new ConnectionData
            {
                SQLServerName = "myserver",
                DatabaseName = "mydatabase",
                AuthenticationType = AuthenticationType.AzureADIntegrated,
                UserId = "user@domain.com",
                Password = "secretPassword"
            };

            string result = ConnectionHelper.GetConnectionString(connData);

            Assert.Contains("Authentication=ActiveDirectoryIntegrated", result);
            Assert.DoesNotContain("User ID=user@domain.com", result);
            Assert.DoesNotContain("Password=secretPassword", result);
        }

        [TestMethod]
        public void GetConnectionString_ManagedIdentity_ShouldIncludeManagedIdentityClientId()
        {
            var connData = new ConnectionData
            {
                SQLServerName = "myserver.database.windows.net",
                DatabaseName = "mydatabase",
                AuthenticationType = AuthenticationType.ManagedIdentity,
                ManagedIdentityClientId = "client-id-12345"
            };

            string result = ConnectionHelper.GetConnectionString(connData);

            Assert.Contains("Authentication=ActiveDirectoryManagedIdentity", result);
            Assert.Contains("User ID=client-id-12345", result);
        }

        [TestMethod]
        public void GetConnectionString_AzureADInteractive_ShouldSetCorrectAuthMethod()
        {
            var connData = new ConnectionData
            {
                SQLServerName = "myserver",
                DatabaseName = "mydatabase",
                AuthenticationType = AuthenticationType.AzureADInteractive
            };

            string result = ConnectionHelper.GetConnectionString(connData);

            Assert.Contains("Authentication=ActiveDirectoryInteractive", result);
        }

        [TestMethod]
        public void GetConnectionString_NullConnectionData_ShouldReturnEmptyString()
        {
            string result = ConnectionHelper.GetConnectionString(null!);

            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region GetConnection Tests

        [TestMethod]
        public void GetConnection_NullConnectionData_ShouldReturnNull()
        {
            var result = ConnectionHelper.GetConnection(null!);

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetConnection_WithManagedIdentity_ShouldCreateValidConnection()
        {
            var connData = new ConnectionData
            {
                SQLServerName = "myserver.database.windows.net",
                DatabaseName = "mydatabase",
                AuthenticationType = AuthenticationType.ManagedIdentity,
                ManagedIdentityClientId = "client-id-12345",
                ScriptTimeout = 30
            };

            var result = ConnectionHelper.GetConnection(connData);

            Assert.IsNotNull(result);
            Assert.AreEqual("myserver.database.windows.net", result.DataSource);
            Assert.AreEqual(System.Data.ConnectionState.Closed, result.State);
        }

        #endregion

        #region ConnectCryptoKey Tests

        [TestMethod]
        public void ConnectCryptoKey_ShouldIncludeUserName()
        {
            string result = ConnectionHelper.ConnectCryptoKey;

            Assert.IsNotNull(result);
            Assert.Contains(Environment.UserName, result);
        }

        [TestMethod]
        public void ConnectCryptoKey_MultipleCalls_ShouldReturnConsistentValue()
        {
            string result1 = ConnectionHelper.ConnectCryptoKey;
            string result2 = ConnectionHelper.ConnectCryptoKey;

            Assert.AreEqual(result1, result2);
        }

        #endregion

        #region appName Tests

        [TestMethod]
        public void AppName_ShouldBePopulated()
        {
            Assert.IsNotNull(ConnectionHelper.appName);
            Assert.Contains("Sql Build Manager", ConnectionHelper.appName);
            Assert.Contains(Environment.UserName, ConnectionHelper.appName);
        }

        #endregion

        #region Connection Failure Classification

        [TestMethod]
        [DataRow(47073, true)]
        [DataRow(18456, false)]
        public void IsSqlPublicNetworkAccessDenied_ShouldClassifyExactError(
            int errorNumber,
            bool expected)
        {
            Assert.AreEqual(
                expected,
                ConnectionHelper.IsSqlPublicNetworkAccessDenied(errorNumber));
        }

        #endregion

        #region GetTargetDatabase Edge Cases

        [TestMethod]
        public void GetTargetDatabase_NullOverrides_ShouldReturnDefaultDatabase()
        {
            string result = ConnectionHelper.GetTargetDatabase("defaultDb", null!);

            Assert.AreEqual("defaultDb", result);
        }

        [TestMethod]
        public void GetTargetDatabase_EmptyOverridesList_ShouldReturnDefaultDatabase()
        {
            var overrides = new System.Collections.Generic.List<DatabaseOverride>();

            string result = ConnectionHelper.GetTargetDatabase("defaultDb", overrides);

            Assert.AreEqual("defaultDb", result);
        }

        #endregion

        #region ValidateDatabaseOverrides Edge Cases

        [TestMethod]
        public void ValidateDatabaseOverrides_EmptyList_ShouldReturnTrue()
        {
            var overrides = new System.Collections.Generic.List<DatabaseOverride>();

            bool result = ConnectionHelper.ValidateDatabaseOverrides(overrides);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ValidateDatabaseOverrides_WithEmptyDefaultAndEmptyOverride_ShouldReturnFalse()
        {
            var overrides = new System.Collections.Generic.List<DatabaseOverride>
            {
                new DatabaseOverride("server", "", "")
            };

            bool result = ConnectionHelper.ValidateDatabaseOverrides(overrides);

            Assert.IsFalse(result);
        }

        #endregion
    }
}
