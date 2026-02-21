using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Npgsql;
using System.Data.Common;

namespace SqlSync.Connection
{
    /// <summary>
    /// PostgreSQL implementation of IDbConnectionFactory using Npgsql.
    /// Supports Password, AzureADDefault, and ManagedIdentity authentication.
    /// </summary>
    public class PostgresConnectionFactory : IDbConnectionFactory
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// The Azure AD scope for Azure Database for PostgreSQL.
        /// </summary>
        private static readonly string[] PgAadScopes = new[] { "https://ossrdbms-aad.database.windows.net/.default" };

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
                    builder.SslMode = SslMode.Prefer;
                    break;
                case AuthenticationType.Windows:
                    // PostgreSQL GSSAPI/SSPI auth — no username/password needed
                    break;
                case AuthenticationType.AzureADDefault:
                case AuthenticationType.ManagedIdentity:
                    // Acquire an Azure AD token and use it as the password
                    builder.SslMode = SslMode.Require;
                    // PG Entra ID requires the identity name (role name) as the username.
                    // Use uid (identity name) if available; fall back to client ID only if no name provided.
                    builder.Username = !string.IsNullOrEmpty(uid) ? uid : managedIdentityClientId;
                    builder.Password = GetAzureAdAccessToken(managedIdentityClientId);
                    break;
                case AuthenticationType.AzureADPassword:
                    builder.Username = uid;
                    builder.Password = pw;
                    builder.SslMode = SslMode.Require;
                    break;
                case AuthenticationType.AzureADIntegrated:
                case AuthenticationType.AzureADInteractive:
                    builder.Username = uid;
                    builder.Password = pw;
                    builder.SslMode = SslMode.Require;
                    break;
            }

            log.LogDebug($"PostgreSQL Connection string: {builder.ConnectionString}");
            return builder.ConnectionString;
        }

        /// <summary>
        /// Acquires an Azure AD access token for Azure Database for PostgreSQL.
        /// Uses DefaultAzureCredential (supports MI, VS, CLI, etc.) or 
        /// ManagedIdentityCredential if a specific client ID is provided.
        /// </summary>
        private static string GetAzureAdAccessToken(string managedIdentityClientId)
        {
            try
            {
                TokenCredential credential;
                if (!string.IsNullOrEmpty(managedIdentityClientId))
                {
                    credential = new ManagedIdentityCredential(managedIdentityClientId);
                }
                else
                {
                    credential = new DefaultAzureCredential();
                }

                var tokenRequestContext = new TokenRequestContext(PgAadScopes);
                var token = credential.GetToken(tokenRequestContext, default);
                log.LogDebug("Successfully acquired Azure AD token for PostgreSQL");
                return token.Token;
            }
            catch (System.Exception ex)
            {
                log.LogError(ex, "Failed to acquire Azure AD token for PostgreSQL");
                throw;
            }
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
