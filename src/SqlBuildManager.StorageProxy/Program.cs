using Azure.Identity;
using Azure.Storage.Blobs;
using Microsoft.Azure.Relay;

namespace SqlBuildManager.StorageProxy;

internal static class Program
{
    public static async Task Main()
    {
        var relayNamespace = GetRequiredSetting("RELAY_NAMESPACE");
        var relayHost = relayNamespace.Contains(".", StringComparison.Ordinal)
            ? relayNamespace
            : $"{relayNamespace}.servicebus.windows.net";
        var hybridConnectionName = GetRequiredSetting("RELAY_CONNECTION_NAME");
        var storageAccountName = GetRequiredSetting("STORAGE_ACCOUNT_NAME");
        var managedIdentityClientId = GetRequiredSetting("MANAGED_IDENTITY_CLIENT_ID");

        var credential = new ManagedIdentityCredential(
            ManagedIdentityId.FromUserAssignedClientId(managedIdentityClientId));
        var tokenProvider = TokenProvider.CreateManagedIdentityTokenProvider(credential);
        var relayUri = new Uri($"sb://{relayHost}/{hybridConnectionName}");
        var listener = new HybridConnectionListener(relayUri, tokenProvider);
        var storageClient = new BlobServiceClient(
            new Uri($"https://{storageAccountName}.blob.core.windows.net"),
            credential);
        var proxy = new BlobProxyRequestHandler(storageAccountName, hybridConnectionName, storageClient);

        listener.Connecting += (_, _) => Console.WriteLine("Azure Relay listener connecting.");
        listener.Offline += (_, _) => Console.WriteLine("Azure Relay listener offline.");
        listener.Online += (_, _) => Console.WriteLine("Azure Relay listener online.");
        listener.RequestHandler = context => _ = proxy.HandleAsync(context);

        using var shutdown = new CancellationTokenSource();
        Console.CancelKeyPress += (_, args) =>
        {
            args.Cancel = true;
            shutdown.Cancel();
        };
        AppDomain.CurrentDomain.ProcessExit += (_, _) => shutdown.Cancel();

        await listener.OpenAsync(shutdown.Token);
        Console.WriteLine($"Blob upload proxy listening at https://{relayHost}/{hybridConnectionName}.");

        try
        {
            await Task.Delay(Timeout.InfiniteTimeSpan, shutdown.Token);
        }
        catch (OperationCanceledException)
        {
        }
        finally
        {
            await listener.CloseAsync(CancellationToken.None);
        }
    }

    private static string GetRequiredSetting(string name)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrWhiteSpace(value)
            ? throw new InvalidOperationException($"Required environment variable '{name}' is not set.")
            : value;
    }
}
