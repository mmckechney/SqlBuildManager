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
}
