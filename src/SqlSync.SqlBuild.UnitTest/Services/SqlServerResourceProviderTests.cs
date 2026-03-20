using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class SqlServerResourceProviderTests
    {
        private SqlBuild.Services.SqlServerResourceProvider provider = null!;

        [TestInitialize]
        public void Init()
        {
            provider = new SqlBuild.Services.SqlServerResourceProvider();
        }

        #region LoggingTableDdl

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
        public void LoggingTableDdl_ShouldUsePascalCaseTableName()
        {
            Assert.IsTrue(provider.LoggingTableDdl.Contains("SqlBuild_Logging"));
        }

        #endregion

        #region LoggingTableCommitCheckIndex

        [TestMethod]
        public void LoggingTableCommitCheckIndex_ShouldNotBeNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(provider.LoggingTableCommitCheckIndex));
        }

        [TestMethod]
        public void LoggingTableCommitCheckIndex_ShouldContainCreateIndex()
        {
            Assert.IsTrue(provider.LoggingTableCommitCheckIndex.Contains("CREATE", System.StringComparison.OrdinalIgnoreCase));
        }

        #endregion

        #region LogScriptInsert

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
        public void LogScriptInsert_ShouldReferenceSqlBuildLogging()
        {
            Assert.IsTrue(provider.LogScriptInsert.Contains("SqlBuild_Logging"));
        }

        #endregion

        #region CheckTableExistsQuery

        [TestMethod]
        public void CheckTableExistsQuery_ShouldUseSysObjects()
        {
            string query = provider.CheckTableExistsQuery("MyTable");
            Assert.IsTrue(query.Contains("sys.objects"));
        }

        [TestMethod]
        public void CheckTableExistsQuery_ShouldContainTableName()
        {
            string query = provider.CheckTableExistsQuery("TestTable");
            Assert.IsTrue(query.Contains("TestTable"));
        }

        [TestMethod]
        public void CheckTableExistsQuery_ShouldFilterUserTables()
        {
            string query = provider.CheckTableExistsQuery("MyTable");
            Assert.IsTrue(query.Contains("type = 'U'"));
        }

        [TestMethod]
        public void CheckTableExistsQuery_ShouldUseNoLock()
        {
            string query = provider.CheckTableExistsQuery("MyTable");
            Assert.IsTrue(query.Contains("NOLOCK"));
        }

        #endregion

        #region Query Methods

        [TestMethod]
        public void GetBlockingScriptLogQuery_ShouldReferenceSqlBuildLogging()
        {
            string query = provider.GetBlockingScriptLogQuery();
            Assert.IsTrue(query.Contains("SqlBuild_Logging"));
        }

        [TestMethod]
        public void GetBlockingScriptLogQuery_ShouldFilterByAllowScriptBlock()
        {
            string query = provider.GetBlockingScriptLogQuery();
            Assert.IsTrue(query.Contains("AllowScriptBlock = 1"));
        }

        [TestMethod]
        public void GetScriptRunLogQuery_ShouldOrderByCommitDateDesc()
        {
            string query = provider.GetScriptRunLogQuery();
            Assert.IsTrue(query.Contains("ORDER BY CommitDate DESC"));
        }

        [TestMethod]
        public void GetScriptRunLogQuery_ShouldUseNoLock()
        {
            string query = provider.GetScriptRunLogQuery();
            Assert.IsTrue(query.Contains("NOLOCK"));
        }

        [TestMethod]
        public void GetObjectRunHistoryQuery_ShouldFilterByScriptFileName()
        {
            string query = provider.GetObjectRunHistoryQuery();
            Assert.IsTrue(query.Contains("@ScriptFileName"));
        }

        [TestMethod]
        public void GetObjectRunHistoryQuery_ShouldUseNoLock()
        {
            string query = provider.GetObjectRunHistoryQuery();
            Assert.IsTrue(query.Contains("NOLOCK"));
        }

        [TestMethod]
        public void GetHasBlockingSqlLogQuery_ShouldReturnRelevantColumns()
        {
            string query = provider.GetHasBlockingSqlLogQuery();
            Assert.IsTrue(query.Contains("AllowScriptBlock"));
            Assert.IsTrue(query.Contains("ScriptFileHash"));
            Assert.IsTrue(query.Contains("CommitDate"));
            Assert.IsTrue(query.Contains("ScriptText"));
        }

        #endregion

        #region Cross-Platform Comparison

        [TestMethod]
        public void SqlServer_UsesUpperCaseTableName_PostgresUsesLowercase()
        {
            var pgProvider = new SqlBuild.Services.PostgresResourceProvider();
            Assert.IsTrue(provider.LoggingTableDdl.Contains("SqlBuild_Logging"));
            Assert.IsTrue(pgProvider.LoggingTableDdl.Contains("sqlbuild_logging"));
        }

        [TestMethod]
        public void SqlServer_UsesSysObjects_PostgresUsesInformationSchema()
        {
            var pgProvider = new SqlBuild.Services.PostgresResourceProvider();
            Assert.IsTrue(provider.CheckTableExistsQuery("T").Contains("sys.objects"));
            Assert.IsTrue(pgProvider.CheckTableExistsQuery("T").Contains("information_schema"));
        }

        #endregion
    }
}
