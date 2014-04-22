using System;
using System.Collections.Generic;
using System.Text;
using SqlBuildManager.Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using System.IO;

namespace SqlBuildManager.Console.UnitTest
{
    class Initialization : IDisposable
    {
        public static string ConnectionString = @"Data Source=(local)\SQLEXPRESS;Initial Catalog={0}; Trusted_Connection=Yes;CONNECTION TIMEOUT=20;";

        private List<string> tempFiles;
        public string SqlBuildZipFileName;
        public string MultiDbFileName;

        public Initialization()
        {
            tempFiles = new List<string>();
            this.SqlBuildZipFileName = this.GetTrulyUniqueFile("sbm");
            this.MultiDbFileName = this.GetTrulyUniqueFile("multidb");
        }

        public ThreadedExecution_Accessor GetThreadedExecutionAccessor(string[] args)
        {
            return new ThreadedExecution_Accessor(args);
        }

        public void CopySbmFileToTestPath()
        {
            File.WriteAllBytes(this.SqlBuildZipFileName, Properties.Resources.NoTrans_MultiDb_sbm);
        }
        public void CopyMultiDbFileToTestPath()
        {
            File.WriteAllBytes(this.MultiDbFileName, Properties.Resources.NoTrans_MultiDb_multidb);
        }
        public string GetTrulyUniqueFile(string extension)
        {
            string tmpName = Path.GetTempFileName();
            string newName = Path.GetDirectoryName(tmpName) + @"\SqlBuildManager-Console-" + Guid.NewGuid().ToString() + "." + extension;
            File.Move(tmpName, newName);


            this.tempFiles.Add(newName);
            return newName;

        }

        #region IDisposable Members

        public void Dispose()
        {
            foreach (string file in this.tempFiles)
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
