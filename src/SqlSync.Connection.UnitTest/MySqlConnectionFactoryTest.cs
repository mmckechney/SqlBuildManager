using Microsoft.VisualStudio.TestTools.UnitTesting;
using MySqlConnector;

namespace SqlSync.Connection.UnitTest
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

            Assert.IsTrue(connStr.Contains("Server=localhost"), "Should contain Server");
            Assert.IsTrue(connStr.Contains("Database=mydb"), "Should contain Database");
            Assert.IsTrue(connStr.Contains("User ID=mysqluser"), "Should contain User ID");
            Assert.IsTrue(connStr.Contains("******"), "Should contain Password");
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
