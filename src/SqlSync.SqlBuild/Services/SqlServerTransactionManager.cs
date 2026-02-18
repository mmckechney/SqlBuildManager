using Microsoft.Data.SqlClient;
using SqlSync.SqlBuild.SqlLogging;
using System;
using System.Data.Common;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// SQL Server implementation of ITransactionManager. Uses ADO.NET SqlTransaction
    /// methods for savepoint management and "no longer usable" for zombied detection.
    /// </summary>
    internal class SqlServerTransactionManager : ITransactionManager
    {
        public DbTransaction BeginTransaction(DbConnection connection)
        {
            return ((SqlConnection)connection).BeginTransaction(BuildTransaction.TransactionName);
        }

        public void CreateSavePoint(DbTransaction transaction, string savePointName)
        {
            ((SqlTransaction)transaction).Save(savePointName);
        }

        public void RollbackToSavePoint(DbTransaction transaction, string savePointName)
        {
            ((SqlTransaction)transaction).Rollback(savePointName);
        }

        public void Commit(DbTransaction transaction)
        {
            transaction.Commit();
        }

        public void Rollback(DbTransaction transaction)
        {
            transaction.Rollback();
        }

        public bool IsTransactionZombied(Exception ex)
        {
            return ex is InvalidOperationException && ex.Message.Contains("no longer usable");
        }
    }
}
