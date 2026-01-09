using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;
using System.Diagnostics;
using System.IO;

#nullable enable
namespace SqlBuildManager.Console.UnitTest
{


    /// <summary>
    ///This is a test class for ValidationTest and is intended
    ///to contain all ValidationTest Unit Tests
    ///</summary>
    [TestClass()]
    public class YamlTest2
    {


        public TestContext TestContext { get; set; } = null!;
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
            string yml = Kubernetes.KubernetesManager.GenerateJobYaml(cmdLine, "debug");

            Trace.WriteLine(yml);
            Assert.IsTrue(true);
        }
    }
}
