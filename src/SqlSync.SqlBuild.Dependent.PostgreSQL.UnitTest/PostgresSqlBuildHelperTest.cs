using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BuildModels = SqlSync.SqlBuild.Models;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;

namespace SqlSync.SqlBuild.Dependent.PostgreSQL.UnitTest
{
    /// <summary>
    /// Integration tests for the build engine running against PostgreSQL.
    /// Requires a local PostgreSQL instance. Configure via environment variables:
    ///   SBM_TEST_POSTGRES_SERVER (default: localhost)
    ///   SBM_TEST_POSTGRES_USER (default: postgres)
    ///   SBM_TEST_POSTGRES_PASSWORD (default: P0stSqlAdm1n)
    /// </summary>
    [TestClass]
    [DoNotParallelize]
    public class PostgresSqlBuildHelperTest
    {
        private class NullProgressReporter : IProgressReporter
        {
            public bool CancellationPending => false;
            public void ReportProgress(int percent, object userState) { }
        }

        private static List<PostgresInitialization> initColl;
        private static bool isPostgresAvailable = false;

        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            initColl = new List<PostgresInitialization>();
            try
            {
                var server = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_SERVER") ?? "localhost";
                var user = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_USER") ?? "postgres";
                var password = Environment.GetEnvironmentVariable("SBM_TEST_POSTGRES_PASSWORD") ?? "P0stSqlAdm1n";
                using var conn = new NpgsqlConnection($"Host={server};Database=postgres;Username={user};Password={password};Timeout=5");
                conn.Open();
                isPostgresAvailable = true;
            }
            catch
            {
                isPostgresAvailable = false;
            }
        }

