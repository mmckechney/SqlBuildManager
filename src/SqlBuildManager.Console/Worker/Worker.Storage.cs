using Microsoft.Extensions.Logging;
using Spectre.Console;
using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Relay;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console
{
    internal partial class Worker
    {
        internal static async Task<int> ListRelayBlobFilesAsync(
            CommandLineArgs cmdLine,
            string containerName,
            string prefix,
            CancellationToken cancellationToken)
        {
            if (!InitializeRelayStorageCommand(cmdLine))
            {
                return 1;
            }

            try
            {
                var files = await StorageManager.EnumerateBlobFilesThroughRelayAsync(
                    containerName,
                    prefix,
                    cancellationToken).ConfigureAwait(false);
                WriteBlobFileTable(files);
                return 0;
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to list blobs in container '{containerName}': {ex.Message}");
                return 1;
            }
        }

        internal static async Task<int> DownloadRelayBlobFilesAsync(
            CommandLineArgs cmdLine,
            string containerName,
            IEnumerable<string> blobNames,
            DirectoryInfo outputPath,
            CancellationToken cancellationToken)
        {
            if (!InitializeRelayStorageCommand(cmdLine))
            {
                return 1;
            }

            try
            {
                var downloadedFiles = await StorageManager.DownloadBlobFilesThroughRelayAsync(
                    containerName,
                    blobNames,
                    outputPath.FullName,
                    cancellationToken).ConfigureAwait(false);
                foreach (var downloadedFile in downloadedFiles)
                {
                    System.Console.WriteLine(downloadedFile);
                }
                return 0;
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to download blobs from container '{containerName}': {ex.Message}");
                return 1;
            }
        }

        internal static async Task<int> DownloadRelayBlobFilesByPrefixAsync(
            CommandLineArgs cmdLine,
            string containerName,
            string prefix,
            DirectoryInfo outputPath,
            CancellationToken cancellationToken)
        {
            if (!InitializeRelayStorageCommand(cmdLine))
            {
                return 1;
            }

            try
            {
                var files = await StorageManager.EnumerateBlobFilesThroughRelayAsync(
                    containerName,
                    prefix,
                    cancellationToken).ConfigureAwait(false);
                if (files.Count == 0)
                {
                    System.Console.WriteLine(
                        $"No blobs matching prefix '{prefix}' were found in container '{containerName}'.");
                    return 0;
                }

                var downloadedFiles = await StorageManager.DownloadBlobFilesThroughRelayAsync(
                    containerName,
                    files.Select(file => file.Name),
                    outputPath.FullName,
                    cancellationToken).ConfigureAwait(false);
                foreach (var downloadedFile in downloadedFiles)
                {
                    System.Console.WriteLine(downloadedFile);
                }
                return 0;
            }
            catch (Exception ex)
            {
                log.LogError(
                    $"Unable to download blobs matching prefix '{prefix}' from container '{containerName}': {ex.Message}");
                return 1;
            }
        }

        internal static async Task<int> DownloadAllRelayBlobFilesAsync(
            CommandLineArgs cmdLine,
            string containerName,
            DirectoryInfo outputPath,
            CancellationToken cancellationToken)
        {
            if (!InitializeRelayStorageCommand(cmdLine))
            {
                return 1;
            }

            try
            {
                var files = await StorageManager.EnumerateBlobFilesThroughRelayAsync(
                    containerName,
                    string.Empty,
                    cancellationToken).ConfigureAwait(false);
                if (files.Count == 0)
                {
                    System.Console.WriteLine($"No blobs were found in container '{containerName}'.");
                    return 0;
                }

                var destinationPath = GetDownloadAllDestinationPath(outputPath, containerName);
                var downloadedFiles = await StorageManager.DownloadBlobFilesThroughRelayAsync(
                    containerName,
                    files.Select(file => file.Name),
                    destinationPath,
                    cancellationToken).ConfigureAwait(false);
                foreach (var downloadedFile in downloadedFiles)
                {
                    System.Console.WriteLine(downloadedFile);
                }
                return 0;
            }
            catch (Exception ex)
            {
                log.LogError(
                    $"Unable to download all blobs from container '{containerName}': {ex.Message}");
                return 1;
            }
        }

        internal static string GetDownloadAllDestinationPath(DirectoryInfo outputPath, string containerName)
        {
            ArgumentNullException.ThrowIfNull(outputPath);
            if (string.IsNullOrWhiteSpace(containerName))
            {
                throw new ArgumentException("A container name is required.", nameof(containerName));
            }

            return Path.Combine(outputPath.FullName, containerName.Trim());
        }

        private static bool InitializeRelayStorageCommand(CommandLineArgs cmdLine)
        {
            var (success, initializedArgs) = Init(cmdLine);
            if (!success)
            {
                log.LogError("Unable to initialize settings for the Blob Storage Relay command.");
                return false;
            }

            RelayProxyManager.ConfigureEndpoint(initializedArgs.ConnectionArgs.RelayProxyEndpoint);
            if (string.IsNullOrWhiteSpace(RelayProxyManager.Endpoint))
            {
                log.LogError(
                    "A Relay proxy endpoint is required. Provide --relayproxyendpoint or a settings file containing Connections.RelayProxyEndpoint.");
                return false;
            }

            return true;
        }

        private static void WriteBlobFileTable(IReadOnlyList<RelayBlobFile> files)
        {
            var table = new Table()
                .AddColumn("Name")
                .AddColumn(new TableColumn("Bytes").RightAligned())
                .AddColumn("Last modified")
                .AddColumn("Content type");

            foreach (var file in files)
            {
                table.AddRow(
                    Markup.Escape(file.Name),
                    file.ContentLength.ToString("N0"),
                    file.LastModified?.ToString("u") ?? string.Empty,
                    Markup.Escape(file.ContentType ?? string.Empty));
            }

            AnsiConsole.Write(table);
            System.Console.WriteLine($"{files.Count} blob(s)");
        }
    }
}
