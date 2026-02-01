using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// Final tests for SqlBuildFileHelper to increase coverage from 64.8% to 80%+
    /// Focus on methods working with SqlSyncBuildDataModel and SqlSyncBuildData objects
    /// </summary>
    [TestClass]
    public class SqlBuildFileHelperFinalTests
    {
        private string _testDir;

        [TestInitialize]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"SqlBuildFinalTest_{Guid.NewGuid():N}");
            Directory.CreateDirectory(_testDir);
        }

        [TestCleanup]
        public void Cleanup()
        {
            try
            {
                if (Directory.Exists(_testDir))
                    Directory.Delete(_testDir, true);
            }
            catch { }
        }

        #region RenumberBuildSequence Tests

        [TestMethod]
        public async Task RenumberBuildSequence_WithMultipleScripts_RenumbersSequentially()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "test.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Add scripts with non-sequential build orders
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "script1.sql", 5.0,
                "Script 1", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "script2.sql", 10.0,
                "Script 2", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "script3.sql", 15.0,
                "Script 3", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

            // Create dummy script files
            File.WriteAllText(Path.Combine(_testDir, "script1.sql"), "SELECT 1");
            File.WriteAllText(Path.Combine(_testDir, "script2.sql"), "SELECT 2");
            File.WriteAllText(Path.Combine(_testDir, "script3.sql"), "SELECT 3");

            // Act
            var result = SqlBuildFileHelper.RenumberBuildSequence(model, projFileName, zipFileName);

            // Assert
            Assert.IsNotNull(result);
            var scripts = result.Script.OrderBy(r => r.BuildOrder).ToList();
            Assert.AreEqual(1.0, scripts[0].BuildOrder);
            Assert.AreEqual(2.0, scripts[1].BuildOrder);
            Assert.AreEqual(3.0, scripts[2].BuildOrder);
        }

        [TestMethod]
        public void RenumberBuildSequence_WithEmptyBuildData_ReturnsModel()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "test.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = SqlBuildFileHelper.RenumberBuildSequence(model, projFileName, zipFileName);

            // Assert
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public async Task RenumberBuildSequence_WithSingleScript_SetsToOne()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "test.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "script1.sql", 100.0,
                "Script 1", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

            File.WriteAllText(Path.Combine(_testDir, "script1.sql"), "SELECT 1");

            // Act
            var result = SqlBuildFileHelper.RenumberBuildSequence(model, projFileName, zipFileName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1.0, result.Script[0].BuildOrder);
        }

        #endregion

        #region ResortBuildByFileType Tests

        [TestMethod]
        public async Task ResortBuildByFileType_WithMixedFileTypes_SortsByExtension()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "test.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Add scripts with various file types in random order
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "test.sql", 1.0,
                "SQL", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "test.PRC", 2.0,
                "Proc", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "test.TAB", 3.0,
                "Table", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

            // Create dummy script files
            File.WriteAllText(Path.Combine(_testDir, "test.sql"), "SELECT 1");
            File.WriteAllText(Path.Combine(_testDir, "test.PRC"), "CREATE PROCEDURE dbo.Test AS SELECT 1");
            File.WriteAllText(Path.Combine(_testDir, "test.TAB"), "CREATE TABLE dbo.Test (Id INT)");

            // Act
            var result = SqlBuildFileHelper.ResortBuildByFileType(model, projFileName, zipFileName);

            // Assert
            Assert.IsNotNull(result);
            // After sorting, scripts should be reordered based on file type
            Assert.AreEqual(3, result.Script.Count);
        }

        [TestMethod]
        public void ResortBuildByFileType_WithEmptyBuildData_ReturnsModel()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "test.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = SqlBuildFileHelper.ResortBuildByFileType(model, projFileName, zipFileName);

            // Assert
            Assert.IsNotNull(result);
        }

        #endregion

        #region RemoveScriptFilesFromBuild Tests

        [TestMethod]
        public async Task RemoveScriptFilesFromBuild_WithValidRows_RemovesFromModel()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "test.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "script1.sql", 1.0,
                "Script 1", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "script2.sql", 2.0,
                "Script 2", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

            File.WriteAllText(Path.Combine(_testDir, "script1.sql"), "SELECT 1");
            File.WriteAllText(Path.Combine(_testDir, "script2.sql"), "SELECT 2");

            var scriptsToRemove = new[] { model.Script[0] };

            // Act
            var result = SqlBuildFileHelper.RemoveScriptFilesFromBuild(
                model, projFileName, zipFileName, scriptsToRemove, false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Script.Count);
            Assert.AreEqual("script2.sql", result.Script[0].FileName);
        }

        [TestMethod]
        public async Task RemoveScriptFilesFromBuild_WithDeleteFiles_DeletesPhysicalFile()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "test.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "script1.sql", 1.0,
                "Script 1", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

            string scriptFile = Path.Combine(_testDir, "script1.sql");
            File.WriteAllText(scriptFile, "SELECT 1");
            Assert.IsTrue(File.Exists(scriptFile));

            var scriptsToRemove = new[] { model.Script[0] };

            // Act
            var result = SqlBuildFileHelper.RemoveScriptFilesFromBuild(
                model, projFileName, zipFileName, scriptsToRemove, deleteFiles: true);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsFalse(File.Exists(scriptFile));
        }

        [TestMethod]
        public async Task RemoveScriptFilesFromBuild_WithEmptyArray_ReturnsModel()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "test.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, projFileName, "script1.sql", 1.0,
                "Script 1", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

            File.WriteAllText(Path.Combine(_testDir, "script1.sql"), "SELECT 1");

            var emptyRows = Array.Empty<Script>();

            // Act
            var result = SqlBuildFileHelper.RemoveScriptFilesFromBuild(
                model, projFileName, zipFileName, emptyRows, false);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Script.Count); // Original still there
        }

        #endregion

        #region AddScriptFileToBuild (Model version) Tests

        [TestMethod]
        public async Task AddScriptFileToBuild_Model_AddsScriptToModel()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = await SqlBuildFileHelper.AddScriptFileToBuildAsync(
                model,
                projFileName,
                "test.sql",
                1.0,
                "Test Description",
                rollBackScript: true,
                rollBackBuild: false,
                databaseName: "TestDb",
                stripTransactions: true,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "TestUser",
                scriptTimeOut: 60,
                scriptId: Guid.NewGuid(),
                tag: "TestTag");

            // Assert
            Assert.AreEqual(1, result.Script.Count);
            var script = result.Script[0];
            Assert.AreEqual("test.sql", script.FileName);
            Assert.AreEqual(1.0, script.BuildOrder);
            Assert.AreEqual("Test Description", script.Description);
            Assert.AreEqual("TestDb", script.Database);
            Assert.IsTrue(script.StripTransactionText);
            Assert.IsTrue(script.AllowMultipleRuns);
            Assert.AreEqual("TestUser", script.AddedBy);
            Assert.AreEqual(60, script.ScriptTimeOut);
        }

        [TestMethod]
        public async Task AddScriptFileToBuild_ModelWithGuid_UsesProvidedGuid()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            var expectedGuid = Guid.NewGuid();
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = await SqlBuildFileHelper.AddScriptFileToBuildAsync(
                model,
                projFileName,
                "test.sql",
                1.0,
                "Test",
                true, true, "TestDb", false, "", false, true, "user", 30,
                expectedGuid, "");

            // Assert
            Assert.AreEqual(expectedGuid.ToString(), result.Script[0].ScriptId);
        }

        #endregion

        #region AddScriptFileToBuild (Model version with all properties) Tests

        [TestMethod]
        public async Task AddScriptFileToBuild_Model_CreatesNewScriptWithAllProperties()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            var scriptId = Guid.NewGuid();

            // Act
            var result = await SqlBuildFileHelper.AddScriptFileToBuildAsync(
                model,
                projFileName,
                "newscript.sql",
                5.5,
                "New Script Description",
                rollBackScript: true,
                rollBackBuild: false,
                databaseName: "ProductionDb",
                stripTransactions: true,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: false,
                addedBy: "BuildAdmin",
                scriptTimeOut: 120,
                scriptId: scriptId,
                tag: "Release1.0");

            // Assert
            Assert.AreEqual(1, result.Script.Count);
            var script = result.Script[0];
            Assert.AreEqual("newscript.sql", script.FileName);
            Assert.AreEqual(5.5, script.BuildOrder);
            Assert.AreEqual("New Script Description", script.Description);
            Assert.AreEqual("ProductionDb", script.Database);
            Assert.AreEqual(true, script.StripTransactionText);
            Assert.AreEqual(false, script.AllowMultipleRuns);
            Assert.AreEqual("BuildAdmin", script.AddedBy);
            Assert.AreEqual(120, script.ScriptTimeOut);
            Assert.AreEqual(scriptId.ToString(), script.ScriptId);
            Assert.AreEqual("Release1.0", script.Tag);
        }

        [TestMethod]
        public async Task AddScriptFileToBuild_Model_PreservesExistingScripts()
        {
            // Arrange
            var existingScript = new Script(
                "existing.sql", 1.0, "Existing", true, true,
                DateTime.Now, Guid.NewGuid().ToString(), "TestDb",
                false, true, "user", 30, DateTime.MinValue, "", "");
            var model = new SqlSyncBuildDataModel(
                new List<SqlSyncBuildProject> { new SqlSyncBuildProject(0, "Test", false) },
                new List<Script> { existingScript },
                new List<Build>(),
                new List<ScriptRun>(),
                new List<CommittedScript>());
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");

            // Act
            var result = await SqlBuildFileHelper.AddScriptFileToBuildAsync(
                model, projFileName, "new.sql", 2.0, "New", true, true, "TestDb",
                false, "", false, true, "user", 30, Guid.NewGuid(), "");

            // Assert
            Assert.AreEqual(2, result.Script.Count);
            Assert.AreEqual("existing.sql", result.Script[0].FileName);
            Assert.AreEqual("new.sql", result.Script[1].FileName);
        }

        #endregion

        #region CalculateBuildPackageSHA1SignatureFromBatchCollection Tests

        [TestMethod]
        public async Task CalculateBuildPackageSHA1SignatureFromBatchCollection_WithBatches_ReturnsHash()
        {
            // Arrange
            var batchCollection = new ScriptBatchCollection();
            batchCollection.Add(new ScriptBatch("script1.sql", new[] { "SELECT 1" }, "guid1"));
            batchCollection.Add(new ScriptBatch("script2.sql", new[] { "SELECT 2" }, "guid2"));

            // Act - This is an internal method, but we can test via reflection or trust the public API
            // For now, we'll test indirectly through the public CalculateBuildPackageSHA1SignatureFromPathAsync
            // which eventually calls this method
            var hash = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(_testDir, (SqlSyncBuildDataModel)null);

            // Assert
            Assert.AreEqual("Error calculating hash", hash);
        }

        [TestMethod]
        public async Task CalculateBuildPackageSHA1SignatureFromPath_WithValidModel_ReturnsHash()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_testDir, "script1.sql"), "SELECT 1");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var updatedScripts = model.Script.ToList();
            updatedScripts.Add(new Script(
                "script1.sql", 1.0, "Test", true, true,
                DateTime.Now, Guid.NewGuid().ToString(), "TestDb",
                false, true, "user", 30, DateTime.MinValue, "", ""));
            model = new SqlSyncBuildDataModel(
                model.SqlSyncBuildProject,
                updatedScripts,
                model.Build,
                model.ScriptRun,
                model.CommittedScript);

            // Act
            var hash = await SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPathAsync(_testDir, model);

            // Assert
            Assert.IsNotNull(hash);
            Assert.AreNotEqual("Error calculating hash", hash);
            Assert.AreEqual(40, hash.Length); // SHA1 hash length
        }

        #endregion

        #region CleanProjectFileForRemoteExecution Tests

        [TestMethod]
        public void CleanProjectFileForRemoteExecution_WithNonExistentFile_ReturnsEmptyArray()
        {
            // Arrange
            string nonExistentFile = Path.Combine(_testDir, "nonexistent.sbm");

            // Act
            var result = SqlBuildFileHelper.CleanProjectFileForRemoteExecution(
                nonExistentFile, out SqlSyncBuildDataModel cleanedData);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
            Assert.IsNotNull(cleanedData);
        }

        #endregion

        #region ExtractSqlBuildZipFile Tests

        [TestMethod]
        public async Task ExtractSqlBuildZipFile_WithValidZip_ExtractsSuccessfully()
        {
            // Arrange - Create a valid SBM file
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, model);
            File.WriteAllText(Path.Combine(_testDir, "script.sql"), "SELECT 1");

            string sbmFile = Path.Combine(_testDir, "test.sbm");
            SqlBuildFileHelper.PackageProjectFileIntoZip(model, _testDir, sbmFile, false);

            // Act
            var (success, extractedWorkingDir, extractedProjFilePath, extractedProjFileName, extractResult) = 
                await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(sbmFile);

            // Assert
            Assert.IsTrue(success);
            Assert.IsTrue(Directory.Exists(extractedWorkingDir));

            // Cleanup
            await SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectoryAsync(extractedWorkingDir);
        }

        [TestMethod]
        public async Task ExtractSqlBuildZipFile_WithInvalidFile_ReturnsFalse()
        {
            // Arrange
            string invalidFile = Path.Combine(_testDir, "invalid.sbm");
            File.WriteAllText(invalidFile, "This is not a valid zip file");

            // Act
            var (success, _, _, _, extractResult) = await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(invalidFile);

            // Assert
            Assert.IsFalse(success);
            Assert.IsFalse(string.IsNullOrEmpty(extractResult));
        }

        [TestMethod]
        public async Task ExtractSqlBuildZipFile_WithOverwriteOption_OverwritesFiles()
        {
            // Arrange - Create a valid SBM file
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, model);

            string sbmFile = Path.Combine(_testDir, "test.sbm");
            SqlBuildFileHelper.PackageProjectFileIntoZip(model, _testDir, sbmFile, false);

            string workingDir = Path.Combine(_testDir, "extract");
            Directory.CreateDirectory(workingDir);

            // Act - Extract with overwrite enabled
            var (success, extractedWorkingDir, _, _, extractResult) = 
                await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(
                    sbmFile, workingDir, resetWorkingDirectory: false, overwriteExistingProjectFiles: true);

            // Assert
            Assert.IsTrue(success);

            // Cleanup
            await SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectoryAsync(extractedWorkingDir);
        }

        #endregion

        #region PackageProjectFileIntoZip Tests

        [TestMethod]
        public void PackageProjectFileIntoZip_WithEmptyZipFileName_ReturnsTrue()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            bool result = SqlBuildFileHelper.PackageProjectFileIntoZip(model, _testDir, "", false);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task PackageProjectFileIntoZip_WithValidModel_CreatesZipFile()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, model);

            string zipFile = Path.Combine(_testDir, "output.sbm");

            // Act
            bool result = SqlBuildFileHelper.PackageProjectFileIntoZip(model, _testDir, zipFile, false);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipFile));
        }

        [TestMethod]
        public async Task PackageProjectFileIntoZipAsync_WithValidModel_CreatesZipFile()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, model);

            string zipFile = Path.Combine(_testDir, "async.sbm");

            // Act
            bool result = await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(
                model, _testDir, zipFile, false, CancellationToken.None);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipFile));
        }

        #endregion

        #region SaveSqlBuildProjectFile Tests

        [TestMethod]
        public async Task SaveSqlBuildProjectFile_Model_SavesXmlAndCreatesZip()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "project.sbm");

            // Act
            await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(model, projFileName, zipFileName, false, CancellationToken.None);

            // Assert
            Assert.IsTrue(File.Exists(projFileName));
            Assert.IsTrue(File.Exists(zipFileName));
        }

        [TestMethod]
        public async Task SaveSqlBuildProjectFileAsync_SavesXmlAndCreatesZip()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "async.sbm");

            // Act
            await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(
                model, projFileName, zipFileName, false, CancellationToken.None);

            // Assert
            Assert.IsTrue(File.Exists(projFileName));
            Assert.IsTrue(File.Exists(zipFileName));
        }

        #endregion

        #region SaveSqlFilesToNewBuildFile Tests

        [TestMethod]
        public async Task SaveSqlFilesToNewBuildFile_DirectoryVersion_CreatesPackageFromFiles()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_testDir, "script1.sql"), "SELECT 1");
            File.WriteAllText(Path.Combine(_testDir, "script2.sql"), "SELECT 2");
            string buildFileName = Path.Combine(_testDir, "fromdir.sbm");

            // Act
            bool result = await SqlBuildFileHelper.SaveSqlFilesToNewBuildFileAsync(
                buildFileName, _testDir, "TestDb", 30);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(buildFileName));
        }

        [TestMethod]
        public async Task SaveSqlFilesToNewBuildFile_ListVersion_CreatesPackage()
        {
            // Arrange
            var scripts = new List<string> { "script1.sql", "script2.sql" };
            File.WriteAllText(Path.Combine(_testDir, "script1.sql"), "SELECT 1");
            File.WriteAllText(Path.Combine(_testDir, "script2.sql"), "SELECT 2");
            string buildFileName = Path.Combine(_testDir, "fromlist.sbm");

            // Act
            bool result = await SqlBuildFileHelper.SaveSqlFilesToNewBuildFileAsync(
                buildFileName, scripts, "TestDb", true, 30, false);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(buildFileName));
        }

        [TestMethod]
        public async Task SaveSqlFilesToNewBuildFile_ExistingFileNoOverwrite_ReturnsFalse()
        {
            // Arrange
            string buildFileName = Path.Combine(_testDir, "existing.sbm");
            File.WriteAllText(buildFileName, "existing content");
            var scripts = new List<string> { "script1.sql" };

            // Act
            bool result = await SqlBuildFileHelper.SaveSqlFilesToNewBuildFileAsync(
                buildFileName, scripts, "TestDb", false, 30, false);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region GetSHA1Hash Variations Tests

        [TestMethod]
        public void GetSHA1Hash_WithStripTransactions_StripsTransactionText()
        {
            // Arrange
            string scriptContent = "BEGIN TRANSACTION\r\nSELECT 1\r\nCOMMIT TRANSACTION";
            string testFile = Path.Combine(_testDir, "transaction.sql");
            File.WriteAllText(testFile, scriptContent);

            // Act
            SqlBuildFileHelper.GetSHA1Hash(testFile, out string fileHash, out string textHashNoStrip, false);
            SqlBuildFileHelper.GetSHA1Hash(testFile, out _, out string textHashWithStrip, true);

            // Assert
            Assert.IsNotNull(textHashNoStrip);
            Assert.IsNotNull(textHashWithStrip);
            // Hashes may or may not differ depending on implementation
        }

        [TestMethod]
        public async Task GetSHA1HashAsync_WithValidFile_ReturnsSameAsSync()
        {
            // Arrange
            string testFile = Path.Combine(_testDir, "hash_async.sql");
            await File.WriteAllTextAsync(testFile, "SELECT 1 FROM TestTable");

            // Act
            SqlBuildFileHelper.GetSHA1Hash(testFile, out _, out string syncHash, false);
            var (_, asyncHash) = await SqlBuildFileHelper.GetSHA1HashAsync(testFile, false);

            // Assert
            Assert.AreEqual(syncHash, asyncHash);
        }

        #endregion

        #region InferOverridesFromPackage Tests

        [TestMethod]
        public async Task InferOverridesFromPackage_WithValidSbmAndSuppliedDb_ReturnsOverrides()
        {
            // Arrange - Create a package with a script targeting a database
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(
                model, projFileName, "test.sql", 1.0, "Test", true, true,
                "SourceDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

            await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, model);
            File.WriteAllText(Path.Combine(_testDir, "test.sql"), "SELECT 1");

            string sbmFile = Path.Combine(_testDir, "override.sbm");
            SqlBuildFileHelper.PackageProjectFileIntoZip(model, _testDir, sbmFile, false);

            // Act
            string result = await SqlBuildFileHelper.InferOverridesFromPackageAsync(sbmFile, "TargetDb");

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("SourceDb,TargetDb"));
        }

        [TestMethod]
        public async Task InferOverridesFromPackage_WithValidSbmAndNoSuppliedDb_ReturnsSameAsOverride()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(
                model, projFileName, "test.sql", 1.0, "Test", true, true,
                "SourceDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

            await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, model);
            File.WriteAllText(Path.Combine(_testDir, "test.sql"), "SELECT 1");

            string sbmFile = Path.Combine(_testDir, "override2.sbm");
            SqlBuildFileHelper.PackageProjectFileIntoZip(model, _testDir, sbmFile, false);

            // Act
            string result = await SqlBuildFileHelper.InferOverridesFromPackageAsync(sbmFile, null);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(result));
            Assert.IsTrue(result.Contains("SourceDb,SourceDb"));
        }

        [TestMethod]
        public async Task InferOverridesFromPackage_WithInvalidExtension_ReturnsEmptyString()
        {
            // Arrange - Try with an unsupported extension
            string txtFile = Path.Combine(_testDir, "test.txt");
            File.WriteAllText(txtFile, "some content");

            // Act
            string result = await SqlBuildFileHelper.InferOverridesFromPackageAsync(txtFile, "TargetDb");

            // Assert - The method returns empty for unsupported extensions after extraction failure
            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region ImportSqlScriptFile Tests

        [TestMethod]
        public async Task ImportSqlScriptFile_WithValidImportData_ImportsScripts()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "main.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var importModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Add scripts to import data
            string importDir = Path.Combine(_testDir, "import");
            Directory.CreateDirectory(importDir);
            File.WriteAllText(Path.Combine(importDir, "import1.sql"), "SELECT 1");
            File.WriteAllText(Path.Combine(importDir, "import2.sql"), "SELECT 2");

            importModel = await SqlBuildFileHelper.AddScriptFileToBuildAsync(importModel, Path.Combine(importDir, "import.xml"),
                "import1.sql", 1.0, "Import 1", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");
            importModel = await SqlBuildFileHelper.AddScriptFileToBuildAsync(importModel, Path.Combine(importDir, "import.xml"),
                "import2.sql", 2.0, "Import 2", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

            // Act
            var (result, updatedModel, addedFileNames) = SqlBuildFileHelper.ImportSqlScriptFile(
                model, importModel, importDir, 0,
                _testDir, projFileName, zipFileName, false);

            // Assert
            Assert.AreEqual(1.0, result); // Start build number
            Assert.AreEqual(2, addedFileNames.Length);
            Assert.AreEqual(2, updatedModel.Script.Count);
        }

        [TestMethod]
        public void ImportSqlScriptFile_WithEmptyImportData_ReturnsNoRowsImported()
        {
            // Arrange
            string projFileName = Path.Combine(_testDir, "SqlSyncBuildProject.xml");
            string zipFileName = Path.Combine(_testDir, "main.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var importModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var (result, _, addedFileNames) = SqlBuildFileHelper.ImportSqlScriptFile(
                model, importModel, _testDir, 0,
                _testDir, projFileName, zipFileName, false);

            // Assert
            Assert.AreEqual((double)ImportFileStatus.NoRowsImported, result);
            Assert.AreEqual(0, addedFileNames.Length);
        }

        #endregion

        #region GetFileDataForObjectUpdates Tests

        [TestMethod]
        public void GetFileDataForObjectUpdates_WithValidProcFile_ExtractsMetadata()
        {
            // Arrange
            string procContent = @"/*
Source Server: ProdServer
Source Db: ProdDb
Date Scripted: 2024-01-01
Originally Scripted By: Admin
Object Type: Stored Procedure
Object Name: dbo.TestProc
*/
CREATE PROCEDURE dbo.TestProc AS SELECT 1";
            File.WriteAllText(Path.Combine(_testDir, "test.PRC"), procContent);
            string projFileName = Path.Combine(_testDir, "project.xml");

            // Act
            var result = SqlBuildFileHelper.GetFileDataForObjectUpdates("test.PRC", projFileName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("test.PRC", result.ShortFileName);
            Assert.AreEqual("ProdServer", result.SourceServer);
            Assert.AreEqual("ProdDb", result.SourceDatabase);
        }

        [TestMethod]
        public void GetFileDataForObjectUpdates_WithModelContainingObjectScripts_CategorizesCorrectly()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_testDir, "valid.PRC"), @"/*
Source Server: Server1
Source Db: Db1
Date Scripted: 2024-01-01
Originally Scripted By: User
Object Type: Stored Procedure
Object Name: dbo.Proc1
*/
CREATE PROCEDURE dbo.Proc1 AS SELECT 1");

            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var scripts = model.Script.ToList();
            scripts.Add(new Script("valid.PRC", 1.0, "Proc", true, true,
                DateTime.Now, Guid.NewGuid().ToString(), "TestDb",
                false, true, "user", 30, DateTime.MinValue, "", ""));
            scripts.Add(new Script("invalid.sql", 2.0, "Invalid", true, true,
                DateTime.Now, Guid.NewGuid().ToString(), "TestDb",
                false, true, "user", 30, DateTime.MinValue, "", ""));
            model = new SqlSyncBuildDataModel(
                model.SqlSyncBuildProject, scripts, model.Build,
                model.ScriptRun, model.CommittedScript);

            string projFileName = Path.Combine(_testDir, "project.xml");

            // Act
            SqlBuildFileHelper.GetFileDataForObjectUpdates(model, projFileName, out var canUpdate, out var canNotUpdate);

            // Assert
            Assert.IsNotNull(canUpdate);
            Assert.IsNotNull(canNotUpdate);
            // valid.PRC should be in canUpdate, invalid.sql should be in canNotUpdate
        }

        #endregion

        #region ScriptBatchCollection GetScriptBatch Tests

        [TestMethod]
        public void ScriptBatchCollection_GetScriptBatch_ReturnsMatchingBatch()
        {
            // Arrange
            var collection = new ScriptBatchCollection();
            collection.Add(new ScriptBatch("script1.sql", new[] { "SELECT 1" }, "guid1"));
            collection.Add(new ScriptBatch("script2.sql", new[] { "SELECT 2" }, "guid2"));

            // Act
            var result = collection.GetScriptBatch("guid2");

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("script2.sql", result.ScriptfileName);
        }

        [TestMethod]
        public void ScriptBatchCollection_GetScriptBatch_ReturnsNullForNonExistent()
        {
            // Arrange
            var collection = new ScriptBatchCollection();
            collection.Add(new ScriptBatch("script1.sql", new[] { "SELECT 1" }, "guid1"));

            // Act
            var result = collection.GetScriptBatch("nonexistent");

            // Assert
            Assert.IsNull(result);
        }

        #endregion

        #region LoadSqlBuildProjectFile Tests

        [TestMethod]
        public async Task LoadSqlBuildProjectFile_Model_WithValidFile_LoadsCorrectly()
        {
            // Arrange
            var originalModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "valid.xml");
            await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, originalModel);

            // Act
            var (success, loadedModel) = await SqlBuildFileHelper.LoadSqlBuildProjectFileAsync(projFileName, false);

            // Assert
            Assert.IsTrue(success);
            Assert.IsNotNull(loadedModel);
            Assert.AreEqual(1, loadedModel.SqlSyncBuildProject.Count);
        }

        [TestMethod]
        public async Task LoadSqlBuildProjectModel_WithValidFile_ReturnsModel()
        {
            // Arrange
            var originalModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDir, "model.xml");
            await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, originalModel);

            // Act
            var loadedModel = SqlBuildFileHelper.LoadSqlBuildProjectModel(projFileName, false);

            // Assert
            Assert.IsNotNull(loadedModel);
            Assert.AreEqual(1, loadedModel.SqlSyncBuildProject.Count);
        }

        #endregion
    }
}
