using Microsoft.Azure.Amqp.Framing;
using Microsoft.Data.SqlClient;
using Microsoft.SqlServer.Dac.Model;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.SqlBuild.Models;
using SqlSync.SqlBuild.Services;
using SqlSync.SqlBuild.SqlLogging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BuildModels = SqlSync.SqlBuild.Models;
using LoggingCommittedScript = SqlSync.SqlBuild.SqlLogging.CommittedScript;
namespace SqlSync.SqlBuild.Dependent.UnitTest
{


    /// <summary>
    ///This is a test class for SqlBuildHelperTest and is intended
    ///to contain all SqlBuildHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    [DoNotParallelize()]
    public class SqlBuildHelperTest
    {
        private class NullProgressReporter :IProgressReporter
        {
            public bool CancellationPending => false;
            public void ReportProgress(int percent, object userState)
            {
                // Do nothing
            }
        }
        private static List<Initialization> initColl;

        [ClassInitialize()]
        public static void InitializeTests(TestContext testContext)
        {
            initColl = new List<Initialization>();
        }
        private Initialization GetInitializationObject()
        {
            Initialization init = new Initialization();
            initColl.Add(init);
            return init;
        }

        private static async Task<BuildModels.Build> RunBuildScriptsAsync(
            SqlBuildHelper sbh,
            SqlSyncBuildDataModel buildDataModel,
            Build myBuildModel,
            string serverName,
            bool isMultiDbRun,
            ScriptBatchCollection scriptBatchColl)
            
        {
            var scripts = buildDataModel.Script ?? new List<BuildModels.Script>();
;
            if (!(buildDataModel.Build?.Any(b => b.BuildId == myBuildModel.BuildId) ?? false))
            {
                var builds = buildDataModel.Build?.ToList() ?? new List<BuildModels.Build>();
                builds.Add(myBuildModel);
                buildDataModel = new BuildModels.SqlSyncBuildDataModel(
                    sqlSyncBuildProject: buildDataModel.SqlSyncBuildProject,
                    script: buildDataModel.Script,
                    build: builds,
                    scriptRun: buildDataModel.ScriptRun,
                    committedScript: buildDataModel.CommittedScript);
            }
            return await sbh.RunBuildScriptsAsync(scripts, myBuildModel, serverName, isMultiDbRun, scriptBatchColl, buildDataModel);
        }
        [ClassCleanup()]
        public static void Cleanup()
        {
            for (int i = 0; i < initColl.Count; i++)
            {
                initColl[i].Dispose();
            }
        }

