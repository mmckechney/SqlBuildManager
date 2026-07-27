//using Microsoft.VisualStudio.TestTools.UnitTesting;
//using SqlBuildManager.Console.CommandLine;
//using System;
//using System.Collections.Generic;
//using System.CommandLine;
//using System.IO;
//using System.Linq;
//using System.Text;

//namespace SqlBuildManager.Console.MySQL.AzureTest
//{
//    /// <summary>
//    /// Local integration tests for MySQL targets.
//    /// Requires Azure MySQL Flexible Server provisioned via azd up.
//    /// Uses --platform MySQL and --authtype AzureADDefault for MI auth.
//    /// </summary>
//    [TestClass]
//    public class LocalTests
//    {
//        public TestContext TestContext { get; set; }

//        private CommandLineArgs cmdLine = null!;
//        private List<string> overrideFileContents = null!;
//        private string overrideFilePath = string.Empty;
//        private string settingsFilePath = string.Empty;
//        private string settingsFileKeyPath = string.Empty;

//        [TestInitialize]
//        public void ConfigureProcessInfo()
//        {
//            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<LocalTests>("SqlBuildManager.Console.log", Path.GetTempPath());

//            settingsFilePath = Path.GetFullPath("TestConfig/settingsfile-aci-mi-only.json");
//            settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");
//            overrideFilePath = Path.GetFullPath("TestConfig/mysql-databasetargets.cfg");

//            if (!File.Exists(overrideFilePath))
//            {
//                Assert.Inconclusive("MySQL database targets config file not found. Run azd up with deployMySQL=true first.");
//            }

//            cmdLine = new CommandLineArgs();
//            cmdLine.FileInfoSettingsFile = new FileInfo(settingsFilePath);
//            cmdLine.SettingsFileKey = settingsFileKeyPath;
//            bool ds;
//            (ds, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
//            overrideFileContents = File.ReadAllLines(overrideFilePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();
//        }

//        [TestCleanup]
//        public void CleanUp()
//        {
//        }

//        #region Helpers

//        string StandardExecutionErrorMessage(string logContents)
//        {
//            return logContents + Environment.NewLine +
//                $"Please check the {cmdLine.RootLoggingPath}\\SqlBuildManager.Console.Execution.log for details.\r\n" +
//                "You may need to add a MySQL firewall rule or run azd up with deployMySQL=true.";
//        }

//        #endregion

//        [TestMethod]
//        public void LocalThreaded_PG_SBMSource_Success()
//        {
//            string sbmFileName = MySqlTestHelper.GetMySqlSimpleSelectSbm();
//            int startingLine = MySqlTestHelper.LogFileCurrentLineCount();

//            var args = new string[]{
//                "threaded", "run",
//                "--override", overrideFilePath,
//                "--packagename", sbmFileName,
//                "--platform", "MySQL",
//                "--authtype", "AzureADDefault",
//                "--rootloggingpath", cmdLine.RootLoggingPath
//            };

//            RootCommand rootCommand = CommandLineBuilder.SetUp();
//            var val = rootCommand.Parse(args).InvokeAsync();
//            val.Wait();
//            var result = val.Result;

//            var logFileContents = MySqlTestHelper.RelevantLogFileContents(startingLine);
//            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
//            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "The threaded PG run should have completed successfully");
//            Assert.IsTrue(logFileContents.Contains($"Total number of targets: {overrideFileContents.Count}"), $"Should have run against {overrideFileContents.Count} databases");
//        }

//        [TestMethod]
//        public void LocalSingleRun_PG_SBMSource_Success()
//        {
//            string sbmFileName = MySqlTestHelper.GetMySqlSimpleSelectSbm();
//            int startingLine = MySqlTestHelper.LogFileCurrentLineCount();

//            var args = new string[]{
//                "build",
//                "--override", overrideFileContents[0].Split(":")[1],
//                "--packagename", sbmFileName,
//                "--platform", "MySQL",
//                "--authtype", "AzureADDefault",
//                "--rootloggingpath", cmdLine.RootLoggingPath,
//                "--server", overrideFileContents[0].Split(":")[0]
//            };

//            RootCommand rootCommand = CommandLineBuilder.SetUp();
//            var val = rootCommand.Parse(args).InvokeAsync();
//            val.Wait();
//            var result = val.Result;

//            var logFileContents = MySqlTestHelper.RelevantLogFileContents(startingLine);
//            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
//            Assert.IsTrue(logFileContents.Contains("Committing transaction for"), "The single PG build should have committed successfully");
//        }

//        [TestMethod]
//        public void LocalThreaded_PG_DoubleClient_SBMSource_Success()
//        {
//            var doubleClientOverridePath = Path.GetFullPath("TestConfig/mysql-clientdbtargets-doubledb.cfg");
//            if (!File.Exists(doubleClientOverridePath))
//            {
//                Assert.Inconclusive("MySQL double-client database targets config file not found.");
//            }

//            string sbmFileName = MySqlTestHelper.GetMySqlSimpleSelectDoubleClientSbm();
//            int startingLine = MySqlTestHelper.LogFileCurrentLineCount();

//            var overrides = File.ReadAllLines(doubleClientOverridePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

//            var args = new string[]{
//                "threaded", "run",
//                "--override", doubleClientOverridePath,
//                "--packagename", sbmFileName,
//                "--platform", "MySQL",
//                "--authtype", "AzureADDefault",
//                "--rootloggingpath", cmdLine.RootLoggingPath
//            };

//            RootCommand rootCommand = CommandLineBuilder.SetUp();
//            var val = rootCommand.Parse(args).InvokeAsync();
//            val.Wait();
//            var result = val.Result;

//            var logFileContents = MySqlTestHelper.RelevantLogFileContents(startingLine);
//            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
//            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "The double-client PG run should have completed successfully");
//        }

//        [TestMethod]
//        public void LocalThreaded_PG_SBMSource_Concurrency_MaxPerServer_Success()
//        {
//            string sbmFileName = MySqlTestHelper.GetMySqlSimpleSelectSbm();
//            int startingLine = MySqlTestHelper.LogFileCurrentLineCount();

//            var args = new string[]{
//                "threaded", "run",
//                "--override", overrideFilePath,
//                "--packagename", sbmFileName,
//                "--platform", "MySQL",
//                "--authtype", "AzureADDefault",
//                "--concurrencytype", ConcurrencyType.MaxPerServer.ToString(),
//                "--concurrency", "5",
//                "--rootloggingpath", cmdLine.RootLoggingPath
//            };

//            RootCommand rootCommand = CommandLineBuilder.SetUp();
//            var val = rootCommand.Parse(args).InvokeAsync();
//            val.Wait();
//            var result = val.Result;

//            var logFileContents = MySqlTestHelper.RelevantLogFileContents(startingLine);
//            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
//            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "MaxPerServer concurrency should have completed successfully");
//        }
//    }
//}
