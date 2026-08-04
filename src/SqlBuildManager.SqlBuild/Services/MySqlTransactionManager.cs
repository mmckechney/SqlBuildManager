using System;
using System.Data.Common;

namespace SqlBuildManager.SqlBuild.Services
{
    /// <summary>
    /// MySQL implementation of ITransactionManager.
    /// Uses SQL commands for savepoint management.
    /// </summary>
    internal class MySqlTransactionManager : ITransactionManager
    {
        public DbTransaction BeginTransaction(DbConnection connection)
        {
            return connection.BeginTransaction();
        }

        public void CreateSavePoint(DbTransaction transaction, string savePointName)
        {
            using var cmd = transaction.Connection!.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = $"SAVEPOINT `{savePointName}`";
            cmd.ExecuteNonQuery();
        }

        public void RollbackToSavePoint(DbTransaction transaction, string savePointName)
        {
            using var cmd = transaction.Connection!.CreateCommand();
            cmd.Transaction = transaction;
            cmd.CommandText = $"ROLLBACK TO SAVEPOINT `{savePointName}`";
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
            return ex is InvalidOperationException &&
                   (ex.Message.Contains("no longer usable", StringComparison.OrdinalIgnoreCase) ||
                    ex.Message.Contains("transaction has completed", StringComparison.OrdinalIgnoreCase));
        }
    }
}
