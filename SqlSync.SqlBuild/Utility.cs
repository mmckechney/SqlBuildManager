using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using System.Linq;
namespace SqlSync.SqlBuild
{
    public class UtilityHelper
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        public const string ConfigFileName = "SqlSync.cfg";
        public static List<string> GetRecentServers(out ServerConnectConfig.ServerConfigurationDataTable serverConfigTbl)
        {
            serverConfigTbl = null;
            string homePath = SqlBuildManager.Logging.Configure.AppDataPath + @"\";
            List<string> recentDbs = new List<string>();

            if (System.IO.File.Exists(homePath + ConfigFileName))
            {
                try
                {
                    ServerConnectConfig config = new ServerConnectConfig();
                    config.ReadXml(homePath + ConfigFileName);
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
                string homePath = SqlBuildManager.Logging.Configure.AppDataPath + @"\";
                ServerConnectConfig config = new ServerConnectConfig();
                if (File.Exists(homePath + ConfigFileName))
                    config.ReadXml(homePath + ConfigFileName);

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
                config.WriteXml(homePath + ConfigFileName);
            }
            catch (Exception exe)
            {
                log.Error("Error updating Recent Server List", exe);
            }


        }
        public static AuthenticationType GetServerCredentials(ServerConnectConfig.ServerConfigurationDataTable serverConfigTbl, string serverName, out string username, out string password)
        {
            if (serverConfigTbl != null)
            {
                var row = serverConfigTbl.Where(r => r.Name.Trim().ToLower() == serverName.Trim().ToLower());
                if (row.Any())
                {
                    var r = row.First();
                    if (!string.IsNullOrWhiteSpace(r.UserName))
                    {

                        username = Cryptography.DecryptText(r.UserName, ConnectionHelper.ConnectCryptoKey);
                    }
                    else
                    {
                        username = string.Empty;
                    }

                    if (!string.IsNullOrWhiteSpace(r.Password))
                    {
                        password = Cryptography.DecryptText(r.Password, ConnectionHelper.ConnectCryptoKey);
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
                        return AuthenticationType.UserNamePassword;
                    }

                }
            }

            password = string.Empty;
            username = string.Empty;
            return AuthenticationType.UserNamePassword;
        }

        private static string defaultBrowser = string.Empty;
        public static string DefaultBrowser
        {
            get
            {
                if (defaultBrowser.Length == 0)
                {
                    Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.ClassesRoot.OpenSubKey(@"HTTP\shell\open\command", false);

                    //trim off quotes
                    string browser = key.GetValue(null).ToString().ToLower().Replace("\"", "");
                    if (!browser.EndsWith("exe"))
                    {
                        //get rid of everything after the ".exe"
                        browser = browser.Substring(0, browser.LastIndexOf(".exe") + 4);
                    }
                    if (browser.Length > 0)
                        defaultBrowser = browser;
                    else
                        defaultBrowser = @"C:\Program Files\Internet Explorer\IEXPLORE.EXE";
                }
                return defaultBrowser;
            }
        }
        public static void OpenManual(string anchor)
        {
            if (anchor == null)
                anchor = string.Empty;

            if (anchor.Length > 0)
            {
                if (!anchor.StartsWith("#")) anchor = "#" + anchor;
            }
            string path = Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string htmlFile = path + @"\Manual\SqlBuildManagerManual.htm" + anchor;
            System.Diagnostics.Process prc = new Process();
            prc.StartInfo.FileName = @"C:\Program Files\Internet Explorer\IEXPLORE.EXE"; // Utility.DefaultBrowser;
            prc.StartInfo.Arguments = htmlFile.Replace('\\','/'); 
            prc.Start();
        }
    }
}
