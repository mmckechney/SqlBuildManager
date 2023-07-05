using Microsoft.SqlServer.Management.HadrModel;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MoreLinq;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ContainerApp.Internal;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
namespace SqlBuildManager.Console.UnitTest
{
    [TestClass()]
    public partial class CommandLineParsingTest
    {
        [DataRow("TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void SettingsFile_basic(string settingsFile)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            if (!File.Exists(settingsFile)) Assert.Inconclusive($"Settings file '{settingsFile}' not found");
     
            string[] args = new string[] {
                "batch", "run",
                "--override", "XXXXX",
                "--settingsfile", settingsFile

            };

            (var cmdLine, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
            Assert.IsNotNull(cmdLine.BatchArgs);
            Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.EssentialOnly));
            Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.ScriptErrors));
        }

        [DataRow("TestConfig/settingsfile-batch-linux.json")]
        [DataTestMethod]
        public void SettingsFile_old_style_EventHubLogging(string settingsFile)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            if (!File.Exists(settingsFile)) Assert.Inconclusive($"Settings file '{settingsFile}' not found");

            string[] args = new string[] {
                "batch", "run",
                "--override", "XXXXX",
                "--settingsfile", settingsFile

            };

            (var cmdLine, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
            Assert.IsNotNull(cmdLine.BatchArgs);
            Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.EssentialOnly));
            Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.ScriptErrors));
        }

    }
}
