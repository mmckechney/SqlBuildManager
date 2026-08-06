using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlBuildManager.Connection.UnitTest
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

            Assert.Contains("Data Source=myserver", connStr);
            Assert.Contains("Initial Catalog=mydb", connStr);
            Assert.Contains("User ID=myuser", connStr);
            Assert.Contains("Password=mypass", connStr);
        }

        [TestMethod]
        public void RedactConnectionString_ShouldRemovePasswordValue()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "myuser", "mypass", AuthenticationType.Password, 30, "");

            string redacted = ConnectionStringRedactor.Redact(connStr);
            var redactedBuilder = new SqlConnectionStringBuilder(redacted);

            Assert.DoesNotContain("mypass", redacted, "Should not contain full password value");
            Assert.AreEqual("mypaxx", redactedBuilder.Password, "Password should be masked keeping the first 4 chars");
            Assert.AreEqual("myuser", redactedBuilder.UserID, "Should preserve non-secret connection details");
        }

        [TestMethod]
        public void BuildConnectionString_WindowsAuth_ShouldSetIntegratedSecurity()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.Windows, 30, "");

            Assert.Contains("Integrated Security=True", connStr, "Should set Integrated Security");
            Assert.DoesNotContain("Trust Server Certificate=True", connStr, "Should NOT trust server cert by default (secure-by-default)");
        }

        [TestMethod]
        public void BuildConnectionString_AzureADDefault_ShouldSetAuthentication()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.AzureADDefault, 30, "");

            Assert.Contains("Authentication=ActiveDirectoryDefault", connStr, "Should set AD Default auth");
            Assert.DoesNotContain("Trust Server Certificate=True", connStr, "Should NOT trust server cert by default (secure-by-default)");
        }

        [TestMethod]
        public void BuildConnectionString_AzureADDefault_WithManagedIdentityClientId_ShouldSetUserId()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.AzureADDefault, 30, "my-client-id");

            Assert.Contains("Authentication=ActiveDirectoryDefault", connStr, "Should set AD Default auth");
            Assert.Contains("User ID=my-client-id", connStr, "Should use managed identity client ID as User ID");
        }

        [TestMethod]
        public void Register_ShouldSetAzureIdentitySqlAuthenticationProviders()
        {
            SqlServerAuthenticationProvider.Register();

            Assert.IsNotNull(SqlAuthenticationProvider.GetProvider(SqlAuthenticationMethod.ActiveDirectoryDefault));
            Assert.IsNotNull(SqlAuthenticationProvider.GetProvider(SqlAuthenticationMethod.ActiveDirectoryManagedIdentity));
            Assert.IsNotNull(SqlAuthenticationProvider.GetProvider(SqlAuthenticationMethod.ActiveDirectoryMSI));
        }

        [TestMethod]
        public void BuildConnectionString_ManagedIdentity_ShouldSetClientId()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.ManagedIdentity, 30, "my-client-id");

            Assert.Contains("Authentication=ActiveDirectoryManagedIdentity", connStr, "Should set MI auth");
            Assert.Contains("User ID=my-client-id", connStr, "Should use client ID as User ID");
        }

        [TestMethod]
        public void BuildConnectionString_AzureADInteractive_ShouldSetAuthentication()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.AzureADInteractive, 30, "");

            Assert.Contains("Authentication=ActiveDirectoryInteractive", connStr, "Should set AD Interactive auth");
        }

        [TestMethod]
        public void BuildConnectionString_AzureADIntegrated_ShouldSetAuthentication()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.AzureADIntegrated, 30, "");

            Assert.Contains("Authentication=ActiveDirectoryIntegrated", connStr, "Should set AD Integrated auth");
        }

        [TestMethod]
        public void BuildConnectionString_Timeout_ShouldBeSet()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "u", "p", AuthenticationType.Password, 120, "");

            Assert.Contains("Connect Timeout=120", connStr, "Should set connect timeout");
        }

        [TestMethod]
        public void BuildConnectionString_PoolingEnabledWithSafeLimits()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "u", "p", AuthenticationType.Password, 30, "");

            var builder = new SqlConnectionStringBuilder(connStr);
            Assert.IsTrue(builder.Pooling);
            Assert.AreEqual(0, builder.MinPoolSize);
            Assert.AreEqual(100, builder.MaxPoolSize);
        }

        [TestMethod]
        public void BuildConnectionString_ShouldSetRetryParameters()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "u", "p", AuthenticationType.Password, 30, "");

            Assert.Contains("Connect Retry Count=3", connStr, "Should set retry count");
            Assert.Contains("Connect Retry Interval=10", connStr, "Should set retry interval");
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

            Assert.Contains("Data Source=sqlserver1", connStr, "Should use server from ConnectionData");
            Assert.Contains("Initial Catalog=testdb", connStr, "Should use database from ConnectionData");
            Assert.Contains("Connect Timeout=45", connStr, "Should use timeout from ConnectionData");
        }

        #endregion

        #region TrustServerCertificate (TLS) Tests

        [TestMethod]
        public void BuildConnectionString_PasswordAuth_DefaultsToNoTrust()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "u", "p", AuthenticationType.Password, 30, "");

            var builder = new SqlConnectionStringBuilder(connStr);
            Assert.IsFalse(builder.TrustServerCertificate, "Default (7-arg) overload must NOT trust the server certificate");
        }

        [TestMethod]
        public void BuildConnectionString_PasswordAuth_TrustOptIn_SetsTrustTrue()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "u", "p", AuthenticationType.Password, 30, "", true);

            var builder = new SqlConnectionStringBuilder(connStr);
            Assert.IsTrue(builder.TrustServerCertificate, "Opt-in (8-arg) overload must trust the server certificate when requested");
        }

        [TestMethod]
        public void BuildConnectionString_WindowsAuth_TrustOptIn_SetsTrustTrue()
        {
            string connStr = factory.BuildConnectionString("mydb", "myserver", "", "", AuthenticationType.Windows, 30, "", true);

            var builder = new SqlConnectionStringBuilder(connStr);
            Assert.IsTrue(builder.TrustServerCertificate, "Windows auth should honor the trust opt-in");
        }

        [TestMethod]
        public void BuildConnectionString_FromConnectionData_HonorsTrustServerCertificate()
        {
            var connData = new ConnectionData
            {
                DatabaseName = "testdb",
                SQLServerName = "sqlserver1",
                UserId = "user1",
                Password = "pass1",
                AuthenticationType = AuthenticationType.Password,
                ScriptTimeout = 45,
                TrustServerCertificate = true
            };

            var builder = new SqlConnectionStringBuilder(factory.BuildConnectionString(connData));
            Assert.IsTrue(builder.TrustServerCertificate, "ConnectionData.TrustServerCertificate should flow into the connection string");
        }

        [TestMethod]
        public void BuildConnectionString_FromConnectionData_DefaultsToNoTrust()
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

            var builder = new SqlConnectionStringBuilder(factory.BuildConnectionString(connData));
            Assert.IsFalse(builder.TrustServerCertificate, "ConnectionData defaults to NOT trusting the server certificate");
        }

        [TestMethod]
        public void Postgres_BuildConnectionString_IgnoresTrustServerCertificate()
        {
            var pgFactory = new PostgresConnectionFactory();
            string withTrust = pgFactory.BuildConnectionString("db", "srv", "u", "p", AuthenticationType.Password, 30, "", true);
            string withoutTrust = pgFactory.BuildConnectionString("db", "srv", "u", "p", AuthenticationType.Password, 30, "");

            Assert.AreEqual(withoutTrust, withTrust, "PostgreSQL uses SslMode and must ignore the TrustServerCertificate flag");
            Assert.DoesNotContain("Trust Server Certificate", withTrust, "PostgreSQL connection string should not contain TrustServerCertificate");
        }

        [TestMethod]
        public void BuildConnectionString_AmbientDefault_AppliesToFieldOverloads()
        {
            // The process-wide ambient default lets the operator opt in once (e.g. via
            // --trustservercertificate) so helper paths that rebuild connections from individual
            // fields honor that choice even though they don't carry a ConnectionData.
            bool original = ConnectionHelper.TrustServerCertificate;
            try
            {
                ConnectionHelper.TrustServerCertificate = true;
                var builder = new SqlConnectionStringBuilder(
                    factory.BuildConnectionString("mydb", "myserver", "u", "p", AuthenticationType.Password, 30, ""));
                Assert.IsTrue(builder.TrustServerCertificate, "Field-based overload should honor the ambient ConnectionHelper.TrustServerCertificate default");
            }
            finally
            {
                ConnectionHelper.TrustServerCertificate = original;
            }
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
