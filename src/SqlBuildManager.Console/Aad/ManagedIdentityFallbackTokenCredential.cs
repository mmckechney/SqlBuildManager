using Azure.Core;
using Azure.Identity;
using Microsoft.Identity.Client;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Aad
{
    internal sealed class ManagedIdentityFallbackTokenCredential : TokenCredential
    {
        private const string AllSourcesUnavailableErrorCode = "managed_identity_all_sources_unavailable";
        private readonly TokenCredential innerCredential;

        internal ManagedIdentityFallbackTokenCredential(string managedIdentityClientId)
            : this(CreateManagedIdentityCredential(managedIdentityClientId))
        {
        }

        internal ManagedIdentityFallbackTokenCredential(TokenCredential innerCredential)
        {
            this.innerCredential = innerCredential ?? throw new ArgumentNullException(nameof(innerCredential));
        }

        public override AccessToken GetToken(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            try
            {
                return innerCredential.GetToken(requestContext, cancellationToken);
            }
            catch (AuthenticationFailedException ex) when (IsManagedIdentityUnavailableForFallback(ex))
            {
                throw new CredentialUnavailableException(
                    "Managed identity is unavailable in the current environment. Falling back to developer credentials.",
                    ex);
            }
        }

        public override async ValueTask<AccessToken> GetTokenAsync(TokenRequestContext requestContext, CancellationToken cancellationToken)
        {
            try
            {
                return await innerCredential.GetTokenAsync(requestContext, cancellationToken).ConfigureAwait(false);
            }
            catch (AuthenticationFailedException ex) when (IsManagedIdentityUnavailableForFallback(ex))
            {
                throw new CredentialUnavailableException(
                    "Managed identity is unavailable in the current environment. Falling back to developer credentials.",
                    ex);
            }
        }

        internal static bool IsManagedIdentityUnavailableForFallback(AuthenticationFailedException exception)
        {
            if (exception == null)
            {
                return false;
            }

            if (ContainsMsalErrorCode(exception, AllSourcesUnavailableErrorCode))
            {
                return true;
            }

            var message = exception.Message ?? string.Empty;
            if (message.IndexOf("all managed identity sources are unavailable", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (message.IndexOf("imdsv2 probe failed", StringComparison.OrdinalIgnoreCase) >= 0 ||
                message.IndexOf("imdsv1 probe failed", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            if (message.IndexOf("169.254.169.254", StringComparison.OrdinalIgnoreCase) >= 0 &&
                message.IndexOf("unreachable", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return true;
            }

            return false;
        }

        private static bool ContainsMsalErrorCode(Exception exception, string errorCode)
        {
            Exception? current = exception;
            while (current != null)
            {
                if (current is MsalClientException msal &&
                    string.Equals(msal.ErrorCode, errorCode, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }

                current = current.InnerException;
            }

            return false;
        }

        private static TokenCredential CreateManagedIdentityCredential(string managedIdentityClientId)
        {
            if (string.IsNullOrWhiteSpace(managedIdentityClientId))
            {
                return new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned);
            }

            return new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(managedIdentityClientId));
        }
    }
}
