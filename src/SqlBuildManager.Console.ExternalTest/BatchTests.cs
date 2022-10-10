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
using Newtonsoft.Json;

namespace SqlBuildManager.Console.ExternalTest
{
    /// <summary>
    /// To run these tests, you will need to have an Azure environment set up.
    /// You can easily do this by following the script instructions found in the /docs/localbuild.md file
    /// </summary>
    [TestClass]
    public class BatchTests
    {
 
        private CommandLineArgs cmdLine;
        private List<string> overrideFileContents;

        private string overrideFilePath;
        private string settingsFilePath;
        private string linuxSettingsFilePath;
        private string settingsFileKeyPath;

        [TestInitialize]
        public void ConfigureProcessInfo()
        {

            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<BatchTests>("SqlBuildManager.Console.log", @"C:\temp");
            this.settingsFilePath = Path.GetFullPath("TestConfig/settingsfile-batch-windows.json");
            this.settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");
            this.linuxSettingsFilePath = Path.GetFullPath("TestConfig/settingsfile-batch-linux.json");
            this.overrideFilePath = Path.GetFullPath("TestConfig/databasetargets.cfg");

            this.cmdLine = new CommandLineArgs();
            this.cmdLine.FileInfoSettingsFile = new FileInfo(this.settingsFilePath);
            this.cmdLine.SettingsFileKey = this.settingsFileKeyPath;
            bool ds;
            (ds, this.cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            this.overrideFileContents = File.ReadAllLines(this.overrideFilePath).ToList();


        }
        [TestCleanup]
        public void CleanUp()
        {

        }

        #region Helpers

        string StandardExecutionErrorMessage(string logContents)
        {
            return logContents + System.Environment.NewLine + $"Please check the {this.cmdLine.RootLoggingPath}\\SqlBuildManager.Console.Execution.log file to see if you need to add an Azure SQL firewall rule to allow connections.\r\nYou may also need to create your Azure environment - please see the /docs/localbuild.md file for instuctions on executing the script";
        }

        private static string GetUniqueBatchJobName(string prefix)
        {
            string name = prefix + "-" + DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-" + Guid.NewGuid().ToString().ToLower().Replace("-", "").Substring(0, 3);
            return name;
        }
        public static IEnumerable<string> ReadLines(string path)
        {
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 0x1000, FileOptions.SequentialScan))
            using (var sr = new StreamReader(fs, Encoding.UTF8))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    yield return line;
                }
            }
        }
        public static string LogFileName
        {
            get
            {
                return Path.GetFileNameWithoutExtension(SqlBuildManager.Console.Program.applicationLogFileName) + DateTime.Now.ToString("yyyyMMdd") + Path.GetExtension(SqlBuildManager.Console.Program.applicationLogFileName);
            }
        }
        public static int LogFileCurrentLineCount()
        {
            string logFile = Path.Combine(@"C:\temp", LogFileName);
            int startingLines = 0;
            if (File.Exists(logFile))
            {
                startingLines = ReadLines(logFile).Count() -1;
            }

            return startingLines;
        }
        public static string ReleventLogFileContents(int startingLine)
        {

            string logFile = Path.Combine(@"C:\temp", LogFileName);
           return string.Join(Environment.NewLine, ReadLines(logFile).Skip(startingLine).ToArray());
        }

        #endregion



        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows.json", ConcurrencyType.Count, 10)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows.json", ConcurrencyType.Count, 10)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux.json", ConcurrencyType.Count, 10)]

        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows.json", ConcurrencyType.Server, 2)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows.json", ConcurrencyType.Server, 2)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux.json", ConcurrencyType.Server, 2)]

        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows.json", ConcurrencyType.MaxPerServer, 2)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows.json", ConcurrencyType.MaxPerServer, 2)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux.json", ConcurrencyType.MaxPerServer, 2)]
        [DataTestMethod]
        public void Batch_Override_SBMSource_ByConcurrencyType_Success(string batchMethod, string settingsFile, ConcurrencyType concurType, int concurrency)
        {
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }

            settingsFile = Path.GetFullPath(settingsFile);

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            var args = new string[]{
                "--loglevel", "Debug",
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", this.overrideFilePath,
                "--packagename", sbmFileName,
                "--concurrency", "2",
                "--concurrencytype","Server" };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;


            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            if (batchMethod == "run")
            {
                Assert.IsTrue(logFileContents.Contains($"Batch complete"), $"Should indicate that this was run as a batch job");
            }
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(logFileContents.Contains($"Total number of targets: {this.overrideFileContents.Count()}"), $"Should have run against a {this.overrideFileContents.Count()} databases");
            }
        }

        [DataRow("run", "TestConfig/settingsfile-batch-windows-mi.json", ConcurrencyType.Count, 10)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-mi.json", ConcurrencyType.Count, 10)]
        [DataTestMethod]
        public void Batch_Override_SBMSource_ManagedIdentity_ByConcurrencyType_Success(string batchMethod, string settingsFile, ConcurrencyType concurType, int concurrency)
        {
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }

            settingsFile = Path.GetFullPath(settingsFile);

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", this.overrideFilePath,
                "--packagename", sbmFileName,
                "--concurrency", "2",
                "--concurrencytype","Server",
            };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;


            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            if (batchMethod == "run")
            {
                Assert.IsTrue(logFileContents.Contains($"Batch complete"), $"Should indicate that this was run as a batch job");
            }
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(logFileContents.Contains($"Total number of targets: {this.overrideFileContents.Count()}"), $"Should have run against a {this.overrideFileContents.Count()} databases");
            }
        }
       


        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Override_SBMSource_RunWithError_MissingPackage(string batchMethod, string settingsFile)
        {
            settingsFile = Path.GetFullPath(settingsFile);

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", this.overrideFilePath };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;

            Assert.AreEqual(-101, result);
            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.IsTrue(logFileContents.Contains("Completed with Errors"), "This test was supposed to have errors in the run");
            Assert.IsTrue(logFileContents.Contains("Invalid command line set") && logFileContents.ToLower().Contains("packagename"), "This test should report a missing commandline");
        }
        

        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Override_PlatinumDbSource_Succes(string batchMethod, string settingsFile)
        {
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, firstOverride);
            settingsFile = Path.GetFullPath(settingsFile);

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdbsource", database,
                "--platinumserversource", server };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;


            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(logFileContents.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
            }
        }

        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Override_PlatinumDbSource_FirstDbAlreadyInSync(string batchMethod, string settingsFile)
        {
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string server2, database2;
            string secondOverride = this.overrideFileContents.ElementAt(1);
            (server2, database2) = DatabaseHelper.ExtractServerAndDbFromLine(secondOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, new List<string>() { firstOverride, secondOverride });

            settingsFile = Path.GetFullPath(settingsFile);

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdbsource", database,
                "--platinumserversource", server };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(logFileContents.Contains($"{database2}.dacpac are already in  sync. Looping to next database"), "First comparison DB already in sync. Should go to the next one to create a diff DACPAC");

            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(logFileContents.Contains($"{database2}:Dacpac Databases In Sync"), "The second database should already be in sync with the first");
                Assert.IsTrue(logFileContents.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
            }

        }
        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Override_PlatinumDbSource_ADbAlreadyInSync(string batchMethod, string settingsFile)
        {
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string server3, database3;
            string thirdDbOverride = this.overrideFileContents.ElementAt(2);
            (server3, database3) = DatabaseHelper.ExtractServerAndDbFromLine(thirdDbOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, new List<string>() { firstOverride, thirdDbOverride });

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();


            settingsFile = Path.GetFullPath(settingsFile);
            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdbsource", database,
                "--platinumserversource", server };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;
