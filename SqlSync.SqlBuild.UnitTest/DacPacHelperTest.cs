using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Reflection;
namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class DacPacHelperTest
    {
        [TestMethod]
        public void ScriptDacPacDelta_Test()
        {
            string workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            File.WriteAllBytes(workingDir + @"\PlatunumSchema.dacpac", Properties.Resources.PlatunumSchema);
            File.WriteAllBytes(workingDir + @"\TarnishedSchema.dacpac", Properties.Resources.TarnishedSchema);

            string result = DacPacHelper.ScriptDacPacDeltas(workingDir + @"\PlatunumSchema.dacpac", workingDir + @"\TarnishedSchema.dacpac", workingDir);
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }
        
        [TestMethod]
        public void CreateSbmFromDacPacDifferences_Test()
        {
            string workingDir = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);

            File.WriteAllBytes(workingDir + @"\PlatunumSchema.dacpac", Properties.Resources.PlatunumSchema);
            File.WriteAllBytes(workingDir + @"\TarnishedSchema.dacpac", Properties.Resources.TarnishedSchema);
            string buildFileName = Path.GetTempFileName();

            var result = DacPacHelper.CreateSbmFromDacPacDifferences(workingDir + @"\PlatunumSchema.dacpac", workingDir + @"\TarnishedSchema.dacpac", out buildFileName);

            Assert.IsTrue(result == DacpacDeltasStatus.Success);
            Assert.IsTrue(File.ReadAllBytes(buildFileName).Length > 0);
        }

        [TestMethod]
        public void CleanDacPacScript_Test()
        {
            string raw = Properties.Resources.SyncScriptRaw;
            string cleaned = DacPacHelper.CleanDacPacScript(raw);

            Assert.IsTrue(raw != cleaned);
            Assert.IsTrue(raw.Length > cleaned.Length);
        }
    }
}
