using System;
using System.Data.SqlClient;
using System.Data.Common;
using System.Collections.Generic;
using log4net;
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
      
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string appName = "Application Name=Sql Build Manager v{0} [{1}];";
        static ConnectionHelper()
        {
            string version;
            if (System.Reflection.Assembly.GetEntryAssembly() != null)
                version = System.Reflection.Assembly.GetEntryAssembly().GetName().Version.ToString();
            else
                version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            appName = string.Format("Application Name=Sql Build Manager v{0} [{1}];", version, System.Environment.UserName);
        }
        public static SqlConnection GetConnection(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut)
        {
            String conn = GetConnectionString(dbName, serverName, uid, pw, authType, scriptTimeOut);
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
                connData.ScriptTimeout);
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
                connData.ScriptTimeout);
        }
        public static string GetConnectionString(string dbName, string serverName, string uid, string pw, AuthenticationType authType, int scriptTimeOut)
        {
            string conn;
            switch(authType)
            {
                case AuthenticationType.WindowsAuthentication:
                    conn = "Data Source=" + serverName + ";Initial Catalog=" + dbName + ";Trusted_Connection=Yes;CONNECTION TIMEOUT=" + scriptTimeOut.ToString() + ";Pooling=false;" + appName;
                    break;
                case AuthenticationType.AzureActiveDirectory:
                    conn = "Data Source=" + serverName + ";Initial Catalog=" + dbName + ";Authentication=Active Directory Integrated;CONNECTION TIMEOUT=" + scriptTimeOut.ToString() + ";Pooling=false;" + appName;
                    break;
                case AuthenticationType.AzureUserNamePassword:
                    conn = "Data Source=" + serverName + ";Initial Catalog=" + dbName + ";Authentication=Active Directory Password;UID=" + uid + ";PWD=" + pw + ";CONNECTION TIMEOUT=" + scriptTimeOut.ToString() + ";Pooling=false;" + appName;
                    break;
                case AuthenticationType.UserNamePassword:
                default:
                    conn = "Data Source=" + serverName + ";Initial Catalog=" + dbName + ";User ID=" + uid + ";Password=" + pw + ";CONNECTION TIMEOUT=" + scriptTimeOut.ToString() + ";Pooling=false;" + appName;
                    break;
            }
            return conn;
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

        public static bool TestDatabaseConnection(string dbName, string serverName, int scriptTimeOut)
        {
            return TestDatabaseConnection(new ConnectionData() { DatabaseName = dbName, ScriptTimeout = scriptTimeOut, SQLServerName = serverName , AuthenticationType = AuthenticationType.UserNamePassword});
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
                log.Warn("TestConnection failed", exe);
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
