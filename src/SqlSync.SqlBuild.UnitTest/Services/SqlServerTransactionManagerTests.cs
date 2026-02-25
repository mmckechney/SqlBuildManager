using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class SqlServerTransactionManagerTests
    {
        private SqlBuild.Services.SqlServerTransactionManager manager = null!;

        [TestInitialize]
        public void Init()
        {
            manager = new SqlBuild.Services.SqlServerTransactionManager();
        }

        [TestMethod]
        public void IsTransactionZombied_WithNoLongerUsableMessage_ShouldReturnTrue()
        {
            var ex = new InvalidOperationException("This SqlTransaction has completed; it is no longer usable.");
            Assert.IsTrue(manager.IsTransactionZombied(ex));
        }

        [TestMethod]
        public void IsTransactionZombied_WithPartialNoLongerUsableMessage_ShouldReturnTrue()
        {
            var ex = new InvalidOperationException("The transaction is no longer usable due to a serious error.");
            Assert.IsTrue(manager.IsTransactionZombied(ex));
        }

        [TestMethod]
        public void IsTransactionZombied_WithUnrelatedInvalidOperationException_ShouldReturnFalse()
        {
            var ex = new InvalidOperationException("Connection is closed.");
            Assert.IsFalse(manager.IsTransactionZombied(ex));
        }

        [TestMethod]
        public void IsTransactionZombied_WithNonInvalidOperationException_ShouldReturnFalse()
        {
            // Must be InvalidOperationException, not just any exception
            var ex = new Exception("no longer usable");
            Assert.IsFalse(manager.IsTransactionZombied(ex));
        }

        [TestMethod]
        public void IsTransactionZombied_WithPostgresStyleMessage_ShouldReturnFalse()
        {
            // SQL Server should not detect PostgreSQL-style zombied transactions
            var ex = new InvalidOperationException("current transaction is aborted");
            Assert.IsFalse(manager.IsTransactionZombied(ex));
        }

        [TestMethod]
        public void IsTransactionZombied_WithEmptyMessage_ShouldReturnFalse()
        {
            var ex = new InvalidOperationException("");
            Assert.IsFalse(manager.IsTransactionZombied(ex));
        }
    }
}
