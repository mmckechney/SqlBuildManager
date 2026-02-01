using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;
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
        ///A test for ProcessBuildAsync
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task ProcessBuildTest_CommitWithZeroRetries()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 20);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);
            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 0;

            BuildItemStatus expected = BuildItemStatus.Committed;
            Build actual;
            actual = await target.ProcessBuildAsync(runData, allowableTimeoutRetries, string.Empty, scriptBatchColl);
            Assert.AreEqual(expected, actual.FinalStatus);

        }

        /// <summary>
        ///A test for ProcessBuildAsync
        ///</summary>
        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task ProcessBuildTest_CommitWithRetriesNotUsed()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 20);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);
            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 3;

            BuildItemStatus expected = BuildItemStatus.Committed;
            Build actual;
            actual = await target.ProcessBuildAsync(runData, allowableTimeoutRetries, string.Empty, scriptBatchColl);
            Assert.AreEqual(expected, actual.FinalStatus);

        }

        [TestMethod()]
        [Ignore]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task ProcessBuildTest_RollbackWithThreeRetries()
        {
            Initialization init = GetInitializationObject();
            init.TableLockingLoopCount = 10000000;
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 1);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);

            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 3;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(new ParameterizedThreadStart(StartInfiniteLockingThread));
                THRInfinite.Start(init);

                BuildItemStatus expected = BuildItemStatus.RolledBackAfterRetries;
                Build actual;
                actual = await target.ProcessBuildAsync(runData, allowableTimeoutRetries, string.Empty, scriptBatchColl);
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
        public async Task ProcessBuildTest_RollbackWithZeroRetries()
        {
            Initialization init = GetInitializationObject();
            init.TableLockingLoopCount = 10000000;
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 2);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);

            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 0;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(new ParameterizedThreadStart(StartInfiniteLockingThread));
                THRInfinite.Start(init);

                BuildItemStatus expected = BuildItemStatus.RolledBack;
                Build actual;
                actual = await target.ProcessBuildAsync(runData, allowableTimeoutRetries, string.Empty, scriptBatchColl);
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
        public async Task ProcessBuildTest_CommitAfterRetries()
        {
            Initialization init = GetInitializationObject();
            init.TableLockingLoopCount = 700000;
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 2);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);

            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 30;

            Thread THRInfinite = null;
            try
            {
                THRInfinite = new Thread(new ParameterizedThreadStart(StartInfiniteLockingThread));
                THRInfinite.Start(init);

                BuildItemStatus expected = BuildItemStatus.CommittedWithTimeoutRetries;
                Build actual;
                actual = await target.ProcessBuildAsync(runData, allowableTimeoutRetries, string.Empty, scriptBatchColl);
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
