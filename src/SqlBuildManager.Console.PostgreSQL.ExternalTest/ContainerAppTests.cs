using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlBuildManager.Console.PostgreSQL.ExternalTest
{
    /// <summary>
    /// Azure Container App integration tests for PostgreSQL targets.
    /// Requires Azure environment provisioned via azd up with deployPostgreSQL=true and Container Apps deployed.
    /// Note: DACPAC/Platinum tests are not applicable to PostgreSQL.
    /// </summary>
    [TestClass]
    public class ContainerAppTests
    {
        public TestContext TestContext { get; set; }

        private string settingsFileKeyPath;
        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();

        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<ContainerAppTests>("SqlBuildManager.Console.log", Path.GetTempPath());
            settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");

            System.Console.SetOut(new StringWriter(ConsoleOutput));
            ConsoleOutput.Clear();
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [DataRow("TestConfig/settingsfile-containerapp-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.MaxPerServer)]
        [TestMethod]
        public void ContainerApp_PG_Run_Queue_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL database targets config file not found.");
                }

                var sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = PgTestHelper.GetUniqueJobName("ca-pg");

                var args = new string[]{
                    "--loglevel", "Debug",
                    "containerapp", "run",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "-P", sbmFileName,
                    "--override", overrideFile,
                    "--jobname", jobName,
                    "--platform", "PostgreSQL",
                    "--concurrencytype", concurrencyType.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--maxcontainers", containerCount.ToString(),
                    "--imagetag", imageTag,
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream", "true",
                    "--deletewhendone", "true"
                };

                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));
            }
            finally
            {
                TestContext.WriteLine(ConsoleOutput.ToString());
            }
        }

        [DataRow("TestConfig/settingsfile-containerapp-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public void ContainerApp_PG_StepWise_Queue_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL database targets config file not found.");
                }

                var sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = PgTestHelper.GetUniqueJobName("ca-pg");

                // Prep
                var args = new string[]{
                    "containerapp", "prep",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "--jobname", jobName,
                    "--packagename", sbmFileName
                };

                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                // Enqueue
                args = new string[]{
                    "containerapp", "enqueue",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
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
                    "containerapp", "deploy",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "-P", sbmFileName,
                    "--override", overrideFile,
                    "--jobname", jobName,
                    "--platform", "PostgreSQL",
                    "--concurrencytype", concurrencyType.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--maxcontainers", containerCount.ToString(),
                    "--imagetag", imageTag,
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream", "true",
                    "--deletewhendone", "true"
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

        [DataRow("TestConfig/settingsfile-containerapp-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public void ContainerApp_PG_Queue_ManagedIdentity_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL database targets config file not found.");
                }

                var sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = PgTestHelper.GetUniqueJobName("ca-pg");

                // Prep
                var args = new string[]{
                    "containerapp", "prep",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "--jobname", jobName,
                    "--packagename", sbmFileName
                };

                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                // Enqueue
                args = new string[]{
                    "containerapp", "enqueue",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
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
                    "--loglevel", "Debug",
                    "containerapp", "deploy",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "-P", sbmFileName,
                    "--override", overrideFile,
                    "--jobname", jobName,
                    "--platform", "PostgreSQL",
                    "--concurrencytype", concurrencyType.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--maxcontainers", containerCount.ToString(),
                    "--imagetag", imageTag,
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream", "true",
                    "--deletewhendone", "false"
                };
                val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                result = val.Result;
                TestContext.WriteLine(ConsoleOutput.ToString());
                Assert.AreEqual(0, result);

                var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));
            }
            finally
            {
                TestContext.WriteLine(ConsoleOutput.ToString());
            }
        }

        [DataRow("TestConfig/settingsfile-containerapp-mi-only.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [TestMethod]
        public void ContainerApp_PG_Run_DoubleDbConfig_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
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
                string jobName = PgTestHelper.GetUniqueJobName("ca-pg");

                var args = new string[]{
                    "--loglevel", "Debug",
                    "containerapp", "run",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "-P", sbmFileName,
                    "--override", overrideFile,
                    "--jobname", jobName,
                    "--platform", "PostgreSQL",
                    "--concurrencytype", concurrencyType.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--maxcontainers", containerCount.ToString(),
                    "--imagetag", imageTag,
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream", "true",
                    "--deletewhendone", "true"
                };

                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));
            }
            finally
            {
                TestContext.WriteLine(ConsoleOutput.ToString());
            }
        }
    }
}
