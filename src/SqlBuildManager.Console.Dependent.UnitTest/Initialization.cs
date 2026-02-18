using System;
using System.Collections.Generic;
using System.IO;
using SqlSync.Connection;
namespace SqlBuildManager.Console.Dependent.UnitTest
{
    class Initialization : IDisposable
    {
        public static string ConnectionString;

        /// <summary>
        /// The SQL Server name used for tests, from SBM_TEST_SQL_SERVER env var or defaulting to (local)\SQLEXPRESS.
        /// </summary>
        public static string TestServer { get; }

        public static string TestAuthType => string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SBM_TEST_SQL_USER"))
            ? AuthenticationType.Windows.ToString()
            : AuthenticationType.Password.ToString();

        public static string[] GetAuthArgs()
        {
            var user = Environment.GetEnvironmentVariable("SBM_TEST_SQL_USER");
            var password = Environment.GetEnvironmentVariable("SBM_TEST_SQL_PASSWORD");
            if (!string.IsNullOrWhiteSpace(user))
                return new[] { "--authtype", AuthenticationType.Password.ToString(), "--username", user, "--password", password ?? "" };
            else
                return new[] { "--authtype", AuthenticationType.Windows.ToString() };
        }

        static Initialization()
        {
            TestServer = Environment.GetEnvironmentVariable("SBM_TEST_SQL_SERVER") ?? @"(local)\SQLEXPRESS";
            var user = Environment.GetEnvironmentVariable("SBM_TEST_SQL_USER");
            var password = Environment.GetEnvironmentVariable("SBM_TEST_SQL_PASSWORD");
            if (!string.IsNullOrWhiteSpace(user))
                ConnectionString = $"Server={TestServer};Database={{0}};User ID={user};Password={password};TrustServerCertificate=True;Encrypt=False;";
            else
                ConnectionString = $@"Server={TestServer};Database={{0}};Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";
        }

        private static List<string> tempFiles;
        public static string SqlBuildZipFileName;
        public static string MultiDbFileName;
        public static string DbConfigFileName;
        public static string SqlScriptOverrideFileName;

        public Initialization()
        {
            tempFiles = new List<string>();
            Initialization.SqlBuildZipFileName = GetTrulyUniqueFile("sbm");
            Initialization.MultiDbFileName = GetTrulyUniqueFile("multidb");
            Initialization.DbConfigFileName = GetTrulyUniqueFile("cfg");
            Initialization.SqlScriptOverrideFileName = GetTrulyUniqueFile("sql");
        }
        public static void CleanUp()
        {
            foreach (string f in tempFiles)
            {
                try
                {
                    File.Delete(f);
                }
                catch { }

            }
        }

        public void CopySbmFileToTestPath()
        {
            File.WriteAllBytes(Initialization.SqlBuildZipFileName, Properties.Resources.NoTrans_MultiDb1);
        }
        public void CopyMultiDbFileToTestPath()
        {
            File.WriteAllBytes(Initialization.MultiDbFileName, Properties.Resources.NoTrans_MultiDb);
        }
        public void CopyDbConfigFileToTestPath()
        {
            WriteDbConfigWithServer(Initialization.DbConfigFileName, Properties.Resources.dbconfig);
        }
        public void CopySqlScriptOverrideFiletoTestPath()
        {
            File.WriteAllText(Initialization.SqlScriptOverrideFileName, Properties.Resources.override_sql);
        }
        public void CopyDbConfigFile100ToTestPath()
        {
            WriteDbConfigWithServer(Initialization.DbConfigFileName, Properties.Resources.dbconfig_100);
        }
        public void CopyDbConfigFile50ToTestPath()
        {
            WriteDbConfigWithServer(Initialization.DbConfigFileName, Properties.Resources.dbconfig_50);
        }
        public void CopyDbConfigFile20ToTestPath()
        {
            WriteDbConfigWithServer(Initialization.DbConfigFileName, Properties.Resources.dbconfig_20);
        }
        public void CopyDbConfigFile10ToTestPath()
        {
            WriteDbConfigWithServer(Initialization.DbConfigFileName, Properties.Resources.dbconfig_10);
        }
        public void CopyDoubleDbConfigFileToTestPath()
        {
            WriteDbConfigWithServer(Initialization.DbConfigFileName, Properties.Resources.dbconfig_doubledb);
        }

        /// <summary>
        /// Writes a cfg resource file to disk, replacing the embedded server name with TestServer.
        /// </summary>
        private static void WriteDbConfigWithServer(string targetPath, byte[] resourceBytes)
        {
            string content = System.Text.Encoding.UTF8.GetString(resourceBytes);
            content = content.Replace(@"(local)\SQLEXPRESS", TestServer);
            File.WriteAllText(targetPath, content);
        }
        public string GetTrulyUniqueFile(string extension)
        {
            if (extension.StartsWith(".")) extension = extension.Replace(".", "");
            string tmpName = Path.GetTempFileName();
            string newName = Path.GetDirectoryName(tmpName) + @"\SqlBuildManager-Console-" + Guid.NewGuid().ToString() + "." + extension;
            File.Move(tmpName, newName);


            Initialization.tempFiles.Add(newName);
            return newName;

        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (string file in Initialization.tempFiles)
            {
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch
                {
                }
            }
        }

        #endregion
    }
}
