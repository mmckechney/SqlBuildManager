using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;

namespace SqlSync.Connection.UnitTest
{
    [TestClass]
    public class PostgresConnectionFactoryTest
    {
        private PostgresConnectionFactory factory;
        private static string appNameString;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _ = new ConnectionHelper();
            appNameString = ConnectionHelper.appName;
        }

        [TestInitialize]
        public void TestInit()
        {
            factory = new PostgresConnectionFactory();
        }

        #region BuildConnectionString Tests

        [TestMethod]
        public void BuildConnectionString_PasswordAuth_ShouldContainCredentials()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "pguser", "pgpass", AuthenticationType.Password, 30, "");

            Assert.IsTrue(connStr.Contains("Host=localhost"), "Should contain Host");
            Assert.IsTrue(connStr.Contains("Database=mydb"), "Should contain Database");
            Assert.IsTrue(connStr.Contains("Username=pguser"), "Should contain Username");
            Assert.IsTrue(connStr.Contains("Password=pgpass"), "Should contain Password");
        }

        [TestMethod]
        public void BuildConnectionString_DefaultPort_ShouldBe5432()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "pguser", "pgpass", AuthenticationType.Password, 30, "");
            Assert.IsTrue(connStr.Contains("Port=5432"), "Should use default port 5432");
        }

        [TestMethod]
        public void BuildConnectionString_CustomPort_ShouldParseFromServerName()
        {
            string connStr = factory.BuildConnectionString("mydb", "myhost:5433", "pguser", "pgpass", AuthenticationType.Password, 30, "");

            Assert.IsTrue(connStr.Contains("Host=myhost"), "Should parse host");
            Assert.IsTrue(connStr.Contains("Port=5433"), "Should parse custom port");
        }

        [TestMethod]
        public void BuildConnectionString_Timeout_ShouldBeSet()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "pguser", "pgpass", AuthenticationType.Password, 60, "");
            Assert.IsTrue(connStr.Contains("Timeout=60"), "Should contain timeout");
        }

        [TestMethod]
        public void BuildConnectionString_PoolingDisabled()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "pguser", "pgpass", AuthenticationType.Password, 30, "");
            Assert.IsTrue(connStr.Contains("Pooling=False"), "Should have pooling disabled");
        }

        [TestMethod]
        public void BuildConnectionString_ApplicationName_ShouldBeSet()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "pguser", "pgpass", AuthenticationType.Password, 30, "");
            Assert.IsTrue(connStr.Contains("Application Name="), "Should contain application name");
        }

        [TestMethod]
        public void BuildConnectionString_WindowsAuth_ShouldNotContainCredentials()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "", "", AuthenticationType.Windows, 30, "");
            Assert.IsFalse(connStr.Contains("Username="), "Should not contain Username for GSSAPI/SSPI auth");
            Assert.IsFalse(connStr.Contains("Password="), "Should not contain Password for GSSAPI/SSPI auth");
        }

        [TestMethod]
        public void BuildConnectionString_SslModePrefer_ForPasswordAuth()
        {
            string connStr = factory.BuildConnectionString("mydb", "localhost", "pguser", "pgpass", AuthenticationType.Password, 30, "");
            Assert.IsTrue(connStr.Contains("SSL Mode=Prefer"), "Should set SSL mode to Prefer");
        }

        [TestMethod]
        public void BuildConnectionString_FromConnectionData_ShouldWork()
        {
            var connData = new ConnectionData
            {
                DatabaseName = "testdb",
                SQLServerName = "pgserver:5433",
                UserId = "user1",
                Password = "pass1",
                AuthenticationType = AuthenticationType.Password,
                ScriptTimeout = 45
            };

            string connStr = factory.BuildConnectionString(connData);

            Assert.IsTrue(connStr.Contains("Host=pgserver"), "Should parse host from ConnectionData");
            Assert.IsTrue(connStr.Contains("Port=5433"), "Should parse port from ConnectionData");
            Assert.IsTrue(connStr.Contains("Database=testdb"), "Should use database from ConnectionData");
        }

        [TestMethod]
        public void BuildConnectionString_InvalidPort_ShouldUseDefault()
        {
            string connStr = factory.BuildConnectionString("mydb", "myhost:abc", "pguser", "pgpass", AuthenticationType.Password, 30, "");
            Assert.IsTrue(connStr.Contains("Port=5432"), "Should fall back to default port for invalid port");
        }

        #endregion

        #region CreateConnection Tests

        [TestMethod]
        public void CreateConnection_ShouldReturnNpgsqlConnection()
        {
            var conn = factory.CreateConnection("mydb", "localhost", "pguser", "pgpass", AuthenticationType.Password, 30, "");
            Assert.IsInstanceOfType(conn, typeof(NpgsqlConnection));
            conn.Dispose();
        }

        [TestMethod]
        public void CreateConnection_FromConnectionData_ShouldReturnNpgsqlConnection()
        {
            var connData = new ConnectionData
            {
                DatabaseName = "mydb",
                SQLServerName = "localhost",
                UserId = "pguser",
                Password = "pgpass",
                AuthenticationType = AuthenticationType.Password,
                ScriptTimeout = 30,
                DatabasePlatform = DatabasePlatform.PostgreSQL
            };

            var conn = factory.CreateConnection(connData);
            Assert.IsInstanceOfType(conn, typeof(NpgsqlConnection));
            conn.Dispose();
        }

        #endregion

        #region CreateCommand Tests

        [TestMethod]
        public void CreateCommand_ShouldReturnNpgsqlCommand()
        {
            using var conn = new NpgsqlConnection("Host=localhost;Database=mydb;Username=pguser;Password=pgpass");
            var cmd = factory.CreateCommand("SELECT 1", conn);
            Assert.IsInstanceOfType(cmd, typeof(NpgsqlCommand));
            Assert.AreEqual("SELECT 1", cmd.CommandText);
            cmd.Dispose();
        }

        #endregion

        #region CreateParameter Tests

        [TestMethod]
        public void CreateParameter_ShouldReturnNpgsqlParameter()
        {
            var param = factory.CreateParameter("@name", "value");
            Assert.IsInstanceOfType(param, typeof(NpgsqlParameter));
            Assert.AreEqual("@name", param.ParameterName);
            Assert.AreEqual("value", param.Value);
        }

        #endregion
    }
}
