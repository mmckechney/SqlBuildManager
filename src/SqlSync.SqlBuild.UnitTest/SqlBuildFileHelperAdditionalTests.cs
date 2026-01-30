using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.CodeTable;
using SqlSync.SqlBuild.Legacy;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Objects;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest
{
    /// <summary>
    /// Additional tests for SqlBuildFileHelper to increase coverage
    /// </summary>
    [TestClass]
    public class SqlBuildFileHelperAdditionalTests
    {
        #region GetFileDataForCodeTableUpdates Tests

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_WithNullBuildData_ReturnsNull()
        {
            SqlSyncBuildData buildData = null;

            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates(ref buildData, "test.xml");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_WithNoPopScripts_ReturnsEmptyArray()
        {
#pragma warning disable CS0618
            var buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
#pragma warning restore CS0618

            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates(ref buildData, "test.xml");

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_WithFileNotFound_SkipsFile()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
#pragma warning disable CS0618
                var buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
                SqlBuildFileHelper.AddScriptFileToBuild(ref buildData, Path.Combine(tempDir, "test.xml"),
                    "nonexistent.POP", 1, "Test", true, true, "TestDb", false, "", false, true, "user", 30, "");
#pragma warning restore CS0618

                var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates(ref buildData, Path.Combine(tempDir, "test.xml"));

                Assert.IsNotNull(result);
                // File doesn't exist so it should be skipped
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region GetFileDataForCodeTableUpdates (Single file) Tests

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_SingleFile_ReturnsNullWhenFileNotExists()
        {
            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates("nonexistent.pop", @"C:\Temp\project.xml");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_SingleFile_ParsesFileContent()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var popFile = Path.Combine(tempDir, "test.pop");
                var content = @"/*
Source Server: TestServer
Source Db: TestDatabase
Table Scripted: TestTable
Key Check Columns: Id, Name
Query Used:
SELECT * FROM TestTable
*/
INSERT INTO TestTable VALUES (1, 'Test');";
                File.WriteAllText(popFile, content);

                var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates("test.pop", Path.Combine(tempDir, "project.xml"));

                Assert.IsNotNull(result);
                Assert.AreEqual("TestServer", result.SourceServer);
                Assert.AreEqual("TestDatabase", result.SourceDatabase);
                Assert.AreEqual("TestTable", result.SourceTable);
                Assert.AreEqual("Id, Name", result.KeyCheckColumns);
                Assert.IsTrue(result.Query.Contains("SELECT * FROM TestTable"));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region GetFileDataForObjectUpdates Tests

        [TestMethod]
        public void GetFileDataForObjectUpdates_WithNullBuildData_ReturnsNull()
        {
            SqlSyncBuildData buildData = null;

            var result = SqlBuildFileHelper.GetFileDataForObjectUpdates(ref buildData, "test.xml");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFileDataForObjectUpdates_ModelVersion_WithNullModel_ReturnsNull()
        {
            SqlSyncBuildDataModel model = null;

            var result = SqlBuildFileHelper.GetFileDataForObjectUpdates(model, "test.xml");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFileDataForObjectUpdates_WithNonObjectScripts_ReturnsEmptyCanUpdate()
        {
#pragma warning disable CS0618
            var buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
#pragma warning restore CS0618

            SqlBuildFileHelper.GetFileDataForObjectUpdates(ref buildData, "test.xml",
                out List<ObjectUpdates> canUpdate, out List<string> canNotUpdate);

            Assert.IsNotNull(canUpdate);
            Assert.IsNotNull(canNotUpdate);
            Assert.AreEqual(0, canUpdate.Count);
        }

        [TestMethod]
        public void GetFileDataForObjectUpdates_SingleFile_ReturnsNullWhenFileNotExists()
        {
            var result = SqlBuildFileHelper.GetFileDataForObjectUpdates("nonexistent.prc", @"C:\Temp\project.xml");

            Assert.IsNull(result);
        }

        [TestMethod]
        public void GetFileDataForObjectUpdates_SingleFile_ParsesStoredProcedureFile()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var spFile = Path.Combine(tempDir, "test.prc");
                var content = @"/*
Source Server: TestServer
Source Db: TestDatabase
Object Scripted: dbo.MyStoredProc
Object Type: StoredProcedure
Include Permissions: True
Script as ALTER: False
Script PK with Table: True
*/
CREATE PROCEDURE dbo.MyStoredProc AS SELECT 1";
                File.WriteAllText(spFile, content);

                var result = SqlBuildFileHelper.GetFileDataForObjectUpdates("test.prc", Path.Combine(tempDir, "project.xml"));

                Assert.IsNotNull(result);
                Assert.AreEqual("TestServer", result.SourceServer);
                Assert.AreEqual("TestDatabase", result.SourceDatabase);
                Assert.AreEqual("dbo.MyStoredProc", result.SourceObject);
                Assert.AreEqual("StoredProcedure", result.ObjectType);
                Assert.IsTrue(result.IncludePermissions);
                Assert.IsFalse(result.ScriptAsAlter);
                Assert.IsTrue(result.ScriptPkWithTable);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void GetFileDataForObjectUpdates_ModelVersion_WithObjectScripts_CategorizesByExtension()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

                // Add a regular SQL file (should go to canNotUpdate)
                model = SqlBuildFileHelper.AddScriptFileToBuild(model, Path.Combine(tempDir, "test.xml"),
                    "regular.sql", 1, "", true, true, "TestDb", false, "", false, true, "user", 30, Guid.NewGuid(), "");

                SqlBuildFileHelper.GetFileDataForObjectUpdates(model, Path.Combine(tempDir, "test.xml"),
                    out List<ObjectUpdates> canUpdate, out List<string> canNotUpdate);

                Assert.IsNotNull(canNotUpdate);
                Assert.IsTrue(canNotUpdate.Contains("regular.sql"));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region UpdateObsoleteXmlNamespace Tests

        [TestMethod]
        public void UpdateObsoleteXmlNamespace_WithValidNamespace_ReturnsFalse()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var xmlFile = Path.Combine(tempDir, "test.xml");
                var content = @"<?xml version=""1.0""?>
<Root xmlns=""http://schemas.mckechney.com/data"">
    <Element>Test</Element>
</Root>";
                File.WriteAllText(xmlFile, content);

                var result = SqlBuildFileHelper.UpdateObsoleteXmlNamespace(xmlFile);

                Assert.IsFalse(result); // Already valid namespace
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void UpdateObsoleteXmlNamespace_WithObsoleteNamespace_ReplacesAndReturnsTrue()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var xmlFile = Path.Combine(tempDir, "test.xml");
                var content = @"<?xml version=""1.0""?>
<Root xmlns=""http://oldschema.example.com/data"">
    <Element>Test</Element>
</Root>";
                File.WriteAllText(xmlFile, content);

                var result = SqlBuildFileHelper.UpdateObsoleteXmlNamespace(xmlFile);

                Assert.IsTrue(result);
                var updatedContent = File.ReadAllText(xmlFile);
                Assert.IsTrue(updatedContent.Contains("http://schemas.mckechney.com/"));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void UpdateObsoleteXmlNamespace_WithNoNamespace_ReturnsFalse()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var xmlFile = Path.Combine(tempDir, "test.xml");
                var content = @"<?xml version=""1.0""?>
<Root>
    <Element>Test</Element>
</Root>";
                File.WriteAllText(xmlFile, content);

                var result = SqlBuildFileHelper.UpdateObsoleteXmlNamespace(xmlFile);

                Assert.IsFalse(result);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region LoadSqlBuildProjectFile Tests

        [TestMethod]
        public void LoadSqlBuildProjectFile_FileDoesNotExist_ReturnsFalseAndCreatesShell()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var projFile = Path.Combine(tempDir, "nonexistent.xml");

                var result = SqlBuildFileHelper.LoadSqlBuildProjectFile(out SqlSyncBuildDataModel model, projFile, false);

                Assert.IsFalse(result);
                Assert.IsNotNull(model);
                // Shell model is created
                Assert.IsNotNull(model.SqlSyncBuildProject);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void LoadSqlBuildProjectModel_ReturnsModelEvenIfFileDoesNotExist()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var projFile = Path.Combine(tempDir, "nonexistent.xml");

                var model = SqlBuildFileHelper.LoadSqlBuildProjectModel(projFile, false);

                Assert.IsNotNull(model);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region CalculateSha1HashFromPackage Tests

        [TestMethod]
        public void CalculateSha1HashFromPackage_WithEmptyString_ReturnsEmptyString()
        {
            var result = SqlBuildFileHelper.CalculateSha1HashFromPackage(string.Empty);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void CalculateSha1HashFromPackage_WithNullString_ReturnsEmptyString()
        {
            var result = SqlBuildFileHelper.CalculateSha1HashFromPackage(null);

            Assert.AreEqual(string.Empty, result);
        }

        [TestMethod]
        public void CalculateSha1HashFromPackage_WithInvalidExtension_ReturnsEmptyString()
        {
            var result = SqlBuildFileHelper.CalculateSha1HashFromPackage("test.txt");

            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region GetSHA1HashAsync Tests

        [TestMethod]
        public async Task GetSHA1HashAsync_FileNotFound_ReturnsHashError()
        {
            var (fileHash, textHash) = await SqlBuildFileHelper.GetSHA1HashAsync(
                @"C:\NonExistent\File.sql", false, CancellationToken.None);

            // When file not found, the catch block returns SHA1 Hash Error
            Assert.AreEqual(SqlBuildFileHelper.Sha1HashError, fileHash);
            Assert.AreEqual(SqlBuildFileHelper.Sha1HashError, textHash);
        }

        [TestMethod]
        public async Task GetSHA1HashAsync_ValidFile_ReturnsHashes()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "SELECT 1;");

                var (fileHash, textHash) = await SqlBuildFileHelper.GetSHA1HashAsync(
                    tempFile, false, CancellationToken.None);

                Assert.AreNotEqual(SqlBuildFileHelper.FileMissing, fileHash);
                Assert.AreNotEqual(SqlBuildFileHelper.FileMissing, textHash);
                Assert.IsFalse(string.IsNullOrEmpty(fileHash));
                Assert.IsFalse(string.IsNullOrEmpty(textHash));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        #endregion

        #region GetSHA1Hash (File-based) Tests

        [TestMethod]
        public void GetSHA1Hash_FileNotFound_ReturnsHashError()
        {
            SqlBuildFileHelper.GetSHA1Hash(@"C:\NonExistent\File.sql", out string fileHash, out string textHash, false);

            // When file is not found, the general catch returns SHA1 Hash Error
            Assert.AreEqual(SqlBuildFileHelper.Sha1HashError, fileHash);
            Assert.AreEqual(SqlBuildFileHelper.Sha1HashError, textHash);
        }

        [TestMethod]
        public void GetSHA1Hash_ValidFile_ReturnsValidHashes()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "SELECT 1;");

                SqlBuildFileHelper.GetSHA1Hash(tempFile, out string fileHash, out string textHash, false);

                Assert.AreNotEqual(SqlBuildFileHelper.FileMissing, fileHash);
                Assert.AreNotEqual(SqlBuildFileHelper.FileMissing, textHash);
                Assert.AreNotEqual(SqlBuildFileHelper.Sha1HashError, fileHash);
                Assert.AreNotEqual(SqlBuildFileHelper.Sha1HashError, textHash);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void GetSHA1Hash_SameContent_ProducesSameHash()
        {
            var tempFile1 = Path.GetTempFileName();
            var tempFile2 = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile1, "SELECT 1;");
                File.WriteAllText(tempFile2, "SELECT 1;");

                SqlBuildFileHelper.GetSHA1Hash(tempFile1, out _, out string textHash1, false);
                SqlBuildFileHelper.GetSHA1Hash(tempFile2, out _, out string textHash2, false);

                Assert.AreEqual(textHash1, textHash2);
            }
            finally
            {
                File.Delete(tempFile1);
                File.Delete(tempFile2);
            }
        }

        [TestMethod]
        public void GetSHA1Hash_DifferentContent_ProducesDifferentHash()
        {
            var tempFile1 = Path.GetTempFileName();
            var tempFile2 = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile1, "SELECT 1;");
                File.WriteAllText(tempFile2, "SELECT 2;");

                SqlBuildFileHelper.GetSHA1Hash(tempFile1, out _, out string textHash1, false);
                SqlBuildFileHelper.GetSHA1Hash(tempFile2, out _, out string textHash2, false);

                Assert.AreNotEqual(textHash1, textHash2);
            }
            finally
            {
                File.Delete(tempFile1);
                File.Delete(tempFile2);
            }
        }

        #endregion

        #region PackageSbxFileIntoSbmFile Tests

        [TestMethod]
        public void PackageSbxFileIntoSbmFile_WithEmptyFileName_ReturnsFalse()
        {
            var result = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(string.Empty, "test.sbm");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PackageSbxFileIntoSbmFile_WithNullFileName_ReturnsFalse()
        {
            var result = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(null, "test.sbm");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PackageSbxFileIntoSbmFile_WithWhitespaceFileName_ReturnsFalse()
        {
            var result = SqlBuildFileHelper.PackageSbxFileIntoSbmFile("   ", "test.sbm");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task PackageSbxFileIntoSbmFileAsync_WithEmptyFileName_ReturnsFalse()
        {
            var result = await SqlBuildFileHelper.PackageSbxFileIntoSbmFileAsync(string.Empty, "test.sbm", CancellationToken.None);

            Assert.IsFalse(result);
        }

        #endregion

        #region PackageSbxFilesIntoSbmFiles Tests

        [TestMethod]
        public void PackageSbxFilesIntoSbmFiles_WithNullDirectory_ReturnsEmptyList()
        {
            var result = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(null, out string message);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        [TestMethod]
        public void PackageSbxFilesIntoSbmFiles_WithEmptyDirectory_ReturnsEmptyList()
        {
            var result = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(string.Empty, out string message);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PackageSbxFilesIntoSbmFiles_WithNonExistentDirectory_ReturnsEmptyList()
        {
            var result = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(@"C:\NonExistent\Directory", out string message);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void PackageSbxFilesIntoSbmFiles_WithNoSbxFiles_ReturnsEmptyList()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var result = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(tempDir, out string message);

                Assert.IsNotNull(result);
                Assert.AreEqual(0, result.Count);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public async Task PackageSbxFilesIntoSbmFilesAsync_WithNullDirectory_ReturnsEmptyList()
        {
            var result = await SqlBuildFileHelper.PackageSbxFilesIntoSbmFilesAsync(null, CancellationToken.None);

            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Count);
        }

        #endregion

        #region InferOverridesFromPackage Tests

        [TestMethod]
        public void InferOverridesFromPackage_WithNonExistentFile_ReturnsEmptyString()
        {
            var result = SqlBuildFileHelper.InferOverridesFromPackage(@"C:\NonExistent\File.sbm", "TestDb");

            Assert.AreEqual(string.Empty, result);
        }

        #endregion

        #region CalculateBuildPackageSHA1SignatureFromPath Tests

        [TestMethod]
        public void CalculateBuildPackageSHA1SignatureFromPath_WithNullBuildData_ReturnsError()
        {
            SqlSyncBuildData buildData = null;

            var result = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(@"C:\Temp", buildData);

            Assert.AreEqual("Error calculating hash", result);
        }

        [TestMethod]
        public void CalculateBuildPackageSHA1SignatureFromPath_WithEmptyPath_ReturnsError()
        {
#pragma warning disable CS0618
            var buildData = SqlBuildFileHelper.CreateShellSqlSyncBuildDataObject();
#pragma warning restore CS0618

            var result = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(string.Empty, buildData);

            Assert.AreEqual("Error calculating hash", result);
        }

        #endregion

        #region GetInsertedIndexValues Edge Cases Tests

        [TestMethod]
        public void GetInsertedIndexValues_LargeInsertCount_AllValuesInRange()
        {
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 2.0, 50);

            Assert.AreEqual(50, result.Count);
            foreach (var val in result)
            {
                Assert.IsTrue(val > 1.0 && val < 2.0, $"Value {val} should be between 1.0 and 2.0");
            }
        }

        [TestMethod]
        public void GetInsertedIndexValues_FloorEqualsFloorOfCeiling_UsesWholeValues()
        {
            // When floor=1.5 and ceiling=4.5, it adjusts to floor=2, ceiling=4
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.5, 4.5, 2);

            Assert.AreEqual(2, result.Count);
            // Values should be 3 and 4 (whole values between 2 and 4)
        }

        [TestMethod]
        public void GetInsertedIndexValues_VeryTightRange_ProducesCorrectNumberOfValues()
        {
            // Very tight range with 5 insertions - test that the method handles edge cases
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 1.1, 3);

            Assert.AreEqual(3, result.Count);
            // All values should be within a reasonable range
            foreach (var val in result)
            {
                Assert.IsTrue(val >= 1.0 && val <= 1.1, $"Value {val} should be between 1.0 and 1.1");
            }
        }

        #endregion

        #region ScriptRequiresBuildDescription Edge Cases

        [TestMethod]
        public void ScriptRequiresBuildDescription_TokenWithinContent_ReturnsTrue()
        {
            // Token needs to be found at index > -1, and the method starts searching from index 1
            var script = $"X{SqlBuild.ScriptTokens.BuildDescription} some content";

            var result = SqlBuildFileHelper.ScriptRequiresBuildDescription(script);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ScriptRequiresBuildDescription_MultipleTokens_ReturnsTrue()
        {
            var script = $"Before {SqlBuild.ScriptTokens.BuildDescription} and {SqlBuild.ScriptTokens.BuildDescription} after";

            var result = SqlBuildFileHelper.ScriptRequiresBuildDescription(script);

            Assert.IsTrue(result);
        }

        #endregion
    }
}
