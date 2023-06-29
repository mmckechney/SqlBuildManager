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
    public class ConnectionHelper
    {
        public static string ConnectCryptoKey
        {
            get
            {
                return String.Format("ewrwecwt9-3467u435bgQ{0}@#Q1569[';./?#%4witg9uv-$#!@&)(_(#!@$30r0fasdap;{0}aw56-049q3", System.Environment.UserName);
            }
        }

        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public static string appName = "Sql Build Manager v{0} [{1}];";
        static ConnectionHelper()
        {
            string version;
            if (System.Reflection.Assembly.GetEntryAssembly() != null)
                version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            else
                version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            appName = string.Format($"Sql Build Manager v{version} [{System.Environment.UserName}];");
        }
        public static SqlConnection GetConnection(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {
            String conn = GetConnectionString(dbName, serverName, uid, pw, authType, scriptTimeOut, managedIdentityClientId);
            SqlConnection dbConn = new SqlConnection(conn);

            return dbConn;
        }
        public static SqlConnection GetConnection(ConnectionData connData)
        {
            if (connData == null)
                return null;

            return GetConnection(connData.DatabaseName,
                connData.SQLServerName,
                connData.UserId,
                connData.Password,
                connData.AuthenticationType,
                connData.ScriptTimeout,
                connData.ManagedIdentityClientId);
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
                connData.ManagedIdentityClientId);
        }
        public static string GetConnectionString(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut, string managedIdentityClientId)
        {

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
                    builder.TrustServerCertificate = true;
                    break;
                case AuthenticationType.AzureADIntegrated:
                    builder.Authentication = SqlAuthenticationMethod.ActiveDirectoryIntegrated;
                    builder.IntegratedSecurity = true;
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
                    break;
            }
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
            return TestDatabaseConnection(new ConnectionData()
            {
                DatabaseName = dbName,
                ScriptTimeout = scriptTimeOut,
                SQLServerName = serverName,
                UserId = username,
                Password = password,
                AuthenticationType = authType,
                ManagedIdentityClientId = managedIdentityClientId
            });
        }
        public static bool TestDatabaseConnection(ConnectionData connData)
        {
            DbConnection conn = null;
            try
            {
                conn = GetConnection(connData);
                conn.Open();
                conn.Close();
                return true;
            }
            catch (Exception exe)
            {
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
