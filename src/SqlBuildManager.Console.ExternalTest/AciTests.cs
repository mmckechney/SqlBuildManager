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
namespace SqlBuildManager.Console.ExternalTest
{
    /// <summary>
    /// To run these tests, you will need to have an Azure environment set up.
    /// You can easily do this by following the script instructions found in the /docs/localbuild.md file
    /// </summary>
    [TestClass]
    public class AciTests
    {
 
        private string settingsFileKeyPath;

        [TestInitialize]
        public void ConfigureProcessInfo()
        {

            SqlBuildManager.Logging.ApplicationLogging.CreateLogger<AciTests>("SqlBuildManager.Console.log", @"C:\temp");
            this.settingsFileKeyPath = Path.GetFullPath("TestConfig/settingsfilekey.txt");

        }
        [TestCleanup]
        public void CleanUp()
        {

        }

   

        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 5, ConcurrencyType.MaxPerServer)]
        [DataRow("TestConfig/settingsfile-aci-no-registry.json", "latest-vNext", 3, 2, ConcurrencyType.Server)]
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
            int startingLine = TestHelper.LogFileCurrentLineCount();

            RootCommand rootCommand = CommandLineBuilder.SetUp();
            string jobName = TestHelper.GetUniqueJobName("aci");
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

        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataTestMethod]
        public void ACI_Queue_DacpacSource_KeyVault_Secrets_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            settingsFile = Path.GetFullPath(settingsFile);
            var overrideFile = Path.GetFullPath("TestConfig/databasetargets.cfg");

            int removeCount = 1;
            string server, database;

            var overrideFileContents = File.ReadAllLines(overrideFile).ToList();
            string firstOverride = overrideFileContents.First();
            (server, database) = DatabaseHelper.ExtractServerAndDbFromLine(firstOverride);

            string minusFirst = Path.GetFullPath("TestConfig/minusFirst.cfg");
            File.WriteAllLines(minusFirst, DatabaseHelper.ModifyTargetList(overrideFileContents, removeCount));

            //Get the creds locally from the K8s file
            var secretsFile = Path.GetFullPath("TestConfig/secrets.yaml");
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
            string jobName = TestHelper.GetUniqueJobName("aci");
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "aci",  "prep",
                "--settingsfile", settingsFile,
                "--tag", imageTag,
                "--jobname", jobName,
                "--platinumdacpac", dacpacName,
                "--outputfile", outputFile,
                "--containercount", containerCount.ToString(),
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--override", minusFirst
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
                 "--override", minusFirst
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
                "--override", minusFirst,
                "--unittest", "true",
                "--monitor", "true"
            };
            val = rootCommand.InvokeAsync(args);
            val.Wait();
            result = val.Result;
            Assert.AreEqual(0, result);
        }
        [DataRow("TestConfig/settingsfile-aci.json", "latest-vNext", 3, 2, ConcurrencyType.Count)]
        [DataTestMethod]
        public void ACI_Queue_DacpacSource_ForceApplyCustom_eyVault_Secrets_Success(string settingsFile, string imageTag, int containerCount, int concurrency, ConcurrencyType concurrencyType)
        {
            settingsFile = Path.GetFullPath(settingsFile);
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

            //Get the creds locally from the K8s file
            var secretsFile = Path.GetFullPath("TestConfig/secrets.yaml");
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
            string jobName = TestHelper.GetUniqueJobName("aci");
            string outputFile = Path.Combine(Directory.GetCurrentDirectory(), jobName + ".json");

            //Prep the build
            var args = new string[]{
                "aci",  "prep",
                "--settingsfile", settingsFile,
                "--tag", imageTag,
                "--jobname", jobName,
                "--platinumdacpac", dacpacName,
                "--outputfile", outputFile,
                "--containercount", containerCount.ToString(),
                "--concurrencytype", concurrencyType.ToString(),
                "--concurrency", concurrency.ToString(),
                "--override", minusFirst
            };

            var val = rootCommand.InvokeAsync(args);
            val.Wait();
            int result = val.Result;
            Assert.AreEqual(0, result);

            //Create another table in the first that will be applied when the custom DACPAC is created
            DatabaseHelper.CreateRandomTable(cmdLine, firstOverride);

            //enqueue the topic messages
            args = new string[]{
                "aci",  "enqueue",
                "--settingsfile", settingsFile,
                "--jobname", jobName,
                 "--concurrencytype", concurrencyType.ToString(),
                 "--override", minusFirst
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
                "--override", minusFirst,
                "--unittest", "true",
                "--monitor", "true",
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
