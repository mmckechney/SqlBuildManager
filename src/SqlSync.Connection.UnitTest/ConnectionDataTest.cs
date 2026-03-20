using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;

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
            string expected = Path.Combine(Path.GetTempPath(), "Test", "Directory");

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
                StartingDirectory = Path.Combine(Path.GetTempPath(), "Source"),
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

        [TestMethod]
        public void Fill_WithEmptySource_ShouldSetEmptyValues()
        {
            var source = new ConnectionData();
            var target = new ConnectionData
            {
                SQLServerName = "PreExisting",
                DatabaseName = "PreExistingDB"
            };

            target.Fill(source);

            Assert.AreEqual(string.Empty, target.SQLServerName);
            Assert.AreEqual(string.Empty, target.DatabaseName);
        }

        [TestMethod]
        public void Fill_OverwritesExistingValues()
        {
            var target = new ConnectionData
            {
                SQLServerName = "OldServer",
                DatabaseName = "OldDatabase",
                UserId = "OldUser",
                Password = "OldPassword",
                ScriptTimeout = 10
            };

            var source = new ConnectionData
            {
                SQLServerName = "NewServer",
                DatabaseName = "NewDatabase",
                UserId = "NewUser",
                Password = "NewPassword",
                ScriptTimeout = 60
            };

            target.Fill(source);

            Assert.AreEqual("NewServer", target.SQLServerName);
            Assert.AreEqual("NewDatabase", target.DatabaseName);
            Assert.AreEqual("NewUser", target.UserId);
            Assert.AreEqual("NewPassword", target.Password);
            Assert.AreEqual(60, target.ScriptTimeout);
        }

        #endregion

        #region All AuthenticationTypes Tests

        [TestMethod]
        public void AuthenticationType_AllValues_CanBeSetAndRetrieved()
        {
            var target = new ConnectionData();

            target.AuthenticationType = AuthenticationType.Password;
            Assert.AreEqual(AuthenticationType.Password, target.AuthenticationType);

            target.AuthenticationType = AuthenticationType.Windows;
            Assert.AreEqual(AuthenticationType.Windows, target.AuthenticationType);

            target.AuthenticationType = AuthenticationType.AzureADPassword;
            Assert.AreEqual(AuthenticationType.AzureADPassword, target.AuthenticationType);

            target.AuthenticationType = AuthenticationType.AzureADIntegrated;
            Assert.AreEqual(AuthenticationType.AzureADIntegrated, target.AuthenticationType);

            target.AuthenticationType = AuthenticationType.ManagedIdentity;
            Assert.AreEqual(AuthenticationType.ManagedIdentity, target.AuthenticationType);

            target.AuthenticationType = AuthenticationType.AzureADInteractive;
            Assert.AreEqual(AuthenticationType.AzureADInteractive, target.AuthenticationType);
        }

        #endregion

        #region Boundary Tests

        [TestMethod]
        public void ScriptTimeout_ZeroValue_CanBeSet()
        {
            var target = new ConnectionData();
            target.ScriptTimeout = 0;
            Assert.AreEqual(0, target.ScriptTimeout);
        }

        [TestMethod]
        public void ScriptTimeout_NegativeValue_CanBeSet()
        {
            var target = new ConnectionData();
            target.ScriptTimeout = -1;
            Assert.AreEqual(-1, target.ScriptTimeout);
        }

        [TestMethod]
        public void ScriptTimeout_LargeValue_CanBeSet()
        {
            var target = new ConnectionData();
            target.ScriptTimeout = int.MaxValue;
            Assert.AreEqual(int.MaxValue, target.ScriptTimeout);
        }

        [TestMethod]
        public void SQLServerName_EmptyString_CanBeSet()
        {
            var target = new ConnectionData("server", "database");
            target.SQLServerName = string.Empty;
            Assert.AreEqual(string.Empty, target.SQLServerName);
        }

        [TestMethod]
        public void DatabaseName_NullValue_CanBeSet()
        {
            var target = new ConnectionData();
            target.DatabaseName = null!;
            Assert.IsNull(target.DatabaseName);
        }

        [TestMethod]
        public void Password_SpecialCharacters_CanBeSet()
        {
            var target = new ConnectionData();
            string specialPassword = "P@$$w0rd!#$%^&*()_+-=[]{}|;':\",./<>?";
            target.Password = specialPassword;
            Assert.AreEqual(specialPassword, target.Password);
        }

        [TestMethod]
        public void SQLServerName_LongValue_CanBeSet()
        {
            var target = new ConnectionData();
            string longServerName = new string('a', 1000);
            target.SQLServerName = longServerName;
            Assert.AreEqual(longServerName, target.SQLServerName);
        }

        #endregion

        #region Integration-like Tests

        [TestMethod]
        public void ConnectionData_ChainedPropertySets_WorkCorrectly()
        {
            var target = new ConnectionData();
            var startingDir = Path.Combine(Path.GetTempPath(), "Dir1");
            target.SQLServerName = "server1";
            target.DatabaseName = "db1";
            target.UserId = "user1";
            target.Password = "pass1";
            target.AuthenticationType = AuthenticationType.Password;
            target.ScriptTimeout = 30;
            target.StartingDirectory = startingDir;
            target.ManagedIdentityClientId = "client1";

            Assert.AreEqual("server1", target.SQLServerName);
            Assert.AreEqual("db1", target.DatabaseName);
            Assert.AreEqual("user1", target.UserId);
            Assert.AreEqual("pass1", target.Password);
            Assert.AreEqual(AuthenticationType.Password, target.AuthenticationType);
            Assert.AreEqual(30, target.ScriptTimeout);
            Assert.AreEqual(startingDir, target.StartingDirectory);
            Assert.AreEqual("client1", target.ManagedIdentityClientId);
        }

        [TestMethod]
        public void ConnectionData_ConstructorThenFill_OverwritesConstructorValues()
        {
            var target = new ConnectionData("initialServer", "initialDB");
            Assert.AreEqual(AuthenticationType.Windows, target.AuthenticationType);

            var source = new ConnectionData
            {
                SQLServerName = "newServer",
                DatabaseName = "newDB",
                AuthenticationType = AuthenticationType.Password,
                UserId = "user",
                Password = "pass"
            };

            target.Fill(source);

            Assert.AreEqual("newServer", target.SQLServerName);
            Assert.AreEqual("newDB", target.DatabaseName);
            Assert.AreEqual(AuthenticationType.Password, target.AuthenticationType);
        }

        [TestMethod]
        public void ConnectionData_ManagedIdentityScenario_SetupCorrectly()
        {
            var target = new ConnectionData
            {
                SQLServerName = "myserver.database.windows.net",
                DatabaseName = "mydb",
                AuthenticationType = AuthenticationType.ManagedIdentity,
                ManagedIdentityClientId = "12345678-1234-1234-1234-123456789012",
                ScriptTimeout = 60
            };

            Assert.AreEqual("myserver.database.windows.net", target.SQLServerName);
            Assert.AreEqual("mydb", target.DatabaseName);
            Assert.AreEqual(AuthenticationType.ManagedIdentity, target.AuthenticationType);
            Assert.AreEqual("12345678-1234-1234-1234-123456789012", target.ManagedIdentityClientId);
        }

        [TestMethod]
        public void ConnectionData_AzureADPasswordScenario_SetupCorrectly()
        {
            var target = new ConnectionData
            {
                SQLServerName = "myserver.database.windows.net",
                DatabaseName = "mydb",
                AuthenticationType = AuthenticationType.AzureADPassword,
                UserId = "user@domain.com",
                Password = "MyP@ssword123",
                ScriptTimeout = 30
            };

            Assert.AreEqual(AuthenticationType.AzureADPassword, target.AuthenticationType);
            Assert.AreEqual("user@domain.com", target.UserId);
            Assert.AreEqual("MyP@ssword123", target.Password);
        }

        #endregion
    }
}
