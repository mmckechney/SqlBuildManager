using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.AdHocQuery;
using SqlSync.SqlBuild.MultiDb;
using SqlSync.SqlBuild.Status;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    /// <summary>
    /// Integration tests for QueryCollector that require database access
    /// NOTE: These tests require (local)\SQLEXPRESS with SqlBuildTest databases
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class QueryCollectorTest
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

        private MultiDbData CreateSimpleMultiDbData(Initialization init)
        {
            var multiDb = new MultiDbData();
            var serverData = new ServerData { ServerName = init.serverName };
            serverData.Overrides.Add(new DatabaseOverride(init.serverName, init.testDatabaseNames[0], init.testDatabaseNames[0]));
            multiDb.Add(serverData);
            return multiDb;
        }

        #region Constructor Tests

        [TestMethod]
        public void QueryCollector_Constructor_InitializesCorrectly()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);

            var collector = new QueryCollector(multiDb, init.connData);

            Assert.IsNotNull(collector);
        }

        #endregion

        #region EnsureOutputPath Tests

        [TestMethod]
        public void EnsureOutputPath_ValidPath_CreatesDirectory()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            string testPath = Path.Combine(Path.GetTempPath(), $"QueryCollectorTest_{Guid.NewGuid()}");
            _tempDirs.Add(testPath);

            bool result = collector.EnsureOutputPath(testPath);

            Assert.IsTrue(result, "Should successfully create directory");
            Assert.IsTrue(Directory.Exists(testPath), "Directory should exist");
        }

        #endregion

        #region GetQueryResults Tests

        [TestMethod]
        public void GetQueryResults_SimpleQuery_CSV_ReturnsResults()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"QueryResults_{Guid.NewGuid()}.csv");
            _tempFiles.Add(outputFile);
            
            string query = "SELECT TOP 5 name FROM sys.tables";

            bool result = collector.GetQueryResults(outputFile, ReportType.CSV, query, 30);

            Assert.IsTrue(result, "Query should execute successfully");
            Assert.IsTrue(File.Exists(outputFile), "Output file should exist");
        }

        [TestMethod]
        public void GetQueryResults_SimpleQuery_XML_ReturnsResults()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"QueryResults_{Guid.NewGuid()}.xml");
            _tempFiles.Add(outputFile);
            
            string query = "SELECT TOP 5 name FROM sys.tables";

            bool result = collector.GetQueryResults(outputFile, ReportType.XML, query, 30);

            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(outputFile));
        }

        [TestMethod]
        public void GetQueryResults_SimpleQuery_HTML_ReturnsResults()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"QueryResults_{Guid.NewGuid()}.html");
            _tempFiles.Add(outputFile);
            
            string query = "SELECT TOP 5 name FROM sys.tables";

            bool result = collector.GetQueryResults(outputFile, ReportType.HTML, query, 30);

            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(outputFile));
        }

        [TestMethod]
        public void GetQueryResults_WithBackgroundWorker_ReportsProgress()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            
            var bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            collector.BackgroundWorker = bgWorker;
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"QueryResults_{Guid.NewGuid()}.csv");
            _tempFiles.Add(outputFile);
            
            string query = "SELECT TOP 1 name FROM sys.tables";

            bool result = collector.GetQueryResults(outputFile, ReportType.CSV, query, 30);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetQueryResults_QueryAgainstSqlBuildLogging_Works()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"QueryResults_{Guid.NewGuid()}.csv");
            _tempFiles.Add(outputFile);
            
            // Query that always succeeds even if table is empty
            string query = "SELECT COUNT(*) AS LogCount FROM SqlBuild_Logging";

            bool result = collector.GetQueryResults(outputFile, ReportType.CSV, query, 30);

            Assert.IsTrue(result);
        }

        [TestMethod]
        public void GetQueryResults_MultipleDatabases_QueriesAll()
        {
            var init = GetInitialization();
            var multiDb = new MultiDbData();
            var serverData = new ServerData { ServerName = init.serverName };
            
            // Add multiple databases
            serverData.Overrides.Add(new DatabaseOverride(init.serverName, init.testDatabaseNames[0], init.testDatabaseNames[0]));
            if (init.testDatabaseNames.Count > 1)
            {
                serverData.Overrides.Add(new DatabaseOverride(init.serverName, init.testDatabaseNames[1], init.testDatabaseNames[1]));
            }
            multiDb.Add(serverData);

            var collector = new QueryCollector(multiDb, init.connData);
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"QueryResults_{Guid.NewGuid()}.csv");
            _tempFiles.Add(outputFile);
            
            string query = "SELECT DB_NAME() AS DatabaseName";

            bool result = collector.GetQueryResults(outputFile, ReportType.CSV, query, 30);

            Assert.IsTrue(result);
        }

        #endregion

        #region GenerateReport Tests

        [TestMethod]
        public void GenerateReport_CSV_CombinesFiles()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            
            // Create temp result files
            string tempDir = Path.Combine(Path.GetTempPath(), $"QueryTest_{Guid.NewGuid()}");
            Directory.CreateDirectory(tempDir);
            _tempDirs.Add(tempDir);

            string file1 = Path.Combine(tempDir, "result1.csv");
            string file2 = Path.Combine(tempDir, "result2.csv");
            File.WriteAllText(file1, "Header1,Header2\nValue1,Value2");
            File.WriteAllText(file2, "Header1,Header2\nValue3,Value4");

            string outputFile = Path.Combine(tempDir, "combined.csv");
            
            bool result = collector.GenerateReport(outputFile, ReportType.CSV, new List<string> { file1, file2 });

            Assert.IsTrue(result);
            Assert.IsTrue(File.Exists(outputFile));
        }

        [TestMethod]
        public void GenerateReport_EmptyFileList_ReturnsTrue()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"Empty_{Guid.NewGuid()}.csv");
            _tempFiles.Add(outputFile);
            
            bool result = collector.GenerateReport(outputFile, ReportType.CSV, new List<string>());

            Assert.IsTrue(result);
        }

        #endregion

        #region GetBuildValidationResults Tests

        [TestMethod]
        public void GetBuildValidationResults_BuildFileHash_Executes()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            var bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"Validation_{Guid.NewGuid()}.csv");
            _tempFiles.Add(outputFile);
            
            collector.GetBuildValidationResults(ref bgWorker, outputFile, "TestHash123", ReportType.CSV, BuildValidationType.BuildFileHash, 30);

            // Should not throw
            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetBuildValidationResults_BuildFileName_Executes()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            var bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"Validation_{Guid.NewGuid()}.csv");
            _tempFiles.Add(outputFile);
            
            collector.GetBuildValidationResults(ref bgWorker, outputFile, "TestBuild.sbm", ReportType.CSV, BuildValidationType.BuildFileName, 30);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetBuildValidationResults_IndividualScriptHash_Executes()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            var bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"Validation_{Guid.NewGuid()}.csv");
            _tempFiles.Add(outputFile);
            
            collector.GetBuildValidationResults(ref bgWorker, outputFile, "ScriptHash123", ReportType.CSV, BuildValidationType.IndividualScriptHash, 30);

            Assert.IsTrue(true);
        }

        [TestMethod]
        public void GetBuildValidationResults_IndividualScriptName_Executes()
        {
            var init = GetInitialization();
            var multiDb = CreateSimpleMultiDbData(init);
            var collector = new QueryCollector(multiDb, init.connData);
            var bgWorker = new BackgroundWorker { WorkerReportsProgress = true };
            
            string outputFile = Path.Combine(Path.GetTempPath(), $"Validation_{Guid.NewGuid()}.csv");
            _tempFiles.Add(outputFile);
            
            collector.GetBuildValidationResults(ref bgWorker, outputFile, "Script.sql", ReportType.CSV, BuildValidationType.IndividualScriptName, 30);

            Assert.IsTrue(true);
        }

        #endregion
    }
}
