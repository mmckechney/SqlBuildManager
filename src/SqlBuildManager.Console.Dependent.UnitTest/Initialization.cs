using SqlBuildManager.Test.Common;
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
        public static string TestServer => TestEnvironment.SqlServer;

        public static string TestAuthType => TestEnvironment.UseSqlAuth
            ? AuthenticationType.Password.ToString()
            : AuthenticationType.Windows.ToString();

        public static string[] GetAuthArgs() => TestEnvironment.GetSqlAuthArgs();

        static Initialization()
        {
            ConnectionString = TestEnvironment.GetSqlConnectionStringTemplate();
        }

        private static List<string> tempFiles = null!;
        public static string SqlBuildZipFileName = null!;
        public static string MultiDbFileName = null!;
        public static string DbConfigFileName = null!;
        public static string SqlScriptOverrideFileName = null!;

        public Initialization()
        {
            tempFiles = new List<string>();
            Initialization.SqlBuildZipFileName = TestFileHelper.GetTrulyUniqueFile("sbm");
            tempFiles.Add(Initialization.SqlBuildZipFileName);
            Initialization.MultiDbFileName = TestFileHelper.GetTrulyUniqueFile("multidb");
            tempFiles.Add(Initialization.MultiDbFileName);
            Initialization.DbConfigFileName = TestFileHelper.GetTrulyUniqueFile("cfg");
            tempFiles.Add(Initialization.DbConfigFileName);
            Initialization.SqlScriptOverrideFileName = TestFileHelper.GetTrulyUniqueFile("sql");
            tempFiles.Add(Initialization.SqlScriptOverrideFileName);
        }
        public static void CleanUp()
        {
            TestFileHelper.CleanupTempFiles(tempFiles);
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

        #region IDisposable Members

        public void Dispose()
        {
            TestFileHelper.CleanupTempFiles(tempFiles);
        }

        #endregion
    }
}
