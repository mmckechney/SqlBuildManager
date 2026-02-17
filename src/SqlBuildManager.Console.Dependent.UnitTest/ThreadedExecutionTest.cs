using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using Microsoft.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using MoreLinq.Extensions;
using Microsoft.Extensions.Logging;

namespace SqlBuildManager.Console.Dependent.UnitTest
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
        private static string GetLocalhostFolderName(string loggingRoot)
        {
            if (Directory.Exists(Path.Combine(loggingRoot, "working", "(local)", "SQLEXPRESS")))
            {
                return Path.Combine(loggingRoot, "working", "(local)", "SQLEXPRESS");
            }
            else if (Directory.Exists(Path.Combine(loggingRoot, "working", "localhost", "SQLEXPRESS")))
            {
                return Path.Combine(loggingRoot, "working", "localhost", "SQLEXPRESS");
            }
            else
            {
                throw new Exception($"Unable to find localhost temp directory at root: {loggingRoot}");
            }
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

        public static IEnumerable<string> ReadLines(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }

        #region ExecuteTest - Not Trial - Transactional
        [TestMethod()]
        public async Task ExecuteTest_ConcurrencyByNumber_1()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);
            init.CopyDbConfigFile10ToTestPath();

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "--loglevel","debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0",
                 "--concurrency", "1",
                "--concurrencytype",  "Count"

            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int actual;
            actual = await target.ExecuteAsync();

            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string[] executionLogFile = ReadLines(Path.Combine(loggingPath, "SqlBuildManager.ThreadedExecution.log")).ToArray();

                //Should be all sequential!
                Assert.IsTrue(executionLogFile[2].IndexOf("SqlBuildTest: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[3].IndexOf("SqlBuildTest: Starting up thread") > -1);
                Assert.IsTrue(executionLogFile[4].IndexOf("SqlBuildTest: Thread complete") > -1);

                Assert.IsTrue(executionLogFile[5].IndexOf("SqlBuildTest1: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[6].IndexOf("SqlBuildTest1: Starting up thread") > -1);
                Assert.IsTrue(executionLogFile[7].IndexOf("SqlBuildTest1: Thread complete") > -1);

                Assert.IsTrue(executionLogFile[8].IndexOf("SqlBuildTest2: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[9].IndexOf("SqlBuildTest2: Starting up thread") > -1);
                Assert.IsTrue(executionLogFile[10].IndexOf("SqlBuildTest2: Thread complete") > -1);

                Assert.IsTrue(executionLogFile[11].IndexOf("SqlBuildTest3: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[12].IndexOf("SqlBuildTest3: Starting up thread") > -1);
                Assert.IsTrue(executionLogFile[13].IndexOf("SqlBuildTest3: Thread complete") > -1);
            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }

            }

        }
        [TestMethod()]
        public async Task ExecuteTest_ConcurrencyByNumber_2()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);
            init.CopyDbConfigFile20ToTestPath();

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0",
                 "--concurrency", "2",
                "--concurrencytype",  "Count"
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int actual;
            actual = await target.ExecuteAsync();

            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string[] executionLogFile = ReadLines(Path.Combine(loggingPath, "SqlBuildManager.ThreadedExecution.log")).ToArray();
                //Should not all sequential!
                Assert.IsTrue(executionLogFile[2].IndexOf("SqlBuildTest: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[3].IndexOf("SqlBuildTest: Starting up thread") > -1);

                Assert.IsTrue(executionLogFile[4].IndexOf("SqlBuildTest10: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[5].IndexOf("SqlBuildTest10: Starting up thread") > -1);

            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }

            }

        }
        [TestMethod()]
        public async Task ExecuteTest_ConcurrencyByNumber_4()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);
            init.CopyDbConfigFile100ToTestPath();

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0",
                 "--concurrency", "4",
                "--concurrencytype",  "Count"
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int actual;
            actual = await target.ExecuteAsync();

            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string[] executionLogFile = ReadLines(Path.Combine(loggingPath, "SqlBuildManager.ThreadedExecution.log")).ToArray();
                //Should not all sequential!
                Assert.IsTrue(executionLogFile[2].IndexOf("SqlBuildTest: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[3].IndexOf("SqlBuildTest: Starting up thread") > -1);

                Assert.IsTrue(executionLogFile[4].IndexOf("SqlBuildTest6: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[5].IndexOf("SqlBuildTest6: Starting up thread") > -1);

                Assert.IsTrue(executionLogFile[6].IndexOf("SqlBuildTest12: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[7].IndexOf("SqlBuildTest12: Starting up thread") > -1);

                Assert.IsTrue(executionLogFile[8].IndexOf("SqlBuildTest18: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[9].IndexOf("SqlBuildTest18: Starting up thread") > -1);

            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }

            }

        }

        [TestMethod()]
        public async Task ExecuteTest_ConcurrencyByServer()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);
            init.CopyDbConfigFile20ToTestPath();

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0",
                 "--concurrency", "1",
                "--concurrencytype",  "Server"
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int actual;
            actual = await target.ExecuteAsync();

            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");


                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string[] executionLogFile = ReadLines(Path.Combine(loggingPath, "SqlBuildManager.ThreadedExecution.log")).ToArray();

                //Should be all sequential!
                Assert.IsTrue(executionLogFile[2].IndexOf("SqlBuildTest: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[3].IndexOf("SqlBuildTest: Starting up thread") > -1);
                Assert.IsTrue(executionLogFile[4].IndexOf("SqlBuildTest: Thread complete") > -1);

                Assert.IsTrue(executionLogFile[5].IndexOf("SqlBuildTest1: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[6].IndexOf("SqlBuildTest1: Starting up thread") > -1);
                Assert.IsTrue(executionLogFile[7].IndexOf("SqlBuildTest1: Thread complete") > -1);

                Assert.IsTrue(executionLogFile[8].IndexOf("SqlBuildTest2: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[9].IndexOf("SqlBuildTest2: Starting up thread") > -1);
                Assert.IsTrue(executionLogFile[10].IndexOf("SqlBuildTest2: Thread complete") > -1);

                Assert.IsTrue(executionLogFile[11].IndexOf("SqlBuildTest3: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[12].IndexOf("SqlBuildTest3: Starting up thread") > -1);
                Assert.IsTrue(executionLogFile[13].IndexOf("SqlBuildTest3: Thread complete") > -1);
            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }

            }

        }
        [TestMethod()]
        public async Task ExecuteTest_ConcurrencyByMaxByServer_2()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);
            init.CopyDbConfigFile50ToTestPath();

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0",
                 "--concurrency", "2",
                "--concurrencytype",  "MaxPerServer"
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)ExecutionReturn.Successful;
            int actual;
            actual = await target.ExecuteAsync();

            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                Assert.AreEqual(expected, actual);
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string[] executionLogFile = ReadLines(Path.Combine(loggingPath, "SqlBuildManager.ThreadedExecution.log")).ToArray();
                //Should not all sequential!
                Assert.IsTrue(executionLogFile[2].IndexOf("SqlBuildTest: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[3].IndexOf("SqlBuildTest: Starting up thread") > -1);

                Assert.IsTrue(executionLogFile[4].IndexOf("SqlBuildTest4: Queuing up thread") > -1);
                Assert.IsTrue(executionLogFile[5].IndexOf("SqlBuildTest4: Starting up thread") > -1);

            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }

            }

        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public async Task ExecuteTest_CommitWithoutUsingRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
            localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";

            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0",
                "--authtype",
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)ExecutionReturn.Successful;
            int actual;
            actual = await target.ExecuteAsync();

            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                Assert.AreEqual(expected, actual);
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();

                //SqlBuildTest should still committed with no timeout messages
                string[] logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") == -1, "SqlBuildTest contains a 'ROLLBACK' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");



                //SqlBuildTest1 should still committed with no timeout messages
                logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest1"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest1 Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }

            }

        }

        [TestMethod()]
        public async Task InfiniteLock_Test()
        {
            var res = StartInfiniteLockingThread(1.5);
            Assert.AreEqual(0, res);
        }
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public async Task ExecuteTest_CommitWithRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();
            int retryCount = 20;

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", retryCount.ToString()
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)ExecutionReturn.Successful;
            int actual;

            // Use a cancellation token to properly clean up the locking task
            using var lockCts = new CancellationTokenSource();
            // Use a signal to know when the lock is actually acquired
            using var lockAcquired = new ManualResetEventSlim(false);
            Task lockTask = null;

            try
            {
                // Start the locking thread with proper signaling
                lockTask = System.Threading.Tasks.Task.Run(() =>
                {
                    // Hold lock for 30 seconds - enough for some retries to fail, but 
                    // releases before all 20 retries are exhausted (each retry ~3 sec)
                    StartLockingThreadWithSignal(0.5, lockAcquired, lockCts.Token);
                });

                // Wait for the lock to actually be acquired (with timeout)
                bool lockWasAcquired = lockAcquired.Wait(TimeSpan.FromSeconds(30));
                Assert.IsTrue(lockWasAcquired, "Failed to acquire database lock within 30 seconds");

                // Small delay to ensure lock is fully established
                await System.Threading.Tasks.Task.Delay(500);

                actual = await target.ExecuteAsync();

                if (actual == -600)
                    Assert.Fail("Unable to complete test!");

                Assert.AreEqual(expected, actual);
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();

                //SqlBuildTest should still committed with at least one timeout message
                string[] logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                Regex regFindTimeout = new Regex("Timeout expired", RegexOptions.IgnoreCase);

                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count > 0, "No Timeout messages were encountered");
                Assert.IsTrue(matches.Count < retryCount + 1, String.Format("There were more Timeout retries than configured for! Allocated {0}; Found {1}", retryCount + 1, matches.Count));
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest does not contain a 'ROLLBACK' message. It should for the retries.");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should still committed with no timeout messages
                logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest1"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest1 Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");



            }
            finally
            {
                // Cancel the locking task so it releases the lock
                lockCts.Cancel();
                
                // Wait briefly for the lock task to complete
                if (lockTask != null)
                {
                    try { await lockTask.WaitAsync(TimeSpan.FromSeconds(5)); } catch { }
                }

                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public async Task ExecuteTest_RollbackWithThreeRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();
            int retryCount = 3;

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", retryCount.ToString()
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)ExecutionReturn.FinishingWithErrors;
            int actual;

            // Use a cancellation token to properly clean up the locking task
            using var lockCts = new CancellationTokenSource();
            using var lockAcquired = new ManualResetEventSlim(false);
            Task lockTask = null;

            try
            {
                // Start the locking thread - lock for 1 minute to ensure all 3 retries fail
                // (3 retries × ~3 sec each = ~12 seconds, so 1 min lock will outlast all retries)
                lockTask = System.Threading.Tasks.Task.Run(() =>
                {
                    StartLockingThreadWithSignal(1.0, lockAcquired, lockCts.Token);
                });

                // Wait for the lock to actually be acquired
                bool lockWasAcquired = lockAcquired.Wait(TimeSpan.FromSeconds(30));
                Assert.IsTrue(lockWasAcquired, "Failed to acquire database lock within 30 seconds");
                await System.Threading.Tasks.Task.Delay(500);

                actual = await target.ExecuteAsync();

                if (actual == -600)
                    Assert.Fail("Unable to complete test!");

                Assert.AreEqual(expected, actual);
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();

                //SqlBuildTest should have rolledback with 1 more timeout messages than is set for the retry value
                Regex regFindTimeout = new Regex("imeout expired", RegexOptions.IgnoreCase);
                string[] logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count == retryCount + 1, String.Format("Timeout message count of {0} does not equal retrycount +1 value of {1}", matches.Count.ToString(), (retryCount + 1).ToString()));
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'ROLLBACK' message");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest Log contains a 'COMMIT' message - it should not for a rollback");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should have committed
                logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest1"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest1 Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");


            }
            finally
            {
                lockCts.Cancel();
                if (lockTask != null)
                {
                    try { await lockTask.WaitAsync(TimeSpan.FromSeconds(5)); } catch { }
                }
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public async Task ExecuteTest_RollbackWithFiveRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();
            int retryCount = 5;

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", retryCount.ToString()
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)ExecutionReturn.FinishingWithErrors;
            int actual;

            // Use a cancellation token to properly clean up the locking task
            using var lockCts = new CancellationTokenSource();
            using var lockAcquired = new ManualResetEventSlim(false);
            Task lockTask = null;

            try
            {
                // Start the locking thread - lock for 1 minute to ensure all 5 retries fail
                // (5 retries × ~3 sec each = ~18 seconds, so 1 min lock will outlast all retries)
                lockTask = System.Threading.Tasks.Task.Run(() =>
                {
                    StartLockingThreadWithSignal(1.0, lockAcquired, lockCts.Token);
                });

                // Wait for the lock to actually be acquired
                bool lockWasAcquired = lockAcquired.Wait(TimeSpan.FromSeconds(30));
                Assert.IsTrue(lockWasAcquired, "Failed to acquire database lock within 30 seconds");
                await System.Threading.Tasks.Task.Delay(500);

                actual = await target.ExecuteAsync();

                if (actual == -600)
                    Assert.Fail("Unable to complete test!");

                Assert.AreEqual(expected, actual);
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();

                //SqlBuildTest should have rolledback with 1 more timeout messages than is set for the retry value
                Regex regFindTimeout = new Regex("Timeout Expired", RegexOptions.IgnoreCase);
                string[] logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count == retryCount + 1, String.Format("Timeout message count of {0} does not equal retrycount +1 value of {1}", matches.Count.ToString(), (retryCount + 1).ToString()));
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'ROLLBACK' message");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest Log contains a 'COMMIT' message - it should not for a rollback");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should have committed
                logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest1"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") > -1, "SqlBuildTest1 Log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                lockCts.Cancel();
                if (lockTask != null)
                {
                    try { await lockTask.WaitAsync(TimeSpan.FromSeconds(5)); } catch { }
                }
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public async Task ExecuteTest_NegativeTimeoutRetryCount()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest1,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount","-1"
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);

            int expected = (int)ExecutionReturn.NegativeTimeoutRetryCount;
            int actual;
            actual = await target.ExecuteAsync();
            SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();




            if (actual == -600)
                Assert.Fail("Unable to complete test!");

            Assert.AreEqual(expected, actual);

            try
            {
                if (File.Exists(sbmFileName))
                    File.Delete(sbmFileName);
                if (File.Exists(multiDbOverrideSettingFileName))
                    File.Delete(multiDbOverrideSettingFileName);
                if (Directory.Exists(loggingPath))
                    Directory.Delete(loggingPath, true);
            }
            catch { }
        }

        #endregion

        #region ExecuteTest - Not Trial - Non Transactional
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public async Task ExecuteTest_SuccessWithoutTransactionsNoUsingRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "false",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount","0"
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)ExecutionReturn.Successful;
            int actual;
            actual = await target.ExecuteAsync();
            SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                Assert.AreEqual(expected, actual, "Run result was not the expected value of \"Success\"");

                //SqlBuildTest should still committed with no timeout messages
                string[] logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest Log contains a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") == -1, "SqlBuildTest log contains a 'ROLLBACK' message");
                Assert.IsTrue(logContents.IndexOf("Completed: No Transaction Set") > -1, "SqlBuildTest does not contain a 'Completed: No Transaction Set' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");



                //SqlBuildTest1 should still committed with no timeout messages
                logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest1"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest1 Log contains a'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("Completed: No Transaction Set") > -1, "SqlBuildTest1 does not contain a 'Completed: No Transaction Set' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }

            }

        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public async Task ExecuteTest_NonTransactionalWithRetriesArgsFailure()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllText(sbmFileName, "");
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, "");
            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "false",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount","20"
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)ExecutionReturn.BadRetryCountAndTransactionalCombo;
            int actual;
            SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
            try
            {
                actual = await target.ExecuteAsync();
                Assert.AreEqual(expected, actual);
            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }
            }

        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public async Task ExecuteTest_ErrorWithoutTransactionsNoUsingRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "false",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount","0"
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)ExecutionReturn.FinishingWithErrors;
            int actual;

            // Use a cancellation token to properly clean up the locking task
            using var lockCts = new CancellationTokenSource();
            using var lockAcquired = new ManualResetEventSlim(false);
            Task lockTask = null;

            try
            {
                // Start the locking thread - lock for 30 seconds (no retries, so just needs to hold during single attempt)
                lockTask = System.Threading.Tasks.Task.Run(() =>
                {
                    StartLockingThreadWithSignal(0.5, lockAcquired, lockCts.Token);
                });

                // Wait for the lock to actually be acquired
                bool lockWasAcquired = lockAcquired.Wait(TimeSpan.FromSeconds(30));
                Assert.IsTrue(lockWasAcquired, "Failed to acquire database lock within 30 seconds");
                await System.Threading.Tasks.Task.Delay(500);

                actual = await target.ExecuteAsync();
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                Assert.AreEqual(expected, actual);


                //SqlBuildTest should still committed with no timeout messages
                string[] logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest Log contains a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") == -1, "SqlBuildTest log contains a 'ROLLBACK' message");
                Assert.IsTrue(logContents.IndexOf("ERROR: No Transaction Set") > -1, "SqlBuildTest does not contain a 'ERROR: No Transaction Set' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");



                //SqlBuildTest1 should still committed with no timeout messages
                logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest1"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "SqlBuildTest1 Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, "SqlBuildTest1 Log contains a'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("Completed: No Transaction Set") > -1, "SqlBuildTest1 does not contain a 'Completed: No Transaction Set' message");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                lockCts.Cancel();
                if (lockTask != null)
                {
                    try { await lockTask.WaitAsync(TimeSpan.FromSeconds(5)); } catch { }
                }
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }
            }

        }
        #endregion

        #region ExecuteTest - Trial - Transactional
        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public async Task ExecuteTest_TrialSuccessWithRollbackWithRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();
            int retryCount = 20;

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "true",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", retryCount.ToString()
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)ExecutionReturn.Successful;
            int actual;

            // Use a cancellation token to properly clean up the locking task
            using var lockCts = new CancellationTokenSource();
            using var lockAcquired = new ManualResetEventSlim(false);
            Task lockTask = null;

            try
            {
                // Start the locking thread - lock for 30 seconds, allowing some retries then success
                lockTask = System.Threading.Tasks.Task.Run(() =>
                {
                    StartLockingThreadWithSignal(0.5, lockAcquired, lockCts.Token);
                });

                // Wait for the lock to actually be acquired
                bool lockWasAcquired = lockAcquired.Wait(TimeSpan.FromSeconds(30));
                Assert.IsTrue(lockWasAcquired, "Failed to acquire database lock within 30 seconds");
                await System.Threading.Tasks.Task.Delay(500);

                actual = await target.ExecuteAsync();
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                if (actual == -600)
                    Assert.Fail("Unable to complete test!");

                Assert.AreEqual(expected, actual);

                //SqlBuildTest should have rolledback with 1 more timeout messages than is set for the retry value
                Regex regFindTimeout = new Regex("Error Message: Timeout expired");
                string[] logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count < retryCount + 1, String.Format("Timeout message count of {0} should be less than the retrycount +1 value of {1}", matches.Count.ToString(), (retryCount + 1).ToString()));
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'ROLLBACK' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, " SqlBuildTest Log contains a 'COMMIT' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should have committed
                logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest1"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, " SqlBuildTest1 Log does not contain a 'ROLLBACK' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, " SqlBuildTest1 Log contains a 'COMMIT' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                lockCts.Cancel();
                if (lockTask != null)
                {
                    try { await lockTask.WaitAsync(TimeSpan.FromSeconds(5)); } catch { }
                }
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }

        /// <summary>
        ///A test for Execute
        ///</summary>
        [TestMethod()]
        public async Task ExecuteTest_TrialFailureRollbackWithRetries()
        {
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.InsertForThreadedTest);

            string cfgContents = @"localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest
