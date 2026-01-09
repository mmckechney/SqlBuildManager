using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using SqlSync.SqlBuild.Models;
namespace SqlSync.SqlBuild.Dependent.UnitTest
{
    [TestClass]
    public class SqlBuildHelperTest_ProcessBuild
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
        /// <summary>
        ///A test for ProcessBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void ProcessBuildTest_CommitWithZeroRetries()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddScriptForProcessBuild(ref buildData, true, 20);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);
            BackgroundWorker bgWorker = init.GetBackgroundWorker();
            DoWorkEventArgs e = new DoWorkEventArgs(null);
            string serverName = init.serverName;
            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 0;

            string expected = BuildItemStatus.Committed;
            Build actual;
            actual = target.ProcessBuild(runData, bgWorker, e, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);
            Assert.AreEqual(expected, actual.FinalStatus);

        }

        /// <summary>
        ///A test for ProcessBuild
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void ProcessBuildTest_CommitWithRetriesNotUsed()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddScriptForProcessBuild(ref buildData, true, 20);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);
            BackgroundWorker bgWorker = init.GetBackgroundWorker();
            DoWorkEventArgs e = new DoWorkEventArgs(null);
            string serverName = init.serverName;
            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 3;

            string expected = BuildItemStatus.Committed;
            Build actual;
            actual = target.ProcessBuild(runData, bgWorker, e, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);
            Assert.AreEqual(expected, actual.FinalStatus);

        }

        [TestMethod()]
        [Ignore]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void ProcessBuildTest_RollbackWithThreeRetries()
        {
            Initialization init = GetInitializationObject();
            init.TableLockingLoopCount = 10000000;
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddScriptForProcessBuild(ref buildData, true, 1);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);

            BackgroundWorker bgWorker = init.GetBackgroundWorker();
            DoWorkEventArgs e = new DoWorkEventArgs(null);
            string serverName = init.serverName;
            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 3;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(new ParameterizedThreadStart(StartInfiniteLockingThread));
                THRInfinite.Start(init);

                string expected = BuildItemStatus.RolledBackAfterRetries;
                Build actual;
                actual = target.ProcessBuild(runData, bgWorker, e, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);
                Assert.AreEqual(expected, actual.FinalStatus);
            }
            finally
            {
                if (THRInfinite != null)
                    THRInfinite.Interrupt();
            }

        }

        [TestMethod()]
        [Ignore]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void ProcessBuildTest_RollbackWithZeroRetries()
        {
            Initialization init = GetInitializationObject();
            init.TableLockingLoopCount = 10000000;
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddScriptForProcessBuild(ref buildData, true, 2);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);

            BackgroundWorker bgWorker = init.GetBackgroundWorker();
            DoWorkEventArgs e = new DoWorkEventArgs(null);
            string serverName = init.serverName;
            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 0;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(new ParameterizedThreadStart(StartInfiniteLockingThread));
                THRInfinite.Start(init);

                string expected = BuildItemStatus.RolledBack;
                Build actual;
                actual = target.ProcessBuild(runData, bgWorker, e, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);
                Assert.AreEqual(expected, actual.FinalStatus);
            }
            finally
            {
                if (THRInfinite != null)
                    THRInfinite.Interrupt();
            }

        }

        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public void ProcessBuildTest_CommitAfterRetries()
        {
            Initialization init = GetInitializationObject();
            init.TableLockingLoopCount = 700000;
            SqlSyncBuildData buildData = init.CreateSqlSyncSqlBuildDataObject();
            init.AddScriptForProcessBuild(ref buildData, true, 2);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);

            BackgroundWorker bgWorker = init.GetBackgroundWorker();
            DoWorkEventArgs e = new DoWorkEventArgs(null);
            string serverName = init.serverName;
            bool isMultiDbRun = false;
            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 30;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(new ParameterizedThreadStart(StartInfiniteLockingThread));
                THRInfinite.Start(init);

                string expected = BuildItemStatus.CommittedWithTimeoutRetries;
                Build actual;
                actual = target.ProcessBuild(runData, bgWorker, e, serverName, isMultiDbRun, scriptBatchColl, allowableTimeoutRetries);
                Assert.AreEqual(expected, actual.FinalStatus);
            }
            finally
            {
                if (THRInfinite != null)
                    THRInfinite.Interrupt();
            }

        }

        private void StartInfiniteLockingThread(object initObj)
        {
            Initialization init = (Initialization)initObj;
            string connStr = string.Format(init.connectionString, init.testDatabaseNames[0]);
            SqlConnection conn = new SqlConnection(connStr);
            SqlCommand cmd = new SqlCommand(init.GetTableLockingScript(), conn);
            try
            {
                conn.Open();
                cmd.ExecuteNonQuery();
            }
            catch
            {
                // Swallow exceptions to avoid crashing test host; timeouts are expected during locking.
            }
        }

    }
}
