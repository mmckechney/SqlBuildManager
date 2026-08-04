using Azure;
using Azure.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Relay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class RelayProxyClientTest
    {
        [TestMethod]
        public void Constructor_AzureRelayEndpoint_DoesNotThrow()
        {
            _ = new RelayProxyClient("https://example.servicebus.windows.net/relayproxy");
        }

        [TestMethod]
        public void Constructor_NonHttpsEndpoint_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(
                () => new RelayProxyClient("http://proxy.example.test/relayproxy"));
        }

        [TestMethod]
        public void Constructor_NonRelayHost_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(
                () => new RelayProxyClient("https://proxy.example.test/relayproxy"));
        }

        [TestMethod]
        public void Constructor_NestedPath_ThrowsArgumentException()
        {
            Assert.ThrowsExactly<ArgumentException>(
                () => new RelayProxyClient("https://example.servicebus.windows.net/relayproxy/extra"));
        }

        [TestMethod]
        public void IsFallbackEligible_StorageForbidden_ReturnsTrue()
        {
            var exception = new RequestFailedException(403, "Storage public access is disabled.");

            Assert.IsTrue(RelayProxyClient.IsFallbackEligible(exception));
        }

        [TestMethod]
        public void IsFallbackEligible_UnrelatedStorageFailure_ReturnsFalse()
        {
            var exception = new RequestFailedException(404, "Blob not found.");

            Assert.IsFalse(RelayProxyClient.IsFallbackEligible(exception));
        }

        [TestMethod]
        public void IsFallbackEligible_NetworkFailure_ReturnsTrue()
        {
            Assert.IsTrue(RelayProxyClient.IsFallbackEligible(new HttpRequestException("Network unavailable.")));
        }

        [TestMethod]
        public void IsFallbackEligible_ManagedIdentityUnavailableAuthenticationFailure_ReturnsTrue()
        {
            var exception = new AuthenticationFailedException(
                "All Managed Identity sources are unavailable. IMDSv2 probe failed. " +
                "A socket operation was attempted to an unreachable network. (169.254.169.254:80)");

            Assert.IsTrue(RelayProxyClient.IsFallbackEligible(exception));
        }

        [TestMethod]
        public void IsFallbackEligible_CredentialUnavailable_ReturnsTrue()
        {
            Assert.IsTrue(RelayProxyClient.IsFallbackEligible(
                new CredentialUnavailableException("Managed identity is unavailable in the current environment.")));
        }

        [TestMethod]
        public void IsTransientAuthenticationFailure_AzureCliTimeout_ReturnsTrue()
        {
            Assert.IsTrue(RelayProxyClient.IsTransientAuthenticationFailure(
                new AuthenticationFailedException(
                    "The ChainedTokenCredential failed due to an unhandled exception: Azure CLI authentication timed out.")));
        }

        [TestMethod]
        public void IsTransientAuthenticationFailure_InvalidCredential_ReturnsFalse()
        {
            Assert.IsFalse(RelayProxyClient.IsTransientAuthenticationFailure(
                new AuthenticationFailedException("Azure CLI authentication failed because the account is not logged in.")));
        }

        [TestMethod]
        public void IsTransientAuthenticationFailure_TimeoutException_ReturnsTrue()
        {
            Assert.IsTrue(RelayProxyClient.IsTransientAuthenticationFailure(
                new TimeoutException("Credential process timed out.")));
        }

        [TestMethod]
        [DataRow(47073, "Connection was denied because Deny Public Network Access is set to Yes.", true)]
        [DataRow(47073, "Localized Azure SQL network denial.", true)]
        [DataRow(18456, "Deny Public Network Access is set to Yes.", false)]
        public void IsSqlPrivateNetworkDenial_ClassifiesExactSqlFailure(
            int number,
            string message,
            bool expected)
        {
            Assert.AreEqual(
                expected,
                RelayProxyClient.IsSqlPrivateNetworkDenial(number, message));
        }

        [TestMethod]
        public void IsRelayProxyFallbackEligible_ConfiguredProxyAndStorageForbidden_ReturnsTrue()
        {
            try
            {
                RelayProxyManager.ConfigureEndpoint(
                    "https://example.servicebus.windows.net/relayproxy");

                Assert.IsTrue(RelayProxyManager.IsFallbackEligible(
                    new RequestFailedException(403, "Storage public access is disabled.")));
            }
            finally
            {
                RelayProxyManager.ConfigureEndpoint(string.Empty);
            }
        }

        [TestMethod]
        public void IsRelayProxyFallbackEligible_NoConfiguredProxy_ReturnsFalse()
        {
            RelayProxyManager.ConfigureEndpoint(string.Empty);

            Assert.IsFalse(RelayProxyManager.IsFallbackEligible(
                new RequestFailedException(403, "Storage public access is disabled.")));
        }

        [TestMethod]
        public void GetSafeDownloadPath_NestedBlob_PreservesRelativePath()
        {
            var root = Path.Combine(Path.GetTempPath(), "blob-download");

            var result = RelayProxyClient.GetSafeDownloadPath(root, "worker-1/logs/results.csv");

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
                () => RelayProxyClient.GetSafeDownloadPath(Path.GetTempPath(), blobName));
        }

        [TestMethod]
        public async Task DownloadBlobsAsync_NoBlobNames_ThrowsArgumentException()
        {
            var client = new RelayProxyClient("https://example.servicebus.windows.net/relayproxy");

            await Assert.ThrowsExactlyAsync<ArgumentException>(
                () => client.DownloadBlobsAsync("container", [], Path.GetTempPath()));
        }

        [TestMethod]
        public async Task DownloadBlobsAsync_UnsafeSelection_ValidatesBeforeSending()
        {
            var client = new RelayProxyClient("https://example.servicebus.windows.net/relayproxy");
            var blobNames = new List<string> { "safe.txt", "../unsafe.txt" };

            await Assert.ThrowsExactlyAsync<ArgumentException>(
                () => client.DownloadBlobsAsync("container", blobNames, Path.GetTempPath()));
        }
    }
}