;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            if (batchMethod == "runthreaded")
            {
                
               Assert.IsTrue(logFileContents.Contains($"Custom dacpac required for {server3} : {database3}. Generating file"), "Should create a custom DACPAC for this database since the update would have failed b/c they are in sync.");
                Assert.IsTrue(logFileContents.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
                Assert.IsTrue(logFileContents.Contains($"{database3}:Dacpac Databases In Sync"), "The third database should already be in sync with the first");
            }


        }

        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Override_DacpacSource_Success(string batchMethod, string settingsFile)
        {
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, firstOverride);

            string dacpacName = DatabaseHelper.CreateDacpac(this.cmdLine, server, database);
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test");

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();


            settingsFile = Path.GetFullPath(settingsFile);
            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdacpac", dacpacName };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;
            ;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(logFileContents.Contains("Successfully created SBM from two dacpacs"), "Indication that the script creation was good");
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(logFileContents.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
            }

        }

        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Override_DacpacSource_FirstDbAlreadyInSync(string batchMethod, string settingsFile)
        {
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string server2, database2;
            string secondOverride = this.overrideFileContents.ElementAt(1);
            (server2, database2) = DatabaseHelper.ExtractServerAndDbFromLine(secondOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, new List<string>() { firstOverride, secondOverride });

            string dacpacName = DatabaseHelper.CreateDacpac(this.cmdLine, server, database);
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test");

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();


            settingsFile = Path.GetFullPath(settingsFile);
            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdacpac", dacpacName };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;
            ;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(logFileContents.Contains($"{database2}.dacpac are already in  sync. Looping to next database"), "First comparison DB already in sync. Should go to the next one to create a diff DACPAC");
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(logFileContents.Contains($"{database2}:Dacpac Databases In Sync"), "The second database should already be in sync with the first");
                Assert.IsTrue(logFileContents.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
            }

        }

        [DataRow("querythreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Query_SelectSuccess(string batchMethod, string settingsFile)
        {

            string overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
            string outputFile = Path.GetFullPath($"{Guid.NewGuid().ToString()}.csv");
            try
            {
                string selectquery = Path.GetFullPath("selectquery.sql");
                if (!File.Exists(selectquery))
                {
                    File.WriteAllText(selectquery, Properties.Resources.selectquery);
                }

                //get the size of the log file before we start
                int startingLine = LogFileCurrentLineCount();

                settingsFile = Path.GetFullPath(settingsFile);
                var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", overrideFile,
                "--outputfile", outputFile,
                "--queryfile", selectquery,
                "--silent"};

                RootCommand rootCommand = CommandLineBuilder.SetUp();
                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                var result = val.Result;

                var logFileContents = ReleventLogFileContents(startingLine);
                Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
                switch (batchMethod)
                {
                    case "querythreaded":
                        Assert.IsTrue(logFileContents.Contains("Query complete. The results are in the output file"), "Should have created an output file");
                        break;

                    case "query":
                        Assert.IsTrue(logFileContents.Contains("Output file copied locally to"), "Should have copied output file locally");
                        break;
                }
                Assert.IsTrue(File.Exists(outputFile), "The output file should exist");
                var outputLength = File.ReadAllLines(outputFile).Length;
                var overrideLength = File.ReadAllLines(overrideFile).Length;

                Assert.IsTrue(outputLength > overrideLength, "There should be more lines in the output than were in the override");
            }
            finally
            {
                if(File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }
            }

            
        }

        [DataRow("querythreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Query_InsertFail(string batchMethod, string settingsFile)
        {

            string overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
            string outputFile = Path.GetFullPath($"{Guid.NewGuid().ToString()}.csv");
            string insertquery = Path.GetFullPath("insertquery.sql");
            if (!File.Exists(insertquery))
            {
                File.WriteAllText(insertquery, Properties.Resources.insertquery);
            }

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            settingsFile = Path.GetFullPath(settingsFile);
            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", overrideFile,
                "--outputfile", outputFile,
                "--queryfile", insertquery,
                "--silent"};

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;
            ;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(5, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("An INSERT, UPDATE or DELETE keyword was found"), "An INSERT statement should have been found");
        }

        [DataRow("querythreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Query_DeleteFail(string batchMethod, string settingsFile)
        {

            string overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
            string outputFile = Path.GetFullPath($"{Guid.NewGuid().ToString()}.csv");
            string deletequery = Path.GetFullPath("deletequery.sql");
            if (!File.Exists(deletequery))
            {
                File.WriteAllText(deletequery, Properties.Resources.deletequery);
            }

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            settingsFile = Path.GetFullPath(settingsFile);
            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", overrideFile,
                "--outputfile", outputFile,
                "--queryfile", deletequery,
                "--silent"};

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;
            
            SqlBuildManager.Logging.Configure.CloseAndFlushAllLoggers();
            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(5, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("An INSERT, UPDATE or DELETE keyword was found"), "A DELETE statement should have been found");
        }

        [DataRow("querythreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Query_UpdateFail(string batchMethod, string settingsFile)
        {

            string overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
            string outputFile = Path.GetFullPath($"{Guid.NewGuid().ToString()}.csv");
            string updatequery = Path.GetFullPath("updatequery.sql");
            if (!File.Exists(updatequery))
            {
                File.WriteAllText(updatequery, Properties.Resources.updatequery);
            }
            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            settingsFile = Path.GetFullPath(settingsFile);
            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", overrideFile,
                "--outputfile", outputFile,
                "--queryfile", updatequery,
                "--silent"};

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;
            ;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(5, result, StandardExecutionErrorMessage(logFileContents));
            //Assert.IsTrue(logFileContents.Contains("An INSERT, UPDATE or DELETE keyword was found"), "An UPDATE statement should have been found");
        }

        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows-queue.json", ConcurrencyType.Server, 2)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue.json", ConcurrencyType.Server, 2)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue.json",ConcurrencyType.Server, 2)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue-keyvault.json",ConcurrencyType.Server, 2)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue-keyvault.json",ConcurrencyType.Server, 2)]
        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows-queue.json", ConcurrencyType.MaxPerServer, 5)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue.json", ConcurrencyType.MaxPerServer, 5)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue.json", ConcurrencyType.MaxPerServer, 5)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue-keyvault.json", ConcurrencyType.MaxPerServer, 5)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue-keyvault.json", ConcurrencyType.MaxPerServer, 5)]
        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows-queue.json", ConcurrencyType.Count, 5)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue.json", ConcurrencyType.Count, 5)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue.json", ConcurrencyType.Count, 5)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue-keyvault.json", ConcurrencyType.Count, 5)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue-keyvault.json", ConcurrencyType.Count, 5)]
        [DataTestMethod]
        public void Batch_Queue_SBMSource_ByConcurrencyType_Success(string batchMethod, string settingsFile, ConcurrencyType concurType, int concurrency)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }
            string jobName = GetUniqueBatchJobName("batch-sbm");
            int startingLine = LogFileCurrentLineCount();

            var args = new string[]{ 
                "batch", "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override" , this.overrideFilePath,
                "--concurrencytype",  concurType.ToString(),
                "--jobname", jobName};

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            Task<int> val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

            args = new string[]{
            "batch",  batchMethod,
            "--settingsfile", settingsFile,
            "--settingsfilekey", this.settingsFileKeyPath,
            "--override", this.overrideFilePath,
            "--packagename", sbmFileName,
            "--concurrencytype", concurType.ToString(),
            "--concurrency", concurrency.ToString(),
            "--jobname", jobName,
            "--unittest",
            "--monitor",
            "--stream"
            };

            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;

            logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
        }

        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue-mi.json", ConcurrencyType.Count, 10)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue-keyvault-mi.json", ConcurrencyType.Count, 10)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue-mi.json", ConcurrencyType.Count, 10)]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue-keyvault-mi.json", ConcurrencyType.Count, 10)]
        [DataTestMethod]
        public void Batch_Queue_SBMSource_ManagedIdentity_ByConcurrencyType_Success(string batchMethod, string settingsFile, ConcurrencyType concurType, int concurrency)
        {
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }

            settingsFile = Path.GetFullPath(settingsFile);

            string jobName = GetUniqueBatchJobName("batch-sbm");
            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();


            var args = new string[]{
                "batch", "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override" , this.overrideFilePath,
                "--concurrencytype",  concurType.ToString(),
                "--jobname", jobName};

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            Task<int> val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

            args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", this.overrideFilePath,
                "--packagename", sbmFileName,
                "--concurrency", "2",
                "--concurrencytype",  concurType.ToString(),
                "--jobname", jobName,
                "--monitor",
                "--stream",
                "--unittest"
            };

            rootCommand = CommandLineBuilder.SetUp();
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;


            logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            if (batchMethod == "run")
            {
                Assert.IsTrue(logFileContents.Contains($"Batch complete"), $"Should indicate that this was run as a batch job");
            }
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(logFileContents.Contains($"Total number of targets: {this.overrideFileContents.Count()}"), $"Should have run against a {this.overrideFileContents.Count()} databases");
            }
        }

        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue.json", ConcurrencyType.MaxPerServer, 5)]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue.json", ConcurrencyType.Count, 5)]
        [DataTestMethod]
        public void Batch_Queue_SBMSource_MissingEventHubConnection_Success(string batchMethod, string settingsFile, ConcurrencyType concurType, int concurrency)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }

            string settingFileNoEventHub = Path.Combine(Path.GetDirectoryName(settingsFile), "settingsfile-no-eventhub.json");

            CommandLineArgs cmdLine = new CommandLineArgs() { FileInfoSettingsFile = new FileInfo(settingsFile) };
            cmdLine.ConnectionArgs.EventHubConnectionString = null;
            var updatedJson = JsonConvert.SerializeObject(cmdLine, Formatting.Indented, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            File.WriteAllText(settingFileNoEventHub, updatedJson);
            RootCommand rootCommand = CommandLineBuilder.SetUp();

            string jobName = GetUniqueBatchJobName("batch-sbm");
            int startingLine = LogFileCurrentLineCount();

             var args = new string[]{
                "batch", "enqueue",
                "--settingsfile", settingFileNoEventHub,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override" , this.overrideFilePath,
                "--concurrencytype",  concurType.ToString(),
                "--jobname", jobName};

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var  result = val.Result;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

            args = new string[]{
            "batch",  batchMethod,
            "--settingsfile", settingFileNoEventHub,
            "--settingsfilekey", this.settingsFileKeyPath,
            "--override", this.overrideFilePath,
            "--packagename", sbmFileName,
            "--concurrencytype", concurType.ToString(),
            "--concurrency", concurrency.ToString(),
            "--jobname", jobName,
            "--unittest",
            "--monitor",
            "--stream"
            };

            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;

            logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
        }

        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue-keyvault.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue-keyvault.json")]
        [DataTestMethod]
        public void Batch_Queue_PlatinumDbSource_Success(string batchMethod, string settingsFile)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            DatabaseHelper.CreateRandomTable(this.cmdLine, firstOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            string jobName = GetUniqueBatchJobName("batch-plat");
            int startingLine = LogFileCurrentLineCount();
            var concurType = ConcurrencyType.Count.ToString();
            var args = new string[]{
                "batch", "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override" , minusFirst,
                "--concurrencytype",  concurType,
                "--jobname", jobName };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            Task<int> val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

            args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdbsource", database,
                "--platinumserversource", server,
                "--concurrencytype", concurType,
                "--concurrency", "5",
                "--jobname", jobName,
                "--unittest",
                "--monitor",
                "--stream"
            };

            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;

            logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
        }

        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-windows-queue-keyvault.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue-keyvault.json")]
        [DataTestMethod]
        public void Batch_Queue_DacpacSource_Success(string batchMethod, string settingsFile)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, firstOverride);

            string dacpacName = DatabaseHelper.CreateDacpac(this.cmdLine, server, database);
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test");

           string jobName = GetUniqueBatchJobName("batch-dacpac");
           int startingLine = LogFileCurrentLineCount();
            var concurType = ConcurrencyType.Count.ToString();
            var args = new string[]{
                "--loglevel", "Debug",
                "batch", "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override" , minusFirst,
                "--concurrencytype",  concurType,
                "--jobname", jobName};

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            Task<int> val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

            args = new string[]{
                "--loglevel", "Debug",
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdacpac", dacpacName,
                "--concurrencytype", concurType,
                "--concurrency", "5",
                "--jobname", jobName,
                "--unittest",
                "--monitor",
                "--stream"
            };

            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;

            logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
        }


        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void Batch_Override_DacpacSource_FocceApplyCustom_Success(string batchMethod, string settingsFile)
        {
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string server2, database2;
            string thirdOverride = this.overrideFileContents.ElementAt(2);
            (server2, database2) = DatabaseHelper.ExtractServerAndDbFromLine(thirdOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, new List<string>() { firstOverride, thirdOverride });

            string dacpacName = DatabaseHelper.CreateDacpac(this.cmdLine, server, database);
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test");

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            //Create new table in first so that the third will need a custom DACPAC
            DatabaseHelper.CreateRandomTable(this.cmdLine,  firstOverride);


            settingsFile = Path.GetFullPath(settingsFile);
            var args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdacpac", dacpacName };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;
            ;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");

            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(logFileContents.Contains($"Custom dacpac required for"), "A custom DACPAC should have been required for a database");
                Assert.IsTrue(logFileContents.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
            }

        }

        [DataRow("runthreaded", "TestConfig/settingsfile-batch-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-batch-linux-queue.json")]
        [DataTestMethod]
        public void Batch_Queue_DacpacSource_FocceApplyCustom_Success(string batchMethod, string settingsFile)
        {
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string server2, database2;
            string thirdOverride = this.overrideFileContents.ElementAt(2);
            (server2, database2) = DatabaseHelper.ExtractServerAndDbFromLine(thirdOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, new List<string>() { firstOverride, thirdOverride });

            string dacpacName = DatabaseHelper.CreateDacpac(this.cmdLine, server, database);
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test");

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            //Create new table in first so that the third will need a custom DACPAC
            DatabaseHelper.CreateRandomTable(this.cmdLine, firstOverride);


            string jobName = GetUniqueBatchJobName("bat");
            var concurType = ConcurrencyType.Count.ToString();
            var args = new string[]{
                "batch", "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override" , minusFirst,
                "--concurrencytype",  concurType,
                "--jobname", jobName
            };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            Task<int> val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;

            args = new string[]{
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdacpac", dacpacName,
                "--concurrencytype", concurType,
                "--concurrency", "5",
                "--jobname", jobName,
                "--unittest",
                "--monitor",
                "--stream"
            };

            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(logFileContents.Contains($"Custom dacpac required for"), "A custom DACPAC should have been required for a database");
            }

        }


    }
}
