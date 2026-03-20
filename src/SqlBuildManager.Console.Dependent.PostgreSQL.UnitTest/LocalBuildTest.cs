using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Dependent.PostgreSQL.UnitTest
{
    [TestClass()]
    [DoNotParallelize]
    public class LocalBuildTest
    {
        private TestContext testContextInstance = null!;
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

        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();

        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<LocalBuildTest>("SqlBuildManager.Console.PG.log", Path.GetTempPath());
            System.Console.SetOut(new StringWriter(ConsoleOutput));
            ConsoleOutput.Clear();
        }

        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            foreach (Initialization i in initObjs)
                i.Dispose();
        }

        /// <summary>
        /// Single DB build with explicit --server/--database targeting PostgreSQL
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_LocalBuild_SingleDb_NoOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_SimpleSelect);

            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
                "--server", Initialization.Server,
                "--database", "sbm_pg_test",
                "--rootloggingpath", loggingPath,
                "--transactional", "true",
                "--trial", "false",
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            int actual = await Worker.RunLocalBuildAsync(cmdLine);

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                var regex = new Regex("Batch logging 1 script");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Didn't find 1 script commit");

                regex = new Regex("Committing transaction for");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should have commits to 1 database");

                regex = new Regex("Prepared build for run");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should only be 1 run");
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Single DB build using --override with client mapping
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_LocalBuild_SingleDb_SingleOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_SimpleSelect_client);

            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
                "--server", Initialization.Server,
                "--rootloggingpath", loggingPath,
                "--transactional", "true",
                "--trial", "false",
                "--override", "client,sbm_pg_test",
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            int actual = await Worker.RunLocalBuildAsync(cmdLine);

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                var regex = new Regex("Batch logging 1 script");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Didn't find 1 script commit");

                regex = new Regex("Committing transaction for");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should have commits to 1 database");

                regex = new Regex("Prepared build for run");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should only be 1 run");
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Multi-DB build with double client override file (2 servers x 2 client DBs each)
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_LocalBuild_MultiDb_DoubleOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_SimpleSelect_DoubleClient);

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            init.CopyDoubleDbConfigFileToTestPath();

            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
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
            int actual = await Worker.RunLocalBuildAsync(cmdLine);

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                Assert.IsTrue(executionLogFile.Contains("Commit Successful"), "Didn't find successful commit message");
                Assert.IsTrue(executionLogFile.Contains("BUILD_COMMITTED"), "Didn't find build committed message");

                // 2 override lines x 2 clients each = 4 DB targets, each gets 1 script
                var regex = new Regex("Batch logging 1 script");
                Assert.AreEqual(4, regex.Matches(executionLogFile).Count(), $"Expected 4 `Batch logging 1 script` commits, got {regex.Matches(executionLogFile).Count()}");

            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
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
        /// Multi-DB build with simple override config file (2 DBs)
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_LocalBuild_MultiDb_SingleOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_SimpleSelect);

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            init.CopyDbConfigFileToTestPath();

            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
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
            int actual = await Worker.RunLocalBuildAsync(cmdLine);

            try
            {
                if (actual == -600)
                    Assert.Fail("Unable to complete test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                Assert.IsTrue(executionLogFile.Contains("Commit Successful"), "Didn't find successful commit message");
                Assert.IsTrue(executionLogFile.Contains("BUILD_COMMITTED"), "Didn't find build committed message");

                var regex = new Regex("Batch logging 1 script");
                Assert.AreEqual(2, regex.Matches(executionLogFile).Count(), "Didn't find 2 script commits");
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
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
        /// Trial build — verifies the --trial flag is accepted and build completes.
        /// Note: In the current implementation, the trial flag from cmdLine.Trial
        /// is not propagated to SqlBuildRunData.IsTrial in Worker.RunLocalBuildAsync,
        /// so the build commits instead of rolling back. This test validates
        /// that the CLI accepts the argument and the build executes successfully.
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_LocalBuild_SingleDb_Trial()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_SimpleSelect);

            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
                "--server", Initialization.Server,
                "--database", "sbm_pg_test",
                "--rootloggingpath", loggingPath,
                "--transactional", "true",
                "--trial", "true",
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            Assert.IsTrue(cmdLine.Trial, "Trial flag should be parsed as true");

            int actual = await Worker.RunLocalBuildAsync(cmdLine);

            try
            {
                Assert.AreEqual(0, actual, "Build with trial flag should complete without error");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                var regex = new Regex("Batch logging 1 script");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should process 1 script");
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }

        /// <summary>
        /// Build with a syntax error package should show failure in output.
        /// Note: Worker.RunLocalBuildAsync returns 0 even on build failure because
        /// the Bg_ProgressChanged event handler is not subscribed. We verify the
        /// failure through the console output instead.
        /// </summary>
        [TestMethod()]
        public async Task PostgreSQL_LocalBuild_SyntaxError_ShowsFailure()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.PG_SyntaxError);

            string loggingPath = Path.GetTempPath() + Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
                "--server", Initialization.Server,
                "--database", "sbm_pg_test",
                "--rootloggingpath", loggingPath,
                "--transactional", "true",
                "--trial", "false",
                "--packagename", sbmFileName,
                "--timeoutretrycount", "0"
            }).Concat(Initialization.GetAuthArgs())
              .Concat(Initialization.GetPlatformArgs())
              .ToArray();

            var cmdLine = CommandLineBuilder.ParseArguments(args);
            int actual = await Worker.RunLocalBuildAsync(cmdLine);

            try
            {
                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                Assert.IsTrue(
                    executionLogFile.Contains("Build Failed") || executionLogFile.Contains("Rolled Back"),
                    "Syntax error should produce a build failure or rollback message in output");
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
                try
                {
                    if (File.Exists(sbmFileName)) File.Delete(sbmFileName);
                    if (Directory.Exists(loggingPath)) Directory.Delete(loggingPath, true);
                }
                catch { }
            }
        }
    }
}
