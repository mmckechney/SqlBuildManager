using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace SqlSync.Connection
{
    /// <summary>
    /// PostgreSQL implementation of IDbConnectionFactory using Npgsql.
    /// </summary>
    public class PostgresConnectionFactory : IDbConnectionFactory
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        public DbConnection CreateConnection(ConnectionData connData)
        {
            return CreateConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password,
                connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
        }

        public DbConnection CreateConnection(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {
            string conn = BuildConnectionString(dbName, serverName, uid, pw, authType, scriptTimeOut, managedIdentityClientId);
            return new NpgsqlConnection(conn);
        }

        public string BuildConnectionString(ConnectionData connData)
        {
            return BuildConnectionString(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password,
                connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
        }

        public string BuildConnectionString(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {
            // Parse host:port if port is included in server name
            string host = serverName;
            int port = 5432;
            if (serverName.Contains(':'))
            {
                var parts = serverName.Split(':');
                host = parts[0];
                if (parts.Length > 1 && int.TryParse(parts[1], out int parsedPort))
                    port = parsedPort;
            }

            var builder = new NpgsqlConnectionStringBuilder();
            builder.Host = host;
            builder.Port = port;
            builder.Database = dbName;
            builder.Timeout = scriptTimeOut;
            builder.Pooling = false;
            builder.ApplicationName = ConnectionHelper.appName;

            switch (authType)
            {
                case AuthenticationType.Password:
                default:
                    builder.Username = uid;
                    builder.Password = pw;
                    // Trust self-signed certs in dev/test environments
                    builder.SslMode = SslMode.Prefer;
                    builder.TrustServerCertificate = true;
                    break;
                case AuthenticationType.Windows:
                    // PostgreSQL can use SSPI/GSSAPI on domain-joined systems via IntegratedSecurity
                    builder["Integrated Security"] = true;
                    break;
                case AuthenticationType.AzureADDefault:
                case AuthenticationType.ManagedIdentity:
                    // Azure AD/Managed Identity requires Npgsql.Authentication.AzureIdentity plugin
                    // The plugin auto-registers and provides tokens when configured
                    builder.Username = string.IsNullOrEmpty(managedIdentityClientId) ? uid : managedIdentityClientId;
                    break;
                case AuthenticationType.AzureADPassword:
                    builder.Username = uid;
                    builder.Password = pw;
                    break;
                case AuthenticationType.AzureADIntegrated:
                case AuthenticationType.AzureADInteractive:
                    // These modes are less common for PostgreSQL — fall back to password
                    builder.Username = uid;
                    builder.Password = pw;
                    break;
            }

            log.LogDebug($"PostgreSQL Connection string: {builder.ConnectionString}");
            return builder.ConnectionString;
        }

        public DbCommand CreateCommand(string sql, DbConnection connection, DbTransaction transaction = null)
        {
            var cmd = new NpgsqlCommand(sql, (NpgsqlConnection)connection);
            if (transaction != null)
                cmd.Transaction = (NpgsqlTransaction)transaction;
            return cmd;
        }

        public DbParameter CreateParameter(string name, object value)
        {
            return new NpgsqlParameter(name, value);
        }
    }
}
