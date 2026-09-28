using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;
using Azure.Identity;
using System;

namespace SqlBuildManager.Connection.UnitTest
{
    [TestClass]
    public class MySqlConnectionFactoryTest
    {
        private MySqlConnectionFactory factory = null!;
        private static string appNameString = string.Empty;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _ = new ConnectionHelper();
            appNameString = ConnectionHelper.appName;
        }

        [TestInitialize]
        public void TestInit()
        {
            factory = new MySqlConnectionFactory();
        }

        [TestMethod]
        public void BuildConnectionString_PasswordAuth_ShouldContainCredentials()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "mysqluser", "mysqlpass", AuthenticationType.Password, 30, "");
            var builder = new MySqlConnectionStringBuilder(connStr);

            Assert.IsTrue(connStr.Contains("Server=localhost"), "Should contain Server");
            Assert.IsTrue(connStr.Contains("Database=mydb"), "Should contain Database");
            Assert.IsTrue(connStr.Contains("User ID=mysqluser"), "Should contain User ID");
            Assert.AreEqual("mysqlpass", builder.Password, "Should contain Password");
            Assert.AreEqual(MySqlSslMode.Preferred, builder.SslMode, "Native local authentication must retain its existing behavior.");
        }

        [TestMethod]
        [DataRow(AuthenticationType.ManagedIdentity)]
        [DataRow(AuthenticationType.AzureADDefault)]
        public void BuildConnectionString_EntraAuth_UsesTokenAndDatabasePrincipal(AuthenticationType authType)
        {
            var tokenFactory = new MySqlConnectionFactory(clientId =>
            {
                Assert.AreEqual("test-client-id", clientId);
                return "synthetic-entra-token";
            });
            var builder = new MySqlConnectionStringBuilder(tokenFactory.BuildConnectionString(
                "sbm_mysql_test1", "test.mysql.database.azure.com", "id-test", "unused-native-password",
                authType, 30, "test-client-id"));

            Assert.AreEqual("id-test", builder.UserID);
            Assert.AreEqual("synthetic-entra-token", builder.Password);
            Assert.AreEqual(MySqlSslMode.VerifyFull, builder.SslMode);
        }

        [TestMethod]
        public void BuildConnectionString_NativeAuth_DoesNotAcquireAzureToken()
        {
            var tokenFactory = new MySqlConnectionFactory(_ => throw new InvalidOperationException("Unexpected Azure authentication"));
            var builder = new MySqlConnectionStringBuilder(tokenFactory.BuildConnectionString(
                "local", "localhost", "root", "local-only", AuthenticationType.Password, 30, ""));
            Assert.AreEqual("local-only", builder.Password);
        }

        [TestMethod]
        public void CreateTokenCredential_Aks_UsesWorkloadIdentity()
        {
            var credential = MySqlConnectionFactory.CreateTokenCredential(
                "11111111-1111-1111-1111-111111111111",
                "22222222-2222-2222-2222-222222222222", "synthetic-token-file");
            Assert.IsInstanceOfType(credential, typeof(WorkloadIdentityCredential));
        }

        [TestMethod]
        public void CreateTokenCredential_AzureContainer_UsesManagedIdentity()
        {
            var credential = MySqlConnectionFactory.CreateTokenCredential(
                "11111111-1111-1111-1111-111111111111", "", "");
            Assert.IsInstanceOfType(credential, typeof(ManagedIdentityCredential));
        }

        [TestMethod]
        public void CreateTokenCredential_Developer_UsesDefaultCredential()
        {
            Assert.IsInstanceOfType(MySqlConnectionFactory.CreateTokenCredential("", "", ""), typeof(DefaultAzureCredential));
        }

        [TestMethod]
        public void CreateTokenCredential_IncompleteWorkloadIdentity_DoesNotFallBack()
        {
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                MySqlConnectionFactory.CreateTokenCredential("client-id", "", "token-file"));
            Assert.ThrowsExactly<InvalidOperationException>(() =>
                MySqlConnectionFactory.CreateTokenCredential("", "tenant-id", "token-file"));
        }

        [TestMethod]
        public void RedactConnectionString_ShouldRemovePasswordValue()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "mysqluser", "mysqlpass", AuthenticationType.Password, 30, "");

            string redacted = ConnectionStringRedactor.Redact(connStr);
            var redactedBuilder = new MySqlConnectionStringBuilder(redacted);

            Assert.IsFalse(redacted.Contains("mysqlpass"), "Should not contain full password value");
            Assert.AreEqual(ConnectionStringRedactor.MaskKey("mysqlpass"), redactedBuilder.Password, "Password should be masked keeping the first 4 chars");
            Assert.AreEqual("mysqluser", redactedBuilder.UserID, "Should preserve non-secret connection details");
        }

        [TestMethod]
        public void BuildConnectionString_DefaultPort_ShouldBe3306()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "mysqluser", "mysqlpass", AuthenticationType.Password, 30, "");
            Assert.IsTrue(connStr.Contains("Port=3306"), "Should use default port 3306");
        }

        [TestMethod]
        public void BuildConnectionString_CustomPort_ShouldParseFromServerName()
        {
            string connStr = factory.BuildConnectionString("mydb", "mysqlhost:3307", "mysqluser", "mysqlpass", AuthenticationType.Password, 30, "");

            Assert.IsTrue(connStr.Contains("Server=mysqlhost"), "Should parse server");
            Assert.IsTrue(connStr.Contains("Port=3307"), "Should parse custom port");
        }

        [TestMethod]
        public void BuildConnectionString_Timeout_ShouldBeSet()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "mysqluser", "mysqlpass", AuthenticationType.Password, 60, "");
            Assert.IsTrue(connStr.Contains("Connection Timeout=60"), "Should contain timeout");
        }

        [TestMethod]
        public void BuildConnectionString_PoolingEnabledWithSafeLimits()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "mysqluser", "mysqlpass", AuthenticationType.Password, 30, "");
            var builder = new MySqlConnectionStringBuilder(connStr);
            Assert.IsTrue(builder.Pooling);
            Assert.AreEqual((uint)0, builder.MinimumPoolSize);
            Assert.AreEqual((uint)100, builder.MaximumPoolSize);
        }

        [TestMethod]
        public void BuildConnectionString_ApplicationName_ShouldBeSet()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "mysqluser", "mysqlpass", AuthenticationType.Password, 30, "");
            Assert.IsTrue(connStr.Contains("Application Name="), "Should contain application name");
            Assert.IsTrue(connStr.Contains(appNameString), "Should use shared app name");
        }

        [TestMethod]
        public void BuildConnectionString_InvalidPort_ShouldUseDefault()
        {
            string connStr = factory.BuildConnectionString("mydb", "mysqlhost:abc", "mysqluser", "mysqlpass", AuthenticationType.Password, 30, "");
            Assert.IsTrue(connStr.Contains("Port=3306"), "Should fall back to default port for invalid port");
        }

        [TestMethod]
        public void CreateConnection_ShouldReturnMySqlConnection()
        {
            var conn = factory.CreateConnection("mydb", "localhost", "mysqluser", "mysqlpass", AuthenticationType.Password, 30, "");
            Assert.IsInstanceOfType(conn, typeof(MySqlConnection));
            conn.Dispose();
        }

        [TestMethod]
        public void CreateConnection_FromConnectionData_ShouldReturnMySqlConnection()
        {
            var connData = new ConnectionData
            {
                DatabaseName = "mydb",
                SQLServerName = "localhost",
                UserId = "mysqluser",
                Password = "mysqlpass",
                AuthenticationType = AuthenticationType.Password,
                ScriptTimeout = 30,
                DatabasePlatform = DatabasePlatform.MySQL
            };

            var conn = factory.CreateConnection(connData);
            Assert.IsInstanceOfType(conn, typeof(MySqlConnection));
            conn.Dispose();
        }

        [TestMethod]
        public void CreateCommand_ShouldReturnMySqlCommand()
        {
            using var conn = new MySqlConnection("Server=localhost;Database=mydb;User ID=mysqluser;******");
            var cmd = factory.CreateCommand("SELECT 1", conn);
            Assert.IsInstanceOfType(cmd, typeof(MySqlCommand));
            Assert.AreEqual("SELECT 1", cmd.CommandText);
            cmd.Dispose();
        }

        [TestMethod]
        public void CreateParameter_ShouldReturnMySqlParameter()
        {
            var param = factory.CreateParameter("@name", "value");
            Assert.IsInstanceOfType(param, typeof(MySqlParameter));
            Assert.AreEqual("@name", param.ParameterName);
            Assert.AreEqual("value", param.Value);
        }
    }
}
