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
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.EssentialOnly));
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.ScriptErrors));


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
        public void SettingsFile_oldstyle_eventhublogging()
        {
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            File.WriteAllBytes(tmpCfg, Properties.Resources.batch_setting_oldstyle_eventhublogging);
            try
            {
                string[] args = new string[] {
                    "batch", "run",
                    "--override", "XXXXX",
                    "--settingsfile", tmpCfg

                };

                (var cmdLine, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsNotNull(cmdLine.BatchArgs);
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.EssentialOnly));
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.ScriptErrors));


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
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.ConsolidatedScriptResults));



                args = new string[] {
                "batch", "run",
                "--override", "XXXXX",
                "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString(),
                "--settingsfile", tmpCfg

                };

                (cmdLine, message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.IndividualScriptResults));

                args = new string[] {
                "batch", "run",
                "--settingsfile", tmpCfg,
                "--override", "XXXXX",
                "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString(),
                "--eventhublogging", EventHubLogging.VerboseMessages.ToString()
                };

                (cmdLine, message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.IndividualScriptResults));
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.VerboseMessages));

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
        public void BatchRun_Only_CommandArgs()
        {
            var args = new string[]
            {
                "batch", "run",
                "--override", "XXXXX",
                "--eventhublogging", "IndividualScriptResults",
                "--eventhublogging", "VerboseMessages"
            };

            (var cmdLine, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
            Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.IndividualScriptResults));
            Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.VerboseMessages));
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
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.IndividualScriptResults));
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.VerboseMessages));


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
        public void SettingsFile_MultipleEHSettings_OldStyle()
        {
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            File.WriteAllBytes(tmpCfg, Properties.Resources.batch_settings_multipleeh_oldstyle);
            try
            {
                string[] args = new string[] {
                    "batch", "run",
                    "--override", "XXXXX",
                    "--settingsfile", tmpCfg

                };

                (var cmdLine, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsNotNull(cmdLine.BatchArgs);
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.IndividualScriptResults));
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.VerboseMessages));


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
        public void SettingsFile_SaveSettings_FromOldStyleEH()
        {
            var tmpCfg = Path.GetTempPath() + Guid.NewGuid().ToString() + ".json";
            File.WriteAllBytes(tmpCfg, Properties.Resources.batch_settings_multipleeh_oldstyle);
            try
            {
                string[] args = new string[] {
                    "batch", "savesettings",
                    "--settingsfile", tmpCfg,
                    "--silent"

                };

                (var cmdLine, string message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsNotNull(cmdLine.BatchArgs);

                Worker.SaveAndEncryptBatchSettings(cmdLine, true);

                (cmdLine, message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                Assert.IsNotNull(cmdLine.BatchArgs);
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.IndividualScriptResults));
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.EssentialOnly));
                Assert.IsTrue(cmdLine.EventHubArgs.Logging.Contains(EventHubLogging.VerboseMessages));


            }
            finally
            {
                if (File.Exists(tmpCfg))
                {
                    File.Delete(tmpCfg);
                }
            }
        }
    }
}
