using System.Data.Common;

namespace SqlSync.Connection
{
    /// <summary>
    /// Abstracts database connection creation so that both SQL Server and PostgreSQL
    /// connections can be created through the same interface.
    /// </summary>
    public interface IDbConnectionFactory
    {
        DbConnection CreateConnection(ConnectionData connData);
        DbConnection CreateConnection(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId);
        string BuildConnectionString(ConnectionData connData);
        string BuildConnectionString(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId);
        DbCommand CreateCommand(string sql, DbConnection connection, DbTransaction transaction = null);
        DbParameter CreateParameter(string name, object value);
    }
}
