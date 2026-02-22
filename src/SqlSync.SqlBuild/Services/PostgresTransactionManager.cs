using System;
using System.Data.Common;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// PostgreSQL implementation of ITransactionManager.
    /// Uses SQL commands for savepoint management since Npgsql doesn't expose
    /// savepoint methods on DbTransaction directly.
    /// </summary>
    internal class PostgresTransactionManager : ITransactionManager
    {
        public DbTransaction BeginTransaction(DbConnection connection)
        {
            return connection.BeginTransaction();
        }

        public void CreateSavePoint(DbTransaction transaction, string savePointName)
        {
            using var cmd = transaction.Connection!.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = $"SAVEPOINT \"{savePointName}\"";
            cmd.ExecuteNonQuery();
        }

        public void RollbackToSavePoint(DbTransaction transaction, string savePointName)
        {
            using var cmd = transaction.Connection!.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = $"ROLLBACK TO SAVEPOINT \"{savePointName}\"";
            cmd.ExecuteNonQuery();
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
            // PostgreSQL sets the transaction to an error state when any statement fails.
            // Npgsql throws PostgresException with "current transaction is aborted" message.
            return ex.Message.Contains("current transaction is aborted", StringComparison.OrdinalIgnoreCase);
        }
    }
}
