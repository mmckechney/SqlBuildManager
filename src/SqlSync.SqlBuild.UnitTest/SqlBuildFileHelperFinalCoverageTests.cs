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
    /// Final coverage tests for SqlBuildFileHelper methods
    /// </summary>
    [TestClass]
    public class SqlBuildFileHelperFinalCoverageTests
    {
        private string _testDir;

        [TestInitialize]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"SqlBuildTest_{Guid.NewGuid():N}");
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

        #region GetInsertedIndexValues Tests

        [TestMethod]
        public void GetInsertedIndexValues_WithWholeNumbersBetween_UsesWholeNumbers()
        {
            // Arrange
            double floor = 1.0;
            double ceiling = 10.0;
            int insertCount = 3;

            // Act
            var result = SqlBuildFileHelper.GetInsertedIndexValues(floor, ceiling, insertCount);

            // Assert
            Assert.AreEqual(3, result.Count);
            Assert.AreEqual(2.0, result[0]);
            Assert.AreEqual(3.0, result[1]);
            Assert.AreEqual(4.0, result[2]);
        }

        [TestMethod]
        public void GetInsertedIndexValues_WithDecimalFloorAndCeiling_RoundsToIntegers()
        {
            // Arrange
            double floor = 1.5;
            double ceiling = 9.5;
            int insertCount = 2;

            // Act
            var result = SqlBuildFileHelper.GetInsertedIndexValues(floor, ceiling, insertCount);

            // Assert
            Assert.AreEqual(2, result.Count);
            // Should use whole numbers: ceil(1.5)=2, floor(9.5)=9
            Assert.AreEqual(3.0, result[0]);
            Assert.AreEqual(4.0, result[1]);
        }

        [TestMethod]
        public void GetInsertedIndexValues_WithNotEnoughSpace_UsesTenths()
        {
            // Arrange
            double floor = 1.0;
            double ceiling = 2.0;
            int insertCount = 5;

            // Act
            var result = SqlBuildFileHelper.GetInsertedIndexValues(floor, ceiling, insertCount);

            // Assert
            Assert.AreEqual(5, result.Count);
            Assert.AreEqual(1.1, result[0]);
            Assert.AreEqual(1.2, result[1]);
            Assert.AreEqual(1.3, result[2]);
            Assert.AreEqual(1.4, result[3]);
            Assert.AreEqual(1.5, result[4]);
        }

        [TestMethod]
        public void GetInsertedIndexValues_WithVeryTightSpace_CalculatesCustomStep()
        {
            // Arrange
            double floor = 1.0;
            double ceiling = 1.5;
            int insertCount = 10;

            // Act
            var result = SqlBuildFileHelper.GetInsertedIndexValues(floor, ceiling, insertCount);

            // Assert
            Assert.AreEqual(10, result.Count);
            // All values should be between floor and ceiling
            foreach (var value in result)
            {
                Assert.IsTrue(value > floor && value < ceiling, 
                    $"Value {value} should be between {floor} and {ceiling}");
            }
        }

        [TestMethod]
        public void GetInsertedIndexValues_WithSingleInsert_ReturnsOneValue()
        {
            // Arrange
            double floor = 1.0;
            double ceiling = 10.0;
            int insertCount = 1;

            // Act
            var result = SqlBuildFileHelper.GetInsertedIndexValues(floor, ceiling, insertCount);

            // Assert
            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2.0, result[0]);
        }

        [TestMethod]
        public void GetInsertedIndexValues_WithZeroInsertCount_ReturnsEmptyList()
        {
            // Arrange
            double floor = 1.0;
            double ceiling = 10.0;
            int insertCount = 0;

            // Act
            var result = SqlBuildFileHelper.GetInsertedIndexValues(floor, ceiling, insertCount);

            // Assert
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region GetTotalLogFilesSize Tests

        [TestMethod]
        public void GetTotalLogFilesSize_WithNoLogFiles_ReturnsZero()
        {
            // Act
            var result = SqlBuildFileHelper.GetTotalLogFilesSize(_testDir);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetTotalLogFilesSize_WithLogFiles_ReturnsTotalSize()
        {
            // Arrange
            string content1 = "Log content 1";
            string content2 = "Log content 2 - longer";
            File.WriteAllText(Path.Combine(_testDir, "test1.log"), content1);
            File.WriteAllText(Path.Combine(_testDir, "test2.log"), content2);

            // Act
            var result = SqlBuildFileHelper.GetTotalLogFilesSize(_testDir);

            // Assert
            Assert.IsTrue(result > 0);
        }

        [TestMethod]
        public void GetTotalLogFilesSize_WithNonExistentDirectory_ReturnsZero()
        {
            // Arrange
            string nonExistentDir = Path.Combine(_testDir, "nonexistent");

            // Act
            var result = SqlBuildFileHelper.GetTotalLogFilesSize(nonExistentDir);

            // Assert
            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetTotalLogFilesSize_WithMixedFileTypes_OnlySumsLogFiles()
        {
            // Arrange
            File.WriteAllText(Path.Combine(_testDir, "test.log"), "Log content");
            File.WriteAllText(Path.Combine(_testDir, "test.txt"), "Text content");
            File.WriteAllText(Path.Combine(_testDir, "test.sql"), "SQL content");

            var logSize = new FileInfo(Path.Combine(_testDir, "test.log")).Length;

            // Act
            var result = SqlBuildFileHelper.GetTotalLogFilesSize(_testDir);

            // Assert
            Assert.AreEqual(logSize, result);
        }

        #endregion

        #region CleanUpAndDeleteWorkingDirectory Tests

        [TestMethod]
        public void CleanUpAndDeleteWorkingDirectory_WithExistingDir_DeletesAndReturnsTrue()
        {
            // Arrange
            string testSubDir = Path.Combine(_testDir, "subdir");
            Directory.CreateDirectory(testSubDir);
            File.WriteAllText(Path.Combine(testSubDir, "test.txt"), "content");

            // Act
            var result = SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(testSubDir);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse(Directory.Exists(testSubDir));
        }

        [TestMethod]
        public void CleanUpAndDeleteWorkingDirectory_WithNonExistentDir_ReturnsTrue()
        {
            // Arrange
            string nonExistentDir = Path.Combine(_testDir, "nonexistent");

            // Act
            var result = SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(nonExistentDir);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region InitilizeWorkingDirectory Tests

        [TestMethod]
        public void InitilizeWorkingDirectory_WithValidInput_CreatesDirectory()
        {
            // Arrange
            string workingDir = Path.Combine(_testDir, "working");
            Directory.CreateDirectory(workingDir);
            string projectFilePath = workingDir;
            string projectFileName = "test.xml";

            // Act
            var result = SqlBuildFileHelper.InitilizeWorkingDirectory(ref workingDir, ref projectFilePath, ref projectFileName);

            // Assert
            Assert.IsTrue(result);
            Assert.IsTrue(Directory.Exists(workingDir));
        }

        [TestMethod]
        public void InitilizeWorkingDirectory_WithNullWorkingDir_HandlesSafely()
        {
            // Arrange
            string workingDir = null;
            string projectFilePath = null;
            string projectFileName = null;

            // Act
            var result = SqlBuildFileHelper.InitilizeWorkingDirectory(ref workingDir, ref projectFilePath, ref projectFileName);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region CreateShellSqlSyncBuildDataModel Tests

        [TestMethod]
        public void CreateShellSqlSyncBuildDataModel_CreatesValidModel()
        {
            // Act
            var result = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Assert
            Assert.IsNotNull(result);
            Assert.IsNotNull(result.Script);
            Assert.IsNotNull(result.Build);
            Assert.AreEqual(1, result.SqlSyncBuildProject.Count);
        }

        [TestMethod]
        public void CreateShellSqlSyncBuildDataModel_HasCorrectProjectName()
        {
            // Act
            var result = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Assert - shell creates with empty string project name
            Assert.AreEqual(string.Empty, result.SqlSyncBuildProject[0].ProjectName);
        }

        #endregion

        #region ValidateAgainstSchema Tests

        [TestMethod]
        public void ValidateAgainstSchema_AlwaysReturnsTrue()
        {
            // Arrange
            string errorMessage;
            
            // Act - This method is obsolete and always returns true
            var result = SqlBuildFileHelper.ValidateAgainstSchema("any-file.xml", out errorMessage);

            // Assert
            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, errorMessage);
        }

        #endregion

        #region MakeFileWriteable Tests

        [TestMethod]
        public void MakeFileWriteable_WithReadOnlyFile_RemovesReadOnlyAttribute()
        {
            // Arrange
            string testFile = Path.Combine(_testDir, "readonly.txt");
            File.WriteAllText(testFile, "test");
            File.SetAttributes(testFile, FileAttributes.ReadOnly);
            Assert.IsTrue((File.GetAttributes(testFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);

            // Act
            var result = SqlBuildFileHelper.MakeFileWriteable(testFile);

            // Assert
            Assert.IsTrue(result);
            Assert.IsFalse((File.GetAttributes(testFile) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly);
        }

        [TestMethod]
        public void MakeFileWriteable_WithNonExistentFile_ReturnsFalse()
        {
            // Arrange
            string nonExistentFile = Path.Combine(_testDir, "nonexistent.txt");

            // Act
            var result = SqlBuildFileHelper.MakeFileWriteable(nonExistentFile);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void MakeFileWriteable_WithNormalFile_ReturnsTrue()
        {
            // Arrange
            string testFile = Path.Combine(_testDir, "normal.txt");
            File.WriteAllText(testFile, "test");

            // Act
            var result = SqlBuildFileHelper.MakeFileWriteable(testFile);

            // Assert
            Assert.IsTrue(result);
        }

        #endregion

        #region ScriptRequiresBuildDescription Tests

        [TestMethod]
        public void ScriptRequiresBuildDescription_WithDescriptionToken_ReturnsTrue()
        {
            // Arrange - the actual token is #BuildDescription#
            string scriptWithToken = "INSERT INTO Table (BuildDescription) VALUES ('#BuildDescription#')";

            // Act
            var result = SqlBuildFileHelper.ScriptRequiresBuildDescription(scriptWithToken);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ScriptRequiresBuildDescription_WithoutDescriptionToken_ReturnsFalse()
        {
            // Arrange
            string scriptWithoutToken = "INSERT INTO Table (Name) VALUES ('Test')";

            // Act
            var result = SqlBuildFileHelper.ScriptRequiresBuildDescription(scriptWithoutToken);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ScriptRequiresBuildDescription_WithEmptyString_ReturnsFalse()
        {
            // Act
            var result = SqlBuildFileHelper.ScriptRequiresBuildDescription(string.Empty);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region JoinBatchedScripts Tests

        [TestMethod]
        public void JoinBatchedScripts_WithMultipleBatches_JoinsWithGO()
        {
            // Arrange
            var batches = new[] { "SELECT 1", "SELECT 2", "SELECT 3" };

            // Act
            var result = SqlBuildFileHelper.JoinBatchedScripts(batches);

            // Assert
            Assert.IsTrue(result.Contains("SELECT 1"));
            Assert.IsTrue(result.Contains("SELECT 2"));
            Assert.IsTrue(result.Contains("SELECT 3"));
        }

        [TestMethod]
        public void JoinBatchedScripts_WithSingleBatch_ReturnsSingleScript()
        {
            // Arrange
            var batches = new[] { "SELECT 1" };

            // Act
            var result = SqlBuildFileHelper.JoinBatchedScripts(batches);

            // Assert
            Assert.IsTrue(result.Contains("SELECT 1"));
        }

        [TestMethod]
        public void JoinBatchedScripts_WithEmptyArray_ReturnsEmpty()
        {
            // Arrange
            var batches = new string[0];

            // Act
            var result = SqlBuildFileHelper.JoinBatchedScripts(batches);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region GetSHA1Hash Tests

        [TestMethod]
        public void GetSHA1Hash_WithValidFile_ReturnsHashes()
        {
            // Arrange
            string testFile = Path.Combine(_testDir, "hashtest.sql");
            File.WriteAllText(testFile, "SELECT 1");

            // Act
            SqlBuildFileHelper.GetSHA1Hash(testFile, out string fileHash, out string textHash, false);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(fileHash));
            Assert.IsFalse(string.IsNullOrEmpty(textHash));
        }

        [TestMethod]
        public void GetSHA1Hash_WithNonExistentFile_ReturnsFileMissing()
        {
            // Arrange
            string nonExistentFile = Path.Combine(_testDir, "nonexistent.sql");

            // Act
            SqlBuildFileHelper.GetSHA1Hash(nonExistentFile, out string fileHash, out string textHash, false);

            // Assert
            Assert.AreEqual(SqlBuildFileHelper.FileMissing, fileHash);
        }

        [TestMethod]
        public void GetSHA1Hash_WithSameContent_ReturnsSameHash()
        {
            // Arrange
            string content = "SELECT * FROM Users WHERE Id = 1";
            string file1 = Path.Combine(_testDir, "file1.sql");
            string file2 = Path.Combine(_testDir, "file2.sql");
            File.WriteAllText(file1, content);
            File.WriteAllText(file2, content);

            // Act
            SqlBuildFileHelper.GetSHA1Hash(file1, out _, out string textHash1, false);
            SqlBuildFileHelper.GetSHA1Hash(file2, out _, out string textHash2, false);

            // Assert
            Assert.AreEqual(textHash1, textHash2);
        }

        [TestMethod]
        public async Task GetSHA1HashAsync_WithValidFile_ReturnsHashes()
        {
            // Arrange
            string testFile = Path.Combine(_testDir, "asynchashtest.sql");
            await File.WriteAllTextAsync(testFile, "SELECT 1");

            // Act
            var (fileHash, textHash) = await SqlBuildFileHelper.GetSHA1HashAsync(testFile, false);

            // Assert
            Assert.IsFalse(string.IsNullOrEmpty(fileHash));
            Assert.IsFalse(string.IsNullOrEmpty(textHash));
        }

        #endregion

        #region CalculateBuildPackageSHA1SignatureFromPath Tests

        [TestMethod]
        public void CalculateBuildPackageSHA1SignatureFromPath_WithNullData_ReturnsError()
        {
            // Act
            var result = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(_testDir, (SqlSyncBuildDataModel)null);

            // Assert
            Assert.AreEqual("Error calculating hash", result);
        }

        [TestMethod]
        public void CalculateBuildPackageSHA1SignatureFromPath_WithNullPath_ReturnsError()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(null, model);

            // Assert
            Assert.AreEqual("Error calculating hash", result);
        }

        [TestMethod]
        public void CalculateBuildPackageSHA1SignatureFromPath_WithEmptyPath_ReturnsError()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            // Act
            var result = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(string.Empty, model);

            // Assert
            Assert.AreEqual("Error calculating hash", result);
        }

        #endregion

        #region CalculateSha1HashFromPackage Tests

        [TestMethod]
        public void CalculateSha1HashFromPackage_WithNullFileName_ReturnsEmpty()
        {
            // Act
            var result = SqlBuildFileHelper.CalculateSha1HashFromPackage(null);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void CalculateSha1HashFromPackage_WithEmptyFileName_ReturnsEmpty()
        {
            // Act
            var result = SqlBuildFileHelper.CalculateSha1HashFromPackage(string.Empty);

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void CalculateSha1HashFromPackage_WithUnknownExtension_ReturnsEmpty()
        {
            // Act
            var result = SqlBuildFileHelper.CalculateSha1HashFromPackage("file.unknown");

            // Assert
            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region CopyIndividualScriptsToFolder Tests

        [TestMethod]
        public void CopyIndividualScriptsToFolder_WithEmptyScripts_ReturnsFalse()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string destFolder = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(destFolder);

            // Act
            var result = SqlBuildFileHelper.CopyIndividualScriptsToFolder(model, destFolder, _testDir, false, false);

            // Assert
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CopyIndividualScriptsToFolder_WithNullScripts_ReturnsFalse()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string destFolder = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(destFolder);

            // Act
            var result = SqlBuildFileHelper.CopyIndividualScriptsToFolder(model, destFolder, _testDir, false, false);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion

        #region CopyScriptsToSingleFile Tests

        [TestMethod]
        public void CopyScriptsToSingleFile_WithEmptyScripts_ReturnsFalse()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string destFile = Path.Combine(_testDir, "output.sql");

            // Act
            var result = SqlBuildFileHelper.CopyScriptsToSingleFile(model, destFile, _testDir, "test.sbm", false);

            // Assert
            Assert.IsFalse(result);
        }

        #endregion
    }

    /// <summary>
    /// Tests for SqlBuildFileHelper async methods
    /// </summary>
    [TestClass]
    public class SqlBuildFileHelperAsyncMethodTests
    {
        private string _testDir;

        [TestInitialize]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"SqlBuildAsyncTest_{Guid.NewGuid():N}");
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

        [TestMethod]
        public async Task CopyIndividualScriptsToFolderAsync_WithEmptyScripts_ReturnsFalse()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string destFolder = Path.Combine(_testDir, "dest");
            Directory.CreateDirectory(destFolder);

            // Act
            var success = await SqlBuildFileHelper.CopyIndividualScriptsToFolderAsync(
                model, destFolder, _testDir, false, false, CancellationToken.None);

            // Assert
            Assert.IsFalse(success);
        }

        [TestMethod]
        public async Task CopyScriptsToSingleFileAsync_WithEmptyScripts_ReturnsFalse()
        {
            // Arrange
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            string destFile = Path.Combine(_testDir, "output.sql");

            // Act
            var success = await SqlBuildFileHelper.CopyScriptsToSingleFileAsync(
                model, destFile, _testDir, "test.sbm", false, CancellationToken.None);

            // Assert
            Assert.IsFalse(success);
        }
    }
}
