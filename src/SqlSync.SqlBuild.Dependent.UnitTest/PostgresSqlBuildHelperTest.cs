using Microsoft.VisualStudio.TestTools.UnitTesting;
using Npgsql;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SqlSync.SqlBuild.Dependent.UnitTest
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
        private static List<PostgresInitialization> initColl;
        private static bool isPostgresAvailable = false;

        [ClassInitialize]
        public static void InitializeTests(TestContext testContext)
        {
            initColl = new List<PostgresInitialization>();
            // Check if PostgreSQL is available
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

            // Clear the logging table verification cache between tests
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
    }
}
