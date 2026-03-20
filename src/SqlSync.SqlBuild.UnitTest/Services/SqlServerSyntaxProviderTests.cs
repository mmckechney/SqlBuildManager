using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class SqlServerSyntaxProviderTests
    {
        private SqlBuild.Services.SqlServerSyntaxProvider provider = null!;

        [TestInitialize]
        public void Init()
        {
            provider = new SqlBuild.Services.SqlServerSyntaxProvider();
        }

        [TestMethod]
        public void BatchDelimiterPattern_ShouldNotBeNullOrEmpty()
        {
            Assert.IsFalse(string.IsNullOrWhiteSpace(provider.BatchDelimiterPattern));
        }

        [TestMethod]
        public void RequiresBatchSplitting_ShouldBeTrue()
        {
            Assert.IsTrue(provider.RequiresBatchSplitting);
        }

        [TestMethod]
        public void NoLockHint_ShouldBeWithNoLock()
        {
            Assert.AreEqual("WITH (NOLOCK)", provider.NoLockHint);
        }

        [TestMethod]
        public void IdentifierQuoteStart_ShouldBeBracket()
        {
            Assert.AreEqual("[", provider.IdentifierQuoteStart);
        }

        [TestMethod]
        public void IdentifierQuoteEnd_ShouldBeBracket()
        {
            Assert.AreEqual("]", provider.IdentifierQuoteEnd);
        }

        [TestMethod]
        public void DefaultAdminDatabase_ShouldBeMaster()
        {
            Assert.AreEqual("master", provider.DefaultAdminDatabase);
        }

        [TestMethod]
        public void StringConcatOperator_ShouldBePlus()
        {
            Assert.AreEqual("+", provider.StringConcatOperator);
        }

        [TestMethod]
        public void TopNRowsClause_ShouldReturnTopN()
        {
            Assert.AreEqual("TOP(10)", provider.TopNRowsClause(10));
        }

        [TestMethod]
        public void TopNRowsClause_WithOne_ShouldReturnTopOne()
        {
            Assert.AreEqual("TOP(1)", provider.TopNRowsClause(1));
        }

        [TestMethod]
        public void BooleanTrueLiteral_ShouldBeOne()
        {
            Assert.AreEqual("1", provider.BooleanTrueLiteral);
        }

        [TestMethod]
        public void AllProperties_DifferFromPostgres()
        {
            var pg = new SqlBuild.Services.PostgresSyntaxProvider();

            Assert.AreNotEqual(provider.RequiresBatchSplitting, pg.RequiresBatchSplitting);
            Assert.AreNotEqual(provider.NoLockHint, pg.NoLockHint);
            Assert.AreNotEqual(provider.IdentifierQuoteStart, pg.IdentifierQuoteStart);
            Assert.AreNotEqual(provider.DefaultAdminDatabase, pg.DefaultAdminDatabase);
            Assert.AreNotEqual(provider.StringConcatOperator, pg.StringConcatOperator);
            Assert.AreNotEqual(provider.BooleanTrueLiteral, pg.BooleanTrueLiteral);
        }
    }
}
