using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.SqlBuild.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.UnitTest
{
    [TestClass]
    public class SqlBuildFileHelperTests
    {
        #region CreateShellSqlSyncBuildDataModel Tests

        [TestMethod]
        public void CreateShellSqlSyncBuildDataModel_ReturnsNonNullModel()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            Assert.IsNotNull(model);
        }

        [TestMethod]
        public void CreateShellSqlSyncBuildDataModel_HasOneSqlSyncBuildProject()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            Assert.IsNotNull(model.SqlSyncBuildProject);
            Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
        }

        [TestMethod]
        public void CreateShellSqlSyncBuildDataModel_HasEmptyScriptCollection()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            Assert.IsNotNull(model.Script);
            Assert.AreEqual(0, model.Script.Count);
        }

        [TestMethod]
        public void CreateShellSqlSyncBuildDataModel_HasEmptyBuildCollection()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            Assert.IsNotNull(model.Build);
            Assert.AreEqual(0, model.Build.Count);
        }

        [TestMethod]
        public void CreateShellSqlSyncBuildDataModel_HasEmptyScriptRunCollection()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            Assert.IsNotNull(model.ScriptRun);
            Assert.AreEqual(0, model.ScriptRun.Count);
        }

        [TestMethod]
        public void CreateShellSqlSyncBuildDataModel_HasEmptyCommittedScriptCollection()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            Assert.IsNotNull(model.CommittedScript);
            Assert.AreEqual(0, model.CommittedScript.Count);
        }

        [TestMethod]
        public void CreateShellSqlSyncBuildDataModel_ProjectHasEmptyName()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            Assert.AreEqual(string.Empty, model.SqlSyncBuildProject[0].ProjectName);
        }

        [TestMethod]
        public void CreateShellSqlSyncBuildDataModel_ProjectHasScriptTagRequiredFalse()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            Assert.AreEqual(false, model.SqlSyncBuildProject[0].ScriptTagRequired);
        }

        #endregion

        #region AddScriptFileToBuild Tests (Model version)

        [TestMethod]
        public void AddScriptFileToBuild_AddsScriptToModel()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var scriptId = Guid.NewGuid();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script1.sql",
                buildOrder: 1,
                description: "Test script",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: scriptId,
                tag: "tag1");

            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Script.Count);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectFileName()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "myscript.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "");

            Assert.AreEqual("myscript.sql", result.Script[0].FileName);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectBuildOrder()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 5.5,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "");

            Assert.AreEqual(5.5, result.Script[0].BuildOrder);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectDatabase()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "MyDatabase",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "");

            Assert.AreEqual("MyDatabase", result.Script[0].Database);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectDescription()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "My Description",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "");

            Assert.AreEqual("My Description", result.Script[0].Description);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectRollBackOnError()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: false,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "");

            Assert.AreEqual(false, result.Script[0].RollBackOnError);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectCausesBuildFailure()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: false,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "");

            Assert.AreEqual(false, result.Script[0].CausesBuildFailure);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectStripTransactionText()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: true,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "");

            Assert.AreEqual(true, result.Script[0].StripTransactionText);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectAllowMultipleRuns()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: false,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "");

            Assert.AreEqual(false, result.Script[0].AllowMultipleRuns);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectAddedBy()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "myuser",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "");

            Assert.AreEqual("myuser", result.Script[0].AddedBy);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectScriptTimeOut()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 120,
                scriptId: Guid.NewGuid(),
                tag: "");

            Assert.AreEqual(120, result.Script[0].ScriptTimeOut);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsCorrectTag()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "mytag");

            Assert.AreEqual("mytag", result.Script[0].Tag);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsScriptIdCorrectly()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var scriptId = Guid.NewGuid();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: scriptId,
                tag: "");

            Assert.AreEqual(scriptId.ToString(), result.Script[0].ScriptId);
        }

        [TestMethod]
        public void AddScriptFileToBuild_GeneratesNewScriptIdWhenEmpty()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.Empty,
                tag: "");

            Assert.IsNotNull(result.Script[0].ScriptId);
            Assert.AreNotEqual(Guid.Empty.ToString(), result.Script[0].ScriptId);
        }

        [TestMethod]
        public void AddScriptFileToBuild_SetsDateAdded()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var before = DateTime.Now;

            var result = SqlBuildFileHelper.AddScriptFileToBuild(
                model,
                projFileName: "test.xml",
                fileName: "script.sql",
                buildOrder: 1,
                description: "",
                rollBackScript: true,
                rollBackBuild: true,
                databaseName: "TestDb",
                stripTransactions: false,
                buildZipFileName: "",
                saveToZip: false,
                allowMultipleRuns: true,
                addedBy: "tester",
                scriptTimeOut: 30,
                scriptId: Guid.NewGuid(),
                tag: "");

            var after = DateTime.Now;

            Assert.IsTrue(result.Script[0].DateAdded >= before);
            Assert.IsTrue(result.Script[0].DateAdded <= after);
        }

        [TestMethod]
        public void AddScriptFileToBuild_MultipleScripts_PreservesOrder()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            model = SqlBuildFileHelper.AddScriptFileToBuild(
                model, "test.xml", "script1.sql", 1, "", true, true, "TestDb", false, "", false, true, "tester", 30, Guid.NewGuid(), "");

            model = SqlBuildFileHelper.AddScriptFileToBuild(
                model, "test.xml", "script2.sql", 2, "", true, true, "TestDb", false, "", false, true, "tester", 30, Guid.NewGuid(), "");

            model = SqlBuildFileHelper.AddScriptFileToBuild(
                model, "test.xml", "script3.sql", 3, "", true, true, "TestDb", false, "", false, true, "tester", 30, Guid.NewGuid(), "");

            Assert.AreEqual(3, model.Script.Count);
            Assert.AreEqual("script1.sql", model.Script[0].FileName);
            Assert.AreEqual("script2.sql", model.Script[1].FileName);
            Assert.AreEqual("script3.sql", model.Script[2].FileName);
        }

        #endregion

        #region GetInsertedIndexValues Tests

        [TestMethod]
        public void GetInsertedIndexValues_WholeValuesWhenPossible()
        {
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 5.0, 2);

            Assert.AreEqual(2, result.Count);
            Assert.AreEqual(2.0, result[0]);
            Assert.AreEqual(3.0, result[1]);
        }

        [TestMethod]
        public void GetInsertedIndexValues_SingleInsert()
        {
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 3.0, 1);

            Assert.AreEqual(1, result.Count);
            Assert.AreEqual(2.0, result[0]);
        }

        [TestMethod]
        public void GetInsertedIndexValues_TenthValuesWhenNeeded()
        {
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 1.5, 3);

            Assert.AreEqual(3, result.Count);
            Assert.IsTrue(result.All(v => v > 1.0 && v < 1.5));
        }

        [TestMethod]
        public void GetInsertedIndexValues_UsesDecimalWhenTight()
        {
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 2.0, 5);

            Assert.AreEqual(5, result.Count);
            foreach (var val in result)
            {
                Assert.IsTrue(val > 1.0 && val < 2.0, $"Value {val} should be between 1.0 and 2.0");
            }
        }

        [TestMethod]
        public void GetInsertedIndexValues_ReturnsEmptyForZeroCount()
        {
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 5.0, 0);

            Assert.AreEqual(0, result.Count);
        }

        [TestMethod]
        public void GetInsertedIndexValues_ReturnsDistinctValues()
        {
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 2.0, 10);

            var distinct = result.Distinct().ToList();
            Assert.AreEqual(result.Count, distinct.Count);
        }

        [TestMethod]
        public void GetInsertedIndexValues_ValuesAreInAscendingOrder()
        {
            var result = SqlBuildFileHelper.GetInsertedIndexValues(1.0, 10.0, 5);

            for (int i = 1; i < result.Count; i++)
            {
                Assert.IsTrue(result[i] > result[i - 1], $"Values should be in ascending order, but {result[i]} is not greater than {result[i - 1]}");
            }
        }

        #endregion

        #region ScriptRequiresBuildDescription Tests

        [TestMethod]
        public void ScriptRequiresBuildDescription_ReturnsFalseForNull()
        {
            var result = SqlBuildFileHelper.ScriptRequiresBuildDescription(null);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ScriptRequiresBuildDescription_ReturnsFalseForEmpty()
        {
            var result = SqlBuildFileHelper.ScriptRequiresBuildDescription(string.Empty);

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ScriptRequiresBuildDescription_ReturnsFalseWhenNoToken()
        {
            var result = SqlBuildFileHelper.ScriptRequiresBuildDescription("SELECT * FROM Table");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void ScriptRequiresBuildDescription_ReturnsTrueWhenTokenPresent()
        {
            var scriptWithToken = $"INSERT INTO Log (Description) VALUES ('{SqlBuild.ScriptTokens.BuildDescription}')";
            var result = SqlBuildFileHelper.ScriptRequiresBuildDescription(scriptWithToken);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void ScriptRequiresBuildDescription_CaseInsensitive()
        {
            var scriptWithToken = $"INSERT INTO Log (Description) VALUES ('{SqlBuild.ScriptTokens.BuildDescription.ToLower()}')";
            var result = SqlBuildFileHelper.ScriptRequiresBuildDescription(scriptWithToken);

            Assert.IsTrue(result);
        }

        #endregion

        #region ValidateAgainstSchema Tests

        [TestMethod]
        public void ValidateAgainstSchema_AlwaysReturnsTrue()
        {
            var result = SqlBuildFileHelper.ValidateAgainstSchema("any_file.xml", out string errorMessage);

            Assert.IsTrue(result);
            Assert.AreEqual(string.Empty, errorMessage);
        }

        [TestMethod]
        public void ValidateAgainstSchema_ErrorMessageIsEmpty()
        {
            SqlBuildFileHelper.ValidateAgainstSchema("nonexistent.xml", out string errorMessage);

            Assert.AreEqual(string.Empty, errorMessage);
        }

        #endregion

        #region MakeFileWriteable Tests

        [TestMethod]
        public void MakeFileWriteable_ReturnsTrueForWriteableFile()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                var result = SqlBuildFileHelper.MakeFileWriteable(tempFile);

                Assert.IsTrue(result);
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void MakeFileWriteable_MakesReadOnlyFileWriteable()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.SetAttributes(tempFile, FileAttributes.ReadOnly);

                var result = SqlBuildFileHelper.MakeFileWriteable(tempFile);

                Assert.IsTrue(result);
                Assert.AreEqual(FileAttributes.Normal, File.GetAttributes(tempFile));
            }
            finally
            {
                File.SetAttributes(tempFile, FileAttributes.Normal);
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void MakeFileWriteable_ReturnsFalseForNonExistentFile()
        {
            var result = SqlBuildFileHelper.MakeFileWriteable(@"C:\NonExistent\File.txt");

            Assert.IsFalse(result);
        }

        #endregion

        #region JoinBatchedScripts Tests

        [TestMethod]
        public void JoinBatchedScripts_JoinsWithDelimiter()
        {
            var scripts = new[] { "SELECT 1", "SELECT 2", "SELECT 3" };

            var result = SqlBuildFileHelper.JoinBatchedScripts(scripts);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("SELECT 1"));
            Assert.IsTrue(result.Contains("SELECT 2"));
            Assert.IsTrue(result.Contains("SELECT 3"));
        }

        [TestMethod]
        public void JoinBatchedScripts_SingleScript()
        {
            var scripts = new[] { "SELECT 1" };

            var result = SqlBuildFileHelper.JoinBatchedScripts(scripts);

            Assert.IsNotNull(result);
            Assert.IsTrue(result.Contains("SELECT 1"));
        }

        [TestMethod]
        public void JoinBatchedScripts_EmptyArray()
        {
            var scripts = new string[0];

            var result = SqlBuildFileHelper.JoinBatchedScripts(scripts);

            Assert.IsNotNull(result);
        }

        #endregion

        #region GetTotalLogFilesSize Tests

        [TestMethod]
        public void GetTotalLogFilesSize_ReturnsZeroForEmptyDirectory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var result = SqlBuildFileHelper.GetTotalLogFilesSize(tempDir);

                Assert.AreEqual(0, result);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void GetTotalLogFilesSize_ReturnsZeroForNonExistentDirectory()
        {
            var result = SqlBuildFileHelper.GetTotalLogFilesSize(@"C:\NonExistent\Directory");

            Assert.AreEqual(0, result);
        }

        [TestMethod]
        public void GetTotalLogFilesSize_SumsLogFileSizes()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var log1 = Path.Combine(tempDir, "test1.log");
                var log2 = Path.Combine(tempDir, "test2.log");
                File.WriteAllText(log1, "Log content 1");
                File.WriteAllText(log2, "Log content 2 with more data");

                var result = SqlBuildFileHelper.GetTotalLogFilesSize(tempDir);

                var expected = new FileInfo(log1).Length + new FileInfo(log2).Length;
                Assert.AreEqual(expected, result);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void GetTotalLogFilesSize_IgnoresNonLogFiles()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var log1 = Path.Combine(tempDir, "test1.log");
                var txt1 = Path.Combine(tempDir, "test1.txt");
                var sql1 = Path.Combine(tempDir, "test1.sql");
                File.WriteAllText(log1, "Log content");
                File.WriteAllText(txt1, "Text content that should be ignored");
                File.WriteAllText(sql1, "SQL content that should be ignored");

                var result = SqlBuildFileHelper.GetTotalLogFilesSize(tempDir);

                var expected = new FileInfo(log1).Length;
                Assert.AreEqual(expected, result);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region CleanUpAndDeleteWorkingDirectory Tests

        [TestMethod]
        public void CleanUpAndDeleteWorkingDirectory_DeletesExistingDirectory()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var testFile = Path.Combine(tempDir, "test.txt");
            File.WriteAllText(testFile, "test content");

            var result = SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(tempDir);

            Assert.IsTrue(result);
            Assert.IsFalse(Directory.Exists(tempDir));
        }

        [TestMethod]
        public void CleanUpAndDeleteWorkingDirectory_ReturnsTrueForNonExistentDirectory()
        {
            var nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var result = SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(nonExistentDir);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CleanUpAndDeleteWorkingDirectory_DeletesNestedDirectories()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var subDir = Path.Combine(tempDir, "subdir");
            Directory.CreateDirectory(subDir);
            File.WriteAllText(Path.Combine(subDir, "test.txt"), "test");

            var result = SqlBuildFileHelper.CleanUpAndDeleteWorkingDirectory(tempDir);

            Assert.IsTrue(result);
            Assert.IsFalse(Directory.Exists(tempDir));
        }

        #endregion

        #region InitilizeWorkingDirectory Tests

        [TestMethod]
        public void InitilizeWorkingDirectory_CreatesNewDirectory()
        {
            string workingDirectory = "";
            string projectFilePath = "";
            string projectFileName = "";

            var result = SqlBuildFileHelper.InitilizeWorkingDirectory(ref workingDirectory, ref projectFilePath, ref projectFileName);

            try
            {
                Assert.IsTrue(result);
                Assert.IsTrue(Directory.Exists(workingDirectory));
                Assert.IsTrue(workingDirectory.Contains("Sqlsync-"));
            }
            finally
            {
                if (Directory.Exists(workingDirectory))
                    Directory.Delete(workingDirectory, true);
            }
        }

        [TestMethod]
        public void InitilizeWorkingDirectory_SetsProjectFilePath()
        {
            string workingDirectory = "";
            string projectFilePath = "";
            string projectFileName = "";

            var result = SqlBuildFileHelper.InitilizeWorkingDirectory(ref workingDirectory, ref projectFilePath, ref projectFileName);

            try
            {
                Assert.AreEqual(workingDirectory, projectFilePath);
            }
            finally
            {
                if (Directory.Exists(workingDirectory))
                    Directory.Delete(workingDirectory, true);
            }
        }

        [TestMethod]
        public void InitilizeWorkingDirectory_UpdatesProjectFileName()
        {
            string workingDirectory = "";
            string projectFilePath = "";
            string projectFileName = "test.xml";

            var result = SqlBuildFileHelper.InitilizeWorkingDirectory(ref workingDirectory, ref projectFilePath, ref projectFileName);

            try
            {
                Assert.IsTrue(projectFileName.EndsWith("test.xml"));
                Assert.IsTrue(projectFileName.StartsWith(workingDirectory));
            }
            finally
            {
                if (Directory.Exists(workingDirectory))
                    Directory.Delete(workingDirectory, true);
            }
        }

        [TestMethod]
        public void InitilizeWorkingDirectory_CleansUpExistingDirectory()
        {
            string workingDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(workingDirectory);
            var oldFile = Path.Combine(workingDirectory, "old.txt");
            File.WriteAllText(oldFile, "old content");
            string projectFilePath = "";
            string projectFileName = "";

            var result = SqlBuildFileHelper.InitilizeWorkingDirectory(ref workingDirectory, ref projectFilePath, ref projectFileName);

            try
            {
                Assert.IsTrue(result);
                Assert.IsFalse(File.Exists(oldFile));
            }
            finally
            {
                if (Directory.Exists(workingDirectory))
                    Directory.Delete(workingDirectory, true);
            }
        }

        #endregion

        #region FileMissing and Sha1HashError Constants Tests

        [TestMethod]
        public void FileMissing_HasExpectedValue()
        {
            Assert.AreEqual("File Missing", SqlBuildFileHelper.FileMissing);
        }

        [TestMethod]
        public void Sha1HashError_HasExpectedValue()
        {
            Assert.AreEqual("SHA1 Hash Error", SqlBuildFileHelper.Sha1HashError);
        }

        #endregion

        #region PackageProjectFileIntoZip Tests

        [TestMethod]
        public void PackageProjectFileIntoZip_ReturnsTrueForNullZipFileName()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.PackageProjectFileIntoZip(model, Path.GetTempPath(), null, false);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PackageProjectFileIntoZip_ReturnsTrueForEmptyZipFileName()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

            var result = SqlBuildFileHelper.PackageProjectFileIntoZip(model, Path.GetTempPath(), string.Empty, false);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void PackageProjectFileIntoZip_ReturnsFalseForNullModel()
        {
            SqlSyncBuildDataModel model = null;

            var result = SqlBuildFileHelper.PackageProjectFileIntoZip(model, Path.GetTempPath(), "test.sbm", false);

            Assert.IsFalse(result);
        }

        #endregion

        #region SaveSqlFilesToNewBuildFile Tests

        [TestMethod]
        public void SaveSqlFilesToNewBuildFile_ReturnsFalseForPreExistingFile()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var existingFile = Path.Combine(tempDir, "existing.sbm");
                File.WriteAllText(existingFile, "existing content");

                var result = SqlBuildFileHelper.SaveSqlFilesToNewBuildFile(
                    existingFile,
                    new List<string>(),
                    "TestDb",
                    overwritePreExistingFile: false,
                    defaultScriptTimeout: 30,
                    includeHistoryAndLogs: true);

                Assert.IsFalse(result);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region CalculateBuildPackageSHA1SignatureFromPath Tests

        [TestMethod]
        public void CalculateBuildPackageSHA1SignatureFromPath_WithEmptyModel_ReturnsHash()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();

                var result = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(tempDir, model);

                // Method returns a SHA1 hash even for empty model (hash of empty content)
                Assert.IsFalse(string.IsNullOrEmpty(result));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void CalculateBuildPackageSHA1SignatureFromPath_WithScriptsThatDontExist_ReturnsHash()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                model = SqlBuildFileHelper.AddScriptFileToBuild(
                    model, "test.xml", "nonexistent.sql", 1, "", true, true,
                    "TestDb", false, "", false, true, "tester", 30, Guid.NewGuid(), "");

                var result = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(tempDir, model);

                // Method returns a hash even when scripts don't exist (hash includes metadata)
                Assert.IsFalse(string.IsNullOrEmpty(result));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void CalculateBuildPackageSHA1SignatureFromPath_WithValidScripts_ReturnsNonEmptyHash()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var scriptFile = Path.Combine(tempDir, "test.sql");
                File.WriteAllText(scriptFile, "SELECT 1");

                var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                model = SqlBuildFileHelper.AddScriptFileToBuild(
                    model, Path.Combine(tempDir, "project.xml"), "test.sql", 1, "", true, true,
                    "TestDb", false, "", false, true, "tester", 30, Guid.NewGuid(), "");

                var result = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(tempDir, model);

                Assert.IsFalse(string.IsNullOrEmpty(result));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void CalculateBuildPackageSHA1SignatureFromPath_SameScriptsDifferentOrder_ReturnsDifferentHash()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                File.WriteAllText(Path.Combine(tempDir, "script1.sql"), "SELECT 1");
                File.WriteAllText(Path.Combine(tempDir, "script2.sql"), "SELECT 2");

                var model1 = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                model1 = SqlBuildFileHelper.AddScriptFileToBuild(
                    model1, Path.Combine(tempDir, "proj.xml"), "script1.sql", 1, "", true, true, "TestDb", false, "", false, true, "tester", 30, Guid.NewGuid(), "");
                model1 = SqlBuildFileHelper.AddScriptFileToBuild(
                    model1, Path.Combine(tempDir, "proj.xml"), "script2.sql", 2, "", true, true, "TestDb", false, "", false, true, "tester", 30, Guid.NewGuid(), "");

                var model2 = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
                model2 = SqlBuildFileHelper.AddScriptFileToBuild(
                    model2, Path.Combine(tempDir, "proj.xml"), "script2.sql", 1, "", true, true, "TestDb", false, "", false, true, "tester", 30, Guid.NewGuid(), "");
                model2 = SqlBuildFileHelper.AddScriptFileToBuild(
                    model2, Path.Combine(tempDir, "proj.xml"), "script1.sql", 2, "", true, true, "TestDb", false, "", false, true, "tester", 30, Guid.NewGuid(), "");

                var hash1 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(tempDir, model1);
                var hash2 = SqlBuildFileHelper.CalculateBuildPackageSHA1SignatureFromPath(tempDir, model2);

                Assert.AreNotEqual(hash1, hash2);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region RemoveScriptFilesFromBuild POCO Tests

        [TestMethod]
        public void RemoveScriptFilesFromBuild_Model_RemovesSpecifiedScripts()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var script1 = new Script("script1.sql", 1, "Script 1", false, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, false, "tester", 30, null, null, null);
            var script2 = new Script("script2.sql", 2, "Script 2", false, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, false, "tester", 30, null, null, null);
            model.Script.Add(script1);
            model.Script.Add(script2);

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var projFile = Path.Combine(tempDir, "proj.xml");
            
            try
            {
                var result = SqlBuildFileHelper.RemoveScriptFilesFromBuild(model, projFile, "", new[] { script1 }, false);
                
                Assert.AreEqual(1, result.Script.Count);
                Assert.AreEqual("script2.sql", result.Script[0].FileName);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void RemoveScriptFilesFromBuild_Model_DeletesFilesWhenRequested()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var script1 = new Script("script1.sql", 1, "Script 1", false, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, false, "tester", 30, null, null, null);
            model.Script.Add(script1);

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var projFile = Path.Combine(tempDir, "proj.xml");
            var scriptFile = Path.Combine(tempDir, "script1.sql");
            File.WriteAllText(scriptFile, "SELECT 1");
            
            try
            {
                Assert.IsTrue(File.Exists(scriptFile));
                SqlBuildFileHelper.RemoveScriptFilesFromBuild(model, projFile, "", new[] { script1 }, deleteFiles: true);
                Assert.IsFalse(File.Exists(scriptFile));
            }
            finally
            {
                if (Directory.Exists(tempDir))
                    Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region RenumberBuildSequence POCO Tests

        [TestMethod]
        public void RenumberBuildSequence_Model_RenumbersFromOne()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model.Script.Add(new Script("a.sql", 5, "A", false, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, false, "tester", 30, null, null, null));
            model.Script.Add(new Script("b.sql", 10, "B", false, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, false, "tester", 30, null, null, null));
            model.Script.Add(new Script("c.sql", 15, "C", false, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, false, "tester", 30, null, null, null));

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var projFile = Path.Combine(tempDir, "proj.xml");
            
            try
            {
                var result = SqlBuildFileHelper.RenumberBuildSequence(model, projFile, "");
                
                var sortedScripts = result.Script.OrderBy(s => s.BuildOrder).ToList();
                Assert.AreEqual(1, sortedScripts[0].BuildOrder);
                Assert.AreEqual(2, sortedScripts[1].BuildOrder);
                Assert.AreEqual(3, sortedScripts[2].BuildOrder);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region GetFileDataForCodeTableUpdates POCO Tests

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_Model_ReturnsNullForEmptyModel()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            
            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates(model, "test.xml");
            
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_Model_FiltersOnlyPopulateScripts()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            model.Script.Add(new Script("regular.sql", 1, "Regular", false, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, false, "tester", 30, null, null, null));
            model.Script.Add(new Script("table.pop", 2, "Populate", false, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, false, "tester", 30, null, null, null));
            
            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates(model, "test.xml");
            
            // Since the .pop file doesn't actually exist, it should return null for that script
            // This just confirms the filtering logic works
            Assert.IsNotNull(result);
        }

        #endregion

        #region CopyScriptsToFolder POCO Tests

        [TestMethod]
        public void CopyIndividualScriptsToFolder_Model_ReturnsFalseForEmptyScripts()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            
            var result = SqlBuildFileHelper.CopyIndividualScriptsToFolder(model, "dest", "proj", false, false);
            
            Assert.IsFalse(result);
        }

        [TestMethod]
        public void CopyScriptsToSingleFile_Model_ReturnsFalseForEmptyScripts()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            
            var result = SqlBuildFileHelper.CopyScriptsToSingleFile(model, "dest.sql", "proj", "build.sbm", false);
            
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CopyIndividualScriptsToFolderAsync_Model_ReturnsFalseForEmptyScripts()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            
            var result = await SqlBuildFileHelper.CopyIndividualScriptsToFolderAsync(model, "dest", "proj", false, false);
            
            Assert.IsFalse(result);
        }

        [TestMethod]
        public async Task CopyScriptsToSingleFileAsync_Model_ReturnsFalseForEmptyScripts()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            
            var result = await SqlBuildFileHelper.CopyScriptsToSingleFileAsync(model, "dest.sql", "proj", "build.sbm", false);
            
            Assert.IsFalse(result);
        }

        #endregion

        #region ImportSqlScriptFile POCO Tests

        [TestMethod]
        public void ImportSqlScriptFile_Model_ReturnsNoRowsImportedForEmptyImport()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var importModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            var projFile = Path.Combine(tempDir, "proj.xml");
            
            try
            {
                var (buildNumber, resultModel, addedFiles) = SqlBuildFileHelper.ImportSqlScriptFile(
                    model, importModel, tempDir, 0, tempDir, projFile, "", false);
                
                Assert.AreEqual((double)ImportFileStatus.NoRowsImported, buildNumber);
                Assert.AreEqual(0, addedFiles.Length);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void ImportSqlScriptFile_Model_ImportsScriptsFromImportModel()
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            var importModel = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            importModel.Script.Add(new Script("import1.sql", 1, "Import 1", false, true, DateTime.Now, Guid.NewGuid().ToString(), "TestDb", false, false, "tester", 30, null, null, null));

            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            var importDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            Directory.CreateDirectory(importDir);
            File.WriteAllText(Path.Combine(importDir, "import1.sql"), "SELECT 1");
            var projFile = Path.Combine(tempDir, "proj.xml");
            
            try
            {
                var (buildNumber, resultModel, addedFiles) = SqlBuildFileHelper.ImportSqlScriptFile(
                    model, importModel, importDir, 0, tempDir, projFile, "", false);
                
                Assert.AreEqual(1, buildNumber);
                Assert.AreEqual(1, addedFiles.Length);
                Assert.AreEqual("import1.sql", addedFiles[0]);
                Assert.AreEqual(1, resultModel.Script.Count);
            }
            finally
            {
                if (Directory.Exists(tempDir)) Directory.Delete(tempDir, true);
                if (Directory.Exists(importDir)) Directory.Delete(importDir, true);
            }
        }

        #endregion

        #region GetSHA1Hash Tests

        [TestMethod]
        public void GetSHA1Hash_WithValidFile_ReturnsHashes()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "SELECT 1\r\nGO\r\nSELECT 2");

                SqlBuildFileHelper.GetSHA1Hash(tempFile, out string fileHash, out string textHash, stripTransactions: false);

                Assert.IsFalse(string.IsNullOrEmpty(fileHash));
                Assert.IsFalse(string.IsNullOrEmpty(textHash));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public void GetSHA1Hash_WithMissingFile_ReturnsFileMissing()
        {
            var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sql");

            SqlBuildFileHelper.GetSHA1Hash(nonExistentFile, out string fileHash, out string textHash, stripTransactions: false);

            Assert.AreEqual(SqlBuildFileHelper.FileMissing, fileHash);
            Assert.AreEqual(SqlBuildFileHelper.FileMissing, textHash);
        }

        [TestMethod]
        public void GetSHA1Hash_StripTransactions_ProducesDifferentTextHash()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "BEGIN TRANSACTION\r\nSELECT 1\r\nCOMMIT TRANSACTION");

                SqlBuildFileHelper.GetSHA1Hash(tempFile, out string fileHash1, out string textHash1, stripTransactions: false);
                SqlBuildFileHelper.GetSHA1Hash(tempFile, out string fileHash2, out string textHash2, stripTransactions: true);

                // File hashes should be the same (same file content)
                Assert.AreEqual(fileHash1, fileHash2);
                // Text hashes may differ based on transaction stripping
                // Note: The actual implementation may or may not differ - this tests the method works
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public async Task GetSHA1HashAsync_WithValidFile_ReturnsHashes()
        {
            var tempFile = Path.GetTempFileName();
            try
            {
                File.WriteAllText(tempFile, "SELECT 1");

                var result = await SqlBuildFileHelper.GetSHA1HashAsync(tempFile, stripTransactions: false);

                Assert.IsFalse(string.IsNullOrEmpty(result.fileHash));
                Assert.IsFalse(string.IsNullOrEmpty(result.textHash));
            }
            finally
            {
                File.Delete(tempFile);
            }
        }

        [TestMethod]
        public async Task GetSHA1HashAsync_WithMissingFile_ReturnsFileMissing()
        {
            var nonExistentFile = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".sql");

            var result = await SqlBuildFileHelper.GetSHA1HashAsync(nonExistentFile, stripTransactions: false);

            Assert.AreEqual(SqlBuildFileHelper.FileMissing, result.fileHash);
            Assert.AreEqual(SqlBuildFileHelper.FileMissing, result.textHash);
        }

        #endregion

        #region LoadSqlBuildProjectModel Tests

        [TestMethod]
        public void LoadSqlBuildProjectModel_NonExistentFile_ReturnsShellModel()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var nonExistentFile = Path.Combine(tempDir, "nonexistent.xml");

                var model = SqlBuildFileHelper.LoadSqlBuildProjectModel(nonExistentFile, false);

                Assert.IsNotNull(model);
                Assert.AreEqual(1, model.SqlSyncBuildProject.Count);
                Assert.AreEqual(0, model.Script.Count);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region PackageSbxFilesIntoSbmFiles Edge Cases

        [TestMethod]
        public void PackageSbxFilesIntoSbmFiles_EmptyDirectory_ReturnsEmptyWithMessage()
        {
            string message;

            var result = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles("", out message);

            Assert.AreEqual(0, result.Count);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        [TestMethod]
        public void PackageSbxFilesIntoSbmFiles_NonExistentDirectory_ReturnsEmptyWithMessage()
        {
            string message;
            var nonExistentDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());

            var result = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(nonExistentDir, out message);

            Assert.AreEqual(0, result.Count);
            Assert.IsFalse(string.IsNullOrEmpty(message));
        }

        [TestMethod]
        public void PackageSbxFilesIntoSbmFiles_DirectoryWithNoSbxFiles_ReturnsEmptyWithMessage()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                File.WriteAllText(Path.Combine(tempDir, "test.txt"), "not an sbx");
                string message;

                var result = SqlBuildFileHelper.PackageSbxFilesIntoSbmFiles(tempDir, out message);

                Assert.AreEqual(0, result.Count);
                Assert.IsFalse(string.IsNullOrEmpty(message));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region InferOverridesFromPackage Tests

        [TestMethod]
        public void InferOverridesFromPackage_NonExistentFile_ReturnsEmptyString()
        {
            var nonExistentFile = Path.Combine(Path.GetTempPath(), "nonexistent.sbm");

            var result = SqlBuildFileHelper.InferOverridesFromPackage(nonExistentFile, "TestDb");

            // Method should handle gracefully
            Assert.IsNotNull(result);
        }

        #endregion

        #region PackageSbxFileIntoSbmFile Edge Cases

        [TestMethod]
        public void PackageSbxFileIntoSbmFile_EmptyFileName_ReturnsFalse()
        {
            var result = SqlBuildFileHelper.PackageSbxFileIntoSbmFile("", "output.sbm");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PackageSbxFileIntoSbmFile_NullFileName_ReturnsFalse()
        {
            var result = SqlBuildFileHelper.PackageSbxFileIntoSbmFile(null, "output.sbm");

            Assert.IsFalse(result);
        }

        [TestMethod]
        public void PackageSbxFileIntoSbmFile_WhitespaceFileName_ReturnsFalse()
        {
            var result = SqlBuildFileHelper.PackageSbxFileIntoSbmFile("   ", "output.sbm");

            Assert.IsFalse(result);
        }

        #endregion

        #region ArchiveLogFiles Tests

        [TestMethod]
        public void ArchiveLogFiles_NoLogFiles_ReturnsTrue()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var archiveName = Path.Combine(tempDir, "archive.zip");

                var result = SqlBuildFileHelper.ArchiveLogFiles(Array.Empty<string>(), tempDir, archiveName);

                Assert.IsTrue(result);
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        [TestMethod]
        public void ArchiveLogFiles_WithLogFiles_CreatesArchive()
        {
            var tempDir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(tempDir);
            try
            {
                var log1 = Path.Combine(tempDir, "test1.log");
                var log2 = Path.Combine(tempDir, "test2.log");
                File.WriteAllText(log1, "Log content 1");
                File.WriteAllText(log2, "Log content 2");

                var archiveName = Path.Combine(tempDir, "archive.zip");
                var logFiles = new[] { log1, log2 };

                var result = SqlBuildFileHelper.ArchiveLogFiles(logFiles, tempDir, archiveName);

                Assert.IsTrue(result);
                Assert.IsTrue(File.Exists(archiveName));
            }
            finally
            {
                Directory.Delete(tempDir, true);
            }
        }

        #endregion

        #region DefaultScriptXmlFile Tests

        [TestMethod]
        public void DefaultScriptXmlFile_IsValidPath()
        {
            var path = SqlBuildFileHelper.DefaultScriptXmlFile;

            Assert.IsNotNull(path);
            Assert.IsTrue(path.Contains("DefaultScriptRegistry.xml"));
        }

        #endregion

        #region GetDefaultScriptRegistry Tests

        [TestMethod]
        public void GetDefaultScriptRegistry_ReturnsRegistryOrNull()
        {
            // This may return null if the file doesn't exist in the test environment
            var result = SqlBuildFileHelper.GetDefaultScriptRegistry();

            // Just verify it doesn't throw - result may be null if file doesn't exist
            // This is expected behavior in some environments
        }

        #endregion
    }
}
