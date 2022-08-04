using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Serilog;
using Microsoft.Azure.Management.AppService.Fluent.Models;
using SqlBuildManager.Console.KeyVault;
using System.Diagnostics;
using static SqlBuildManager.Console.CommandLine.CommandLineArgs;

namespace SqlBuildManager.Console.ExternalTest
{
    /// <summary>
    /// To run these tests, you will need to have an Azure environment set up.
    /// You can easily do this by following the script instructions found in the /docs/localbuild.md file
    /// </summary>
    [TestClass]
    public class ContainerAppTests
    {
 
        private string settingsFileKeyPath;
        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();

        [TestInitialize]
        public void ConfigureProcessInfo()
        {

            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<ContainerAppTests>("SqlBuildManager.Console.log", @"C:\temp");
            this.settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");

            System.Console.SetOut(new StringWriter(this.ConsoleOutput));    // Associate StringBuilder with StdOut
            this.ConsoleOutput.Clear();    // Clear text from any previous text runs
        }
        [TestCleanup]
        public void CleanUp()
        {

        }


        [DataRow("TestConfig/settingsfile-containerapp-kv-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp-no-registry-kv-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]

        [DataTestMethod]
        public void ContainerApp_Run_Queue_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
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
            string jobName = TestHelper.GetUniqueJobName("ca");
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //monitor for completion
            var args = new string[]{
                //"--loglevel", "Debug",
                "containerapp",  "run",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "-P", sbmFileName,
                "--override", overrideFile,
                "--jobname", jobName,
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--maxcontainers", containerCount.ToString(),
                "--imagetag", imageTag,
                "--unittest", "true",
                "--monitor", "true",
                "--stream", "true",
                "--deletewhendone", "true"

            };
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            int result = val.Result;
            Assert.AreEqual(0, result);

            var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
            Debug.WriteLine(this.ConsoleOutput.ToString());
            Assert.IsTrue(this.ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));


        }



        [DataRow("TestConfig/settingsfile-containerapp-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp-no-registry.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-containerapp-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataRow("TestConfig/settingsfile-containerapp-no-registry-kv-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]

        [DataTestMethod]
        public void ContainerApp_StepWise_Queue_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
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
            string jobName = TestHelper.GetUniqueJobName("ca");
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "containerapp",  "prep",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--jobname", jobName,
                "--packagename", sbmFileName
                
            };

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            int result = val.Result;
            Assert.AreEqual(0, result);

            //enqueue the topic messages
            args = new string[]{
                "containerapp",  "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
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
                //"--loglevel", "Debug",
                "containerapp",  "deploy",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "-P", sbmFileName,
                "--override", overrideFile,
                "--jobname", jobName,
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--maxcontainers", containerCount.ToString(),
                "--imagetag", imageTag,
                "--unittest", "true",
                "--monitor", "true",
                "--stream", "true",
                "--deletewhendone", "true"

            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
            Debug.WriteLine(this.ConsoleOutput.ToString());
            Assert.IsTrue(this.ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));


        }

