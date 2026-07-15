using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Azure.Relay;
using System.Net;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace SqlBuildManager.StorageProxy;

internal sealed class BlobProxyRequestHandler
{
    private static readonly string[] AppendLogFiles =
        ["commits.log", "errors.log", "successdatabases.cfg", "failuredatabases.cfg"];

    private static readonly Regex ContainerNamePattern = new(
        "^[a-z0-9](?:[a-z0-9-]{1,61}[a-z0-9])?$",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private readonly string storageAccountName;
    private readonly string hybridConnectionName;
    private readonly BlobServiceClient storageClient;

    public BlobProxyRequestHandler(
        string storageAccountName,
        string hybridConnectionName,
        BlobServiceClient storageClient)
    {
        this.storageAccountName = storageAccountName;
        this.hybridConnectionName = hybridConnectionName;
        this.storageClient = storageClient;
    }

    public async Task HandleAsync(RelayedHttpListenerContext context)
    {
        try
        {
            var segments = GetRouteSegments(context.Request.Url);
            if (context.Request.HttpMethod == "GET" && segments.SequenceEqual(["health"]))
            {
                await WriteJsonAsync(context, HttpStatusCode.OK, new { status = "healthy" });
                return;
            }

            if (segments.Length < 2 || segments[0] != "containers")
            {
                await WriteErrorAsync(context, HttpStatusCode.NotFound, "Unknown proxy route.");
                return;
            }

            var containerName = segments[1];
            if (!ContainerNamePattern.IsMatch(containerName))
            {
                await WriteErrorAsync(context, HttpStatusCode.BadRequest, "Invalid blob container name.");
                return;
            }

            var container = storageClient.GetBlobContainerClient(containerName);

            if (context.Request.HttpMethod == "POST" && segments.Length == 2)
            {
                await EnsureContainerAsync(container);
                await WriteJsonAsync(context, HttpStatusCode.OK, new
                {
                    containerUrl = GetContainerUrl(containerName)
                });
                return;
            }

            if (context.Request.HttpMethod == "GET" &&
                segments.Length == 3 &&
                segments[2] == "has-blobs")
            {
                var hasBlobs = await HasBlobsAsync(container);
                await WriteJsonAsync(context, HttpStatusCode.OK, new { hasBlobs });
                return;
            }

            if (context.Request.HttpMethod == "DELETE" && segments.Length == 2)
            {
                var deleted = await container.DeleteIfExistsAsync();
                if (deleted.Value)
                {
                    await WaitForContainerDeletionAsync(container);
                }
                await WriteJsonAsync(context, HttpStatusCode.OK, new { deleted = deleted.Value });
                return;
            }

            if (context.Request.HttpMethod == "POST" &&
                segments.Length == 3 &&
                segments[2] == "consolidate-logs")
            {
                await ConsolidateLogsAsync(container);
                await WriteJsonAsync(context, HttpStatusCode.OK, new { consolidated = true });
                return;
            }

            if (context.Request.HttpMethod == "POST" &&
                segments.Length == 3 &&
                segments[2] == "combine-query")
            {
                var outputBlobName = GetQueryValue(context.Request.Url, "output");
                if (string.IsNullOrWhiteSpace(outputBlobName) || !IsValidBlobName(outputBlobName))
                {
                    await WriteErrorAsync(context, HttpStatusCode.BadRequest, "A valid output blob name is required.");
                    return;
                }

                await CombineQueryOutputAsync(container, outputBlobName);
                await WriteJsonAsync(context, HttpStatusCode.OK, new { combined = true });
                return;
            }

            if (context.Request.HttpMethod == "GET" &&
                segments.Length == 3 &&
                segments[2] == "download")
            {
                var blobName = GetQueryValue(context.Request.Url, "blob");
                if (string.IsNullOrWhiteSpace(blobName) || !IsValidBlobName(blobName))
                {
                    await WriteErrorAsync(context, HttpStatusCode.BadRequest, "A valid blob name is required.");
                    return;
                }

                context.Response.StatusCode = HttpStatusCode.OK;
                context.Response.Headers[HttpResponseHeader.ContentType] = "application/octet-stream";
                var download = await container.GetBlobClient(blobName).DownloadStreamingAsync();
                await download.Value.Content.CopyToAsync(context.Response.OutputStream);
                return;
            }

            if (context.Request.HttpMethod == "PUT" &&
                segments.Length == 4 &&
                segments[2] == "blobs")
            {
                var blobName = segments[3];
                if (!IsValidBlobName(blobName))
                {
                    await WriteErrorAsync(context, HttpStatusCode.BadRequest, "Invalid blob name.");
                    return;
                }

                await EnsureContainerAsync(container);
                var blob = container.GetBlockBlobClient(blobName);
                await blob.UploadAsync(context.Request.InputStream);
                await WriteJsonAsync(context, HttpStatusCode.Created, new
                {
                    blobUrl = GetBlobUrl(containerName, blobName)
                });
                return;
            }

            await WriteErrorAsync(context, HttpStatusCode.NotFound, "Unknown proxy route.");
        }
        catch (Azure.RequestFailedException ex)
        {
            Console.Error.WriteLine($"Storage request failed: {ex.ErrorCode} {ex.Message}");
            await WriteErrorAsync(context, MapStatus(ex.Status), ex.Message);
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine(ex);
            await WriteErrorAsync(context, HttpStatusCode.InternalServerError, "Blob proxy request failed.");
        }
        finally
        {
            context.Response.Close();
        }
    }

    private string GetContainerUrl(string containerName) =>
        $"https://{storageAccountName}.blob.core.windows.net/{containerName}";

    private string GetBlobUrl(string containerName, string blobName) =>
        $"{GetContainerUrl(containerName)}/{Uri.EscapeDataString(blobName)}";

    private static async Task EnsureContainerAsync(BlobContainerClient container)
    {
        for (var attempt = 1; attempt <= 10; attempt++)
        {
            try
            {
                await container.CreateIfNotExistsAsync();
                return;
            }
            catch (Azure.RequestFailedException ex) when (
                ex.ErrorCode == BlobErrorCode.ContainerBeingDeleted.ToString() &&
                attempt < 10)
            {
                await Task.Delay(TimeSpan.FromSeconds(2));
            }
        }
    }

    private static async Task WaitForContainerDeletionAsync(BlobContainerClient container)
    {
        for (var attempt = 1; attempt <= 30; attempt++)
        {
            try
            {
                if (!await container.ExistsAsync())
                {
                    return;
                }
            }
            catch (Azure.RequestFailedException ex) when (
                ex.ErrorCode == BlobErrorCode.ContainerBeingDeleted.ToString())
            {
            }

            await Task.Delay(TimeSpan.FromSeconds(2));
        }

        throw new TimeoutException($"Timed out waiting for container '{container.Name}' to be deleted.");
    }

    private static async Task ConsolidateLogsAsync(BlobContainerClient container)
    {
        await EnsureContainerAsync(container);
        await foreach (var blob in container.GetBlobsAsync())
        {
            if (blob.Properties.BlobType != BlobType.Block ||
                blob.Properties.ContentLength == 0)
            {
                continue;
            }

            foreach (var appendName in AppendLogFiles)
            {
                if (!blob.Name.Contains(appendName, StringComparison.OrdinalIgnoreCase) ||
                    blob.Name.Equals(appendName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }

                var destination = container.GetAppendBlobClient(appendName);
                await destination.CreateIfNotExistsAsync();
                await using var source = await container.GetBlobClient(blob.Name).OpenReadAsync();
                await destination.AppendBlockAsync(source);
            }
        }
    }

    private static async Task CombineQueryOutputAsync(
        BlobContainerClient container,
        string outputBlobName)
    {
        var destination = container.GetAppendBlobClient(outputBlobName);
        await destination.CreateIfNotExistsAsync();
        var counter = 0;

        await foreach (var blob in container.GetBlobsAsync())
        {
            if (blob.Properties.BlobType != BlobType.Block ||
                !blob.Name.EndsWith(".csv", StringComparison.OrdinalIgnoreCase) ||
                blob.Name.Equals(outputBlobName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            await using var source = await container.GetBlobClient(blob.Name).OpenReadAsync();
            if (counter > 0)
            {
                await SkipFirstLineAsync(source);
            }
            await destination.AppendBlockAsync(source);
            counter++;
        }
    }

    private static async Task SkipFirstLineAsync(Stream stream)
    {
        var buffer = new byte[1];
        while (await stream.ReadAsync(buffer) == 1)
        {
            if (buffer[0] == (byte)'\n')
            {
                return;
            }
        }
    }

    private static async Task<bool> HasBlobsAsync(BlobContainerClient container)
    {
        if (!await container.ExistsAsync())
        {
            return false;
        }

        await foreach (var _ in container.GetBlobsAsync().AsPages(pageSizeHint: 1))
        {
            return true;
        }

        return false;
    }

    private string[] GetRouteSegments(Uri requestUri)
    {
        var segments = requestUri.AbsolutePath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .Select(Uri.UnescapeDataString)
            .ToList();
        if (segments.Count > 0 &&
            string.Equals(segments[0], hybridConnectionName, StringComparison.OrdinalIgnoreCase))
        {
            segments.RemoveAt(0);
        }
        return [.. segments];
    }

    private static bool IsValidBlobName(string blobName) =>
        !string.IsNullOrWhiteSpace(blobName) &&
        blobName.Length <= 1024 &&
        !blobName.Contains('/') &&
        !blobName.Contains('\\');

    private static string? GetQueryValue(Uri uri, string key)
    {
        foreach (var pair in uri.Query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (Uri.UnescapeDataString(parts[0]).Equals(key, StringComparison.OrdinalIgnoreCase))
            {
                return parts.Length == 2 ? Uri.UnescapeDataString(parts[1]) : string.Empty;
            }
        }
        return null;
    }

    private static HttpStatusCode MapStatus(int status) =>
        Enum.IsDefined(typeof(HttpStatusCode), status)
            ? (HttpStatusCode)status
            : HttpStatusCode.BadGateway;

    private static Task WriteErrorAsync(
        RelayedHttpListenerContext context,
        HttpStatusCode statusCode,
        string message) =>
        WriteJsonAsync(context, statusCode, new { error = message });

    private static async Task WriteJsonAsync(
        RelayedHttpListenerContext context,
        HttpStatusCode statusCode,
        object value)
    {
        context.Response.StatusCode = statusCode;
        context.Response.Headers[HttpResponseHeader.ContentType] = "application/json";
        await JsonSerializer.SerializeAsync(context.Response.OutputStream, value);
    }
}