        [TestCleanup()]
        public void TestCleanup()
        {
            // Clean up database data immediately after each test
            // This prevents data accumulation when running multiple tests
            if (initColl?.Count > 0)
            {
                var lastInit = initColl[initColl.Count - 1];
                try
                {
                    // Force immediate cleanup by deleting ALL records, not just old ones
                    for (int i = 0; i < lastInit.testDatabaseNames.Count; i++)
                    {
                        string connString = String.Format(lastInit.connectionString, lastInit.testDatabaseNames[i]);
                        using (var conn = new SqlConnection(connString))
                        {
                            conn.Open();
                            
                            // Clean SqlBuild_Logging table
                            using (var cmd = new SqlCommand("DELETE FROM SqlBuild_Logging WHERE BuildFileName LIKE 'SqlSyncTest-%'", conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                            
                            // Clean TransactionTest table
                            using (var cmd = new SqlCommand("DELETE FROM TransactionTest WHERE Message = 'INSERT TEST'", conn))
                            {
                                cmd.ExecuteNonQuery();
                            }
                        }
                    }
                }
                catch (Exception)
                {
                    // Ignore cleanup errors
                }
            }
        }

        #region .: RunBuildScripts  - Transactional :.

        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_FinishCommitted()
        {
            Initialization init = GetInitializationObject();

            //Create the build package...
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());


        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_FinishRollback()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddFailureScript(ref buildData, true, true);


            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test

            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(0 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        /// <summary>
        ///A test for RunBuildScripts. Missing view containing scripts to run..
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_MissingViewData()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();


            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test


            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            var actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);
            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, String.Format("Invalid SqlBuild_Logging Count: {0}. {1}", sqlLoggingCount.ToString(), Initialization.MissingDatabaseErrorMessage));
            Assert.IsTrue(0 == testTableCount, String.Format("Invalid TransactionTest Count: {0}. {1}", testTableCount.ToString(), Initialization.MissingDatabaseErrorMessage));
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_UnableToConnectToDatabase()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptWithBadDatabase(ref buildData);


            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);

            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, String.Format("Invalid SqlBuild_Logging Count: {0}. {1}", sqlLoggingCount.ToString(), Initialization.MissingDatabaseErrorMessage));
            Assert.IsTrue(0 == testTableCount, String.Format("Invalid TransactionTest Count:  {0}. {1}", testTableCount.ToString(), Initialization.MissingDatabaseErrorMessage));
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_SingleRunScript()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, false);

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test

            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);

            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_SkippingPreRunScript()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildDataModel = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddPreRunScript(ref buildDataModel, false);
            

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildDataModel);

            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            // Build model with committed script to trigger skip
            var scripts = buildDataModel.Script ?? new List<BuildModels.Script>();
            var csList = buildDataModel.CommittedScript?.ToList() ?? new List<BuildModels.CommittedScript>();
            var preRun = buildDataModel.Script[0];
            csList.Add(new BuildModels.CommittedScript(preRun.ScriptId.ToString(), init.serverName, DateTime.UtcNow, null, null, null));
            buildDataModel = new BuildModels.SqlSyncBuildDataModel(
                sqlSyncBuildProject: buildDataModel.SqlSyncBuildProject,
                script: buildDataModel.Script,
                build: buildDataModel.Build,
                scriptRun: buildDataModel.ScriptRun,
                committedScript: csList);
            actual = await sbh.RunBuildScriptsAsync(scripts, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, buildDataModel);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByScriptId(0, buildDataModel.Script[0].ScriptId);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(sqlLoggingCount >= 1, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(0 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        [Ignore("With removal of background worker need to reintroduce cancellation token")]
        public async Task RunBuildScriptsTest_WithPendingCancellation()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);

            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);

            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(0 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_TrialSuccessful()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = true;
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.TrialRolledBack, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(0 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_TrialWithFailure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddFailureScript(ref buildData, true, true);
            



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = true;
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(0 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_WithFailureDontCauseFailure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, false);
            



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = false;
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_WithFailureDontRollbackDontCauseFailure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, false, false);
            



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = false;
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_WithPreBatchedScripts()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddBatchInsertScripts(ref buildData, true);
            

            bool isMultiDbRun = false;
            IScriptBatcher scriptBatcher = new DefaultScriptBatcher();
            ScriptBatchCollection scriptBatchColl = scriptBatcher.LoadAndBatchSqlScripts(buildData, string.Empty);
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);

            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(2 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_AsScriptOnly()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null;
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            //Set the script only flag
            sbh.runScriptOnly = true;
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(0 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_SelectData()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddSelectScript(ref buildData);
            

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null;
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(2 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }

        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_AlternateLoggingDb()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddSelectScript(ref buildData);
            

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null;
            BuildModels.Build actual;

            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            ((ISqlBuildRunnerProperties)target).LogToDatabaseName = init.testDatabaseNames[1];

            //Get BuildRow...
            Build myBuild = new(); //init.GetRunBuildRow(po);
            //Execute the run...
            actual = await RunBuildScriptsAsync(target, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(1);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(2 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());
            Assert.AreEqual(init.testDatabaseNames[1], ((ISqlBuildRunnerProperties)target).LogToDatabaseName);

        }
        #endregion

        #region .: RunBuildScripts  - Non-Transactional :.
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_NonTransactional_FinishCommitted()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper target = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(target);
            //Execute the run...
            actual = await RunBuildScriptsAsync(target, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_NonTransactional_FinishCommittedWithScripting()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, true);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_NonTransactional_Failure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);
            

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.FailedNoTransaction, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_NonTransactional_WithFailureDontCauseFailure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, false);
            



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = false;
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public async Task RunBuildScriptsTest_NonTransactional_WithFailureDontRollbackDontCauseFailure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, false, false);
            



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = false;
            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        [Ignore("With removal of background worker need to reintroduce cancellation token")]
        public async Task RunBuildScriptsTest_NonTransactional_Cancelled()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            BuildModels.Build actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);

            //Get BuildRow...
            Build myBuild = init.GetRunBuildRow(sbh);

            //Execute the run...
            actual = await RunBuildScriptsAsync(sbh, buildData, myBuild, init.serverName, isMultiDbRun, scriptBatchColl);

            Assert.AreEqual(BuildItemStatus.FailedNoTransaction, actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(0 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());
        }
        #endregion

        #region .: GetTargetDatabase :.
        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void GetTargetDatabaseTest_GetDefaultBack()
        {
            Initialization init = GetInitializationObject();
            SqlBuildHelper helper = init.CreateSqlBuildHelper_Basic();

            string defaultDatabase = "MyTestDatabase";
            string expected = defaultDatabase;
            string actual;
            actual = helper.GetTargetDatabase(defaultDatabase);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void GetTargetDatabaseTest_GetOverrideBack()
        {
            Initialization init = GetInitializationObject();
            SqlBuildHelper helper = init.CreateSqlBuildHelper_Basic();
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("Server1", "MyDefault", "MyOverride"));
            helper.targetDatabaseOverrides = overrides;
            string defaultDatabase = "MyDefault";
            string expected = "MyOverride";
            string actual;
            actual = helper.GetTargetDatabase(defaultDatabase);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void GetTargetDatabaseTest_DefaultSetToEmptyString()
        {
            Initialization init = GetInitializationObject();
            SqlBuildHelper helper = init.CreateSqlBuildHelper_Basic();
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("Server1", "MyDefault", "MyOverride"));
            helper.targetDatabaseOverrides = overrides;
            string defaultDatabase = string.Empty;
            string expected = "MyOverride";
            string actual;
            actual = helper.GetTargetDatabase(defaultDatabase);
            Assert.AreEqual(expected, actual);
        }
        // <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void GetTargetDatabaseTest_DefaultSetToNull()
        {
            Initialization init = GetInitializationObject();
            SqlBuildHelper helper = init.CreateSqlBuildHelper_Basic();
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("Server1", "MyDefault", "MyOverride"));
            helper.targetDatabaseOverrides = overrides;
            string defaultDatabase = null;
            string expected = "MyOverride";
            string actual;
            actual = helper.GetTargetDatabase(defaultDatabase);
            Assert.AreEqual(expected, actual);
        }

        // <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void GetTargetDatabaseTest_MultipleOverridesSet()
        {
            Initialization init = GetInitializationObject();
            SqlBuildHelper helper = init.CreateSqlBuildHelper_Basic();
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("Server1", "MyDefault", "MyOverride"));
            overrides.Add(new DatabaseOverride("Server1", "MyDefault2", "MyOverride2"));
            helper.targetDatabaseOverrides = overrides;
            string defaultDatabase = "MyDefault2";
            string expected = "MyOverride2";
            string actual;
            actual = helper.GetTargetDatabase(defaultDatabase);
            Assert.AreEqual(expected, actual);
        }

        // <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void GetTargetDatabaseTest_DoesNotFindDefault()
        {
            Initialization init = GetInitializationObject();
            SqlBuildHelper helper = init.CreateSqlBuildHelper_Basic();
            List<DatabaseOverride> overrides = new List<DatabaseOverride>();
            overrides.Add(new DatabaseOverride("Server1", "MyDefault", "MyOverride"));
            overrides.Add(new DatabaseOverride("Server1", "MyDefault2", "MyOverride2"));
            helper.targetDatabaseOverrides = overrides;
            string defaultDatabase = "MyDefault3";
            string expected = defaultDatabase;
            string actual;
            actual = helper.GetTargetDatabase(defaultDatabase);
            Assert.AreEqual(expected, actual);
        }
        #endregion

        #region .: GetFromResources :.
        /// <summary>
        ///A test for GetFromResources
        ///</summary>
        [TestMethod()]
        public void GetFromResourcesTest()
        {
            ConnectionData data = null;
            SqlBuildHelper target = new SqlBuildHelper(data);
            string resourceName = "SqlSync.SqlBuild.SqlLogging.LogScript.sql";
            string expected = @"INSERT INTO SqlBuild_Logging([BuildFileName],[ScriptFileName],[ScriptId],[ScriptFileHash],[CommitDate],[Sequence],[UserId],[AllowScriptBlock],[ScriptText],[Tag],[TargetDatabase],[RunWithVersion],[BuildProjectHash],[BuildRequestedBy],[ScriptRunStart],[ScriptRunEnd],[Description])
VALUES(@BuildFileName,@ScriptFileName,@ScriptId,@ScriptFileHash,@CommitDate,@Sequence, @UserId, 1,@ScriptText,@Tag,@TargetDatabase,@RunWithVersion,@BuildProjectHash,@BuildRequestedBy, @ScriptRunStart, @ScriptRunEnd, @Description)";
            string actual;
            actual = target.GetFromResources(resourceName);
            Assert.AreEqual(expected, actual);
        }
        [TestMethod()]
        [ExpectedException(typeof(System.ApplicationException))]
        public void GetFromResourcesTest_GetException()
        {
            ConnectionData data = null;
            SqlBuildHelper target = new SqlBuildHelper(data);
            string resourceName = "SqlSync.SqlBuild.SqlLogging.NOT_HERE";
            target.GetFromResources(resourceName);
        }
        #endregion

        #region .: PrepareBuildForRun :.
        /// <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task PrepareBuildForRunTest_Success()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = false;
            var prep = await target.PrepareBuildForRunAsync(((ISqlBuildRunnerProperties)target).BuildDataModel, serverName, isMultiDbRun, null);
            Assert.AreEqual(1, prep.FilteredScripts.Count);
            Assert.AreEqual(System.Environment.UserName, prep.Build.UserId);
            Assert.AreEqual(serverName, prep.Build.ServerName);

        }

        /// <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task PrepareBuildForRunTest_Success2()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddSelectScript(ref buildData);
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = false;
            var prep = await target.PrepareBuildForRunAsync(((ISqlBuildRunnerProperties)target).BuildDataModel, serverName, isMultiDbRun, null);
            Assert.AreEqual(1, prep.FilteredScripts.Count);
            Assert.AreEqual(System.Environment.UserName, prep.Build.UserId);
            Assert.AreEqual(serverName, prep.Build.ServerName);


        }

        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task PrepareBuildForRun_Poco_Success()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = false;

            var prep = await target.PrepareBuildForRunAsync(((ISqlBuildRunnerProperties)target).BuildDataModel, serverName, isMultiDbRun, null);
            Assert.AreEqual(1, prep.FilteredScripts.Count);
            Assert.AreEqual(Environment.UserName, prep.Build.UserId);
            Assert.AreEqual(serverName, prep.Build.ServerName);
            Assert.IsFalse(string.IsNullOrEmpty(prep.BuildPackageHash));
        }

        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task PrepareBuildForRun_Poco_NoScripts()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = false;
            var prep = await target.PrepareBuildForRunAsync(((ISqlBuildRunnerProperties)target).BuildDataModel, serverName, isMultiDbRun, null);
            Assert.AreEqual(0, prep.FilteredScripts.Count);
            Assert.AreEqual(Environment.UserName, prep.Build.UserId);
            Assert.AreEqual(serverName, prep.Build.ServerName);
            Assert.IsTrue(string.IsNullOrEmpty(prep.BuildPackageHash));
            Assert.AreEqual(BuildItemStatus.RolledBack, prep.Build.FinalStatus);
        }

        /// <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task PrepareBuildForRunTest_SuccessWithScriptBatch()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddSelectScript(ref buildData);
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);


            string serverName = init.serverName;
            bool isMultiDbRun = false;


            ScriptBatchCollection coll = init.GetScriptBatchCollection();

            var prep = await target.PrepareBuildForRunAsync(((ISqlBuildRunnerProperties)target).BuildDataModel, serverName, isMultiDbRun, coll);
            Assert.AreEqual(1, prep.FilteredScripts.Count);
            Assert.AreEqual(System.Environment.UserName, prep.Build.UserId);
            Assert.AreEqual(serverName, prep.Build.ServerName);
            Assert.AreEqual("DAA5499C760402A0F64DE5393BF82BB45E382075", prep.BuildPackageHash, "Invalid hash, did the methodology change?");

        }


        /// <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task PrepareBuildForRunTest_SuccessWithMultipleScripts()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = false;
            target.runItemIndexes = new double[] { 0, 1 };
            var prep = await target.PrepareBuildForRunAsync(((ISqlBuildRunnerProperties)target).BuildDataModel, serverName, isMultiDbRun, null);
            Assert.AreEqual(3, prep.FilteredScripts.Count);
            Assert.AreEqual(System.Environment.UserName, prep.Build.UserId);
            Assert.AreEqual(serverName, prep.Build.ServerName);

        }
        // <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task PrepareBuildForRunTest_WithNoScriptsAdded()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = false;
            var prep = await target.PrepareBuildForRunAsync(((ISqlBuildRunnerProperties)target).BuildDataModel, serverName, isMultiDbRun, null);
            Assert.AreEqual(0, prep.FilteredScripts.Count);
            Assert.AreEqual(System.Environment.UserName, prep.Build.UserId);
            Assert.AreEqual(serverName, prep.Build.ServerName);
            Assert.AreEqual(BuildItemStatus.RolledBack, prep.Build.FinalStatus);
        }
        // <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task PrepareBuildForRunTest_WithNoScriptsAdded_MultiDbSet()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = true;
            var prep = await target.PrepareBuildForRunAsync(((ISqlBuildRunnerProperties)target).BuildDataModel, serverName, isMultiDbRun, null);

            Assert.AreEqual(0, prep.FilteredScripts.Count);
            Assert.AreEqual(System.Environment.UserName, prep.Build.UserId);
            Assert.AreEqual(serverName, prep.Build.ServerName);
            Assert.AreEqual(BuildItemStatus.PendingRollBack, prep.Build.FinalStatus);
        }
        #endregion

        #region .: GetTargetDatabase :.
        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        //[TestMethod()]
        //public void GetTargetDatabaseTest_MultiServer()
        //{
        //    DatabaseOverride override1a = new DatabaseOverride("default1", "override1");
        //    DatabaseOverride override1b = new DatabaseOverride("default2", "override2");
        //    DatabaseOverride override2a = new DatabaseOverride("default3", "override3");
        //    DatabaseOverride override2b = new DatabaseOverride("default4", "override4");
        //    DatabaseOverride override3a = new DatabaseOverride("default5", "override5");
        //    DatabaseOverride override3b = new DatabaseOverride("default6", "override6");

        //    List<DatabaseOverride> lstOvr1 = new List<DatabaseOverride>();
        //    lstOvr1.Add(override1a);
        //    lstOvr1.Add(override1b);

        //    List<DatabaseOverride> lstOvr2 = new List<DatabaseOverride>();
        //    lstOvr2.Add(override2a);
        //    lstOvr2.Add(override2b);

        //    List<DatabaseOverride> lstOvr3 = new List<DatabaseOverride>();
        //    lstOvr3.Add(override3a);
        //    lstOvr3.Add(override3b);

        //    DbOverrides overrideSeq1 = new DbOverrides();
        //    overrideSeq1.AddRange(lstOvr1);
        //    overrideSeq1.AddRange(lstOvr2);

        //    DbOverrides overrideSeq2 = new DbOverrides();
        //    overrideSeq1.AddRange(lstOvr3);

        //    ServerData server1 = new ServerData();
        //    server1.ServerName = "Server1\\Instance1";
        //    server1.Overrides.AddRange(lstOvr1);
        //    server1.Overrides.AddRange(lstOvr2);

        //    ServerData server2 = new ServerData();
        //    server2.ServerName = "Server2\\Instance2";
        //    server2.Overrides.AddRange(lstOvr3);

        //    MultiDbData multiDbRunData = new MultiDbData();
        //    multiDbRunData.Add(server1);
        //    multiDbRunData.Add(server2);



        //    string actual = SqlBuildHelper.GetTargetDatabase("Server2\\Instance2", "default5", multiDbRunData);
        //    Assert.AreEqual("override5", actual);

        //    actual = SqlBuildHelper.GetTargetDatabase("Server1\\Instance1", "default1", multiDbRunData);
        //    Assert.AreEqual("override1", actual);

        //    //Can't find server case...
        //    actual = SqlBuildHelper.GetTargetDatabase("ServerZZZZ", "defaultZZZZ", multiDbRunData);
        //    Assert.AreEqual("defaultZZZZ", actual);

        //    //Can't find override database case...
        //    actual = SqlBuildHelper.GetTargetDatabase("Server2\\Instance2", "defaultXX", multiDbRunData);
        //    Assert.AreEqual("defaultXX", actual);
        //}

        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void GetTargetDatabaseTest_SingleServer()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            DatabaseOverride override1 = new DatabaseOverride("Server1", "default1", "override1");
            DatabaseOverride override2 = new DatabaseOverride("Server1","default2", "override2");
            DatabaseOverride override3 = new DatabaseOverride("Server1", "default3", "override3");
            List<DatabaseOverride> ovr = new List<DatabaseOverride>();
            ovr.Add(override1);
            ovr.Add(override2);
            ovr.Add(override3);
            target.targetDatabaseOverrides = ovr;
            string actual = target.GetTargetDatabase("default2");
            Assert.AreEqual("override2", actual);

            actual = target.GetTargetDatabase("default3");
            Assert.AreEqual("override3", actual);

            actual = target.GetTargetDatabase("");
            Assert.AreEqual("override1", actual);

            actual = target.GetTargetDatabase(null);
            Assert.AreEqual("override1", actual);

        }

        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void GetTargetDatabaseTest_UseStaticOverride()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            DatabaseOverride override1 = new DatabaseOverride("Server1", "default1", "override1");
            DatabaseOverride override2 = new DatabaseOverride("Server1", "default2", "override2");
            DatabaseOverride override3 = new DatabaseOverride("Server1", "default3", "override3");
            List<DatabaseOverride> ovr = new List<DatabaseOverride>();
            ovr.Add(override1);
            ovr.Add(override2);
            ovr.Add(override3);
            OverrideData.TargetDatabaseOverrides = ovr;

            Assert.IsNull(target.targetDatabaseOverrides);
            Assert.IsNull(((ISqlBuildRunnerProperties)target).TargetDatabaseOverrides);

            string actual = target.GetTargetDatabase("default2");
            Assert.AreEqual("override2", actual);

            actual = target.GetTargetDatabase("default3");
            Assert.AreEqual("override3", actual);

            actual = target.GetTargetDatabase("defaultXXX");
            Assert.AreEqual("defaultXXX", actual);

            actual = target.GetTargetDatabase("");
            Assert.AreEqual("override1", actual);

            actual = target.GetTargetDatabase(null);
            Assert.AreEqual("override1", actual);



        }
        #endregion

        #region .: GetScriptRunLog :.
        /// <summary>
        ///A test for GetScriptRunLog
        ///</summary>
        [TestMethod()]
        public void GetScriptRunLogTest()
        {
            Initialization init = GetInitializationObject();
            init.InsertPreRunScriptEntry();

            Guid scriptId = new Guid(init.PreRunScriptGuid);
            ConnectionData connData = init.connData;
            IReadOnlyList<ScriptRunLogEntry> actual;
            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter,null);

            actual = dbUtil.GetScriptRunLog(scriptId, connData);
            Assert.IsTrue(actual.Count > 0, String.Format("Missing rows for pre-run script. {0}", Initialization.MissingDatabaseErrorMessage));

            actual = dbUtil.GetScriptRunLog(Guid.NewGuid(), connData);
            Assert.IsTrue(actual.Count == 0, String.Format("Rows found for new unique script id. {0}", Initialization.MissingDatabaseErrorMessage));


        }

        /// <summary>
        ///A test for GetScriptRunLog
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ApplicationException))]
        public void GetScriptRunLogTest_ExpectException()
        {
            Initialization init = GetInitializationObject();

            Guid scriptId = new Guid(init.PreRunScriptGuid);
            ConnectionData connData = init.connData;
            connData.DatabaseName = "invalidDatabaseName";
            IReadOnlyList<ScriptRunLogEntry> actual;
            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null);
            actual = dbUtil.GetScriptRunLog(scriptId, connData);
        }
        #endregion

        #region .: SqlBuildHelper_ScriptLogWriteEvent :.
        /// <summary>
        ///A test for SqlBuildHelper_ScriptLogWriteEvent
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        [ExpectedException(typeof(NullReferenceException))]
        public void SqlBuildHelper_ScriptLogWriteEventTest_ExpectException()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            //Reset the scriptLogFileName to NULL to get exception
            target.scriptLogFileName = null;
            object sender = null;
            ScriptLogEventArgs e = new ScriptLogEventArgs(10, "SELECT TestCol FROM [test].[TestTable] WHERE TestCol IS NOT NULL", init.testDatabaseNames[0], "C:\test.sql", "Test Executed");
            target.SqlBuildHelper_ScriptLogWriteEvent(sender, false, e);
        }
        /// <summary>
        ///A test for SqlBuildHelper_ScriptLogWriteEvent
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void SqlBuildHelper_ScriptLogWriteEventTest_CreateScriptLogFile()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            //Delete the file made by the init the scriptLogFileName to NULL to get exception
            if (System.IO.File.Exists(target.scriptLogFileName))
                System.IO.File.Delete(target.scriptLogFileName);

            object sender = null;
            ScriptLogEventArgs e = new ScriptLogEventArgs(10, "SELECT TestCol FROM [test].[TestTable] WHERE TestCol IS NOT NULL", init.testDatabaseNames[0], @"C:\test.sql", "Test Executed");
            target.SqlBuildHelper_ScriptLogWriteEvent(sender, false, e);

            string contents = File.ReadAllText(target.scriptLogFileName);
            Assert.IsTrue(contents.IndexOf("SELECT TestCol FROM [test].[TestTable]") > -1);
            Assert.IsTrue(contents.IndexOf(@"C:\test.sql") > -1);

            //Create the "end" entry for an external log file...
            string fileName = init.GetTrulyUniqueFile();
            target.externalScriptLogFileName = fileName;
            if (File.Exists(fileName))
                File.Delete(fileName);

            e = new ScriptLogEventArgs(-10000, "SELECT TestCol FROM [test].[TestTable] WHERE TestCol IS NOT NULL", init.testDatabaseNames[0], @"C:\test.sql", "Test Executed");
            target.SqlBuildHelper_ScriptLogWriteEvent(sender,false, e);
            contents = File.ReadAllText(target.externalScriptLogFileName);
            Assert.IsTrue(contents.IndexOf("-- END Time:") > -1);
        }

        /// <summary>
        ///A test for SqlBuildHelper_ScriptLogWriteEvent
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void SqlBuildHelper_ScriptLogWriteEventTest_EnterExceptionHandling()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            //Delete the file made by the init the scriptLogFileName to NULL to get exception
            if (System.IO.File.Exists(target.scriptLogFileName))
                System.IO.File.Delete(target.scriptLogFileName);

            object sender = null;

            //Create the "end" entry for an external log file...
            string fileName = init.GetTrulyUniqueFile();
            target.externalScriptLogFileName = fileName;
            File.SetAttributes(fileName, FileAttributes.ReadOnly); //Make read-only so the copy throws an exception

            ScriptLogEventArgs e = new ScriptLogEventArgs(-10000, "SELECT TestCol FROM [test].[TestTable] WHERE TestCol IS NOT NULL", init.testDatabaseNames[0], @"C:\test.sql", "Test Executed");
            target.SqlBuildHelper_ScriptLogWriteEvent(sender, false, e);

            FileInfo inf = new FileInfo(target.externalScriptLogFileName);
            Assert.IsTrue(inf.Length == 0); //file should be zero size because move should have failed.

            File.SetAttributes(fileName, FileAttributes.Normal); //Set back to normal for the test clean-up process.

        }

        /// <summary>
        ///A test for SqlBuildHelper_ScriptLogWriteEvent
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void SqlBuildHelper_ScriptLogWriteEventTest_NonTransactional()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.isTransactional = false;

            //Delete the file made by the init the scriptLogFileName to NULL to get exception
            if (System.IO.File.Exists(target.scriptLogFileName))
                System.IO.File.Delete(target.scriptLogFileName);

            object sender = null;
            ScriptLogEventArgs e = new ScriptLogEventArgs(10, "SELECT TestCol FROM [test].[TestTable] WHERE TestCol IS NOT NULL", init.testDatabaseNames[0], @"C:\test.sql", "Test Executed");
            target.SqlBuildHelper_ScriptLogWriteEvent(sender, false, e);

            string contents = File.ReadAllText(target.scriptLogFileName);
            Assert.IsTrue(contents.IndexOf("-- Executed without a transaction --") > -1);
        }
        #endregion

        #region .: CommitBuild :.
        /// <summary>
        ///A test for CommitBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void CommitBuildTest_NonTransactional()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.isTransactional = false;
            
            bool actual = target.BuildFinalizer.CommitBuild(target.ConnectionsService, target.isTransactional);
            Assert.AreEqual(true, actual);

        }

        /// <summary>
        ///A test for CommitBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void CommitBuildTest_TransactionFailure()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            BuildConnectData connData = target.ConnectionsService.GetOrAddBuildConnectionDataClassWithLocalAuth(init.serverName, init.testDatabaseNames[0], target.isTransactional);
            connData.Transaction.Rollback(); //Rollback transaction to make it unusable

            bool actual = target.BuildFinalizer.CommitBuild(target.ConnectionsService, target.isTransactional);
            Assert.AreEqual(false, actual);

        }
        // <summary>
        ///A test for CommitBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void CommitBuildTest_ClosedConnectionFailure()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            BuildConnectData connData = target.ConnectionsService.GetOrAddBuildConnectionDataClassWithLocalAuth(init.serverName, init.testDatabaseNames[0], target.isTransactional);
            connData.Transaction.Rollback(); //Rollback transaction to make it unusable
            connData.Connection.Close();
            connData.Connection = null; //Set connection to null to create exception when trying to close.

            bool actual = target.BuildFinalizer.CommitBuild(target.ConnectionsService,target.isTransactional);
            Assert.AreEqual(false, actual);

        }
        #endregion

        #region .: RollbackBuild :.
        /// <summary>
        ///A test for RollbackBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void RollbackBuildTest_NonTransactional()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.isTransactional = false;

            bool actual = target.BuildFinalizer.RollbackBuild(target.ConnectionsService,target.isTransactional);
            Assert.AreEqual(false, actual);
        }
        /// <summary>
        ///A test for RollbackBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void RollbackBuildTest_TransactionFailure()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            IConnectionsService connectionsService = new DefaultConnectionsService();
            BuildConnectData connData = connectionsService.GetOrAddBuildConnectionDataClassWithLocalAuth(init.serverName, init.testDatabaseNames[0], target.isTransactional);
            connData.Transaction.Rollback(); //Rollback transaction to make it unusable

            bool actual = target.BuildFinalizer.RollbackBuild(target.ConnectionsService, target.isTransactional);
            Assert.AreEqual(true, actual);

        }
        /// <summary>
        ///A test for RollbackBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void RollbackBuildTest_ClosedConnectionFailure()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            IConnectionsService connectionsService = new DefaultConnectionsService();
            BuildConnectData connData = connectionsService.GetOrAddBuildConnectionDataClassWithLocalAuth(init.serverName, init.testDatabaseNames[0], target.isTransactional);
            connData.Transaction.Rollback(); //Rollback transaction to make it unusable
            connData.Connection.Close();
            connData.Connection = null; //Set connection to null to create exception when trying to close.

            bool actual = target.BuildFinalizer.RollbackBuild(target.ConnectionsService, target.isTransactional);
            Assert.AreEqual(true, actual);

        }
        #endregion

        #region .: PerformScriptTokenReplacement
        /// <summary>
        ///A test for PerformScriptTokenReplacement
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void PerformScriptTokenReplacementTest()
        {
            Initialization init = GetInitializationObject();
            SqlBuildHelper helper = init.CreateSqlBuildHelper_Basic();
            string script = @"INSERT INTO dbo.versions 
			(versionnumber,versiondate,State,Comment) 
		VALUES
			('#BuildDescription#',getdate(),'Completed','#BuildFileName#')";

            string expected = @"INSERT INTO dbo.versions 
			(versionnumber,versiondate,State,Comment) 
		VALUES
			('TestDescription',getdate(),'Completed','TestBuildFile.sbm')";

            //Set the build description
            helper.buildDescription = "TestDescription";
            //Set the build file name
            helper.buildFileName = "TestBuildFile.sbm";

            string actual = helper.PerformScriptTokenReplacement(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PerformScriptTokenReplacement
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void PerformScriptTokenReplacementTest_NullBuildFileName()
        {
            Initialization init = GetInitializationObject();
            SqlBuildHelper helper = init.CreateSqlBuildHelper_Basic();
            string script = @"INSERT INTO dbo.versions 
			(versionnumber,versiondate,State,Comment) 
		VALUES
			('#BuildDescription#',getdate(),'Completed','#BuildFileName#')";

            string expected = @"INSERT INTO dbo.versions 
			(versionnumber,versiondate,State,Comment) 
		VALUES
			('TestDescription',getdate(),'Completed','sbx file')";

            //Set the build description
            helper.buildDescription = "TestDescription";
            //Set the build file name
            helper.buildFileName = null;

            string actual = helper.PerformScriptTokenReplacement(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for PerformScriptTokenReplacement
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void PerformScriptTokenReplacementTest_NullDescriptionValue()
        {
            Initialization init = GetInitializationObject();
            SqlBuildHelper helper = init.CreateSqlBuildHelper_Basic();
            string script = @"INSERT INTO dbo.versions 
			(versionnumber,versiondate,State,Comment) 
		VALUES
			('#BuildDescription#',getdate(),'Completed','#BuildFileName#')";

            string expected = @"INSERT INTO dbo.versions 
			(versionnumber,versiondate,State,Comment) 
		VALUES
			('',getdate(),'Completed','TestBuildFile.sbm')";

            //Set the build description
            helper.buildDescription = null;
            //Set the build file name
            helper.buildFileName = "TestBuildFile.sbm";

            string actual = helper.PerformScriptTokenReplacement(script);
            Assert.AreEqual(expected, actual);
        }

        #endregion

        /// <summary>
        ///A test for GetScriptSourceTable
        ///</summary>
        [TestMethod()]
        public void GetScriptSourceTableTest()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);
            IList<Script> actual = null;
            actual = buildData.Script;

            Assert.IsInstanceOfType(actual, typeof(IList<Script>));
            Assert.IsTrue(1 == actual.Count);
            Assert.AreEqual(actual[0].ScriptId, buildData.Script[0].ScriptId);
            Assert.AreEqual(actual[0].FileName, buildData.Script[0].FileName);
        }

        /// <summary>
        ///A test for LoadAndBatchSqlScripts
        ///</summary>
        [TestMethod()]
        public void LoadAndBatchSqlScriptsTest()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddBatchInsertScripts(ref buildData, true);
            init.AddBatchInsertScripts(ref buildData, true);

            ScriptBatchCollection actual;
            IScriptBatcher scriptBatcher = new DefaultScriptBatcher();
            actual = scriptBatcher.LoadAndBatchSqlScripts(buildData, string.Empty);
            Assert.IsTrue(2 == actual.Count, "Invalid Batch Count " + actual.Count.ToString() + " vs 2");
            Assert.IsTrue(2 == actual[0].ScriptBatchContents.Length, "Invalid Batch Length " + actual.Count.ToString() + " vs 2");
        }

        /// <summary>
        ///A test for GetBlockingSqlLog
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void GetBlockingSqlLogTest()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            init.AddPreRunScript(ref buildData, false);
            Guid scriptId = new Guid(init.PreRunScriptGuid);
            BuildConnectData connData = new BuildConnectData();
            connData.Connection = SqlSync.Connection.ConnectionHelper.GetConnection(init.connData);
            connData.DatabaseName = init.connData.DatabaseName;
            connData.HasLoggingTable = true;
            connData.ServerName = init.serverName;
            connData.Transaction = null;

            BuildConnectData connDataExpected = connData;
            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null);
            bool actual;

            try
            {
                connData.Connection.Open();
                actual = dbUtil.GetBlockingSqlLog(scriptId, ref connData);
                Assert.AreEqual(connDataExpected, connData);
                Assert.AreEqual(true, actual);

                actual = dbUtil.GetBlockingSqlLog(Guid.NewGuid(), ref connData);
                Assert.AreEqual(connDataExpected, connData);
                Assert.AreEqual(false, actual);
            }
            finally
            {
                if (connData.Connection.State == ConnectionState.Open)
                    connData.Connection.Close();
            }

            //Will return false if an exception is thrown (like connection is closed)
            actual = dbUtil.GetBlockingSqlLog(scriptId, ref connData);
            Assert.AreEqual(connDataExpected, connData);
            Assert.AreEqual(false, actual);
        }

        #region HasBlockingSqlLogTest
        /// <summary>
        ///A test for HasBlockingSqlLog
        ///</summary>
        [TestMethod()]
        public void HasBlockingSqlLogTest()
        {
            Initialization init = GetInitializationObject();
            init.InsertPreRunScriptEntry();
            ConnectionData cData = init.connData;
            Guid scriptId = new Guid(init.PreRunScriptGuid);
            string databaseName = init.connData.DatabaseName;
            string scriptHash;
            string scriptHashExpected = "MadeUpHash";
            string scriptTextHash;
            string scriptTextHashExpected = "1E725D850DF0954D04A7C12F7C2B663A0A132EE6";
            DateTime commitDate; //Don't test, this will vary by test server.
            bool expected = true;
            bool actual;
            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null);
            actual = dbUtil.HasBlockingSqlLog(scriptId, cData, databaseName, out scriptHash, out scriptTextHash, out commitDate);
            Assert.AreEqual(scriptHashExpected, scriptHash);
            Assert.AreEqual(scriptTextHashExpected, scriptTextHash);
            Assert.AreEqual(expected, actual);



        }

        /// <summary>
        ///A test for HasBlockingSqlLog
        ///</summary>
        [TestMethod()]
        public void HasBlockingSqlLogTest_InvalidDbName()
        {
            Initialization init = GetInitializationObject();
            ConnectionData cData = init.connData;
            Guid scriptId = new Guid(init.PreRunScriptGuid);
            string databaseName = init.connData.DatabaseName;
            string scriptHash;
            string scriptTextHash;
            DateTime commitDate; //Don't test, this will vary by test server.
            bool actual;
            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter);
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, null);
            actual = dbUtil.HasBlockingSqlLog(scriptId, cData, "invalidDatabaseName", out scriptHash, out scriptTextHash, out commitDate);
            Assert.AreEqual("", scriptHash);
            Assert.AreEqual("", scriptTextHash);
            Assert.AreEqual(DateTime.MinValue, commitDate);
            Assert.AreEqual(false, actual);


        }
        #endregion

        /// <summary>
        ///A test for LogTableExists
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task LogTableExistsTest()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlConnection conn = ConnectionHelper.GetConnection(init.connData);
            IProgressReporter progressReporter = new NullProgressReporter();
            IConnectionsService connectionsService = new DefaultConnectionsService();
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter);
            bool actual = await sqlLoggingService.LogTableExists(conn);
            Assert.AreEqual(true, actual);

            //Invalidate the connection - should return false
            init.connData.DatabaseName = "invalidDBName";
            conn = ConnectionHelper.GetConnection(init.connData);
            actual = await sqlLoggingService.LogTableExists(conn);
            Assert.AreEqual(false, actual);

            //Change to master db - should not have the table
            init.connData.DatabaseName = "master";
            conn = ConnectionHelper.GetConnection(init.connData);
            actual = await sqlLoggingService.LogTableExists(conn);
            Assert.AreEqual(false, actual);
        }

        /// <summary>
        ///A test for ClearScriptBlocks
        ///</summary>
        [TestMethod()]
        [Ignore("The ClearScriptBlocks does not seem to be used anywhere!")]
        public void ClearScriptBlocksTest()
        {
            Initialization init = GetInitializationObject();
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
            ISqlLoggingService sqlLoggingService = new DefaultSqlLoggingService(connectionsService, progressReporter);
            ISqlBuildFileHelper fileHelper = new DefaultSqlBuildFileHelper();
            IDatabaseUtility dbUtil = new DefaultDatabaseUtility(connectionsService, sqlLoggingService, progressReporter, fileHelper);
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string[] scripts = new string[] { Guid.NewGuid().ToString(), buildData.Script[0].ScriptId };
            ClearScriptData scrData = new ClearScriptData(scripts, buildData, init.projectFileName, init.projectFileName);
            
            IProgressReporter reporter = new NullProgressReporter();
            dbUtil.ClearScriptBlocks(scrData, connData, reporter, null);
            Assert.AreEqual(false, buildData.CommittedScript[0].AllowScriptBlock);
        }

        /// <summary>
        ///A test for RecordCommittedScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void RecordCommittedScriptsTest()
        {
            Initialization init = GetInitializationObject();
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

        #region .: SaveBuildDataSet :.
        /// <summary>
        ///A test for SaveBuildDataSet
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        [ExpectedException(typeof(ArgumentException), "The \"projectFileName\" field value is null or empty. Unable to save the DataSet.")]
        public async Task SaveBuildDataSetTest_NullProjectFileName()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.projectFileName = null;
            await target.BuildFinalizer.SaveBuildDataModelAsync(target, false);
        }

        /// <summary>
        ///A test for SaveBuildDataSet
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        [ExpectedException(typeof(ArgumentException), "The \"projectFileName\" field value is null or empty. Unable to save the DataSet.")]
        public async Task SaveBuildDataSetTest_EmptyProjectFileName()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.projectFileName = string.Empty;
            await target.BuildFinalizer.SaveBuildDataModelAsync(target, false);
        }

        /// <summary>
        ///A test for SaveBuildDataSet - validates buildHistoryXmlFile check
        ///Note: projectFileName must be valid to reach the buildHistoryXmlFile validation
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        [ExpectedException(typeof(ArgumentException), "The \"buildHistoryXmlFile\" field value is null or empty. Unable to save the build history DataSet.")]
        public async Task SaveBuildDataSetTest_NullBuildHistoryName()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            // projectFileName must be valid to pass the first validation
            target.projectFileName = init.projectFileName;
            target.buildHistoryXmlFile = null;
            await target.BuildFinalizer.SaveBuildDataModelAsync(target, false);
        }
        /// <summary>
        ///A test for SaveBuildDataSet - validates buildHistoryXmlFile check
        ///Note: projectFileName must be valid to reach the buildHistoryXmlFile validation
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        [ExpectedException(typeof(ArgumentException), "The \"buildHistoryXmlFile\" field value is null or empty. Unable to save the build history DataSet.")]
        public async Task SaveBuildDataSetTest_EmptyBuildHistoryName()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            // projectFileName must be valid to pass the first validation
            target.projectFileName = init.projectFileName;
            target.buildHistoryXmlFile = string.Empty;
            await target.BuildFinalizer.SaveBuildDataModelAsync(target, false);
        }
        #endregion


        #region ReadBatchFromScriptTextTest - old methods
        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_SimplePass()
        {
            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;
            string[] scriptLines = new string[]{
                "This is line 1",
                "This is line 2",
                "  GO  ",
                "This is line 4",
                "This is line 5"};

            string[] actual;
            IScriptBatcher batcher = new DefaultScriptBatcher();
            string scriptContents = string.Join(Environment.NewLine, scriptLines);
            actual = batcher.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter).ToArray();
            Assert.AreEqual(2, actual.Length, "Expected 2 lines, got " + actual.Length.ToString());
            Assert.IsTrue(actual[0].IndexOf("GO") == -1, "Expected the \"GO\" delimiter to be absent");
            Assert.IsTrue(actual[0].IndexOf("2") > -1);
            Assert.IsTrue(actual[1].IndexOf("4") > -1);
        }
        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_KeepGoDelimiter()
        {
            bool stripTransaction = false;
            bool maintainBatchDelimiter = true;
            string[] scriptLines = new string[]{
                "This is line 1",
                "This is line 2",
                "  GO  ",
                "This is line 4",
                "This is line 5"};

            string[] actual;
            IScriptBatcher batcher = new DefaultScriptBatcher();
            string scriptContents = string.Join(Environment.NewLine, scriptLines);
            actual = batcher.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter).ToArray();
            Assert.IsTrue(actual[0].IndexOf("GO") > -1, "Expected the \"GO\" delimiter to be present");
        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_RemoveTransactions()
        {
            bool stripTransaction = true;
            bool maintainBatchDelimiter = false;
            string[] scriptLines = new string[]{
                "BEGIN TRANSACTION",
                "BEGIN TRAN",
                "ROLLBACK TRANSACTION",
                "ROLLBACK TRAN",
                "GO",
                "COMMIT TRANSACTION",
                "COMMIT TRAN",
                "COMMIT",
                "GO",
                "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE"
                };

            string[] actual;
            IScriptBatcher batcher = new DefaultScriptBatcher();
            string scriptContents = string.Join(Environment.NewLine, scriptLines);
            actual = batcher.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter).ToArray();
            Assert.AreEqual(2, actual.Length);
            Assert.IsTrue(actual[0].Trim().Length == 0);
            Assert.IsTrue(actual[1].Trim().Length == 0);

        }
        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_KeepTransactions()
        {
            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;
            string[] scriptLines = new string[]{
                "BEGIN TRANSACTION",
                "BEGIN TRAN",
                "ROLLBACK TRANSACTION",
                "ROLLBACK TRAN",
                "GO",
                "COMMIT TRANSACTION",
                "COMMIT TRAN",
                "COMMIT",
                "GO",
                "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE"
                };

            string[] actual;
            IScriptBatcher batcher = new DefaultScriptBatcher();
            string scriptContents = string.Join(Environment.NewLine, scriptLines);
            actual = batcher.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter).ToArray();
            Assert.AreEqual(3, actual.Length);
            Assert.IsTrue(actual[0].IndexOf("BEGIN TRANSACTION") > -1);
            Assert.IsTrue(actual[0].IndexOf("ROLLBACK TRANSACTION") > -1);
            Assert.IsTrue(actual[1].IndexOf("COMMIT TRANSACTION") > -1);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_StripTransactions()
        {
            string partial = @"These is one reference in comments
            {0}
and one in a big comment 
/* BEGIN TRANSACTION
*/
did it work
to remove the {0}
from the list?";
            string script = String.Format(partial, "BEGIN TRANSACTION");
            string expected = String.Format(partial, "");
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_SimpleBatchWithGoInScript()
        {
            string scriptContents = @"
SELECT * FROM MyTable
GO
SELECT * FROM YourTable
  GO
SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            IScriptBatcher batcher = new DefaultScriptBatcher();
            actual = batcher.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_SimpleBatchWithGoCommented()
        {
            string scriptContents = @"
SELECT * FROM MyTable
GO
SELECT * FROM YourTable
--This GO should still be here
  GO
SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            IScriptBatcher batcher = new DefaultScriptBatcher();
            actual = batcher.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[1].IndexOf("This GO should still be here") > 0);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_SimpleBatchWithGoCommentedInBulk()
        {
            string scriptContents = @"
SELECT * FROM MyTable
GO
SELECT * FROM YourTable
/* 
This GO should still be here since it's in a bulk comment section
GO
*/
  GO
SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            IScriptBatcher batcher = new DefaultScriptBatcher();
            actual = batcher.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[1].IndexOf("This GO should still be here") > 0);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_SimpleBatchRemoveUse()
        {
            string scriptContents = @"
        USE myNewDatabase
SELECT * FROM MyTable
GO
SELECT * FROM YourTable
  GO
USE theirNewDatabase            
SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            IScriptBatcher batcher = new DefaultScriptBatcher();
            actual = batcher.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[0].IndexOf("USE") == -1);
            Assert.IsTrue(actual[2].IndexOf("USE") == -1);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_SimpleBatchRemoveUseButNotCommented()
        {
            string scriptContents = @"
        USE myNewDatabase
SELECT * FROM MyTable
GO
SELECT * FROM YourTable
-- USE theirNewDatabase            
SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            IScriptBatcher batcher = new DefaultScriptBatcher();
            actual = batcher.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(2, actual.Count);
            Assert.IsTrue(actual[0].IndexOf("USE") == -1);
            Assert.IsTrue(actual[1].IndexOf("USE") > 0);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_SimpleBatchRemoveUseButNotBulkCommented()
        {
            string scriptContents = @"
        USE myNewDatabase
SELECT * FROM MyTable
GO
SELECT * FROM YourTable
  GO
/*

USE theirNewDatabase            

*/
SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            IScriptBatcher batcher = new DefaultScriptBatcher();
            actual = batcher.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[0].IndexOf("USE") == -1);
            Assert.IsTrue(actual[2].IndexOf("USE") > 0);

        }
        #endregion

        #region RemoveTransactionReferencesTest
        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_NothingFound()
        {
            string script = "This is my script that has no comm or rollback for transaction text";
            string expected = script;
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_BeginTransactionOnlyInComments()
        {
            string script = @"These references are in comments
            -- BEGIN TRANSACTION
and a big comment 
/* ROLLBACK TRANSACTION
*/
did it work?";
            string expected = script;
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_BeginTransactionInCommentsAndNot()
        {
            string partial = @"These is one reference in comments
            {0}
and one in a big comment 
/* BEGIN TRANSACTION
*/
did it work
to remove the {0}
from the list?";
            string script = String.Format(partial, "BEGIN TRANSACTION");
            string expected = String.Format(partial, "");
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_BeginTranOnlyInComments()
        {
            string script = @"These references are in comments
            -- BEGIN TRANS
and a big comment 
/* ROLLBACK TRANS
*/
did it work?";
            string expected = script;
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_BeginTranInCommentsAndNot()
        {
            string partial = @"These is one reference in comments
            {0}
and one in a big comment 
/* BEGIN TRAN
*/
did it work
to remove the {0}
from the list?";
            string script = String.Format(partial, "BEGIN TRAN");
            string expected = String.Format(partial, "");
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_RollbackTransactionOnlyInComments()
        {
            string script = @"These references are in comments
            -- ROLLBACK TRANSACTION
and a big comment 
/* ROLLBACK TRANSACTION
*/
did it work?";
            string expected = script;
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_RollbackTransactionInCommentsAndNot()
        {
            string partial = @"These is one reference in comments
            {0}
and one in a big comment 
/* ROLLBACK TRANSACTION
*/
did it work?
to remove the {0}
from the list?";
            string script = String.Format(partial, "Rollback Transaction");
            string expected = String.Format(partial, "");
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_RollbackTranOnlyInComments()
        {
            string script = @"These references are in comments
            -- ROLLBACK TRAN
and a big comment 
/* ROLLBACK TRAN
*/
did it work?";
            string expected = script;
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_RollbackTranInCommentsAndNot()
        {
            string partial = @"These is one reference in comments
            {0}
and one in a big comment 
/* ROLLBACK TRAN
*/
did it work
to remove the {0}
from the list?";
            string script = String.Format(partial, "Rollback Tran");
            string expected = String.Format(partial, "");
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_CommitTransactionOnlyInComments()
        {
            string script = @"These references are in comments
            -- COMMIT TRANSACTION
and a big comment 
/* Commit TRANSACTION
*/
did it work?";
            string expected = script;
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_CommitTransactionInCommentsAndNot()
        {
            string partial = @"These is one reference in comments
            {0}
and one in a big comment 
/* COMMIT TRANSACTION
*/
did it work
to remove the {0}
from the list?";
            string script = String.Format(partial, "COMMIT Transaction");
            string expected = String.Format(partial, "");
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_CommitTranOnlyInComments()
        {
            string script = @"These references are in comments
            -- COMMIT TRAN
and a big comment 
/* COMMIT TRAN
*/
did it work?";
            string expected = script;
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_CommitTranInCommentsAndNot()
        {
            string partial = @"These is one reference in comments
            {0}
and one in a big comment 
/* COMMIT TRAN
*/
did it work
to remove the {0}
from the list?";
            string script = String.Format(partial, "COMMIT Tran");
            string expected = String.Format(partial, "");
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_CommitOnlyInComments()
        {
            string script = @"These references are in comments
            -- COMMIT
and a big comment 
/* Commit
*/
did it work?";
            string expected = script;
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_TransLevelInCommentsAndNot()
        {
            string partial = @"These is one reference in comments
            {0}
and one in a big comment 
/* ROLLBACK TRANSACTION
*/
did it work
to remove the {0}
from the list?";
            string script = String.Format(partial, "ROLLBACK TRANSACTION");
            string expected = String.Format(partial, "");
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_TransLevelInComments()
        {
            string script = @"These references are in comments
            -- SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
and a big comment 
/* SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
*/
did it work?";
            string expected = script;
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }

        /// <summary>
        ///A test for RemoveTransactionReferences
        ///</summary>
        [TestMethod()]
        public void RemoveTransactionReferencesTest_ThrowEverythingAtIt()
        {
            string partial = @"These is one reference in comments
            {0}
{1}
            {2}
stuff here
{3}
more here
    {4}
and one in a big comment 
/* COMMIT
            BEGIN TRANSACTION
*/
did it work
to remove the {0}

{1}
 --COMMIT TRAN
    {2}
    {3}
/*
something here
            SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
ROLLBACK TRAN */
        {4}
from the list?";
            string script = String.Format(partial, "COMMIT", "BEGIN TRANSACTION", "ROLLBACK TRAN", "COMMIT TRANSACTION", "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE");
            string expected = String.Format(partial, "", "", "", "", "");
            string actual;
            actual = new DefaultScriptBatcher().RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }


        #endregion




    }
}