        //TODO: Enable Managed Identity****** Managed Identity for SQL Authentication is not available for Container Apps currently, only SB and EH
        //[DataRow("TestConfig/settingsfile-containerapp-kv-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp-no-registry-kv-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        
        [DataTestMethod]
        public void ContainerApp_Queue_ManagedIdentity_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
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
            string jobName = TestHelper.GetUniqueJobName("ca");
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "containerapp",  "prep",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--jobname", jobName,
                "--packagename", sbmFileName

            };

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            int result = val.Result;
            Assert.AreEqual(0, result);

            //enqueue the topic messages
            args = new string[]{
                "containerapp",  "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
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
                "--loglevel", "Debug",
                "containerapp",  "deploy",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "-P", sbmFileName,
                "--override", overrideFile,
                "--jobname", jobName,
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--maxcontainers", containerCount.ToString(),
                "--imagetag", imageTag,
                "--unittest", "true",
                "--monitor", "true",
                "--stream", "true",
                "--deletewhendone", "false"

            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
            Debug.WriteLine(this.ConsoleOutput.ToString());
            Assert.IsTrue(this.ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));


        }

        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataTestMethod]
        public void ContainerApp_EnvOnly_Queue_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
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
            string jobName = TestHelper.GetUniqueJobName("ca");
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "containerapp",  "prep",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--jobname", jobName,
                "--packagename", sbmFileName

            };

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            int result = val.Result;
            Assert.AreEqual(0, result);

            //enqueue the topic messages
            args = new string[]{
                "containerapp",  "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
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
                "containerapp",  "deploy",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "-P", sbmFileName,
                "--override", overrideFile,
                "--jobname", jobName,
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--maxcontainers", containerCount.ToString(),
                "--imagetag", imageTag,
                "--unittest", "true",
                "--monitor", "true",
                "--env", "true",
                "--stream", "true",
                "--deletewhendone", "true"
            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
            Debug.WriteLine(this.ConsoleOutput.ToString());
            Assert.IsTrue(this.ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));

        }

        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataTestMethod]
        public void ContainerApp_Queue_DacpacSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
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

            var cmdLine = new CommandLineArgs();
            cmdLine.SettingsFileKey = this.settingsFileKeyPath;
            cmdLine.FileInfoSettingsFile = new FileInfo(settingsFile);
            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
{
                Assert.Fail("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
            }
            bool tmp;
            (tmp, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);

            DatabaseHelper.CreateRandomTable(cmdLine, firstOverride);

            string dacpacName = DatabaseHelper.CreateDacpac(cmdLine, server, database);
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test");

            string sbmFileName = Path.Combine(Path.GetDirectoryName(dacpacName), Path.GetFileNameWithoutExtension(dacpacName) + ".sbm");



            //get the size of the log file before we start
            int startingLine = TestHelper.LogFileCurrentLineCount();

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            string jobName = TestHelper.GetUniqueJobName("ca");
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "containerapp",  "prep",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--jobname", jobName,
                "--platinumdacpac", dacpacName,
                 "--override", minusFirst

            };

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            int result = val.Result;
            Assert.AreEqual(0, result);

            //enqueue the topic messages
            args = new string[]{
                "containerapp",  "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--jobname", jobName,
                "--concurrencytype", concurrencyType.ToString(),
                "--override", minusFirst
            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            //monitor for completion
            args = new string[]{
                //"--loglevel", "Debug",
                "containerapp",  "deploy",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "-P", sbmFileName,
                "--platinumdacpac", dacpacName,
                "--override", minusFirst,
                "--jobname", jobName,
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--maxcontainers", containerCount.ToString(),
                "--imagetag", imageTag,
                "--unittest", "true",
                "--monitor", "true",
                "--stream", "true",
                "--deletewhendone", "true"

            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            var dbCount = File.ReadAllText(minusFirst).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
            Debug.WriteLine(this.ConsoleOutput.ToString());
            Assert.IsTrue(this.ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));
        }


        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-containerapp-kv-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataTestMethod]
        public void ContainerApp_Queue_DacpacSource_DbAlreadyInSync_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
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

            var cmdLine = new CommandLineArgs();
            cmdLine.SettingsFileKey = this.settingsFileKeyPath;
            cmdLine.FileInfoSettingsFile = new FileInfo(settingsFile);
            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                Assert.Fail("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
            }
            bool tmp;
            (tmp, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);

            //First and 3rd will already be in sync and will result in the creation of a custom DACPAC, but no changes needed
            DatabaseHelper.CreateRandomTable(cmdLine, new List<string>() { firstOverride, thirdOverride });

            string dacpacName = DatabaseHelper.CreateDacpac(cmdLine, server, database);
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test");

            string sbmFileName = Path.Combine(Path.GetDirectoryName(dacpacName), Path.GetFileNameWithoutExtension(dacpacName) + ".sbm");

            //get the size of the log file before we start
            int startingLine = TestHelper.LogFileCurrentLineCount();

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            string jobName = TestHelper.GetUniqueJobName("ca");
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "containerapp",  "prep",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--jobname", jobName,
                "--platinumdacpac", dacpacName,
                 "--override", minusFirst

            };

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            int result = val.Result;
            Assert.AreEqual(0, result);

            //enqueue the topic messages
            args = new string[]{
                "containerapp",  "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--jobname", jobName,
                "--concurrencytype", concurrencyType.ToString(),
                "--override", minusFirst
            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            //monitor for completion
            args = new string[]{
                "--loglevel", "Debug",
                "containerapp",  "deploy",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "-P", sbmFileName,
                "--platinumdacpac", dacpacName,
                "--override", minusFirst,
                "--jobname", jobName,
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--unittest", "true",
                "--monitor", "true",
                "--stream", "true",
                "--deletewhendone", "true"

            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            var logFileContents = TestHelper.ReleventLogFileContents(startingLine);
            Assert.IsTrue(logFileContents.Contains("Dacpac Databases In Sync"), "There should be a DB already in sync that forced a custom DACPAC to be created");

            var dbCount = File.ReadAllText(minusFirst).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
            Debug.WriteLine(this.ConsoleOutput.ToString());
            Assert.IsTrue(this.ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));

        }

        [DataRow("TestConfig/settingsfile-containerapp-kv-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataRow("TestConfig/settingsfile-containerapp-kv-mi.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataTestMethod]
        public void ContainerApp_Queue_DacpacSource_ForceApplyCustom_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
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

            var cmdLine = new CommandLineArgs();
            cmdLine.SettingsFileKey = this.settingsFileKeyPath;
            cmdLine.FileInfoSettingsFile = new FileInfo(settingsFile);
            bool decryptSuccess;
            (decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            if (!decryptSuccess)
            {
                Assert.Fail("There was an error decrypting one or more value from the --settingsfile. Please check that you are using the correct --settingsfilekey value");
            }
            bool tmp;
            (tmp, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);

            //First and 3rd will already be in sync, which will cause an SBM failure and force a new custom SBM to be created from the Platinum DACPAC
            DatabaseHelper.CreateRandomTable(cmdLine, new List<string>() { firstOverride, thirdOverride });
            string dacpacName = DatabaseHelper.CreateDacpac(cmdLine, server, database);
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test");

            string sbmFileName = Path.Combine(Path.GetDirectoryName(dacpacName), Path.GetFileNameWithoutExtension(dacpacName) + ".sbm");

            //get the size of the log file before we start
            int startingLine = TestHelper.LogFileCurrentLineCount();

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            string jobName = TestHelper.GetUniqueJobName("ca");
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "containerapp",  "prep",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--jobname", jobName,
                "--platinumdacpac", dacpacName,
                "--override", minusFirst

            };

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            int result = val.Result;
            Assert.AreEqual(0, result);

            //Create another table in the first that will be applied when the custom DACPAC is created
            DatabaseHelper.CreateRandomTable(cmdLine, firstOverride);
            DatabaseHelper.CreateRandomTable(cmdLine, thirdOverride);

            //enqueue the topic messages
            args = new string[]{
                "containerapp",  "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--jobname", jobName,
                "--concurrencytype", concurrencyType.ToString(),
                "--override", minusFirst
            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            //monitor for completion
            args = new string[]{
                //"--loglevel", "Debug",
                "containerapp",  "deploy",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "-P", sbmFileName,
                "--platinumdacpac", dacpacName,
                "--override", minusFirst,
                "--jobname", jobName,
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--unittest", "true",
                "--monitor", "true",
                "--stream", "true",
                "--deletewhendone", "true",
                "--allowobjectdelete", "true"

            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            var logFileContents = TestHelper.ReleventLogFileContents(startingLine);
            Assert.IsTrue(logFileContents.Contains("Committed - With Custom Dacpac"), "A custom DACPAC should have been required for a database");

            var dbCount = File.ReadAllText(minusFirst).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
            Debug.WriteLine(this.ConsoleOutput.ToString());
            Assert.IsTrue(this.ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));

        }
    }
}
