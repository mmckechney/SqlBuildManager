using Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CloudStorage;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class BlobProxyClientTest
    {
        [TestMethod]
        public void Constructor_AzureRelayEndpoint_DoesNotThrow()
        {
            _ = new BlobProxyClient("https://example.servicebus.windows.net/blobupload");
        }

        [TestMethod]
        public void Constructor_NonHttpsEndpoint_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(
                () => new BlobProxyClient("http://proxy.example.test/blobupload"));
        }

        [TestMethod]
        public void Constructor_NonRelayHost_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(
                () => new BlobProxyClient("https://proxy.example.test/blobupload"));
        }

        [TestMethod]
        public void Constructor_NestedPath_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(
                () => new BlobProxyClient("https://example.servicebus.windows.net/blobupload/extra"));
        }

        [TestMethod]
        public void IsFallbackEligible_StorageForbidden_ReturnsTrue()
        {
            var exception = new RequestFailedException(403, "Storage public access is disabled.");

            Assert.IsTrue(BlobProxyClient.IsFallbackEligible(exception));
        }

        [TestMethod]
        public void IsFallbackEligible_UnrelatedStorageFailure_ReturnsFalse()
        {
            var exception = new RequestFailedException(404, "Blob not found.");

            Assert.IsFalse(BlobProxyClient.IsFallbackEligible(exception));
        }

        [TestMethod]
        public void IsFallbackEligible_NetworkFailure_ReturnsTrue()
        {
            Assert.IsTrue(BlobProxyClient.IsFallbackEligible(new HttpRequestException("Network unavailable.")));
        }

        [TestMethod]
        public void IsBlobProxyFallbackEligible_ConfiguredProxyAndStorageForbidden_ReturnsTrue()
        {
            try
            {
                StorageManager.ConfigureBlobProxyEndpoint(
                    "https://example.servicebus.windows.net/blobupload");

                Assert.IsTrue(StorageManager.IsBlobProxyFallbackEligible(
                    new RequestFailedException(403, "Storage public access is disabled.")));
            }
            finally
            {
                StorageManager.ConfigureBlobProxyEndpoint(string.Empty);
            }
        }

        [TestMethod]
        public void IsBlobProxyFallbackEligible_NoConfiguredProxy_ReturnsFalse()
        {
            StorageManager.ConfigureBlobProxyEndpoint(string.Empty);

            Assert.IsFalse(StorageManager.IsBlobProxyFallbackEligible(
                new RequestFailedException(403, "Storage public access is disabled.")));
        }

        [TestMethod]
        public void GetSafeDownloadPath_NestedBlob_PreservesRelativePath()
        {
            var root = Path.Combine(Path.GetTempPath(), "blob-download");

            var result = BlobProxyClient.GetSafeDownloadPath(root, "worker-1/logs/results.csv");

            Assert.AreEqual(
                Path.Combine(Path.GetFullPath(root), "worker-1", "logs", "results.csv"),
                result);
        }

        [TestMethod]
        [DataRow("../secret.txt")]
        [DataRow("worker/../../secret.txt")]
        [DataRow("/rooted.txt")]
        [DataRow("worker\\secret.txt")]
        [DataRow("worker/file?.txt")]
        public void GetSafeDownloadPath_UnsafeBlobName_ThrowsArgumentException(string blobName)
        {
            Assert.ThrowsExactly<ArgumentException>(
                () => BlobProxyClient.GetSafeDownloadPath(Path.GetTempPath(), blobName));
        }

        [TestMethod]
        public async Task DownloadBlobsAsync_NoBlobNames_ThrowsArgumentException()
        {
            var client = new BlobProxyClient("https://example.servicebus.windows.net/blobupload");

            await Assert.ThrowsExactlyAsync<ArgumentException>(
                () => client.DownloadBlobsAsync("container", [], Path.GetTempPath()));
        }

        [TestMethod]
        public async Task DownloadBlobsAsync_UnsafeSelection_ValidatesBeforeSending()
        {
            var client = new BlobProxyClient("https://example.servicebus.windows.net/blobupload");
            var blobNames = new List<string> { "safe.txt", "../unsafe.txt" };

            await Assert.ThrowsExactlyAsync<ArgumentException>(
                () => client.DownloadBlobsAsync("container", blobNames, Path.GetTempPath()));
        }
    }
}