        private PostgresInitialization GetInitializationObject()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            var init = new PostgresInitialization();
            initColl.Add(init);
            return init;
        }

        private static async Task<Build> RunBuildScriptsAsync(
            SqlBuildHelper sbh,
            SqlSyncBuildDataModel buildDataModel,
            Build myBuildModel,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl)
        {
            var scripts = buildDataModel.Script ?? new List<Script>();
            if (!(buildDataModel.Build?.Any(b => b.BuildId == myBuildModel.BuildId) ?? false))
            {
                var builds = buildDataModel.Build?.ToList() ?? new List<Build>();
                builds.Add(myBuildModel);
                buildDataModel = new SqlSyncBuildDataModel(
                    sqlSyncBuildProject: buildDataModel.SqlSyncBuildProject,
                    script: buildDataModel.Script,
                    build: builds,
                    scriptRun: buildDataModel.ScriptRun,
                    committedScript: buildDataModel.CommittedScript);
            }
            return await sbh.RunBuildScriptsAsync(scripts, myBuildModel, serverName, isMultiDbRun, scriptBatchColl, buildDataModel);
        }

        [ClassCleanup]
        public static void Cleanup()
        {
            if (initColl != null)
            {
                foreach (var init in initColl)
                    init.Dispose();
            }
        }

        [TestCleanup]
        public void TestCleanup()
        {
            if (initColl?.Count > 0)
            {
                var lastInit = initColl[initColl.Count - 1];
                try
                {
                    foreach (string dbName in lastInit.testDatabaseNames)
                    {
                        using var conn = new NpgsqlConnection($"Host={lastInit.serverName};Database={dbName};Username={lastInit.pgUser};Password={lastInit.pgPassword};Timeout=20");
                        conn.Open();

                        using (var cmd = new NpgsqlCommand("DELETE FROM sqlbuild_logging WHERE buildfilename LIKE 'SqlSyncTest-%'", conn))
                            cmd.ExecuteNonQuery();

                        using (var cmd = new NpgsqlCommand("DELETE FROM transactiontest WHERE message = 'INSERT TEST'", conn))
                            cmd.ExecuteNonQuery();
                    }
                }
                catch { }
            }

            DefaultSqlLoggingService.ClearLoggingTableCache();
        }

        #region RunBuildScripts - Transactional

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_FinishCommitted()
        {
            var init = GetInitializationObject();

            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus, "Build should be committed");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(sqlLoggingCount >= 1, "Should have logging rows: " + sqlLoggingCount);
            Assert.IsTrue(testTableCount >= 1, "Should have test table rows: " + testTableCount);
        }

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_FinishRollback()
        {
            var init = GetInitializationObject();

            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddFailureScript(ref buildData, true, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus, "Build should be rolled back");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, "Should have no logging rows after rollback: " + sqlLoggingCount);
            Assert.IsTrue(0 == testTableCount, "Should have no test table rows after rollback: " + testTableCount);
        }

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_SelectScript_Committed()
        {
            var init = GetInitializationObject();

            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddSelectScript(ref buildData);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus, "SELECT script should be committed");
        }

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_InsertThenSelect_Committed()
        {
            var init = GetInitializationObject();

            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddSelectScript(ref buildData);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus, "Multi-script build should be committed");
        }

        #endregion

        #region Connection and Platform Tests

        [TestMethod]
        public void PostgreSQL_ConnectionData_HasCorrectPlatform()
        {
            var init = GetInitializationObject();
            Assert.AreEqual(DatabasePlatform.PostgreSQL, init.connData.DatabasePlatform);
        }

        [TestMethod]
        public void PostgreSQL_SqlBuildHelper_UsesPgProviders()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);

            Assert.IsInstanceOfType(sbh.TransactionManager, typeof(PostgresTransactionManager), "Should use PostgresTransactionManager");
            Assert.IsInstanceOfType(sbh.SyntaxProvider, typeof(PostgresSyntaxProvider), "Should use PostgresSyntaxProvider");
            Assert.IsInstanceOfType(sbh.ResourceProvider, typeof(PostgresResourceProvider), "Should use PostgresResourceProvider");
        }

        #endregion

        #region Single Run Script

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_SingleRunScript()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, false); // allowMultipleRuns = false

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus, "Single-run script should commit");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(1, sqlLoggingCount, "Should have 1 logging row");
            Assert.AreEqual(1, testTableCount, "Should have 1 test table row");
        }

        #endregion

        #region Pre-Run Script (Skip Already-Run)

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_SkippingPreRunScript()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddPreRunScript(ref buildData, false);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            // Script should be skipped since it was "pre-run" — no new rows
            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus, "Build should commit (script skipped)");
        }

        #endregion

        #region Trial Builds

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_TrialSuccessful()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = true;

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.TrialRolledBack, actual.FinalStatus, "Trial build should roll back");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(0, sqlLoggingCount, "Trial should leave no logging rows");
            Assert.AreEqual(0, testTableCount, "Trial should leave no test rows");
        }

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_TrialWithFailure()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddFailureScript(ref buildData, true, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = true;

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus, "Trial with failure should roll back");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(0, sqlLoggingCount, "Should have no logging rows after trial rollback");
            Assert.AreEqual(0, testTableCount, "Should have no test rows after trial rollback");
        }

        #endregion

        #region Failure Tolerance

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_WithFailureDontCauseFailure()
        {
            // PostgreSQL behavior: when an error occurs inside a transaction, the transaction
            // enters an "aborted" state and no further commands can run. Unlike SQL Server,
            // PostgreSQL cannot continue after an error in a transaction even with CausesBuildFailure=false.
            // The build will roll back.
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, false); // causeBuildFailure = false

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            // PostgreSQL rolls back the whole transaction on any error
            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus, "PostgreSQL aborts transaction on error, causing rollback");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(0, sqlLoggingCount, "Should have no logging rows after rollback");
            Assert.AreEqual(0, testTableCount, "Should have no test rows after rollback");
        }

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_WithFailureDontRollbackDontCauseFailure()
        {
            // PostgreSQL behavior: same as above — the aborted transaction state
            // means the "don't rollback" flag is effectively ignored for transactional builds.
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, false, false); // rollBackOnError=false, causeBuildFailure=false

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus, "PostgreSQL aborts transaction on error");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(0, sqlLoggingCount, "Should have no logging rows after rollback");
            Assert.AreEqual(0, testTableCount, "Should have no test rows after rollback");
        }

        #endregion

        #region Bad Database Target

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_UnableToConnectToDatabase()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptWithBadDatabase(ref buildData);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus, "Bad database should cause rollback");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(0, sqlLoggingCount, "Should have no logging rows after rollback");
            Assert.AreEqual(0, testTableCount, "Should have no test rows after rollback");
        }

        #endregion

        #region Batched Scripts

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_WithPreBatchedScripts()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddBatchInsertScripts(ref buildData, true);

            IScriptBatcher scriptBatcher = new DefaultScriptBatcher();
            ScriptBatchCollection scriptBatchColl = scriptBatcher.LoadAndBatchSqlScripts(buildData, string.Empty);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus, "Batched scripts should commit");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(1, sqlLoggingCount, "Should have 1 logging row for the batch");
            Assert.AreEqual(2, testTableCount, "Should have 2 test rows (one per batch segment)");
        }

        #endregion

        #region Script-Only Mode

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_AsScriptOnly()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);
            sbh.runScriptOnly = true;

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus, "Script-only should report committed");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(0, sqlLoggingCount, "Script-only should not log to database");
            Assert.AreEqual(0, testTableCount, "Script-only should not insert rows");
        }

        #endregion

        #region Multi-Script with Select

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_InsertThenSelect_VerifyLogging()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddSelectScript(ref buildData);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(2, sqlLoggingCount, "Should have 2 logging rows (insert + select)");
            Assert.AreEqual(1, testTableCount, "Should have 1 test table row (from insert only)");
        }

        #endregion

        #region Alternate Logging Database

        [TestMethod]
        [Ignore("Alternate logging DB with PostgreSQL requires additional connection routing work")]
        public async Task PostgreSQL_RunBuildScripts_AlternateLoggingDb()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddSelectScript(ref buildData);

            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            ((ISqlBuildRunnerProperties)target).LogToDatabaseName = init.testDatabaseNames[1];

            Build myBuild = init.GetRunBuildRow(target);

            var actual = await RunBuildScriptsAsync(target, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus, "Alternate logging should commit");

            // Logging goes to database index 1, data goes to index 0
            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(1);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(2, sqlLoggingCount, "Should have 2 logging rows in alternate db");
            Assert.AreEqual(1, testTableCount, "Should have 1 test table row in primary db");
            Assert.AreEqual(init.testDatabaseNames[1], ((ISqlBuildRunnerProperties)target).LogToDatabaseName);
        }

        #endregion

        #region Non-Transactional Builds

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_NonTransactional_FinishCommitted()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus, "Non-transactional should commit");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(1, sqlLoggingCount, "Should have 1 logging row");
            Assert.AreEqual(1, testTableCount, "Should have 1 test table row");
        }

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_NonTransactional_FinishCommittedWithScripting()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, true); // withScriptLog = true
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus, "Non-transactional with scripting should commit");

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(1, sqlLoggingCount, "Should have 1 logging row");
            Assert.AreEqual(1, testTableCount, "Should have 1 test table row");
        }

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_NonTransactional_Failure()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.FailedNoTransaction, actual.FinalStatus, "Non-transactional failure should report FailedNoTransaction");

            // In non-transactional mode, the successful script's work persists
            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(1, sqlLoggingCount, "Successful script should have logged");
            Assert.AreEqual(1, testTableCount, "Successful script's insert should persist");
        }

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_NonTransactional_WithFailureDontCauseFailure()
        {
            // Note: In non-transactional mode on PostgreSQL, each script runs independently.
            // Even with CausesBuildFailure=false, PostgreSQL's error propagation currently
            // causes FailedNoTransaction. This documents the current behavior.
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, false);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            // PostgreSQL non-transactional: successful script's data persists regardless
            Assert.IsTrue(actual.FinalStatus == BuildItemStatus.Committed || actual.FinalStatus == BuildItemStatus.FailedNoTransaction,
                $"Expected Committed or FailedNoTransaction, got {actual.FinalStatus}");

            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(testTableCount >= 1, "Successful script's insert should persist: " + testTableCount);
        }

        [TestMethod]
        public async Task PostgreSQL_RunBuildScripts_NonTransactional_WithFailureDontRollbackDontCauseFailure()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, false, false);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.IsTrue(actual.FinalStatus == BuildItemStatus.Committed || actual.FinalStatus == BuildItemStatus.FailedNoTransaction,
                $"Expected Committed or FailedNoTransaction, got {actual.FinalStatus}");

            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(testTableCount >= 1, "Successful script's insert should persist: " + testTableCount);
        }

        #endregion

        #region Logging Service Operations

        [TestMethod]
        public async Task PostgreSQL_LogTableExists_ReturnsTrue()
        {
            var init = GetInitializationObject();
            var factory = ConnectionHelper.GetFactory(DatabasePlatform.PostgreSQL);
            var conn = factory.CreateConnection(init.connData);

            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            var resourceProvider = new PostgresResourceProvider();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter, resourceProvider);

            bool actual = await sqlLoggingService.LogTableExists(conn);
            Assert.IsTrue(actual, "Logging table should exist in test database");
        }

        [TestMethod]
        public async Task PostgreSQL_LogTableExists_InvalidDb_ReturnsFalse()
        {
            var init = GetInitializationObject();
            var badConnData = init.CreateConnectionData("nonexistent_database_xyz");
            var factory = ConnectionHelper.GetFactory(DatabasePlatform.PostgreSQL);
            var conn = factory.CreateConnection(badConnData);

            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            var resourceProvider = new PostgresResourceProvider();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter, resourceProvider);

            bool actual = await sqlLoggingService.LogTableExists(conn);
            Assert.IsFalse(actual, "Logging table should not exist in nonexistent database");
        }

        [TestMethod]
        public async Task PostgreSQL_LogTableExists_NoDB_ReturnsFalse()
        {
            var init = GetInitializationObject();
            // Use the admin database "postgres" which shouldn't have our logging table
            var adminConnData = init.CreateConnectionData("postgres");
            var factory = ConnectionHelper.GetFactory(DatabasePlatform.PostgreSQL);
            var conn = factory.CreateConnection(adminConnData);

            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            var resourceProvider = new PostgresResourceProvider();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter, resourceProvider);

            bool actual = await sqlLoggingService.LogTableExists(conn);
            Assert.IsFalse(actual, "Admin database should not have the logging table");
        }

        #endregion

        #region HasBlockingSqlLog

        [TestMethod]
        public void PostgreSQL_HasBlockingSqlLog_FindsExistingEntry()
        {
            var init = GetInitializationObject();
            init.InsertPreRunScriptEntry();

            Guid scriptId = new Guid(init.PreRunScriptGuid);
            string databaseName = init.connData.DatabaseName;

            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            var resourceProvider = new PostgresResourceProvider();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter, resourceProvider);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null, resourceProvider);

            bool actual = dbUtil.HasBlockingSqlLog(scriptId, init.connData, databaseName, out string scriptHash, out string scriptTextHash, out DateTime commitDate);

            Assert.IsTrue(actual, "Should find pre-run script entry");
            Assert.AreEqual("MadeUpHash", scriptHash, "Script hash should match");
            Assert.IsFalse(string.IsNullOrEmpty(scriptTextHash), "Script text hash should not be empty");
        }

        [TestMethod]
        public void PostgreSQL_HasBlockingSqlLog_DoesNotFindMissing()
        {
            var init = GetInitializationObject();

            Guid scriptId = Guid.NewGuid(); // Non-existent
            string databaseName = init.connData.DatabaseName;

            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            var resourceProvider = new PostgresResourceProvider();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter, resourceProvider);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null, resourceProvider);

            bool actual = dbUtil.HasBlockingSqlLog(scriptId, init.connData, databaseName, out string scriptHash, out string scriptTextHash, out DateTime commitDate);

            Assert.IsFalse(actual, "Should not find non-existent script entry");
            Assert.AreEqual("", scriptHash);
            Assert.AreEqual("", scriptTextHash);
        }

        [TestMethod]
        public void PostgreSQL_HasBlockingSqlLog_InvalidDbName()
        {
            var init = GetInitializationObject();

            Guid scriptId = new Guid(init.PreRunScriptGuid);

            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            var resourceProvider = new PostgresResourceProvider();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter, resourceProvider);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null, resourceProvider);

            bool actual = dbUtil.HasBlockingSqlLog(scriptId, init.connData, "invalidDatabaseName", out string scriptHash, out string scriptTextHash, out DateTime commitDate);

            Assert.IsFalse(actual, "Invalid database name should return false");
            Assert.AreEqual("", scriptHash);
            Assert.AreEqual("", scriptTextHash);
            Assert.AreEqual(DateTime.MinValue, commitDate);
        }

        #endregion

        #region GetBlockingSqlLog

        [TestMethod]
        public void PostgreSQL_GetBlockingSqlLog_FindsAndDoesntFindEntries()
        {
            var init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddPreRunScript(ref buildData, false);

            Guid scriptId = new Guid(init.PreRunScriptGuid);
            var factory = ConnectionHelper.GetFactory(DatabasePlatform.PostgreSQL);
            BuildConnectData connData = new BuildConnectData();
            connData.Connection = factory.CreateConnection(init.connData);
            connData.DatabaseName = init.connData.DatabaseName;
            connData.HasLoggingTable = true;
            connData.ServerName = init.serverName;
            connData.Transaction = null;

            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            var resourceProvider = new PostgresResourceProvider();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter, resourceProvider);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null, resourceProvider);

            try
            {
                connData.Connection.Open();

                // Should find the pre-run script
                bool actual = dbUtil.GetBlockingSqlLog(scriptId, ref connData);
                Assert.IsTrue(actual, "Should find pre-run script entry");

                // Should not find a random GUID
                actual = dbUtil.GetBlockingSqlLog(Guid.NewGuid(), ref connData);
                Assert.IsFalse(actual, "Should not find random script ID");
            }
            finally
            {
                if (connData.Connection.State == ConnectionState.Open)
                    connData.Connection.Close();
            }

            // Closed connection should return false gracefully
            bool closedResult = dbUtil.GetBlockingSqlLog(scriptId, ref connData);
            Assert.IsFalse(closedResult, "Closed connection should return false");
        }

        #endregion

        #region .: ClearScriptBlocks :.

        [TestMethod]
        [Ignore("ClearScriptBlocks is not yet implemented (throws NotImplementedException)")]
        public void PostgreSQL_ClearScriptBlocks()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            ConnectionData connData = init.connData;
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);

            Models.CommittedScript row = new();
            row.ScriptId = buildData.Script[0].ScriptId;
            row.ServerName = init.serverName;
            row.AllowScriptBlock = true;
            buildData.CommittedScript.Add(row);

            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            var resourceProvider = new PostgresResourceProvider();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter, resourceProvider);
            ISqlBuildFileHelper fileHelper = new DefaultSqlBuildFileHelper();
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, fileHelper, resourceProvider);

            string[] scripts = new string[] { Guid.NewGuid().ToString(), buildData.Script[0].ScriptId };
            ClearScriptData scrData = new ClearScriptData(scripts, buildData, init.projectFileName, init.projectFileName);

            IProgressReporter reporter = new NullProgressReporter();
            dbUtil.ClearScriptBlocks(scrData, connData, reporter, null);
            Assert.AreEqual(false, buildData.CommittedScript[0].AllowScriptBlock);
        }

        #endregion

        #region .: RecordCommittedScripts :.

        [TestMethod]
        public void PostgreSQL_RecordCommittedScripts()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();

            Guid scriptID = Guid.NewGuid();
            string hash = "WETRWEW@#$@$WEQW#$#R";
            LoggingCommittedScript script = new LoggingCommittedScript(scriptID, hash, 20, "My script text", "TAGID", init.serverName, init.testDatabaseNames[0]);
            List<LoggingCommittedScript> committedScripts = new List<LoggingCommittedScript>();
            committedScripts.Add(script);

            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            SqlSyncBuildDataModel buildModelUpdated;
            buildModelUpdated = target.BuildFinalizer.RecordCommittedScripts(committedScripts, buildData);
            Assert.IsNotNull(buildModelUpdated);

            Assert.AreEqual(scriptID.ToString(), buildData.CommittedScript[0].ScriptId);
            Assert.AreEqual(true, buildData.CommittedScript[0].AllowScriptBlock);
            Assert.AreEqual(hash, buildData.CommittedScript[0].ScriptHash);
        }

        #endregion

        #region .: GetScriptRunLog :.

        [TestMethod]
        public async Task PostgreSQL_GetScriptRunLog_ReturnsEntries()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            init.InsertPreRunScriptEntry();

            Guid scriptId = new Guid(init.PreRunScriptGuid);
            ConnectionData connData = init.connData;
            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            var resourceProvider = new PostgresResourceProvider();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter, resourceProvider);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null, resourceProvider);

            var actual = dbUtil.GetScriptRunLog(scriptId, connData);
            Assert.IsTrue(actual.Count > 0, string.Format("Missing rows for pre-run script. {0}", PostgresInitialization.MissingDatabaseErrorMessage));

            actual = dbUtil.GetScriptRunLog(Guid.NewGuid(), connData);
            Assert.IsTrue(actual.Count == 0, "Rows found for new unique script id.");
        }

        [TestMethod]
        public void PostgreSQL_GetScriptRunLog_InvalidDb_ThrowsException()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();

            Guid scriptId = new Guid(init.PreRunScriptGuid);
            ConnectionData connData = init.connData;
            connData.DatabaseName = "invalidDatabaseName";
            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            var resourceProvider = new PostgresResourceProvider();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter, resourceProvider);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null, resourceProvider);

            Assert.ThrowsExactly<ApplicationException>(() => dbUtil.GetScriptRunLog(scriptId, connData));
        }

        #endregion

        #region .: SaveBuildDataSet :.

        [TestMethod]
        public async Task PostgreSQL_SaveBuildDataSet_NullProjectFileName()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.projectFileName = null;
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => target.BuildFinalizer.SaveBuildDataModelAsync(target, false));
        }

        [TestMethod]
        public async Task PostgreSQL_SaveBuildDataSet_EmptyProjectFileName()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.projectFileName = string.Empty;
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => target.BuildFinalizer.SaveBuildDataModelAsync(target, false));
        }

        [TestMethod]
        public async Task PostgreSQL_SaveBuildDataSet_NullBuildHistoryName()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.projectFileName = init.projectFileName;
            target.buildHistoryXmlFile = null;
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => target.BuildFinalizer.SaveBuildDataModelAsync(target, false));
        }

        [TestMethod]
        public async Task PostgreSQL_SaveBuildDataSet_EmptyBuildHistoryName()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.projectFileName = init.projectFileName;
            target.buildHistoryXmlFile = string.Empty;
            await Assert.ThrowsExactlyAsync<ArgumentException>(() => target.BuildFinalizer.SaveBuildDataModelAsync(target, false));
        }

        #endregion

        #region .: ProcessBuild End-to-End :.

        [TestMethod]
        public async Task PostgreSQL_ProcessBuild_SingleInsert_CommitsAndLogs()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddSelectScript(ref buildData);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(sqlLoggingCount > 0, "Expected logging entries after committed build, got " + sqlLoggingCount);
            Assert.IsTrue(testTableCount > 0, "Expected test table entries after committed build, got " + testTableCount);
        }

        [TestMethod]
        public async Task PostgreSQL_ProcessBuild_FailureCausesBuildFailure_RollsBack()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.AreEqual(0, sqlLoggingCount, "Should have 0 logging entries after rollback");
            Assert.AreEqual(0, testTableCount, "Should have 0 test table entries after rollback");
        }

        [TestMethod]
        public async Task PostgreSQL_ProcessBuild_MultipleInserts_AllCommit()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(testTableCount >= 3, "Expected at least 3 test table entries, got " + testTableCount);
        }

        [TestMethod]
        public async Task PostgreSQL_ProcessBuild_AsMultiDbRun()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            Build myBuild = init.GetRunBuildRow(sbh);

            // isMultiDbRun = true
            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, true, null);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);
        }

        [TestMethod]
        public async Task PostgreSQL_ProcessBuild_NonTransactional_FailureThenSuccess()
        {
            if (!isPostgresAvailable)
                Assert.Inconclusive("PostgreSQL is not available.");

            PostgresInitialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            // First: a failure that doesn't cause build failure
            init.AddFailureScript(ref buildData, false, false);
            // Then: a success script
            init.AddInsertScript(ref buildData, true);

            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            Build myBuild = init.GetRunBuildRow(sbh);

            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, false, null);

            // Non-transactional with non-failing failure should still complete  
            Assert.AreNotEqual(BuildItemStatus.RolledBack, actual.FinalStatus, "Should not be RolledBack in non-transactional mode");
        }

        #endregion
    }
}
