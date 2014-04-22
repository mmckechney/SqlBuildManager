using SqlBuildManager.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting.Web;
using System.Collections.Generic;
using System.IO;
using SqlBuildManager.Services.History;

namespace SqlBuildManager.Services.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for BuildServiceTest and is intended
    ///to contain all BuildServiceTest Unit Tests
    ///</summary>
    [TestClass()]
    public class BuildServiceTest
    {



        #region ParseErrorsLogLine
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void ParseErrorsLogLineTest_ParseFinalLine()
        {
            BuildService_Accessor target = new BuildService_Accessor();
            string line = "[12/15/2010 16:02:39.480]		Finishing with Errors";
            string serverName = string.Empty;
            string serverNameExpected = string.Empty;
            string instanceName = string.Empty;
            string instanceNameExpected = string.Empty;
            string databaseName = string.Empty;
            string databaseNameExpected = string.Empty;
            bool expected = false;
            bool actual;
            actual = target.ParseErrorsLogLine(line, out serverName, out instanceName, out databaseName);
            Assert.AreEqual(serverNameExpected, serverName);
            Assert.AreEqual(instanceNameExpected, instanceName);
            Assert.AreEqual(databaseNameExpected, databaseName);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void ParseErrorsLogLineTest_GoodParsingWithInstanceName()
        {
            BuildService_Accessor target = new BuildService_Accessor();
            string line = @"[11/30/2010 14:35:17.674]		localhost\SQLEXPRESS.SqlBuildTest1 : Changes Rolled back. Return code: -400";
            string serverName = string.Empty;
            string serverNameExpected = "localhost";
            string instanceName = string.Empty;
            string instanceNameExpected = "SQLEXPRESS";
            string databaseName = string.Empty;
            string databaseNameExpected = "SqlBuildTest1";
            bool expected = true;
            bool actual;
            actual = target.ParseErrorsLogLine(line, out serverName, out instanceName, out databaseName);
            Assert.AreEqual(serverNameExpected, serverName);
            Assert.AreEqual(instanceNameExpected, instanceName);
            Assert.AreEqual(databaseNameExpected, databaseName);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void ParseErrorsLogLineTest_GoodParsingNoInstanceName()
        {
            BuildService_Accessor target = new BuildService_Accessor();
            string line = @"[11/30/2010 14:35:17.674]		localhost\SqlBuildTest1 : Changes Rolled back. Return code: -400";
            string serverName = string.Empty;
            string serverNameExpected = "localhost";
            string instanceName = string.Empty;
            string instanceNameExpected = "";
            string databaseName = string.Empty;
            string databaseNameExpected = "SqlBuildTest1";
            bool expected = true;
            bool actual;
            actual = target.ParseErrorsLogLine(line, out serverName, out instanceName, out databaseName);
            Assert.AreEqual(serverNameExpected, serverName);
            Assert.AreEqual(instanceNameExpected, instanceName);
            Assert.AreEqual(databaseNameExpected, databaseName);
            Assert.AreEqual(expected, actual);
        }

        [TestMethod()]
        public void ParseErrorsLogLineTest_ParseInitializeLine()
        {
            BuildService_Accessor target = new BuildService_Accessor();
            string line = "11/30/2010 12:51:30.347]		***Log for Run ID:dc7c5725b42642cdbed17014621d6a32";
            string serverName = string.Empty;
            string serverNameExpected = string.Empty;
            string instanceName = string.Empty;
            string instanceNameExpected = string.Empty;
            string databaseName = string.Empty;
            string databaseNameExpected = string.Empty;
            bool expected = false;
            bool actual;
            actual = target.ParseErrorsLogLine(line, out serverName, out instanceName, out databaseName);
            Assert.AreEqual(serverNameExpected, serverName);
            Assert.AreEqual(instanceNameExpected, instanceName);
            Assert.AreEqual(databaseNameExpected, databaseName);
            Assert.AreEqual(expected, actual);
        }
        #endregion

        [TestMethod()]
        public void ParseErrorLogForDirectoriesTest()
        {
            BuildService_Accessor target = new BuildService_Accessor();
            string mainErrorLogContents = @"[11/30/2010 12:58:27.645]		***Log for Run ID:dc7c5725b42642cdbed17014621d6a32
[11/30/2010 12:58:27.645]		localhost\SQLEXPRESS.SqlBuildTest : Changes Rolled back. Return code: -400
[11/30/2010 12:58:27.676]		localhost\SQLEXPRESS.SqlBuildTest1 : Changes Rolled back. Return code: -400
[11/30/2010 12:58:27.708]		localhost\SQLEXPRESS.SqlBuildTest2 : Changes Rolled back. Return code: -400
[11/30/2010 12:58:46.348]		Server1\Database_001 : Changes Rolled back. Return code: -400
[11/30/2010 12:58:46.380]		Finishing with Errors";

            string logPath = @"C:\temp";
            List<string> actual;
            actual = target.ParseErrorLogForDirectories(mainErrorLogContents, logPath);
            Assert.AreEqual(4, actual.Count);
            Assert.AreEqual(@"C:\temp\Server1\Database_001", actual[0]);
            Assert.AreEqual(@"C:\temp\localhost\SQLEXPRESS\SqlBuildTest", actual[3]);

        }
        //[TestMethod()]
        //[DeploymentItem("SqlBuildManager.Services.dll")]
        //public void GetAllLogFileNamesTest()
        //{
        //    BuildService_Accessor target = new BuildService_Accessor(); 
        //    List<string> parsedDirectories = null; 
        //    List<string> expected = null; 
        //    List<string> actual;
        //    actual = target.GetAllLogFileNames(parsedDirectories);
        //    Assert.AreEqual(expected, actual);
        //    Assert.Inconclusive("Verify the correctness of this test method.");
        //}



        #region GetCorrespondingLogFile
        /// <summary>
        ///A test for GetCorrespondingLogFileTest
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetCorrespondingLogFileTest_GetOldestViaMaxDateTime()
        {
            BuildService_Accessor target = new BuildService_Accessor();

            string databasePathLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(databasePathLogPath);

            File.WriteAllText(databasePathLogPath + @"\LogFile1.log", "File1");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile1.log", new DateTime(2011,1,1,11,30,0,0));

            File.WriteAllText(databasePathLogPath + @"\LogFile2.log", "File2");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile2.log", new DateTime(2011, 1, 1, 11, 30, 30, 500));

            File.WriteAllText(databasePathLogPath + @"\LogFile3.log", "File3");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile3.log", new DateTime(2011, 1, 1, 11, 32, 12, 500));

            DateTime submittedDate = DateTime.MaxValue;
            FileInfo actual;
            actual = target.GetCorrespondingLogFile(databasePathLogPath, submittedDate);

            Directory.Delete(databasePathLogPath, true);

            Assert.AreEqual("LogFile3.log", actual.Name);
            
        }

        /// <summary>
        ///A test for GetCorrespondingLogFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetCorrespondingLogFileTest_GetFirstFileWithin10SecondDelta()
        {
            BuildService_Accessor target = new BuildService_Accessor();

            string databasePathLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(databasePathLogPath);

            File.WriteAllText(databasePathLogPath + @"\LogFile1.log", "File1");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile1.log", new DateTime(2011, 1, 1, 11, 30, 0, 0));

            File.WriteAllText(databasePathLogPath + @"\LogFile2.log", "File2");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile2.log", new DateTime(2011, 1, 1, 11, 30, 30, 500));

            File.WriteAllText(databasePathLogPath + @"\LogFile3.log", "File3");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile3.log", new DateTime(2011, 1, 1, 11, 32, 12, 500));

            DateTime submittedDate = new DateTime(2011, 1, 1, 11, 30, 8, 500);
            FileInfo actual;
            actual = target.GetCorrespondingLogFile(databasePathLogPath, submittedDate);

            Directory.Delete(databasePathLogPath, true);

            Assert.AreEqual("LogFile1.log", actual.Name);

        }

        /// <summary>
        ///A test for GetCorrespondingLogFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetCorrespondingLogFileTest_GetSecondFile()
        {
            BuildService_Accessor target = new BuildService_Accessor();

            string databasePathLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(databasePathLogPath);

            File.WriteAllText(databasePathLogPath + @"\LogFile1.log", "File1");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile1.log", new DateTime(2011, 1, 1, 11, 30, 0, 0));

            File.WriteAllText(databasePathLogPath + @"\LogFile2.log", "File2");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile2.log", new DateTime(2011, 1, 1, 11, 30, 30, 500));

            File.WriteAllText(databasePathLogPath + @"\LogFile3.log", "File3");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile3.log", new DateTime(2011, 1, 1, 11, 32, 12, 500));

            DateTime submittedDate = new DateTime(2011, 1, 1, 11, 30, 32, 500);
            FileInfo actual;
            actual = target.GetCorrespondingLogFile(databasePathLogPath, submittedDate);

            Directory.Delete(databasePathLogPath, true);

            Assert.AreEqual("LogFile2.log", actual.Name);

        }

        /// <summary>
        ///A test for GetCorrespondingLogFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetCorrespondingLogFileTest_GetWhichFile()
        {
            BuildService_Accessor target = new BuildService_Accessor();

            string databasePathLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(databasePathLogPath);

            File.WriteAllText(databasePathLogPath + @"\LogFile1.log", "File1");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile1.log", new DateTime(2011, 1, 1, 11, 30, 0, 0));

            File.WriteAllText(databasePathLogPath + @"\LogFile2.log", "File2");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile2.log", new DateTime(2011, 1, 1, 11, 30, 30, 500));

            File.WriteAllText(databasePathLogPath + @"\LogFile3.log", "File3");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile3.log", new DateTime(2011, 1, 1, 11, 32, 12, 500));

            DateTime submittedDate = new DateTime(2011, 1, 1, 11, 31, 30, 500);
            FileInfo actual;
            actual = target.GetCorrespondingLogFile(databasePathLogPath, submittedDate);

            Directory.Delete(databasePathLogPath, true);

            Assert.AreEqual("LogFile2.log", actual.Name);

        }

        /// <summary>
        ///A test for GetCorrespondingLogFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetCorrespondingLogFileTest_GetFirstFile()
        {
            BuildService_Accessor target = new BuildService_Accessor();

            string databasePathLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(databasePathLogPath);

            File.WriteAllText(databasePathLogPath + @"\LogFile1.log", "File1");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile1.log", new DateTime(2011, 1, 1, 11, 30, 0, 0));

            File.WriteAllText(databasePathLogPath + @"\LogFile2.log", "File2");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile2.log", new DateTime(2011, 1, 1, 11, 30, 30, 500));

            File.WriteAllText(databasePathLogPath + @"\LogFile3.log", "File3");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile3.log", new DateTime(2011, 1, 1, 11, 32, 12, 500));

            DateTime submittedDate = new DateTime(2011, 1, 1, 11, 29, 0, 500);
            FileInfo actual;
            actual = target.GetCorrespondingLogFile(databasePathLogPath, submittedDate);

            Directory.Delete(databasePathLogPath, true);

            Assert.IsNull(actual);

        }

        /// <summary>
        ///A test for GetCorrespondingLogFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetCorrespondingLogFileTest_GetLastFileByDefault()
        {
            BuildService_Accessor target = new BuildService_Accessor();

            string databasePathLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(databasePathLogPath);

            File.WriteAllText(databasePathLogPath + @"\LogFile1.log", "File1");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile1.log", new DateTime(2011, 1, 1, 11, 30, 0, 0));

            File.WriteAllText(databasePathLogPath + @"\LogFile2.log", "File2");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile2.log", new DateTime(2011, 1, 1, 11, 30, 30, 500));

            File.WriteAllText(databasePathLogPath + @"\LogFile3.log", "File3");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile3.log", new DateTime(2011, 1, 1, 11, 32, 12, 500));

            DateTime submittedDate = new DateTime(2011, 1, 1, 11, 34, 0, 0);
            FileInfo actual;
            actual = target.GetCorrespondingLogFile(databasePathLogPath, submittedDate);

            Directory.Delete(databasePathLogPath, true);

            Assert.AreEqual("LogFile3.log", actual.Name);

        }

        /// <summary>
        ///A test for GetCorrespondingLogFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetCorrespondingLogFileTest_ReturnNull_NoDirectory()
        {
            BuildService_Accessor target = new BuildService_Accessor();

            string databasePathLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
      
            DateTime submittedDate = new DateTime(2011, 1, 1, 11, 34, 0, 0);
            FileInfo actual;
            actual = target.GetCorrespondingLogFile(databasePathLogPath, submittedDate);

            Assert.IsNull(actual);

        }
        /// <summary>
        ///A test for GetCorrespondingLogFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetCorrespondingLogFileTest_ReturnNull_NoFiles()
        {
            BuildService_Accessor target = new BuildService_Accessor();

            string databasePathLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(databasePathLogPath);

            DateTime submittedDate = new DateTime(2011, 1, 1, 11, 34, 0, 0);
            FileInfo actual;
            actual = target.GetCorrespondingLogFile(databasePathLogPath, submittedDate);

            Assert.IsNull(actual);

        }
        #endregion

        #region GetDetailedDatabaseLogFileContentsTest
        ///A test for GetDetailedDatabaseLogFileContents
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetDetailedDatabaseLogFileContentsTest_GetLatestFile()
        {
            string serverAndDatabase = @"Server1\Instance1.Database1";
   
            BuildService_Accessor target = new BuildService_Accessor();

            string rootLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(rootLogPath);

            string databasePathLogPath = rootLogPath + @"\Server1\Instance1\Database1";
            Directory.CreateDirectory(databasePathLogPath);
            

            File.WriteAllText(databasePathLogPath + @"\LogFile1.log", "File1");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile1.log", new DateTime(2011, 1, 1, 11, 30, 0, 0));

            File.WriteAllText(databasePathLogPath + @"\LogFile2.log", "File2");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile2.log", new DateTime(2011, 1, 1, 11, 30, 30, 500));

            File.WriteAllText(databasePathLogPath + @"\LogFile3.log", "File3");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile3.log", new DateTime(2011, 1, 1, 11, 32, 12, 500));

            DateTime logEntryDateStamp = DateTime.MaxValue;
            string actual;
            actual = target.GetDetailedDatabaseLogFileContents(rootLogPath, serverAndDatabase, logEntryDateStamp);

            Directory.Delete(rootLogPath, true);

            Assert.IsTrue(actual.IndexOf("File3") > -1);
        }

        ///A test for GetDetailedDatabaseLogFileContents
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetDetailedDatabaseLogFileContentsTest_LogDateTooEarly_GetNoFile()
        {
            string serverAndDatabase = @"Server1\Instance1.Database1";

            BuildService_Accessor target = new BuildService_Accessor();

            string rootLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(rootLogPath);

            string databasePathLogPath = rootLogPath + @"\Server1\Instance1\Database1";
            Directory.CreateDirectory(databasePathLogPath);


            File.WriteAllText(databasePathLogPath + @"\LogFile1.log", "File1");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile1.log", new DateTime(2011, 1, 1, 11, 30, 0, 0));

            File.WriteAllText(databasePathLogPath + @"\LogFile2.log", "File2");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile2.log", new DateTime(2011, 1, 1, 11, 30, 30, 500));

            File.WriteAllText(databasePathLogPath + @"\LogFile3.log", "File3");
            File.SetLastWriteTime(databasePathLogPath + @"\LogFile3.log", new DateTime(2011, 1, 1, 11, 32, 12, 500));

            DateTime logEntryDateStamp = new DateTime(2011, 1, 1, 11, 0, 0, 0);
            string actual;
            string expected = String.Format("Unable to find log file in the directory \"{0}\" for requested date {1}", databasePathLogPath, logEntryDateStamp.ToString());
            actual = target.GetDetailedDatabaseLogFileContents(rootLogPath, serverAndDatabase, logEntryDateStamp);

            Directory.Delete(rootLogPath, true);

            Assert.AreEqual(expected, actual);
        }
        ///A test for GetDetailedDatabaseLogFileContents
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetDetailedDatabaseLogFileContentsTest_MissingRootDirectory()
        {
            string serverAndDatabase = @"Server1\Instance1.Database1";

            BuildService_Accessor target = new BuildService_Accessor();

            string rootLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            string databasePathLogPath = rootLogPath + @"\Server1\Instance1\Database1";

            DateTime logEntryDateStamp = new DateTime(2011, 1, 1, 11, 0, 0, 0);
            string actual;
            string expected = String.Format("Unable to find directory \"{0}\" for requested database {1}", rootLogPath, serverAndDatabase);
            actual = target.GetDetailedDatabaseLogFileContents(rootLogPath, serverAndDatabase, logEntryDateStamp);



            Assert.AreEqual(expected, actual);
        }
        ///A test for GetDetailedDatabaseLogFileContents
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetDetailedDatabaseLogFileContentsTest_MissingDatabaseDirectory()
        {
            string serverAndDatabase = @"Server1\Instance1.Database1";

            BuildService_Accessor target = new BuildService_Accessor();

            string rootLogPath = Path.GetTempPath() + Guid.NewGuid().ToString();
            Directory.CreateDirectory(rootLogPath);
            string databasePathLogPath = rootLogPath + @"\Server1\Instance1\Database1";

            DateTime logEntryDateStamp = new DateTime(2011, 1, 1, 11, 0, 0, 0);
            string actual;
            string expected = String.Format("Unable to find directory \"{0}\" for requested database {1}", databasePathLogPath, serverAndDatabase);
            actual = target.GetDetailedDatabaseLogFileContents(rootLogPath, serverAndDatabase, logEntryDateStamp);

            Directory.Delete(rootLogPath, true);

            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region GetLastExecutionCommitsLogTest
        /// <summary>
        ///A test for GetLastExecutionCommitsLog
        ///</summary>
        [TestMethod()]
        public void GetLastExecutionCommitsLogTest_FileNotThere()
        {
            string historyFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SqlBuildManager.BuildHistory.xml";
            File.WriteAllText(historyFile, Properties.Resources.SqlBuildManager_BuildHistory);

            if (!Directory.Exists(@"C:\temp\run4"))
                Directory.CreateDirectory(@"C:\temp\run4");

            if(File.Exists(@"C:\temp\run4\Commits.log"))
                File.Delete(@"C:\temp\run4\Commits.log");

            BuildService target = new BuildService();
            string expected = @"Unable to retrieve C:\temp\run4\Commits.log";
            string actual;
            actual = target.GetLastExecutionCommitsLog();
            Assert.AreEqual(expected, actual);

        }

        /// <summary>
        ///A test for GetLastExecutionCommitsLog
        ///</summary>
        [TestMethod()]
        public void GetLastExecutionCommitsLogTest_FileThere()
        {
            string historyFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SqlBuildManager.BuildHistory.xml";
            File.WriteAllText(historyFile, Properties.Resources.SqlBuildManager_BuildHistory);

            if (!Directory.Exists(@"C:\temp\run4"))
                Directory.CreateDirectory(@"C:\temp\run4");

            File.WriteAllText(@"C:\temp\run4\Commits.log", "Found me");

            BuildService target = new BuildService();
 
            string actual;
            actual = target.GetLastExecutionCommitsLog();
            Assert.IsTrue(actual.IndexOf("Found me") > -1);

        }

        #endregion



        #region GetBuildHistoryRootLogPathTest
        /// <summary>
        ///A test for GetBuildHistoryRootLogPath
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void GetBuildHistoryRootLogPathTest_GetMiddlePath()
        {
            string historyFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SqlBuildManager.BuildHistory.xml";
            File.WriteAllText(historyFile, Properties.Resources.SqlBuildManager_BuildHistory);

            BuildService_Accessor target = new BuildService_Accessor();
            DateTime submittedDate = new DateTime(2010,10,26,16,2,59);
            string expected = @"C:\temp\run2";
            string actual;
            actual = target.GetBuildHistoryRootLogPath(submittedDate);
            Assert.AreEqual(expected, actual);
        }
        #endregion


        #region ReadBuildHistoryFileTest
        /// <summary>
        ///A test for ReadBuildHistoryFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void ReadBuildHistoryFileTest_GoodRead()
        {
            BuildService_Accessor target = new BuildService_Accessor();
            string historyFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SqlBuildManager.BuildHistory.xml";
            File.WriteAllText(historyFile, Properties.Resources.SqlBuildManager_BuildHistory);


            IList<BuildRecord> actual;
            actual = target.ReadBuildHistoryFile();

            Assert.IsNotNull(actual);
            Assert.AreEqual(4, actual.Count);
            Assert.AreEqual("myuseridAA", actual[3].RequestedBy);

        }

        /// <summary>
        ///A test for ReadBuildHistoryFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void ReadBuildHistoryFileTest_MissingFile()
        {
            BuildService_Accessor target = new BuildService_Accessor();
            string historyFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SqlBuildManager.BuildHistory.xml";
            if (File.Exists(historyFile))
                File.Delete(historyFile);


            IList<BuildRecord> actual;
            actual = target.ReadBuildHistoryFile();

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Count);

        }

        /// <summary>
        ///A test for ReadBuildHistoryFile
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlBuildManager.Services.dll")]
        public void ReadBuildHistoryFileTest_BadFile()
        {
            BuildService_Accessor target = new BuildService_Accessor();
            string historyFile = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location) + @"\SqlBuildManager.BuildHistory.xml";
            File.WriteAllText(historyFile, "This is invalid data");


            IList<BuildRecord> actual;
            actual = target.ReadBuildHistoryFile();

            Assert.IsNotNull(actual);
            Assert.AreEqual(0, actual.Count);

        }
        #endregion
    }
}
