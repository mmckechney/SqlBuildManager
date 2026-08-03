using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CommandLine;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class AciCommandLineTest
    {
        [TestMethod]
        public void AciEnqueue_SettingsFileKey_Parses()
        {
            var result = CommandLineBuilder.Parse(
            [
                "aci",
                "enqueue",
                "--settingsfile",
                "settings.json",
                "--settingsfilekey",
                "settingsfilekey.txt",
                "--jobname",
                "job-1",
                "--concurrencytype",
                "Count",
                "--override",
                "targets.cfg"
            ]);

            Assert.IsEmpty(result.Errors);
        }

        [TestMethod]
        public void AciRun_SettingsFileKey_Parses()
        {
            var result = CommandLineBuilder.Parse(
            [
                "aci",
                "run",
                "--settingsfile",
                "settings.json",
                "--settingsfilekey",
                "settingsfilekey.txt",
                "--jobname",
                "job-1",
                "--override",
                "targets.cfg",
                "--containercount",
                "1",
                "--concurrency",
                "1",
                "--concurrencytype",
                "Count"
            ]);

            Assert.IsEmpty(result.Errors);
        }

        [TestMethod]
        public void AciDeploy_SettingsFileKey_Parses()
        {
            var result = CommandLineBuilder.Parse(
            [
                "aci",
                "deploy",
                "--settingsfile",
                "settings.json",
                "--settingsfilekey",
                "settingsfilekey.txt",
                "--jobname",
                "job-1",
                "--containercount",
                "1",
                "--concurrency",
                "1",
                "--concurrencytype",
                "Count"
            ]);

            Assert.IsEmpty(result.Errors);
        }

        [TestMethod]
        public void AciMonitor_SettingsFileKey_Parses()
        {
            var result = CommandLineBuilder.Parse(
            [
                "aci",
                "monitor",
                "--settingsfile",
                "settings.json",
                "--settingsfilekey",
                "settingsfilekey.txt"
            ]);

            Assert.IsEmpty(result.Errors);
        }

        [TestMethod]
        public void AciDequeue_SettingsFileKey_Parses()
        {
            var result = CommandLineBuilder.Parse(
            [
                "aci",
                "dequeue",
                "--settingsfile",
                "settings.json",
                "--settingsfilekey",
                "settingsfilekey.txt",
                "--jobname",
                "job-1",
                "--concurrencytype",
                "Count"
            ]);

            Assert.IsEmpty(result.Errors);
        }
    }
}
