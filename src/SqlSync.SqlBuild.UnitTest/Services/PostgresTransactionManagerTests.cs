using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class PostgresTransactionManagerTests
    {
        private SqlBuild.Services.PostgresTransactionManager manager;

        [TestInitialize]
        public void Init()
        {
            manager = new SqlBuild.Services.PostgresTransactionManager();
        }

        [TestMethod]
        public void IsTransactionZombied_WithAbortedMessage_ShouldReturnTrue()
        {
            var ex = new Exception("ERROR: current transaction is aborted, commands ignored until end of transaction block");
            Assert.IsTrue(manager.IsTransactionZombied(ex));
        }

        [TestMethod]
        public void IsTransactionZombied_WithUpperCaseAbortedMessage_ShouldReturnTrue()
        {
            var ex = new Exception("CURRENT TRANSACTION IS ABORTED");
            Assert.IsTrue(manager.IsTransactionZombied(ex));
        }

        [TestMethod]
        public void IsTransactionZombied_WithUnrelatedMessage_ShouldReturnFalse()
        {
            var ex = new Exception("Connection timed out");
            Assert.IsFalse(manager.IsTransactionZombied(ex));
        }

        [TestMethod]
        public void IsTransactionZombied_WithEmptyMessage_ShouldReturnFalse()
        {
            var ex = new Exception("");
            Assert.IsFalse(manager.IsTransactionZombied(ex));
        }

        [TestMethod]
        public void IsTransactionZombied_DiffersFromSqlServer()
        {
            var sqlServerManager = new SqlBuild.Services.SqlServerTransactionManager();
            // SQL Server looks for InvalidOperationException with "no longer usable"
            // PostgreSQL looks for "current transaction is aborted" in any exception
            var pgError = new Exception("current transaction is aborted");
            var sqlError = new InvalidOperationException("This SqlTransaction has completed; it is no longer usable.");

            Assert.IsTrue(manager.IsTransactionZombied(pgError));
            Assert.IsFalse(manager.IsTransactionZombied(sqlError));
            Assert.IsTrue(sqlServerManager.IsTransactionZombied(sqlError));
            Assert.IsFalse(sqlServerManager.IsTransactionZombied(pgError));
        }
    }
}
