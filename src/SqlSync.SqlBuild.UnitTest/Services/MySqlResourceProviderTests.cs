using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class MySqlResourceProviderTests
    {
        private SqlBuild.Services.MySqlResourceProvider provider = null!;

        [TestInitialize]
        public void Init()
        {
            provider = new SqlBuild.Services.MySqlResourceProvider();
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
        public void GetHasBlockingSqlLogQuery_ShouldReturnRelevantColumns()
        {
            string query = provider.GetHasBlockingSqlLogQuery();
            Assert.IsTrue(query.Contains("allowscriptblock"));
            Assert.IsTrue(query.Contains("scriptfilehash"));
        }
    }
}
