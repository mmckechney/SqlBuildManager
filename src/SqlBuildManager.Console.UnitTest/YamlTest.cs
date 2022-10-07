using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.MultiDb;
using System.Diagnostics;
using System.IO;
namespace SqlBuildManager.Console.UnitTest
{


    /// <summary>
    ///This is a test class for ValidationTest and is intended
    ///to contain all ValidationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class YamlTest2
    {


        private TestContext testContextInstance;

        /// <summary>
        ///Gets or sets the test context which provides
        ///information about and functionality for the current test run.
        ///</summary>
        public TestContext TestContext
        {
            get
            {
                return testContextInstance;
            }
            set
            {
                testContextInstance = value;
            }
        }
        [TestMethod()]
        public void JobYaml_Format_Test()
        {
            string settingsFile = Path.GetFullPath("TestConfig/settingsfile-k8s-kv.json");
            if (!File.Exists(settingsFile))
            {
                Assert.Inconclusive($"Could not find required settings file {settingsFile}");
            }
            var cmdLine = new CommandLineArgs();
            cmdLine.FileInfoSettingsFile = new FileInfo(settingsFile);
            cmdLine.ServiceAccountName = "sbm000federatedid-name";
            string yml = Kubernetes.KubernetesManager.GenerateJobYaml(cmdLine);
            
            Trace.WriteLine(yml);
            Assert.IsTrue(true);
        }
    }
}
