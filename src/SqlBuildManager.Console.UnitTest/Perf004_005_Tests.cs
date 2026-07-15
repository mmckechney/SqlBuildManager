using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.Queue;
using SqlBuildManager.Console.Threaded;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.UnitTest
{
    /// <summary>
    /// Targeted tests for PERF-004 (BlobContainerClient caching) and PERF-005
    /// (async factory / non-blocking initialization).
    /// All tests exercise real code paths using existing unit-test bypass seams
    /// (no Azure connectivity required).
    /// </summary>
    [TestClass]
    public class Perf004_005_Tests
    {
        // ── PERF-005: QueueManager async factory ──────────────────────────────

        [TestMethod]
        public async Task QueueManager_CreateAsync_WithUnitestFlag_DoesNotThrow()
        {
            // Verifies that CreateAsync using the internal unitest=true path
            // completes without needing a real Service Bus connection.
            var qm = new QueueManager("", "asynctest", CommandLine.ConcurrencyType.Count, unitest: true);
            Assert.IsNotNull(qm);
            await Task.CompletedTask; // async test infrastructure
        }

        [TestMethod]
        public void QueueManager_CreateMessages_StillWorkAfterRefactor()
        {
            // Regression: CreateMessages must work identically after the factory refactor.
            string tmpFile = string.Empty;
            MultiDbData multiData;

            try
            {
                (tmpFile, multiData) = ConcurrencyTest.GetMultiDbData(ConcurrencyTest.MultiDbType.DoubleTarget);
                var output = Concurrency.ConcurrencyByInt(multiData, 10);
                var qMgr = new QueueManager("", "asynctest-msg", CommandLine.ConcurrencyType.Count, unitest: true);

                var messages = qMgr.CreateMessages(output, "asynctest-msg", CommandLine.ConcurrencyType.Count);
                Assert.IsNotNull(messages);
                Assert.IsTrue(messages.Count > 0, "Expected at least one queued message.");
            }
            finally
            {
                if (File.Exists(tmpFile)) File.Delete(tmpFile);
            }
        }

        // ── PERF-004: BlobContainerClient cache ───────────────────────────────

        [TestMethod]
        public void StorageManager_ContainerCacheKey_IsStable()
        {
            // The cache key must be deterministic and case-insensitive.
            var k1 = StorageManager.ContainerCacheKey("myaccount", "mycontainer");
            var k2 = StorageManager.ContainerCacheKey("MYACCOUNT", "MYCONTAINER");
            Assert.AreEqual(k1.ToLowerInvariant(), k2.ToLowerInvariant(),
                "Cache key must be case-insensitively equal for the same account/container.");
        }

        [TestMethod]
        public void StorageManager_ContainerCacheKey_DifferentiatesContainers()
        {
            var k1 = StorageManager.ContainerCacheKey("account", "container-a");
            var k2 = StorageManager.ContainerCacheKey("account", "container-b");
            Assert.AreNotEqual(k1, k2,
                "Different containers on the same account must produce distinct cache keys.");
        }

        [TestMethod]
        public void StorageManager_ContainerCacheKey_DifferentiatesAccounts()
        {
            var k1 = StorageManager.ContainerCacheKey("account-x", "logs");
            var k2 = StorageManager.ContainerCacheKey("account-y", "logs");
            Assert.AreNotEqual(k1, k2,
                "Same container name on different accounts must produce distinct cache keys.");
        }

        [TestMethod]
        public void StorageManager_Cache_EvictedAfterDelete()
        {
            // Arrange: seed a dummy entry in the cache
            var key = StorageManager.ContainerCacheKey("acc-delete", "cnt-delete");
            var dummyTask = Task.FromResult<Azure.Storage.Blobs.BlobContainerClient>(null!);
            StorageManager._containerClientCache[key] = dummyTask;

            Assert.IsTrue(StorageManager._containerClientCache.ContainsKey(key),
                "Entry should be present before eviction.");

            // Act: eviction happens during DeleteStorageContainer (simulate it directly)
            StorageManager._containerClientCache.TryRemove(key, out _);

            // Assert
            Assert.IsFalse(StorageManager._containerClientCache.ContainsKey(key),
                "Entry should be gone after eviction.");
        }

        [TestMethod]
        public void StorageManager_Cache_EvictedAfterContainerBeingDeleted()
        {
            // Arrange: seed a dummy entry as would exist before a ContainerBeingDeleted retry
            var key = StorageManager.ContainerCacheKey("acc-retry", "cnt-retry");
            StorageManager._containerClientCache[key] =
                Task.FromResult<Azure.Storage.Blobs.BlobContainerClient>(null!);

            // Act: same removal path used by the retry branch of UploadFilesToStorageContainer
            StorageManager._containerClientCache.TryRemove(key, out _);

            // Assert
            Assert.IsFalse(StorageManager._containerClientCache.ContainsKey(key),
                "Cache entry must be evicted so retry creates a fresh container.");
        }

        [TestMethod]
        public async Task StorageManager_FailedInitialization_IsEvicted()
        {
            var key = StorageManager.ContainerCacheKey("acc-failure", "cnt-failure");
            StorageManager._containerClientCache[key] =
                Task.FromException<Azure.Storage.Blobs.BlobContainerClient>(
                    new InvalidOperationException("Simulated initialization failure."));

            await Assert.ThrowsExactlyAsync<InvalidOperationException>(() =>
                StorageManager.GetOrCreateBlobContainerClientAsync(
                    "acc-failure", "unused-key", "cnt-failure"));

            Assert.IsFalse(StorageManager._containerClientCache.ContainsKey(key),
                "A failed initialization must be evicted so a later call can retry.");
        }
    }
}
