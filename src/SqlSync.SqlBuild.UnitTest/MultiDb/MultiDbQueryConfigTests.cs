using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.MultiDb;

namespace SqlSync.SqlBuild.UnitTest.MultiDb
{
    [TestClass]
    public class MultiDbQueryConfigTests
    {
        [TestMethod]
        public void Constructor_Default_InitializesEmptyProperties()
        {
            // Act
            var config = new MultiDbQueryConfig();

            // Assert
            Assert.AreEqual(string.Empty, config.SourceServer);
            Assert.AreEqual(string.Empty, config.Database);
            Assert.AreEqual(string.Empty, config.Query);
        }

        [TestMethod]
        public void Constructor_WithParameters_SetsAllProperties()
        {
            // Arrange
            string sourceServer = "TestServer";
            string database = "TestDatabase";
            string query = "SELECT * FROM Table1";

            // Act
            var config = new MultiDbQueryConfig(sourceServer, database, query);

            // Assert
            Assert.AreEqual(sourceServer, config.SourceServer);
            Assert.AreEqual(database, config.Database);
            Assert.AreEqual(query, config.Query);
        }

        [TestMethod]
        public void SourceServer_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var config = new MultiDbQueryConfig();
            string expected = "MyServer";

            // Act
            config.SourceServer = expected;

            // Assert
            Assert.AreEqual(expected, config.SourceServer);
        }

        [TestMethod]
        public void Database_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var config = new MultiDbQueryConfig();
            string expected = "MyDatabase";

            // Act
            config.Database = expected;

            // Assert
            Assert.AreEqual(expected, config.Database);
        }

        [TestMethod]
        public void Query_SetAndGet_ReturnsCorrectValue()
        {
            // Arrange
            var config = new MultiDbQueryConfig();
            string expected = "SELECT COUNT(*) FROM Users";

            // Act
            config.Query = expected;

            // Assert
            Assert.AreEqual(expected, config.Query);
        }

        [TestMethod]
        public void Properties_SetMultipleTimes_ReturnsLastValue()
        {
            // Arrange
            var config = new MultiDbQueryConfig();

            // Act
            config.SourceServer = "Server1";
            config.SourceServer = "Server2";
            config.Database = "DB1";
            config.Database = "DB2";
            config.Query = "Query1";
            config.Query = "Query2";

            // Assert
            Assert.AreEqual("Server2", config.SourceServer);
            Assert.AreEqual("DB2", config.Database);
            Assert.AreEqual("Query2", config.Query);
        }

        [TestMethod]
        public void Properties_SetToNull_AcceptsNullValues()
        {
            // Arrange
            var config = new MultiDbQueryConfig("Server", "DB", "Query");

            // Act
            config.SourceServer = null;
            config.Database = null;
            config.Query = null;

            // Assert
            Assert.IsNull(config.SourceServer);
            Assert.IsNull(config.Database);
            Assert.IsNull(config.Query);
        }

        [TestMethod]
        public void Constructor_WithSpecialCharacters_HandlesCorrectly()
        {
            // Arrange
            string sourceServer = "Server\\Instance";
            string database = "DB-Name_Test";
            string query = "SELECT * FROM [Table With Spaces] WHERE Col = 'Value'";

            // Act
            var config = new MultiDbQueryConfig(sourceServer, database, query);

            // Assert
            Assert.AreEqual(sourceServer, config.SourceServer);
            Assert.AreEqual(database, config.Database);
            Assert.AreEqual(query, config.Query);
        }
    }
}
