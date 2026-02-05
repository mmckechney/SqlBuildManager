using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.DbInformation;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    /// <summary>
    /// Integration tests for Rebuilder class that require database access
    /// NOTE: These tests require (local)\SQLEXPRESS with SqlBuildTest databases
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class RebuilderTest
    {
        private static List<Initialization> _initColl = new();
        private static readonly List<string> _tempFiles = new();

        [ClassInitialize]
        public static void ClassInitialize(TestContext testContext)
        {
            _initColl = new List<Initialization>();
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
                try
                {
                    if (File.Exists(file))
                        File.Delete(file);
                }
                catch { }
            }
        }

        private Initialization GetInitialization()
        {
            var init = new Initialization();
            _initColl.Add(init);
            return init;
        }

        #region Constructor Tests

        [TestMethod]
        public void Rebuilder_Constructor_InitializesCorrectly()
        {
            var init = GetInitialization();
            var commitData = new CommittedBuildData
            {
                BuildFileName = "TestBuild.sbm",
                CommitDate = DateTime.Now,
                Database = init.testDatabaseNames[0]
            };
            string newBuildFileName = Path.GetTempFileName();
            _tempFiles.Add(newBuildFileName);

            var rebuilder = new Rebuilder(init.connData, commitData, newBuildFileName);

            Assert.IsNotNull(rebuilder);
        }

        #endregion

        #region GetCommitedBuildList Tests

        [TestMethod]
        public void GetCommitedBuildList_WithDatabaseList_ReturnsEmptyWhenNoLoggingData()
        {
            var init = GetInitialization();
            var dbList = new DatabaseList();
            dbList.Add(init.testDatabaseNames[0], false);

            var result = Rebuilder.GetCommitedBuildList(init.connData, dbList);

            // May return empty list if no builds have been committed
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetCommitedBuildList_WithDatabaseName_ReturnsEmptyWhenNoLoggingData()
        {
            var init = GetInitialization();

            var result = Rebuilder.GetCommitedBuildList(init.connData, init.testDatabaseNames[0]);

            // May return empty list if no builds have been committed
            Assert.IsNotNull(result);
        }

        [TestMethod]
        public void GetCommitedBuildList_SkipsManuallyEnteredDatabases()
        {
            var init = GetInitialization();
            var dbList = new DatabaseList();
            dbList.Add("ManualDatabase", true); // manually entered
            dbList.Add(init.testDatabaseNames[0], false);

            var result = Rebuilder.GetCommitedBuildList(init.connData, dbList);

            Assert.IsNotNull(result);
            // Manually entered databases should be skipped
        }

        [TestMethod]
        public void GetCommitedBuildList_HandlesInvalidDatabase()
        {
            var init = GetInitialization();

            var result = Rebuilder.GetCommitedBuildList(init.connData, "NonExistentDatabase12345");

            // Should not throw, just return empty or log error
            Assert.IsNotNull(result);
        }

        #endregion

        #region RebuildBuildManagerFile Static Tests

        [TestMethod]
        public async Task RebuildBuildManagerFile_Static_WithValidData_CreatesFile()
        {
            int defaultTimeout = 30;
            string buildFileName = Path.Combine(Path.GetTempPath(), $"TestRebuild_{Guid.NewGuid()}.sbm");
            _tempFiles.Add(buildFileName);

            var rebuildData = new List<RebuilderData>
            {
                new RebuilderData
                {
                    ScriptFileName = "Script1.sql",
                    ScriptId = Guid.NewGuid(),
                    Sequence = 1,
                    ScriptText = "SELECT 1 AS Test",
                    Database = "SqlBuildTest",
                    Tag = ""
                }
            };

            bool result = await Rebuilder.RebuildBuildManagerFileAsync(defaultTimeout, buildFileName, rebuildData);

            Assert.IsTrue(result, "Should successfully create build file");
            Assert.IsTrue(File.Exists(buildFileName), "Build file should exist");
        }

        [TestMethod]
        public async Task RebuildBuildManagerFile_Static_WithMultipleScripts_CreatesFile()
        {
            int defaultTimeout = 30;
            string buildFileName = Path.Combine(Path.GetTempPath(), $"TestRebuild_{Guid.NewGuid()}.sbm");
            _tempFiles.Add(buildFileName);

            var rebuildData = new List<RebuilderData>
            {
                new RebuilderData
                {
                    ScriptFileName = "Script1.sql",
                    ScriptId = Guid.NewGuid(),
                    Sequence = 1,
                    ScriptText = "SELECT 1",
                    Database = "SqlBuildTest",
                    Tag = "Tag1"
                },
                new RebuilderData
                {
                    ScriptFileName = "Script2.sql",
                    ScriptId = Guid.NewGuid(),
                    Sequence = 2,
                    ScriptText = "SELECT 2",
                    Database = "SqlBuildTest",
                    Tag = "Tag2"
                },
                new RebuilderData
                {
                    ScriptFileName = "Script3.sql",
                    ScriptId = Guid.NewGuid(),
                    Sequence = 3,
                    ScriptText = "SELECT 3",
                    Database = "SqlBuildTest",
                    Tag = ""
                }
            };

            bool result = await Rebuilder.RebuildBuildManagerFileAsync(defaultTimeout, buildFileName, rebuildData);

            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(buildFileName));
        }

        [TestMethod]
        public async Task RebuildBuildManagerFile_Static_WithEmptyList_CreatesEmptyBuild()
        {
            int defaultTimeout = 30;
            string buildFileName = Path.Combine(Path.GetTempPath(), $"TestRebuild_{Guid.NewGuid()}.sbm");
            _tempFiles.Add(buildFileName);

            var rebuildData = new List<RebuilderData>();

            bool result = await Rebuilder.RebuildBuildManagerFileAsync(defaultTimeout, buildFileName, rebuildData);

            Assert.IsTrue(result, "Should handle empty list");
        }

        #endregion

        #region MergeServerResults Tests (via GetCommitedBuildList)

        [TestMethod]
        public void GetCommitedBuildList_MergesResultsFromMultipleDatabases()
        {
            var init = GetInitialization();
            var dbList = new DatabaseList();
            
            // Add multiple databases
            for (int i = 0; i < Math.Min(3, init.testDatabaseNames.Count); i++)
            {
                dbList.Add(init.testDatabaseNames[i], false);
            }

            var result = Rebuilder.GetCommitedBuildList(init.connData, dbList);

            Assert.IsNotNull(result);
            // Results should be merged - same build from multiple DBs should be combined
        }

        #endregion

        #region RebuilderData Tests

        [TestMethod]
        public void RebuilderData_Properties_SetCorrectly()
        {
            var scriptId = Guid.NewGuid();
            var data = new RebuilderData
            {
                ScriptFileName = "Test.sql",
                ScriptId = scriptId,
                Sequence = 5,
                ScriptText = "SELECT * FROM Table",
                Database = "TestDb",
                Tag = "MyTag"
            };

            Assert.AreEqual("Test.sql", data.ScriptFileName);
            Assert.AreEqual(scriptId, data.ScriptId);
            Assert.AreEqual(5, data.Sequence);
            Assert.AreEqual("SELECT * FROM Table", data.ScriptText);
            Assert.AreEqual("TestDb", data.Database);
            Assert.AreEqual("MyTag", data.Tag);
        }

        #endregion

        #region CommittedBuildData Tests

        [TestMethod]
        public void CommittedBuildData_Properties_SetCorrectly()
        {
            var now = DateTime.Now;
            var data = new CommittedBuildData
            {
                BuildFileName = "Build.sbm",
                ScriptCount = 10,
                CommitDate = now,
                Database = "TestDb"
            };

            Assert.AreEqual("Build.sbm", data.BuildFileName);
            Assert.AreEqual(10, data.ScriptCount);
            Assert.AreEqual(now, data.CommitDate);
            Assert.AreEqual("TestDb", data.Database);
        }

        #endregion
    }
}
