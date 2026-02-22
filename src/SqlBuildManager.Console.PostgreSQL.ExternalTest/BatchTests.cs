using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.PostgreSQL.ExternalTest
{
    /// <summary>
    /// Azure Batch integration tests for PostgreSQL targets.
    /// Requires Azure environment provisioned via azd up with deployPostgreSQL=true.
    /// Note: DACPAC/Platinum tests are not applicable to PostgreSQL.
    /// </summary>
    [TestClass]
    public class BatchTests
    {
        public TestContext TestContext { get; set; }

        private CommandLineArgs cmdLine = null!;
        private List<string> overrideFileContents = null!;
        private string overrideFilePath = string.Empty;
        private string settingsFilePath = string.Empty;
        private string settingsFileKeyPath = string.Empty;

        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();
        private TextWriter originalConsoleOut = null!;

        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<BatchTests>("SqlBuildManager.Console.log", Path.GetTempPath());
            settingsFilePath = Path.GetFullPath("TestConfig/settingsfile-batch-linux-mi-only.json");
            settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");
            overrideFilePath = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");

            if (!File.Exists(overrideFilePath))
            {
                Assert.Inconclusive("PostgreSQL database targets config file not found. Run azd up with deployPostgreSQL=true first.");
            }

            cmdLine = new CommandLineArgs();
            cmdLine.FileInfoSettingsFile = new FileInfo(settingsFilePath);
            cmdLine.SettingsFileKey = settingsFileKeyPath;
            bool ds;
            (ds, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            overrideFileContents = File.ReadAllLines(overrideFilePath).Where(l => !string.IsNullOrWhiteSpace(l)).ToList();

            Aad.AadHelper.ManagedIdentityClientId = string.Empty;
            Aad.AadHelper.TenantId = string.Empty;

            originalConsoleOut = System.Console.Out;
            ConsoleOutput.Clear();
            System.Console.SetOut(new StringWriter(ConsoleOutput));
        }

        [TestCleanup]
        public void CleanUp()
        {
            System.Console.SetOut(originalConsoleOut);
            TestContext.WriteLine(ConsoleOutput.ToString());
        }

        #region Helpers

        string StandardExecutionErrorMessage(string logContents)
        {
            return logContents + Environment.NewLine +
                $"Please check the {cmdLine.RootLoggingPath}\\SqlBuildManager.Console.Execution.log for details.\r\n" +
                "You may need to add a PostgreSQL firewall rule or run azd up with deployPostgreSQL=true.";
        }

        private static string GetUniqueBatchJobName(string prefix)
        {
            string name = prefix + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-" + Guid.NewGuid().ToString().ToLower().Replace("-", "").Substring(0, 3);
            return name;
        }

        private string CombinedLogAndConsoleOutput(int startingLine)
        {
            return PgTestHelper.RelevantLogFileContents(startingLine) + Environment.NewLine + ConsoleOutput.ToString();
        }

        #endregion

        [DataRow("run", "TestConfig/settingsfile-batch-linux-mi-only.json", ConcurrencyType.Count, 10)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-mi-only.json", ConcurrencyType.Server, 2)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-mi-only.json", ConcurrencyType.MaxPerServer, 2)]
        [TestMethod]
        public void Batch_PG_Override_SBMSource_ByConcurrencyType_Success(string batchMethod, string settingsFile, ConcurrencyType concurType, int concurrency)
        {
            string sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
            settingsFile = Path.GetFullPath(settingsFile);
            string jobName = GetUniqueBatchJobName("batch-pg");
            int startingLine = PgTestHelper.LogFileCurrentLineCount();

            var args = new string[]{
                "--loglevel", "Debug",
                "batch", batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--override", overrideFilePath,
                "--packagename", sbmFileName,
                "--platform", "PostgreSQL",
                "--concurrency", concurrency.ToString(),
                "--concurrencytype", concurType.ToString(),
                "--jobname", jobName,
                "--unittest",
                "--monitor",
                "--stream",
                "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString()
            };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.Parse(args).InvokeAsync();
            val.Wait();
            var result = val.Result;

            var logFileContents = CombinedLogAndConsoleOutput(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "Batch PG run should have completed successfully");
            Assert.IsTrue(logFileContents.Contains("Batch complete"), "Should indicate a batch job");
        }

        [DataRow("run", "TestConfig/settingsfile-batch-linux-mi-only.json", ConcurrencyType.Count, 10)]
        [TestMethod]
        public void Batch_PG_Override_SBMSource_ManagedIdentity_Success(string batchMethod, string settingsFile, ConcurrencyType concurType, int concurrency)
        {
            string sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
            settingsFile = Path.GetFullPath(settingsFile);
            string jobName = GetUniqueBatchJobName("batch-pg-mi");
            int startingLine = PgTestHelper.LogFileCurrentLineCount();

            var args = new string[]{
                "--loglevel", "Debug",
                "batch", batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--override", overrideFilePath,
                "--packagename", sbmFileName,
                "--platform", "PostgreSQL",
                "--concurrency", concurrency.ToString(),
                "--concurrencytype", concurType.ToString(),
                "--jobname", jobName,
                "--unittest",
                "--monitor",
                "--stream",
                "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString()
            };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.Parse(args).InvokeAsync();
            val.Wait();
            var result = val.Result;

            var logFileContents = CombinedLogAndConsoleOutput(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "Batch PG MI run should have completed successfully");
        }

        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue-mi-only.json", ConcurrencyType.Count, 10)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue-mi-only.json", ConcurrencyType.Server, 2)]
        [TestMethod]
        public void Batch_PG_Queue_SBMSource_ByConcurrencyType_Success(string batchMethod, string settingsFile, ConcurrencyType concurType, int concurrency)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            string sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
            string jobName = GetUniqueBatchJobName("batch-pg-q");
            int startingLine = PgTestHelper.LogFileCurrentLineCount();

            // Enqueue
            var args = new string[]{
                "batch", "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--override", overrideFilePath,
                "--concurrencytype", concurType.ToString(),
                "--jobname", jobName
            };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            Task<int> val = rootCommand.Parse(args).InvokeAsync();
            val.Wait();
            var result = val.Result;

            var logFileContents = CombinedLogAndConsoleOutput(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

            // Run
            args = new string[]{
                "--loglevel", "debug",
                "batch", batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--override", overrideFilePath,
                "--packagename", sbmFileName,
                "--platform", "PostgreSQL",
                "--concurrencytype", concurType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--jobname", jobName,
                "--unittest",
                "--monitor",
                "--stream"
            };

            val = rootCommand.Parse(args).InvokeAsync();
            val.Wait();
            result = val.Result;

            logFileContents = CombinedLogAndConsoleOutput(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
        }

        [DataRow("query", "TestConfig/settingsfile-batch-linux-mi-only.json")]
        [TestMethod]
        public void Batch_PG_Query_Override_SelectSuccess(string batchMethod, string settingsFile)
        {
            string outputFile = Path.GetFullPath($"{Guid.NewGuid()}.csv");
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var queryFile = PgTestHelper.GetPgSelectQueryFile();
                string jobName = GetUniqueBatchJobName("batch-pg-qry");
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                var args = new string[]{
                    "--loglevel", "Debug",
                    "batch", batchMethod,
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "--override", overrideFilePath,
                    "--queryfile", queryFile,
                    "--outputfile", outputFile,
                    "--platform", "PostgreSQL",
                    "--concurrencytype", ConcurrencyType.Count.ToString(),
                    "--concurrency", "10",
                    "--jobname", jobName,
                    "--unittest",
                    "--monitor",
                    "--stream"
                };

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                var result = val.Result;

                var logFileContents = CombinedLogAndConsoleOutput(startingLine);
                Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

                Assert.IsTrue(File.Exists(outputFile), "The output file should exist");
                var outputLength = File.ReadAllLines(outputFile).Length;
                Assert.IsTrue(outputLength > overrideFileContents.Count, "There should be more lines in the output than targets");
            }
            finally
            {
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }
            }
        }

        [DataRow("run", "TestConfig/settingsfile-batch-linux-mi-only.json")]
        [TestMethod]
        public void Batch_PG_Override_SBMSource_RunWithError_MissingPackage(string batchMethod, string settingsFile)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            string jobName = GetUniqueBatchJobName("batch-pg-err");
            int startingLine = PgTestHelper.LogFileCurrentLineCount();

            var args = new string[]{
                "--loglevel", "Debug",
                "batch", batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--override", overrideFilePath,
                "--packagename", "doesnotexist.sbm",
                "--platform", "PostgreSQL",
                "--concurrency", "10",
                "--concurrencytype", ConcurrencyType.Count.ToString(),
                "--jobname", jobName
            };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.Parse(args).InvokeAsync();
            val.Wait();
            var result = val.Result;

            Assert.IsTrue(result != 0);
        }

        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue-mi-only.json", ConcurrencyType.Count, 10)]
        [TestMethod]
        public void Batch_PG_Queue_SBMSource_ManagedIdentity_Success(string batchMethod, string settingsFile, ConcurrencyType concurType, int concurrency)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            string sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
            string jobName = GetUniqueBatchJobName("batch-pg-mi-q");
            int startingLine = PgTestHelper.LogFileCurrentLineCount();

            // Enqueue
            var args = new string[]{
                "batch", "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--override", overrideFilePath,
                "--concurrencytype", concurType.ToString(),
                "--jobname", jobName
            };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            Task<int> val = rootCommand.Parse(args).InvokeAsync();
            val.Wait();
            var result = val.Result;

            var logFileContents = CombinedLogAndConsoleOutput(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

            // Run
            args = new string[]{
                "--loglevel", "debug",
                "batch", batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--override", overrideFilePath,
                "--packagename", sbmFileName,
                "--platform", "PostgreSQL",
                "--concurrencytype", concurType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--jobname", jobName,
                "--unittest",
                "--monitor",
                "--stream",
                "--eventhublogging", EventHubLogging.ConsolidatedScriptResults.ToString()
            };

            val = rootCommand.Parse(args).InvokeAsync();
            val.Wait();
            result = val.Result;

            logFileContents = CombinedLogAndConsoleOutput(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
        }
    }
}
