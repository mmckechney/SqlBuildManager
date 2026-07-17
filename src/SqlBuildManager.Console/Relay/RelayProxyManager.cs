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

        internal static bool IsSqlFallbackEligible(Exception exception) =>
            !string.IsNullOrWhiteSpace(Endpoint) &&
            RelayProxyClient.IsSqlPrivateNetworkDenial(exception);

        internal static bool ExtractSqlTestDacpac(
            string server,
            string database,
            string destinationPath)
        {
            CreateClient()
                .ExtractSqlTestDacpacAsync(server, database, destinationPath)
                .GetAwaiter()
                .GetResult();
            return true;
        }

        internal static RelayProxyClient CreateClient() =>
            string.IsNullOrWhiteSpace(Endpoint)
                ? throw new InvalidOperationException("A Relay proxy endpoint is required to use Azure Relay.")
                : new RelayProxyClient(Endpoint);
    }
}
