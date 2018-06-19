using SqlBuildManager.Console;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild;
using System.Collections.Generic;
using System;
using System.IO;
using SqlBuildManager.Interfaces.Console;
using System.Data.SqlClient;
using System.Threading;
using System.Text.RegularExpressions;
namespace SqlBuildManager.Console.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for ThreadedExecutionTest and is intended
    ///to contain all ThreadedExecutionTest Unit Tests
    ///</summary>
    [TestClass()]
    public class ThreadedExecutionTest
    {


        private TestContext testContextInstance;
        private static List<Initialization> initObjs = new List<Initialization>();
        private Initialization GetInitializationObject()
        {
            Initialization i = new Initialization();
            initObjs.Add(i);
            return i;
        }
        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }


        // 
        //You can use the following additional attributes as you write your tests:
        //
        //Use ClassInitialize to run code before running the first test in the class
        //[ClassInitialize()]
        //public static void MyClassInitialize(TestContext testContext)
        //{
        //}
        //
        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            foreach (Initialization i in initObjs)
                i.Dispose();
        }

        #region ExecuteTest - Not Trial - Transactional
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_CommitWithoutUsingRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[8];
            args[0] = "/Action=threaded";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=false";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=0";
            ThreadedExecution target = new ThreadedExecution(args);
            int expected = (int)ExecutionReturn.Successful;
            int actual;
            actual = target.Execute();

            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                Assert.AreEqual(expected, actual);

                //SqlBuildTest should still committed with no timeout messages
                string[] logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") == -1, "SqlBuildTest contains a 'ROLLBACK' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");



                //SqlBuildTest1 should still committed with no timeout messages
                logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest1\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest1 Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                if (Directory.Exists(loggingPath))
                    Directory.Delete(loggingPath, true);
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);

            }

       }
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_CommitWithRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();
            int retryCount = 20;
            string[] args = new string[8];
            args[0] = "/Action=threaded";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=false";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=" + retryCount.ToString();
            ThreadedExecution target = new ThreadedExecution(args);
            int expected = (int)ExecutionReturn.Successful;
            int actual;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(StartInfiniteLockingThread);
                THRInfinite.Start(600000);

                actual = target.Execute();

                if (actual == -600)
                    Assert.Fail("Unable to complete test!");

                Assert.AreEqual(expected, actual);

                //SqlBuildTest should still committed with at least one timeout message
                string[] logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest\","*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                Regex regFindTimeout = new Regex("Error Message: Timeout expired");

                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count > 0, "No Timeout messages were encountered");
                Assert.IsTrue(matches.Count < retryCount+1 , String.Format("There were more Timeout retries than configured for! Allocated {0}; Found {1}",retryCount+1, matches.Count));
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest does not contain a 'ROLLBACK' message. It should for the retries.");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should still committed with no timeout messages
                logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest1\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest1 Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");



            }
            finally
            {
                if (THRInfinite != null)
                    THRInfinite.Abort();

                if (Directory.Exists(loggingPath))
                    Directory.Delete(loggingPath, true);
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);
            }
        }
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_RollbackWithThreeRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();
            int retryCount = 3;
            string[] args = new string[8];
            args[0] = "/Action=threaded";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=false";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=" + retryCount.ToString();
            ThreadedExecution target = new ThreadedExecution(args);
            int expected = (int)ExecutionReturn.FinishingWithErrors;
            int actual;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(StartInfiniteLockingThread);
                THRInfinite.Start(10000000);

                actual = target.Execute();

                if (actual == -600)
                    Assert.Fail("Unable to complete test!");

                Assert.AreEqual(expected, actual);

                //SqlBuildTest should have rolledback with 1 more timeout messages than is set for the retry value
                Regex regFindTimeout = new Regex("Error Message: Timeout expired");
                string[] logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count == retryCount + 1, String.Format("Timeout message count of {0} does not equal retrycount +1 value of {1}", matches.Count.ToString(),(retryCount+1).ToString()));
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'ROLLBACK' message");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest Log contains a 'COMMIT' message - it should not for a rollback");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should have committed
                logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest1\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest1 Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");


            }
            finally
            {
                if (THRInfinite != null)
                    THRInfinite.Abort();

                if (Directory.Exists(loggingPath))
                    Directory.Delete(loggingPath, true);
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);
            }
        }
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_RollbackWithFiveRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();
            int retryCount = 5;
            string[] args = new string[8];
            args[0] = "/Action=threaded";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=false";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=" + retryCount.ToString();
            ThreadedExecution target = new ThreadedExecution(args);
            int expected = (int)ExecutionReturn.FinishingWithErrors;
            int actual;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(StartInfiniteLockingThread);
                THRInfinite.Start(10000000);

                actual = target.Execute();

                if (actual == -600)
                    Assert.Fail("Unable to complete test!");

                Assert.AreEqual(expected, actual);

                //SqlBuildTest should have rolledback with 1 more timeout messages than is set for the retry value
                Regex regFindTimeout = new Regex("Error Message: Timeout expired");
                string[] logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count == retryCount + 1, String.Format("Timeout message count of {0} does not equal retrycount +1 value of {1}", matches.Count.ToString(), (retryCount + 1).ToString()));
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'ROLLBACK' message");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest Log contains a 'COMMIT' message - it should not for a rollback");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should have committed
                logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest1\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest1 Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                if (THRInfinite != null)
                    THRInfinite.Abort();

                if (Directory.Exists(loggingPath))
                    Directory.Delete(loggingPath, true);
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);
            }
        }
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_NegativeTimeoutRetryCount()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest1,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[8];
            args[0] = "/Action=threaded";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=false";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=-1";
            ThreadedExecution target = new ThreadedExecution(args);
            int expected = (int)ExecutionReturn.NegativeTimeoutRetryCount;
            int actual;
            actual = target.Execute();

            if (Directory.Exists(loggingPath))
                Directory.Delete(loggingPath, true);
            if (File.Exists(sbmFileName))
                File.Delete(sbmFileName);
            if (File.Exists(multiDbOverrideSettingFileName))
                File.Delete(multiDbOverrideSettingFileName);

            if (actual == -600)
                Assert.Fail("Unable to complete test!");

            Assert.AreEqual(expected, actual);
        }

        #endregion

        #region ExecuteTest - Not Trial - Non Transactional
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_SuccessWithoutTransactionsNoUsingRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[8];
            args[0] = "/Action=threaded";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=false";
            args[3] = "/trial=false";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=0";
            ThreadedExecution target = new ThreadedExecution(args);
            int expected = (int)ExecutionReturn.Successful;
            int actual;
            actual = target.Execute();

            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                Assert.AreEqual(expected, actual);

                //SqlBuildTest should still committed with no timeout messages
                string[] logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest Log contains a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") == -1, "SqlBuildTest log contains a 'ROLLBACK' message");
                Assert.IsTrue(logContents.IndexOf("Completed: No Transaction Set") > -1, "SqlBuildTest does not contain a 'Completed: No Transaction Set' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");



                //SqlBuildTest1 should still committed with no timeout messages
                logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest1\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest1 Log contains a'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("Completed: No Transaction Set") > -1, "SqlBuildTest1 does not contain a 'Completed: No Transaction Set' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                if (Directory.Exists(loggingPath))
                    Directory.Delete(loggingPath, true);
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);

            }

        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_NonTransactionalWithRetriesArgsFailure()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllText(sbmFileName, "");
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, "");
            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[8];
            args[0] = "/Action=threaded";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=false";
            args[3] = "/trial=false";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=20";
            ThreadedExecution target = new ThreadedExecution(args);
            int expected = (int)ExecutionReturn.BadRetryCountAndTransactionalCombo;
            int actual;

            try
            {
                actual = target.Execute();
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);
            }

        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_ErrorWithoutTransactionsNoUsingRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[8];
            args[0] = "/Action=threaded";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=false";
            args[3] = "/trial=false";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=0";
            ThreadedExecution target = new ThreadedExecution(args);
            int expected = (int)ExecutionReturn.FinishingWithErrors;
            int actual;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(StartInfiniteLockingThread);
                THRInfinite.Start(10000000);

                actual = target.Execute();

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                Assert.AreEqual(expected, actual);


                //SqlBuildTest should still committed with no timeout messages
                string[] logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest Log contains a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") == -1, "SqlBuildTest log contains a 'ROLLBACK' message");
                Assert.IsTrue(logContents.IndexOf("ERROR: No Transaction Set") > -1, "SqlBuildTest does not contain a 'ERROR: No Transaction Set' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");



                //SqlBuildTest1 should still committed with no timeout messages
                logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest1\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest1 Log contains a'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("Completed: No Transaction Set") > -1, "SqlBuildTest1 does not contain a 'ERROR: No Transaction Set' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                if (THRInfinite != null)
                    THRInfinite.Abort();

                if (Directory.Exists(loggingPath))
                    Directory.Delete(loggingPath, true);
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);

            }

        }
        #endregion

        #region ExecuteTest - Trial - Transactional
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_TrialSuccessWithRollbackWithRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();
            int retryCount = 20;
            string[] args = new string[8];
            args[0] = "/Action=threaded";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=" + retryCount.ToString();
            ThreadedExecution target = new ThreadedExecution(args);
            int expected = (int)ExecutionReturn.Successful;
            int actual;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(StartInfiniteLockingThread);
                THRInfinite.Start(200000);

                actual = target.Execute();

                if (actual == -600)
                    Assert.Fail("Unable to complete test!");

                Assert.AreEqual(expected, actual);

                //SqlBuildTest should have rolledback with 1 more timeout messages than is set for the retry value
                Regex regFindTimeout = new Regex("Error Message: Timeout expired");
                string[] logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count < retryCount +1 , String.Format("Timeout message count of {0} should be less than the retrycount +1 value of {1}", matches.Count.ToString(), (retryCount + 1).ToString()));
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'ROLLBACK' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, " SqlBuildTest Log contains a 'COMMIT' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should have committed
                logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest1\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, " SqlBuildTest1 Log does not contain a 'ROLLBACK' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, " SqlBuildTest1 Log contains a 'COMMIT' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                if (THRInfinite != null)
                    THRInfinite.Abort();

                if (Directory.Exists(loggingPath))
                    Directory.Delete(loggingPath, true);
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);
            }
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public void ExecuteTest_TrialFailureRollbackWithRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();
            int retryCount = 3;
            string[] args = new string[8];
            args[0] = "/Action=threaded";
            args[1] = "/RootLoggingPath=\"" + loggingPath + "\"";
            args[2] = "/transactional=true";
            args[3] = "/trial=true";
            args[4] = "/override=\"" + multiDbOverrideSettingFileName + "\"";
            args[5] = "/PackageName=\"" + sbmFileName + "\"";
            args[6] = "/DistributionType=equal";
            args[7] = "/TimeoutRetryCount=" + retryCount.ToString();
            ThreadedExecution target = new ThreadedExecution(args);
            int expected = (int)ExecutionReturn.FinishingWithErrors;
            int actual;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(StartInfiniteLockingThread);
                THRInfinite.Start(10000000);

                actual = target.Execute();

                if (actual == -600)
                    Assert.Fail("Unable to complete test!");

                Assert.AreEqual(expected, actual);

                //SqlBuildTest should have rolledback with 1 more timeout messages than is set for the retry value
                Regex regFindTimeout = new Regex("Error Message: Timeout expired");
                string[] logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count == retryCount + 1, String.Format("Timeout message count of {0} should be equal to the retrycount +1 value of {1}", matches.Count.ToString(), (retryCount + 1).ToString()));
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'ROLLBACK' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, " SqlBuildTest Log contains a 'COMMIT' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should have committed
                logFiles = Directory.GetFiles(loggingPath + @"\localhost\SQLEXPRESS\SqlBuildTest1\", "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, " SqlBuildTest1 Log does not contain a 'ROLLBACK' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, " SqlBuildTest1 Log contains a 'COMMIT' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                if (THRInfinite != null)
                    THRInfinite.Abort();

                if (Directory.Exists(loggingPath))
                    Directory.Delete(loggingPath, true);
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);
            }
        }
        #endregion

        private void StartInfiniteLockingThread(object loopCount)
        {
            int loop = (int)loopCount;
            string connStr = string.Format(Initialization.ConnectionString, "SqlBuildTest");
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(String.Format(Properties.Resources.TableLockingScript,loop.ToString()), conn);
            conn.Open();
            cmd.ExecuteNonQuery();
        }
    }
}
