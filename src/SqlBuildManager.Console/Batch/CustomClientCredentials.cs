using Azure.Core;
using Microsoft.Rest;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Microsoft.Extensions.Caching.Memory;
namespace SqlBuildManager.Console.Batch
{
    /// <summary>
    /// From https://stackoverflow.com/questions/35228042/how-to-create-serviceclientcredential-to-be-used-with-microsoft-azure-management
    /// </summary>
    internal class CustomClientCredentials : ServiceClientCredentials
    {
        private const string BearerTokenType = "Bearer";
        private TokenCredential _tokenCredential;
        private readonly string[] _scopes;
        private readonly IMemoryCache _cache;

        public CustomClientCredentials(TokenCredential tokenCredential)
        {
            _tokenCredential = tokenCredential ?? throw new ArgumentNullException(nameof(tokenCredential));
            _scopes = new string[] { "https://management.core.windows.net/.default" };
            _cache = new MemoryCache(new MemoryCacheOptions());
        }
        public CustomClientCredentials(TokenCredential tokenCredential, string[] scopes, IMemoryCache cache)
        {
            _tokenCredential = tokenCredential ?? throw new ArgumentNullException(nameof(tokenCredential));
            _scopes = scopes ?? throw new ArgumentNullException(nameof(scopes));
            _cache = cache;
        }

        public override async Task ProcessHttpRequestAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request == null)
            {
                throw new ArgumentNullException(nameof(request));
            }

            var token = await _cache.GetOrCreateAsync("accessToken-tokenProvider." + string.Join("#", _scopes), async e =>
            {
                var accessToken = await _tokenCredential.GetTokenAsync(new TokenRequestContext(_scopes), cancellationToken);
                e.AbsoluteExpiration = accessToken.ExpiresOn;
                return accessToken.Token;
            });
            request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue(BearerTokenType, token);
            await base.ProcessHttpRequestAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }
}
