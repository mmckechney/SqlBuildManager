using Microsoft.VisualStudio.TestTools.UnitTesting;
using Moq;
using Moq.Protected;
using System;
using System.Data.Common;

namespace SqlSync.SqlBuild.UnitTest.Services
{
    [TestClass]
    public class MySqlTransactionManagerTests
    {
        private SqlBuild.Services.MySqlTransactionManager manager = null!;

        [TestInitialize]
        public void Init()
        {
            manager = new SqlBuild.Services.MySqlTransactionManager();
        }

        [TestMethod]
        public void IsTransactionZombied_WithCompletedMessage_ShouldReturnTrue()
        {
            var ex = new InvalidOperationException("This transaction has completed and is no longer usable.");
            Assert.IsTrue(manager.IsTransactionZombied(ex));
        }

        [TestMethod]
        public void IsTransactionZombied_WithUnrelatedMessage_ShouldReturnFalse()
        {
            var ex = new InvalidOperationException("Connection timed out.");
            Assert.IsFalse(manager.IsTransactionZombied(ex));
        }

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

            Assert.AreEqual("SAVEPOINT `SP1`", mockCmd.Object.CommandText);
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

            Assert.AreEqual("ROLLBACK TO SAVEPOINT `SP1`", mockCmd.Object.CommandText);
            mockCmd.Verify(c => c.ExecuteNonQuery(), Times.Once);
        }
    }
}
