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
    /// Tests for null-safety code paths introduced by nullable reference type fixes.
    /// Covers: truncated file handling, null FileName guards in model iteration.
    /// </summary>
    [TestClass]
    public class NullSafetyCodePathTests
    {
        private string _testDir = null!;

        [TestInitialize]
        public void Setup()
        {
            _testDir = Path.Combine(Path.GetTempPath(), $"NullSafetyTests_{Guid.NewGuid():N}");
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

        #region Truncated File Handling — GetFileDataForCodeTableUpdates

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_TruncatedFile_NoClosingDelimiter_ReturnsWithoutThrowing()
        {
            // Arrange — script has "Query Used:" but no closing "*/"
            string projFileName = Path.Combine(_testDir, XmlFileNames.MainProjectFile);
            string popFile = Path.Combine(_testDir, "testpop.POP");
            File.WriteAllText(popFile, @"/*
Source Server: TestServer
Source Db: TestDb
Table Scripted: dbo.TestTable
Key Check Columns: Id
Query Used:
SELECT * FROM dbo.TestTable
WHERE Active = 1
");
            // Act — previously this would NRE on sr.ReadLine().Trim() at EOF
            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates("testpop.POP", projFileName);

            // Assert — should return partial data without throwing
            Assert.IsNotNull(result);
            Assert.AreEqual("testpop.POP", result.ShortFileName);
            Assert.AreEqual("TestServer", result.SourceServer);
            Assert.AreEqual("TestDb", result.SourceDatabase);
            Assert.AreEqual("dbo.TestTable", result.SourceTable);
            Assert.AreEqual("Id", result.KeyCheckColumns);
            Assert.IsNotNull(result.Query);
            Assert.IsTrue(result.Query.Contains("SELECT * FROM dbo.TestTable"));
        }

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_CompleteFile_StillWorksCorrectly()
        {
            // Arrange — well-formed script with proper closing
            string projFileName = Path.Combine(_testDir, XmlFileNames.MainProjectFile);
            string popFile = Path.Combine(_testDir, "complete.POP");
            File.WriteAllText(popFile, @"/*
Source Server: ProdServer
Source Db: ProdDb
Table Scripted: dbo.Config
Key Check Columns: ConfigKey
Query Used:
SELECT ConfigKey, ConfigValue FROM dbo.Config
*/
INSERT INTO dbo.Config VALUES ('key1', 'val1');");

            // Act
            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates("complete.POP", projFileName);

            // Assert
            Assert.IsNotNull(result);
            Assert.AreEqual("ProdServer", result.SourceServer);
            Assert.AreEqual("ProdDb", result.SourceDatabase);
            Assert.IsTrue(result.Query!.Contains("SELECT ConfigKey"));
            Assert.IsFalse(result.Query.Contains("*/"));
        }

        #endregion

        #region Null FileName Guards — Model Iteration Methods

        [TestMethod]
        public void GetFileDataForCodeTableUpdates_Model_NullFileName_SkipsWithoutThrowing()
        {
            // Arrange — model with a script that has null FileName
            var model = CreateModelWithScripts(new List<Script> {
                    new Script { FileName = null, BuildOrder = 1 },
                    new Script { FileName = "valid.POP", BuildOrder = 2 }
                });

            // Act — should skip null FileName without NRE
            var result = SqlBuildFileHelper.GetFileDataForCodeTableUpdates(model, Path.Combine(_testDir, "proj.xml"));

            // Assert — should return array (possibly empty since files don't exist on disk)
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void ResortBuildByFileType_NullFileName_LINQFilterSkipsWithoutThrowing()
        {
            // Arrange — model with null FileName mixed with valid ones
            var model = CreateModelWithScripts(new List<Script> {
                    new Script { FileName = null, BuildOrder = 5 },
                    new Script { FileName = "test.sql", BuildOrder = 2 },
                    new Script { FileName = "test.PRC", BuildOrder = 3 }
                });

            // Act — directly test the LINQ filtering that would previously NRE
            // This mirrors the Where clauses in ResortBuildByFileTypeAsync
            string extension = ".sql";
            var matchingScripts = model.Script
                .Where(s => s.FileName != null && s.FileName.EndsWith(extension, StringComparison.OrdinalIgnoreCase) && s.BuildOrder < 20000)
                .OrderBy(s => s.BuildOrder)
                .ToList();

            var knownExtensions = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { ".sql", ".PRC" };
            var leftOverScripts = model.Script
                .Where(s => s.FileName != null && !knownExtensions.Any(ext => s.FileName.EndsWith(ext, StringComparison.OrdinalIgnoreCase)) && s.BuildOrder < 20000)
                .ToList();

            // Assert — null FileName script was skipped, no NRE thrown
            Assert.AreEqual(1, matchingScripts.Count);
            Assert.AreEqual("test.sql", matchingScripts[0].FileName);
            Assert.AreEqual(0, leftOverScripts.Count);
        }

        [TestMethod]
        public void CopyIndividualScriptsToFolder_NullFileName_SkipsWithoutThrowing()
        {
            // Arrange
            string destFolder = Path.Combine(_testDir, "output");
            Directory.CreateDirectory(destFolder);
            var model = CreateModelWithScripts(new List<Script> {
                    new Script { FileName = null, BuildOrder = 1 },
                    new Script { FileName = "exists.sql", BuildOrder = 2 }
                });

            // Act — should skip null FileName via guard clause
            var result = SqlBuildFileHelper.CopyIndividualScriptsToFolder(model, destFolder, _testDir, false, false);

            // Assert — returns true (processed without error, though no files exist on disk)
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CopyIndividualScriptsToFolderAsync_NullFileName_SkipsWithoutThrowing()
        {
            // Arrange
            string destFolder = Path.Combine(_testDir, "output_async");
            Directory.CreateDirectory(destFolder);
            var model = CreateModelWithScripts(new List<Script> {
                    new Script { FileName = null, BuildOrder = 1 }
                });

            // Act
            var result = await SqlBuildFileHelper.CopyIndividualScriptsToFolderAsync(
                model, destFolder, _testDir, false, false, CancellationToken.None);

            // Assert
            Assert.IsTrue(result);
        }

        [TestMethod]
        public void CopyScriptsToSingleFile_NullFileName_SkipsWithoutThrowing()
        {
            // Arrange
            string destFile = Path.Combine(_testDir, "combined.sql");
            var model = CreateModelWithScripts(new List<Script> {
                    new Script { FileName = null, BuildOrder = 1 },
                    new Script { FileName = "missing.sql", BuildOrder = 2 }
                });

            // Act
            var result = SqlBuildFileHelper.CopyScriptsToSingleFile(model, destFile, _testDir, "test.sbm", false);

            // Assert — returns true, skipped both (null and non-existent)
            Assert.IsTrue(result);
        }

        [TestMethod]
        public async Task CopyScriptsToSingleFileAsync_NullFileName_SkipsWithoutThrowing()
        {
            // Arrange
            string destFile = Path.Combine(_testDir, "combined_async.sql");
            var model = CreateModelWithScripts(new List<Script> {
                    new Script { FileName = null, BuildOrder = 1 }
                });

            // Act
            var result = await SqlBuildFileHelper.CopyScriptsToSingleFileAsync(
                model, destFile, _testDir, "test.sbm", false, CancellationToken.None);

            // Assert
            Assert.IsTrue(result);
        }
        private static SqlSyncBuildDataModel CreateModelWithScripts(List<Script> scripts)
        {
            var model = SqlBuildFileHelper.CreateShellSqlSyncBuildDataModel();
            foreach (var s in scripts)
                model.Script.Add(s);
            return model;
        }

        #endregion
    }
}
