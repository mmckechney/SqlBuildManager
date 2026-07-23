using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ExternalTest;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.MySQL.ExternalTest
{
    /// <summary>
    /// ACI integration tests for MySQL targets.
    /// Requires Azure environment provisioned via azd up with deployMySQL=true.
    /// </summary>
    [TestClass]
    public class AciTests
    {
        public TestContext TestContext { get; set; }

        private string settingsFileKeyPath = string.Empty;
        private AciTestResourceTracker aciResources = null!;
        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();

        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<AciTests>("SqlBuildManager.Console.log", Path.GetTempPath());
            settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");
            aciResources = new AciTestResourceTracker(settingsFileKeyPath);

            System.Console.SetOut(new StringWriter(ConsoleOutput));
            ConsoleOutput.Clear();
        }

        [TestCleanup]
        public async Task CleanUp()
        {
            await aciResources.CleanupAsync();
        }

        [DataRow("TestConfig/settingsfile-aci-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public async Task ACI_Queue_Run_PG_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/mysql-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("MySQL database targets config file not found. Run azd up with deployMySQL=true first.");
                }

                var sbmFileName = MySqlTestHelper.GetMySqlSimpleSelectSbm();
                int startingLine = MySqlTestHelper.LogFileCurrentLineCount();

                var rootCommand = CommandLineBuilder.SetUp();
                string jobName = aciResources.Track(MySqlTestHelper.GetUniqueJobName("aci-pg"), settingsFile);
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                var args = new string[]{
                    "--loglevel", "debug",
                    "aci", "run",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--aciname", jobName,
                    "--packagename", sbmFileName,
                    "--override", overrideFile,
                    "--platform", "MySQL",
                    "--concurrencytype", concurrencyType.ToString(),
                    "--containercount", containerCount.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream",
                    "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString()
                };

                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                // Validate blob storage logs agree with ACI PG test result
                var logFileContents = MySqlTestHelper.RelevantLogFileContents(startingLine);
                var combinedLog = logFileContents + Environment.NewLine + ConsoleOutput.ToString();
                WriteCommandExecutionLog();
                BlobLogValidator.AssertBlobContainerNameInLog(combinedLog, jobName, TestContext);

                var (storageAcct, storageKey) = BlobLogValidator.GetStorageCredentials(settingsFile, settingsFileKeyPath);
                var dbCount = File.ReadAllLines(overrideFile).Where(l => !string.IsNullOrWhiteSpace(l)).Count();
                var blobValidator = new BlobLogValidator(storageAcct, storageKey, jobName);
                await blobValidator.LoadLogsAsync();
                blobValidator.AssertBuildSuccess(dbCount, TestContext);
            }
            finally
            {
                WriteRemainingCommandOutput();
            }
        }

        [DataRow("TestConfig/settingsfile-aci-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public async Task ACI_Queue_PG_SBMSource_ManagedIdentity_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/mysql-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("MySQL database targets config file not found. Run azd up with deployMySQL=true first.");
                }

                var sbmFileName = MySqlTestHelper.GetMySqlSimpleSelectSbm();
                int startingLine = MySqlTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = aciResources.Track(MySqlTestHelper.GetUniqueJobName("aci-pg"), settingsFile);
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                // Prep
                var args = new string[]{
                    "aci", "prep",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--packagename", sbmFileName,
                    "--override", overrideFile,
                    "--platform", "MySQL"
                };

                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                // Enqueue
                args = new string[]{
                    "aci", "enqueue",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--concurrencytype", concurrencyType.ToString(),
                    "--override", overrideFile
                };
                val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                // Deploy + Monitor
                args = new string[]{
                    "--loglevel", "debug",
                    "aci", "deploy",
                    "--settingsfile", settingsFile,
                    "--packagename", sbmFileName,
                    "--jobname", jobName,
                    "--aciname", jobName,
                    "--containercount", containerCount.ToString(),
                    "--concurrencytype", concurrencyType.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--override", overrideFile,
                    "--platform", "MySQL",
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream",
                    "--eventhublogging", EventHubLogging.ConsolidatedScriptResults.ToString()
                };
                val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                // Validate blob storage logs
                var logFileContents = MySqlTestHelper.RelevantLogFileContents(startingLine);
                var combinedLog = logFileContents + Environment.NewLine + ConsoleOutput.ToString();
                WriteCommandExecutionLog();
                BlobLogValidator.AssertBlobContainerNameInLog(combinedLog, jobName, TestContext);

                var dbCount = File.ReadAllLines(overrideFile).Where(l => !string.IsNullOrWhiteSpace(l)).Count();
                var (storageAcct, storageKey) = BlobLogValidator.GetStorageCredentials(settingsFile, settingsFileKeyPath);
                var blobValidator = new BlobLogValidator(storageAcct, storageKey, jobName);
                await blobValidator.LoadLogsAsync();
                blobValidator.AssertBuildSuccess(dbCount, TestContext);
            }
            finally
            {
                WriteRemainingCommandOutput();
            }
        }

        [DataRow("TestConfig/settingsfile-aci-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public async Task ACI_Queue_PG_DoubleDbConfig_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/mysql-clientdbtargets-doubledb.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("MySQL double-client database targets config file not found.");
                }

                var sbmFileName = MySqlTestHelper.GetMySqlSimpleSelectDoubleClientSbm();
                int startingLine = MySqlTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = aciResources.Track(MySqlTestHelper.GetUniqueJobName("aci-pg"), settingsFile);
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                // Prep
                var args = new string[]{
                    "aci", "prep",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--packagename", sbmFileName
                };

                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                // Enqueue
                args = new string[]{
                    "aci", "enqueue",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--concurrencytype", concurrencyType.ToString(),
                    "--override", overrideFile
                };
                val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                // Deploy + Monitor
                args = new string[]{
                    "--loglevel", "debug",
                    "aci", "deploy",
                    "--settingsfile", settingsFile,
                    "--packagename", sbmFileName,
                    "--jobname", jobName,
                    "--aciname", jobName,
                    "--containercount", containerCount.ToString(),
                    "--concurrencytype", concurrencyType.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--override", overrideFile,
                    "--platform", "MySQL",
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream"
                };
                val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));

                // Validate blob storage logs
                var logFileContents = MySqlTestHelper.RelevantLogFileContents(startingLine);
                var combinedLog = logFileContents + Environment.NewLine + ConsoleOutput.ToString();
                WriteCommandExecutionLog();
                BlobLogValidator.AssertBlobContainerNameInLog(combinedLog, jobName, TestContext);

                var (storageAcct, storageKey) = BlobLogValidator.GetStorageCredentials(settingsFile, settingsFileKeyPath);
                var blobValidator = new BlobLogValidator(storageAcct, storageKey, jobName);
                await blobValidator.LoadLogsAsync();
                blobValidator.AssertBuildSuccess(dbCount, TestContext);
            }
            finally
            {
                WriteRemainingCommandOutput();
            }
        }

        [DataRow("TestConfig/settingsfile-aci-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public async Task ACI_Queue_PG_Query_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            string outputFile = Path.GetFullPath($"{Guid.NewGuid().ToString()}.csv");
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/mysql-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("MySQL database targets config file not found.");
                }

                var queryFile = MySqlTestHelper.GetMySqlSelectQueryFile();
                int startingLine = MySqlTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = aciResources.Track(MySqlTestHelper.GetUniqueJobName("aci-pg"), settingsFile);

                var args = new string[]{
                    "--loglevel", "debug",
                    "aci", "query",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--aciname", jobName,
                    "--override", overrideFile,
                    "--outputfile", outputFile,
                    "--queryfile", queryFile,
                    "--platform", "MySQL",
                    "--concurrencytype", concurrencyType.ToString(),
                    "--containercount", containerCount.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream"
                };

                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                Assert.IsTrue(File.Exists(outputFile), "The output file should exist");
                var outputLength = File.ReadAllLines(outputFile).Length;
                var overrideLength = File.ReadAllLines(overrideFile).Length;
                Assert.IsTrue(outputLength > overrideLength, "There should be more lines in the output than were in the override");

                // Validate blob storage logs
                var logFileContents = MySqlTestHelper.RelevantLogFileContents(startingLine);
                var combinedLog = logFileContents + Environment.NewLine + ConsoleOutput.ToString();
                WriteCommandExecutionLog();
                BlobLogValidator.AssertBlobContainerNameInLog(combinedLog, jobName, TestContext);

                var (storageAcct, storageKey) = BlobLogValidator.GetStorageCredentials(settingsFile, settingsFileKeyPath);
                var blobValidator = new BlobLogValidator(storageAcct, storageKey, jobName);
                await blobValidator.LoadLogsAsync();
                blobValidator.AssertQuerySuccess(TestContext);
            }
            finally
            {
                WriteRemainingCommandOutput();
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }
            }
        }

        private void WriteCommandExecutionLog()
        {
            TestContext.WriteLine("--- Command Execution Log ---");
            TestContext.WriteLine(ConsoleOutput.ToString());
            ConsoleOutput.Clear();
        }

        private void WriteRemainingCommandOutput()
        {
            if (ConsoleOutput.Length > 0)
            {
                TestContext.WriteLine(ConsoleOutput.ToString());
                ConsoleOutput.Clear();
            }
        }
    }
}
