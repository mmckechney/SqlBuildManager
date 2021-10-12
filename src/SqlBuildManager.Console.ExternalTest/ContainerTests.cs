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
namespace SqlBuildManager.Console.ExternalTest
{
    /// <summary>
    /// To run these tests, you will need to have an Azure environment set up.
    /// You can easily do this by following the script instructions found in the /docs/localbuild.md file
    /// </summary>
    [TestClass]
    public class ContainerTests
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

            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<ContainerTests>("SqlBuildManager.Console.log", @"C:\temp");
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


        [DataRow("TestConfig/runtime.yaml", "TestConfig/secrets.yaml", "TestConfig/basic_job.yaml")]
        [DataTestMethod]
        public void Kubernetes_Queue_SBMSource_Local_Secrets_Success(string runtimeFile, string secretsFile, string deployFile)
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
                "--jobname", GetUniqueJobName(),
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
        public void Kubernetes_Queue_SBMSource_KeyVault_Secrets_Success(string runtimeFile, string secretsFile, string secretsProviderFile, string podIdentityFile, string deployFile)
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
                "--jobname", GetUniqueJobName(),
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
        public void ACI_Queue_SBMSource_KeyVault_Secrets_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
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
            string jobName = GetUniqueJobName();
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "aci",  "prep",
                "--settingsfile", settingsFile,
                "--tag", imageTag,
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

        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-containerapp.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataTestMethod]
        public void ContainerApp_Queue_SBMSource_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
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
            string jobName = GetUniqueJobName();
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "containerapp",  "prep",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
                "--jobname", jobName,
                "--packagename", sbmFileName
                
            };

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            int result = val.Result;
            Assert.AreEqual(0, result);

            //enqueue the topic messages
            args = new string[]{
                "containerapp",  "enqueue",
                "--settingsfile", settingsFile,
                "--settingsfilekey", this.settingsFileKeyPath,
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
                "containerapp",  "deploy",
                 "--settingsfile", settingsFile,
                 "--settingsfilekey", this.settingsFileKeyPath,
                 "-P", sbmFileName,
                 "--imagetag", imageTag,
                "--override", overrideFile,
                "--jobname", jobName,
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
