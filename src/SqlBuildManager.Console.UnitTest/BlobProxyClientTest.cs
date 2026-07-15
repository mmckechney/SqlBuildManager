using Azure;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.CloudStorage;
using System;
using System.Net.Http;

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
    }
}
