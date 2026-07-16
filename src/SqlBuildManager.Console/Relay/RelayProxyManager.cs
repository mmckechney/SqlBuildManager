using System;

namespace SqlBuildManager.Console.Relay
{
    public static class RelayProxyManager
    {
        public static string Endpoint { get; private set; } = string.Empty;

        public static void ConfigureEndpoint(string endpoint) =>
            Endpoint = endpoint ?? string.Empty;

        public static bool IsFallbackEligible(Exception exception) =>
            !string.IsNullOrWhiteSpace(Endpoint) &&
            RelayProxyClient.IsFallbackEligible(exception);

        internal static RelayProxyClient CreateClient() =>
            string.IsNullOrWhiteSpace(Endpoint)
                ? throw new InvalidOperationException("A Relay proxy endpoint is required to use Azure Relay.")
                : new RelayProxyClient(Endpoint);
    }
}
