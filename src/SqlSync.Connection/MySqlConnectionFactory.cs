using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using MySqlConnector;
using System;
using System.Collections.Concurrent;
using System.Data.Common;

namespace SqlSync.Connection
{
    /// <summary>
    /// MySQL implementation of IDbConnectionFactory using MySqlConnector.
    /// Supports Password, AzureADDefault, and ManagedIdentity authentication.
    /// </summary>
    public class MySqlConnectionFactory : IDbConnectionFactory
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        /// <summary>
        /// The Azure AD scope for Azure Database for MySQL Flexible Server.
        /// </summary>
        private static readonly string[] MySqlAadScopes = new[] { "https://ossrdbms-aad.database.windows.net/.default" };
        private static readonly TimeSpan TokenRefreshBuffer = TimeSpan.FromMinutes(5);
        private static readonly ConcurrentDictionary<string, TokenCredential> Credentials = new();
        private static readonly ConcurrentDictionary<string, AccessToken> Tokens = new();
        private static readonly ConcurrentDictionary<string, object> TokenLocks = new();

        public DbConnection CreateConnection(ConnectionData connData)
        {
            return CreateConnection(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password,
                connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
        }

        public DbConnection CreateConnection(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {
            string conn = BuildConnectionString(dbName, serverName, uid, pw, authType, scriptTimeOut, managedIdentityClientId);
            return new MySqlConnection(conn);
        }

        // MySQL TLS behavior is controlled via SslMode; trustServerCertificate is accepted for interface parity and ignored.
        public DbConnection CreateConnection(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId, bool trustServerCertificate)
        {
            return CreateConnection(dbName, serverName, uid, pw, authType, scriptTimeOut, managedIdentityClientId);
        }

        public string BuildConnectionString(ConnectionData connData)
        {
            return BuildConnectionString(connData.DatabaseName, connData.SQLServerName, connData.UserId, connData.Password,
                connData.AuthenticationType, connData.ScriptTimeout, connData.ManagedIdentityClientId);
        }

        public string BuildConnectionString(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {
            string host = serverName;
            uint port = 3306;
            if (serverName.Contains(':'))
            {
                var parts = serverName.Split(':');
                host = parts[0];
                if (parts.Length > 1 && uint.TryParse(parts[1], out uint parsedPort))
                {
                    port = parsedPort;
                }
            }

            var builder = new MySqlConnectionStringBuilder
            {
                Server = host,
                Port = port,
                Database = dbName,
                ConnectionTimeout = (uint)Math.Max(scriptTimeOut, 0),
                Pooling = true,
                MinimumPoolSize = (uint)ConnectionHelper.MinimumPoolSize,
                MaximumPoolSize = (uint)ConnectionHelper.MaximumPoolSize,
                ApplicationName = ConnectionHelper.appName
            };

            switch (authType)
            {
                case AuthenticationType.Password:
                default:
                    builder.UserID = uid;
                    builder.Password = pw;
                    builder.SslMode = MySqlSslMode.Preferred;
                    break;
                case AuthenticationType.Windows:
                    // MySQL Windows integrated authentication is not supported by this client path.
                    break;
                case AuthenticationType.AzureADDefault:
                case AuthenticationType.ManagedIdentity:
                    builder.SslMode = MySqlSslMode.Required;
                    // MySQL Entra ID requires the identity name as the username.
                    // Use uid (identity name) if available; fall back to client ID.
                    builder.UserID = !string.IsNullOrEmpty(uid) ? uid : managedIdentityClientId;
                    builder.Password = GetAzureAdAccessToken(managedIdentityClientId);
                    break;
                case AuthenticationType.AzureADPassword:
                case AuthenticationType.AzureADIntegrated:
                case AuthenticationType.AzureADInteractive:
                    builder.UserID = uid;
                    builder.Password = pw;
                    builder.SslMode = MySqlSslMode.Required;
                    break;
            }

            log.LogDebug($"MySQL Connection string: {ConnectionStringRedactor.Redact(builder.ConnectionString)}");
            return builder.ConnectionString;
        }

        // MySQL TLS behavior is controlled via SslMode; trustServerCertificate is accepted for interface parity and ignored.
        public string BuildConnectionString(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId, bool trustServerCertificate)
        {
            return BuildConnectionString(dbName, serverName, uid, pw, authType, scriptTimeOut, managedIdentityClientId);
        }

        private static string GetAzureAdAccessToken(string managedIdentityClientId)
        {
            try
            {
                string cacheKey = string.IsNullOrWhiteSpace(managedIdentityClientId)
                    ? "<default>"
                    : managedIdentityClientId;
                if (Tokens.TryGetValue(cacheKey, out AccessToken cachedToken) &&
                    cachedToken.ExpiresOn > DateTimeOffset.UtcNow.Add(TokenRefreshBuffer))
                {
                    return cachedToken.Token;
                }

                lock (TokenLocks.GetOrAdd(cacheKey, _ => new object()))
                {
                    if (Tokens.TryGetValue(cacheKey, out cachedToken) &&
                        cachedToken.ExpiresOn > DateTimeOffset.UtcNow.Add(TokenRefreshBuffer))
                    {
                        return cachedToken.Token;
                    }

                    TokenCredential credential = Credentials.GetOrAdd(cacheKey, _ =>
                        string.IsNullOrWhiteSpace(managedIdentityClientId)
                            ? new DefaultAzureCredential()
                            : new ManagedIdentityCredential(
                                ManagedIdentityId.FromUserAssignedClientId(managedIdentityClientId)));

                    var tokenRequestContext = new TokenRequestContext(MySqlAadScopes);
                    AccessToken token = credential.GetToken(tokenRequestContext, default);
                    Tokens[cacheKey] = token;
                    log.LogDebug("Successfully acquired Azure AD token for MySQL");
                    return token.Token;
                }
            }
            catch (Exception ex)
            {
                log.LogError(ex, "Failed to acquire Azure AD token for MySQL");
                throw;
            }
        }

        public DbCommand CreateCommand(string sql, DbConnection connection, DbTransaction transaction = null!)
        {
            var cmd = new MySqlCommand(sql, (MySqlConnection)connection);
            if (transaction != null)
            {
                cmd.Transaction = (MySqlTransaction)transaction;
            }
            return cmd;
        }

        public DbParameter CreateParameter(string name, object value)
        {
            return new MySqlParameter(name, value ?? DBNull.Value);
        }
    }
}
