using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Aad
{
    internal sealed class RetryingTokenCredential : TokenCredential
    {
        internal const int MaxRetries = 2;
        internal static readonly TimeSpan DefaultRetryDelay = TimeSpan.FromSeconds(2);

        private static readonly ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<RetryingTokenCredential>();
        private readonly TokenCredential innerCredential;
        private readonly TimeSpan retryDelay;

        internal RetryingTokenCredential(
            TokenCredential innerCredential,
            int maxRetries = MaxRetries,
            TimeSpan? retryDelay = null)
        {
            this.innerCredential = innerCredential ?? throw new ArgumentNullException(nameof(innerCredential));
            if (maxRetries < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(maxRetries));
            }

            MaxRetryAttempts = maxRetries;
            this.retryDelay = retryDelay ?? DefaultRetryDelay;
        }

        private int MaxRetryAttempts { get; }

        public override AccessToken GetToken(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    return innerCredential.GetToken(requestContext, cancellationToken);
                }
                catch (Exception exception) when (ShouldRetry(exception, cancellationToken, attempt))
                {
                    LogRetry(exception, attempt);
                    WaitBeforeRetry(cancellationToken);
                }
            }
        }

        public override async ValueTask<AccessToken> GetTokenAsync(
            TokenRequestContext requestContext,
            CancellationToken cancellationToken)
        {
            for (var attempt = 0; ; attempt++)
            {
                try
                {
                    return await innerCredential.GetTokenAsync(
                        requestContext,
                        cancellationToken).ConfigureAwait(false);
                }
                catch (Exception exception) when (ShouldRetry(exception, cancellationToken, attempt))
                {
                    LogRetry(exception, attempt);
                    await Task.Delay(retryDelay, cancellationToken).ConfigureAwait(false);
                }
            }
        }

        internal static bool IsTransientAuthenticationFailure(Exception exception) =>
            GetExceptions(exception).Any(candidate =>
                candidate is TimeoutException or TaskCanceledException ||
                candidate is AuthenticationFailedException authenticationFailed &&
                    IsAzureCliAuthenticationTimeout(authenticationFailed));

        private bool ShouldRetry(
            Exception exception,
            CancellationToken cancellationToken,
            int attempt) =>
            !cancellationToken.IsCancellationRequested &&
            attempt < MaxRetryAttempts &&
            IsTransientAuthenticationFailure(exception);

        private static bool IsAzureCliAuthenticationTimeout(AuthenticationFailedException exception)
        {
            var message = exception.Message ?? string.Empty;
            return message.Contains("Azure CLI", StringComparison.OrdinalIgnoreCase) &&
                message.Contains("timed out", StringComparison.OrdinalIgnoreCase);
        }

        private void LogRetry(Exception exception, int attempt)
        {
            log.LogWarning(
                "Transient Azure credential failure ({ExceptionType}); retrying token acquisition attempt {Attempt} of {Maximum}.",
                exception.GetType().Name,
                attempt + 2,
                MaxRetryAttempts + 1);
        }

        private void WaitBeforeRetry(CancellationToken cancellationToken) =>
            Task.Delay(retryDelay, cancellationToken).GetAwaiter().GetResult();

        private static IEnumerable<Exception> GetExceptions(Exception exception)
        {
            if (exception is AggregateException aggregate)
            {
                foreach (var innerException in aggregate.Flatten().InnerExceptions)
                {
                    foreach (var candidate in GetExceptions(innerException))
                    {
                        yield return candidate;
                    }
                }
                yield break;
            }

            yield return exception;
            if (exception.InnerException != null)
            {
                foreach (var candidate in GetExceptions(exception.InnerException))
                {
                    yield return candidate;
                }
            }
        }
    }
}
