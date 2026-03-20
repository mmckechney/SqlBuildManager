using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.Connection.UnitTest
{
    [TestClass]
    public class ConnectionHelperGetFactoryTest
    {
        [TestMethod]
        public void GetFactory_SqlServer_ReturnsSqlServerConnectionFactory()
        {
            var factory = ConnectionHelper.GetFactory(DatabasePlatform.SqlServer);
            Assert.IsInstanceOfType(factory, typeof(SqlServerConnectionFactory));
        }

        [TestMethod]
        public void GetFactory_PostgreSQL_ReturnsPostgresConnectionFactory()
        {
            var factory = ConnectionHelper.GetFactory(DatabasePlatform.PostgreSQL);
            Assert.IsInstanceOfType(factory, typeof(PostgresConnectionFactory));
        }

        [TestMethod]
        public void GetFactory_FromConnectionData_SqlServer_ReturnsSqlServerFactory()
        {
            var connData = new ConnectionData { DatabasePlatform = DatabasePlatform.SqlServer };
            var factory = ConnectionHelper.GetFactory(connData);
            Assert.IsInstanceOfType(factory, typeof(SqlServerConnectionFactory));
        }

        [TestMethod]
        public void GetFactory_FromConnectionData_PostgreSQL_ReturnsPostgresFactory()
        {
            var connData = new ConnectionData { DatabasePlatform = DatabasePlatform.PostgreSQL };
            var factory = ConnectionHelper.GetFactory(connData);
            Assert.IsInstanceOfType(factory, typeof(PostgresConnectionFactory));
        }

        [TestMethod]
        public void GetFactory_FromNullConnectionData_ReturnsSqlServerFactory()
        {
            var factory = ConnectionHelper.GetFactory((ConnectionData)null!);
            Assert.IsInstanceOfType(factory, typeof(SqlServerConnectionFactory));
        }

        [TestMethod]
        public void GetDbConnection_PostgreSQL_ReturnsNpgsqlConnection()
        {
            var connData = new ConnectionData
            {
                SQLServerName = "localhost",
                DatabaseName = "mydb",
                UserId = "pguser",
                Password = "pgpass",
                AuthenticationType = AuthenticationType.Password,
                DatabasePlatform = DatabasePlatform.PostgreSQL
            };

            var conn = ConnectionHelper.GetDbConnection(connData);
            Assert.IsInstanceOfType(conn, typeof(Npgsql.NpgsqlConnection));
            conn.Dispose();
        }

        [TestMethod]
        public void GetDbConnection_SqlServer_ReturnsSqlConnection()
        {
            var connData = new ConnectionData
            {
                SQLServerName = "localhost",
                DatabaseName = "mydb",
                UserId = "user",
                Password = "pass",
                AuthenticationType = AuthenticationType.Password,
                DatabasePlatform = DatabasePlatform.SqlServer
            };

            var conn = ConnectionHelper.GetDbConnection(connData);
            Assert.IsInstanceOfType(conn, typeof(Microsoft.Data.SqlClient.SqlConnection));
            conn.Dispose();
        }

        [TestMethod]
        public void GetDbConnection_NullConnData_ReturnsNull()
        {
            var conn = ConnectionHelper.GetDbConnection(null!);
            Assert.IsNull(conn);
        }
    }
}
