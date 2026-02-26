using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ExternalTest;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlBuildManager.Console.PostgreSQL.ExternalTest
{
    /// <summary>
    /// ACI integration tests for PostgreSQL targets.
    /// Requires Azure environment provisioned via azd up with deployPostgreSQL=true.
    /// </summary>
    [TestClass]
    public class AciTests
    {
        public TestContext TestContext { get; set; }

        private string settingsFileKeyPath = string.Empty;
        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();

        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<AciTests>("SqlBuildManager.Console.log", Path.GetTempPath());
            settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");

            System.Console.SetOut(new StringWriter(ConsoleOutput));
            ConsoleOutput.Clear();
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [DataRow("TestConfig/settingsfile-aci-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public void ACI_Queue_Run_PG_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL database targets config file not found. Run azd up with deployPostgreSQL=true first.");
                }

                var sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                var rootCommand = CommandLineBuilder.SetUp();
                string jobName = PgTestHelper.GetUniqueJobName("aci-pg");
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                var args = new string[]{
                    "--loglevel", "debug",
                    "aci", "run",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--packagename", sbmFileName,
                    "--override", overrideFile,
                    "--platform", "PostgreSQL",
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
                var logFileContents = PgTestHelper.RelevantLogFileContents(startingLine);
                var combinedLog = logFileContents + Environment.NewLine + ConsoleOutput.ToString();
                BlobLogValidator.AssertBlobContainerNameInLog(combinedLog, jobName, TestContext);

                var (storageAcct, storageKey) = BlobLogValidator.GetStorageCredentials(settingsFile, settingsFileKeyPath);
                var dbCount = File.ReadAllLines(overrideFile).Where(l => !string.IsNullOrWhiteSpace(l)).Count();
                var blobValidator = new BlobLogValidator(storageAcct, storageKey, jobName);
                blobValidator.LoadLogsAsync().Wait();
                blobValidator.AssertBuildSuccess(dbCount, TestContext);
            }
            finally
            {
                TestContext.WriteLine(ConsoleOutput.ToString());
            }
        }

        [DataRow("TestConfig/settingsfile-aci-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public void ACI_Queue_PG_SBMSource_ManagedIdentity_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL database targets config file not found. Run azd up with deployPostgreSQL=true first.");
                }

                var sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = PgTestHelper.GetUniqueJobName("aci-pg");
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                // Prep
                var args = new string[]{
                    "aci", "prep",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--packagename", sbmFileName,
                    "--override", overrideFile,
                    "--platform", "PostgreSQL"
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
                    "--containercount", containerCount.ToString(),
                    "--concurrencytype", concurrencyType.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--override", overrideFile,
                    "--platform", "PostgreSQL",
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream",
                    "--eventhublogging", EventHubLogging.ConsolidatedScriptResults.ToString()
                };
                val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);
            }
            finally
            {
                TestContext.WriteLine(ConsoleOutput.ToString());
            }
        }

        [DataRow("TestConfig/settingsfile-aci-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public void ACI_Queue_PG_DoubleDbConfig_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-clientdbtargets-doubledb.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL double-client database targets config file not found.");
                }

                var sbmFileName = PgTestHelper.GetPgSimpleSelectDoubleClientSbm();
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = PgTestHelper.GetUniqueJobName("aci-pg");
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
                    "--containercount", containerCount.ToString(),
                    "--concurrencytype", concurrencyType.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--override", overrideFile,
                    "--platform", "PostgreSQL",
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
            }
            finally
            {
                TestContext.WriteLine(ConsoleOutput.ToString());
            }
        }

        [DataRow("TestConfig/settingsfile-aci-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public void ACI_Queue_PG_Query_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            string outputFile = Path.GetFullPath($"{Guid.NewGuid().ToString()}.csv");
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL database targets config file not found.");
                }

                var queryFile = PgTestHelper.GetPgSelectQueryFile();
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = PgTestHelper.GetUniqueJobName("aci-pg");

                var args = new string[]{
                    "--loglevel", "debug",
                    "aci", "query",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--override", overrideFile,
                    "--outputfile", outputFile,
                    "--queryfile", queryFile,
                    "--platform", "PostgreSQL",
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
            }
            finally
            {
                TestContext.WriteLine(ConsoleOutput.ToString());
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }
            }
        }
    }
}
