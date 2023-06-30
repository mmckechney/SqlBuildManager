using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.ExternalTest
{
    /// <summary>
    /// To run these tests, you will need to have an Azure environment set up.
    /// You can easily do this by following the script instructions found in the /docs/localbuild.md file
    /// </summary>
    [TestClass]
    public class AciTests
    {

        private string settingsFileKeyPath;
        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();

        [TestInitialize]
        public void ConfigureProcessInfo()
        {

            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<AciTests>("SqlBuildManager.Console.log", @"C:\temp");
            settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");

            System.Console.SetOut(new StringWriter(ConsoleOutput));    // Associate StringBuilder with StdOut
            ConsoleOutput.Clear();    // Clear text from any previous text runs


        }
        [TestCleanup]
        public void CleanUp()
        {

        }

        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataRow("TestConfig/settingsfile-aci-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci-no-registry-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataTestMethod]
        public void ACI_Queue_Run_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
                var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
                if (!File.Exists(sbmFileName))
                {
                    File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
                }


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                var parser = CommandLineBuilder.GetCommandParser();
                string jobName = TestHelper.GetUniqueJobName("aci");
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                //Prep the build
                var args = new string[]{
                    "--loglevel", "debug",
                    "aci",  "run",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--packagename", sbmFileName,
                     "--override", overrideFile,
                    "--concurrencytype", concurrencyType.ToString(),
                    "--containercount", containerCount.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream",
                    "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString()
                };

                var val = parser.InvokeAsync(args);
                val.Wait();
                int result = val.Result;
               Assert.AreEqual(0, result);
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }


        }

        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataTestMethod]
        public void ACI_Queue_SBMSource_KeyVault_Secrets_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
                var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
                if (!File.Exists(sbmFileName))
                {
                    File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
                }


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = TestHelper.GetUniqueJobName("aci");
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                //Prep the build
                var args = new string[]{
                "aci",  "prep",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                "--packagename", sbmFileName
                };

                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                //enqueue the topic messages
                args = new string[]{
                "aci",  "enqueue",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                 "--concurrencytype", concurrencyType.ToString(),
                 "--override", overrideFile
            };
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                //monitor for completion
                args = new string[]{
                "--loglevel", "debug",
                "aci",  "deploy",
                "--settingsfile", settingsFile,
                "--packagename", sbmFileName,
                "--jobname", jobName,
                "--containercount", containerCount.ToString(),
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--override", overrideFile,
                "--unittest", "true",
                "--monitor", "true",
                "--stream",
                "--eventhublogging", EventHubLogging.ConsolidatedScriptResults.ToString()
            };
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }


        }

        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataTestMethod]
        public void ACI_Queue_SBMSource_DoubleDbConfig_KeyVault_Secrets_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/clientdbtargets-doubledb.cfg");
                var sbmFileName = Path.GetFullPath("SimpleSelect_DoubleClient.sbm");
                if (!File.Exists(sbmFileName))
                {
                    File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect_DoubleClient);
                }


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = TestHelper.GetUniqueJobName("aci");
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                //Prep the build
                var args = new string[]{
                "aci",  "prep",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                "--packagename", sbmFileName

            };

                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                //enqueue the topic messages
                args = new string[]{
                "aci",  "enqueue",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                 "--concurrencytype", concurrencyType.ToString(),
                 "--override", overrideFile
            };
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                //monitor for completion
                args = new string[]{
               "--loglevel", "debug",
                "aci",  "deploy",
                "--settingsfile", settingsFile,
                "--packagename", sbmFileName,
                "--jobname", jobName,
                "--containercount", containerCount.ToString(),
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--override", overrideFile,
                "--unittest", "true",
                "--monitor", "true",
                "--stream"
            };
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                var logFileContents = TestHelper.ReleventLogFileContents(startingLine);

                var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }

        }

        [DataRow("TestConfig/settingsfile-aci-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci-no-registry-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataTestMethod]
        public void ACI_Queue_SBMSource_ManagedIdentity_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
                var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
                if (!File.Exists(sbmFileName))
                {
                    File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
                }


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = TestHelper.GetUniqueJobName("aci");
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                //Prep the build
                var args = new string[]{
                "aci",  "prep",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                "--packagename", sbmFileName,
                "--override", overrideFile
            };

                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                //enqueue the topic messages
                args = new string[]{
                "aci",  "enqueue",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                 "--concurrencytype", concurrencyType.ToString(),
                 "--override", overrideFile
            };
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                //monitor for completion
                args = new string[]{
                "--loglevel", "debug",
                "aci",  "deploy",
                "--settingsfile", settingsFile,
                "--packagename", sbmFileName,
                "--jobname", jobName,
                "--containercount", containerCount.ToString(),
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--override", overrideFile,
                "--unittest", "true",
                "--monitor", "true",
                "--stream",
                "--eventhublogging", EventHubLogging.ConsolidatedScriptResults.ToString()
            };
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }

        }

        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataTestMethod]
        public void ACI_Queue_DacpacSource_KeyVault_Secrets_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");

                int removeCount = 1;
                string server, database;

                var overrideFileContents = File.ReadAllLines(overrideFile).ToList();
                string firstOverride = overrideFileContents.First();
                (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

                string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
                File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(overrideFileContents, removeCount));

                //Get the creds for the DB to be used for unit test DACPAC creation
                var userNameFile = Path.GetFullPath("TestConfig/un.txt");
                var un = File.ReadAllText(userNameFile).Trim();
                var pwFile = Path.GetFullPath("TestConfig/pw.txt");
                var pw = File.ReadAllText(pwFile).Trim();


                var cmdLine = new CommandLineArgs() { UserName = un, Password = pw };
                DatabaseHelper.CreateRandomTable(cmdLine, firstOverride);
                string dacpacName = DatabaseHelper.CreateDacpac(cmdLine, server, database);

                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = TestHelper.GetUniqueJobName("aci");
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                //Prep the build
                var args = new string[]{
                "aci",  "prep",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                "--platinumdacpac", dacpacName,
                "--override", minusFirst,
            };

                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);

                //enqueue the topic messages
                args = new string[]{
                "aci",  "enqueue",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                 "--concurrencytype", concurrencyType.ToString(),
                 "--override", minusFirst
            };
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                string sbmFileName = Path.Combine(Path.GetDirectoryName(dacpacName), Path.GetFileNameWithoutExtension(dacpacName) + ".sbm");

                //monitor for completion
                args = new string[]{
                "--loglevel", "debug",
                "aci",  "deploy",
                "--settingsfile", settingsFile,
                "--packagename", sbmFileName,
                "--platinumdacpac", dacpacName,
                "--jobname", jobName,
                "--containercount", containerCount.ToString(),
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--override", minusFirst,
                "--unittest", "true",
                "--monitor", "true",
                "--stream"
            };
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }
        }
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataTestMethod]
        public void ACI_Queue_DacpacSource_ForceApplyCustom_KeyVault_Secrets_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");

                int removeCount = 1;
                string server, database;

                var overrideFileContents = File.ReadAllLines(overrideFile).ToList();

                string firstOverride = overrideFileContents.First();
                (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

                string server2, database2;
                string thirdOverride = overrideFileContents.ElementAt(2);
                (server2, database2) = DatabaseHelper.ExtractServerAndDbFromLine(thirdOverride);

                string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
                File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(overrideFileContents, removeCount));

                //Get the creds for the DB to be used for unit test DACPAC creation
                var userNameFile = Path.GetFullPath("TestConfig/un.txt");
                var un = File.ReadAllText(userNameFile).Trim();
                var pwFile = Path.GetFullPath("TestConfig/pw.txt");
                var pw = File.ReadAllText(pwFile).Trim();

                var cmdLine = new CommandLineArgs() { UserName = un, Password = pw };
                DatabaseHelper.CreateRandomTable(cmdLine, new List<string>() { firstOverride, thirdOverride });
                string dacpacName = DatabaseHelper.CreateDacpac(cmdLine, server, database);

                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = TestHelper.GetUniqueJobName("aci");
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                //Prep the build
                var args = new string[]{
                "aci",  "prep",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                "--platinumdacpac", dacpacName,
                "--override", minusFirst,

            };

                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);



                //enqueue the topic messages
                args = new string[]{
                "aci",  "enqueue",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                 "--concurrencytype", concurrencyType.ToString(),
                 "--override", minusFirst
            };
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                //Create another table in the first that will be applied when the custom DACPAC is created
                DatabaseHelper.CreateRandomTable(cmdLine, firstOverride);
                DatabaseHelper.CreateRandomTable(cmdLine, thirdOverride);

                string sbmFileName = Path.Combine(Path.GetDirectoryName(dacpacName), Path.GetFileNameWithoutExtension(dacpacName) + ".sbm");

                //monitor for completion
                args = new string[]{
                "--loglevel", "debug",
                "aci",  "deploy",
                "--settingsfile", settingsFile,
                "--packagename", sbmFileName,
                "--platinumdacpac", dacpacName,
                "--jobname", jobName,
                "--containercount", containerCount.ToString(),
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--override", minusFirst,
                "--unittest", "true",
                "--monitor", "true",
                "--stream"
            };
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                var tmp = ConsoleOutput.ToString();
                Assert.IsTrue(tmp.Contains("Dacpac Databases In Sync") || tmp.Contains("Committed - With Custom Dacpac"), "A custom DACPAC should have been required for a database");
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }

        }


        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataRow("TestConfig/settingsfile-aci-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci-no-registry-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataTestMethod]
        public void ACI_Queue_Query_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            string outputFile = Path.GetFullPath($"{Guid.NewGuid().ToString()}.csv");
            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
                var queryFile = Path.GetFullPath("selectquery.sql");
                if (!File.Exists(queryFile))
                {
                    File.WriteAllText(queryFile, Properties.Resources.selectquery);
                }


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                string jobName = TestHelper.GetUniqueJobName("aci");

                //Prep the build
                var args = new string[]{
                "--loglevel", "debug",
                "aci",  "query",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                 "--override", overrideFile,
                 "--outputfile", outputFile,
                 "--queryfile", queryFile,
                "--concurrencytype", concurrencyType.ToString(),
                "--containercount", containerCount.ToString(),
                "--concurrency", concurrency.ToString(),
                "--unittest", "true",
                "--monitor", "true",
                 "--stream"
                };

                var val = rootCommand.InvokeAsync(args);
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
                Debug.WriteLine(ConsoleOutput.ToString());
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }
            }


        }

        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataTestMethod]
        public void Aci_Queue_LongRunning_SBMSource_ByConcurrencyType_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
            var tmpOverride = Path.Combine(Path.GetDirectoryName(overrideFile), Guid.NewGuid().ToString() + ".cfg");
            File.WriteAllLines(tmpOverride, File.ReadAllLines(overrideFile).Take(6).ToArray());

            try
            {
                settingsFile = Path.GetFullPath(settingsFile);
                var sbmFileName = Path.GetFullPath("LongRunning.sbm");
                if (!File.Exists(sbmFileName))
                {
                    File.WriteAllBytes(sbmFileName, Properties.Resources.long_running);
                }


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                var parser = CommandLineBuilder.GetCommandParser();
                string jobName = TestHelper.GetUniqueJobName("aci");
                string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

                //Prep the build
                var args = new string[]{
                    "--loglevel", "debug",
                    "aci",  "run",
                    "--settingsfile", settingsFile,
                    "--jobname", jobName,
                    "--packagename", sbmFileName,
                     "--override", tmpOverride,
                    "--concurrencytype", concurrencyType.ToString(),
                    "--containercount", containerCount.ToString(),
                    "--concurrency", concurrency.ToString(),
                    "--unittest", "true",
                    "--monitor", "true",
                    "--stream",
                    "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString()
                };

                var val = parser.InvokeAsync(args);
                val.Wait();
                int result = val.Result;
                Assert.AreEqual(0, result);
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }
        }
    }
}
