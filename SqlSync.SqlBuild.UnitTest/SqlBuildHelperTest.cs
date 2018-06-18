using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlSync.Connection;
using SqlSync.DbInformation;
using SqlSync.SqlBuild;
using System.Reflection;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Data.SqlClient;
using System.IO;
using SqlSync.SqlBuild.SqlLogging;

namespace SqlSync.SqlBuild.UnitTest
{
    
    
    /// <summary>
    ///This is a test class for SqlBuildHelperTest and is intended
    ///to contain all SqlBuildHelperTest Unit Tests
    ///</summary>
    [TestClass()]
    public class SqlBuildHelperTest
    {
        
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
        [ClassCleanup()]
        public static void Cleanup()
        {
            for (int i = 0; i < initColl.Count; i++)
            {
                initColl[i].Dispose();
            }
        }

        #region .: RunBuildScripts  - Transactional :.

        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void RunBuildScriptsTest_FinishCommitted()
        {
            Initialization init = GetInitializationObject();

            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            DataView view = buildData.Script.DefaultView;



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);
            
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
        public void RunBuildScriptsTest_FinishRollback()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddFailureScript(ref buildData, true, true);
            DataView view = buildData.Script.DefaultView;


            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual("RolledBack",actual.FinalStatus );

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
        public void RunBuildScriptsTest_MissingViewData()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            DataView view = null;


            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual("RolledBack",actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, String.Format("Invalid SqlBuild_Logging Count: {0}. {1}",sqlLoggingCount.ToString(), Initialization.MissingDatabaseErrorMessage));
            Assert.IsTrue(0 == testTableCount, String.Format("Invalid TransactionTest Count: {0}. {1}", testTableCount.ToString(), Initialization.MissingDatabaseErrorMessage));
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void RunBuildScriptsTest_UnableToConnectToDatabase()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddScriptWithBadDatabase(ref buildData);
            DataView view = buildData.Script.DefaultView;

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);

            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual("RolledBack",actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, String.Format("Invalid SqlBuild_Logging Count: {0}. {1}",sqlLoggingCount.ToString(), Initialization.MissingDatabaseErrorMessage));
            Assert.IsTrue(0 == testTableCount, String.Format("Invalid TransactionTest Count:  {0}. {1}",testTableCount.ToString(), Initialization.MissingDatabaseErrorMessage));
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void RunBuildScriptsTest_SingleRunScript()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, false);
            DataView view = buildData.Script.DefaultView;

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);

            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual(BuildItemStatus.Committed,actual.FinalStatus);


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
        public void RunBuildScriptsTest_SkippingPreRunScript()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddPreRunScript(ref buildData,false);
            DataView view = buildData.Script.DefaultView;

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);

            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);


            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByScriptId(0,buildData.Script[0].ScriptId);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(0 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());
        }
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void RunBuildScriptsTest_WithPendingCancellation()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            DataView view = buildData.Script.DefaultView;

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);

            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Set bgWorker to cancelled
            BackgroundWorker bg = sbh.bgWorker;
            bg.CancelAsync();
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual("RolledBack",actual.FinalStatus);


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
        public void RunBuildScriptsTest_TrialSuccessful()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            DataView view = buildData.Script.DefaultView;



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = true;
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual("TrialRolledBack", actual.FinalStatus);

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
        public void RunBuildScriptsTest_TrialWithFailure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddFailureScript(ref buildData, true, true);
            DataView view = buildData.Script.DefaultView;



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = true;
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual("RolledBack", actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(0 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void RunBuildScriptsTest_WithFailureDontCauseFailure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, false);
            DataView view = buildData.Script.DefaultView;



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = false;
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(1 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void RunBuildScriptsTest_WithFailureDontRollbackDontCauseFailure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, false, false);
            DataView view = buildData.Script.DefaultView;



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = false;
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

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
        public void RunBuildScriptsTest_WithPreBatchedScripts()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddBatchInsertScripts(ref buildData, true);
            DataView view = buildData.Script.DefaultView;

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = SqlBuildHelper.LoadAndBatchSqlScripts(buildData, init.projectFileName);
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);

            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

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
        public void RunBuildScriptsTest_AsScriptOnly()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            DataView view = buildData.Script.DefaultView;

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null;
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Set the script only flag
            sbh.runScriptOnly =  true;
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);
           
            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(0 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(0 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void RunBuildScriptsTest_SelectData()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddSelectScript(ref buildData);
            DataView view = buildData.Script.DefaultView;

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null;
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper(buildData);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(2 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());

        }

        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void RunBuildScriptsTest_AlternateLoggingDb()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddSelectScript(ref buildData);
            DataView view = buildData.Script.DefaultView;

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null;
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.LogToDatabaseName = init.testDatabaseNames[1];

            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = target.buildHistoryData.Build.NewBuildRow(); //init.GetRunBuildRow(po);
            //Execute the run...
            actual = target.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(1);
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(2 == sqlLoggingCount, "Invalid SqlBuild_Logging Count: " + sqlLoggingCount.ToString());
            Assert.IsTrue(1 == testTableCount, "Invalid TransactionTest Count: " + testTableCount.ToString());
            Assert.AreEqual(init.testDatabaseNames[1], target.LogToDatabaseName);

        }
        #endregion

        #region .: RunBuildScripts  - Non-Transactional :.
        /// <summary>
        ///A test for RunBuildScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SQLSync.SqlBuild.dll")]
        public void RunBuildScriptsTest_NonTransactional_FinishCommitted()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            DataView view = buildData.Script.DefaultView;



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper target = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(target);
            //Execute the run...
            actual = target.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual(BuildItemStatus.Committed,actual.FinalStatus);


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
        public void RunBuildScriptsTest_NonTransactional_FinishCommittedWithScripting()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            DataView view = buildData.Script.DefaultView;



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, true);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

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
        public void RunBuildScriptsTest_NonTransactional_Failure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, true);
            DataView view = buildData.Script.DefaultView;

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual("FailedNoTransaction",actual.FinalStatus );

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
        public void RunBuildScriptsTest_NonTransactional_WithFailureDontCauseFailure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, true, false);
            DataView view = buildData.Script.DefaultView;



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = false;
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

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
        public void RunBuildScriptsTest_NonTransactional_WithFailureDontRollbackDontCauseFailure()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, false, false);
            DataView view = buildData.Script.DefaultView;



            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            sbh.isTrialBuild = false;
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

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
        public void RunBuildScriptsTest_NonTransactional_Cancelled()
        {
            Initialization init = GetInitializationObject();
            //Create the build package...
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            DataView view = buildData.Script.DefaultView;

            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = null; //Want this null for this test
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            SqlSyncBuildData.BuildRow actual;

            //Get initialized SqlBuildHelper object...
            SqlBuildHelper sbh = init.CreateSqlBuildHelper_NonTransactional(buildData, false);

            //Get BuildRow...
            SqlSyncBuildData.BuildRow myBuild = init.GetRunBuildRow(sbh);
            //Set bgWorker to cancelled
            BackgroundWorker bg = sbh.bgWorker;
            bg.CancelAsync();
            //Execute the run...
            actual = sbh.RunBuildScripts(view, myBuild, init.serverName, isMultiDbRun, scriptBatchColl, ref workEventArgs);

            Assert.AreEqual("FailedNoTransaction", actual.FinalStatus);


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
            actual =helper.GetTargetDatabase(defaultDatabase);
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
            overrides.Add(new DatabaseOverride("MyDefault", "MyOverride"));
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
            overrides.Add(new DatabaseOverride("MyDefault", "MyOverride"));
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
            overrides.Add(new DatabaseOverride("MyDefault", "MyOverride"));
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
            overrides.Add(new DatabaseOverride("MyDefault", "MyOverride"));
            overrides.Add(new DatabaseOverride("MyDefault2", "MyOverride2"));
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
            overrides.Add(new DatabaseOverride("MyDefault", "MyOverride"));
            overrides.Add(new DatabaseOverride("MyDefault2", "MyOverride2"));
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
        public void PrepareBuildForRunTest_Success()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = false;
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            DoWorkEventArgs workEventArgsExpected = workEventArgs;
            DataView filteredScripts = null; // TODO: Initialize to an appropriate value
            SqlSyncBuildData.BuildRow myBuild = null; // TODO: Initialize to an appropriate value
            target.PrepareBuildForRun(serverName, isMultiDbRun, null, ref workEventArgs, out filteredScripts, out myBuild);
            Assert.AreEqual(workEventArgsExpected, workEventArgs);
            Assert.AreEqual(1, filteredScripts.Count);
            Assert.AreEqual("BuildOrder >=0", filteredScripts.RowFilter);
            Assert.AreEqual(System.Environment.UserName, myBuild.UserId);
            Assert.AreEqual(serverName, myBuild.ServerName);
            
        }

        /// <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void PrepareBuildForRunTest_Success2()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddSelectScript(ref buildData);
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = false;
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            DoWorkEventArgs workEventArgsExpected = workEventArgs;
            DataView filteredScripts = null; 
            SqlSyncBuildData.BuildRow myBuild = null; 
            target.PrepareBuildForRun(serverName, isMultiDbRun, null, ref workEventArgs, out filteredScripts, out myBuild);
            Assert.AreEqual(workEventArgsExpected, workEventArgs);
            Assert.AreEqual(1, filteredScripts.Count);
            Assert.AreEqual("BuildOrder >=0", filteredScripts.RowFilter);
            Assert.AreEqual(System.Environment.UserName, myBuild.UserId);
            Assert.AreEqual(serverName, myBuild.ServerName);
            

        }

        /// <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void PrepareBuildForRunTest_SuccessWithScriptBatch()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddSelectScript(ref buildData);
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);


            string serverName = init.serverName;
            bool isMultiDbRun = false;
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            DoWorkEventArgs workEventArgsExpected = workEventArgs;
            DataView filteredScripts = null;
            SqlSyncBuildData.BuildRow myBuild = null;

            ScriptBatchCollection coll = init.GetScriptBatchCollection();

            target.PrepareBuildForRun(serverName, isMultiDbRun, coll, ref workEventArgs, out filteredScripts, out myBuild);
            Assert.AreEqual(workEventArgsExpected, workEventArgs);
            Assert.AreEqual(1, filteredScripts.Count);
            Assert.AreEqual("BuildOrder >=0", filteredScripts.RowFilter);
            Assert.AreEqual(System.Environment.UserName, myBuild.UserId);
            Assert.AreEqual(serverName, myBuild.ServerName);
            Assert.AreEqual("DAA5499C760402A0F64DE5393BF82BB45E382075", target.buildPackageHash, "Invalid hash, did the methodology change?");

        }


        /// <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void PrepareBuildForRunTest_SuccessWithMultipleScripts()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = false;
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            DoWorkEventArgs workEventArgsExpected = workEventArgs;
            DataView filteredScripts = null;
            SqlSyncBuildData.BuildRow myBuild = null;
            target.runItemIndexes = new double[] { 0, 1 };
            target.PrepareBuildForRun(serverName, isMultiDbRun, null, ref workEventArgs, out filteredScripts, out myBuild);
            Assert.AreEqual(workEventArgsExpected, workEventArgs);
            Assert.AreEqual(3, filteredScripts.Count);
            Assert.AreEqual("BuildOrder IN (0,1)", filteredScripts.RowFilter);
            Assert.AreEqual(System.Environment.UserName, myBuild.UserId);
            Assert.AreEqual(serverName, myBuild.ServerName);

        }
        // <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void PrepareBuildForRunTest_WithNoScriptsAdded()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = false;
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            DoWorkEventArgs workEventArgsExpected = workEventArgs;
            DataView filteredScripts = null; // TODO: Initialize to an appropriate value
            SqlSyncBuildData.BuildRow myBuild = null; // TODO: Initialize to an appropriate value
            target.PrepareBuildForRun(serverName, isMultiDbRun, null, ref workEventArgs, out filteredScripts, out myBuild);
            Assert.AreEqual(workEventArgsExpected, workEventArgs);
            Assert.AreEqual(null, filteredScripts);
            Assert.AreEqual(System.Environment.UserName, myBuild.UserId);
            Assert.AreEqual(serverName, myBuild.ServerName);
            Assert.AreEqual("RolledBack", myBuild.FinalStatus);
        }
        // <summary>
        ///A test for PrepareBuildForRun
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void PrepareBuildForRunTest_WithNoScriptsAdded_MultiDbSet()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string serverName = init.serverName;
            bool isMultiDbRun = true;
            DoWorkEventArgs workEventArgs = new DoWorkEventArgs(null);
            DoWorkEventArgs workEventArgsExpected = workEventArgs;
            DataView filteredScripts = null; // TODO: Initialize to an appropriate value
            SqlSyncBuildData.BuildRow myBuild = null; // TODO: Initialize to an appropriate value
            target.PrepareBuildForRun(serverName, isMultiDbRun, null, ref workEventArgs, out filteredScripts, out myBuild);
            Assert.AreEqual(workEventArgsExpected, workEventArgs);
            Assert.AreEqual(null, filteredScripts);
            Assert.AreEqual(System.Environment.UserName, myBuild.UserId);
            Assert.AreEqual(serverName, myBuild.ServerName);
            Assert.AreEqual("PendingRollback", myBuild.FinalStatus);
        }
        #endregion

        #region .: GetTargetDatabase :.
        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        public void GetTargetDatabaseTest_MultiServer()
        {
            DatabaseOverride override1a = new DatabaseOverride("default1", "override1");
            DatabaseOverride override1b = new DatabaseOverride("default2", "override2");
            DatabaseOverride override2a = new DatabaseOverride("default3", "override3");
            DatabaseOverride override2b = new DatabaseOverride("default4", "override4");
            DatabaseOverride override3a = new DatabaseOverride("default5", "override5");
            DatabaseOverride override3b = new DatabaseOverride("default6", "override6");

            List<DatabaseOverride> lstOvr1 = new List<DatabaseOverride>();
            lstOvr1.Add(override1a);
            lstOvr1.Add(override1b);

            List<DatabaseOverride> lstOvr2 = new List<DatabaseOverride>();
            lstOvr2.Add(override2a);
            lstOvr2.Add(override2b);

            List<DatabaseOverride> lstOvr3 = new List<DatabaseOverride>();
            lstOvr3.Add(override3a);
            lstOvr3.Add(override3b);

            DbOverrideSequence overrideSeq1 = new DbOverrideSequence();
            overrideSeq1.Add("1", lstOvr1);
            overrideSeq1.Add("2", lstOvr2);

            DbOverrideSequence overrideSeq2 = new DbOverrideSequence();
            overrideSeq1.Add("3", lstOvr3);

            ServerData server1 = new ServerData();
            server1.ServerName = "Server1\\Instance1";
            server1.OverrideSequence.Add("1", lstOvr1);
            server1.OverrideSequence.Add("2", lstOvr2);

            ServerData server2 = new ServerData();
            server2.ServerName = "Server2\\Instance2";
            server2.OverrideSequence.Add("1", lstOvr3);

            MultiDbData multiDbRunData = new MultiDbData();
            multiDbRunData.Add(server1);
            multiDbRunData.Add(server2);



            string actual = SqlBuildHelper.GetTargetDatabase("Server2\\Instance2", "default5", multiDbRunData);
            Assert.AreEqual("override5", actual);

            actual = SqlBuildHelper.GetTargetDatabase("Server1\\Instance1", "default1", multiDbRunData);
            Assert.AreEqual("override1", actual);

            //Can't find server case...
            actual = SqlBuildHelper.GetTargetDatabase("ServerZZZZ", "defaultZZZZ", multiDbRunData);
            Assert.AreEqual("defaultZZZZ", actual);

            //Can't find override database case...
            actual = SqlBuildHelper.GetTargetDatabase("Server2\\Instance2", "defaultXX", multiDbRunData);
            Assert.AreEqual("defaultXX", actual);
        }

        /// <summary>
        ///A test for GetTargetDatabase
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void GetTargetDatabaseTest_SingleServer()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            DatabaseOverride override1 = new DatabaseOverride("default1", "override1");
            DatabaseOverride override2 = new DatabaseOverride("default2", "override2");
            DatabaseOverride override3 = new DatabaseOverride("default3", "override3");
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
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void GetTargetDatabaseTest_UseStaticOverride()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            DatabaseOverride override1 = new DatabaseOverride("default1", "override1");
            DatabaseOverride override2 = new DatabaseOverride("default2", "override2");
            DatabaseOverride override3 = new DatabaseOverride("default3", "override3");
            List<DatabaseOverride> ovr = new List<DatabaseOverride>();
            ovr.Add(override1);
            ovr.Add(override2);
            ovr.Add(override3);
            OverrideData.TargetDatabaseOverrides = ovr;

            Assert.IsNull(target.targetDatabaseOverrides);
            Assert.IsNull(target.TargetDatabaseOverrides);

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
            Initialization init = this.GetInitializationObject();
            init.InsertPreRunScriptEntry();

            Guid scriptId = new Guid(init.PreRunScriptGuid);
            ConnectionData connData = init.connData;
            ScriptRunLog actual;
            actual = SqlBuildHelper.GetScriptRunLog(scriptId, connData);
            Assert.IsTrue(actual.Rows.Count > 0, String.Format("Missing rows for pre-run script. {0}", Initialization.MissingDatabaseErrorMessage));

            actual = SqlBuildHelper.GetScriptRunLog(Guid.NewGuid(), connData);
            Assert.IsTrue(actual.Rows.Count == 0, String.Format("Rows found for new unique script id. {0}", Initialization.MissingDatabaseErrorMessage));


        }

        /// <summary>
        ///A test for GetScriptRunLog
        ///</summary>
        [TestMethod()]
        [ExpectedException(typeof(ApplicationException))]
        public void GetScriptRunLogTest_ExpectException()
        {
            Initialization init = this.GetInitializationObject();

            Guid scriptId = new Guid(init.PreRunScriptGuid);
            ConnectionData connData = init.connData;
            connData.DatabaseName = "invalidDatabaseName";
            ScriptRunLog actual;
            actual = SqlBuildHelper.GetScriptRunLog(scriptId, connData);
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
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            //Reset the scriptLogFileName to NULL to get exception
            target.scriptLogFileName = null;
            object sender = null;
            ScriptLogEventArgs e = new ScriptLogEventArgs(10, "SELECT TestCol FROM [test].[TestTable] WHERE TestCol IS NOT NULL", init.testDatabaseNames[0], "C:\test.sql", "Test Executed");
            target.SqlBuildHelper_ScriptLogWriteEvent(sender, e);
        }
        /// <summary>
        ///A test for SqlBuildHelper_ScriptLogWriteEvent
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void SqlBuildHelper_ScriptLogWriteEventTest_CreateScriptLogFile()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            //Delete the file made by the init the scriptLogFileName to NULL to get exception
            if (System.IO.File.Exists(target.scriptLogFileName))
                System.IO.File.Delete(target.scriptLogFileName);

            object sender = null;
            ScriptLogEventArgs e = new ScriptLogEventArgs(10, "SELECT TestCol FROM [test].[TestTable] WHERE TestCol IS NOT NULL", init.testDatabaseNames[0], @"C:\test.sql", "Test Executed");
            target.SqlBuildHelper_ScriptLogWriteEvent(sender, e);

            string contents = File.ReadAllText(target.scriptLogFileName);
            Assert.IsTrue(contents.IndexOf("SELECT TestCol FROM [test].[TestTable]") > -1);
            Assert.IsTrue(contents.IndexOf(@"C:\test.sql") > -1);

            //Create the "end" entry for an external log file...
            string fileName = init.GetTrulyUniqueFile();
            target.externalScriptLogFileName = fileName;
            if (File.Exists(fileName))
                File.Delete(fileName);

            e = new ScriptLogEventArgs(-10000, "SELECT TestCol FROM [test].[TestTable] WHERE TestCol IS NOT NULL", init.testDatabaseNames[0], @"C:\test.sql", "Test Executed");
            target.SqlBuildHelper_ScriptLogWriteEvent(sender, e);
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
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
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
            target.SqlBuildHelper_ScriptLogWriteEvent(sender, e);

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
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.isTransactional = false;

            //Delete the file made by the init the scriptLogFileName to NULL to get exception
            if (System.IO.File.Exists(target.scriptLogFileName))
                System.IO.File.Delete(target.scriptLogFileName);

            object sender = null;
            ScriptLogEventArgs e = new ScriptLogEventArgs(10, "SELECT TestCol FROM [test].[TestTable] WHERE TestCol IS NOT NULL", init.testDatabaseNames[0], @"C:\test.sql", "Test Executed");
            target.SqlBuildHelper_ScriptLogWriteEvent(sender, e);

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
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.isTransactional = false;

            bool actual = target.CommitBuild();
            Assert.AreEqual(true, actual);

        }

        /// <summary>
        ///A test for CommitBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void CommitBuildTest_TransactionFailure()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            BuildConnectData connData = target.GetConnectionDataClass(init.serverName, init.testDatabaseNames[0]);
            connData.Transaction.Rollback(); //Rollback transaction to make it unusable

            bool actual = target.CommitBuild();
            Assert.AreEqual(false, actual);

        }
        // <summary>
        ///A test for CommitBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void CommitBuildTest_ClosedConnectionFailure()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            BuildConnectData connData = target.GetConnectionDataClass(init.serverName, init.testDatabaseNames[0]);
            connData.Transaction.Rollback(); //Rollback transaction to make it unusable
            connData.Connection.Close();
            connData.Connection = null; //Set connection to null to create exception when trying to close.

            bool actual = target.CommitBuild();
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
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.isTransactional = false;

            bool actual = target.RollbackBuild();
            Assert.AreEqual(false, actual);
        }
        /// <summary>
        ///A test for RollbackBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void RollbackBuildTest_TransactionFailure()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            BuildConnectData connData = target.GetConnectionDataClass(init.serverName, init.testDatabaseNames[0]);
            connData.Transaction.Rollback(); //Rollback transaction to make it unusable

            bool actual = target.RollbackBuild();
            Assert.AreEqual(true, actual);

        }
        /// <summary>
        ///A test for RollbackBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void RollbackBuildTest_ClosedConnectionFailure()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            BuildConnectData connData = target.GetConnectionDataClass(init.serverName, init.testDatabaseNames[0]);
            connData.Transaction.Rollback(); //Rollback transaction to make it unusable
            connData.Connection.Close();
            connData.Connection = null; //Set connection to null to create exception when trying to close.

            bool actual = target.RollbackBuild();
            Assert.AreEqual(true, actual);

        }
        #endregion

        #region PerformScriptTokenReplacement
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
            helper.buildFileName =  null;

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
            helper.buildFileName ="TestBuildFile.sbm";

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
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);
            SqlSyncBuildData.ScriptDataTable actual = null;
            actual = SqlBuildHelper.GetScriptSourceTable(buildData);

            Assert.IsInstanceOfType(actual,typeof(SqlSyncBuildData.ScriptDataTable));
            Assert.IsTrue(1 == actual.Rows.Count);
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
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddBatchInsertScripts(ref buildData, true);
            init.AddBatchInsertScripts(ref buildData, true);

            ScriptBatchCollection actual;
            actual = SqlBuildHelper.LoadAndBatchSqlScripts(buildData, init.projectFileName);
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
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
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
            bool actual;

            try
            {
                connData.Connection.Open();
                actual = target.GetBlockingSqlLog(scriptId, ref connData);
                Assert.AreEqual(connDataExpected, connData);
                Assert.AreEqual(true, actual);

                actual = target.GetBlockingSqlLog(Guid.NewGuid(), ref connData);
                Assert.AreEqual(connDataExpected, connData);
                Assert.AreEqual(false, actual);
            }
            finally
            {
                if (connData.Connection.State == ConnectionState.Open) 
                    connData.Connection.Close();
            }

            //Will return false if an exception is thrown (like connection is closed)
            actual = target.GetBlockingSqlLog(scriptId, ref connData);
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
            Initialization init = this.GetInitializationObject();
            init.InsertPreRunScriptEntry();
            ConnectionData cData = init.connData;
            Guid scriptId = new Guid(init.PreRunScriptGuid);
            string databaseName = init.connData.DatabaseName;
            string scriptHash;
            string scriptHashExpected = "MadeUpHash";
            string scriptTextHash;
            string scriptTextHashExpected = "0820B32B206B7352858E8903A838ED14319ACDFD";
            DateTime commitDate; //Don't test, this will vary by test server.
            bool expected = true; 
            bool actual;
            actual = SqlBuildHelper.HasBlockingSqlLog(scriptId, cData, databaseName, out scriptHash, out scriptTextHash, out commitDate);
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
            Initialization init = this.GetInitializationObject();
            ConnectionData cData = init.connData;
            Guid scriptId = new Guid(init.PreRunScriptGuid);
            string databaseName = init.connData.DatabaseName;
            string scriptHash;
            string scriptTextHash;
            DateTime commitDate; //Don't test, this will vary by test server.
            bool actual;
         
            actual = SqlBuildHelper.HasBlockingSqlLog(scriptId, cData, "invalidDatabaseName", out scriptHash, out scriptTextHash, out commitDate);
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
        public void LogTableExistsTest()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            SqlConnection conn = ConnectionHelper.GetConnection(init.connData);
            bool actual = target.LogTableExists(conn);
            Assert.AreEqual(true, actual);

            //Invalidate the connection - should return false
            init.connData.DatabaseName = "invalidDBName";
            conn = ConnectionHelper.GetConnection(init.connData);
            actual = target.LogTableExists(conn);
            Assert.AreEqual(false, actual);

            //Change to master db - should not have the table
            init.connData.DatabaseName = "master";
            conn = ConnectionHelper.GetConnection(init.connData);
            actual = target.LogTableExists(conn);
            Assert.AreEqual(false, actual);
        }

        /// <summary>
        ///A test for ClearScriptBlocks
        ///</summary>
        [TestMethod()]
        public void ClearScriptBlocksTest()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddInsertScript(ref buildData, true);

            SqlSyncBuildData.CommittedScriptRow row = buildData.CommittedScript.NewCommittedScriptRow();
            row.ScriptId = buildData.Script[0].ScriptId;
            row.ServerName = init.serverName;
            row.AllowScriptBlock = true;
            buildData.CommittedScript.Rows.Add(row);

            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);

            string[] scripts = new string[]{Guid.NewGuid().ToString(),buildData.Script[0].ScriptId};
            ClearScriptData scrData = new ClearScriptData(scripts, buildData, init.projectFileName, init.projectFileName);
            BackgroundWorker bgWorker = new BackgroundWorker();
            bgWorker.WorkerReportsProgress = true;
            DoWorkEventArgs e = new DoWorkEventArgs(null);
            target.ClearScriptBlocks(scrData, bgWorker, e);
            Assert.AreEqual(false, buildData.CommittedScript[0].AllowScriptBlock);
        }

        /// <summary>
        ///A test for RecordCommittedScripts
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void RecordCommittedScriptsTest()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
          
            Guid scriptID = Guid.NewGuid();
            string hash = "WETRWEW@#$@$WEQW#$#R";
            CommittedScript script = new CommittedScript(scriptID, hash, 20, "My script text", "TAGID", init.serverName, init.testDatabaseNames[0]);
            List<CommittedScript> committedScripts = new List<CommittedScript>();
            committedScripts.Add(script);

            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            bool actual;
            actual = target.RecordCommittedScripts(committedScripts);
            Assert.AreEqual(true, actual);

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
        [ExpectedException(typeof(ArgumentException),"The \"projectFileName\" field value is null or empty. Unable to save the DataSet.")]
        public void SaveBuildDataSetTest_NullProjectFileName()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.projectFileName = null;
            target.SaveBuildDataSet(false);
        }

        /// <summary>
        ///A test for SaveBuildDataSet
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        [ExpectedException(typeof(ArgumentException),"The \"projectFileName\" field value is null or empty. Unable to save the DataSet.")]
        public void SaveBuildDataSetTest_EmptyProjectFileName()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.projectFileName = string.Empty;
            target.SaveBuildDataSet(false);
        }

        /// <summary>
        ///A test for SaveBuildDataSet
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        [ExpectedException(typeof(ArgumentException), "The \"buildHistoryXmlFile\" field value is null or empty. Unable to save the build history DataSet.")]
        public void SaveBuildDataSetTest_NullBuildHistoryName()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.buildHistoryXmlFile = null;
            target.SaveBuildDataSet(false);
        }
        /// <summary>
        ///A test for SaveBuildDataSet
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        [ExpectedException(typeof(ArgumentException), "The \"buildHistoryXmlFile\" field value is null or empty. Unable to save the build history DataSet.")]
        public void SaveBuildDataSetTest_EmptyBuildHistoryName()
        {
            Initialization init = this.GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            SqlBuildHelper target = init.CreateSqlBuildHelperAccessor(buildData);
            target.buildHistoryXmlFile = string.Empty;
            target.SaveBuildDataSet(false);
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
            actual = SqlBuildHelper.ReadBatchFromScriptText(stripTransaction, maintainBatchDelimiter, scriptLines);
            Assert.AreEqual(2, actual.Length,"Expected 2 lines, got "+actual.Length.ToString());
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
            actual = SqlBuildHelper.ReadBatchFromScriptText(stripTransaction, maintainBatchDelimiter, scriptLines);
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
            actual = SqlBuildHelper.ReadBatchFromScriptText(stripTransaction, maintainBatchDelimiter, scriptLines);
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
            actual = SqlBuildHelper.ReadBatchFromScriptText(stripTransaction, maintainBatchDelimiter, scriptLines);
            Assert.AreEqual(3, actual.Length);
            Assert.IsTrue(actual[0].IndexOf("ROLLBACK") > -1);
            Assert.IsTrue(actual[1].IndexOf("COMMIT") > -1);
            Assert.IsTrue(actual[2].IndexOf("SET TRANSACTION ISOLATION LEVEL SERIALIZABLE") > -1);

        }
        #endregion
        
        #region ReadBatchFromScriptTextTest - new methods

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_NoBatch()
        {
            string scriptContents = @"
SELECT * FROM MyTable

"
;

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(1, actual.Count);
            //Expect that the last \r\n is trimmed from the script
            Assert.AreEqual(scriptContents.Substring(0,scriptContents.Length-2), actual[0]);

        }
        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_SimpleBatch()
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
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_StripTransactions()
        {
            string template = @"
{0}
SELECT * FROM MyTable
{1}         
GO
SELECT * FROM YourTable
  GO
SELECT * FROM TheirTable";

            string scriptContents = String.Format(template, "BEGIN TRAN", "COMMIT TRAN");

            bool stripTransaction = true;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[0].IndexOf("TRAN") == -1);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_LeaveTransactions()
        {
            string template = @"
{0}
SELECT * FROM MyTable
{1}         
GO
SELECT * FROM YourTable
  GO
SELECT * FROM TheirTable";

            string scriptContents = String.Format(template, "BEGIN TRAN", "COMMIT TRAN");


            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[0].IndexOf("BEGIN TRAN") > 0);
            Assert.IsTrue(actual[0].IndexOf("COMMIT TRAN") > 0);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_StripTransactionsSomeInLineComments()
        {
            string template = @"
{0}
SELECT * FROM MyTable
{1}         
GO
-- {0}
SELECT * FROM YourTable
-- {1}
  GO
SELECT * FROM TheirTable";

            string scriptContents = String.Format(template, "BEGIN TRAN", "COMMIT TRAN");

            bool stripTransaction = true;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[0].IndexOf("TRAN") == -1);
            Assert.IsTrue(actual[1].IndexOf("TRAN") > 0);

        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_StripTransactionsSomeInBulkComments()
        {
            string template = @"
{0}
SELECT * FROM MyTable
{1}         
GO
/*  {0} */
SELECT * FROM YourTable
/*

{1}

*/
  GO
SELECT * FROM TheirTable";

            string scriptContents = String.Format(template, "BEGIN TRAN", "COMMIT TRAN");

            bool stripTransaction = true;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[0].IndexOf("TRAN") == -1);
            Assert.IsTrue(actual[1].IndexOf("TRAN") > 0);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_LeaveTransactionsSomeInLineComments()
        {
            string template = @"
{0}
SELECT * FROM MyTable
{1}         
GO
-- {0}
SELECT * FROM YourTable
-- {1}
  GO
SELECT * FROM TheirTable";

            string scriptContents = String.Format(template, "BEGIN TRAN", "COMMIT TRAN");


            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[0].IndexOf("BEGIN TRAN") > 0);
            Assert.IsTrue(actual[0].IndexOf("COMMIT TRAN") > 0);
            Assert.IsTrue(actual[1].IndexOf("BEGIN TRAN") > 0);
            Assert.IsTrue(actual[1].IndexOf("COMMIT TRAN") > 0);

        }

        /// <summary>
        ///A test for ReadBatchFromScriptText
        ///</summary>
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_SimpleBatchWithGoInScript()
        {
            string scriptContents = @"
SELECT Go, Now, Please FROM MyTable
GO
SELECT Not, GO here FROM YourTable
  GO
SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
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
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
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
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
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
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
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
  GO
-- USE theirNewDatabase            
SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual;
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[0].IndexOf("USE") == -1);
            Assert.IsTrue(actual[2].IndexOf("USE") > 0);

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
            actual = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);
            Assert.AreEqual(3, actual.Count);
            Assert.IsTrue(actual[0].IndexOf("USE") == -1);
            Assert.IsTrue(actual[2].IndexOf("USE") > 0);

        }
        #endregion

        #region Compare Old and New ReadBatchFromScriptText
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_NoDelimiterNoTrailingCR()
        {
            string scriptContents = @"
SELECT * FROM MyTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = true;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);

        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_NoDelimiterOneTrailingCR()
        {
            string scriptContents = @"
        SELECT * FROM MyTable
        ";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = true;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);

        }
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_NoDelimiterTwoTrailingCR()
        {
            string scriptContents = @"
        SELECT * FROM MyTable
        
        ";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = true;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);

        }
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_NoDelimiterThreeTrailingCR()
        {
            string scriptContents = @"
        SELECT * FROM MyTable
        
        
        ";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = true;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);

        }
 
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithMaintainDelimiter()
        {
            string scriptContents = @"
SELECT * FROM MyTable
GO
SELECT * FROM YourTable
  GO
SELECT * FROM TheirTable
  GO    
SELECT * from WhoseTable
  GO ";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = true;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);
            Assert.AreEqual(actual2[3], actual1[3]);
        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithDontMaintainDelimiter()
        {
            string scriptContents = @"
SELECT * FROM MyTable   
GO
SELECT * FROM YourTable 
  GO
SELECT * FROM TheirTable 
  GO    
SELECT * from WhoseTable
Go   ";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);
            Assert.AreEqual(actual2[3], actual1[3]);
        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_VaryingEndGo1()
        {
            string scriptContents = @"
SELECT * FROM MyTable   
GO
SELECT * FROM YourTable 
  GO
SELECT * FROM TheirTable 
  GO    
SELECT * from WhoseTable
Go   ";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);
            Assert.AreEqual(actual2[3], actual1[3]);
        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_VaryingEndGo2()
        {
            string scriptContents = @"
SELECT * FROM MyTable   
GO
SELECT * FROM YourTable 
  GO
SELECT * FROM TheirTable 
  GO    
SELECT * from WhoseTable


Go   ";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);
            Assert.AreEqual(actual2[3], actual1[3]);
        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_VaryingEndGo3()
        {
            string scriptContents = @"
SELECT * FROM MyTable   
GO
SELECT * FROM YourTable 
  GO
SELECT * FROM TheirTable 
  GO    
SELECT * from WhoseTable
    Go   ";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);
            Assert.AreEqual(actual2[3], actual1[3]);
        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_VaryingEndGo4()
        {
            string scriptContents = @"
SELECT * FROM MyTable   
GO
SELECT * FROM YourTable 
  GO
SELECT * FROM TheirTable 
  GO    
SELECT * from WhoseTable
      ";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);
            Assert.AreEqual(actual2[3], actual1[3]);
        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_VaryingEndGo5()
        {
            string scriptContents = @"
SELECT * FROM MyTable   
GO
SELECT * FROM YourTable 
  GO
SELECT * FROM TheirTable 
  GO    
SELECT * from WhoseTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);
            Assert.AreEqual(actual2[3], actual1[3]);
        }


        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithDelimiterInLineComment()
        {
            string scriptContents = @"
SELECT * FROM MyTable   
GO
    SELECT * FROM MyTable   
    GO      
SELECT * FROM YourTable
--This GO should still be here
  GO
SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            //We expect a go on the same line as a "--" comment will match
            Assert.IsTrue(actual1.Count == 4);
            Assert.IsTrue(actual2.Length == 4);

            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);
            Assert.AreEqual(actual2[3], actual1[3]);

        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithDelimiterInBulkComment()
        {
            string scriptContents = @"
SELECT * FROM MyTable
GO
SELECT * FROM YourTable
/* 
This should still be here since it's in a bulk comment section
GO
*/
  GO
SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);
            
            //We expect a GO in it's own line of a bulk comment will result in differences
            Assert.IsTrue(actual1.Count == 3);
            Assert.IsTrue(actual2.Length == 4);

            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreNotEqual(actual2[1], actual1[1]);
            Assert.IsTrue(actual1[2].StartsWith("SELECT * FROM TheirTable"));
            Assert.AreEqual(actual2[3], actual1[2]);

        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithRemoveTransactions()
        {
            string template = @"
{0}
SELECT * FROM MyTable
{1}         
GO
SELECT * FROM YourTable
  GO
SELECT * FROM TheirTable";
            string scriptContents = String.Format(template, "BEGIN TRAN", "COMMIT TRAN");

            bool stripTransaction = true;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);

            Assert.IsTrue(actual1.Count == 3);
            Assert.IsTrue(actual2.Length == 3);

            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);

        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithRemoveTransactionsCommented()
        {
            string template = @"
{0}
SELECT * FROM MyTable
{1}         
GO
-- {0}
SELECT * FROM YourTable
-- {1}
  GO
SELECT * FROM TheirTable";
            string scriptContents = String.Format(template, "BEGIN TRAN", "COMMIT TRAN");

            bool stripTransaction = true;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);

            Assert.IsTrue(actual1.Count == 3);
            Assert.IsTrue(actual2.Length == 3);

            Assert.AreEqual(actual2[0], actual1[0]);
            //We expect that the "old" method did a non-qualified replace, while the new honors comments
            Assert.AreNotEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);

            Assert.IsTrue(actual1[1].IndexOf("TRAN") > 0);
            Assert.IsTrue(actual2[1].IndexOf("TRAN") == -1);

        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithRemoveTransactionsCommentedInBulk()
        {
            string template = @"
{0}
SELECT * FROM MyTable
{1}         
GO
/* {0}  */
SELECT * FROM YourTable
/*

{1}

*/
  GO
SELECT * FROM TheirTable";
            string scriptContents = String.Format(template, "BEGIN TRAN", "COMMIT TRAN");

            bool stripTransaction = true;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);

            Assert.IsTrue(actual1.Count == 3);
            Assert.IsTrue(actual2.Length == 3);

            Assert.AreEqual(actual2[0], actual1[0]);
            //We expect that the "old" method did a non-qualified replace, while the new honors comments
            Assert.AreNotEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);

            Assert.IsTrue(actual1[1].IndexOf("TRAN") > 0);
            Assert.IsTrue(actual2[1].IndexOf("TRAN") == -1);

        }


        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithLeaveTransactions()
        {
            string template = @"
{0}
SELECT * FROM MyTable
{1}         
GO
SELECT * FROM YourTable
  GO
SELECT * FROM TheirTable";
            string scriptContents = String.Format(template, "BEGIN TRAN", "COMMIT TRAN");

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);

            Assert.IsTrue(actual1.Count == 3);
            Assert.IsTrue(actual2.Length == 3);

            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);

        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithRemoveUseStatements()
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

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);

            Assert.IsTrue(actual1.Count == 3);
            Assert.IsTrue(actual2.Length == 3);

            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);

        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithRemoveUseStatementsLineComment()
        {
            string scriptContents = @"
        USE myNewDatabase
SELECT * FROM MyTable
GO
SELECT * FROM YourTable
  GO
--    USE theirNewDatabase            

SELECT * FROM TheirTable";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);

            Assert.IsTrue(actual1.Count == 3);
            Assert.IsTrue(actual2.Length == 3);

            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
            Assert.AreEqual(actual2[2], actual1[2]);

        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithRemoveUseStatementsBulkComment()
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

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);

            Assert.IsTrue(actual1.Count == 3);
            Assert.IsTrue(actual2.Length == 3);

            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);

            //Expect a difference between new and old
            Assert.AreNotEqual(actual2[2], actual1[2]);

            Assert.IsTrue(actual1[2].IndexOf("USE") > -1);
            Assert.IsTrue(actual2[2].IndexOf("USE") == -1);

        }

        [TestMethod()]
        public void ReadBatchFromScriptTextTest_CompareWithLeadingGo()
        {
            string scriptContents = @"
GO


UPDATE SView 
SET FromClause = ' vw_MyCoolView vcbdd ON vco.CompanyNumber= vcbdd.CompanyNumber AND vco.PR_CompanyNumber= vcbdd.P_Company_Id'
WHERE View_Id = 12434

UPDATE SView
SET FromClause = ' vw_rpt_Direct_Deposits_Company vdd ON vco.CompanyNumber= vdd.CompanyNumber and vco.companycode = vdd.p_company_id'
WHERE View_Id = 405634
GO

";

            bool stripTransaction = false;
            bool maintainBatchDelimiter = false;

            List<string> actual1;
            actual1 = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, stripTransaction, maintainBatchDelimiter);

            string[] actual2;
            actual2 = SqlBuildHelper.ReadBatchFromScript(stripTransaction, maintainBatchDelimiter, scriptContents);

            Assert.IsTrue(actual1.Count == 2);
            Assert.IsTrue(actual2.Length == 2);

            Assert.AreEqual(actual2[0], actual1[0]);
            Assert.AreEqual(actual2[1], actual1[1]);
         
        }

        
        [TestMethod()]
        public void ReadBatchFromScriptTextTest_ComparePackageFilesRemoveDelimiter()
        {
            string[] paths = new string[] { 
                @"C:\SqlBuildManager_Tests\", 
                @"C:\SqlBuildManager_Tests1\",
                @"C:\SqlBuildManager_Tests2\",
                @"C:\SqlBuildManager_Tests3\",
                @"C:\SqlBuildManager_Tests4\",
                @"C:\SqlBuildManager_Tests5\"};

            bool assertFail = false;
            string hashFromOld;
            string hashFromNew;
            foreach (string path in paths)
            {
                if (!Directory.Exists(path))
                    Assert.Inconclusive("Test path not available");

                string[] files = Directory.GetFiles(path);
                Console.WriteLine(String.Format("Comparing {0} files", files.Length));
                
                foreach (string file in files)
                {

                    string[] scriptLines = File.ReadAllLines(file);
                    string[] batch = SqlBuildHelper.ReadBatchFromScriptText(false, false, scriptLines);

                    string scriptContents = File.ReadAllText(file);
                    string[] batchNew = SqlBuildHelper.ReadBatchFromScriptText(scriptContents, false, false).ToArray();

                    Assert.AreEqual(batch.Length, batchNew.Length, String.Format("Batch lengths are different for {0}. Old={1}; New={2}", file, batch.Length.ToString(), batchNew.Length.ToString()));
                    for (int i = 0; i < batch.Length; i++)
                    {
                        if (batch[i] != batchNew[i])
                        {
                            Console.WriteLine(String.Format("Batch difference found in file {0}, batch index {1}", file, i.ToString()));
                            assertFail = true;
                        }

                    }
                    SqlBuildFileHelper.GetSHA1Hash(batch, out hashFromOld);
                    SqlBuildFileHelper.GetSHA1Hash(batchNew, out hashFromNew);
                    Assert.AreEqual(hashFromOld, hashFromNew);

                }
            }
            Assert.IsFalse(assertFail, "One or more failures. See Console output");

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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
/* SET TRANSACTION ISOLATION LEVEL SERIALIZABLE
*/
did it work
to remove the {0}
from the list?";
            string script = String.Format(partial, "SET TRANSACTION ISOLATION LEVEL SERIALIZABLE");
            string expected = String.Format(partial, "");
            string actual;
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
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
            actual = SqlBuildHelper.RemoveTransactionReferences(script);
            Assert.AreEqual(expected, actual);
        }


        #endregion



        
    }
}
