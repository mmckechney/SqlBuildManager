using Microsoft.Extensions.Logging;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
namespace SqlSync.SqlBuild
{
    public class UtilityHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string ConfigFileName = "SqlSync.cfg";
        public static List<string> GetRecentServers(out ServerConnectConfig.ServerConfigurationDataTable serverConfigTbl)
        {
            serverConfigTbl = null;
            string homePath = SqlBuildManager.Logging.Configure.AppDataPath;
            List<string> recentDbs = new List<string>();

            if (System.IO.File.Exists(Path.Combine(homePath, ConfigFileName)))
            {
                try
                {
                    ServerConnectConfig config = new ServerConnectConfig();
                    config.ReadXml(Path.Combine(homePath, ConfigFileName));
                    serverConfigTbl = config.ServerConfiguration;
                    DataView view = config.ServerConfiguration.DefaultView;
                    view.Sort = config.ServerConfiguration.LastAccessedColumn.ColumnName + " DESC";
                    for (int i = 0; i < view.Count; i++)
                        recentDbs.Add(((ServerConnectConfig.ServerConfigurationRow)view[i].Row).Name);
                }
                catch
                {

                }
            }
            return recentDbs;
        }
        public static void UpdateRecentServerList(string databaseName, string userName, string password, AuthenticationType authType)
        {
            try
            {
                userName = Cryptography.EncryptText(userName, ConnectionHelper.ConnectCryptoKey);
                password = Cryptography.EncryptText(password, ConnectionHelper.ConnectCryptoKey);
                string configFileFullPath = Path.Combine(SqlBuildManager.Logging.Configure.AppDataPath, ConfigFileName);
                ServerConnectConfig config = new ServerConnectConfig();
                if (File.Exists(configFileFullPath))
                    config.ReadXml(configFileFullPath);

                DataRow[] row = config.ServerConfiguration.Select(config.ServerConfiguration.NameColumn.ColumnName + " ='" + databaseName + "'");
                if (row.Length == 0)
                {
                    config.ServerConfiguration.AddServerConfigurationRow(databaseName, DateTime.Now, userName, password, authType.ToString());
                }
                else
                {
                    var r = (ServerConnectConfig.ServerConfigurationRow)row[0];
                    r.LastAccessed = DateTime.UtcNow;
                    r.UserName = userName;
                    r.Password = password;
                    r.AuthenticationType = authType.ToString();
                    r.AcceptChanges();
                }
                config.WriteXml(configFileFullPath);
            }
            catch (Exception exe)
            {
                log.LogError(exe, "Error updating Recent Server List");
            }


        }
        public static AuthenticationType GetServerCredentials(ServerConnectConfig.ServerConfigurationDataTable serverConfigTbl, string serverName, out string username, out string password)
        {
            bool s;
            if (serverConfigTbl != null)
            {
                var row = serverConfigTbl.Where(r => r.Name.Trim().ToLower() == serverName.Trim().ToLower());
                if (row.Any())
                {
                    var r = row.First();
                    if (!string.IsNullOrWhiteSpace(r.UserName))
                    {

                        (s, username) = Cryptography.DecryptText(r.UserName, ConnectionHelper.ConnectCryptoKey, "UserName");
                    }
                    else
                    {
                        username = string.Empty;
                    }

                    if (!string.IsNullOrWhiteSpace(r.Password))
                    {
                        (s, password) = Cryptography.DecryptText(r.Password, ConnectionHelper.ConnectCryptoKey, "Password");
                    }
                    else
                    {
                        password = string.Empty;
                    }


                    AuthenticationType authType;
                    if (Enum.TryParse<AuthenticationType>(r.AuthenticationType, out authType))
                    {
                        return authType;
                    }
                    else
                    {
                        return AuthenticationType.Password;
                    }

                }
            }

            password = string.Empty;
            username = string.Empty;
            return AuthenticationType.Password;
        }

        public static void OpenManual(string anchor)
        {
            if (anchor == null)
                anchor = string.Empty;

            if (anchor.Length > 0)
            {
                if (!anchor.StartsWith("#")) anchor = "#" + anchor;
            }
            System.Diagnostics.Process prc = new Process();
            prc.StartInfo.UseShellExecute = true;
            prc.StartInfo.FileName = $"https://github.com/mmckechney/SqlBuildManager/blob/master/docs/SqlBuildManagerManual.md{anchor}"; // Utility.DefaultBrowser;

            prc.Start();
        }


    }
}
