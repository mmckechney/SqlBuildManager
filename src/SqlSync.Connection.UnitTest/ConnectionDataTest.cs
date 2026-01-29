using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.Connection.UnitTest
{
    /// <summary>
    /// Unit tests for ConnectionData class
    /// </summary>
    [TestClass]
    public class ConnectionDataTest
    {
        #region Constructor Tests

        [TestMethod]
        public void ConnectionDataConstructor_Default_ShouldSetDefaultValues()
        {
            var target = new ConnectionData();

            Assert.IsNotNull(target);
            Assert.AreEqual(string.Empty, target.SQLServerName);
            Assert.AreEqual(string.Empty, target.DatabaseName);
            Assert.AreEqual(string.Empty, target.Password);
            Assert.AreEqual(string.Empty, target.UserId);
            Assert.AreEqual(string.Empty, target.StartingDirectory);
            Assert.AreEqual(string.Empty, target.ManagedIdentityClientId);
            Assert.AreEqual(AuthenticationType.Password, target.AuthenticationType);
            Assert.AreEqual(20, target.ScriptTimeout);
        }

        [TestMethod]
        public void ConnectionDataConstructor_WithServerAndDatabase_ShouldSetProperties()
        {
            string serverName = "myserver";
            string databaseName = "mydatabase";

            var target = new ConnectionData(serverName, databaseName);

            Assert.AreEqual(serverName, target.SQLServerName);
            Assert.AreEqual(databaseName, target.DatabaseName);
            Assert.AreEqual(AuthenticationType.Windows, target.AuthenticationType);
        }

        #endregion

        #region Property Tests

        [TestMethod]
        public void SQLServerName_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ConnectionData();
            string expected = "TestServer";

            target.SQLServerName = expected;

            Assert.AreEqual(expected, target.SQLServerName);
        }

        [TestMethod]
        public void DatabaseName_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ConnectionData();
            string expected = "TestDatabase";

            target.DatabaseName = expected;

            Assert.AreEqual(expected, target.DatabaseName);
        }

        [TestMethod]
        public void Password_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ConnectionData();
            string expected = "SecretPassword123";

            target.Password = expected;

            Assert.AreEqual(expected, target.Password);
        }

        [TestMethod]
        public void UserId_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ConnectionData();
            string expected = "TestUser";

            target.UserId = expected;

            Assert.AreEqual(expected, target.UserId);
        }

        [TestMethod]
        public void StartingDirectory_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ConnectionData();
            string expected = @"C:\Test\Directory";

            target.StartingDirectory = expected;

            Assert.AreEqual(expected, target.StartingDirectory);
        }

        [TestMethod]
        public void ManagedIdentityClientId_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ConnectionData();
            string expected = "client-id-12345";

            target.ManagedIdentityClientId = expected;

            Assert.AreEqual(expected, target.ManagedIdentityClientId);
        }

        [TestMethod]
        public void AuthenticationType_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ConnectionData();

            target.AuthenticationType = AuthenticationType.AzureADIntegrated;
            Assert.AreEqual(AuthenticationType.AzureADIntegrated, target.AuthenticationType);

            target.AuthenticationType = AuthenticationType.ManagedIdentity;
            Assert.AreEqual(AuthenticationType.ManagedIdentity, target.AuthenticationType);
        }

        [TestMethod]
        public void ScriptTimeout_SetAndGet_ShouldReturnCorrectValue()
        {
            var target = new ConnectionData();
            int expected = 120;

            target.ScriptTimeout = expected;

            Assert.AreEqual(expected, target.ScriptTimeout);
        }

        #endregion

        #region Fill Method Tests

        [TestMethod]
        public void Fill_WithValidConnectionData_ShouldCopyAllProperties()
        {
            var source = new ConnectionData
            {
                SQLServerName = "SourceServer",
                DatabaseName = "SourceDatabase",
                Password = "SourcePassword",
                UserId = "SourceUser",
                StartingDirectory = @"C:\Source",
                AuthenticationType = AuthenticationType.AzureADPassword,
                ScriptTimeout = 60,
                ManagedIdentityClientId = "source-client-id"
            };
            var target = new ConnectionData();

            var result = target.Fill(source);

            Assert.AreSame(target, result);
            Assert.AreEqual(source.SQLServerName, target.SQLServerName);
            Assert.AreEqual(source.DatabaseName, target.DatabaseName);
            Assert.AreEqual(source.Password, target.Password);
            Assert.AreEqual(source.UserId, target.UserId);
            Assert.AreEqual(source.StartingDirectory, target.StartingDirectory);
            Assert.AreEqual(source.AuthenticationType, target.AuthenticationType);
            Assert.AreEqual(source.ScriptTimeout, target.ScriptTimeout);
            Assert.AreEqual(source.ManagedIdentityClientId, target.ManagedIdentityClientId);
        }

        [TestMethod]
        public void Fill_ShouldReturnSameInstance()
        {
            var source = new ConnectionData("server", "database");
            var target = new ConnectionData();

            var result = target.Fill(source);

            Assert.AreSame(target, result);
        }

        #endregion
    }
}
