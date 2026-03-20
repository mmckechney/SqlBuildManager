using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Data.Common;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class PostgresTransactionManagerTests
    {
        private SqlBuild.Services.PostgresTransactionManager manager = null!;

        [TestInitialize]
        public void Init()
        {
            manager = new SqlBuild.Services.PostgresTransactionManager();
        }

        #region IsTransactionZombied

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
            var pgError = new Exception("current transaction is aborted");
            var sqlError = new InvalidOperationException("This SqlTransaction has completed; it is no longer usable.");

            Assert.IsTrue(manager.IsTransactionZombied(pgError));
            Assert.IsFalse(manager.IsTransactionZombied(sqlError));
            Assert.IsTrue(sqlServerManager.IsTransactionZombied(sqlError));
            Assert.IsFalse(sqlServerManager.IsTransactionZombied(pgError));
        }

        #endregion

        #region Commit and Rollback (via DbTransaction mock)

        [TestMethod]
        public void Commit_ShouldCallTransactionCommit()
        {
            var mockTxn = new Mock<DbTransaction>();
            manager.Commit(mockTxn.Object);
            mockTxn.Verify(t => t.Commit(), Times.Once);
        }

        [TestMethod]
        public void Rollback_ShouldCallTransactionRollback()
        {
            var mockTxn = new Mock<DbTransaction>();
            manager.Rollback(mockTxn.Object);
            mockTxn.Verify(t => t.Rollback(), Times.Once);
        }

        #endregion

        #region CreateSavePoint (uses SQL command)

        [TestMethod]
        public void CreateSavePoint_ShouldExecuteSavepointCommand()
        {
            var mockCmd = new Mock<DbCommand>();
            mockCmd.SetupAllProperties();
            var mockConn = new Mock<DbConnection>();
            mockConn.Protected().Setup<DbCommand>("CreateDbCommand").Returns(mockCmd.Object);
            var mockTxn = new Mock<DbTransaction>();
            mockTxn.Protected().Setup<DbConnection>("DbConnection").Returns(mockConn.Object);

            manager.CreateSavePoint(mockTxn.Object, "SP1");

            Assert.AreEqual("SAVEPOINT \"SP1\"", mockCmd.Object.CommandText);
            mockCmd.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        [TestMethod]
        public void RollbackToSavePoint_ShouldExecuteRollbackCommand()
        {
            var mockCmd = new Mock<DbCommand>();
            mockCmd.SetupAllProperties();
            var mockConn = new Mock<DbConnection>();
            mockConn.Protected().Setup<DbCommand>("CreateDbCommand").Returns(mockCmd.Object);
            var mockTxn = new Mock<DbTransaction>();
            mockTxn.Protected().Setup<DbConnection>("DbConnection").Returns(mockConn.Object);

            manager.RollbackToSavePoint(mockTxn.Object, "SP1");

            Assert.AreEqual("ROLLBACK TO SAVEPOINT \"SP1\"", mockCmd.Object.CommandText);
            mockCmd.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }

        #endregion

        #region BeginTransaction

        [TestMethod]
        public void BeginTransaction_ShouldCallConnectionBeginTransaction()
        {
            var mockTxn = new Mock<DbTransaction>();
            var mockConn = new Mock<DbConnection>();
            mockConn.Protected().Setup<DbTransaction>("BeginDbTransaction", System.Data.IsolationLevel.Unspecified).Returns(mockTxn.Object);

            var result = manager.BeginTransaction(mockConn.Object);

            Assert.IsNotNull(result);
        }

        #endregion
    }
}
