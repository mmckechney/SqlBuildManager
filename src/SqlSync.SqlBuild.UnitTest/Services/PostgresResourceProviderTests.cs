using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class PostgresResourceProviderTests
    {
        private SqlBuild.Services.PostgresResourceProvider provider;

        [TestInitialize]
        public void Init()
        {
            provider = new SqlBuild.Services.PostgresResourceProvider();
        }

        [TestMethod]
        public void LoggingTableDdl_ShouldNotBeNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(provider.LoggingTableDdl));
        }

        [TestMethod]
        public void LoggingTableDdl_ShouldContainCreateTable()
        {
            Assert.IsTrue(provider.LoggingTableDdl.Contains("CREATE TABLE", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void LoggingTableDdl_ShouldUseLowercaseTableName()
        {
            Assert.IsTrue(provider.LoggingTableDdl.Contains("sqlbuild_logging"));
        }

        [TestMethod]
        public void LoggingTableCommitCheckIndex_ShouldNotBeNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(provider.LoggingTableCommitCheckIndex));
        }

        [TestMethod]
        public void LoggingTableCommitCheckIndex_ShouldContainCreateIndex()
        {
            Assert.IsTrue(provider.LoggingTableCommitCheckIndex.Contains("CREATE INDEX", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void LogScriptInsert_ShouldNotBeNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(provider.LogScriptInsert));
        }

        [TestMethod]
        public void LogScriptInsert_ShouldContainInsertInto()
        {
            Assert.IsTrue(provider.LogScriptInsert.Contains("INSERT INTO", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void LogScriptInsert_ShouldReferenceLowercaseTable()
        {
            Assert.IsTrue(provider.LogScriptInsert.Contains("sqlbuild_logging"));
        }

        [TestMethod]
        public void CheckTableExistsQuery_ShouldUseInformationSchema()
        {
            string query = provider.CheckTableExistsQuery("MyTable");
            Assert.IsTrue(query.Contains("information_schema.tables"));
        }

        [TestMethod]
        public void CheckTableExistsQuery_ShouldLowercaseTableName()
        {
            string query = provider.CheckTableExistsQuery("MyTable");
            Assert.IsTrue(query.Contains("mytable"));
            Assert.IsFalse(query.Contains("MyTable"));
        }

        [TestMethod]
        public void GetBlockingScriptLogQuery_ShouldReferenceLowercaseTable()
        {
            string query = provider.GetBlockingScriptLogQuery();
            Assert.IsTrue(query.Contains("sqlbuild_logging"));
        }

        [TestMethod]
        public void GetScriptRunLogQuery_ShouldOrderByCommitDate()
        {
            string query = provider.GetScriptRunLogQuery();
            Assert.IsTrue(query.Contains("ORDER BY commitdate DESC", System.StringComparison.OrdinalIgnoreCase));
        }

        [TestMethod]
        public void GetObjectRunHistoryQuery_ShouldFilterByScriptFileName()
        {
            string query = provider.GetObjectRunHistoryQuery();
            Assert.IsTrue(query.Contains("@ScriptFileName"));
        }

        [TestMethod]
        public void GetHasBlockingSqlLogQuery_ShouldReturnRelevantColumns()
        {
            string query = provider.GetHasBlockingSqlLogQuery();
            Assert.IsTrue(query.Contains("allowscriptblock"));
            Assert.IsTrue(query.Contains("scriptfilehash"));
        }

        [TestMethod]
        public void ResourceProvider_DiffersFromSqlServer()
        {
            var sqlProvider = new SqlBuild.Services.SqlServerResourceProvider();
            // PostgreSQL uses lowercase table names
            Assert.IsFalse(provider.LoggingTableDdl.Contains("SqlBuild_Logging"));
        }
    }
}
