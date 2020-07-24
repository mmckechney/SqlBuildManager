using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using SqlSync.SqlBuild;

namespace SqlBuildManager.Console.ExternalTest
{
    /// <summary>
    /// To run these tests, you will need to have an Azure environment set up.
    /// You can easily do this by following the script instructions found in the /docs/localbuild.md file
    /// </summary>
    [TestClass]
    public class CliTests
    {
        private System.Diagnostics.Process prc;
        private string output { get; set; }
        private string error { get; set; }
        private void StdOutReader()
        {
            try
            {
                this.output = "";
                string stdOutput = this.prc.StandardOutput.ReadToEnd();
                lock (this)
                {
                    this.output = stdOutput;
                }
            }
            catch (Exception e)
            {
                this.output += e.ToString();
            }
        }
        private void StdErrorReader()
        {
            try
            {
                this.error = "";
                string stdError = this.prc.StandardError.ReadToEnd();
                lock (this)
                {
                    this.error = stdError;
                }
            }
            catch (Exception e)
            {
                this.error += e.ToString();
            }
        }
        private DateTime startTime;
        private DateTime endTime;
        private CommandLineArgs cmdLine;
        private List<string> overrideFileContents;

        private string overrideFilePath;
        private string settingsFilePath;
        private string linuxSettingsFilePath;

        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            this.settingsFilePath = Path.GetFullPath("TestConfig/settingsfile-windows.json");
            this.linuxSettingsFilePath = Path.GetFullPath("TestConfig/settingsfile-linux.json");
            this.overrideFilePath = Path.GetFullPath("TestConfig/databasetargets.cfg");

            this.cmdLine = new CommandLineArgs();
            this.cmdLine.SettingsFile = this.settingsFilePath;

            this.overrideFileContents = File.ReadAllLines(this.overrideFilePath).ToList();
        }

