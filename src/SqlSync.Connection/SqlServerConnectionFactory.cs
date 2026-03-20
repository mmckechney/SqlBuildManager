using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System.Data.Common;

namespace SqlSync.Connection
{
    /// <summary>
    /// SQL Server implementation of IDbConnectionFactory wrapping existing ConnectionHelper logic.
    /// </summary>
    public class SqlServerConnectionFactory : IDbConnectionFactory
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        public DbConnection CreateConnection(ConnectionData connData)
        {
            return CreateConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password,
                connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
        }

        public DbConnection CreateConnection(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {
            string conn = BuildConnectionString(dbName, serverName, uid, pw, authType, scriptTimeOut, managedIdentityClientId);
            return new SqlConnection(conn);
        }

        public string BuildConnectionString(ConnectionData connData)
        {
            return BuildConnectionString(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password,
                connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
        }

        public string BuildConnectionString(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {
            var builder = new SqlConnectionStringBuilder();
            builder.DataSource = serverName;
            builder.InitialCatalog = dbName;
            builder.ConnectTimeout = scriptTimeOut;
            builder.Pooling = false;
            builder.ApplicationName = ConnectionHelper.appName;
            builder.ConnectRetryCount = 3;
            builder.ConnectRetryInterval = 10;

            switch (authType)
            {
                case AuthenticationType.Windows:
                    builder.IntegratedSecurity = true;
                    builder.TrustServerCertificate = true;
                    break;
                case AuthenticationType.AzureADIntegrated:
                    builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
                    builder.IntegratedSecurity = true;
                    builder.TrustServerCertificate = true;
                    break;
                case AuthenticationType.AzureADDefault:
                    builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryDefault;
                    builder.TrustServerCertificate = true;
                    break;
                case AuthenticationType.AzureADPassword:
                    builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryPassword;
                    builder.UserID = uid;
                    builder.Password = pw;
                    break;
                case AuthenticationType.ManagedIdentity:
                    builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryManagedIdentity;
                    builder.UserID = managedIdentityClientId;
                    break;
                case AuthenticationType.AzureADInteractive:
                    builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryInteractive;
                    break;
                case AuthenticationType.Password:
                default:
                    builder.Authentication = SqlAuthenticationMethod.SqlPassword;
                    builder.UserID = uid;
                    builder.Password = pw;
                    builder.TrustServerCertificate = true;
                    break;
            }
            log.LogDebug($"Database Connection string: {builder.ConnectionString}");
            return builder.ConnectionString;
        }

        public DbCommand CreateCommand(string sql, DbConnection connection, DbTransaction transaction = null!)
        {
            var cmd = new SqlCommand(sql, (SqlConnection)connection);
            if (transaction != null)
                cmd.Transaction = (SqlTransaction)transaction;
            return cmd;
        }

        public DbParameter CreateParameter(string name, object value)
        {
            return new SqlParameter(name, value);
        }
    }
}
