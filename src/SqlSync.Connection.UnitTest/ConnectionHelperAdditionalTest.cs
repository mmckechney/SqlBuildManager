using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.Connection.UnitTest
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
            var x = new ConnectionHelper();
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

            Assert.IsTrue(result.Contains("Authentication=ActiveDirectoryIntegrated"));
            Assert.IsTrue(result.Contains("Integrated Security=True"));
            Assert.IsTrue(result.Contains("Trust Server Certificate=True"));
        }

        [TestMethod]
        public void GetConnectionString_AzureADPassword_ShouldIncludeCredentials()
        {
            var connData = new ConnectionData
            {
                SQLServerName = "myserver",
                DatabaseName = "mydatabase",
                AuthenticationType = AuthenticationType.AzureADPassword,
                UserId = "user@domain.com",
                Password = "secretPassword"
            };

            string result = ConnectionHelper.GetConnectionString(connData);

            Assert.IsTrue(result.Contains("Authentication=ActiveDirectoryPassword"));
            Assert.IsTrue(result.Contains("User ID=user@domain.com"));
            Assert.IsTrue(result.Contains("Password=secretPassword"));
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

            Assert.IsTrue(result.Contains("Authentication=ActiveDirectoryManagedIdentity"));
            Assert.IsTrue(result.Contains("User ID=client-id-12345"));
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

            Assert.IsTrue(result.Contains("Authentication=ActiveDirectoryInteractive"));
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
            Assert.IsTrue(result.Contains(Environment.UserName));
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
            Assert.IsTrue(ConnectionHelper.appName.Contains("Sql Build Manager"));
            Assert.IsTrue(ConnectionHelper.appName.Contains(Environment.UserName));
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