        #region Helpers
        public int ExecuteProcess(List<string> args)
        {
            string arguments = string.Join(" ", args.Select(a => a).ToArray());

            this.startTime = DateTime.Now;
            this.prc = new System.Diagnostics.Process();
            this.prc.StartInfo.FileName = "sbm.exe";
            this.prc.StartInfo.Arguments = arguments;
            this.prc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
            this.prc.StartInfo.CreateNoWindow = true;
            this.prc.StartInfo.UseShellExecute = false;
            this.prc.StartInfo.RedirectStandardOutput = true;
            this.prc.StartInfo.RedirectStandardError = true;
            prc.Start();
            Thread THRoutput = new Thread(new ThreadStart(StdOutReader));
            Thread THRerror = new Thread(new ThreadStart(StdErrorReader));
            THRoutput.Name = "StdOutReader";
            THRerror.Name = "StdErrorReader";
            THRoutput.Start();
            THRerror.Start();

            this.prc.WaitForExit();

            if (!THRoutput.Join(new TimeSpan(0, 3, 0))) THRoutput.Abort();
            if (!THRerror.Join(new TimeSpan(0, 3, 0))) THRerror.Abort();

            this.endTime = DateTime.Now;

            return this.prc.ExitCode;

        }
        string StandardExecutionErrorMessage()
        {
            return this.error + System.Environment.NewLine + this.output + System.Environment.NewLine + $"Please check the {this.cmdLine.RootLoggingPath}\\SqlBuildManager.Console.Execution.log file to see if you need to add an Azure SQL firewall rule to allow connections.\r\nYou may also need to create your Azure environment - please see the /docs/localbuild.md file for instuctions on executing the script";
        }
        private string CreateDacpac(CommandLineArgs cmdLine, string server, string database)
        {
            string fullname = Path.GetFullPath($"TestConfig/{database}.dacpac");
            List<string> args = new List<string>();
            args.Add("dacpac");
            args.Add($"--username {cmdLine.AuthenticationArgs.UserName}");
            args.Add($"--password {cmdLine.AuthenticationArgs.Password}");
            args.Add($"--dacpacname {fullname}");
            args.Add($"--database {database}");
            args.Add($"--server {server}");

            if (ExecuteProcess(args) == 0)
            {
                return fullname;
            }
            else
            {
                return null;
            }

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

            List<string> args = new List<string>();
            args.Add("threaded");
            args.Add($"--override {this.overrideFilePath}");
            args.Add($"--packagename {sbmFileName}");
            args.Add($"--username {this.cmdLine.AuthenticationArgs.UserName}");
            args.Add($"--password {this.cmdLine.AuthenticationArgs.Password}");
            args.Add($"--rootloggingpath {this.cmdLine.RootLoggingPath}");

            var result = ExecuteProcess(args);

            Assert.AreEqual(0, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(this.output.Contains($"Total number of targets: {this.overrideFileContents.Count()}"), $"Should have run against a {this.overrideFileContents.Count()} databases");
        }

        [TestMethod]
        public void LocalSingleRun_SBMSource_Success()
        {
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }

            List<string> args = new List<string>();
            args.Add("build");
            args.Add($"--override \"{this.overrideFileContents[0].Split(":")[1]}\"");
            args.Add($"--packagename {sbmFileName}");
            args.Add($"--username {this.cmdLine.AuthenticationArgs.UserName}");
            args.Add($"--password {this.cmdLine.AuthenticationArgs.Password}");
            args.Add($"--rootloggingpath {this.cmdLine.RootLoggingPath}");

            args.Add($"--server  {this.overrideFileContents[0].Split(":")[0]}");

            

            var result = ExecuteProcess(args);

            Assert.AreEqual(0, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(this.output.Contains("Total number of targets: 1"), "Should have run against a single database");
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

            List<string> args = new List<string>();
            args.Add($"batch {batchMethod}");
            args.Add($"--settingsfile {settingsFile}");
            args.Add($"--override {this.overrideFilePath}");
            args.Add($"--packagename {sbmFileName}");



            var result = ExecuteProcess(args);

            Assert.AreEqual(0, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("Completed Successfully"), "This test was should have worked");
            if (batchMethod == "run")
            {
                Assert.IsTrue(this.output.Contains($"Batch complete"), $"Should indicate that this was run as a batch job");
            }
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(this.output.Contains($"Total number of targets: {this.overrideFileContents.Count()}"), $"Should have run against a {this.overrideFileContents.Count()} databases");
            }
        }

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void Batch_SBMSource_RunWithError_MissingPackage(string batchMethod, string settingsFile)
        {
            List<string> args = new List<string>();
            args.Add($"batch {batchMethod}");
            args.Add($"--settingsfile {settingsFile}");
            args.Add($"--override {this.overrideFilePath}");

            var result = ExecuteProcess(args);

            Assert.AreEqual(-101, result, this.error);
            Assert.IsTrue(this.output.Contains("Completed with Errors"), "This test was supposed to have errors in the run");
            Assert.IsTrue(this.output.Contains("Invalid command line set") && this.output.ToLower().Contains("packagename"), "This test should report a missing commandline");
        }
        

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void LocalThreadedBatch_PlatinumDbSource_Success(string batchMethod, string settingsFile)
        {
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, firstOverride);
            List<string> args = new List<string>();
            args.Add($"batch {batchMethod}");
            args.Add($"--settingsfile {settingsFile}");
            args.Add($"--override {minusFirst}");
            args.Add($"--platinumdbsource {database}");
            args.Add($"--platinumserversource {server}");

            var result = ExecuteProcess(args);

            Assert.AreEqual(0, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("Completed Successfully"), "This test was should have worked");
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(this.output.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
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
            List<string> args = new List<string>();
            args.Add($"batch {batchMethod}");
            args.Add($"--settingsfile {settingsFile}");
            args.Add($"--override {minusFirst}");
            args.Add($"--platinumdbsource {database}");
            args.Add($"--platinumserversource {server}");

            var result = ExecuteProcess(args);

            Assert.AreEqual(0, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(this.output.Contains($"{database2}.dacpac are already in  sync. Looping to next database"), "First comparison DB already in sync. Should go to the next one to create a diff DACPAC");
            
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(this.output.Contains($"{database2}: Dacpac Databases In Sync"), "The second database should already be in sync with the first");
                Assert.IsTrue(this.output.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
            }

        }

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void LocalThreadedBatch_PlatinumDbSource_ADbAlreadyInSync(string batchMethod, string settingsFile)
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
            List<string> args = new List<string>();
            args.Add($"batch {batchMethod}");
            args.Add($"--settingsfile {settingsFile}");
            args.Add($"--override {minusFirst}");
            args.Add($"--platinumdbsource {database}");
            args.Add($"--platinumserversource {server}");

            var result = ExecuteProcess(args);

            Assert.AreEqual(0, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("Completed Successfully"), "This test was should have worked");
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(this.output.Contains($"Generating publish script for database '{database3}'."), "Should create a custom DACPAC for this database since the update would have failed b/c they are in sync.");
                Assert.IsTrue(this.output.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
                Assert.IsTrue(this.output.Contains($"{database3}: Dacpac Databases In Sync"), "The third database should already be in sync with the first");
            }


        }

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void LocalThreadedBatch_DacpacSource_Success(string batchMethod, string settingsFile)
        {
            int removeCount = 1;
            string server, database;
            string firstOverride = this.overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(this.overrideFileContents, removeCount));

