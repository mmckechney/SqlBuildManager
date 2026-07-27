using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlBuildManager.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class MySqlSyntaxProviderTests
    {
        private SqlBuild.Services.MySqlSyntaxProvider provider = null!;

        [TestInitialize]
        public void Init()
        {
            provider = new SqlBuild.Services.MySqlSyntaxProvider();
        }

        [TestMethod]
        public void BatchDelimiterPattern_ShouldBeNull()
        {
            Assert.IsNull(provider.BatchDelimiterPattern);
        }

        [TestMethod]
        public void RequiresBatchSplitting_ShouldBeFalse()
        {
            Assert.IsFalse(provider.RequiresBatchSplitting);
        }

        [TestMethod]
        public void NoLockHint_ShouldBeEmpty()
        {
            Assert.AreEqual(string.Empty, provider.NoLockHint);
        }

        [TestMethod]
        public void IdentifierQuoteStart_ShouldBeBacktick()
        {
            Assert.AreEqual("`", provider.IdentifierQuoteStart);
        }

        [TestMethod]
        public void IdentifierQuoteEnd_ShouldBeBacktick()
        {
            Assert.AreEqual("`", provider.IdentifierQuoteEnd);
        }

        [TestMethod]
        public void DefaultAdminDatabase_ShouldBeMySql()
        {
            Assert.AreEqual("mysql", provider.DefaultAdminDatabase);
        }

        [TestMethod]
        public void TopNRowsClause_ShouldReturnLimit()
        {
            Assert.AreEqual("LIMIT 10", provider.TopNRowsClause(10));
        }

        [TestMethod]
        public void BooleanTrueLiteral_ShouldBeOne()
        {
            Assert.AreEqual("1", provider.BooleanTrueLiteral);
        }
    }
}
