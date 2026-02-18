using System;
using System.Data.Common;

namespace SqlSync.SqlBuild.Services
{
    /// <summary>
    /// Abstracts transaction management to support different database providers.
    /// SQL Server and PostgreSQL have different savepoint mechanisms.
    /// </summary>
    public interface ITransactionManager
    {
        DbTransaction BeginTransaction(DbConnection connection);
        void CreateSavePoint(DbTransaction transaction, string savePointName);
        void RollbackToSavePoint(DbTransaction transaction, string savePointName);
        void Commit(DbTransaction transaction);
        void Rollback(DbTransaction transaction);
        bool IsTransactionZombied(Exception ex);
    }
}
