using Azure;
using Azure.Core;
using SqlBuildManager.Console.Aad;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.CloudStorage
{
    internal sealed class BlobProxyClient
    {
        private static readonly HttpClient HttpClient = new(new HttpClientHandler
        {
            AllowAutoRedirect = false
        })
        {
            Timeout = Timeout.InfiniteTimeSpan
        };

        private readonly Uri endpoint;

        public BlobProxyClient(string endpoint)
        {
            if (!Uri.TryCreate(endpoint, UriKind.Absolute, out var parsed) ||
                parsed.Scheme != Uri.UriSchemeHttps ||
                !parsed.IsDefaultPort ||
                !string.IsNullOrEmpty(parsed.UserInfo) ||
                !string.IsNullOrEmpty(parsed.Query) ||
                !string.IsNullOrEmpty(parsed.Fragment) ||
                !parsed.Host.EndsWith(".servicebus.windows.net", StringComparison.OrdinalIgnoreCase) ||
                parsed.AbsolutePath.Split('/', StringSplitOptions.RemoveEmptyEntries).Length != 1)
            {
                throw new ArgumentException(
                    "Blob proxy endpoint must be an Azure Relay HTTPS URI with one Hybrid Connection path segment.",
                    nameof(endpoint));
            }

            this.endpoint = new Uri(parsed.AbsoluteUri.TrimEnd('/') + "/");
        }

        public async Task<string> UploadFileAsync(
            string containerName,
            string filePath,
            CancellationToken cancellationToken = default)
        {
            var blobName = Path.GetFileName(filePath);
            await using var stream = File.OpenRead(filePath);
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");
            using var response = await SendAsync(
                HttpMethod.Put,
                $"containers/{Escape(containerName)}/blobs/{Escape(blobName)}",
                content,
                cancellationToken).ConfigureAwait(false);
            return await ReadUrlAsync(response, "blobUrl", cancellationToken).ConfigureAwait(false);
        }

        public async Task<string> EnsureContainerAsync(
            string containerName,
            CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(
                HttpMethod.Post,
                $"containers/{Escape(containerName)}",
                null,
                cancellationToken).ConfigureAwait(false);
            return await ReadUrlAsync(response, "containerUrl", cancellationToken).ConfigureAwait(false);
        }

        public async Task<bool> HasBlobsAsync(
            string containerName,
            CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(
                HttpMethod.Get,
                $"containers/{Escape(containerName)}/has-blobs",
                null,
                cancellationToken).ConfigureAwait(false);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var result = await JsonSerializer.DeserializeAsync<HasBlobsResponse>(
                stream,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            return result?.HasBlobs ?? false;
        }

        public async Task DeleteContainerAsync(
            string containerName,
            CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(
                HttpMethod.Delete,
                $"containers/{Escape(containerName)}",
                null,
                cancellationToken).ConfigureAwait(false);
        }

        public async Task ConsolidateLogsAsync(
            string containerName,
            CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(
                HttpMethod.Post,
                $"containers/{Escape(containerName)}/consolidate-logs",
                null,
                cancellationToken).ConfigureAwait(false);
        }

        public async Task CombineQueryOutputAsync(
            string containerName,
            string outputBlobName,
            CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(
                HttpMethod.Post,
                $"containers/{Escape(containerName)}/combine-query?output={Escape(outputBlobName)}",
                null,
                cancellationToken).ConfigureAwait(false);
        }

        public async Task DownloadBlobAsync(
            string containerName,
            string blobName,
            string localFilePath,
            CancellationToken cancellationToken = default)
        {
            using var response = await SendAsync(
                HttpMethod.Get,
                $"containers/{Escape(containerName)}/download?blob={Escape(blobName)}",
                null,
                cancellationToken).ConfigureAwait(false);
            await using var source = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            await using var destination = new FileStream(
                localFilePath,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None);
            await source.CopyToAsync(destination, cancellationToken).ConfigureAwait(false);
        }

        public async Task<IReadOnlyList<BlobProxyFile>> ListBlobsAsync(
            string containerName,
            string prefix = "",
            CancellationToken cancellationToken = default)
        {
            var path = $"containers/{Escape(containerName)}/blobs";
            if (!string.IsNullOrEmpty(prefix))
            {
                path += $"?prefix={Escape(prefix)}";
            }

            using var response = await SendAsync(
                HttpMethod.Get,
                path,
                null,
                cancellationToken).ConfigureAwait(false);
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var result = await JsonSerializer.DeserializeAsync<ListBlobsResponse>(
                stream,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            return result?.Blobs ?? [];
        }

        public async Task<IReadOnlyList<string>> DownloadBlobsAsync(
            string containerName,
            IEnumerable<string> blobNames,
            string destinationDirectory,
            CancellationToken cancellationToken = default)
        {
            ArgumentNullException.ThrowIfNull(blobNames);
            var names = blobNames.Distinct(StringComparer.Ordinal).ToArray();
            if (names.Length == 0)
            {
                throw new ArgumentException("At least one blob name is required.", nameof(blobNames));
            }

            var downloads = names
                .Select(blobName => new
                {
                    BlobName = blobName,
                    DestinationPath = GetSafeDownloadPath(destinationDirectory, blobName)
                })
                .ToArray();
            var downloadedFiles = new List<string>(downloads.Length);
            foreach (var download in downloads)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(download.DestinationPath)!);
                await DownloadBlobAsync(
                    containerName,
                    download.BlobName,
                    download.DestinationPath,
                    cancellationToken).ConfigureAwait(false);
                downloadedFiles.Add(download.DestinationPath);
            }

            return downloadedFiles;
        }

        public static bool IsFallbackEligible(Exception exception) =>
            exception is HttpRequestException or TaskCanceledException ||
            exception is RequestFailedException requestFailed &&
                (requestFailed.Status == 0 ||
                 requestFailed.Status == (int)HttpStatusCode.Forbidden ||
                 requestFailed.ErrorCode == "AuthorizationFailure");

        private async Task<HttpResponseMessage> SendAsync(
            HttpMethod method,
            string relativePath,
            HttpContent? content,
            CancellationToken cancellationToken)
        {
            var token = await AadHelper.TokenCredential.GetTokenAsync(
                new TokenRequestContext(["https://relay.azure.net/.default"]),
                cancellationToken).ConfigureAwait(false);
            using var request = new HttpRequestMessage(method, new Uri(endpoint, relativePath))
            {
                Content = content
            };
            request.Headers.TryAddWithoutValidation(
                "ServiceBusAuthorization",
                new AuthenticationHeaderValue("Bearer", token.Token).ToString());
            var response = await HttpClient.SendAsync(
                request,
                HttpCompletionOption.ResponseHeadersRead,
                cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                var error = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
                var statusCode = response.StatusCode;
                var reasonPhrase = response.ReasonPhrase;
                response.Dispose();
                throw new HttpRequestException(
                    $"Blob proxy returned {(int)statusCode} ({reasonPhrase}): {error}",
                    null,
                    statusCode);
            }
            return response;
        }

        private static async Task<string> ReadUrlAsync(
            HttpResponseMessage response,
            string propertyName,
            CancellationToken cancellationToken)
        {
            await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
            var result = await JsonSerializer.DeserializeAsync<UrlResponse>(
                stream,
                cancellationToken: cancellationToken).ConfigureAwait(false);
            var url = propertyName == "blobUrl" ? result?.BlobUrl : result?.ContainerUrl;
            return string.IsNullOrWhiteSpace(url)
                ? throw new InvalidDataException($"Blob proxy response did not include '{propertyName}'.")
                : url;
        }

        private static string Escape(string value) => Uri.EscapeDataString(value);

        internal static string GetSafeDownloadPath(string destinationDirectory, string blobName)
        {
            if (string.IsNullOrWhiteSpace(destinationDirectory))
            {
                throw new ArgumentException("A destination directory is required.", nameof(destinationDirectory));
            }
            if (string.IsNullOrWhiteSpace(blobName) ||
                blobName.Contains('\\') ||
                blobName.StartsWith('/') ||
                blobName.EndsWith('/') ||
                blobName.Split('/').Any(segment =>
                    string.IsNullOrEmpty(segment) ||
                    segment == "." ||
                    segment == ".." ||
                    segment.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0))
            {
                throw new ArgumentException("Blob name contains an unsafe local path.", nameof(blobName));
            }

            var root = Path.GetFullPath(destinationDirectory);
            var relativePath = blobName.Replace('/', Path.DirectorySeparatorChar);
            var destinationPath = Path.GetFullPath(Path.Combine(root, relativePath));
            var rootPrefix = root.TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
            if (!destinationPath.StartsWith(rootPrefix, StringComparison.OrdinalIgnoreCase))
            {
                throw new ArgumentException("Blob name resolves outside the destination directory.", nameof(blobName));
            }

            return destinationPath;
        }

        private sealed class UrlResponse
        {
            [JsonPropertyName("blobUrl")]
            public string BlobUrl { get; set; } = string.Empty;

            [JsonPropertyName("containerUrl")]
            public string ContainerUrl { get; set; } = string.Empty;
        }

        private sealed class HasBlobsResponse
        {
            [JsonPropertyName("hasBlobs")]
            public bool HasBlobs { get; set; }
        }

        private sealed class ListBlobsResponse
        {
            [JsonPropertyName("blobs")]
            public List<BlobProxyFile> Blobs { get; set; } = [];
        }
    }
}
