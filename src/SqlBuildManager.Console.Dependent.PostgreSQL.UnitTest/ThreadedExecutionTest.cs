using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Threaded;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Dependent.PostgreSQL.UnitTest
{
    [TestClass()]
    [DoNotParallelize]
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

        public TestContext TestContext
        {
            get { return testContextInstance; }
            set { testContextInstance = value; }
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            foreach (Initialization i in initObjs)
                i.Dispose();
        }

        public static IEnumerable<string> ReadLines(string path)
        {
            using var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan);
            using var sr = new StreamReader(fs, Encoding.UTF8);
            string line;
            while ((line = sr.ReadLine()) != null)
            {
                yield return line;
            }
        }

        /// <summary>
        /// Sequential threaded execution (concurrency=1) against 4 PostgreSQL databases
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_ExecuteTest_ConcurrencyByNumber_1()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_InsertForThreadedTest);
            init.CopyDbConfigFile4ToTestPath();

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional", "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0",
                "--concurrency", "1",
                "--concurrencytype", "Count"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int actual = await target.ExecuteAsync();

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string[] executionLogFile = ReadLines(Path.Combine(loggingPath, "SqlBuildManager.ThreadedExecution.log")).ToArray();

                // With concurrency=1, should be sequential
                Assert.IsTrue(executionLogFile[2].IndexOf("sbm_pg_test: Queuing up thread") > -1, $"Expected sbm_pg_test queue at line 2, got: {executionLogFile[2]}");
                Assert.IsTrue(executionLogFile[3].IndexOf("sbm_pg_test: Starting up thread") > -1, $"Expected sbm_pg_test start at line 3, got: {executionLogFile[3]}");
                Assert.IsTrue(executionLogFile[4].IndexOf("sbm_pg_test: Thread complete") > -1, $"Expected sbm_pg_test complete at line 4, got: {executionLogFile[4]}");

                Assert.IsTrue(executionLogFile[5].IndexOf("sbm_pg_test1: Queuing up thread") > -1, $"Expected sbm_pg_test1 queue at line 5, got: {executionLogFile[5]}");
                Assert.IsTrue(executionLogFile[6].IndexOf("sbm_pg_test1: Starting up thread") > -1, $"Expected sbm_pg_test1 start at line 6, got: {executionLogFile[6]}");
                Assert.IsTrue(executionLogFile[7].IndexOf("sbm_pg_test1: Thread complete") > -1, $"Expected sbm_pg_test1 complete at line 7, got: {executionLogFile[7]}");
            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName)) File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Parallel threaded execution (concurrency=2) against 4 PostgreSQL databases
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_ExecuteTest_ConcurrencyByNumber_2()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_InsertForThreadedTest);
            init.CopyDbConfigFile4ToTestPath();

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional", "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0",
                "--concurrency", "2",
                "--concurrencytype", "Count"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int actual = await target.ExecuteAsync();

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string[] executionLogFile = ReadLines(Path.Combine(loggingPath, "SqlBuildManager.ThreadedExecution.log")).ToArray();

                // With concurrency=2, first two should start before first completes
                Assert.IsTrue(executionLogFile[2].IndexOf("sbm_pg_test: Queuing up thread") > -1, $"Expected first queue at line 2, got: {executionLogFile[2]}");
                Assert.IsTrue(executionLogFile[3].IndexOf("sbm_pg_test: Starting up thread") > -1, $"Expected first start at line 3, got: {executionLogFile[3]}");
                // Second DB should queue before first completes (parallel)
                Assert.IsTrue(executionLogFile[4].IndexOf("Queuing up thread") > -1, $"Expected second queue at line 4, got: {executionLogFile[4]}");
            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName)) File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Higher concurrency (4) against 8 PostgreSQL database targets
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_ExecuteTest_ConcurrencyByNumber_4()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_InsertForThreadedTest);
            init.CopyDbConfigFile8ToTestPath();

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional", "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0",
                "--concurrency", "4",
                "--concurrencytype", "Count"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int actual = await target.ExecuteAsync();

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string[] executionLogFile = ReadLines(Path.Combine(loggingPath, "SqlBuildManager.ThreadedExecution.log")).ToArray();

                // With concurrency=4 and 8 targets, multiple should queue quickly
                Assert.IsTrue(executionLogFile[2].IndexOf("Queuing up thread") > -1, $"Expected queue at line 2, got: {executionLogFile[2]}");
                Assert.IsTrue(executionLogFile[3].IndexOf("Starting up thread") > -1, $"Expected start at line 3, got: {executionLogFile[3]}");
                Assert.IsTrue(executionLogFile[4].IndexOf("Queuing up thread") > -1, $"Expected queue at line 4, got: {executionLogFile[4]}");
            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName)) File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Threaded execution using Server-based concurrency
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_ExecuteTest_ConcurrencyByServer()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_InsertForThreadedTest);
            init.CopyDbConfigFile4ToTestPath();

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional", "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0",
                "--concurrency", "1",
                "--concurrencytype", "Server"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int actual = await target.ExecuteAsync();

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string[] executionLogFile = ReadLines(Path.Combine(loggingPath, "SqlBuildManager.ThreadedExecution.log")).ToArray();

                // Server-based concurrency with 1 server: should be sequential
                Assert.IsTrue(executionLogFile[2].IndexOf("sbm_pg_test: Queuing up thread") > -1, $"Expected first queue at line 2, got: {executionLogFile[2]}");
                Assert.IsTrue(executionLogFile[3].IndexOf("sbm_pg_test: Starting up thread") > -1, $"Expected first start at line 3, got: {executionLogFile[3]}");
                Assert.IsTrue(executionLogFile[4].IndexOf("sbm_pg_test: Thread complete") > -1, $"Expected first complete at line 4, got: {executionLogFile[4]}");
            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName)) File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }
        /// <summary>
        /// MaxByServer concurrency with 2 max per server against 4 databases on same server
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_ExecuteTest_ConcurrencyByMaxByServer_2()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_InsertForThreadedTest);
            init.CopyDbConfigFile4ToTestPath();

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional", "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0",
                "--concurrency", "2",
                "--concurrencytype", "MaxPerServer"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)SqlBuildManager.Interfaces.Console.ExecutionReturn.Successful;
            int actual = await target.ExecuteAsync();

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                Assert.AreEqual(expected, actual);
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string[] executionLogFile = ReadLines(Path.Combine(loggingPath, "SqlBuildManager.ThreadedExecution.log")).ToArray();

                // MaxPerServer=2 with single server: first two queue then start
                Assert.IsTrue(executionLogFile[2].IndexOf("sbm_pg_test: Queuing up thread") > -1, $"Expected first queue at line 2, got: {executionLogFile[2]}");
                Assert.IsTrue(executionLogFile[3].IndexOf("sbm_pg_test: Starting up thread") > -1, $"Expected first start at line 3, got: {executionLogFile[3]}");
                Assert.IsTrue(executionLogFile[4].IndexOf("Queuing up thread") > -1, $"Expected second queue at line 4, got: {executionLogFile[4]}");
            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName)) File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Commit without using retries - verifies log contains proper commit messages
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_ExecuteTest_CommitWithoutUsingRetries()
        {
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_InsertForThreadedTest);

            string cfgContents = $"localhost:sbm_pg_test,sbm_pg_test\nlocalhost:sbm_pg_test,sbm_pg_test1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional", "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)SqlBuildManager.Interfaces.Console.ExecutionReturn.Successful;
            int actual = await target.ExecuteAsync();

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                Assert.AreEqual(expected, actual);
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();

                // sbm_pg_test should commit 
                string[] logFiles = Directory.GetFiles(Path.Combine(loggingPath, "working", "localhost", "sbm_pg_test"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find sbm_pg_test log file");
                string logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("COMMIT") > -1, "sbm_pg_test log does not contain a 'COMMIT' message");
                Assert.IsTrue(logContents.IndexOf("ROLLBACK") == -1, "sbm_pg_test contains a 'ROLLBACK' message");

                // sbm_pg_test1 should commit
                logFiles = Directory.GetFiles(Path.Combine(loggingPath, "working", "localhost", "sbm_pg_test1"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find sbm_pg_test1 log file");
                logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("COMMIT") > -1, "sbm_pg_test1 log does not contain a 'COMMIT' message");
            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName)) File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Non-transactional threaded execution succeeds without retries
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_ExecuteTest_SuccessWithoutTransactionsNoRetries()
        {
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_InsertForThreadedTest);

            string cfgContents = $"localhost:sbm_pg_test,sbm_pg_test\nlocalhost:sbm_pg_test,sbm_pg_test1";
            string multiDbOverrideSettingFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, cfgContents);

            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "--loglevel", "debug",
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional", "false",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)SqlBuildManager.Interfaces.Console.ExecutionReturn.Successful;
            int actual = await target.ExecuteAsync();

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                Assert.AreEqual(expected, actual, "Run result was not Successful");
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();

                // sbm_pg_test should not have COMMIT/ROLLBACK (non-transactional)
                string[] logFiles = Directory.GetFiles(Path.Combine(loggingPath, "working", "localhost", "sbm_pg_test"), "*.log");
                Assert.AreEqual(1, logFiles.Length, "Unable to find sbm_pg_test log file");
                string logContents = File.ReadAllText(logFiles[0]);
                Assert.IsTrue(logContents.IndexOf("Completed: No Transaction Set") > -1, "sbm_pg_test does not contain 'Completed: No Transaction Set' message");
            }
            finally
            {
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName)) File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Non-transactional with retries should fail with BadRetryCountAndTransactionalCombo
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_ExecuteTest_NonTransactionalWithRetriesArgsFailure()
        {
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllText(sbmFileName, "");
            string multiDbOverrideSettingFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".cfg";
            File.WriteAllText(multiDbOverrideSettingFileName, "");
            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "threaded", "run",
                "--rootloggingpath", loggingPath,
                "--transactional", "false",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename", sbmFileName,
                "--timeoutretrycount", "20"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            ThreadedManager target = new ThreadedManager(cmdLine);
            int expected = (int)SqlBuildManager.Interfaces.Console.ExecutionReturn.BadRetryCountAndTransactionalCombo;
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
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (File.Exists(multiDbOverrideSettingFileName)) File.Delete(multiDbOverrideSettingFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }
    }
}
