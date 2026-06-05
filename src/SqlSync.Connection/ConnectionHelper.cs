using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Data.Common;
namespace SqlSync.Connection
{
    /// <summary>
    /// Summary description for ConnectionHelper.
    /// </summary>
    public class ConnectionHelper : IConnectionHelper
    {
        public static string ConnectCryptoKey
        {
            get
            {
                return String.Format("ewrwecwt9-3467u435bgQ{0}@#Q1569[';./?#%4witg9uv-$#!@&)(_(#!@$30r0fasdap;{0}aw56-049q3", System.Environment.UserName);
            }
        }

        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);
        public static string appName = "Sql Build Manager v{0} [{1}];";

        /// <summary>
        /// Process-wide opt-in for trusting (not validating) the SQL Server TLS certificate.
        /// Secure-by-default (false): certificates are validated. The operator opts in once at
        /// startup via the <c>--trustservercertificate</c> flag (or settings file), which is seeded
        /// here so that connection-string overloads that don't carry a <see cref="ConnectionData"/>
        /// still honor the operator's run-level choice. Connection-data based overloads OR this with
        /// the per-connection <see cref="ConnectionData.TrustServerCertificate"/> value.
        /// </summary>
        public static bool TrustServerCertificate { get; set; } = false;
        static ConnectionHelper()
        {
            string version;
            if (System.Reflection.Assembly.GetEntryAssembly() != null)
                version = System.Reflection.Assembly.GetEntryAssembly()!.GetName().Version!.ToString();
            else
                version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!.ToString();

            appName = string.Format($"Sql Build Manager v{version} [{System.Environment.UserName}];");
        }

        /// <summary>
        /// Returns the appropriate IDbConnectionFactory for the given platform.
        /// </summary>
        public static IDbConnectionFactory GetFactory(DatabasePlatform platform)
        {
            return platform switch
            {
                DatabasePlatform.PostgreSQL => new PostgresConnectionFactory(),
                _ => new SqlServerConnectionFactory(),
            };
        }

        /// <summary>
        /// Returns the appropriate IDbConnectionFactory for the given ConnectionData.
        /// </summary>
        public static IDbConnectionFactory GetFactory(ConnectionData connData)
        {
            return GetFactory(connData?.DatabasePlatform ?? DatabasePlatform.SqlServer);
        }

        /// <summary>
        /// Creates a platform-aware DbConnection from ConnectionData.
        /// </summary>
        public static DbConnection GetDbConnection(ConnectionData connData)
        {
            if (connData == null)
                return null!;
            return GetFactory(connData).CreateConnection(connData);
        }

        public static SqlConnection GetConnection(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {
            SqlServerAuthenticationProvider.Register();
            String conn = GetConnectionString(dbName, serverName, uid, pw, authType, scriptTimeOut, managedIdentityClientId);
            SqlConnection dbConn = new SqlConnection(conn);

            return dbConn;
        }
        public static SqlConnection GetConnection(ConnectionData connData)
        {
            if (connData == null)
                return null!;

            return new SqlConnection(GetConnectionString(connData));
        }
        public static string GetConnectionString(ConnectionData connData)
        {
            if (connData == null)
                return string.Empty;

            return GetConnectionString(connData.DatabaseName,
                connData.SQLServerName,
                connData.UserId,
                connData.Password,
                connData.AuthenticationType,
                connData.ScriptTimeout,
                connData.ManagedIdentityClientId,
                connData.TrustServerCertificate || TrustServerCertificate);
        }
        public static string GetConnectionString(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {
            return GetConnectionString(dbName, serverName, uid, pw, authType, scriptTimeOut, managedIdentityClientId, TrustServerCertificate);
        }
        public static string GetConnectionString(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId, bool trustServerCertificate)
        {
            SqlServerAuthenticationProvider.Register();

            var builder = new SqlConnectionStringBuilder();
            builder.DataSource = serverName;
            builder.InitialCatalog = dbName;
            builder.ConnectTimeout = scriptTimeOut;
            builder.Pooling = false;
            builder.ApplicationName = appName;
            //Set transient values
            builder.ConnectRetryCount = 3;
            builder.ConnectRetryInterval = 10;

            switch (authType)
            {
                case AuthenticationType.Windows:
                    builder.IntegratedSecurity = true;
                    builder.TrustServerCertificate = trustServerCertificate;
                    break;
                case AuthenticationType.AzureADIntegrated:
                    builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
                    builder.IntegratedSecurity = true;
                    builder.TrustServerCertificate = trustServerCertificate;
                    break;
                case AuthenticationType.AzureADDefault:
                    builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryDefault;
                    if (!string.IsNullOrWhiteSpace(managedIdentityClientId))
                        builder.UserID = managedIdentityClientId;
                    builder.TrustServerCertificate = trustServerCertificate;
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
                    builder.TrustServerCertificate = trustServerCertificate;
                    break;
            }
            log.LogDebug($"Database Connection string: {ConnectionStringRedactor.Redact(builder.ConnectionString)}");
            return builder.ConnectionString;
        }

        public static string GetTargetDatabase(string defaultDatabase, List<DatabaseOverride> overrides)
        {
            if (String.IsNullOrEmpty(defaultDatabase) && overrides != null)
                return overrides[0].OverrideDbTarget;


            if (overrides != null)
                for (int z = 0; z < overrides.Count; z++)
                    if (overrides[z].DefaultDbTarget.ToLower() == defaultDatabase.ToLower())
                        return overrides[z].OverrideDbTarget;


            return defaultDatabase;
        }

        public static bool ValidateDatabaseOverrides(List<DatabaseOverride> overrides)
        {
            if (overrides == null)
                return false;

            foreach (DatabaseOverride over in overrides)
            {
                if (over == null)
                    return false;

                if ((over.DefaultDbTarget == null || over.DefaultDbTarget.Length == 0) &&
                        (over.OverrideDbTarget == null || over.OverrideDbTarget.Length == 0))
                    return false;
            }
            return true;
        }

        public static bool TestDatabaseConnection(string dbName, string serverName, string username, string password, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {
            return TestDatabaseConnection(dbName, serverName, username, password, authType, scriptTimeOut, managedIdentityClientId, TrustServerCertificate);
        }
        public static bool TestDatabaseConnection(string dbName, string serverName, string username, string password, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId, bool trustServerCertificate)
        {
            return TestDatabaseConnection(new ConnectionData()
            {
                DatabaseName = dbName,
                ScriptTimeout = scriptTimeOut,
                SQLServerName = serverName,
                UserId = username,
                Password = password,
                AuthenticationType = authType,
                ManagedIdentityClientId = managedIdentityClientId,
                TrustServerCertificate = trustServerCertificate
            });
        }
        public static bool TestDatabaseConnection(ConnectionData connData)
        {
            DbConnection conn = null!;
            try
            {
                if (connData.ScriptTimeout <= 0)
                    connData.ScriptTimeout = 60;
                conn = GetDbConnection(connData);
                conn.Open();
                conn.Close();
                return true;
            }
            catch (Exception exe)
            {
                //if(exe.Message.ToUpper().Contains("ManagedIdentityCredential authentication unavailable".ToUpper()))
                //{
                //    connData.AuthenticationType = AuthenticationType.AzureADIntegrated;
                //    connData.ManagedIdentityClientId = "";
                //    return TestDatabaseConnection(connData);
                //}
                log.LogWarning(exe, "TestConnection failed");
                return false;
            }
            finally
            {
                if (conn != null)
                    conn.Dispose();
            }
        }

    }
}
