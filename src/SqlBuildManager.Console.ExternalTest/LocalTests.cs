﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;

namespace SqlBuildManager.Console.ExternalTest
{
    [TestClass]
    public class LocalTests
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

            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<ContainerAppTests>("SqlBuildManager.Console.log", @"C:\temp");
            settingsFilePath = Path.GetFullPath("TestConfig/settingsfile-batch-windows.json");
            settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");
            linuxSettingsFilePath = Path.GetFullPath("TestConfig/settingsfile-batch-linux.json");
            overrideFilePath = Path.GetFullPath("TestConfig/databasetargets.cfg");

            cmdLine = new CommandLineArgs();
            cmdLine.FileInfoSettingsFile = new FileInfo(settingsFilePath);
            cmdLine.SettingsFileKey = settingsFileKeyPath;
            bool ds;
            (ds, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
            overrideFileContents = File.ReadAllLines(overrideFilePath).ToList();


        }
        [TestCleanup]
        public void CleanUp()
        {

        }

        #region Helpers

        string StandardExecutionErrorMessage(string logContents)
        {
            return logContents + System.Environment.NewLine + $"Please check the {cmdLine.RootLoggingPath}\\SqlBuildManager.Console.Execution.log file to see if you need to add an Azure SQL firewall rule to allow connections.\r\nYou may also need to create your Azure environment - please see the /docs/localbuild.md file for instuctions on executing the script";
        }

        private static string GetUniqueJobName()
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
                startingLines = ReadLines(logFile).Count() - 1;
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
                "--override", overrideFilePath,
                "--packagename", sbmFileName,
               "--username", cmdLine.AuthenticationArgs.UserName,
                "--password", cmdLine.AuthenticationArgs.Password,
                 "--rootloggingpath", cmdLine.RootLoggingPath
                };

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;

            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));

            Assert.IsTrue(logFileContents.Contains("Completed Successfully"), "This test was should have worked");
            Assert.IsTrue(logFileContents.Contains($"Total number of targets: {overrideFileContents.Count()}"), $"Should have run against a {overrideFileContents.Count()} databases");
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
            "--override", overrideFileContents[0].Split(":")[1],
            "--packagename", sbmFileName,
            "--username", cmdLine.AuthenticationArgs.UserName,
            "--password", cmdLine.AuthenticationArgs.Password,
            "--rootloggingpath", cmdLine.RootLoggingPath,
            "--server", overrideFileContents[0].Split(":")[0] };


            RootCommand rootCommand = CommandLineBuilder.SetUp();
            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            var result = val.Result;


            var logFileContents = ReleventLogFileContents(startingLine);
            Assert.AreEqual(0, result, StandardExecutionErrorMessage(logFileContents));
            Assert.IsTrue(logFileContents.Contains("Committing transaction for"), "This test was should have worked");

        }
    }
}
