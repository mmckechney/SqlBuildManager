using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using System;

namespace SqlSync.Connection.Dependent.UnitTest
{
    /// <summary>
    /// Integration tests for PostgreSQL connectivity.
    /// Requires a local PostgreSQL instance. Configure via environment variables:
    ///   SBM_TEST_POSTGRES_SERVER (default: localhost)
    ///   SBM_TEST_POSTGRES_USER (default: postgres)
    ///   SBM_TEST_POSTGRES_PASSWORD (required, or set to test default)
    /// </summary>
    [TestClass]
    public class PostgresConnectionHelperTest
    {
        private static string PgServer => Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_SERVER") ?? "localhost";
        private static string PgUser => Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_USER") ?? "postgres";
        private static string PgPassword => Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_PASSWORD") ?? "P0stSqlAdm1n";

        private static bool IsPostgresAvailable()
        {
            try
            {
                using var conn = new NpgsqlConnection($"Host={PgServer};Database=postgres;Username={PgUser};Password={PgPassword};Timeout=5");
                conn.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        [ClassInitialize]
        public static void ClassInit(TestContext context)
        {
            if (!IsPostgresAvailable())
                Assert.Inconclusive("PostgreSQL is not available. Set SBM_TEST_POSTGRES_SERVER, SBM_TEST_POSTGRES_USER, SBM_TEST_POSTGRES_PASSWORD environment variables.");
        }

        [TestMethod]
        public void TestDatabaseConnection_PostgreSQL_PasswordAuth_Success()
        {
            var connData = new ConnectionData
            {
                SQLServerName = PgServer,
                DatabaseName = "postgres",
                UserId = PgUser,
                Password = PgPassword,
                AuthenticationType = AuthenticationType.Password,
                DatabasePlatform = DatabasePlatform.PostgreSQL,
                ScriptTimeout = 20
            };

            bool result = ConnectionHelper.TestDatabaseConnection(connData);
            Assert.IsTrue(result, $"Should connect to PostgreSQL at {PgServer}");
        }

        [TestMethod]
        public void TestDatabaseConnection_PostgreSQL_BadDatabase_Failure()
        {
            var connData = new ConnectionData
            {
                SQLServerName = PgServer,
                DatabaseName = "this_db_does_not_exist_xyz",
                UserId = PgUser,
                Password = PgPassword,
                AuthenticationType = AuthenticationType.Password,
                DatabasePlatform = DatabasePlatform.PostgreSQL,
                ScriptTimeout = 10
            };

            bool result = ConnectionHelper.TestDatabaseConnection(connData);
            Assert.IsFalse(result, "Should fail for non-existent database");
        }

        [TestMethod]
        public void TestDatabaseConnection_PostgreSQL_BadServer_Failure()
        {
            var connData = new ConnectionData
            {
                SQLServerName = "badserver_that_doesnt_exist",
                DatabaseName = "postgres",
                UserId = PgUser,
                Password = PgPassword,
                AuthenticationType = AuthenticationType.Password,
                DatabasePlatform = DatabasePlatform.PostgreSQL,
                ScriptTimeout = 5
            };

            bool result = ConnectionHelper.TestDatabaseConnection(connData);
            Assert.IsFalse(result, "Should fail for non-existent server");
        }

        [TestMethod]
        public void GetDbConnection_PostgreSQL_ReturnsNpgsqlConnection()
        {
            var connData = new ConnectionData
            {
                SQLServerName = PgServer,
                DatabaseName = "postgres",
                UserId = PgUser,
                Password = PgPassword,
                AuthenticationType = AuthenticationType.Password,
                DatabasePlatform = DatabasePlatform.PostgreSQL,
                ScriptTimeout = 20
            };

            using var conn = ConnectionHelper.GetDbConnection(connData);
            Assert.IsInstanceOfType(conn, typeof(NpgsqlConnection));
            conn.Open();
            Assert.AreEqual(System.Data.ConnectionState.Open, conn.State);
        }

        [TestMethod]
        public void PostgresConnectionFactory_CreateConnection_CanExecuteQuery()
        {
            var factory = new PostgresConnectionFactory();
            using var conn = factory.CreateConnection("postgres", PgServer, PgUser, PgPassword, AuthenticationType.Password, 20, "");

            conn.Open();
            using var cmd = factory.CreateCommand("SELECT 1 AS result", conn);
            var result = cmd.ExecuteScalar();
            Assert.AreEqual(1, result);
        }

        [TestMethod]
        public void PostgresConnectionFactory_WithCustomPort_CanConnect()
        {
            // Default PostgreSQL port is 5432
            var factory = new PostgresConnectionFactory();
            using var conn = factory.CreateConnection("postgres", $"{PgServer}:5432", PgUser, PgPassword, AuthenticationType.Password, 20, "");

            conn.Open();
            Assert.AreEqual(System.Data.ConnectionState.Open, conn.State);
        }
    }
}