localhost\SQLEXPRESS:SqlBuildTest,SqlBuildTest1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();
            int retryCount = 3;

            string[] args = Initialization.GetAuthArgs().Concat(new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "true",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", retryCount.ToString()
            }).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)ExecutionReturn.FinishingWithErrors;
            int actual;

            // Use a cancellation token to properly clean up the locking task
            using var lockCts = new CancellationTokenSource();
            using var lockAcquired = new ManualResetEventSlim(false);
            Task lockTask = null;

            try
            {
                // Start the locking thread - lock for 1 minute to ensure all 3 retries fail
                lockTask = System.Threading.Tasks.Task.Run(() =>
                {
                    StartLockingThreadWithSignal(1.0, lockAcquired, lockCts.Token);
                });

                // Wait for the lock to actually be acquired
                bool lockWasAcquired = lockAcquired.Wait(TimeSpan.FromSeconds(30));
                Assert.IsTrue(lockWasAcquired, "Failed to acquire database lock within 30 seconds");
                await System.Threading.Tasks.Task.Delay(500);

                actual = await target.ExecuteAsync();
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                if (actual == -600)
                    Assert.Fail("Unable to complete test!");

                Assert.AreEqual(expected, actual);

                //SqlBuildTest should have rolledback with 1 more timeout messages than is set for the retry value
                Regex regFindTimeout = new Regex("Timeout expired", RegexOptions.IgnoreCase);
                string[] logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest log file necessary to complete test");
                string logContents = File.ReadAllText(logFiles[0]);
                MatchCollection matches = regFindTimeout.Matches(logContents);
                Assert.IsTrue(matches.Count == retryCount + 1, String.Format("Timeout message count of {0} should be equal to the retrycount +1 value of {1}", matches.Count.ToString(), (retryCount + 1).ToString()));
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, "SqlBuildTest Log does not contain a 'ROLLBACK' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, " SqlBuildTest Log contains a 'COMMIT' message - it should not for a trial run");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest") > -1, "SqlBuildTest Log does not contain the proper database name");

                //SqlBuildTest1 should have committed
                logFiles = Directory.GetFiles(Path.Combine(GetLocalhostFolderName(loggingPath), "SqlBuildTest1"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find SqlBuildTest1 log file necessary to complete test");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Error Message: Timeout expired.") == -1, "Log contains a 'Timeout expired message'");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK TRANSACTION") > -1, " SqlBuildTest1 Log does not contain a 'ROLLBACK' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("COMMIT TRANSACTION") == -1, " SqlBuildTest1 Log contains a 'COMMIT' message - it should for a trial run");
                Assert.IsTrue(logContents.IndexOf("use SqlBuildTest1") > -1, "SqlBuildTest1 Log does not contain the proper database name");

            }
            finally
            {
                lockCts.Cancel();
                if (lockTask != null)
                {
                    try { await lockTask.WaitAsync(TimeSpan.FromSeconds(5)); } catch { }
                }
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName))
                        File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }
        #endregion

        private int StartInfiniteLockingThread(double waitMin)
        {
            string connStr = string.Empty;
            string cmdStr = string.Empty;
            try
            {
                connStr = string.Format(Initialization.ConnectionString, "SqlBuildTest");
                SqlConnection conn = new SqlConnection(connStr);

                cmdStr = string.Format(Properties.Resources.TableLockingScript, waitMin.ToString());
                SqlCommand cmd = new SqlCommand(cmdStr, conn);
                cmd.CommandTimeout = (int)(waitMin * 60 + 30);
                conn.Open();
                cmd.ExecuteNonQuery();
                return 0;
            }
            catch(Exception exe)
            {
                System.Console.WriteLine($"Error: {exe.Message}; {Environment.NewLine}Connection String:{connStr}{Environment.NewLine}Query:{cmdStr}");
                return 1;
            }
        }

        /// <summary>
        /// Starts a locking thread that signals when the lock is acquired.
        /// This is more reliable than the original method because it ensures the lock
        /// is actually held before returning control to the test.
        /// </summary>
        /// <param name="waitMin">How long to hold the lock in minutes</param>
        /// <param name="lockAcquired">Event that will be signaled when lock is acquired</param>
        /// <param name="cancellationToken">Token to cancel the lock early</param>
        /// <returns>0 on success, 1 on failure</returns>
        private int StartLockingThreadWithSignal(double waitMin, ManualResetEventSlim lockAcquired, CancellationToken cancellationToken)
        {
            string connStr = string.Empty;
            try
            {
                connStr = string.Format(Initialization.ConnectionString, "SqlBuildTest");
                using var conn = new SqlConnection(connStr);
                conn.Open();

                // Start a transaction and acquire the lock
                using var transaction = conn.BeginTransaction();
                
                // Acquire the exclusive table lock
                string lockCmd = "SELECT TOP 1 * FROM dbo.TransactionTest WITH (TABLOCKX, HOLDLOCK) WHERE 0 = 1";
                using (var cmd = new SqlCommand(lockCmd, conn, transaction))
                {
                    cmd.CommandTimeout = 30;
                    cmd.ExecuteNonQuery();
                }

                // Signal that the lock is now held
                lockAcquired.Set();

                // Wait for the specified duration or until cancelled
                try
                {
                    // Convert minutes to milliseconds and wait
                    int waitMs = (int)(waitMin * 60 * 1000);
                    cancellationToken.WaitHandle.WaitOne(waitMs);
                }
                catch (OperationCanceledException)
                {
                    // Expected when test completes
                }

                // Transaction is rolled back when disposed
                transaction.Rollback();
                return 0;
            }
            catch (Exception exe)
            {
                System.Console.WriteLine($"Error in locking thread: {exe.Message}; {Environment.NewLine}Connection String:{connStr}");
                lockAcquired.Set(); // Signal anyway so test doesn't hang
                return 1;
            }
        }

        private void StartSqlTimeoutThread(object targetDatabase)
        {
            try
            {
                string connStr = string.Format(Initialization.ConnectionString, targetDatabase.ToString());
                SqlConnection conn = new SqlConnection(connStr);
                SqlCommand cmd = new SqlCommand(Properties.Resources.sql_waitfor_createtimeout, conn);
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch
            {

            }
        }
    }
}
