using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.Connection.UnitTest
{
    [TestClass]
    public class SqlServerConnectionFactoryTest
    {
        private SqlServerConnectionFactory factory = null!;

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            _ = new ConnectionHelper();
        }

        [TestInitialize]
        public void TestInit()
        {
            factory = new SqlServerConnectionFactory();
        }

        #region BuildConnectionString Tests

        [TestMethod]
        public void BuildConnectionString_PasswordAuth_ShouldContainCredentials()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "myuser", "mypass", AuthenticationType.Password, 30, "");

            Assert.IsTrue(connStr.Contains("Data Source=myserver"), "Should contain Data Source");
            Assert.IsTrue(connStr.Contains("Initial Catalog=mydb"), "Should contain Initial Catalog");
            Assert.IsTrue(connStr.Contains("User ID=myuser"), "Should contain User ID");
            Assert.IsTrue(connStr.Contains("Password=mypass"), "Should contain Password");
        }

        [TestMethod]
        public void BuildConnectionString_WindowsAuth_ShouldSetIntegratedSecurity()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.Windows, 30, "");

            Assert.IsTrue(connStr.Contains("Integrated Security=True"), "Should set Integrated Security");
            Assert.IsTrue(connStr.Contains("Trust Server Certificate=True"), "Should trust server cert");
        }

        [TestMethod]
        public void BuildConnectionString_AzureADDefault_ShouldSetAuthentication()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.AzureADDefault, 30, "");

            Assert.IsTrue(connStr.Contains("Authentication=ActiveDirectoryDefault"), "Should set AD Default auth");
            Assert.IsTrue(connStr.Contains("Trust Server Certificate=True"), "Should trust server cert");
        }

        [TestMethod]
        public void BuildConnectionString_ManagedIdentity_ShouldSetClientId()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.ManagedIdentity, 30, "my-client-id");

            Assert.IsTrue(connStr.Contains("Authentication=ActiveDirectoryManagedIdentity"), "Should set MI auth");
            Assert.IsTrue(connStr.Contains("User ID=my-client-id"), "Should use client ID as User ID");
        }

        [TestMethod]
        public void BuildConnectionString_AzureADPassword_ShouldContainCredentials()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "aduser", "adpass", AuthenticationType.AzureADPassword, 30, "");

            Assert.IsTrue(connStr.Contains("Authentication=ActiveDirectoryPassword"), "Should set AD Password auth");
            Assert.IsTrue(connStr.Contains("User ID=aduser"), "Should contain User ID");
            Assert.IsTrue(connStr.Contains("Password=adpass"), "Should contain Password");
        }

        [TestMethod]
        public void BuildConnectionString_AzureADInteractive_ShouldSetAuthentication()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.AzureADInteractive, 30, "");

            Assert.IsTrue(connStr.Contains("Authentication=ActiveDirectoryInteractive"), "Should set AD Interactive auth");
        }

        [TestMethod]
        public void BuildConnectionString_AzureADIntegrated_ShouldSetAuthentication()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.AzureADIntegrated, 30, "");

            Assert.IsTrue(connStr.Contains("Authentication=ActiveDirectoryIntegrated"), "Should set AD Integrated auth");
        }

        [TestMethod]
        public void BuildConnectionString_Timeout_ShouldBeSet()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "u", "p", AuthenticationType.Password, 120, "");

            Assert.IsTrue(connStr.Contains("Connect Timeout=120"), "Should set connect timeout");
        }

        [TestMethod]
        public void BuildConnectionString_PoolingDisabled()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "u", "p", AuthenticationType.Password, 30, "");

            Assert.IsTrue(connStr.Contains("Pooling=False"), "Should have pooling disabled");
        }

        [TestMethod]
        public void BuildConnectionString_ShouldSetRetryParameters()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "u", "p", AuthenticationType.Password, 30, "");

            Assert.IsTrue(connStr.Contains("Connect Retry Count=3"), "Should set retry count");
            Assert.IsTrue(connStr.Contains("Connect Retry Interval=10"), "Should set retry interval");
        }

        [TestMethod]
        public void BuildConnectionString_FromConnectionData_ShouldWork()
        {
            var connData = new ConnectionData
            {
                DatabaseName = "testdb",
                SQLServerName = "sqlserver1",
                UserId = "user1",
                Password = "pass1",
                AuthenticationType = AuthenticationType.Password,
                ScriptTimeout = 45
            };

            string connStr = factory.BuildConnectionString(connData);

            Assert.IsTrue(connStr.Contains("Data Source=sqlserver1"), "Should use server from ConnectionData");
            Assert.IsTrue(connStr.Contains("Initial Catalog=testdb"), "Should use database from ConnectionData");
            Assert.IsTrue(connStr.Contains("Connect Timeout=45"), "Should use timeout from ConnectionData");
        }

        #endregion

        #region CreateConnection Tests

        [TestMethod]
        public void CreateConnection_ShouldReturnSqlConnection()
        {
            var conn = factory.CreateConnection("mydb", "myserver", "u", "p", AuthenticationType.Password, 30, "");
            Assert.IsInstanceOfType(conn, typeof(SqlConnection));
            conn.Dispose();
        }

        [TestMethod]
        public void CreateConnection_FromConnectionData_ShouldReturnSqlConnection()
        {
            var connData = new ConnectionData
            {
                DatabaseName = "mydb",
                SQLServerName = "myserver",
                UserId = "u",
                Password = "p",
                AuthenticationType = AuthenticationType.Password,
                ScriptTimeout = 30
            };

            var conn = factory.CreateConnection(connData);
            Assert.IsInstanceOfType(conn, typeof(SqlConnection));
            conn.Dispose();
        }

        #endregion

        #region CreateCommand Tests

        [TestMethod]
        public void CreateCommand_ShouldReturnSqlCommand()
        {
            using var conn = new SqlConnection("Data Source=fake;Initial Catalog=fake;User ID=u;Password=p;Encrypt=false");
            var cmd = factory.CreateCommand("SELECT 1", conn);
            Assert.IsInstanceOfType(cmd, typeof(SqlCommand));
            Assert.AreEqual("SELECT 1", cmd.CommandText);
            cmd.Dispose();
        }

        #endregion

        #region CreateParameter Tests

        [TestMethod]
        public void CreateParameter_ShouldReturnSqlParameter()
        {
            var param = factory.CreateParameter("@name", "value");
            Assert.IsInstanceOfType(param, typeof(SqlParameter));
            Assert.AreEqual("@name", param.ParameterName);
            Assert.AreEqual("value", param.Value);
        }

        #endregion

        #region Cross-Platform Comparison

        [TestMethod]
        public void SqlServer_ReturnssSqlConnection_PostgresReturnsNpgsqlConnection()
        {
            var pgFactory = new PostgresConnectionFactory();
            var sqlConn = factory.CreateConnection("db", "srv", "u", "p", AuthenticationType.Password, 30, "");
            var pgConn = pgFactory.CreateConnection("db", "srv", "u", "p", AuthenticationType.Password, 30, "");

            Assert.IsInstanceOfType(sqlConn, typeof(SqlConnection));
            Assert.IsInstanceOfType(pgConn, typeof(Npgsql.NpgsqlConnection));

            sqlConn.Dispose();
            pgConn.Dispose();
        }

        #endregion
    }
}
