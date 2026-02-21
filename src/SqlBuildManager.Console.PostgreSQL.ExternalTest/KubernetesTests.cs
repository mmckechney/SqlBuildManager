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
    /// Kubernetes integration tests for PostgreSQL targets.
    /// Requires Azure environment provisioned via azd up with deployPostgreSQL=true and AKS deployed.
    /// Note: DACPAC/Platinum tests are not applicable to PostgreSQL.
    /// </summary>
    [TestClass]
    public class KubernetesTests
    {
        public TestContext TestContext { get; set; }

        private string settingsFileKeyPath = string.Empty;
        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();

        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<KubernetesTests>("SqlBuildManager.Console.log", Path.GetTempPath());
            settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");

            System.Console.SetOut(new StringWriter(ConsoleOutput));
            ConsoleOutput.Clear();
        }

        [TestCleanup]
        public void CleanUp()
        {
        }

        [DataRow("TestConfig/settingsfile-k8s-mi-only.json")]
        [TestMethod]
        public void Kubernetes_PG_Run_Queue_SBMSource_Success(string settingsFile)
        {
            try
            {
                var prc = new ProcessHelper();
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL database targets config file not found.");
                }

                var sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
                string jobName = PgTestHelper.GetUniqueJobName("k8s-pg");
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                // Clear any existing pods
                var result = prc.ExecuteProcess("kubectl", "delete job sqlbuildmanager ");

                var args = new string[]
                {
                    "--loglevel", "debug",
                    "k8s", "run",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "--jobname", jobName,
                    "--override", overrideFile,
                    "--packagename", sbmFileName,
                    "--platform", "PostgreSQL",
                    "--force",
                    "--unittest",
                    "--stream",
                    "--eventhublogging", EventHubLogging.ConsolidatedScriptResults.ToString()
                };

                var val = rootCommand.Parse(args).InvokeAsync();
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

        [DataRow("TestConfig/settingsfile-k8s-mi-only.json")]
        [TestMethod]
        public void Kubernetes_PG_Run_Queue_DoubleDbConfig_SBMSource_Success(string settingsFile)
        {
            try
            {
                var prc = new ProcessHelper();
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-clientdbtargets-doubledb.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL double-client database targets config file not found.");
                }

                var sbmFileName = PgTestHelper.GetPgSimpleSelectDoubleClientSbm();
                string jobName = PgTestHelper.GetUniqueJobName("k8s-pg");
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                // Clear any existing pods
                var result = prc.ExecuteProcess("kubectl", "delete job sqlbuildmanager ");

                var args = new string[]
                {
                    "k8s", "run",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "--jobname", jobName,
                    "--override", overrideFile,
                    "--packagename", sbmFileName,
                    "--platform", "PostgreSQL",
                    "--force",
                    "--unittest",
                    "--stream"
                };

                var val = rootCommand.Parse(args).InvokeAsync();
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

        [DataRow("TestConfig/settingsfile-k8s-mi-only.json", ConcurrencyType.Count, 5)]
        [DataRow("TestConfig/settingsfile-k8s-mi-only.json", ConcurrencyType.Server, 5)]
        [DataRow("TestConfig/settingsfile-k8s-mi-only.json", ConcurrencyType.MaxPerServer, 5)]
        [TestMethod]
        public void Kubernetes_PG_Run_Queue_Concurrency_SBMSource_Success(string settingsFile, ConcurrencyType concurType, int concurrencyCount)
        {
            try
            {
                var prc = new ProcessHelper();
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL database targets config file not found.");
                }

                var sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
                string jobName = PgTestHelper.GetUniqueJobName("k8s-pg");
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                // Clear any existing pods
                var result = prc.ExecuteProcess("kubectl", "delete job sqlbuildmanager ");

                var args = new string[]
                {
                    "--loglevel", "debug",
                    "k8s", "run",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "--jobname", jobName,
                    "--override", overrideFile,
                    "--packagename", sbmFileName,
                    "--platform", "PostgreSQL",
                    "--force",
                    "--unittest",
                    "--stream",
                    "--concurrencytype", concurType.ToString(),
                    "--concurrency", concurrencyCount.ToString()
                };

                var val = rootCommand.Parse(args).InvokeAsync();
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

        [DataRow("TestConfig/settingsfile-k8s-mi-only.json")]
        [TestMethod]
        public void Kubernetes_PG_Query_Queue_SBMSource_Success(string settingsFile)
        {
            string outputFile = Path.GetFullPath($"{Guid.NewGuid()}.csv");
            try
            {
                var prc = new ProcessHelper();
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");
                if (!File.Exists(overrideFile))
                {
                    Assert.Inconclusive("PostgreSQL database targets config file not found.");
                }

                var queryFile = PgTestHelper.GetPgSelectQueryFile();
                string jobName = PgTestHelper.GetUniqueJobName("k8s-pg");
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                // Clear any existing pods
                var result = prc.ExecuteProcess("kubectl", "delete job sqlbuildmanager ");

                var args = new string[]
                {
                    "--loglevel", "debug",
                    "k8s", "query",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "--jobname", jobName,
                    "--override", overrideFile,
                    "--queryfile", queryFile,
                    "--outputfile", outputFile,
                    "--platform", "PostgreSQL",
                    "--force",
                    "--unittest",
                    "--stream",
                    "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString()
                };

                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                result = val.Result;

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

        [DataRow("TestConfig/settingsfile-k8s-mi-only.json")]
        [TestMethod]
        public void Kubernetes_PG_Run_LongRunning_Queue_SBMSource_Success(string settingsFile)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            var overrideFile = Path.GetFullPath("TestConfig/pg-databasetargets.cfg");
            if (!File.Exists(overrideFile))
            {
                Assert.Inconclusive("PostgreSQL database targets config file not found.");
            }

            var tmpOverride = Path.Combine(Path.GetDirectoryName(overrideFile)!, Guid.NewGuid().ToString() + ".cfg");
            File.WriteAllLines(tmpOverride, File.ReadAllLines(overrideFile).Where(l => !string.IsNullOrWhiteSpace(l)).Take(6).ToArray());
            try
            {
                var prc = new ProcessHelper();
                var sbmFileName = PgTestHelper.GetPgSimpleSelectSbm();
                string jobName = PgTestHelper.GetUniqueJobName("k8s-pg-long");
                int startingLine = PgTestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                // Clear any existing pods
                var result = prc.ExecuteProcess("kubectl", "delete job sqlbuildmanager ");

                var args = new string[]
                {
                    "--loglevel", "debug",
                    "k8s", "run",
                    "--settingsfile", settingsFile,
                    "--settingsfilekey", settingsFileKeyPath,
                    "--jobname", jobName,
                    "--override", tmpOverride,
                    "--packagename", sbmFileName,
                    "--platform", "PostgreSQL",
                    "--force",
                    "--unittest",
                    "--stream",
                    "--eventhublogging", EventHubLogging.ConsolidatedScriptResults.ToString()
                };

                var val = rootCommand.Parse(args).InvokeAsync();
                val.Wait();
                result = val.Result;

                Assert.AreEqual(0, result);

                var dbCount = File.ReadAllText(tmpOverride).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));
            }
            finally
            {
                TestContext.WriteLine(ConsoleOutput.ToString());
            }
        }
    }
}
