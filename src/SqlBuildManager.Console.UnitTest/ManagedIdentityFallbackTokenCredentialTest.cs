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
    public class ManagedIdentityFallbackTokenCredentialTest
    {
        private sealed class ThrowingCredential : TokenCredential
        {
            private readonly Exception exception;

            internal ThrowingCredential(Exception exception)
            {
                this.exception = exception;
            }

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                throw exception;
            }

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                throw exception;
            }
        }

        private sealed class StaticTokenCredential : TokenCredential
        {
            private readonly AccessToken token;

            internal StaticTokenCredential(string tokenValue)
            {
                token = new AccessToken(tokenValue, DateTimeOffset.UtcNow.AddMinutes(5));
            }

            public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return token;
            }

            public override ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
            {
                return ValueTask.FromResult(token);
            }
        }

        [TestMethod]
        public void GetToken_ConvertsManagedIdentityUnavailableToCredentialUnavailable()
        {
            var inner = new ThrowingCredential(new AuthenticationFailedException("All Managed Identity sources are unavailable. IMDSv2 probe failed."));
            var target = new ManagedIdentityFallbackTokenCredential(inner);

            try
            {
                target.GetToken(new TokenRequestContext(new[] { "https://storage.azure.com/.default" }), CancellationToken.None);
                Assert.Fail("Expected CredentialUnavailableException.");
            }
            catch (CredentialUnavailableException)
            {
            }
        }

        [TestMethod]
        public void GetToken_DoesNotConvertOtherManagedIdentityFailures()
        {
            var inner = new ThrowingCredential(new AuthenticationFailedException("Managed identity endpoint returned status code 400."));
            var target = new ManagedIdentityFallbackTokenCredential(inner);

            try
            {
                target.GetToken(new TokenRequestContext(new[] { "https://storage.azure.com/.default" }), CancellationToken.None);
                Assert.Fail("Expected AuthenticationFailedException.");
            }
            catch (AuthenticationFailedException)
            {
            }
        }

        [TestMethod]
        public void GetToken_PassesThroughInnerToken()
        {
            var expected = "token-value";
            var target = new ManagedIdentityFallbackTokenCredential(new StaticTokenCredential(expected));

            var token = target.GetToken(new TokenRequestContext(new[] { "https://storage.azure.com/.default" }), CancellationToken.None);

            Assert.AreEqual(expected, token.Token);
        }
    }
}