            DatabaseHelper.CreateRandomTable(this.cmdLine, firstOverride);

            string dacpacName = CreateDacpac(this.cmdLine, server, database);
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test\r\n{StandardExecutionErrorMessage()}");

            List<string> args = new List<string>();
            args.Add($"batch {batchMethod}"); 
            args.Add($"--settingsfile {settingsFile}");
            args.Add($"--override {minusFirst}");
            args.Add($"--platinumdacpac {dacpacName}");

            var result = ExecuteProcess(args);

            Assert.AreEqual(0, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(this.output.Contains("Successfully created SBM from two dacpacs"), "Indication that the script creation was good");
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(this.output.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
            }

        }

        [DataRow("runthreaded", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-windows.json")]
        [DataRow("run", "TestConfig/settingsfile-linux.json")]
        [DataTestMethod]
        public void LocalThreadedBatch_DacpacSource_FirstDbAlreadyInSync(string batchMethod, string settingsFile)
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
            Assert.IsNotNull(dacpacName, $"There was a problem creating the dacpac for this test\r\n{StandardExecutionErrorMessage()}");

            List<string> args = new List<string>();
            args.Add($"batch {batchMethod}"); 
            args.Add($"--settingsfile {settingsFile}");
            args.Add($"--override {minusFirst}");
            args.Add($"--platinumdacpac {dacpacName}");

            var result = ExecuteProcess(args);

            Assert.AreEqual(0, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(this.output.Contains($"{database2}.dacpac are already in  sync. Looping to next database"), "First comparison DB already in sync. Should go to the next one to create a diff DACPAC");
            
            if (batchMethod == "runthreaded")
            {
                Assert.IsTrue(this.output.Contains($"{database2}: Dacpac Databases In Sync"), "The second database should already be in sync with the first");
                Assert.IsTrue(this.output.Contains($"Total number of targets: {this.overrideFileContents.Count() - removeCount}"), $"Should have run against a {this.overrideFileContents.Count() - removeCount} databases");
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

                List<string> args = new List<string>();
                args.Add($"batch {batchMethod}");
                args.Add($"--settingsfile {settingsFile}");
                args.Add($"--override {overrideFile}");
                args.Add($"--outputfile {outputFile}");
                args.Add($"--queryfile {selectquery}");
                args.Add($"--silent");

                var result = ExecuteProcess(args);

                Assert.AreEqual(0, result, StandardExecutionErrorMessage());
                switch (batchMethod)
                {
                    case "querythreaded":
                        Assert.IsTrue(this.output.Contains("Query complete. The results are in the output file"), "Should have created an output file");
                        break;

                    case "query":
                        Assert.IsTrue(this.output.Contains("Output file copied locally to"), "Should have copied output file locally");
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

            List<string> args = new List<string>();
            args.Add($"batch {batchMethod}");
            args.Add($"--settingsfile {settingsFile}");
            args.Add($"--override {overrideFile}");
            args.Add($"--outputfile {outputFile}");
            args.Add($"--queryfile {insertquery}");
            args.Add($"--silent");

            var result = ExecuteProcess(args);

            Assert.AreEqual(5, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("An INSERT, UPDATE or DELETE keyword was found"), "An INSERT statement should have been found");
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

            List<string> args = new List<string>();
            args.Add($"batch {batchMethod}");
            args.Add($"--settingsfile {settingsFile}");
            args.Add($"--override {overrideFile}");
            args.Add($"--outputfile {outputFile}");
            args.Add($"--queryfile {deletequery}");
            args.Add($"--silent");

            var result = ExecuteProcess(args);

            Assert.AreEqual(5, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("An INSERT, UPDATE or DELETE keyword was found"), "A DELETE statement should have been found");
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

            List<string> args = new List<string>();
            args.Add($"batch {batchMethod}");
            args.Add($"--settingsfile {settingsFile}");
            args.Add($"--override {overrideFile}");
            args.Add($"--outputfile {outputFile}");
            args.Add($"--queryfile {updatequery}");
            args.Add($"--silent");

            var result = ExecuteProcess(args);

            Assert.AreEqual(5, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("An INSERT, UPDATE or DELETE keyword was found"), "An UPDATE statement should have been found");
        }




    }
}
