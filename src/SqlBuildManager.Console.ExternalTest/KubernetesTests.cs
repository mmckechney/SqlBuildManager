using Microsoft.Azure.Batch;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Diagnostics;
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
    public class KubernetesTests
    {

        private string settingsFileKeyPath;
        private StringBuilder ConsoleOutput { get; set; } = new StringBuilder();

        [TestInitialize]
        public void ConfigureProcessInfo()
        {
            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<KubernetesTests>("SqlBuildManager.Console.log", @"C:\temp");
            settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");

            System.Console.SetOut(new StringWriter(ConsoleOutput));    // Associate StringBuilder with StdOut
            ConsoleOutput.Clear();    // Clear text from any previous text runs
        }
        [TestCleanup]
        public void CleanUp()
        {

        }

        [DataRow("TestConfig/settingsfile-k8s-kv.json")]
        [DataRow("TestConfig/settingsfile-k8s-kv-mi.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec-mi.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec.json")]
        [DataTestMethod]
        public void Kubernetes_Run_Queue_SBMSource_Success(string settingsFile)
        {
            try
            {
                var prc = new ProcessHelper();
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
                var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
                if (!File.Exists(sbmFileName))
                {
                    File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
                }
                string jobName = TestHelper.GetUniqueJobName("k8s");


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                //Clear any exiting pods
                var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

                var args = new string[]
                {
                    "--loglevel", "debug",
                "k8s", "run",
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--jobname", jobName,
                "--override", overrideFile,
                "--packagename", sbmFileName,
                "--force",
                "--unittest",
                "--stream",
                "--eventhublogging", EventHubLogging.ConsolidatedScriptResults.ToString()
                };


                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;

                Assert.AreEqual(0, result);

                var logFileContents = TestHelper.ReleventLogFileContents(startingLine);

                var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }

        }

        [DataRow("TestConfig/settingsfile-k8s-kv.json")]
        [DataRow("TestConfig/settingsfile-k8s-kv-mi.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec-mi.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec.json")]
        [DataTestMethod]
        public void Kubernetes_Query_Queue_SBMSource_Success(string settingsFile)
        {
            string outputFile = Path.GetFullPath($"{Guid.NewGuid().ToString()}.csv");
            try
            {
                var prc = new ProcessHelper();
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
                var queryFile = Path.GetFullPath("selectquery.sql");
                
                if (!File.Exists(queryFile))
                {
                    File.WriteAllText(queryFile, Properties.Resources.selectquery);
                }
                string jobName = TestHelper.GetUniqueJobName("k8s");


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                //Clear any exiting pods
                var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

                var args = new string[]
                {
                    "--loglevel", "debug",
                "k8s", "query",
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--jobname", jobName,
                "--override", overrideFile,
                "--queryfile", queryFile,
                "--outputfile", outputFile,
                "--force",
                "--unittest",
                "--stream",
                "--eventhublogging", EventHubLogging.IndividualScriptResults.ToString()
                };


                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;

                Assert.AreEqual(0, result);

                var logFileContents = TestHelper.ReleventLogFileContents(startingLine);

                Assert.IsTrue(File.Exists(outputFile), "The output file should exist");
                var outputLength = File.ReadAllLines(outputFile).Length;
                var overrideLength = File.ReadAllLines(overrideFile).Length;

                Assert.IsTrue(outputLength > overrideLength, "There should be more lines in the output than were in the override");
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
 
                if (File.Exists(outputFile))
                {
                    File.Delete(outputFile);
                }
                
            }

        }

        [DataRow("TestConfig/settingsfile-k8s-kv.json")]
        [DataRow("TestConfig/settingsfile-k8s-kv-mi.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec-mi.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec.json")]
        [DataTestMethod]
        public void Kubernetes_Run_Queue_SBMSource_BadTarget_Fail(string settingsFile)
        {
            try
            {
                var prc = new ProcessHelper();
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/databasetargets-badtargets.cfg");
                var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
                if (!File.Exists(sbmFileName))
                {
                    File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
                }
                string jobName = TestHelper.GetUniqueJobName("k8s");


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                //Clear any exiting pods
                var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

                var args = new string[]
                {
                "k8s", "run",
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--jobname", jobName,
                "--override", overrideFile,
                "--packagename", sbmFileName,
                "--force",
                "--unittest",
                "--stream"
                };


                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;

                Assert.AreEqual(1, result);

                var logFileContents = TestHelper.ReleventLogFileContents(startingLine);

                var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {(dbCount - 2).ToString().PadLeft(5, '0')}"));
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Errors:        {(2).ToString().PadLeft(5, '0')}"));

            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }

        }


        [DataRow("TestConfig/settingsfile-k8s-kv.json")]
        [DataRow("TestConfig/settingsfile-k8s-kv-mi.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec-mi.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec.json")]
        [DataTestMethod]
        public void Kubernetes_Run_Queue_DoubleDbConfig_SBMSource_Success(string settingsFile)
        {
            try
            {
                var prc = new ProcessHelper();
                settingsFile = Path.GetFullPath(settingsFile);
                var overrideFile = Path.GetFullPath("TestConfig/clientdbtargets-doubledb.cfg");
                var sbmFileName = Path.GetFullPath("SimpleSelect_DoubleClient.sbm");
                if (!File.Exists(sbmFileName))
                {
                    File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect_DoubleClient);
                }
                string jobName = TestHelper.GetUniqueJobName("k8s");


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                //Clear any exiting pods
                var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

                var args = new string[]
                {
                "k8s", "run",
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--jobname", jobName,
                "--override", overrideFile,
                "--packagename", sbmFileName,
                "--force",
                "--unittest",
                "--stream"
                };


                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;

                Assert.AreEqual(0, result);

                var logFileContents = TestHelper.ReleventLogFileContents(startingLine);

                var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }

        }

        //Can't run local unit tests with MI since an SBM package needs to be created  :-)
        [DataRow("TestConfig/settingsfile-k8s-kv.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec.json")]
        [DataTestMethod]
        public void Kubernetes_Run_Queue_DacpacSource_ForceApplyCustom_Success(string settingsFile)
        {
            try
            {
                var prc = new ProcessHelper();

                var settingsFileKeyString = File.ReadAllText(settingsFileKeyPath);
                var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
                if (!File.Exists(sbmFileName))
                {
                    File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
                }
                string jobName = TestHelper.GetUniqueJobName("k8s");

                var overrideFile = Path.GetFullPath("TestConfig/clientdbtargets.cfg");

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

                //
                var cmdLine = new CommandLineArgs() { FileInfoSettingsFile = new FileInfo(settingsFile) };
                cmdLine.SettingsFileKey = settingsFileKeyPath;
                (bool decryptSuccess, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
                cmdLine.AuthenticationType = SqlSync.Connection.AuthenticationType.Password;
                if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
                {
                    (var x, cmdLine) = SqlBuildManager.Console.KeyVault.KeyVaultHelper.GetSecrets(cmdLine);
                }
                DatabaseHelper.CreateRandomTable(cmdLine, new List<string>() { firstOverride, thirdOverride });
                string dacpacName = DatabaseHelper.CreateDacpac(cmdLine, server, database);


                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                //Clear any exiting pods
                //var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

                var args = new string[]
                {
                "k8s", "run",
                "--settingsfile", settingsFile,
                "--settingsfilekey", settingsFileKeyPath,
                "--jobname", jobName,
                "--override", overrideFile,
                "--platinumdacpac", dacpacName,
                "--force",
                "--unittest",
                "--stream"
                };


                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                var result = val.Result;

                Assert.AreEqual(0, result);


                var logFileContents = TestHelper.ReleventLogFileContents(startingLine);

                var dbCount = File.ReadAllText(overrideFile).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }


        }

        //Can't run local unit tests with MI since an SBM package needs to be created  :-)
        [DataRow("TestConfig/settingsfile-k8s-kv.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec.json")]
        [DataTestMethod]
        public async Task Kubernetes_Yaml_Queue_DacpacSource_Success(string settingsFile)
        {
            try
            {
                var prc = new ProcessHelper();
                settingsFile = Path.GetFullPath(settingsFile);
                var cmdLine = new CommandLineArgs() { FileInfoSettingsFile = new FileInfo(settingsFile), SettingsFileKey = settingsFileKeyPath };
                (var x, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
                if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName) && cmdLine.AuthenticationArgs.AuthenticationType != SqlSync.Connection.AuthenticationType.ManagedIdentity)
                {
                    (x, cmdLine) = SqlBuildManager.Console.KeyVault.KeyVaultHelper.GetSecrets(cmdLine);
                }
                var jobName = TestHelper.GetUniqueJobName("k8s");
                cmdLine.JobName = jobName;
                var yamlFiles = await Kubernetes.KubernetesManager.SaveKubernetesYamlFiles(cmdLine, jobName, new DirectoryInfo(Path.GetDirectoryName(settingsFile)));
                var overrideFile = Path.GetFullPath("TestConfig/clientdbtargets.cfg");


                int removeCount = 1;
                string server, database;

                var overrideFileContents = File.ReadAllLines(overrideFile).ToList();
                string firstOverride = overrideFileContents.First();
                (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

                string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
                File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(overrideFileContents, removeCount));

                DatabaseHelper.CreateRandomTable(cmdLine, firstOverride);
                string dacpacName = DatabaseHelper.CreateDacpac(cmdLine, server, database);

                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                //Clear any exiting pods
                var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

                //Prep the build
                var args = new string[]{
                "k8s",  "prep",
                "--runtimefile", yamlFiles.RuntimeConfigMapFile,
                "--jobname", jobName,
                "--platinumdacpac", dacpacName,
                "--override", minusFirst,
                "--keyvaultname", cmdLine.ConnectionArgs.KeyVaultName,
                "--storageaccountname", cmdLine.ConnectionArgs.StorageAccountName,
                "--storageaccountkey", cmdLine.ConnectionArgs.StorageAccountKey

            };
                if (!string.IsNullOrWhiteSpace(yamlFiles.SecretsFile))
                {
                    var z = new string[] { "--secretsfile", yamlFiles.SecretsFile };
                    args = args.Concat(z).ToArray();
                }
                if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
                {
                    var z = new string[] { "--username", cmdLine.AuthenticationArgs.UserName, "--password", cmdLine.AuthenticationArgs.Password };
                    args = args.Concat(z).ToArray();
                }

                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                //enqueue the topic messages
                args = new string[]{
                "k8s",  "enqueue",
                "--runtimefile", yamlFiles.RuntimeConfigMapFile,
                "--override", minusFirst,
                "--sb", cmdLine.ConnectionArgs.ServiceBusTopicConnectionString,
                "--jobname", jobName
            };
                if (!string.IsNullOrWhiteSpace(yamlFiles.SecretsFile))
                {
                    args = args.Append("--secretsfile").ToArray();
                    args = args.Append(yamlFiles.SecretsFile).ToArray();
                }
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                if (!string.IsNullOrWhiteSpace(yamlFiles.SecretsFile))
                {
                    result = prc.ExecuteProcess("kubectl", $"apply -f {yamlFiles.SecretsFile}");
                    Assert.AreEqual(0, result, "Failed to apply runtime  file");
                }

                result = prc.ExecuteProcess("kubectl", $"apply -f {yamlFiles.RuntimeConfigMapFile}");
                Assert.AreEqual(0, result, "Failed to apply runtime  file");

                result = prc.ExecuteProcess("kubectl", $"apply -f {yamlFiles.JobFileName}");
                Assert.AreEqual(0, result, "Failed to apply deploy  file");

                result = prc.ExecuteProcess("kubectl", $"get pods");
                Assert.AreEqual(0, result);

                //monitor for completion
                args = new string[]{
                    "k8s",  "monitor",
                    "--runtimefile", yamlFiles.RuntimeConfigMapFile,
                    "--override", minusFirst,
                    "--unittest", "true",
                    "--sb", cmdLine.ConnectionArgs.ServiceBusTopicConnectionString,
                    "--eh",  cmdLine.ConnectionArgs.EventHubConnectionString,
                    "--jobname", jobName,
                    "--storageaccountname", cmdLine.ConnectionArgs.StorageAccountName,
                    "--storageaccountkey", cmdLine.ConnectionArgs.StorageAccountKey,
                    "--stream"
                    };
                if (!string.IsNullOrWhiteSpace(yamlFiles.SecretsFile))
                {
                    args = args.Append("--secretsfile").ToArray();
                    args = args.Append(yamlFiles.SecretsFile).ToArray();
                }
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                var logFileContents = TestHelper.ReleventLogFileContents(startingLine);
                Assert.IsTrue(logFileContents.Contains("DACPAC created") || logFileContents.Contains("Dacpac Databases In Sync"), "A DACPAC should have been used for the build");

                var dbCount = File.ReadAllText(minusFirst).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"));
            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }
        }


        //Can't run local unit tests with MI since an SBM package needs to be created  :-)
        [DataRow("TestConfig/settingsfile-k8s-kv.json")]
        [DataRow("TestConfig/settingsfile-k8s-sec.json")]
        [DataTestMethod]
        public async Task Kubernetes_Yaml_Queue_DacpacSource_ForceApplyCustom_Success(string settingsFile)
        {
            try
            {
                var prc = new ProcessHelper();
                settingsFile = Path.GetFullPath(settingsFile);
                var cmdLine = new CommandLineArgs() { FileInfoSettingsFile = new FileInfo(settingsFile), SettingsFileKey = settingsFileKeyPath };
                (var x, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);
                if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName) && cmdLine.AuthenticationArgs.AuthenticationType != SqlSync.Connection.AuthenticationType.ManagedIdentity)
                {
                    (x, cmdLine) = SqlBuildManager.Console.KeyVault.KeyVaultHelper.GetSecrets(cmdLine);
                }
                var jobName = TestHelper.GetUniqueJobName("k8s");
                cmdLine.JobName = jobName;
                var yamlFiles = await Kubernetes.KubernetesManager.SaveKubernetesYamlFiles(cmdLine, jobName, new DirectoryInfo(Path.GetDirectoryName(settingsFile)));
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

                DatabaseHelper.CreateRandomTable(cmdLine, new List<string>() { firstOverride, thirdOverride });
                string dacpacName = DatabaseHelper.CreateDacpac(cmdLine, server, database);

                //get the size of the log file before we start
                int startingLine = TestHelper.LogFileCurrentLineCount();

                RootCommand rootCommand = CommandLineBuilder.SetUp();

                //Clear any exiting pods
                var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

                //Prep the build
                var args = new string[]{
                "k8s",  "prep",
                "--runtimefile", yamlFiles.RuntimeConfigMapFile,
                "--jobname", jobName,
                "--platinumdacpac", dacpacName,
                "--override", minusFirst,
                "--keyvaultname", cmdLine.ConnectionArgs.KeyVaultName,
                "--storageaccountname", cmdLine.ConnectionArgs.StorageAccountName,
                "--storageaccountkey", cmdLine.ConnectionArgs.StorageAccountKey

            };
                if (!string.IsNullOrWhiteSpace(yamlFiles.SecretsFile))
                {
                    var z = new string[] { "--secretsfile", yamlFiles.SecretsFile };
                    args = args.Concat(z).ToArray();
                }
                if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
                {
                    var z = new string[] { "--username", cmdLine.AuthenticationArgs.UserName, "--password", cmdLine.AuthenticationArgs.Password };
                    args = args.Concat(z).ToArray();
                }

                var val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                //Create another table in the first that will be applied when the custom DACPAC is created
                DatabaseHelper.CreateRandomTable(cmdLine, firstOverride);
                DatabaseHelper.CreateRandomTable(cmdLine, thirdOverride);

                //enqueue the topic messages
                args = new string[]{
                "k8s",  "enqueue",
                "--runtimefile", yamlFiles.RuntimeConfigMapFile,
                "--override", minusFirst,
                "--sb", cmdLine.ConnectionArgs.ServiceBusTopicConnectionString,
                "--jobname", jobName
            };
                if (!string.IsNullOrWhiteSpace(yamlFiles.SecretsFile))
                {
                    args = args.Append("--secretsfile").ToArray();
                    args = args.Append(yamlFiles.SecretsFile).ToArray();
                }
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                if (!string.IsNullOrWhiteSpace(yamlFiles.SecretsFile))
                {
                    result = prc.ExecuteProcess("kubectl", $"apply -f {yamlFiles.SecretsFile}");
                    Assert.AreEqual(0, result, "Failed to apply runtime  file");
                }

                result = prc.ExecuteProcess("kubectl", $"apply -f {yamlFiles.RuntimeConfigMapFile}");
                Assert.AreEqual(0, result, "Failed to apply runtime  file");

                result = prc.ExecuteProcess("kubectl", $"apply -f {yamlFiles.JobFileName}");
                Assert.AreEqual(0, result, "Failed to apply deploy  file");

                result = prc.ExecuteProcess("kubectl", $"get pods");
                Assert.AreEqual(0, result);

                //monitor for completion
                args = new string[]{
                    "k8s",  "monitor",
                    "--runtimefile", yamlFiles.RuntimeConfigMapFile,
                    "--override", minusFirst,
                    "--unittest", "true",
                    "--sb", cmdLine.ConnectionArgs.ServiceBusTopicConnectionString,
                    "--eh",  cmdLine.ConnectionArgs.EventHubConnectionString,
                    "--jobname", jobName,
                    "--storageaccountname", cmdLine.ConnectionArgs.StorageAccountName,
                    "--storageaccountkey", cmdLine.ConnectionArgs.StorageAccountKey,
                    "--stream"
                };
                if (!string.IsNullOrWhiteSpace(yamlFiles.SecretsFile))
                {
                    args = args.Append("--secretsfile").ToArray();
                    args = args.Append(yamlFiles.SecretsFile).ToArray();
                }
                val = rootCommand.InvokeAsync(args);
                val.Wait();
                result = val.Result;
                Assert.AreEqual(0, result);

                var dbCount = File.ReadAllText(minusFirst).Split(Environment.NewLine, StringSplitOptions.RemoveEmptyEntries).Length;
                Assert.IsTrue(ConsoleOutput.ToString().Contains($"Database Commits:       {dbCount.ToString().PadLeft(5, '0')}"), "Should have committed to {db");

                var logFileContents = TestHelper.ReleventLogFileContents(startingLine);
                Assert.IsTrue(logFileContents.Contains("Committed - With Custom Dacpac"), "A custom DACPAC should have been required for a database");

            }
            finally
            {
                Debug.WriteLine(ConsoleOutput.ToString());
            }


        }
    }
}
