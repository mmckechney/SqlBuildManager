using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
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

        // Used to signal when the locking thread has acquired its lock
        private ManualResetEventSlim _lockAcquiredEvent;
        // Used to signal the locking thread to release and exit
        private CancellationTokenSource _lockingCts;
        // Reference to the locking connection for cleanup
        private SqlConnection _lockingConnection;
        // Duration to hold lock (null = indefinite until cancelled)
        private TimeSpan? _lockDuration;

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

        [TestInitialize]
        public void TestSetup()
        {
            _lockAcquiredEvent = new ManualResetEventSlim(false);
            _lockingCts = new CancellationTokenSource();
            _lockingConnection = null;
            _lockDuration = null; // Default: hold indefinitely
        }

        [TestCleanup]
        public void TestTeardown()
        {
            // Ensure the locking thread is stopped and resources are cleaned up
            _lockingCts?.Cancel();
            
            // Force close the connection to release any locks
            try
            {
                _lockingConnection?.Close();
                _lockingConnection?.Dispose();
            }
            catch { /* Ignore cleanup errors */ }
            
            _lockAcquiredEvent?.Dispose();
            _lockingCts?.Dispose();
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
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task ProcessBuildTest_RollbackWithThreeRetries()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 1);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);

            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 3;

            Thread lockingThread = null;
            try
            {
                // Hold lock indefinitely for this test
                _lockDuration = null;
                lockingThread = new Thread(() => StartLockingThreadWithSignal(init));
                lockingThread.Start();

                // Wait for the lock to be acquired before proceeding
                bool lockAcquired = _lockAcquiredEvent.Wait(TimeSpan.FromSeconds(10));
                Assert.IsTrue(lockAcquired, "Locking thread failed to acquire lock within timeout");

                BuildItemStatus expected = BuildItemStatus.RolledBackAfterRetries;
                Build actual;
                actual = await target.ProcessBuildAsync(runData, allowableTimeoutRetries, string.Empty, scriptBatchColl);
                Assert.AreEqual(expected, actual.FinalStatus);
            }
            finally
            {
                // Signal the locking thread to stop and close the connection
                _lockingCts?.Cancel();
                _lockingConnection?.Close();
                lockingThread?.Join(TimeSpan.FromSeconds(5));
            }

        }

        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task ProcessBuildTest_RollbackWithZeroRetries()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 2);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);

            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 0;

            Thread lockingThread = null;
            try
            {
                // Hold lock indefinitely for this test
                _lockDuration = null;
                lockingThread = new Thread(() => StartLockingThreadWithSignal(init));
                lockingThread.Start();

                // Wait for the lock to be acquired before proceeding
                bool lockAcquired = _lockAcquiredEvent.Wait(TimeSpan.FromSeconds(10));
                Assert.IsTrue(lockAcquired, "Locking thread failed to acquire lock within timeout");

                BuildItemStatus expected = BuildItemStatus.RolledBack;
                Build actual;
                actual = await target.ProcessBuildAsync(runData, allowableTimeoutRetries, string.Empty, scriptBatchColl);
                Assert.AreEqual(expected, actual.FinalStatus);
            }
            finally
            {
                // Signal the locking thread to stop and close the connection
                _lockingCts?.Cancel();
                _lockingConnection?.Close();
                lockingThread?.Join(TimeSpan.FromSeconds(5));
            }

        }

        [TestMethod()]
        [DeploymentItem("SqlSync.SqlBuild.dll")]
        public async Task ProcessBuildTest_CommitAfterRetries()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 2);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel_TransactionalNotTrial(buildData);

            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 30;

            Thread lockingThread = null;
            try
            {
                // Hold lock for only 8 seconds, then release so retry can succeed
                // Script timeout is 2 seconds, so after ~4 retries the lock should be released
                _lockDuration = TimeSpan.FromSeconds(8);
                lockingThread = new Thread(() => StartLockingThreadWithSignal(init));
                lockingThread.Start();

                // Wait for the lock to be acquired before proceeding
                bool lockAcquired = _lockAcquiredEvent.Wait(TimeSpan.FromSeconds(10));
                Assert.IsTrue(lockAcquired, "Locking thread failed to acquire lock within timeout");

                BuildItemStatus expected = BuildItemStatus.CommittedWithTimeoutRetries;
                Build actual;
                actual = await target.ProcessBuildAsync(runData, allowableTimeoutRetries, string.Empty, scriptBatchColl);
                Assert.AreEqual(expected, actual.FinalStatus);
            }
            finally
            {
                // Signal the locking thread to stop and close the connection
                _lockingCts?.Cancel();
                _lockingConnection?.Close();
                lockingThread?.Join(TimeSpan.FromSeconds(5));
            }

        }

        private void StartLockingThreadWithSignal(Initialization init)
        {
            string connStr = string.Format(init.connectionString, init.testDatabaseNames[0]);
            _lockingConnection = new SqlConnection(connStr);
            DateTime? lockEndTime = _lockDuration.HasValue ? DateTime.UtcNow.Add(_lockDuration.Value) : (DateTime?)null;
            
            try
            {
                _lockingConnection.Open();
                
                // Begin a transaction to hold locks
                using (var transaction = _lockingConnection.BeginTransaction())
                {
                    // Insert and acquire lock on TransactionTest table
                    using (var cmd = new SqlCommand("INSERT INTO dbo.TransactionTest VALUES ('PROCESS LOCK', newid(), getdate())", _lockingConnection, transaction))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Acquire exclusive table lock
                    using (var cmd = new SqlCommand("SELECT TOP 1 * FROM dbo.TransactionTest WITH (TABLOCKX)", _lockingConnection, transaction))
                    {
                        cmd.ExecuteNonQuery();
                    }
                    
                    // Signal that lock is acquired
                    _lockAcquiredEvent.Set();
                    
                    // Hold the lock until duration expires or cancelled
                    while (!_lockingCts.Token.IsCancellationRequested)
                    {
                        // Check if lock duration has expired
                        if (lockEndTime.HasValue && DateTime.UtcNow >= lockEndTime.Value)
                        {
                            break;
                        }
                        
                        // Small delay to prevent tight loop
                        Thread.Sleep(100);
                    }
                    
                    // Rollback transaction to release locks
                    transaction.Rollback();
                }
            }
            catch
            {
                // Swallow exceptions - connection may be closed by test cleanup
            }
            finally
            {
                try
                {
                    _lockingConnection?.Close();
                }
                catch { /* Ignore */ }
            }
        }

    }
}
