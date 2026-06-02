using Azure.Core;
using Azure.Identity;
using Microsoft.Data.SqlClient;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace SqlSync.Connection
{
    public static class SqlServerAuthenticationProvider
    {
        private static readonly object SyncRoot = new object();
        private static bool registered;

        public static void Register()
        {
            if (registered)
                return;

            lock (SyncRoot)
            {
                if (registered)
                    return;

                var provider = new AzureIdentitySqlAuthenticationProvider();
                RegisterProvider(provider, SqlAuthenticationMethod.ActiveDirectoryManagedIdentity);
                RegisterProvider(provider, SqlAuthenticationMethod.ActiveDirectoryMSI);
                RegisterProvider(provider, SqlAuthenticationMethod.ActiveDirectoryDefault);

                registered = true;
            }
        }

        private static void RegisterProvider(SqlAuthenticationProvider provider, SqlAuthenticationMethod authenticationMethod)
        {
            if (provider.IsSupported(authenticationMethod) && SqlAuthenticationProvider.GetProvider(authenticationMethod) == null)
                SqlAuthenticationProvider.SetProvider(authenticationMethod, provider);
        }

        private sealed class AzureIdentitySqlAuthenticationProvider : SqlAuthenticationProvider
        {
            private const string DefaultSqlScope = "https://database.windows.net/.default";
            private readonly DefaultAzureCredential defaultCredential = new DefaultAzureCredential();
            private readonly ConcurrentDictionary<string, ManagedIdentityCredential> managedIdentityCredentials = new ConcurrentDictionary<string, ManagedIdentityCredential>();

            public override bool IsSupported(SqlAuthenticationMethod authenticationMethod)
            {
                return authenticationMethod == SqlAuthenticationMethod.ActiveDirectoryDefault ||
                       authenticationMethod == SqlAuthenticationMethod.ActiveDirectoryManagedIdentity ||
                       authenticationMethod == SqlAuthenticationMethod.ActiveDirectoryMSI;
            }

            public override async Task<SqlAuthenticationToken> AcquireTokenAsync(SqlAuthenticationParameters parameters)
            {
                var credential = CreateCredential(parameters);
                var token = await credential.GetTokenAsync(
                    new TokenRequestContext(new[] { GetScope(parameters.Resource) }),
                    CancellationToken.None);

                return new SqlAuthenticationToken(token.Token, token.ExpiresOn);
            }

            private TokenCredential CreateCredential(SqlAuthenticationParameters parameters)
            {
                if (parameters.AuthenticationMethod == SqlAuthenticationMethod.ActiveDirectoryDefault)
                {
                    if (string.IsNullOrWhiteSpace(parameters.UserId))
                        return defaultCredential;

                    return new DefaultAzureCredential(new DefaultAzureCredentialOptions
                    {
                        ManagedIdentityClientId = parameters.UserId
                    });
                }

                return managedIdentityCredentials.GetOrAdd(parameters.UserId ?? string.Empty, CreateManagedIdentityCredential);
            }

            private static ManagedIdentityCredential CreateManagedIdentityCredential(string clientId)
            {
                if (string.IsNullOrWhiteSpace(clientId))
                    return new ManagedIdentityCredential(ManagedIdentityId.SystemAssigned);

                return new ManagedIdentityCredential(ManagedIdentityId.FromUserAssignedClientId(clientId));
            }

            private static string GetScope(string resource)
            {
                if (string.IsNullOrWhiteSpace(resource))
                    return DefaultSqlScope;

                return resource.TrimEnd('/') + "/.default";
            }
        }
    }
}
