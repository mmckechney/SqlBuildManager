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
        [TestMethod]
        public void SettingsFile_basic()
        {
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            File.WriteAllBytes(tmpCfg, Properties.Resources.batch_settings);
            try
            {
                string[] args = new string[] {
                    "batch", "run",
                    "--override", "XXXXX",
                    "--settingsfile", tmpCfg

                };

                (var cmdLine, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsNotNull(cmdLine.BatchArgs);
                Assert.IsTrue(cmdLine.EventHubLogging.Contains(EventHubLogging.EssentialOnly));


            }
            finally
            {
                if (File.Exists(tmpCfg))
                {
                    File.Delete(tmpCfg);
                }
            }
        }



        [TestMethod]
        public void SettingsFile_ChangeBatchRg()
        {
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            File.WriteAllBytes(tmpCfg, Properties.Resources.batch_settings);
            string batchRg = "CHANGEDBYARG";
            try
            {
                string[] args = new string[] {
                    "batch", "run",
                    "--override", "XXXXX",
                    "--settingsfile", tmpCfg,
                    "--batchrg", batchRg

                };

                (var cmdLine, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.AreEqual(batchRg, cmdLine.BatchArgs.ResourceGroup);



                args = new string[] {
                "batch", "run",
                "--override", "XXXXX",
                "--batchrg", batchRg,
                "--settingsfile", tmpCfg

                };

                (cmdLine, message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.AreEqual(batchRg, cmdLine.BatchArgs.ResourceGroup);

                args = new string[] {
                "batch", "run",
                "--settingsfile", tmpCfg,
                "--override", "XXXXX",
                "--batchrg", batchRg


                };

                (cmdLine, message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.AreEqual(batchRg, cmdLine.BatchArgs.ResourceGroup);


            }
            finally
            {
                if (File.Exists(tmpCfg))
                {
                    File.Delete(tmpCfg);
                }
            }
        }

        [TestMethod]
        public void SettingsFile_ChangingEventHubLogging()
        {
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            File.WriteAllBytes(tmpCfg, Properties.Resources.batch_settings);
            try
            {
                string[] args = new string[] {
                    "batch", "run",
                    "--override", "XXXXX",
                    "--settingsfile", tmpCfg,
                    "--eventhublogging", EventHubLogging.ConsolidatedScriptResults.ToString()

                };

                (var cmdLine, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsTrue(cmdLine.EventHubLogging.Contains(EventHubLogging.ConsolidatedScriptResults));



                args = new string[] {
                "batch", "run",
                "--override", "XXXXX",
                "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString(),
                "--settingsfile", tmpCfg

                };

                (cmdLine, message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsTrue(cmdLine.EventHubLogging.Contains(EventHubLogging.IndividualScriptResults));

                args = new string[] {
                "batch", "run",
                "--settingsfile", tmpCfg,
                "--override", "XXXXX",
                "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString(),
                "--eventhublogging", EventHubLogging.VerboseMessages.ToString()
                };

                (cmdLine, message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsTrue(cmdLine.EventHubLogging.Contains(EventHubLogging.IndividualScriptResults));
                Assert.IsTrue(cmdLine.EventHubLogging.Contains(EventHubLogging.VerboseMessages));

                args = new string[] {
                "batch", "run",
                "--settingsfile", tmpCfg,
                "--override", "XXXXX",
                "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString(),
                "--eventhublogging", EventHubLogging.VerboseMessages.ToString(),
                "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString(),
                "--eventhublogging", EventHubLogging.VerboseMessages.ToString()
                };

                (cmdLine, message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsTrue(cmdLine.EventHubLogging.Length == 3);

            }
            finally
            {
                if (File.Exists(tmpCfg))
                {
                    File.Delete(tmpCfg);
                }
            }
        }


        [TestMethod]
        public void SettingsFile_MultipleEHSettings()
        {
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            File.WriteAllBytes(tmpCfg, Properties.Resources.batch_settings_multipleeh);
            try
            {
                string[] args = new string[] {
                    "batch", "run",
                    "--override", "XXXXX",
                    "--settingsfile", tmpCfg

                };

                (var cmdLine, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsNotNull(cmdLine.BatchArgs);
                Assert.IsTrue(cmdLine.EventHubLogging.Contains(EventHubLogging.IndividualScriptResults));
                Assert.IsTrue(cmdLine.EventHubLogging.Contains(EventHubLogging.VerboseMessages));


            }
            finally
            {
                if (File.Exists(tmpCfg))
                {
                    File.Delete(tmpCfg);
                }
            }
        }



        
        [DataRow("TestConfig/settingsfile-batch-linux-mi.json")]
        [DataRow("TestConfig/settingsfile-batch-linux-queue-keyvault-mi.json")]
        [DataRow("TestConfig/settingsfile-batch-linux-queue-mi.json")]
        [DataRow("TestConfig/settingsfile-batch-windows-mi.json")]
        [DataRow("TestConfig/settingsfile-batch-windows-queue-keyvault-mi.json")]
        [DataRow("TestConfig/settingsfile-batch-windows-queue-mi.json")]
        [DataTestMethod]
        public void BatchConnectionString_Test(string settingsFile)
        {

            settingsFile = Path.GetFullPath(settingsFile);
            if(!File.Exists(settingsFile))
            {
                Assert.Inconclusive("Settings file not found: " + settingsFile);
            }
            CommandLineArgs cmdLine = new CommandLineArgs();
            cmdLine.FileInfoSettingsFile = new FileInfo(settingsFile);

            string cmdString = cmdLine.ToStringExtension(StringType.Batch);
            Assert.IsTrue(cmdString.Contains("--authtype \"ManagedIdentity\""), "Missing --authtype \"ManagedIdentity\" flag");

            CommandLineArgs cmd2 = new CommandLineArgs();
            
            var splitter = CommandLineStringSplitter.Instance;
            var args = splitter.Split(cmdString);
            args = args.Insert<string>(new string[] { "batch", "runthreaded" }, 0);

            (cmd2, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args.ToArray());

            Assert.IsFalse(string.IsNullOrWhiteSpace(cmd2.AuthenticationArgs.UserName), "The UserName should be set to the value of the Managed Identity");
        }
    }
}
