using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Threading;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Microsoft.SqlServer.Management.HadrData;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities.Resources;
using SqlSync.SqlBuild;

namespace SqlBuildManager.Console.Dependent.UnitTest
{
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

        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            this.settingsFilePath = Path.GetFullPath("TestConfig/settingsfile.json");
            this.overrideFilePath = Path.GetFullPath("TestConfig/databasetargets.cfg");

            this.cmdLine = new CommandLineArgs();
            this.cmdLine.SettingsFile = this.settingsFilePath;

            this.overrideFileContents = File.ReadAllLines(this.overrideFilePath).ToList();
        }

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
            return this.error + "\r\n" + this.output + "\r\n" + $"Please check the {this.cmdLine.RootLoggingPath}\\SqlBuildManager.Console.Execution.log file to see if you need to add an Azure SQL firewall rule to allow connections";
        }

        



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

            Assert.AreEqual(0, StandardExecutionErrorMessage());
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

        [TestMethod]
        public void AzureBatch_SBMSource_Succes()
        {
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }

            List<string> args = new List<string>();
            args.Add("batch run");
            args.Add($"--settingsfile {this.settingsFilePath}");
            args.Add($"--override {this.overrideFilePath}");
            args.Add($"--packagename {sbmFileName}");



            var result = ExecuteProcess(args);

            Assert.AreEqual(0, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(this.output.Contains($"Batch complete"), $"Should indicate that this was run as a batch job");
        }

        [TestMethod]
        public void LocalThreadedBatch_SBMSource_RunWithError_MissingPackage()
        {
            List<string> args = new List<string>();
            args.Add("batch runthreaded");
            args.Add($"--settingsfile {this.settingsFilePath}");
            args.Add($"--override {this.overrideFilePath}");

            var result = ExecuteProcess(args);

            Assert.AreEqual(-101, result, this.error);
            Assert.IsTrue(this.output.Contains("Completed with Errors"), "This test was supposed to have errors in the run");
            Assert.IsTrue(this.output.Contains("Invalid command line set") && this.output.ToLower().Contains("packagename"), "This test should report a missing commandline");
        }

        [TestMethod]
        public void LocalThreadedBatch_SBMSource_Success()
        {
            string sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }

            List<string> args = new List<string>();
            args.Add("batch runthreaded");
            args.Add($"--settingsfile {this.settingsFilePath}");
            args.Add($"--override {this.overrideFilePath}");
            args.Add($"--packagename {sbmFileName}");



            var result = ExecuteProcess(args);

            Assert.AreEqual(0, result, StandardExecutionErrorMessage());
            Assert.IsTrue(this.output.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(this.output.Contains($"Total number of targets: {this.overrideFileContents.Count()}"), $"Should have run against a {this.overrideFileContents.Count()} databases");
        }
    }
}
