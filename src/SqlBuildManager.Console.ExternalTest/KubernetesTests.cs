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
using ydn = YamlDotNet.Serialization;
using System.Dynamic;

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

        [TestInitialize]
        public void ConfigureProcessInfo()
        {

            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<KubernetesTests>("SqlBuildManager.Console.log", @"C:\temp");
            this.settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");

        }
        [TestCleanup]
        public void CleanUp()
        {

        }

        [DataRow("TestConfig/runtime.yaml", "TestConfig/secrets.yaml", "TestConfig/basic_job.yaml")]
        [DataRow("TestConfig/runtime.yaml", "TestConfig/secrets.yaml", "TestConfig/acr_basic_job.yaml")]
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
            int startingLine = TestHelper.LogFileCurrentLineCount();

            RootCommand rootCommand = CommandLineBuilder.SetUp();

            //Clear any exiting pods
            var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

            //Prep the build
            var args = new string[]{
                "k8s",  "prep",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--jobname", TestHelper.GetUniqueJobName("k8s"),
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

        [DataRow("TestConfig/mi-full-runtime.yaml", "TestConfig/mi-full-secrets.yaml", "TestConfig/secretProviderClass.yaml", "TestConfig/podIdentityAndBinding.yaml", "TestConfig/acr_basic_job.yaml")]
        [DataTestMethod]
        public void Kubernetes_Queue_SBMSource_ManagedIdentity_Success(string runtimeFile, string secretsFile, string secretsProviderFile, string podIdentityFile, string deployFile)
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
            int startingLine = TestHelper.LogFileCurrentLineCount();

            RootCommand rootCommand = CommandLineBuilder.SetUp();

            //Clear any exiting pods
            var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

            //Prep the build
            var args = new string[]{
                "k8s",  "prep",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--jobname", TestHelper.GetUniqueJobName("k8s"),
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

            result = prc.ExecuteProcess("kubectl", $"apply -f {secretsFile}");
            Assert.AreEqual(0, result, "Failed to apply secrets file");

            result = prc.ExecuteProcess("kubectl", $"apply -f {runtimeFile}");
            Assert.AreEqual(0, result, "Failed to apply runtime configmap file");

            result = prc.ExecuteProcess("kubectl", $"apply -f {deployFile}");
            Assert.AreEqual(0, result, "Failed to apply job deployment file");

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
        [DataRow("TestConfig/runtime.yaml", "TestConfig/secrets.yaml", "TestConfig/secretProviderClass.yaml", "TestConfig/podIdentityAndBinding.yaml", "TestConfig/acr_basic_job_keyvault.yaml")]
        [DataTestMethod]
        public void Kubernetes_Queue_SBMSource_KeyVault_Secrets_Success(string runtimeFile, string secretsFile, string secretsProviderFile, string podIdentityFile, string deployFile)
        {
            var prc = new ProcessHelper();
            secretsProviderFile = Path.GetFullPath(secretsProviderFile);
            podIdentityFile = Path.GetFullPath(podIdentityFile);
            runtimeFile = Path.GetFullPath(runtimeFile);
            secretsFile = Path.GetFullPath(secretsFile);
            deployFile = Path.GetFullPath(deployFile);
            var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");
            var sbmFileName = Path.GetFullPath("SimpleSelect.sbm");
            if (!File.Exists(sbmFileName))
            {
                File.WriteAllBytes(sbmFileName, Properties.Resources.SimpleSelect);
            }


            //get the size of the log file before we start
            int startingLine = TestHelper.LogFileCurrentLineCount();

            RootCommand rootCommand = CommandLineBuilder.SetUp();

            //Clear any exiting pods
            var result = prc.ExecuteProcess("kubectl", $"delete job sqlbuildmanager ");

            //Prep the build
            var args = new string[]{
                "k8s",  "prep",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--jobname", TestHelper.GetUniqueJobName("k8s-kv"),
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

            secretsFile = Path.GetFullPath("TestConfig/mi-full-secrets.yaml");
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
        [DataRow("TestConfig/runtime.yaml", "TestConfig/secrets.yaml", "TestConfig/secretProviderClass.yaml", "TestConfig/podIdentityAndBinding.yaml", "TestConfig/acr_basic_job_keyvault.yaml")]
        [DataTestMethod]
        public void Kubernetes_Queue_DacpacSource_KeyVault_Secrets_Success(string runtimeFile, string secretsFile, string secretsProviderFile, string podIdentityFile, string deployFile)
        {
            var prc = new ProcessHelper();
            secretsProviderFile = Path.GetFullPath(secretsProviderFile);
            podIdentityFile = Path.GetFullPath(podIdentityFile);
            runtimeFile = Path.GetFullPath(runtimeFile);
            deployFile = Path.GetFullPath(deployFile);
            secretsFile = Path.GetFullPath(secretsFile);
            var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");

            int removeCount = 1;
            string server, database;

            var overrideFileContents = File.ReadAllLines(overrideFile).ToList();
            string firstOverride = overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(overrideFileContents, removeCount));

            var ymlD = new ydn.Deserializer();
            var obj = ymlD.Deserialize<dynamic>(File.ReadAllText(secretsFile));
            var pw = Encoding.UTF8.GetString(Convert.FromBase64String(obj["data"]["Password"]));
            var un = Encoding.UTF8.GetString(Convert.FromBase64String(obj["data"]["UserName"]));

            var cmdLine = new CommandLineArgs() { UserName = un, Password = pw };
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
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--jobname", TestHelper.GetUniqueJobName("k8s-kv"),
                "--platinumdacpac", dacpacName,
                "--override", minusFirst
            };

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            //enqueue the topic messages
            args = new string[]{
                "k8s",  "enqueue",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--override", minusFirst
            };
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
                "--override", minusFirst        ,
                "--unittest", "true"};
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            var logFileContents = TestHelper.ReleventLogFileContents(startingLine);
            Assert.IsTrue(logFileContents.Contains("DACPAC created"), "A DACPAC should have been used for the build");


        }

        [DataRow("TestConfig/runtime.yaml", "TestConfig/secrets.yaml", "TestConfig/secretProviderClass.yaml", "TestConfig/podIdentityAndBinding.yaml", "TestConfig/acr_basic_job_keyvault.yaml")]
        [DataTestMethod]
        public void Kubernetes_Queue_DacpacSource_ForceApplyCustom_KeyVault_Secrets_Success(string runtimeFile, string secretsFile, string secretsProviderFile, string podIdentityFile, string deployFile)
        {
            var prc = new ProcessHelper();
            secretsProviderFile = Path.GetFullPath(secretsProviderFile);
            podIdentityFile = Path.GetFullPath(podIdentityFile);
            runtimeFile = Path.GetFullPath(runtimeFile);
            deployFile = Path.GetFullPath(deployFile);
            secretsFile = Path.GetFullPath(secretsFile);
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

            var ymlD = new ydn.Deserializer();
            var obj = ymlD.Deserialize<dynamic>(File.ReadAllText(secretsFile));
            var pw = Encoding.UTF8.GetString(Convert.FromBase64String(obj["data"]["Password"]));
            var un = Encoding.UTF8.GetString(Convert.FromBase64String(obj["data"]["UserName"]));

            var cmdLine = new CommandLineArgs() { UserName = un, Password = pw };
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
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--jobname", TestHelper.GetUniqueJobName("k8s-kv"),
                "--platinumdacpac", dacpacName,
                "--override", minusFirst
            };

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            //Create another table in the first that will be applied when the custom DACPAC is created
            DatabaseHelper.CreateRandomTable(cmdLine, firstOverride);

            //enqueue the topic messages
            args = new string[]{
                "k8s",  "enqueue",
                "--secretsfile", secretsFile,
                "--runtimefile", runtimeFile,
                "--override", minusFirst
            };
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
                "--override", minusFirst,
                "--unittest", "true",
                "--stream", "true"
            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);

            var logFileContents = TestHelper.ReleventLogFileContents(startingLine);
            Assert.IsTrue(logFileContents.Contains("Committed - With Custom Dacpac"), "A custom DACPAC should have been required for a database");


        }




    }
}
