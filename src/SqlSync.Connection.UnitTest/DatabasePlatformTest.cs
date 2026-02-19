using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.Connection.UnitTest
{
    [TestClass]
    public class DatabasePlatformTest
    {
        [TestMethod]
        public void ConnectionData_DefaultPlatform_ShouldBeSqlServer()
        {
            var connData = new ConnectionData();
            Assert.AreEqual(DatabasePlatform.SqlServer, connData.DatabasePlatform);
        }

        [TestMethod]
        public void ConnectionData_SetPostgreSQL_ShouldReturnPostgreSQL()
        {
            var connData = new ConnectionData();
            connData.DatabasePlatform = DatabasePlatform.PostgreSQL;
            Assert.AreEqual(DatabasePlatform.PostgreSQL, connData.DatabasePlatform);
        }

        [TestMethod]
        public void ConnectionData_SetSqlServer_ShouldReturnSqlServer()
        {
            var connData = new ConnectionData();
            connData.DatabasePlatform = DatabasePlatform.PostgreSQL;
            connData.DatabasePlatform = DatabasePlatform.SqlServer;
            Assert.AreEqual(DatabasePlatform.SqlServer, connData.DatabasePlatform);
        }

        [TestMethod]
        public void ConnectionData_ConstructorWithServerDb_DefaultsPlatformToSqlServer()
        {
            var connData = new ConnectionData("myserver", "mydb");
            Assert.AreEqual(DatabasePlatform.SqlServer, connData.DatabasePlatform);
        }

        [TestMethod]
        public void ConnectionData_Fill_ShouldCopyDatabasePlatform()
        {
            var source = new ConnectionData
            {
                SQLServerName = "pgserver",
                DatabaseName = "pgdb",
                DatabasePlatform = DatabasePlatform.PostgreSQL,
                AuthenticationType = AuthenticationType.Password,
                UserId = "pguser",
                Password = "pgpass"
            };
            var target = new ConnectionData();

            target.Fill(source);

            Assert.AreEqual(DatabasePlatform.PostgreSQL, target.DatabasePlatform);
        }

        [TestMethod]
        public void ConnectionData_Fill_ShouldOverwritePlatform()
        {
            var source = new ConnectionData { DatabasePlatform = DatabasePlatform.SqlServer };
            var target = new ConnectionData { DatabasePlatform = DatabasePlatform.PostgreSQL };

            target.Fill(source);

            Assert.AreEqual(DatabasePlatform.SqlServer, target.DatabasePlatform);
        }
    }
}
