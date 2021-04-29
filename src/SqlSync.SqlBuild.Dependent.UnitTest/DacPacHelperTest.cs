using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    [TestClass]
    public class DacPacHelperTest
    {
        [TestMethod]
        public void ScriptDacPacDelta_Test()
        {
            string workingDir = @"C:\temp"; // SqlBuildManager.Logging.Configure.AppDataPath;

            File.WriteAllBytes(workingDir + @"\PlatinumSchema_simple.dacpac", Properties.Resources.PlatinumSchema_simple);
            File.WriteAllBytes(workingDir + @"\TarnishedSchema_simple.dacpac", Properties.Resources.TarnishedSchema_simple);

            string result = DacPacHelper.ScriptDacPacDeltas(workingDir + @"\PlatinumSchema_simple.dacpac", workingDir + @"\TarnishedSchema_simple.dacpac", workingDir);
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        [Ignore]
        public void ScriptDacPacDelta_TestFull()
        {
            string workingDir = @"C:\temp"; // SqlBuildManager.Logging.Configure.AppDataPath;

            File.WriteAllBytes(workingDir + @"\PlatunumSchema.dacpac", Properties.Resources.PlatunumSchema);
            File.WriteAllBytes(workingDir + @"\TarnishedSchema.dacpac", Properties.Resources.TarnishedSchema);

            string result = DacPacHelper.ScriptDacPacDeltas(workingDir + @"\PlatunumSchema.dacpac", workingDir + @"\TarnishedSchema.dacpac", workingDir);
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void CreateSbmFromDacPacDifferences_Test()
        {
            string workingDir = @"C:\temp"; //SqlBuildManager.Logging.Configure.AppDataPath;

            File.WriteAllBytes(workingDir + @"\PlatinumSchema_simple.dacpac", Properties.Resources.PlatinumSchema_simple);
            File.WriteAllBytes(workingDir + @"\TarnishedSchema_simple.dacpac", Properties.Resources.TarnishedSchema_simple);
            string buildFileName = Path.GetTempFileName();

            var result = DacPacHelper.CreateSbmFromDacPacDifferences(workingDir + @"\PlatinumSchema_simple.dacpac", workingDir + @"\TarnishedSchema_simple.dacpac", false, string.Empty,500, out buildFileName);

            Assert.IsTrue(result == DacpacDeltasStatus.Success);
            Assert.IsTrue(File.ReadAllBytes(buildFileName).Length > 0);
        }

        [TestMethod]
        public void CleanDacPacScript_Test()
        {
            string raw = Properties.Resources.SyncScriptRaw;
            string cleaned;
            var stat = DacPacHelper.CleanDacPacScript(raw, out cleaned);

            Assert.AreEqual(DacpacDeltasStatus.Success, stat);
            Assert.IsTrue(raw.Length > cleaned.Length);
            Assert.AreNotEqual(raw,cleaned);
            
        }
    }
}
