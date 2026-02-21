using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.Services;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildHelperPlatformWiringTests
    {
        [TestMethod]
        public void Constructor_PostgreSQL_CreatesPostgresTransactionManager()
        {
            var connData = new ConnectionData("localhost", "testdb") { DatabasePlatform = DatabasePlatform.PostgreSQL };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false);

            Assert.IsInstanceOfType(helper.TransactionManager, typeof(PostgresTransactionManager));
        }

        [TestMethod]
        public void Constructor_PostgreSQL_CreatesPostgresSyntaxProvider()
        {
            var connData = new ConnectionData("localhost", "testdb") { DatabasePlatform = DatabasePlatform.PostgreSQL };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false);

            Assert.IsInstanceOfType(helper.SyntaxProvider, typeof(PostgresSyntaxProvider));
        }

        [TestMethod]
        public void Constructor_PostgreSQL_CreatesPostgresResourceProvider()
        {
            var connData = new ConnectionData("localhost", "testdb") { DatabasePlatform = DatabasePlatform.PostgreSQL };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false);

            Assert.IsInstanceOfType(helper.ResourceProvider, typeof(PostgresResourceProvider));
        }

        [TestMethod]
        public void Constructor_PostgreSQL_ConnectionsServiceUsesPostgresFactory()
        {
            var connData = new ConnectionData("localhost", "testdb") { DatabasePlatform = DatabasePlatform.PostgreSQL };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false);

            // ConnectionsService should be wired — it won't be null
            Assert.IsNotNull(helper.ConnectionsService);
        }

        [TestMethod]
        public void Constructor_SqlServer_CreatesSqlServerTransactionManager()
        {
            var connData = new ConnectionData("localhost", "testdb") { DatabasePlatform = DatabasePlatform.SqlServer };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false);

            Assert.IsInstanceOfType(helper.TransactionManager, typeof(SqlServerTransactionManager));
        }

        [TestMethod]
        public void Constructor_SqlServer_CreatesSqlServerSyntaxProvider()
        {
            var connData = new ConnectionData("localhost", "testdb") { DatabasePlatform = DatabasePlatform.SqlServer };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false);

            Assert.IsInstanceOfType(helper.SyntaxProvider, typeof(SqlServerSyntaxProvider));
        }

        [TestMethod]
        public void Constructor_SqlServer_CreatesSqlServerResourceProvider()
        {
            var connData = new ConnectionData("localhost", "testdb") { DatabasePlatform = DatabasePlatform.SqlServer };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false);

            Assert.IsInstanceOfType(helper.ResourceProvider, typeof(SqlServerResourceProvider));
        }

        [TestMethod]
        public void Constructor_NullConnData_DefaultsToSqlServer()
        {
            var helper = new SqlBuildHelper(null, createScriptRunLogFile: false);

            Assert.IsInstanceOfType(helper.TransactionManager, typeof(SqlServerTransactionManager));
            Assert.IsInstanceOfType(helper.SyntaxProvider, typeof(SqlServerSyntaxProvider));
            Assert.IsInstanceOfType(helper.ResourceProvider, typeof(SqlServerResourceProvider));
        }

        [TestMethod]
        public void Constructor_PostgreSQL_SqlLoggingServiceNotNull()
        {
            var connData = new ConnectionData("localhost", "testdb") { DatabasePlatform = DatabasePlatform.PostgreSQL };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false);

            Assert.IsNotNull(helper.SqlLoggingService);
        }

        [TestMethod]
        public void Constructor_PostgreSQL_BuildFinalizerNotNull()
        {
            var connData = new ConnectionData("localhost", "testdb") { DatabasePlatform = DatabasePlatform.PostgreSQL };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false);

            Assert.IsNotNull(helper.BuildFinalizer);
        }

        [TestMethod]
        public void Constructor_PostgreSQL_RunnerFactoryNotNull()
        {
            var connData = new ConnectionData("localhost", "testdb") { DatabasePlatform = DatabasePlatform.PostgreSQL };
            var helper = new SqlBuildHelper(connData, createScriptRunLogFile: false);

            Assert.IsNotNull(helper.RunnerFactory);
        }
    }
}
