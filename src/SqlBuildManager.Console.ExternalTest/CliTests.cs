using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.CommandLine;
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
    public class CliTests
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
            this.settingsFilePath = Path.GetFullPath("TestConfig/settingsfile-windows.json");
            this.settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");
            this.linuxSettingsFilePath = Path.GetFullPath("TestConfig/settingsfile-linux.json");
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
        private string CreateDacpac(CommandLineArgs cmdLine, string server, string database)
        {
            string fullname = Path.GetFullPath($"TestConfig/{database}.dacpac");

            var args = new string[]{
                "dacpac",
                "--username", cmdLine.AuthenticationArgs.UserName,
                "--password", cmdLine.AuthenticationArgs.Password,
                "--dacpacname", fullname,
                "--database", database,
                "--server", server };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;

            if (result == 0)
            {
                return fullname;
            }
            else
            {
                return null;
            }

        }
        private static string GetUniqueBatchJobName()
        {
            string name = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss") + "-" + Guid.NewGuid().ToString().ToLower().Replace("-", "").Substring(0, 3);
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
        public string ReleventLogFileContents(int startingLine)
        {

            string logFile = Path.Combine(@"C:\temp", LogFileName);
           return string.Join(Environment.NewLine, ReadLines(logFile).Skip(startingLine).ToArray());
        }
 
        #endregion

        [TestMethod]
        public void LocalThreaded_SBMSource_Success()
        {
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }
            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            var args = new string[]{
                "threaded",  "run",
                "--override", this.overrideFilePath,
                "--packagename", sbmFileName,
               "--username", this.cmdLine.AuthenticationArgs.UserName,
                "--password", this.cmdLine.AuthenticationArgs.Password,
                 "--rootloggingpath", this.cmdLine.RootLoggingPath                
                };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            
            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(logFileContents.Contains($"Total number of targets: {this.overrideFileContents.Count()}"), $"Should have run against a {this.overrideFileContents.Count()} databases");
        }

        [TestMethod]
        public void LocalSingleRun_SBMSource_Success()
        {
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }

            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            var args = new string[]{
                "build",
            "--override", this.overrideFileContents[0].Split(":")[1],
            "--packagename", sbmFileName,
            "--username", this.cmdLine.AuthenticationArgs.UserName,
            "--password", this.cmdLine.AuthenticationArgs.Password,
            "--rootloggingpath", this.cmdLine.RootLoggingPath,
            "--server", this.overrideFileContents[0].Split(":")[0] };


            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;


            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Build Committed"), "This test was should have worked");

        }

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void Batch_SBMSource_Success(string batchMethod, string settingsFile)
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
                "--packagename", sbmFileName};

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
        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void Batch_SBMSource_ServerConcurrency_Success(string batchMethod, string settingsFile)
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
        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void Batch_SBMSource_MaxServerConcurrency_Success(string batchMethod, string settingsFile)
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
                "--concurrencytype","MaxPerServer" };

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

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void Batch_SBMSource_RunWithError_MissingPackage(string batchMethod, string settingsFile)
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
        

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void LocalThreadedAndBatch_PlatinumDbSource_Succes(string batchMethod, string settingsFile)
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

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void Batch_PlatinumDbSource_FirstDbAlreadyInSync(string batchMethod, string settingsFile)
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
        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void LocalThreadedAndBatch_PlatinumDbSource_ADbAlreadyInSync(string batchMethod, string settingsFile)
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

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void LocalThreadedAndBatch_DacpacSource_Success(string batchMethod, string settingsFile)
        {
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, firstOverride);

            string dacpacName = CreateDacpac(this.cmdLine, server, database);
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

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void LocalThreadedAndBatch_DacpacSource_FirstDbAlreadyInSync(string batchMethod, string settingsFile)
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

            string dacpacName = CreateDacpac(this.cmdLine, server, database);
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

        [DataRow("querythreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void BatchQuery_SelectSuccess(string batchMethod, string settingsFile)
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

        [DataRow("querythreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void BatchQuery_InsertFail(string batchMethod, string settingsFile)
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

        [DataRow("querythreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void BatchQuery_DeleteFail(string batchMethod, string settingsFile)
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

        [DataRow("querythreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-windows.json")]
        [DataRow("query", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void BatchQuery_UpdateFail(string batchMethod, string settingsFile)
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

        [DataRow("runthreaded", "TestConfig/settingsfile-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-linux-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-windows-queue-keyvault.json")]
        [DataRow("run", "TestConfig/settingsfile-linux-queue-keyvault.json")]
        [DataTestMethod]
        public void Batch_Queue_SBMSource_ByServer_Success(string batchMethod, string settingsFile)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }
            string jobName = GetUniqueBatchJobName();
            var concurType = ConcurrencyType.Server.ToString();
            int startingLine = LogFileCurrentLineCount();

            var args = new string[]{ 
                "batch", "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override" , this.overrideFilePath,
                "--concurrencytype",  concurType,
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
            "--concurrencytype", concurType,
            "--concurrency", "2",
            "--jobname", jobName };

            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;

            logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
        }

        [DataRow("runthreaded", "TestConfig/settingsfile-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-linux-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-windows-queue-keyvault.json")]
        [DataRow("run", "TestConfig/settingsfile-linux-queue-keyvault.json")]
        [DataTestMethod]
        public void Batch_Queue_SBMSource_MaxPerserver_Success(string batchMethod, string settingsFile)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }
            string jobName = GetUniqueBatchJobName();
            int startingLine = LogFileCurrentLineCount();
            var concurType = ConcurrencyType.MaxPerServer.ToString();
            var args = new string[]{
                "batch", "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override" , this.overrideFilePath,
                "--concurrencytype",  concurType,
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
            "--concurrencytype", concurType,
            "--concurrency", "5",
            "--jobname", jobName };

            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;

            logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

        }

        [DataRow("runthreaded", "TestConfig/settingsfile-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-linux-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-windows-queue-keyvault.json")]
        [DataRow("run", "TestConfig/settingsfile-linux-queue-keyvault.json")]
        [DataTestMethod]
        public void Batch_Queue_SBMSource_Count_Success(string batchMethod, string settingsFile)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }
            string jobName = GetUniqueBatchJobName();
            int startingLine = LogFileCurrentLineCount();
            var concurType = ConcurrencyType.Count.ToString();
            var args = new string[]{
                "batch", "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override" , this.overrideFilePath,
                "--concurrencytype",  concurType,
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
            "--concurrencytype", concurType,
            "--concurrency", "5",
            "--jobname", jobName };

            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;

            logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

        }

        [DataRow("runthreaded", "TestConfig/settingsfile-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-linux-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-windows-queue-keyvault.json")]
        [DataRow("run", "TestConfig/settingsfile-linux-queue-keyvault.json")]
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

            string jobName = GetUniqueBatchJobName();
            int startingLine = LogFileCurrentLineCount();
            var concurType = ConcurrencyType.Count.ToString();
            var args = new string[]{
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
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdbsource", database,
                "--platinumserversource", server,
                "--concurrencytype", concurType,
                "--concurrency", "5",
                "--jobname", jobName };

            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;

            logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
        }

        [DataRow("runthreaded", "TestConfig/settingsfile-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-windows-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-linux-queue.json")]
        [DataRow("run", "TestConfig/settingsfile-windows-queue-keyvault.json")]
        [DataRow("run", "TestConfig/settingsfile-linux-queue-keyvault.json")]
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

            string dacpacName = CreateDacpac(this.cmdLine, server, database);
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test");

           string jobName = GetUniqueBatchJobName();
           int startingLine = LogFileCurrentLineCount();
            var concurType = ConcurrencyType.Count.ToString();
            var args = new string[]{
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
                "batch",  batchMethod,
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--override", minusFirst,
                "--platinumdacpac", dacpacName,
                "--concurrencytype", concurType,
                "--concurrency", "5",
                "--jobname", jobName };

            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;

            logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
        }

        [DataRow("TestConfig/runtime.yaml", "TestConfig/secrets.yaml", "TestConfig/basic_job.yaml")]
        [DataTestMethod]
        public void Kubernetes_SBMSource_Success(string runtimeFile, string secretsFile, string deployFile)
        {
            var prc = new ProcessHelper();
            secretsFile = Path.GetFullPath(secretsFile);
            runtimeFile = Path.GetFullPath(runtimeFile);
            deployFile = Path.GetFullPath(deployFile);
            var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
            var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }

            
            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            RootCommand rootCommand = CommandLineBuilder.SetUp();

            //Clear any exiting pods
            var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

            //Prep the build
            var args = new string[]{
                "k8s",  "prep",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--jobname", GetUniqueBatchJobName(),
                "--packagename", sbmFileName};

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            //enqueue the topic messages
            args = new string[]{
                "k8s",  "enqueue",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--override", overrideFile};
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            result = prc.ExecuteProcess("kubectl", $"apply -f {secretsFile}");
            Assert.AreEqual(0, result);

            result = prc.ExecuteProcess("kubectl", $"apply -f {runtimeFile}");
            Assert.AreEqual(0, result);

            result = prc.ExecuteProcess("kubectl", $"apply -f {deployFile}");
            Assert.AreEqual(0, result);

            result = prc.ExecuteProcess("kubectl", $"get pods");
            Assert.AreEqual(0, result);

            //monitor for completion
            args = new string[]{
                "k8s",  "monitor",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--override", overrideFile,
                "--unittest", "true"};
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

           
        }

        [DataRow("TestConfig/runtime.yaml", "TestConfig/secrets.yaml", "TestConfig/secretProviderClass.yaml", "TestConfig/podIdentityAndBinding.yaml", "TestConfig/basic_job_keyvault.yaml")]
        [DataTestMethod]
        public void Kubernetes_SBMSource_KeyVault_Secrets_Success(string runtimeFile, string secretsFile, string secretsProviderFile, string podIdentityFile, string deployFile)
        {
            var prc = new ProcessHelper();
            secretsProviderFile = Path.GetFullPath(secretsProviderFile);
            podIdentityFile = Path.GetFullPath(podIdentityFile);
            runtimeFile = Path.GetFullPath(runtimeFile);
            deployFile = Path.GetFullPath(deployFile);
            var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
            var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }


            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            RootCommand rootCommand = CommandLineBuilder.SetUp();

            //Clear any exiting pods
            var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

            //Prep the build
            var args = new string[]{
                "k8s",  "prep",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--jobname", GetUniqueBatchJobName(),
                "--packagename", sbmFileName};

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            //enqueue the topic messages
            args = new string[]{
                "k8s",  "enqueue",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--override", overrideFile};
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            result = prc.ExecuteProcess("kubectl", $"apply -f {secretsProviderFile}");
            Assert.AreEqual(0, result, "Failed to apply secrets provider file");

            result = prc.ExecuteProcess("kubectl", $"apply -f {podIdentityFile}");
            Assert.AreEqual(0, result, "Failed to apply pod identity file");

            result = prc.ExecuteProcess("kubectl", $"apply -f {runtimeFile}");
            Assert.AreEqual(0, result, "Failed to apply runtime  file");

            result = prc.ExecuteProcess("kubectl", $"apply -f {deployFile}");
            Assert.AreEqual(0, result, "Failed to apply deploy  file");

            result = prc.ExecuteProcess("kubectl", $"get pods");
            Assert.AreEqual(0, result);

            //monitor for completion
            args = new string[]{
                "k8s",  "monitor",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--override", overrideFile,
                "--unittest", "true"};
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);


        }

        [DataRow("TestConfig/settingsfile-linux-aci-queue-keyvault.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-linux-aci-queue-keyvault.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-linux-aci-queue-keyvault.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataTestMethod]
        public void ACI_SBMSource_KeyVault_Secrets_Success(string settingsFile, string containerTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
            var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }


            //get the size of the log file before we start
            int startingLine = LogFileCurrentLineCount();

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            string jobName = GetUniqueBatchJobName();
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "aci",  "prep",
                "--settingsfile", settingsFile,
                "--tag", containerTag,
                "--jobname", jobName,
                "--packagename", sbmFileName,
                "--outputfile", outputFile,
                "--containercount", containerCount.ToString(),
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString()
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
                "aci",  "deploy",
                 "--settingsfile", settingsFile,
                 "--templatefile", outputFile,
                "--override", overrideFile,
                "--unittest", "true",
                "--monitor", "true"
            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);


        }



    }
}
