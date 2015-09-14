using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Collections.Specialized;
using System.Text.RegularExpressions;
using System.Diagnostics;
using System.IO;
namespace SqlSync
{
    class Utility
    {
        public const string ConfigFileName = "SqlSync.cfg";
        internal static List<string> GetRecentServers()
        {
            string homePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\";
            List<string> recentDbs = new List<string>();

            if (System.IO.File.Exists(homePath + ConfigFileName))
            {
                try
                {
                    SqlSyncConfig config = new SqlSyncConfig();
                    config.ReadXml(homePath + ConfigFileName);
                    DataView view = config.RecentDatabase.DefaultView;
                    view.Sort = config.RecentDatabase.LastAccessedColumn.ColumnName + " DESC";
                    for (int i = 0; i < view.Count; i++)
                        recentDbs.Add(((SqlSyncConfig.RecentDatabaseRow)view[i].Row).Name);
                }
                catch
                {

                }
            }
            return recentDbs;
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
