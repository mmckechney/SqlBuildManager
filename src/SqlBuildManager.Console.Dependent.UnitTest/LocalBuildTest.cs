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
using SqlBuildManager.Console.Threaded;
using Microsoft.Extensions.Logging;
using System.Text;
using System.Linq;
using SqlBuildManager.Console.CommandLine;
using SqlSync.Connection;
using Microsoft.Rest.TransientFaultHandling;
using System.Diagnostics;

namespace SqlBuildManager.Console.Dependent.UnitTest
{
    /// <summary>
    ///This is a test class for LocalBuildTest and is intended
    ///to contain all LocalBuildTest Unit Tests
    ///</summary>
    [TestClass()]
    public class LocalBuildTest
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

        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();
        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<LocalBuildTest>("SqlBuildManager.Console.log", @"C:\temp");

            System.Console.SetOut(new StringWriter(this.ConsoleOutput));    // Associate StringBuilder with StdOut
            this.ConsoleOutput.Clear();    // Clear text from any previous text runs
        }

        //Use ClassCleanup to run code after all tests in a class have run
        [ClassCleanup()]
        public static void MyClassCleanup()
        {
            foreach (Initialization i in initObjs)
                i.Dispose();
        }


        [TestMethod()]
        public void LocalBuild_SingleDb_NoOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect_client);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[] {
                "build",
                "--server", "localhost\\sqlexpress",
                "--database", "SqlBuildTest",
                "--authtype", AuthenticationType.Windows.ToString(),
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0"

            };
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = this.ConsoleOutput.ToString();


                var regex = new Regex("Recording Commited Script:");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Didn't find 1 script commit");

                regex = new Regex("Committing transaction for");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should have commits to 1 database");

                regex = new Regex("Prepared build for run");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should only be 1 run (1 Db's per run)");
            }

            finally
            {
                Debug.WriteLine(this.ConsoleOutput.ToString());
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
        public void LocalBuild_SingleDb_SingleOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect_client);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[] {
                "build",
                "--server", "localhost\\sqlexpress",
                "--authtype", AuthenticationType.Windows.ToString(),
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", "client,SqlBuildTest",
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0"

            };
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = this.ConsoleOutput.ToString();


                var regex = new Regex("Recording Commited Script:");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Didn't find 1 script commit");

                regex = new Regex("Committing transaction for");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should have commits to 1 database");

                regex = new Regex("Prepared build for run");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should only be 1 run (1 Db's per run)");
            }

            finally
            {
                Debug.WriteLine(this.ConsoleOutput.ToString());
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
        public void LocalBuild_SingleDb_NoOverrideSetting_NoDbTarget_Success()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);

            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[] {
                "build",
                "--server", "localhost\\sqlexpress",
                "--authtype", AuthenticationType.Windows.ToString(),
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0"

            };
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = this.ConsoleOutput.ToString();

                var regex = new Regex("Recording Commited Script:");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Didn't find 1 script commit");

                regex = new Regex("Committing transaction for");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should have commits to 1 database");

                regex = new Regex("Prepared build for run");
                Assert.AreEqual(1, regex.Matches(executionLogFile).Count(), "Should only be 1 run (1 Db's per run)");
            }

            finally
            {
                Debug.WriteLine(this.ConsoleOutput.ToString());
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
        public void LocalBuild_MultiDb_DoubleOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect_DoubleClient);

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            init.CopyDoubleDbConfigFileToTestPath();


            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[] {
                "build",
                "--authtype", AuthenticationType.Windows.ToString(),
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0"

            };
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = this.ConsoleOutput.ToString();

                //Should be all sequential!
                Assert.IsTrue(executionLogFile.Contains("Commit Successful"), "Didn't find successful commit message");
                Assert.IsTrue(executionLogFile.Contains("Build Committed"), "Didn't find build commited message");
                var regex = new Regex("Recording Commited Script:");
                Assert.AreEqual(10,regex.Matches(executionLogFile).Count(), "Didn't find 10 scripts commits");

                Assert.IsTrue(executionLogFile.Contains("Build Committed"), "Didn't find commit message");
            }

            finally
            {
                Debug.WriteLine(this.ConsoleOutput.ToString());
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
        public void LocalBuild_MultiDb_SingleOverrideSetting()
        {
            Initialization init = GetInitializationObject();
            string sbmFileName = Path.GetTempPath() + System.Guid.NewGuid().ToString() + ".sbm";
            File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);

            string multiDbOverrideSettingFileName = Initialization.DbConfigFileName;
            init.CopyDbConfigFileToTestPath();


            string loggingPath = Path.GetTempPath() + System.Guid.NewGuid().ToString();

            string[] args = new string[] {
                "build",
                "--authtype", AuthenticationType.Windows.ToString(),
                "--rootloggingpath", loggingPath,
                "--transactional" , "true",
                "--trial", "false",
                "--override", multiDbOverrideSettingFileName,
                "--packagename",  sbmFileName,
                "--timeoutretrycount", "0"

            };
            var cmdLine = CommandLineBuilder.ParseArguments(args);

            int actual = Worker.RunLocalBuildAsync(cmdLine);


            try
            {

                if (actual == -600)
                    Assert.Fail("Unable to completed test.");

                SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
                string executionLogFile = this.ConsoleOutput.ToString();

                //Should be all sequential!
                Assert.IsTrue(executionLogFile.Contains("Commit Successful"), "Didn't find successful commit message");
                Assert.IsTrue(executionLogFile.Contains("Build Committed"), "Didn't find build commited message");

               var regex = new Regex("Recording Commited Script:");
                Assert.AreEqual(2, regex.Matches(executionLogFile).Count(), "Didn't find 2 scripts commits");

                Assert.IsTrue(executionLogFile.Contains("Build Committed"), "Didn't find commit message");

            }

            finally
            {
                Debug.WriteLine(this.ConsoleOutput.ToString());
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
