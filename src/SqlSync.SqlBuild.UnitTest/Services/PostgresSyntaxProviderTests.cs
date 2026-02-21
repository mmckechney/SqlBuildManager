using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class PostgresSyntaxProviderTests
    {
        private SqlBuild.Services.PostgresSyntaxProvider provider = null!;

        [TestInitialize]
        public void Init()
        {
            provider = new SqlBuild.Services.PostgresSyntaxProvider();
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
        public void IdentifierQuoteStart_ShouldBeDoubleQuote()
        {
            Assert.AreEqual("\"", provider.IdentifierQuoteStart);
        }

        [TestMethod]
        public void IdentifierQuoteEnd_ShouldBeDoubleQuote()
        {
            Assert.AreEqual("\"", provider.IdentifierQuoteEnd);
        }

        [TestMethod]
        public void DefaultAdminDatabase_ShouldBePostgres()
        {
            Assert.AreEqual("postgres", provider.DefaultAdminDatabase);
        }

        [TestMethod]
        public void StringConcatOperator_ShouldBePipe()
        {
            Assert.AreEqual("||", provider.StringConcatOperator);
        }

        [TestMethod]
        public void TopNRowsClause_ShouldReturnLimit()
        {
            Assert.AreEqual("LIMIT 10", provider.TopNRowsClause(10));
        }

        [TestMethod]
        public void TopNRowsClause_WithOne_ShouldReturnLimitOne()
        {
            Assert.AreEqual("LIMIT 1", provider.TopNRowsClause(1));
        }

        [TestMethod]
        public void IdentifierQuoting_DiffersFromSqlServer()
        {
            var sqlServerProvider = new SqlBuild.Services.SqlServerSyntaxProvider();
            Assert.AreNotEqual(sqlServerProvider.IdentifierQuoteStart, provider.IdentifierQuoteStart);
            Assert.AreNotEqual(sqlServerProvider.IdentifierQuoteEnd, provider.IdentifierQuoteEnd);
        }

        [TestMethod]
        public void BatchSplitting_DiffersFromSqlServer()
        {
            var sqlServerProvider = new SqlBuild.Services.SqlServerSyntaxProvider();
            Assert.AreNotEqual(sqlServerProvider.RequiresBatchSplitting, provider.RequiresBatchSplitting);
        }
    }
}
