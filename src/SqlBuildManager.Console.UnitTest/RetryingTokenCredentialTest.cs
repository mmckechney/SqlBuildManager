using Azure;
using Azure.Core;
using Azure.Identity;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using SqlBuildManager.Console.Aad;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.UnitTest
{
    [TestClass]
    public class RetryingTokenCredentialTest
    {
        private sealed class SequenceCredential : TokenCredential
        {
            private readonly Func<int, AccessToken> getToken;
            private int attempts;

            internal SequenceCredential(Func<int, AccessToken> getToken)
            {
                this.getToken = getToken;
            }

            public override AccessToken GetToken(
                TokenRequestContext requestContext,
                CancellationToken cancellationToken) =>
                getToken(Interlocked.Increment(ref attempts));

            public override ValueTask<AccessToken> GetTokenAsync(
                TokenRequestContext requestContext,
                CancellationToken cancellationToken) =>
                ValueTask.FromResult(getToken(Interlocked.Increment(ref attempts)));
        }

        [TestMethod]
        public async Task GetTokenAsync_TimeoutThenSuccess_Retries()
        {
            var credential = new SequenceCredential(attempt =>
            {
                if (attempt == 1)
                {
                    throw new TimeoutException("Credential process timed out.");
                }

                return new AccessToken("token", DateTimeOffset.UtcNow.AddMinutes(5));
            });
            var target = new RetryingTokenCredential(
                credential,
                maxRetries: 2,
                retryDelay: TimeSpan.Zero);

            var token = await target.GetTokenAsync(
                new TokenRequestContext(["https://management.azure.com/.default"]),
                CancellationToken.None);

            Assert.AreEqual("token", token.Token);
        }

        [TestMethod]
        public async Task GetTokenAsync_InvalidCredential_DoesNotRetry()
        {
            var attempts = 0;
            var credential = new SequenceCredential(_ =>
            {
                attempts++;
                throw new AuthenticationFailedException("Azure CLI account is not logged in.");
            });
            var target = new RetryingTokenCredential(
                credential,
                maxRetries: 2,
                retryDelay: TimeSpan.Zero);

            await Assert.ThrowsExactlyAsync<AuthenticationFailedException>(
                () => target.GetTokenAsync(
                    new TokenRequestContext(["https://management.azure.com/.default"]),
                    CancellationToken.None).AsTask());

            Assert.AreEqual(1, attempts);
        }

        [TestMethod]
        public void IsTransientAuthenticationFailure_RecognizesWrappedAzureCliTimeout()
        {
            var exception = new AuthenticationFailedException(
                "The ChainedTokenCredential failed due to an unhandled exception: Azure CLI authentication timed out.");

            Assert.IsTrue(RetryingTokenCredential.IsTransientAuthenticationFailure(exception));
        }
    }
}
