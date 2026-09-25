using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Npgsql;
using System;
using System.Collections.Concurrent;
using System.Data.Common;

namespace SqlBuildManager.Connection
{
    /// <summary>
    /// PostgreSQL implementation of IDbConnectionFactory using Npgsql.
    /// Supports Password, AzureADDefault, and ManagedIdentity authentication.
    /// </summary>
    public class PostgresConnectionFactory : IDbConnectionFactory
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        /// <summary>
        /// The Azure AD scope for Azure Database for PostgreSQL.
        /// </summary>
        private static readonly string[] PgAadScopes = new[] { "https://ossrdbms-aad.database.windows.net/.default" };
        private static readonly TimeSpan TokenRefreshBuffer = TimeSpan.FromMinutes(5);
        private static readonly ConcurrentDictionary<string, TokenCredential> Credentials = new();
        private static readonly ConcurrentDictionary<string, AccessToken> Tokens = new();
        private static readonly ConcurrentDictionary<string, object> TokenLocks = new();
        private static readonly string[] AzurePostgresSuffixes =
        {
            ".postgres.database.azure.com",
            ".postgres.database.usgovcloudapi.net",
            ".postgres.database.chinacloudapi.cn",
            ".postgres.database.microsoftazure.de"
        };
        private readonly Func<string, string> tokenProvider;

        public PostgresConnectionFactory() : this(GetAzureAdAccessToken)
        {
        }

        internal PostgresConnectionFactory(Func<string, string> tokenProvider)
        {
            this.tokenProvider = tokenProvider ?? throw new ArgumentNullException(nameof(tokenProvider));
        }

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

        // PostgreSQL uses SslMode (not TrustServerCertificate); the flag is accepted for interface parity and ignored.
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
            // Keep multi-host/per-host ports intact for Npgsql; retain single-host port parsing.
            string host = string.Join(",", serverName.Split(',', StringSplitOptions.TrimEntries));
            int port = 5432;
            if (!host.Contains(',') && host.IndexOf(':') > 0 && host.IndexOf(':') == host.LastIndexOf(':'))
            {
                var parts = host.Split(':');
                host = parts[0];
                if (parts.Length > 1 && int.TryParse(parts[1], out int parsedPort))
                    port = parsedPort;
            }

            var builder = new NpgsqlConnectionStringBuilder();
            builder.Host = host;
            builder.Port = port;
            builder.Database = dbName;
            builder.Timeout = scriptTimeOut;
            builder.Pooling = true;
            builder.MinPoolSize = ConnectionHelper.MinimumPoolSize;
            builder.MaxPoolSize = ConnectionHelper.MaximumPoolSize;
            builder.ApplicationName = ConnectionHelper.appName;
            // Npgsql VerifyFull validates both hostname and chain using OS certificate trust.
            builder.SslMode = ContainsAzurePostgresHost(serverName) ? SslMode.VerifyFull : SslMode.Prefer;

            switch (authType)
            {
                case AuthenticationType.Password:
                default:
                    builder.Username = uid;
                    builder.Password = pw;
                    break;
                case AuthenticationType.Windows:
                    // PostgreSQL GSSAPI/SSPI auth — no username/password needed
                    break;
                case AuthenticationType.AzureADDefault:
                case AuthenticationType.ManagedIdentity:
                    // Acquire an Azure AD token and use it as the password
                    builder.SslMode = SslMode.VerifyFull;
                    // PG Entra ID requires the identity name (role name) as the username.
                    // Use uid (identity name) if available; fall back to client ID only if no name provided.
                    builder.Username = !string.IsNullOrEmpty(uid) ? uid : managedIdentityClientId;
                    builder.Password = tokenProvider(managedIdentityClientId);
                    break;
                case AuthenticationType.AzureADIntegrated:
                case AuthenticationType.AzureADInteractive:
                    builder.Username = uid;
                    builder.Password = pw;
                    builder.SslMode = SslMode.VerifyFull;
                    break;
            }

            log.LogDebug($"PostgreSQL Connection string: {ConnectionStringRedactor.Redact(builder.ConnectionString)}");
            return builder.ConnectionString;
        }

        private static bool ContainsAzurePostgresHost(string serverName)
        {
            foreach (string endpoint in serverName.Split(',', StringSplitOptions.TrimEntries))
            {
                // Azure DNS names cannot contain colons; remove an optional port before matching.
                string hostname = endpoint.Split(':')[0].Trim().TrimEnd('.');
                foreach (string suffix in AzurePostgresSuffixes)
                {
                    if (hostname.EndsWith(suffix, StringComparison.OrdinalIgnoreCase))
                        return true;
                }
            }

            return false;
        }

        // PostgreSQL uses SslMode (not TrustServerCertificate); the flag is accepted for interface parity and ignored.
        public string BuildConnectionString(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId, bool trustServerCertificate)
        {
            return BuildConnectionString(dbName, serverName, uid, pw, authType, scriptTimeOut, managedIdentityClientId);
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

                    var tokenRequestContext = new TokenRequestContext(PgAadScopes);
                    AccessToken token = credential.GetToken(tokenRequestContext, default);
                    Tokens[cacheKey] = token;
                    log.LogDebug("Successfully acquired Azure AD token for PostgreSQL");
                    return token.Token;
                }
            }
            catch (System.Exception ex)
            {
                log.LogError(ex, "Failed to acquire Azure AD token for PostgreSQL");
                throw;
            }
        }

        public DbCommand CreateCommand(string sql, DbConnection connection, DbTransaction transaction = null!)
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
