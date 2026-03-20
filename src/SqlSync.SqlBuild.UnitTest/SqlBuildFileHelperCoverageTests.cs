using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// Additional tests for SqlBuildFileHelper to improve code coverage
    /// </summary>
    [TestClass]
    public class SqlBuildFileHelperCoverageTests
    {
        private string _testDirectory = string.Empty;

        [TestInitialize]
        public void Setup()
        {
            _testDirectory = Path.Combine(Path.GetTempPath(), "SqlBuildFileHelperCoverageTests_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_testDirectory);
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_testDirectory))
            {
                try { Directory.Delete(_testDirectory, true); } catch { }
            }
        }

        #region ValidateAgainstSchema Tests

        [TestMethod]
        public void ValidateAgainstSchema_AlwaysReturnsTrue()
        {
            // Arrange
            string fileName = Path.Combine(_testDirectory, "test.xml");
            File.WriteAllText(fileName, "<root></root>");

            // Act
            bool result = SqlBuildFileHelper.ValidateAgainstSchema(fileName, out string validationErrorMessage);

            // Assert - the method always returns true per the obsolete implementation
            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, validationErrorMessage);
        }

        #endregion

        #region LoadSqlBuildProjectFile Tests

        [TestMethod]
        public async Task LoadSqlBuildProjectFile_NonExistentFile_ReturnsFalseAndCreatesShellModel()
        {
            // Arrange
            string projFileName = Path.Combine(_testDirectory, "NonExistent.xml");

            // Act
            var (result, model) = await SqlBuildFileHelper.LoadSqlBuildProjectFileAsync(projFileName, false);

            // Assert
            Assert.IsFalse(result);
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
        }

        [TestMethod]
        public async Task LoadSqlBuildProjectModel_NonExistentFile_ReturnsShellModel()
        {
            // Arrange
            string projFileName = Path.Combine(_testDirectory, "NonExistent.xml");

            // Act
            var model = await SqlBuildFileHelper.LoadSqlBuildProjectModelAsync(projFileName, false);

            // Assert
            Assert.IsNotNull(model);
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
        }

        [TestMethod]
        public async Task LoadSqlBuildProjectFile_ExistingValidFile_ReturnsTrue()
        {
            // Arrange
            string projFileName = Path.Combine(_testDirectory, XmlFileNames.MainProjectFile);
            var shellModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            await SqlSyncBuildDataXmlSerializer.SaveAsync(projFileName, shellModel);

            // Act
            var (result, model) = await SqlBuildFileHelper.LoadSqlBuildProjectFileAsync(projFileName, false);

            // Assert
            Assert.IsTrue(result);
            Assert.IsNotNull(model);
        }

        #endregion

        #region PackageProjectFileIntoZip Tests

        [TestMethod]
        public async Task PackageProjectFileIntoZip_EmptyZipFileName_ReturnsTrue()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            bool result = await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(model, _testDirectory, string.Empty, false);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task PackageProjectFileIntoZip_NullModel_ReturnsFalse()
        {
            // Arrange
            SqlSyncBuildDataModel model = null!;

            // Act
            bool result = await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(model, _testDirectory, "test.sbm", false);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task PackageProjectFileIntoZip_ValidModel_CreatesZipFile()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string zipFileName = Path.Combine(_testDirectory, "test.sbm");

            // Act
            bool result = await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(model, _testDirectory, zipFileName, false);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(zipFileName));
        }

        [TestMethod]
        public async Task PackageProjectFileIntoZipAsync_EmptyZipFileName_ReturnsTrue()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            bool result = await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(model, _testDirectory, string.Empty, false);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task PackageProjectFileIntoZipAsync_NullModel_ReturnsFalse()
        {
            // Arrange
            SqlSyncBuildDataModel model = null!;

            // Act
            bool result = await SqlBuildFileHelper.PackageProjectFileIntoZipAsync(model, _testDirectory, "test.sbm", false);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region SaveSqlBuildProjectFile Tests

        [TestMethod]
        public async Task SaveSqlBuildProjectFile_ValidModel_SavesFiles()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDirectory, XmlFileNames.MainProjectFile);
            string zipFileName = Path.Combine(_testDirectory, "test.sbm");

            // Act
            await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(model, projFileName, zipFileName, false);

            // Assert
            Assert.IsTrue(File.Exists(projFileName));
            Assert.IsTrue(File.Exists(zipFileName));
        }

        [TestMethod]
        public async Task SaveSqlBuildProjectFileAsync_ValidModel_SavesFiles()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDirectory, XmlFileNames.MainProjectFile);
            string zipFileName = Path.Combine(_testDirectory, "testAsync.sbm");

            // Act
            await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(model, projFileName, zipFileName, false);

            // Assert
            Assert.IsTrue(File.Exists(projFileName));
            Assert.IsTrue(File.Exists(zipFileName));
        }

        #endregion

        #region SaveSqlFilesToNewBuildFile Tests

        [TestMethod]
        public async Task SaveSqlFilesToNewBuildFile_ExistingFileNoOverwrite_ReturnsFalse()
        {
            // Arrange
            string buildFileName = Path.Combine(_testDirectory, "existing.sbm");
            File.WriteAllText(buildFileName, "existing content");
            var fileNames = new List<string>();

            // Act
            bool result = await SqlBuildFileHelper.SaveSqlFilesToNewBuildFileAsync(buildFileName, fileNames, "TestDb", false, 30, false);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task SaveSqlFilesToNewBuildFile_ValidInput_CreatesFile()
        {
            // Arrange
            string buildFileName = Path.Combine(_testDirectory, "new.sbm");
            string sqlFile = Path.Combine(_testDirectory, "test.sql");
            File.WriteAllText(sqlFile, "SELECT 1");
            var fileNames = new List<string> { sqlFile };

            // Act
            bool result = await SqlBuildFileHelper.SaveSqlFilesToNewBuildFileAsync(buildFileName, fileNames, "TestDb", true, 30, false);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(buildFileName));
        }

        [TestMethod]
        public async Task SaveSqlFilesToNewBuildFile_SkipsMainProjectFile()
        {
            // Arrange
            string buildFileName = Path.Combine(_testDirectory, "new.sbm");
            var fileNames = new List<string> { XmlFileNames.MainProjectFile, XmlFileNames.ExportFile };

            // Act
            bool result = await SqlBuildFileHelper.SaveSqlFilesToNewBuildFileAsync(buildFileName, fileNames, "TestDb", true, 30, false);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region AddScriptFileToBuild Tests

        [TestMethod]
        public async Task AddScriptFileToBuild_Model_AddsScriptToModel()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDirectory, XmlFileNames.MainProjectFile);
            int initialCount = model.Script.Count;

            // Act
            var updatedModel = await SqlBuildFileHelper.AddScriptFileToBuildAsync(
                model,
                projFileName,
                "test.sql",
                1.0,
                "Test description",
                true,
                true,
                "TestDb",
                false,
                string.Empty,
                false,
                true,
                "TestUser",
                30,
                Guid.NewGuid(),
                "v1.0");

            // Assert
            Assert.AreEqual(initialCount + 1, updatedModel.Script.Count);
            Assert.AreEqual("test.sql", updatedModel.Script[0].FileName);
            Assert.AreEqual("Test description", updatedModel.Script[0].Description);
            Assert.AreEqual("TestDb", updatedModel.Script[0].Database);
            Assert.AreEqual("v1.0", updatedModel.Script[0].Tag);
        }

        [TestMethod]
        public async Task AddScriptFileToBuild_Model_WithEmptyGuid_GeneratesNewGuid()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string projFileName = Path.Combine(_testDirectory, XmlFileNames.MainProjectFile);

            // Act
            var updatedModel = await SqlBuildFileHelper.AddScriptFileToBuildAsync(
                model, projFileName, "test.sql", 1.0, "Description",
                true, true, "TestDb", false, string.Empty, false, true,
                "TestUser", 30, Guid.Empty, string.Empty);

            // Assert
            Assert.IsNotNull(updatedModel.Script[0].ScriptId);
            Assert.AreNotEqual(Guid.Empty.ToString(), updatedModel.Script[0].ScriptId);
        }

        #endregion

        #region GetInsertedIndexValues Tests

        [TestMethod]
        public void GetInsertedIndexValues_WholeValuesAvailable_ReturnsWholeNumbers()
        {
            // Act
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 10.0, 3);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2.0, result[0]);
            Assert.AreEqual(3.0, result[1]);
            Assert.AreEqual(4.0, result[2]);
        }

        [TestMethod]
        public void GetInsertedIndexValues_TenthValuesNeeded_ReturnsTenthValues()
        {
            // Act - floor and ceiling close together
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 2.0, 3);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(1.1, result[0]);
            Assert.AreEqual(1.2, result[1]);
            Assert.AreEqual(1.3, result[2]);
        }

        [TestMethod]
        public void GetInsertedIndexValues_FractionalStep_CalculatesCorrectly()
        {
            // Act - very small range
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 1.05, 3);

            // Assert
            Assert.AreEqual(3, result.Count);
            // Values should be calculated with fractional steps
            Assert.IsTrue(result.All(v => v > 1.0 && v < 1.05));
        }

        [TestMethod]
        public void GetInsertedIndexValues_SingleInsert_ReturnsOneValue()
        {
            // Act
            var result = SqlBuildFileHelper.GetInsertedIndexValues(5.0, 10.0, 1);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(6.0, result[0]);
        }

        #endregion

        #region CleanUpAndDeleteWorkingDirectory Tests

        [TestMethod]
        public async Task CleanUpAndDeleteWorkingDirectory_ExistingDirectory_ReturnsTrue()
        {
            // Arrange
            string testDir = Path.Combine(_testDirectory, "ToDelete");
            Directory.CreateDirectory(testDir);

            // Act
            bool result = await SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectoryAsync(testDir);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(Directory.Exists(testDir));
        }

        [TestMethod]
        public async Task CleanUpAndDeleteWorkingDirectory_NonExistentDirectory_ReturnsTrue()
        {
            // Arrange
            string testDir = Path.Combine(_testDirectory, "DoesNotExist");

            // Act
            bool result = await SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectoryAsync(testDir);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region InitializeWorkingDirectory Tests

        [TestMethod]
        public async Task InitializeWorkingDirectory_CreatesDirectory()
        {
            // Act
            var (success, workingDirectory, projectFilePath, projectFileName) = await SqlBuildFileHelper.InitializeWorkingDirectoryAsync();

            // Assert
            Assert.IsTrue(success);
            Assert.IsTrue(Directory.Exists(workingDirectory));
            Assert.AreEqual(workingDirectory, projectFilePath);
            
            // Cleanup
            await SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectoryAsync(workingDirectory);
        }

        #endregion

        #region MakeFileWriteable Tests

        [TestMethod]
        public void MakeFileWriteable_ExistingFile_ReturnsTrue()
        {
            // Arrange
            string fileName = Path.Combine(_testDirectory, "readonly.txt");
            File.WriteAllText(fileName, "test");
            File.SetAttributes(fileName, FileAttributes.ReadOnly);

            // Act
            bool result = SqlBuildFileHelper.MakeFileWriteable(fileName);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(FileAttributes.Normal, File.GetAttributes(fileName));
        }

        [TestMethod]
        public void MakeFileWriteable_NonExistentFile_ReturnsFalse()
        {
            // Arrange
            string fileName = Path.Combine(_testDirectory, "doesnotexist.txt");

            // Act
            bool result = SqlBuildFileHelper.MakeFileWriteable(fileName);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region ScriptRequiresBuildDescription Tests

        [TestMethod]
        public void ScriptRequiresBuildDescription_ContainsToken_ReturnsTrue()
        {
            // Arrange
            string script = "-- Some script with " + ScriptTokens.BuildDescription + " token";

            // Act
            bool result = SqlBuildFileHelper.ScriptRequiresBuildDescription(script);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ScriptRequiresBuildDescription_NoToken_ReturnsFalse()
        {
            // Arrange
            string script = "SELECT * FROM Table1";

            // Act
            bool result = SqlBuildFileHelper.ScriptRequiresBuildDescription(script);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ScriptRequiresBuildDescription_NullString_ReturnsFalse()
        {
            // Act
            bool result = SqlBuildFileHelper.ScriptRequiresBuildDescription(null!);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ScriptRequiresBuildDescription_EmptyString_ReturnsFalse()
        {
            // Act
            bool result = SqlBuildFileHelper.ScriptRequiresBuildDescription(string.Empty);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region GetTotalLogFilesSize Tests

        [TestMethod]
        public void GetTotalLogFilesSize_EmptyDirectory_ReturnsZero()
        {
            // Act
            long result = SqlBuildFileHelper.GetTotalLogFilesSize(_testDirectory);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetTotalLogFilesSize_WithLogFiles_ReturnsTotalSize()
        {
            // Arrange
            string logFile1 = Path.Combine(_testDirectory, "test1.log");
            string logFile2 = Path.Combine(_testDirectory, "test2.log");
            File.WriteAllText(logFile1, "Log content 1");
            File.WriteAllText(logFile2, "Log content 2");

            // Act
            long result = SqlBuildFileHelper.GetTotalLogFilesSize(_testDirectory);

            // Assert
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void GetTotalLogFilesSize_NonExistentDirectory_ReturnsZero()
        {
            // Act
            long result = SqlBuildFileHelper.GetTotalLogFilesSize(Path.Combine(_testDirectory, "nonexistent"));

            // Assert
            Assert.AreEqual(0, result);
        }

        #endregion

        #region SHA1 Hash Tests

        [TestMethod]
        public void GetSHA1Hash_ValidFile_ReturnsHashes()
        {
            // Arrange
            string fileName = Path.Combine(_testDirectory, "hashtest.sql");
            File.WriteAllText(fileName, "SELECT 1");

            // Act
            SqlBuildFileHelper.GetSHA1Hash(fileName, out string fileHash, out string textHash, false);

            // Assert
            Assert.IsNotNull(fileHash);
            Assert.IsNotNull(textHash);
            Assert.AreNotEqual(SqlBuildFileHelper.FileMissing, fileHash);
            Assert.AreNotEqual(SqlBuildFileHelper.Sha1HashError, fileHash);
        }

        [TestMethod]
        public void GetSHA1Hash_NonExistentFile_ReturnsFileMissing()
        {
            // Arrange
            string fileName = Path.Combine(_testDirectory, "nonexistent.sql");

            // Act
            SqlBuildFileHelper.GetSHA1Hash(fileName, out string fileHash, out string textHash, false);

            // Assert
            Assert.AreEqual(SqlBuildFileHelper.FileMissing, fileHash);
            Assert.AreEqual(SqlBuildFileHelper.FileMissing, textHash);
        }

        [TestMethod]
        public async Task GetSHA1HashAsync_ValidFile_ReturnsHashes()
        {
            // Arrange
            string fileName = Path.Combine(_testDirectory, "hashtestAsync.sql");
            File.WriteAllText(fileName, "SELECT 1");

            // Act
            var (fileHash, textHash) = await SqlBuildFileHelper.GetSHA1HashAsync(fileName, false);

            // Assert
            Assert.IsNotNull(fileHash);
            Assert.IsNotNull(textHash);
            Assert.AreNotEqual(SqlBuildFileHelper.FileMissing, fileHash);
        }

        [TestMethod]
        public async Task GetSHA1HashAsync_NonExistentFile_ReturnsFileMissing()
        {
            // Arrange
            string fileName = Path.Combine(_testDirectory, "nonexistentAsync.sql");

            // Act
            var (fileHash, textHash) = await SqlBuildFileHelper.GetSHA1HashAsync(fileName, false);

            // Assert
            Assert.AreEqual(SqlBuildFileHelper.FileMissing, fileHash);
            Assert.AreEqual(SqlBuildFileHelper.FileMissing, textHash);
        }

        [TestMethod]
        public async Task CalculateSha1HashFromPackage_EmptyFileName_ReturnsEmptyString()
        {
            // Act
            string result = await SqlBuildFileHelper.CalculateSha1HashFromPackageAsync(string.Empty);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public async Task CalculateSha1HashFromPackage_InvalidExtension_ReturnsEmptyString()
        {
            // Arrange
            string fileName = Path.Combine(_testDirectory, "test.txt");
            File.WriteAllText(fileName, "content");

            // Act
            string result = await SqlBuildFileHelper.CalculateSha1HashFromPackageAsync(fileName);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region UpdateObsoleteXmlNamespace Tests

        [TestMethod]
        public void UpdateObsoleteXmlNamespace_ValidFile_UpdatesNamespace()
        {
            // Arrange
            string fileName = Path.Combine(_testDirectory, "obsolete.xml");
            File.WriteAllText(fileName, "<root xmlns=\"http://oldnamespace.com/test\"></root>");

            // Act
            bool result = SqlBuildFileHelper.UpdateObsoleteXmlNamespace(fileName);

            // Assert
            Assert.IsTrue(result);
            string contents = File.ReadAllText(fileName);
            Assert.IsTrue(contents.Contains("xmlns=\"http://schemas.mckechney.com/"));
        }

        [TestMethod]
        public void UpdateObsoleteXmlNamespace_AlreadyCorrect_ReturnsFalse()
        {
            // Arrange
            string fileName = Path.Combine(_testDirectory, "current.xml");
            File.WriteAllText(fileName, "<root xmlns=\"http://schemas.mckechney.com/test\"></root>");

            // Act
            bool result = SqlBuildFileHelper.UpdateObsoleteXmlNamespace(fileName);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region RenumberBuildSequence Tests

        [TestMethod]
        public async Task RenumberBuildSequence_ValidBuildData_ReturnsNotNull()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, Path.Combine(_testDirectory, "test.xml"),
                "script1.sql", 5, "desc", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(model, Path.Combine(_testDirectory, "test.xml"),
                "script2.sql", 3, "desc", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

            File.WriteAllText(Path.Combine(_testDirectory, "script1.sql"), "SELECT 1");
            File.WriteAllText(Path.Combine(_testDirectory, "script2.sql"), "SELECT 2");

            string projFileName = Path.Combine(_testDirectory, XmlFileNames.MainProjectFile);
            string zipFileName = Path.Combine(_testDirectory, "test.sbm");

            // Act
            var result = await SqlBuildFileHelper.RenumberBuildSequenceAsync(model, projFileName, zipFileName);

            // Assert
            Assert.IsNotNull(result);
        }

        #endregion

        #region ExtractSqlBuildZipFile Tests

        [TestMethod]
        public async Task ExtractSqlBuildZipFile_NonExistentFile_ReturnsFalse()
        {
            // Act
            var (success, workingDirectory, projectFilePath, projectFileName, resultMessage) = 
                await SqlBuildFileHelper.ExtractSqlBuildZipFileAsync(Path.Combine(_testDirectory, "nonexistent.sbm"));

            // Assert
            Assert.IsFalse(success);
        }

        #endregion

        #region PackageSbxFilesIntoSbmFiles Tests

        [TestMethod]
        public async Task PackageSbxFilesIntoSbmFiles_EmptyDirectory_ReturnsEmptyList()
        {
            // Act
            var result = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(string.Empty);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task PackageSbxFilesIntoSbmFiles_NonExistentDirectory_ReturnsEmptyList()
        {
            // Act
            var result = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(
                Path.Combine(_testDirectory, "nonexistent"));

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public async Task PackageSbxFilesIntoSbmFiles_NoSbxFiles_ReturnsEmptyList()
        {
            // Act
            var result = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(_testDirectory);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region GetFileDataForCodeTableUpdates Tests

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_NullBuildData_ReturnsNull()
        {
            // Arrange
            SqlSyncBuildDataModel model = null!;

            // Act
            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates(model, _testDirectory);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_NonExistentFile_ReturnsNull()
        {
            // Act
            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates("nonexistent.POP", _testDirectory);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_ValidPopFile_ReturnsUpdates()
        {
            // Arrange
            string projFileName = Path.Combine(_testDirectory, XmlFileNames.MainProjectFile);
            string popFile = Path.Combine(_testDirectory, "test.POP");
            File.WriteAllText(popFile, @"/*
Source Server: TestServer
Source Db: TestDb
Table Scripted: TestTable
Key Check Columns: Col1,Col2
Query Used:
SELECT * FROM TestTable
*/
INSERT INTO TestTable VALUES (1, 'test');");

            // Act
            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates("test.POP", projFileName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("test.POP", result.ShortFileName);
            Assert.AreEqual("TestServer", result.SourceServer);
            Assert.AreEqual("TestDb", result.SourceDatabase);
            Assert.AreEqual("TestTable", result.SourceTable);
            Assert.AreEqual("Col1,Col2", result.KeyCheckColumns);
        }

        #endregion

        #region GetFileDataForObjectUpdates Tests

        [TestMethod]
        public void GetFileDataForObjectUpdates_NullBuildData_ReturnsNull()
        {
            // Arrange
            SqlSyncBuildDataModel model = null!;

            // Act
            var result = SqlBuildFileHelper.GetFileDataForObjectUpdates(model, _testDirectory);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFileDataForObjectUpdates_NullModel_ReturnsNull()
        {
            // Arrange
            SqlSyncBuildDataModel model = null!;

            // Act
            var result = SqlBuildFileHelper.GetFileDataForObjectUpdates(model, _testDirectory);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFileDataForObjectUpdates_NonExistentFile_ReturnsNull()
        {
            // Act
            var result = SqlBuildFileHelper.GetFileDataForObjectUpdates("nonexistent.PRC", _testDirectory);

            // Assert
            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFileDataForObjectUpdates_ValidPrcFile_ReturnsUpdates()
        {
            // Arrange
            string projFileName = Path.Combine(_testDirectory, XmlFileNames.MainProjectFile);
            string prcFile = Path.Combine(_testDirectory, "testproc.PRC");
            File.WriteAllText(prcFile, @"/*
Source Server: TestServer
Source Db: TestDb
Object Scripted: dbo.TestProc
Object Type: StoredProcedure
Include Permissions: True
Script as ALTER: False
Script PK with Table: False
*/
CREATE PROCEDURE dbo.TestProc AS SELECT 1;");

            // Act
            var result = SqlBuildFileHelper.GetFileDataForObjectUpdates("testproc.PRC", projFileName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("testproc.PRC", result.ShortFileName);
            Assert.AreEqual("TestServer", result.SourceServer);
            Assert.AreEqual("TestDb", result.SourceDatabase);
            Assert.AreEqual("dbo.TestProc", result.SourceObject);
            Assert.AreEqual("StoredProcedure", result.ObjectType);
            Assert.IsTrue(result.IncludePermissions);
            Assert.IsFalse(result.ScriptAsAlter);
        }

        #endregion

        #region GetFileDataForObjectUpdates With Model Tests

        [TestMethod]
        public void GetFileDataForObjectUpdates_ModelWithInvalidExtensions_AddsToCanNotUpdate()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var scripts = new List<Script>
            {
                new Script("test.sql", 1, "desc", true, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, true, "user", 30, DateTime.MinValue, "", ""),
                new Script("test.txt", 2, "desc", true, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, true, "user", 30, DateTime.MinValue, "", "")
            };
            model = new SqlSyncBuildDataModel(model.SqlSyncBuildProject, scripts, model.Build, model.ScriptRun, model.CommittedScript);

            // Act
            SqlBuildFileHelper.GetFileDataForObjectUpdates(model, _testDirectory, out var canUpdate, out var canNotUpdate);

            // Assert
            Assert.IsNotNull(canNotUpdate);
            Assert.AreEqual(2, canNotUpdate.Count);
        }

        #endregion

        #region InferOverridesFromPackage Tests

        [TestMethod]
        public async Task InferOverridesFromPackage_ValidSbm_ReturnsOverrides()
        {
            // Arrange - create a valid SBM package
            string sbmFile = Path.Combine(_testDirectory, "override_test.sbm");
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model = await SqlBuildFileHelper.AddScriptFileToBuildAsync(
                model, 
                Path.Combine(_testDirectory, XmlFileNames.MainProjectFile),
                "test.sql", 1, "test", true, true, "SourceDb", false, string.Empty, false, true, "user", 30, Guid.NewGuid(), "");
            
            string projFileName = Path.Combine(_testDirectory, XmlFileNames.MainProjectFile);
            File.WriteAllText(Path.Combine(_testDirectory, "test.sql"), "SELECT 1");
            await SqlBuildFileHelper.SaveSqlBuildProjectFileAsync(model, projFileName, sbmFile, false);

            // Act
            string result = await SqlBuildFileHelper.InferOverridesFromPackageAsync(sbmFile, "TargetDb");

            // Assert
            Assert.IsTrue(result.Contains("SourceDb,TargetDb"));
        }

        #endregion

        #region JoinBatchedScripts Tests

        [TestMethod]
        public void JoinBatchedScripts_ValidArray_JoinsCorrectly()
        {
            // Arrange
            string[] batches = new[] { "SELECT 1", "SELECT 2", "SELECT 3" };

            // Act
            string result = SqlBuildFileHelper.JoinBatchedScripts(batches);

            // Assert
            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("SELECT 1"));
            Assert.IsTrue(result.Contains("SELECT 2"));
            Assert.IsTrue(result.Contains("SELECT 3"));
        }

        #endregion
    }
}
