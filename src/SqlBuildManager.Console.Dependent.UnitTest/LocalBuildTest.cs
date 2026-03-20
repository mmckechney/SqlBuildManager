using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Dependent.UnitTest
{
    /// <summary>
    ///This is a test class for LocalBuildTest and is intended
    ///to contain all LocalBuildTest Unit Tests
    ///</summary>
    [TestClass()]
    [DoNotParallelize]
    public class LocalBuildTest
    {


        public TestContext TestContext { get; set; }
        private static List<Initialization> initObjs = new List<Initialization>();
        private Initialization GetInitializationObject()
        {
            Initialization i = new Initialization();
            initObjs.Add(i);
            return i;
        }
        private static string GetLocalhostFolderName(string loggingRoot)
        {
            return SqlBuildManager.Test.Common.TestPathHelper.FindServerLogDirectory(loggingRoot, Initialization.TestServer);
        }
       
        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();
        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<LocalBuildTest>("SqlBuildManager.Console.log", Path.GetTempPath());

            System.Console.SetOut(new StringWriter(ConsoleOutput));    // Associate StringBuilder with StdOut
            ConsoleOutput.Clear();    // Clear text from any previous text runs
        }

        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            foreach (Initialization i in initObjs)
                i.Dispose();
        }


        [TestMethod()]
        public async Task LocalBuild_SingleDb_NoOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect_client);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
                "--server", Initialization.TestServer,
                "--database", "SqlBuildTest",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0"

            }).Concat(Initialization.GetAuthArgs()).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = await Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();


                var regex = new Regex("Batch logging 1 script");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Didn't find 1 script commit");

                regex = new Regex("Committing transaction for");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should have commits to 1 database");

                regex = new Regex("Prepared build for run");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should only be 1 run (1 Db's per run)");
            }

            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }

            }
        }

        [TestMethod()]
        public async Task LocalBuild_SingleDb_SingleOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect_client);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
                "--server", Initialization.TestServer,
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", "client,SqlBuildTest",
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0"

            }).Concat(Initialization.GetAuthArgs()).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = await Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                var regex = new Regex("Batch logging 1 script");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Didn't find 1 script commit");

                regex = new Regex("Committing transaction for");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should have commits to 1 database");

                regex = new Regex("Prepared build for run");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should only be 1 run (1 Db's per run)");
            }

            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }

            }
        }

        [TestMethod()]
        public async Task LocalBuild_SingleDb_NoOverrideSetting_NoDbTarget_Success()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
                "--server", Initialization.TestServer,
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0"

            }).Concat(Initialization.GetAuthArgs()).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = await Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                var regex = new Regex("Batch logging 1 script");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Didn't find 1 script commit");

                regex = new Regex("Committing transaction for");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should have commits to 1 database");

                regex = new Regex("Prepared build for run");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should only be 1 run (1 Db's per run)");
            }

            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
                try
                {
                    if (File.Exists(sbmFileName))
                        File.Delete(sbmFileName);
                    if (Directory.Exists(loggingPath))
                        Directory.Delete(loggingPath, true);
                }
                catch { }

            }
        }

        [TestMethod()]
        public async Task LocalBuild_MultiDb_DoubleOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect_DoubleClient);

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            init.CopyDoubleDbConfigFileToTestPath();


            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0"

            }).Concat(Initialization.GetAuthArgs()).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = await Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                //Should be all sequential!
                Assert.IsTrue(executionLogFile.Contains("Commit Successful"), "Didn't find successful commit message");
                Assert.IsTrue(executionLogFile.Contains("BUILD_COMMITTED"), "Didn't find build commited message");
                var regex = new Regex("Batch logging 2 script");
                Assert.AreEqual(5, regex.Matches(executionLogFile).Count(), "Didn't find 5 `Batch logging 2 script` scripts commits");

                regex = new Regex("Batch logging 1 script");
                Assert.AreEqual(5, regex.Matches(executionLogFile).Count(), "Didn't find 5 `Batch logging 1 script` scripts commits");


            }

            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
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
        public async Task LocalBuild_MultiDb_SingleOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            init.CopyDbConfigFileToTestPath();


            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0"

            }).Concat(Initialization.GetAuthArgs()).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = await Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                //Should be all sequential!
                Assert.IsTrue(executionLogFile.Contains("Commit Successful"), "Didn't find successful commit message");
                Assert.IsTrue(executionLogFile.Contains("BUILD_COMMITTED"), "Didn't find build commited message");

                var regex = new Regex("Batch logging 1 script");
                Assert.AreEqual(2, regex.Matches(executionLogFile).Count(), "Didn't find 2 scripts commits");

            }

            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
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
        [Ignore("The local machine name isn't resolving")]
        public async Task LocalBuild_MultiDb_SqlScriptOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);

            string multiDbOverrideSettingFileName = Initialization.SqlScriptOverrideFileName;
            init.CopySqlScriptOverrideFiletoTestPath();


            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = (new string[] {
                "build",
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0", 
                "--database", "master",
                "--server", Initialization.TestServer


            }).Concat(Initialization.GetAuthArgs()).ToArray();
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = await Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = ConsoleOutput.ToString();

                //Should be all sequential!
                Assert.IsTrue(executionLogFile.Contains("Commit Successful"), "Didn't find successful commit message");
                Assert.IsTrue(executionLogFile.Contains("Build Committed"), "Didn't find build commited message");

                var regex = new Regex("Recording Commited Script:");
                Assert.AreEqual(24, regex.Matches(executionLogFile).Count(), "Didn't find 24 scripts commits");

                Assert.IsTrue(executionLogFile.Contains("Build Committed"), "Didn't find commit message");

            }

            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
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
    }
}
