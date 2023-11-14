using Microsoft.SqlServer.Management.Dmf;
using Microsoft.SqlServer.Management.HadrModel;
using Microsoft.SqlServer.TransactSql.ScriptDom;
using Microsoft.VisualStudio.TestPlatform.CommunicationUtilities;
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
using static Microsoft.Azure.Amqp.Serialization.SerializableType;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass()]
    public partial class CommandLineArgsTest
    {

        [TestMethod]
        public void Duplicate_ArgumentCheck_Test()
        {
            CommandLineArgs cmdLine;
            string message;
            string[] args = new string[0];
            var commandList = CommandLineBuilder.ListCommands();
            var sortedCommands = commandList.OrderBy(x => x[0]).ToList();
            try
            {
                foreach(var command in sortedCommands)
                {
                    command.Add("-h");
                    args = command.ToArray();
                    (cmdLine, message) = CommandLineBuilder.ParseArgumentsWithMessage(args);
                    Assert.IsTrue(message.Length == 0, $"Expected empty message, instead returned: '{message}'");

                    System.Console.WriteLine(string.Join(' ', command));
                }
            }
            catch(Exception exe)
            {
                if(exe.Message.ToLower().Contains("an item with the same key has already been added"))
                {
                    Assert.Fail($"There is a duplicate key in the command line arguments: '{string.Join(' ', args)}' : {exe.Message}");
                }
                else
                {
                    Assert.Fail($"Error parsing arguments: '{string.Join(' ', args)}' : {exe.Message}");
                }
            }
        }

        [TestMethod]
        public void WorkerInit_success_with_keyfile()
        {
            var tmpJsonFile = Path.GetTempFileName();
            var tmpSecretFile = Path.GetTempFileName();
            try
            {

                File.WriteAllBytes(tmpJsonFile, Properties.Resources.batch_settings_encrypted);
                File.WriteAllText(tmpSecretFile, Properties.Resources.settingsfilekey);
                var cmdLine = new CommandLineArgs();
                cmdLine.SettingsFileKey = tmpSecretFile;
                cmdLine.SettingsFile = tmpJsonFile;
                (bool success, cmdLine) = Worker.Init(cmdLine, false);

                Assert.IsTrue(success);
                Assert.IsTrue(cmdLine.Decrypted);
                Assert.IsTrue(cmdLine.AuthenticationArgs.Password.EndsWith("="));
            }
            finally
            {
                if (File.Exists(tmpJsonFile))
                    File.Delete(tmpJsonFile);

                if (File.Exists(tmpSecretFile))
                    File.Delete(tmpSecretFile);
            }
        }

        [TestMethod]
        public void WorkerInit_fail_with_bad_keyfile()
        {
            var tmpJsonFile = Path.GetTempFileName();
            var tmpSecretFile = Path.GetTempFileName();
            try
            {

                File.WriteAllBytes(tmpJsonFile, Properties.Resources.batch_settings_encrypted);
                File.WriteAllText(tmpSecretFile, "QDQE@Q!EQQEQD#EQ#DQ#DQ#D#DQ#DQ");
                var cmdLine = new CommandLineArgs();
                cmdLine.SettingsFileKey = tmpSecretFile;
                cmdLine.SettingsFile = tmpJsonFile;
                (bool success, cmdLine) = Worker.Init(cmdLine, false);

                Assert.IsFalse(success);
                Assert.IsFalse(cmdLine.Decrypted);
            }
            finally
            {
                if (File.Exists(tmpJsonFile))
                    File.Delete(tmpJsonFile);

                if (File.Exists(tmpSecretFile))
                    File.Delete(tmpSecretFile);
            }

        }

        [TestMethod]
        public void WorkerInit_success_with_keystring()
        {
            var tmpJsonFile = Path.GetTempFileName();
            try
            {

                File.WriteAllBytes(tmpJsonFile, Properties.Resources.batch_settings_encrypted);
                var cmdLine = new CommandLineArgs();
                cmdLine.SettingsFileKey = Properties.Resources.settingsfilekey;
                cmdLine.SettingsFile = tmpJsonFile;
                (bool success, cmdLine) = Worker.Init(cmdLine, false);

                Assert.IsTrue(success);
                Assert.IsTrue(cmdLine.Decrypted);
                Assert.IsTrue(cmdLine.AuthenticationArgs.Password.EndsWith("="));
            }
            finally
            {
                if (File.Exists(tmpJsonFile))
                    File.Delete(tmpJsonFile);
            }

        }

        [TestMethod]
        public void WorkerInit_decrypt_success_and_keyvault_fail()
        {
            var tmpJsonFile = Path.GetTempFileName();
            try
            {

                File.WriteAllBytes(tmpJsonFile, Properties.Resources.batch_settings_encrypted);
                var cmdLine = new CommandLineArgs();
                cmdLine.SettingsFileKey = Properties.Resources.settingsfilekey;
                cmdLine.SettingsFile = tmpJsonFile;
                cmdLine.KeyVaultName = "doesnotexistawtgfs";
                (bool success, cmdLine) = Worker.Init(cmdLine, false);

                Assert.IsFalse(success);
                Assert.IsTrue(cmdLine.Decrypted);
                Assert.IsTrue(cmdLine.AuthenticationArgs.Password.EndsWith("="));
            }
            finally
            {
                if (File.Exists(tmpJsonFile))
                    File.Delete(tmpJsonFile);
            }

        }
        [TestMethod]
        public void WorkerInit_fail_with_bad_keystring()
        {
            var tmpJsonFile = Path.GetTempFileName();
            try
            {

                File.WriteAllBytes(tmpJsonFile, Properties.Resources.batch_settings_encrypted);
                var cmdLine = new CommandLineArgs();
                cmdLine.SettingsFileKey = "XXXXX";
                cmdLine.SettingsFile = tmpJsonFile;
                (bool success, cmdLine) = Worker.Init(cmdLine, false);
                Assert.IsFalse(success);
                Assert.IsFalse(cmdLine.Decrypted);
            }
            finally
            {
                if (File.Exists(tmpJsonFile))
                    File.Delete(tmpJsonFile);
            }

        }

        [TestMethod]
        public void WorkerInit_fail_with_missing_keystring()
        {
            var tmpJsonFile = Path.GetTempFileName();
            try
            {

                File.WriteAllBytes(tmpJsonFile, Properties.Resources.batch_settings_encrypted);
                var cmdLine = new CommandLineArgs();
                cmdLine.SettingsFile = tmpJsonFile;
                (bool success, cmdLine) = Worker.Init(cmdLine, false);

                Assert.IsFalse(success);
                Assert.IsFalse(cmdLine.Decrypted);
            }
            finally
            {
                if (File.Exists(tmpJsonFile))
                    File.Delete(tmpJsonFile);
            }

        }


    }
}
