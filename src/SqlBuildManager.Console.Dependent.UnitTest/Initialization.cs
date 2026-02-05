using System;
using System.Collections.Generic;
using System.IO;
namespace SqlBuildManager.Console.Dependent.UnitTest
{
    class Initialization : IDisposable
    {
        public static string ConnectionString = @"Server=(local)\SQLEXPRESS;Database={0};Trusted_Connection=True;TrustServerCertificate=True;Encrypt=False;";

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
            File.WriteAllBytes(Initialization.DbConfigFileName, Properties.Resources.dbconfig);
        }
        public void CopySqlScriptOverrideFiletoTestPath()
        {
            File.WriteAllText(Initialization.SqlScriptOverrideFileName, Properties.Resources.override_sql);
        }
        public void CopyDbConfigFile100ToTestPath()
        {
            File.WriteAllBytes(Initialization.DbConfigFileName, Properties.Resources.dbconfig_100);
        }
        public void CopyDbConfigFile50ToTestPath()
        {
            File.WriteAllBytes(Initialization.DbConfigFileName, Properties.Resources.dbconfig_50);
        }
        public void CopyDbConfigFile20ToTestPath()
        {
            File.WriteAllBytes(Initialization.DbConfigFileName, Properties.Resources.dbconfig_20);
        }
        public void CopyDbConfigFile10ToTestPath()
        {
            File.WriteAllBytes(Initialization.DbConfigFileName, Properties.Resources.dbconfig_10);
        }
        public void CopyDoubleDbConfigFileToTestPath()
        {
            File.WriteAllBytes(Initialization.DbConfigFileName, Properties.Resources.dbconfig_doubledb);
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
