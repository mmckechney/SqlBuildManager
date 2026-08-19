using System;

namespace SqlBuildManager.LocalContainer.IntegrationTest;

public static class LocalContainerTestEnvironment
{
    public static string Platform =>
        Environment.GetEnvironmentVariable("SBM_TEST_PLATFORM") ?? "all";

    public static string BlobEndpoint =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SBM_TEST_BLOB_ENDPOINT"))
            ? Environment.GetEnvironmentVariable("SBM_TEST_BLOB_ENDPOINT")!
            : !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SBM_BLOB_ENDPOINT"))
                ? Environment.GetEnvironmentVariable("SBM_BLOB_ENDPOINT")!
                : "http://localhost:10000/devstoreaccount1";

    public static string EventHubConnectionString =>
        Environment.GetEnvironmentVariable("SBM_TEST_EVENTHUB_CONNECTION_STRING") ?? string.Empty;

    public static string EventHubName =>
        Environment.GetEnvironmentVariable("SBM_TEST_EVENTHUB_NAME") ?? "sbm-events";

    public static string ServiceBusConnectionString =>
        Environment.GetEnvironmentVariable("SBM_TEST_SERVICEBUS_CONNECTION_STRING") ?? string.Empty;

    public static bool EmulatorsConfigured =>
        !string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable("SBM_TEST_BLOB_ENDPOINT")) &&
        !string.IsNullOrWhiteSpace(EventHubConnectionString) &&
        !string.IsNullOrWhiteSpace(ServiceBusConnectionString);

    public static int RuntimeContainerCount
    {
        get
        {
            var configured = Environment.GetEnvironmentVariable("SBM_RUNTIME_CONTAINER_COUNT");
            if (string.IsNullOrWhiteSpace(configured))
            {
                return 2;
            }

            if (!int.TryParse(configured, out var count) || count < 1)
            {
                throw new InvalidOperationException("SBM_RUNTIME_CONTAINER_COUNT must be an integer greater than zero.");
            }

            return count;
        }
    }

    public static void RequireRuntimeInfrastructure()
    {
        if (!EmulatorsConfigured)
        {
            throw new InvalidOperationException(
                "Local-container tests require Azurite, Event Hubs Emulator, and Service Bus Emulator configuration.");
        }

        foreach (var name in new[] { "SBM_RUNTIME_IMAGE", "SBM_RUNTIME_NETWORK", "SBM_TEST_RESULTS_VOLUME" })
        {
            if (string.IsNullOrWhiteSpace(Environment.GetEnvironmentVariable(name)))
            {
                throw new InvalidOperationException($"{name} must be configured for local-container tests.");
            }
        }

        _ = RuntimeContainerCount;
    }
}
