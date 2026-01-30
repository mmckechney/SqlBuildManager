using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    /// <summary>
    /// Integration tests for QueryCollectionRunner that require database access
    /// NOTE: These tests require (local)\SQLEXPRESS with SqlBuildTest databases
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class QueryCollectionRunnerTest
    {
        private static List<Initialization> _initColl = new();
        private static readonly List<string> _tempFiles = new();
        private static readonly List<string> _tempDirs = new();

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

        private string CreateTempWorkingDir()
        {
            string dir = Path.Combine(Path.GetTempPath(), $"QueryRunnerTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(dir);
            _tempDirs.Add(dir);
            return dir;
        }

        #region Constructor Tests

        [TestMethod]
        public void QueryCollectionRunner_Constructor_InitializesCorrectly()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT 1",
                new List<QueryRowItem>(),
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            Assert.IsNotNull(runner);
        }

        [TestMethod]
        public void QueryCollectionRunner_Query_GetSet()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT 1",
                new List<QueryRowItem>(),
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            runner.Query = "SELECT 2";
            
            Assert.AreEqual("SELECT 2", runner.Query);
        }

        [TestMethod]
        public void QueryCollectionRunner_AppendData_GetSet()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var appendData = new List<QueryRowItem>
            {
                new QueryRowItem { ColumnName = "Col1", Value = "Val1" }
            };
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT 1",
                appendData,
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            Assert.AreEqual(1, runner.AppendData.Count);
        }

        #endregion

        #region CollectQueryData Tests

        [TestMethod]
        public async Task CollectQueryData_SimpleQuery_ReturnsResults()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT TOP 5 name FROM sys.tables",
                new List<QueryRowItem>(),
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            var (resultCode, resultFile) = await runner.CollectQueryData();

            Assert.AreEqual(0, resultCode, "Query should succeed with code 0");
            Assert.IsFalse(string.IsNullOrEmpty(resultFile), "Should return result file path");
        }

        [TestMethod]
        public async Task CollectQueryData_XMLFormat_ReturnsResults()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT TOP 5 name FROM sys.tables",
                new List<QueryRowItem>(),
                ReportType.XML,
                workingDir,
                30,
                init.connData);

            var (resultCode, resultFile) = await runner.CollectQueryData();

            Assert.AreEqual(0, resultCode);
            Assert.IsFalse(string.IsNullOrEmpty(resultFile));
        }

        [TestMethod]
        public async Task CollectQueryData_CountQuery_ReturnsCount()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT COUNT(*) AS TableCount FROM sys.tables",
                new List<QueryRowItem>(),
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            var (resultCode, resultFile) = await runner.CollectQueryData();

            Assert.AreEqual(0, resultCode);
        }

        [TestMethod]
        public async Task CollectQueryData_SqlBuildLoggingQuery_Works()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT TOP 10 * FROM SqlBuild_Logging",
                new List<QueryRowItem>(),
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            var (resultCode, resultFile) = await runner.CollectQueryData();

            // 0 = success, even if no rows returned
            Assert.AreEqual(0, resultCode);
        }

        [TestMethod]
        public async Task CollectQueryData_InvalidQuery_ReturnsError()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT * FROM NonExistentTable12345",
                new List<QueryRowItem>(),
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            var (resultCode, resultFile) = await runner.CollectQueryData();

            // Should return error code (not 0)
            Assert.AreNotEqual(0, resultCode, "Invalid query should return non-zero result code");
        }

        [TestMethod]
        public async Task CollectQueryData_WithAppendData_IncludesAppendData()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var appendData = new List<QueryRowItem>
            {
                new QueryRowItem { ColumnName = "ServerTag", Value = "TestServer" }
            };
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT DB_NAME() AS DatabaseName",
                appendData,
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            var (resultCode, resultFile) = await runner.CollectQueryData();

            Assert.AreEqual(0, resultCode);
        }

        [TestMethod]
        public async Task CollectQueryData_MultipleColumns_ReturnsAllColumns()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT name, object_id, type, create_date FROM sys.tables",
                new List<QueryRowItem>(),
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            var (resultCode, resultFile) = await runner.CollectQueryData();

            Assert.AreEqual(0, resultCode);
        }

        [TestMethod]
        public async Task CollectQueryData_NullValues_HandledCorrectly()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            // Query that may return NULL values
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT BuildFileName, Tag FROM SqlBuild_Logging WHERE 1=0 UNION ALL SELECT NULL, NULL",
                new List<QueryRowItem>(),
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            var (resultCode, resultFile) = await runner.CollectQueryData();

            Assert.AreEqual(0, resultCode);
        }

        #endregion

        #region Dispose Tests

        [TestMethod]
        public async Task Dispose_CleansUpTempFile()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT 1",
                new List<QueryRowItem>(),
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            var (resultCode, resultFile) = await runner.CollectQueryData();
            
            runner.Dispose();

            // File should be deleted after dispose
            Assert.IsFalse(File.Exists(resultFile), "Temp file should be cleaned up after Dispose");
        }

        #endregion

        #region Event Tests

        [TestMethod]
        public async Task CollectQueryData_FiresUpdateEvent()
        {
            var init = GetInitialization();
            string workingDir = CreateTempWorkingDir();
            bool eventFired = false;
            
            var runner = new QueryCollectionRunner(
                init.serverName,
                init.testDatabaseNames[0],
                "SELECT 1",
                new List<QueryRowItem>(),
                ReportType.CSV,
                workingDir,
                30,
                init.connData);

            runner.QueryCollectionRunnerUpdate += (sender, args) =>
            {
                eventFired = true;
                Assert.AreEqual(init.serverName, args.Server);
                Assert.AreEqual(init.testDatabaseNames[0], args.Database);
            };

            await runner.CollectQueryData();

            Assert.IsTrue(eventFired, "Update event should be fired");
        }

        #endregion

        #region QueryRowItem Tests

        [TestMethod]
        public void QueryRowItem_Properties_SetCorrectly()
        {
            var item = new QueryRowItem
            {
                ColumnName = "TestColumn",
                Value = "TestValue"
            };

            Assert.AreEqual("TestColumn", item.ColumnName);
            Assert.AreEqual("TestValue", item.Value);
        }

        #endregion

        #region QueryCollectionRunnerUpdateEventArgs Tests

        [TestMethod]
        public void QueryCollectionRunnerUpdateEventArgs_Constructor_SetsProperties()
        {
            var args = new QueryCollectionRunnerUpdateEventArgs("ServerA", "DatabaseB", "Test message");

            Assert.AreEqual("ServerA", args.Server);
            Assert.AreEqual("DatabaseB", args.Database);
            Assert.AreEqual("Test message", args.Message);
        }

        #endregion
    }
}
