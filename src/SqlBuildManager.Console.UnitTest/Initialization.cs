using SqlBuildManager.Test.Common;
using System;
using System.Collections.Generic;
using System.IO;
namespace SqlBuildManager.Console.UnitTest
{
    class Initialization : IDisposable
    {
        public static string ConnectionString = TestEnvironment.GetSqlConnectionStringTemplate();

        private static List<string> tempFiles;
        public static string SqlBuildZipFileName;
        public static string MultiDbFileName;
        public static string DbConfigFileName;

        public Initialization()
        {
            tempFiles = new List<string>();
            Initialization.SqlBuildZipFileName = TestFileHelper.GetTrulyUniqueFile("sbm");
            tempFiles.Add(Initialization.SqlBuildZipFileName);
            Initialization.MultiDbFileName = TestFileHelper.GetTrulyUniqueFile("multidb");
            tempFiles.Add(Initialization.MultiDbFileName);
            Initialization.DbConfigFileName = TestFileHelper.GetTrulyUniqueFile("cfg");
            tempFiles.Add(Initialization.DbConfigFileName);
        }
        public static void CleanUp()
        {
            TestFileHelper.CleanupTempFiles(tempFiles);
        }

        public void CopySbmFileToTestPath()
        {
            File.WriteAllBytes(Initialization.SqlBuildZipFileName, Properties.Resources.NoTrans_MultiDb_sbm);
        }
        public void CopyMultiDbFileToTestPath()
        {
            File.WriteAllBytes(Initialization.MultiDbFileName, Properties.Resources.NoTrans_MultiDb_multidb);
        }
        public void CopyDbConfigFileeToTestPath()
        {
            File.WriteAllBytes(Initialization.DbConfigFileName, Properties.Resources.dbconfig);
        }

        #region IDisposable Members

        public void Dispose()
        {
            TestFileHelper.CleanupTempFiles(tempFiles);
        }

        #endregion
    }
}
