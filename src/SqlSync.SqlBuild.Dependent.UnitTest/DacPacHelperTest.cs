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
            string workingDir = @"C:\temp"; 
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema_simple.dacpac");

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema_simple);

            string result = DacPacHelper.ScriptDacPacDeltas(platinumPath, tarnishedPath, workingDir);
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void ScriptDacPacDelta_InSync_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);

            string result = DacPacHelper.ScriptDacPacDeltas(platinumPath, platinumPath, workingDir);
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        [Ignore]
        public void ScriptDacPacDelta_TestFull()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema.dacpac");

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatunumSchema);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema);

            string result = DacPacHelper.ScriptDacPacDeltas(platinumPath, tarnishedPath, workingDir);
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void CreateSbmFromDacPacDifferences_Success_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema_simple.dacpac");

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema_simple);
            string buildFileName = Path.GetTempFileName();

            var result = DacPacHelper.CreateSbmFromDacPacDifferences(platinumPath, tarnishedPath, false, string.Empty,500, out buildFileName);

            Assert.IsTrue(result == DacpacDeltasStatus.Success);
            Assert.IsTrue(File.ReadAllBytes(buildFileName).Length > 0);
        }
        [TestMethod]
        public void CreateSbmFromDacPacDifferences1_Success_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema1.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema1.dacpac");

            File.WriteAllBytes(platinumPath, Properties.Resources.Platinumschema1);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema1);
            string buildFileName = Path.GetTempFileName();

            var result = DacPacHelper.CreateSbmFromDacPacDifferences(platinumPath, tarnishedPath, false, string.Empty, 500, out buildFileName);

            Assert.IsTrue(result == DacpacDeltasStatus.Success);
            Assert.IsTrue(File.ReadAllBytes(buildFileName).Length > 0);
        }

        [TestMethod]
        public void CreateSbmFromDacPacDifferences_InSync_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);
            string buildFileName = Path.GetTempFileName();

            var result = DacPacHelper.CreateSbmFromDacPacDifferences(platinumPath, platinumPath, false, string.Empty, 500, out buildFileName);

            Assert.IsTrue(result == DacpacDeltasStatus.InSync);
            Assert.IsTrue(string.IsNullOrEmpty(buildFileName));
        }

        [TestMethod]
        public void CreateSbmFromDacPacDifferences1_InSync_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema1.dacpac");

            File.WriteAllBytes(platinumPath, Properties.Resources.Platinumschema1);
            string buildFileName = Path.GetTempFileName();

            var result = DacPacHelper.CreateSbmFromDacPacDifferences(platinumPath, platinumPath, false, string.Empty, 500, out buildFileName);

            Assert.IsTrue(result == DacpacDeltasStatus.InSync);
            Assert.IsTrue(string.IsNullOrEmpty(buildFileName));
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
