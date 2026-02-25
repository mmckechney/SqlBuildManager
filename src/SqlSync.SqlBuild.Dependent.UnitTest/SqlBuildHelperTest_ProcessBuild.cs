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
        private static List<Initialization> initColl = null!;

        // Used to signal when the locking thread has acquired its lock
        private ManualResetEventSlim _lockAcquiredEvent = null!;
        // Used to signal the locking thread to release and exit
        private CancellationTokenSource _lockingCts = null!;
        // Reference to the locking connection for cleanup
        private SqlConnection _lockingConnection = null!;
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
            _lockingConnection = null!;
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
        
        public async Task ProcessBuildTest_CommitWithZeroRetries()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 20);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel(buildData, true, false);
            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 0;

            BuildItemStatus expected = BuildItemStatus.Committed;
            Build actual;
            actual = await target.ProcessBuildAsync(runData, allowableTimeoutRetries, string.Empty, scriptBatchColl);
            Assert.AreEqual(expected, actual.FinalStatus);

            // Verify database state: pre-batched scripts use newid() so only check logging
            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            Assert.IsTrue(sqlLoggingCount > 0, $"Expected rows in SqlBuild_Logging but found {sqlLoggingCount}");

        }

        /// <summary>
        ///A test for ProcessBuildAsync
        ///</summary>
        [TestMethod()]
        
        public async Task ProcessBuildTest_CommitWithRetriesNotUsed()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 20);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel(buildData, true, false);
            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 3;

            BuildItemStatus expected = BuildItemStatus.Committed;
            Build actual;
            actual = await target.ProcessBuildAsync(runData, allowableTimeoutRetries, string.Empty, scriptBatchColl);
            Assert.AreEqual(expected, actual.FinalStatus);

            // Verify database state: pre-batched scripts use newid() so only check logging
            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            Assert.IsTrue(sqlLoggingCount > 0, $"Expected rows in SqlBuild_Logging but found {sqlLoggingCount}");

        }

        [TestMethod()]
        
        public async Task ProcessBuildTest_RollbackWithThreeRetries()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 1);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel(buildData, true, false);

            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 3;

            Thread lockingThread = null!;
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

                // Verify database state: rollback means nothing logged
                int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
                Assert.AreEqual(0, sqlLoggingCount, $"Expected 0 rows in SqlBuild_Logging after rollback but found {sqlLoggingCount}");
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
        
        public async Task ProcessBuildTest_RollbackWithZeroRetries()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 2);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel(buildData, true, false);

            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 0;

            Thread lockingThread = null!;
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

                // Verify database state: rollback means nothing logged
                int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
                Assert.AreEqual(0, sqlLoggingCount, $"Expected 0 rows in SqlBuild_Logging after rollback but found {sqlLoggingCount}");
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
        
        public async Task ProcessBuildTest_CommitAfterRetries()
        {
            Initialization init = GetInitializationObject();
           SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddScriptForProcessBuild(ref buildData, true, 2);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel(buildData, true, false);

            ScriptBatchCollection scriptBatchColl = init.GetScriptBatchCollectionForProcessBuild();
            int allowableTimeoutRetries = 30;

            Thread lockingThread = null!;
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

                // Verify database state: committed after retries means logging rows should be present
                int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
                Assert.IsTrue(sqlLoggingCount > 0, $"Expected rows in SqlBuild_Logging after commit but found {sqlLoggingCount}");
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

        #region Phase 2: New ProcessBuildAsync Scenarios

        [TestMethod()]
        public async Task ProcessBuildTest_MultipleScripts_AllCommitAndLog()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            // Add 3 insert scripts with different build orders
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            init.AddInsertScript(ref buildData, true);
            // Set distinct build orders
            buildData.Script[0].BuildOrder = 1;
            buildData.Script[1].BuildOrder = 2;
            buildData.Script[2].BuildOrder = 3;

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel(buildData, isTransactional: true, isTrial: false);

            Build actual = await target.ProcessBuildAsync(runData, 0, string.Empty, null!);
            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            // All 3 scripts should have inserted rows and been logged
            int testTableCount = init.GetTestTableRowCount(0);
            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            Assert.IsTrue(testTableCount >= 3, $"Expected at least 3 rows in TransactionTest but found {testTableCount}");
            Assert.IsTrue(sqlLoggingCount >= 3, $"Expected at least 3 rows in SqlBuild_Logging but found {sqlLoggingCount}");
        }

        [TestMethod()]
        public async Task ProcessBuildTest_FailureCausesBuildFailure_RollsBack()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            // Add a good insert first, then a failure script
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, rollBackOnError: true, causeBuildFailure: true);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel(buildData, isTransactional: true, isTrial: false);

            Build actual = await target.ProcessBuildAsync(runData, 0, string.Empty, null!);
            Assert.AreEqual(BuildItemStatus.RolledBack, actual.FinalStatus);

            // Rollback means nothing should be committed
            int testTableCount = init.GetTestTableRowCount(0);
            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            Assert.AreEqual(0, testTableCount, $"Expected 0 rows in TransactionTest after rollback but found {testTableCount}");
            Assert.AreEqual(0, sqlLoggingCount, $"Expected 0 rows in SqlBuild_Logging after rollback but found {sqlLoggingCount}");
        }

        [TestMethod()]
        public async Task ProcessBuildTest_NonTransactional_PartialCommitOnFailure()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            // Add a good insert first, then a failure script
            init.AddInsertScript(ref buildData, true);
            init.AddFailureScript(ref buildData, rollBackOnError: true, causeBuildFailure: true);

            SqlBuildHelper target = init.CreateSqlBuildHelper_NonTransactional(buildData, false);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel(buildData, isTransactional: false, isTrial: false);

            Build actual = await target.ProcessBuildAsync(runData, 0, string.Empty, null!);
            Assert.AreEqual(BuildItemStatus.FailedNoTransaction, actual.FinalStatus);

            // Non-transactional: the first insert should persist even though build failed
            int testTableCount = init.GetTestTableRowCount(0);
            Assert.IsTrue(testTableCount >= 1, $"Expected at least 1 row in TransactionTest (partial commit) but found {testTableCount}");
        }

        [TestMethod()]
        public async Task ProcessBuildTest_Trial_RollsBackWithNoDataChanges()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            init.AddInsertScript(ref buildData, true);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel(buildData, isTransactional: true, isTrial: true);

            Build actual = await target.ProcessBuildAsync(runData, 0, string.Empty, null!);
            Assert.AreEqual(BuildItemStatus.TrialRolledBack, actual.FinalStatus);

            // Trial build: everything should be rolled back
            int testTableCount = init.GetTestTableRowCount(0);
            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            Assert.AreEqual(0, testTableCount, $"Expected 0 rows in TransactionTest after trial but found {testTableCount}");
            Assert.AreEqual(0, sqlLoggingCount, $"Expected 0 rows in SqlBuild_Logging after trial but found {sqlLoggingCount}");
        }

        [TestMethod()]
        public async Task ProcessBuildTest_BatchedScripts_AllBatchesExecute()
        {
            Initialization init = GetInitializationObject();
            SqlSyncBuildDataModel buildData = init.CreateSqlSyncSqlBuildDataModelObject();
            // AddBatchInsertScripts creates a script with 2 INSERT batches separated by GO
            init.AddBatchInsertScripts(ref buildData, true);

            SqlBuildHelper target = init.CreateSqlBuildHelper(buildData);
            SqlBuildRunDataModel runData = init.GetSqlBuildRunDataModel(buildData, isTransactional: true, isTrial: false);

            Build actual = await target.ProcessBuildAsync(runData, 0, string.Empty, null!);
            Assert.AreEqual(BuildItemStatus.Committed, actual.FinalStatus);

            // The batched script has 2 GO-separated INSERT statements
            int testTableCount = init.GetTestTableRowCount(0);
            int sqlLoggingCount = init.GetSqlBuildLoggingRowCountByBuildFileName(0);
            Assert.IsTrue(testTableCount >= 2, $"Expected at least 2 rows in TransactionTest from batched script but found {testTableCount}");
            Assert.IsTrue(sqlLoggingCount >= 1, $"Expected at least 1 row in SqlBuild_Logging but found {sqlLoggingCount}");
        }

        #endregion

    }
}
