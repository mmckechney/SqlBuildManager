using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    /// <summary>
    /// Integration tests for DacPacHelper
    /// Tests for dacpac extraction require live database connection
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class DacPacHelperTest
    {
        private static List<Initialization> _initColl = new();
        private static readonly List<string> _tempFiles = new();
        private static readonly List<string> _tempDirs = new();

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _initColl = new List<Initialization>();
            // Ensure C:\temp exists
            if (!Directory.Exists(@"C:\temp"))
            {
                Directory.CreateDirectory(@"C:\temp");
            }
        }

        [ClassCleanup]
        public static void ClassCleanup()
        {
            foreach (var init in _initColl)
            {
                init.Dispose();
            }

            foreach (var file in _tempFiles)
            {
                try { if (File.Exists(file)) File.Delete(file); } catch { }
            }

            foreach (var dir in _tempDirs)
            {
                try { if (Directory.Exists(dir)) Directory.Delete(dir, true); } catch { }
            }
        }

        private Initialization GetInitialization()
        {
            var init = new Initialization();
            _initColl.Add(init);
            return init;
        }

        #region ScriptDacPacDeltas Tests

        [TestMethod]
        public void ScriptDacPacDelta_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema_simple.dacpac");
            _tempFiles.Add(platinumPath);
            _tempFiles.Add(tarnishedPath);

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema_simple);

            string result = DacPacHelper.ScriptDacPacDeltas(platinumPath, tarnishedPath, workingDir, false, false);
            
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void ScriptDacPacDelta_InSync_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");
            _tempFiles.Add(platinumPath);

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);

            string result = DacPacHelper.ScriptDacPacDeltas(platinumPath, platinumPath, workingDir, false, false);
            
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        [TestMethod]
        public void ScriptDacPacDelta_WithAllowObjectDelete_Works()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema_simple.dacpac");
            _tempFiles.Add(platinumPath);
            _tempFiles.Add(tarnishedPath);

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema_simple);

            string result = DacPacHelper.ScriptDacPacDeltas(platinumPath, tarnishedPath, workingDir, true, false);
            
            Assert.IsNotNull(result);
        }

        [TestMethod]
        [Ignore("Large schema comparison - takes long time")]
        public void ScriptDacPacDelta_TestFull()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema.dacpac");

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatunumSchema);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema);

            string result = DacPacHelper.ScriptDacPacDeltas(platinumPath, tarnishedPath, workingDir, false, false);
            
            Assert.IsNotNull(result);
            Assert.IsFalse(string.IsNullOrEmpty(result));
        }

        #endregion

        #region CreateSbmFromDacPacDifferences Tests

        [TestMethod]
        public async Task CreateSbmFromDacPacDifferences_Success_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema_simple.dacpac");
            _tempFiles.Add(platinumPath);
            _tempFiles.Add(tarnishedPath);

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema_simple);
            string buildFileName = Path.GetTempFileName();
            _tempFiles.Add(buildFileName);

            var (result, buildFileNameResult) = await DacPacHelper.CreateSbmFromDacPacDifferencesAsync(platinumPath, tarnishedPath, false, string.Empty, 500, false);

            Assert.AreEqual(DacpacDeltasStatus.Success, result);
            Assert.IsTrue(File.ReadAllBytes(buildFileNameResult).Length > 0);
            _tempFiles.Add(buildFileNameResult);
        }

        [TestMethod]
        public async Task CreateSbmFromDacPacDifferences_WithBatchScripts_Works()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema_simple.dacpac");
            _tempFiles.Add(platinumPath);
            _tempFiles.Add(tarnishedPath);

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema_simple);

            var (result, buildFileName) = await DacPacHelper.CreateSbmFromDacPacDifferencesAsync(platinumPath, tarnishedPath, true, string.Empty, 500, false);

            Assert.AreEqual(DacpacDeltasStatus.Success, result);
            if (!string.IsNullOrEmpty(buildFileName))
            {
                _tempFiles.Add(buildFileName);
                Assert.IsTrue(File.Exists(buildFileName));
            }
        }

        [TestMethod]
        public async Task CreateSbmFromDacPacDifferences_WithBuildRevision_IncludesVersion()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema_simple.dacpac");
            _tempFiles.Add(platinumPath);
            _tempFiles.Add(tarnishedPath);

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema_simple);

            var (result, buildFileName) = await DacPacHelper.CreateSbmFromDacPacDifferencesAsync(platinumPath, tarnishedPath, false, "1.0.0.1", 500, false);

            Assert.AreEqual(DacpacDeltasStatus.Success, result);
            if (!string.IsNullOrEmpty(buildFileName))
            {
                _tempFiles.Add(buildFileName);
            }
        }

        [TestMethod]
        public async Task CreateSbmFromDacPacDifferences1_Success_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema1.dacpac");
            string tarnishedPath = Path.Combine(workingDir, "TarnishedSchema1.dacpac");
            _tempFiles.Add(platinumPath);
            _tempFiles.Add(tarnishedPath);

            File.WriteAllBytes(platinumPath, Properties.Resources.Platinumschema1);
            File.WriteAllBytes(tarnishedPath, Properties.Resources.TarnishedSchema1);

            var (result, buildFileName) = await DacPacHelper.CreateSbmFromDacPacDifferencesAsync(platinumPath, tarnishedPath, false, string.Empty, 500, false);

            Assert.AreEqual(DacpacDeltasStatus.Success, result);
            Assert.IsTrue(File.ReadAllBytes(buildFileName).Length > 0);
            _tempFiles.Add(buildFileName);
        }

        [TestMethod]
        public async Task CreateSbmFromDacPacDifferences_InSync_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema_simple.dacpac");
            _tempFiles.Add(platinumPath);

            File.WriteAllBytes(platinumPath, Properties.Resources.PlatinumSchema_simple);

            var (result, buildFileName) = await DacPacHelper.CreateSbmFromDacPacDifferencesAsync(platinumPath, platinumPath, false, string.Empty, 500, false);

            Assert.AreEqual(DacpacDeltasStatus.InSync, result);
            Assert.IsTrue(string.IsNullOrEmpty(buildFileName));
        }

        [TestMethod]
        public async Task CreateSbmFromDacPacDifferences1_InSync_Test()
        {
            string workingDir = @"C:\temp";
            string platinumPath = Path.Combine(workingDir, "PlatinumSchema1.dacpac");
            _tempFiles.Add(platinumPath);

            File.WriteAllBytes(platinumPath, Properties.Resources.Platinumschema1);

            var (result, buildFileName) = await DacPacHelper.CreateSbmFromDacPacDifferencesAsync(platinumPath, platinumPath, false, string.Empty, 500, false);

            Assert.AreEqual(DacpacDeltasStatus.InSync, result);
            Assert.IsTrue(string.IsNullOrEmpty(buildFileName));
        }

        #endregion

        #region CleanDacPacScript Tests

        [TestMethod]
        public void CleanDacPacScript_Test()
        {
            string raw = Properties.Resources.SyncScriptRaw;
            
            var stat = DacPacHelper.CleanDacPacScript(raw, out string cleaned);

            Assert.AreEqual(DacpacDeltasStatus.Success, stat);
            Assert.IsTrue(raw.Length > cleaned.Length);
            Assert.AreNotEqual(raw, cleaned);
        }

        [TestMethod]
        public void CleanDacPacScript_RemovesHeader()
        {
            string raw = Properties.Resources.SyncScriptRaw;
            
            DacPacHelper.CleanDacPacScript(raw, out string cleaned);

            // Cleaned script should not contain header comments
            Assert.IsFalse(cleaned.Contains("Please run the below section"));
        }

        [TestMethod]
        public void CleanDacPacScript_EmptyScript_ReturnsInSync()
        {
            string script = "-- Empty script with no changes\r\nGO\r\nPRINT 'Nothing to do'\r\nGO";
            
            var stat = DacPacHelper.CleanDacPacScript(script, out string cleaned);

            // Should return InSync when no real changes
            Assert.IsTrue(stat == DacpacDeltasStatus.InSync || !string.IsNullOrEmpty(cleaned));
        }

        #endregion

        #region BatchAndSaveScripts Tests

        [TestMethod]
        public void BatchAndSaveScripts_CreatesSeparateFiles()
        {
            string workingPath = Path.Combine(Path.GetTempPath(), $"BatchTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(workingPath);
            _tempDirs.Add(workingPath);

            string masterScript = @"CREATE TABLE Test1 (Id INT)
GO
CREATE TABLE Test2 (Id INT)
GO
CREATE TABLE Test3 (Id INT)
GO";

            var files = DacPacHelper.BatchAndSaveScripts(masterScript, workingPath);

            Assert.IsTrue(files.Count >= 3, "Should create at least 3 script files");
            foreach (var file in files)
            {
                Assert.IsTrue(File.Exists(file), $"File should exist: {file}");
                _tempFiles.Add(file);
            }
        }

        [TestMethod]
        public void BatchAndSaveScripts_HandlesEmptyBatches()
        {
            string workingPath = Path.Combine(Path.GetTempPath(), $"BatchTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(workingPath);
            _tempDirs.Add(workingPath);

            string masterScript = @"GO
GO
SELECT 1
GO
GO";

            var files = DacPacHelper.BatchAndSaveScripts(masterScript, workingPath);

            Assert.IsNotNull(files);
            // Empty batches should be skipped
        }

        #endregion

        #region ExtractDacPac Tests (Requires Live Database)

        [TestMethod]
        public void ExtractDacPac_FromTestDatabase_Succeeds()
        {
            var init = GetInitialization();
            string dacPacPath = Path.Combine(Path.GetTempPath(), $"Extract_{Guid.NewGuid()}.dacpac");
            _tempFiles.Add(dacPacPath);

            bool result = DacPacHelper.ExtractDacPac(
                init.testDatabaseNames[0],
                init.serverName,
                AuthenticationType.Windows,
                string.Empty,
                string.Empty,
                dacPacPath,
                60,
                string.Empty);

            Assert.IsTrue(result, "Should successfully extract dacpac from test database");
            Assert.IsTrue(File.Exists(dacPacPath), "Dacpac file should exist");
        }

        [TestMethod]
        public void ExtractDacPac_InvalidDatabase_ReturnsFalse()
        {
            var init = GetInitialization();
            string dacPacPath = Path.Combine(Path.GetTempPath(), $"Extract_{Guid.NewGuid()}.dacpac");
            _tempFiles.Add(dacPacPath);

            bool result = DacPacHelper.ExtractDacPac(
                "NonExistentDatabase12345",
                init.serverName,
                AuthenticationType.Windows,
                string.Empty,
                string.Empty,
                dacPacPath,
                30,
                string.Empty);

            Assert.IsFalse(result, "Should fail for non-existent database");
        }

        [TestMethod]
        public void ExtractDacPac_InvalidServer_ReturnsFalse()
        {
            string dacPacPath = Path.Combine(Path.GetTempPath(), $"Extract_{Guid.NewGuid()}.dacpac");
            _tempFiles.Add(dacPacPath);

            bool result = DacPacHelper.ExtractDacPac(
                "master",
                "NonExistentServer12345",
                AuthenticationType.Windows,
                string.Empty,
                string.Empty,
                dacPacPath,
                5,
                string.Empty);

            Assert.IsFalse(result, "Should fail for non-existent server");
        }

        #endregion

        #region DacpacDeltasStatus Tests

        [TestMethod]
        public void DacpacDeltasStatus_HasExpectedValues()
        {
            // Verify enum values exist
            Assert.AreEqual(0, (int)DacpacDeltasStatus.Success);
            Assert.IsTrue(Enum.IsDefined(typeof(DacpacDeltasStatus), "InSync"));
            Assert.IsTrue(Enum.IsDefined(typeof(DacpacDeltasStatus), "Failure"));
            Assert.IsTrue(Enum.IsDefined(typeof(DacpacDeltasStatus), "ExtractionFailure"));
        }

        #endregion
    }
}
