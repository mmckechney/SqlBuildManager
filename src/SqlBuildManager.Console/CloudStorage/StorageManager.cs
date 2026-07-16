using Azure.Storage;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Sas;
using Microsoft.Azure.Batch;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlSync.Connection;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.CloudStorage
{
    public class StorageManager
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        // PERF-004: Cache initialized BlobContainerClients by "{accountName}|{containerName}".
        // CreateIfNotExistsAsync is called at most once per key per process lifetime.
        internal static readonly ConcurrentDictionary<string, Task<BlobContainerClient>> _containerClientCache
            = new ConcurrentDictionary<string, Task<BlobContainerClient>>(StringComparer.OrdinalIgnoreCase);
        internal static string BlobProxyEndpoint { get; set; } = string.Empty;

        public static void ConfigureBlobProxyEndpoint(string endpoint) =>
            BlobProxyEndpoint = endpoint ?? string.Empty;

        public static bool IsBlobProxyFallbackEligible(Exception exception) =>
            CanUseBlobProxy(exception);

        public static bool IsSqlPrivateNetworkDenial(Exception exception) =>
            BlobProxyClient.IsSqlPrivateNetworkDenial(exception);

        internal static string ContainerCacheKey(string storageAccountName, string containerName)
            => $"{storageAccountName}|{containerName}";

        /// <summary>
        /// Returns a cached, fully initialized BlobContainerClient.
        /// CreateIfNotExistsAsync is called exactly once per account/container per process.
        /// </summary>
        internal static async Task<BlobContainerClient> GetOrCreateBlobContainerClientAsync(
            string storageAccountName, string storageAccountKey, string containerName)
        {
            var key = ContainerCacheKey(storageAccountName, containerName);
            var initializationTask = _containerClientCache.GetOrAdd(key, _ =>
                CreateAndEnsureContainerAsync(storageAccountName, storageAccountKey, containerName));

            try
            {
                return await initializationTask;
            }
            catch
            {
                ((ICollection<KeyValuePair<string, Task<BlobContainerClient>>>)_containerClientCache)
                    .Remove(new KeyValuePair<string, Task<BlobContainerClient>>(key, initializationTask));
                throw;
            }
        }

        private static async Task<BlobContainerClient> CreateAndEnsureContainerAsync(
            string storageAccountName, string storageAccountKey, string containerName)
        {
            BlobContainerClient client;
            if (string.IsNullOrWhiteSpace(storageAccountKey))
            {
                var url = $"https://{storageAccountName}.blob.core.windows.net/{containerName}";
                log.LogDebug($"Creating container client with URL: '{url}' and Token Credential");
                client = new BlobContainerClient(new Uri(url), Aad.AadHelper.TokenCredential);
            }
            else
            {
                var connstr = GetStorageConnectionString(storageAccountName, storageAccountKey);
                log.LogDebug($"Creating container client for '{storageAccountName}/{containerName}' with storage key");
                client = new BlobContainerClient(connstr, containerName);
            }
            await client.CreateIfNotExistsAsync();
            return client;
        }

        // Unique per-process instance ID to prevent blob path collisions when multiple
        // containers in the same ACI group share the same HOSTNAME.
        private static readonly string _instanceSuffix = Guid.NewGuid().ToString("N").Substring(0, 8);
        internal static BlobServiceClient CreateStorageClient(string storageAccountName, string storageAccountKey)
        {
            BlobServiceClient serviceClient = null!;
            if (string.IsNullOrWhiteSpace(storageAccountKey))
            {
                serviceClient = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"), Aad.AadHelper.TokenCredential);
            }
            else
            {
                StorageSharedKeyCredential creds = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
                serviceClient = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"), creds);
            }



            return serviceClient;
        }
        internal static StorageSharedKeyCredential GetStorageSharedKeyCredential(string storageAccountName, string storageAccountKey)
        {
            return new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
        }
        private static string GetStorageConnectionString(string storageAccountName, string storageAccountKey)
        {
            return $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey};EndpointSuffix=core.windows.net";
        }

        internal static bool ConsolidateLogFiles(string storageAccountName, string storageAccountKey, string outputContainerName, List<string> workerFiles)
        {
            try
            {
                var client = CreateStorageClient(storageAccountName, storageAccountKey);
                return ConsolidateLogFilesCore(client, outputContainerName, workerFiles);
            }
            catch (Exception ex) when (CanUseBlobProxy(ex))
            {
                log.LogWarning($"Direct Blob Storage access failed while consolidating '{outputContainerName}'. Retrying through the blob proxy.");
                CreateBlobProxyClient().ConsolidateLogsAsync(outputContainerName).GetAwaiter().GetResult();
                return true;
            }
        }
        internal static bool ConsolidateLogFiles(BlobServiceClient storageSvcClient, string outputContainerName, List<string> workerFiles)
        {
            try
            {
                return ConsolidateLogFilesCore(storageSvcClient, outputContainerName, workerFiles);
            }
            catch (Exception ex) when (CanUseBlobProxy(ex))
            {
                log.LogWarning($"Direct Blob Storage access failed while consolidating '{outputContainerName}'. Retrying through the blob proxy.");
                CreateBlobProxyClient().ConsolidateLogsAsync(outputContainerName).GetAwaiter().GetResult();
                return true;
            }
        }

        private static bool ConsolidateLogFilesCore(BlobServiceClient storageSvcClient, string outputContainerName, List<string> workerFiles)
        {
            workerFiles.AddRange(new string[] { "dacpac", "sbm", "sql", "execution.log", "csv", "cfg" });
            var container = storageSvcClient.GetBlobContainerClient(outputContainerName);
            container.CreateIfNotExists();
            var blobs = container.GetBlobs();
            bool hadErrors = false;

            //Move worker files to "Working" directory
            foreach (var blob in blobs)
            {
                try
                {
                    if (blob.Properties.BlobType == BlobType.Block)
                    {
                        var sourceBlob = container.GetBlobClient(blob.Name);
                        if (sourceBlob.GetProperties().Value.ContentLength == 0)
                        {
                            continue;
                        }
                        foreach (string append in Program.AppendLogFiles)
                        {
                            try
                            {
                                if (blob.Name.ToLower().Contains(append.ToLower()))
                                {
                                    var destinationBlob = container.GetAppendBlobClient(append);
                                    try
                                    {
                                        destinationBlob.CreateIfNotExists();
                                    }
                                    catch { }


                                    using (var stream = sourceBlob.OpenRead())
                                    {
                                        destinationBlob.AppendBlock(stream);
                                    }
                                    log.LogInformation($"Consolidated {blob.Name} to {append}");
                                }
                            }
                            catch (Azure.RequestFailedException exe)
                            {
                                if (exe.ErrorCode == "BlobAlreadyExists")
                                {
                                    log.LogWarning($"Unable to consolidate log file, '{blob.Name}': That file already exists");
                                    hadErrors = true;
                                }
                                else if (exe.ErrorCode == "InvalidHeaderValue")
                                {
                                    log.LogWarning($"Unable to consolidate log file, '{blob.Name}': Problem with appendind the consolidated file. {Environment.NewLine}{exe.Message}");
                                    hadErrors = true;
                                }
                                else
                                {
                                    throw;
                                }
                            }
                        }
                    }
                }
                catch (Azure.RequestFailedException exe)
                {
                    if (exe.ErrorCode == "BlobAlreadyExists")
                    {
                        log.LogWarning($"Unable to consolidate log file, '{blob.Name}': That file already exists. This can happen when you run two Batch jobs with the same job name");
                        hadErrors = true;
                    }
                    else
                    {
                        throw;
                    }
                }
            }
            return !hadErrors;

        }

        internal static async Task<string> WriteFileToLocalStorage(string storageAccountName, string storageAccountKey, string sourceContainerName, string filename)
        {
            try
            {
                var client = CreateStorageClient(storageAccountName, storageAccountKey);
                return await WriteFileToLocalStorage(client, sourceContainerName, filename);
            }
            catch(Exception exe)
            {
                log.LogError($"Error creating storage client: {exe.Message}");
                return string.Empty;
            }

        }
        internal static async Task<string> WriteFileToLocalStorage(BlobServiceClient storageSvcClient, string sourceContainerName, string filename)
        {
            try
            {
                var localPath = Path.Combine(Directory.GetCurrentDirectory(), filename);
                var container = storageSvcClient.GetBlobContainerClient(sourceContainerName);
                var blob = container.GetBlobClient(filename);

                log.LogInformation($"Downloading {blob.Uri} to local working file {localPath}");
                BlobDownloadInfo blobDownload = await blob.DownloadAsync();

                using (FileStream fileStream = System.IO.File.OpenWrite(localPath))
                {
                    await blobDownload.Content.CopyToAsync(fileStream);
                }
                return localPath;
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to save file {filename} to local storage: {ex.Message}");
                return string.Empty;
            }
        }

        internal static string GetContainerRawUrl(string storageAccountName, string outputContainerName)
        {
            return $"https://{storageAccountName}.blob.core.windows.net/{outputContainerName}";
        }

        internal static async Task<string> EnsureOutputContainerAsync(
            string storageAccountName,
            string outputContainerName,
            CancellationToken cancellationToken = default)
        {
            try
            {
                var serviceClient = new BlobServiceClient(
                    new Uri($"https://{storageAccountName}.blob.core.windows.net"),
                    Aad.AadHelper.TokenCredential);
                await serviceClient
                    .GetBlobContainerClient(outputContainerName)
                    .CreateIfNotExistsAsync(cancellationToken: cancellationToken)
                    .ConfigureAwait(false);
                return GetContainerRawUrl(storageAccountName, outputContainerName);
            }
            catch (Exception ex) when (CanUseBlobProxy(ex))
            {
                log.LogWarning($"Direct Blob Storage access failed for container '{outputContainerName}'. Retrying through the blob proxy.");
                return await CreateBlobProxyClient()
                    .EnsureContainerAsync(outputContainerName, cancellationToken)
                    .ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Async version of GetOutputContainerSasUrl.
        /// </summary>
        internal static async Task<string> GetOutputContainerSasUrlAsync(string storageAccountName, string storageAccountKey, string outputContainerName, bool forRead, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(storageAccountKey))
            {
                throw new InvalidOperationException(
                    "SAS generation requires a Storage account key. Managed-identity workflows must use an identity-authenticated Blob URL.");
            }

            var cred = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            return await GetOutputContainerSasUrlWithKeyAsync(
                storageAccountName,
                outputContainerName,
                cred,
                forRead,
                cancellationToken).ConfigureAwait(false);
        }

        private static async Task<string> GetOutputContainerSasUrlWithKeyAsync(string storageAccountName, string outputContainerName, StorageSharedKeyCredential storageCreds, bool forRead, CancellationToken cancellationToken = default)
        {
            log.LogDebug($"Ensuring presence of output blob container '{outputContainerName}'");
            var container = new BlobContainerClient(new Uri($"https://{storageAccountName}.blob.core.windows.net/{outputContainerName}"), storageCreds);
            await container.CreateIfNotExistsAsync(cancellationToken: cancellationToken).ConfigureAwait(false);

            BlobSasBuilder sasConstraints;
            if (!forRead)
            {
                var permissions = BlobSasPermissions.Add | BlobSasPermissions.Create | BlobSasPermissions.Write | BlobSasPermissions.Read | BlobSasPermissions.List;
                sasConstraints = new BlobSasBuilder(permissions, new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0, 0, 0)));
                sasConstraints.StartsOn = DateTime.UtcNow.AddHours(-1);
                sasConstraints.ExpiresOn = DateTime.UtcNow.AddHours(4);
            }
            else
            {
                var permissions = BlobSasPermissions.Read | BlobSasPermissions.List;
                sasConstraints = new BlobSasBuilder(permissions, new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0, 0, 0, 0, 0)));
                sasConstraints.StartsOn = DateTime.UtcNow.AddHours(-1);
                sasConstraints.ExpiresOn = DateTime.UtcNow.AddHours(7);
            }
            sasConstraints.BlobContainerName = outputContainerName;

            return container.GenerateSasUri(sasConstraints).ToString();
        }

        internal static (string jobId, string storageContainerName) GetJobAndStorageNames(CommandLineArgs cmdLine)
        {
            string jobId, storageContainerName;
            string jobToken = DateTime.Now.ToString("yyyy-MM-dd-HHmm-ss-fff");
            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchJobName))
            {
                cmdLine.BatchJobName = cmdLine.BatchArgs.BatchJobName.ToLower();
                if (cmdLine.BatchArgs.BatchJobName.Length < 3 || cmdLine.BatchArgs.BatchJobName.Length > 41 || !Regex.IsMatch(cmdLine.BatchArgs.BatchJobName, @"^[a-z0-9]+(-[a-z0-9]+)*$"))
                {
                    throw new ArgumentException($"The job name must be lower case, between 3 and 41 characters in length, and the only special character allowed are dashes '-'. Value provided: '{cmdLine.JobName}'");
                }
                jobId = cmdLine.BatchArgs.BatchJobName;
                storageContainerName = cmdLine.BatchArgs.BatchJobName + "-" + jobToken; ;
            }
            else
            {
                throw new ArgumentException("The job name is required and must be lower case, between 3 and 41 characters in length, and the only special character allowed are dashes '-'. No Value provided!");
            }

            return (jobId, storageContainerName);
        }
        internal static bool StorageContainerHasExistingFiles(string storageAccountName, string storageAccountKey, string containerName)
        {
            try
            {
                var container = GetBlobContainerClient(storageAccountName, storageAccountKey, containerName);
                return container.GetBlobs().Any();
            }
            catch (Exception ex) when (CanUseBlobProxy(ex))
            {
                log.LogWarning($"Direct Blob Storage access failed while checking container '{containerName}'. Retrying through the blob proxy.");
                return CreateBlobProxyClient().HasBlobsAsync(containerName).GetAwaiter().GetResult();
            }
        }
        internal static async Task<bool> DeleteStorageContainer(string storageAccountName, string storageAccountKey, string containerName)
        {
            try
            {
                // Evict from cache before deletion so the next caller re-creates the container
                _containerClientCache.TryRemove(ContainerCacheKey(storageAccountName, containerName), out _);
                var container = GetBlobContainerClient(storageAccountName, storageAccountKey, containerName);
                var result = await container.DeleteAsync();
                if (result.Status == 202)
                {
                    while (await container.ExistsAsync())
                    {
                        await Task.Delay(1000);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                if (CanUseBlobProxy(ex))
                {
                    log.LogWarning($"Direct Blob Storage access failed while deleting container '{containerName}'. Retrying through the blob proxy.");
                    await CreateBlobProxyClient().DeleteContainerAsync(containerName).ConfigureAwait(false);
                    _containerClientCache.TryRemove(ContainerCacheKey(storageAccountName, containerName), out _);
                    return true;
                }
                log.LogError($"Unable to delete container {containerName} in storage account {storageAccountName}: {ex.Message}");
                return false;
            }
        }
        internal static async Task<bool> UploadFilesToStorageContainer(string storageAccountName, string storageAccountKey, string containerName, string[] filePaths, bool isRetry = false)
        {
            BlobContainerClient container;
            try
            {
                container = await GetOrCreateBlobContainerClientAsync(storageAccountName, storageAccountKey, containerName);
            }
            catch (Azure.RequestFailedException rfExe)
            {
                if (rfExe.ErrorCode == "ContainerBeingDeleted")
                {
                    log.LogInformation("Existing storage container is still being deleted. waiting...");
                    // Evict stale cache entry so retry creates a fresh container
                    _containerClientCache.TryRemove(ContainerCacheKey(storageAccountName, containerName), out _);
                    await Task.Delay(3000);
                    return await UploadFilesToStorageContainer(storageAccountName, storageAccountKey, containerName, filePaths, true);
                }
                if (CanUseBlobProxy(rfExe))
                {
                    return await UploadFilesThroughProxyAsync(containerName, filePaths).ConfigureAwait(false);
                }
                log.LogError($"Unable to upload files '{string.Join(',', filePaths)}' to container {containerName}: {rfExe.Message}");
                return false;
            }
            catch (Exception ex)
            {
                if (CanUseBlobProxy(ex))
                {
                    return await UploadFilesThroughProxyAsync(containerName, filePaths).ConfigureAwait(false);
                }
                log.LogError($"Unable to upload files '{string.Join(',', filePaths)}' to container {containerName}: {ex.Message}");
                return false;
            }

            foreach (var filePath in filePaths)
            {
                if (!isRetry)
                {
                    log.LogInformation($"Uploading file {filePath} to container [{containerName}]...");
                }

                try
                {
                    var blobData = container.GetBlockBlobClient(Path.GetFileName(filePath));
                    using var fs = new FileStream(filePath, FileMode.Open);
                    await blobData.UploadAsync(fs);
                }
                catch (Exception ex) when (CanUseBlobProxy(ex))
                {
                    log.LogWarning($"Direct upload failed for '{filePath}'. Retrying through the blob proxy.");
                    await CreateBlobProxyClient().UploadFileAsync(containerName, filePath).ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    log.LogError($"Unable to upload file '{filePath}' to container {containerName}: {ex.Message}");
                    return false;
                }
                log.LogInformation($"File {filePath} uploaded to container [{containerName}]");
            }
            return true;
        }
        internal static async Task<bool> DownloadBlobToLocal(string sasUrl, string localFileName)
        {
            var containerUri = new Uri(sasUrl);
            var containerName = containerUri.AbsolutePath.Trim('/');
            try
            {
                var cloudBlobContainer = new BlobContainerClient(containerUri);
                var blob = cloudBlobContainer.GetBlobClient(Path.GetFileName(localFileName));
                var resp = await blob.DownloadToAsync(localFileName);
                if (resp.Status < 300)
                {
                    return true;
                }
                else
                {
                    log.LogError($"Unable to download file {Path.GetFileName(localFileName)} to {localFileName}: {resp.ReasonPhrase}");
                    return false;
                }
            }
            catch (Exception ex) when (CanUseBlobProxy(ex))
            {
                log.LogWarning($"Direct Blob Storage access failed while downloading '{localFileName}'. Retrying through the blob proxy.");
                await CreateBlobProxyClient()
                    .DownloadBlobAsync(containerName, Path.GetFileName(localFileName), localFileName)
                    .ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to download file {Path.GetFileName(localFileName)} to {localFileName}: {ex.Message}");
                return false;
            }
        }

        internal static async Task<bool> DownloadBlobToLocal(BlobServiceClient blobSvcClient, string containerName, string localFileName)
        {
            try
            {
                
                var cloudBlobContainer = blobSvcClient.GetBlobContainerClient(containerName);
                var blob = cloudBlobContainer.GetBlobClient(Path.GetFileName(localFileName));
                var resp = await blob.DownloadToAsync(localFileName);
                if (resp.Status < 300)
                {
                    return true;
                }
                else
                {
                    log.LogError($"Unable to download file {Path.GetFileName(localFileName)} to {localFileName}: {resp.ReasonPhrase}");
                    return false;
                }
            }
            catch (Exception ex) when (CanUseBlobProxy(ex))
            {
                log.LogWarning($"Direct Blob Storage access failed while downloading '{localFileName}'. Retrying through the blob proxy.");
                await CreateBlobProxyClient()
                    .DownloadBlobAsync(containerName, Path.GetFileName(localFileName), localFileName)
                    .ConfigureAwait(false);
                return true;
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to download file {Path.GetFileName(localFileName)} to {localFileName}: {ex.Message}");
                return false;
            }
        }

        internal static async Task<ResourceFile> UploadFileToBatchContainerAsync(
            string storageAcctName,
            string containerName,
            StorageSharedKeyCredential storageCreds,
            string filePath,
            string identityResourceId = "",
            CancellationToken cancellationToken = default)
        {
            log.LogInformation($"Uploading file {filePath} to container [{containerName}]...");

            string blobName = Path.GetFileName(filePath);
            BlockBlobClient blobData;
            
            if (storageCreds != null)
            {
                // Legacy: Use storage key
                blobData = new BlockBlobClient(new Uri($"https://{storageAcctName}.blob.core.windows.net/{containerName}/{blobName}"), storageCreds);
            }
            else
            {
                // Use Managed Identity
                blobData = new BlockBlobClient(new Uri($"https://{storageAcctName}.blob.core.windows.net/{containerName}/{blobName}"), Aad.AadHelper.TokenCredential);
            }

            try
            {
                using var fs = new FileStream(filePath, FileMode.Open);
                await blobData.UploadAsync(fs, cancellationToken: cancellationToken).ConfigureAwait(false);
            }
            catch (Exception ex) when (CanUseBlobProxy(ex))
            {
                log.LogWarning($"Direct upload failed for '{filePath}'. Retrying through the blob proxy.");
                var proxiedBlobUrl = await CreateBlobProxyClient().UploadFileAsync(
                    containerName,
                    filePath,
                    cancellationToken).ConfigureAwait(false);
                return CreateBatchResourceFile(proxiedBlobUrl, blobName, identityResourceId);
            }

            if (!string.IsNullOrWhiteSpace(identityResourceId))
            {
                return CreateBatchResourceFile(blobData.Uri.ToString(), blobName, identityResourceId);
            }

            // Legacy shared-key deployments still use SAS.
            if (storageCreds != null)
            {
                // Legacy: Use storage key for SAS
                var sasPermissions = new BlobSasBuilder(BlobSasPermissions.Read, new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0, 0, 0)));
                sasPermissions.BlobName = blobName;
                sasPermissions.BlobContainerName = containerName;
                sasPermissions.StartsOn = DateTime.UtcNow.AddHours(-1);
                sasPermissions.ExpiresOn = DateTime.UtcNow.AddHours(3);
                string blobSasUri = blobData.GenerateSasUri(sasPermissions).ToString();
                return ResourceFile.FromUrl(blobSasUri, blobName, null);
            }
            throw new InvalidOperationException(
                "A Batch managed identity resource ID is required when Storage shared-key authentication is disabled.");
        }

        private static bool CanUseBlobProxy(Exception exception) =>
            !string.IsNullOrWhiteSpace(BlobProxyEndpoint) &&
            BlobProxyClient.IsFallbackEligible(exception);

        private static BlobProxyClient CreateBlobProxyClient() =>
            string.IsNullOrWhiteSpace(BlobProxyEndpoint)
                ? throw new InvalidOperationException("A Blob proxy endpoint is required to use Azure Relay.")
                : new BlobProxyClient(BlobProxyEndpoint);

        /// <summary>
        /// Lists blobs through the configured Azure Relay proxy, optionally restricted by prefix.
        /// </summary>
        public static Task<IReadOnlyList<BlobProxyFile>> EnumerateBlobFilesThroughRelayAsync(
            string containerName,
            string prefix = "",
            CancellationToken cancellationToken = default) =>
            CreateBlobProxyClient().ListBlobsAsync(containerName, prefix, cancellationToken);

        /// <summary>
        /// Downloads the selected blobs through the configured Azure Relay proxy.
        /// Nested blob paths are preserved beneath <paramref name="destinationDirectory"/>.
        /// </summary>
        public static Task<IReadOnlyList<string>> DownloadBlobFilesThroughRelayAsync(
            string containerName,
            IEnumerable<string> blobNames,
            string destinationDirectory,
            CancellationToken cancellationToken = default) =>
            CreateBlobProxyClient().DownloadBlobsAsync(
                containerName,
                blobNames,
                destinationDirectory,
                cancellationToken);

        public static Task CreateSqlTestTableThroughRelayAsync(
            string endpoint,
            string server,
            string database,
            string tableName,
            string columnName,
            CancellationToken cancellationToken = default) =>
            new BlobProxyClient(endpoint).CreateSqlTestTableAsync(
                server,
                database,
                tableName,
                columnName,
                cancellationToken);

        public static Task ExtractSqlTestDacpacThroughRelayAsync(
            string endpoint,
            string server,
            string database,
            string destinationPath,
            CancellationToken cancellationToken = default) =>
            new BlobProxyClient(endpoint).ExtractSqlTestDacpacAsync(
                server,
                database,
                destinationPath,
                cancellationToken);

        private static ResourceFile CreateBatchResourceFile(
            string blobUrl,
            string blobName,
            string identityResourceId) =>
            ResourceFile.FromUrl(
                blobUrl,
                new ComputeNodeIdentityReference { ResourceId = identityResourceId },
                blobName,
                null);

        private static async Task<bool> UploadFilesThroughProxyAsync(string containerName, IEnumerable<string> filePaths)
        {
            log.LogWarning($"Direct Blob Storage access failed for container '{containerName}'. Retrying uploads through the blob proxy.");
            var proxy = CreateBlobProxyClient();
            foreach (var filePath in filePaths)
            {
                await proxy.UploadFileAsync(containerName, filePath).ConfigureAwait(false);
                log.LogInformation($"File {filePath} uploaded to container [{containerName}] through the blob proxy");
            }
            return true;
        }

        internal static bool CombineQueryOutputfiles(BlobServiceClient storageSvcClient, string storageContainerName, string outputFile)
        {
            try
            {
                return CombineQueryOutputfilesCore(storageSvcClient, storageContainerName, outputFile);
            }
            catch (Exception ex) when (CanUseBlobProxy(ex))
            {
                log.LogWarning($"Direct Blob Storage access failed while combining query output in '{storageContainerName}'. Retrying through the blob proxy.");
                CreateBlobProxyClient()
                    .CombineQueryOutputAsync(storageContainerName, Path.GetFileName(outputFile))
                    .GetAwaiter()
                    .GetResult();
                return true;
            }
        }

        private static bool CombineQueryOutputfilesCore(BlobServiceClient storageSvcClient, string storageContainerName, string outputFile)
        {
            bool hadErrors = false;
            log.LogInformation("Consolidating Query output files...");
            outputFile = Path.GetFileName(outputFile);
            var container = storageSvcClient.GetBlobContainerClient(storageContainerName);
            var blobs = container.GetBlobs();

            int counter = 0;
            foreach (var blob in blobs)
            {
                var destinationBlob = container.GetAppendBlobClient(outputFile);
                try
                {
                    destinationBlob.CreateIfNotExists();
                }
                catch { }
                if (blob.Properties.BlobType == BlobType.Block)
                {

                    if (blob.Name.ToLower().EndsWith(".csv"))
                    {
                        if (counter == 0)
                        {
                            var sourceBlob = container.GetBlobClient(blob.Name);
                            using (var stream = sourceBlob.OpenRead())
                            {
                                destinationBlob.AppendBlock(stream);
                            }
                        }
                        else // we need to trim the first line off 
                        {
                            var sourceBlob = container.GetBlobClient(blob.Name);
                            var tmp = Path.GetTempFileName();
                            sourceBlob.DownloadTo(tmp);
                            using (StreamReader reader = new StreamReader(tmp))
                            {
                                var s = reader.ReadLine(); //dump the first line
                                reader.BaseStream.Position = Encoding.UTF8.GetBytes(s!).Length;
                                destinationBlob.AppendBlock(reader.BaseStream);

                            }

                            if (System.IO.File.Exists(tmp))
                            {
                                System.IO.File.Delete(tmp);
                            }
                        }
                        counter++;
                        //sourceBlob.Delete();
                    }
                }
            }
            return !hadErrors;
        }

        internal static async Task<bool> WriteLogsToBlobContainer(string storageAccountName, string storageAccountKey, string outputContainerName, string rootLoggingPath)
        {
            // PERF-004: Use the async cache so CreateIfNotExistsAsync is paid at most once per container per run.
            var container = await GetOrCreateBlobContainerClientAsync(storageAccountName, storageAccountKey, outputContainerName);
            var res = await WriteLogsToBlobContainer(container, rootLoggingPath);
            return res;
        }
        internal static async Task<bool> WriteLogsToBlobContainer(string outputContainerSasUrl, string rootLoggingPath)
        {
            BlobContainerClient container = new BlobContainerClient(new Uri(outputContainerSasUrl));
            var res = await WriteLogsToBlobContainer(container, rootLoggingPath);
            return res;

        }
        private static async Task<bool> WriteLogsToBlobContainer(BlobContainerClient containerClient, string rootLoggingPath)
        {
            try
            {
                //var writeTasks = new List<Task>();
                log.LogInformation($"Writing log files to blob storage at {containerClient.AccountName}");
                var renameLogFiles = new string[] { "sqlbuildmanager", "csv" };

                log.LogInformation($"Getting file list from {rootLoggingPath}");
                var fileList = Directory.GetFiles(rootLoggingPath, "*.*", SearchOption.AllDirectories);
                var taskId = Environment.GetEnvironmentVariable("AZ_BATCH_TASK_ID");
                if (string.IsNullOrEmpty(taskId))
                {
                    taskId = Environment.GetEnvironmentVariable("CONTAINER_APP_REPLICA_NAME");
                }
                if (string.IsNullOrEmpty(taskId))
                {
                    // ACI containers in the same group share HOSTNAME — append instance suffix for uniqueness
                    var hostname = Environment.GetEnvironmentVariable("HOSTNAME");
                    if (!string.IsNullOrEmpty(hostname))
                        taskId = $"{hostname}-{_instanceSuffix}";
                }
                if (string.IsNullOrEmpty(taskId))
                {
                    taskId = $"{Environment.MachineName}-{_instanceSuffix}";
                }
                if (string.IsNullOrEmpty(taskId))
                {
                    taskId = Guid.NewGuid().ToString("N").Substring(0, 12);
                }
                string machine = Environment.MachineName;
                log.LogInformation($"Using task identifier '{taskId}' for blob path prefixes");

                foreach (var f in fileList)
                {
                    try
                    {
                        var fileInfo = new FileInfo(f);
                        if (fileInfo.Length == 0)
                        {
                            log.LogDebug($"No errors logged — '{f}' is empty");
                            continue;
                        }

                        var tmp = Path.GetRelativePath(rootLoggingPath, f);

                        if (Program.AppendLogFiles.Any(a => tmp.ToLower().IndexOf(a) > -1))
                        {
                            tmp = $"{taskId}/{tmp}";
                        }
                        else if (renameLogFiles.Any(a => tmp.ToLower().IndexOf(a) > -1))
                        {
                            tmp = $"{taskId}/{tmp}";
                        }

                        var bClient = containerClient.GetBlockBlobClient(tmp);
                        log.LogInformation($"Saving File '{f}' as '{tmp}'");
                        using (var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                        {
                            await bClient.UploadAsync(fs);
                        }
                    }
                    catch (Exception e)
                    {
                        log.LogError($"Unable to upload log file '{f}' to blob storage: {e.Message}");
                    }
                }
                return true;
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to upload log files to blob storage.{Environment.NewLine}{exe.Message}");
                return false;
            }
        }
        internal static BlobContainerClient GetBlobContainerClient(string storageAccountName, string storageAccountKey, string containerName)
        {
            BlobContainerClient containerClient = null!;
            if (string.IsNullOrWhiteSpace(storageAccountKey))
            {
                var url = $"https://{storageAccountName}.blob.core.windows.net/{containerName}";
                log.LogDebug($"Creating container with URL: '{url}' and Token Credential");
                containerClient = new BlobContainerClient(new Uri(url), Aad.AadHelper.TokenCredential);
            }
            else
            {

                var connstr = GetStorageConnectionString(storageAccountName, storageAccountKey);
                log.LogDebug($"Creating container with account name '{storageAccountName}' and container name '{containerName}' and key '{ConnectionStringRedactor.MaskKey(storageAccountKey)}'. ");
                containerClient = new BlobContainerClient(connstr, containerName);
            }

            containerClient.CreateIfNotExists();
            return containerClient;
        }
        private static BlockBlobClient GetBlockBlobClient(string storageAccountName, string storageAccountKey, string containerName, string blobName)
        {
            BlockBlobClient containerClient = null!;
            if (string.IsNullOrWhiteSpace(storageAccountKey))
            {
                var url = $"https://{storageAccountName}.blob.core.windows.net/{containerName}/{blobName}";
                containerClient = new BlockBlobClient(new Uri(url), Aad.AadHelper.TokenCredential);
            }
            else
            {
                var connstr = GetStorageConnectionString(storageAccountName, storageAccountKey);
                containerClient = new BlockBlobClient(connstr, containerName, blobName);
            }
            return containerClient;
        }

        internal static async Task<bool> CopyFileToStorage(string storageAccountName, string storageAccountKey, string jobName, string localName, string storageName)
        {
            try
            {
                var container = await GetOrCreateBlobContainerClientAsync(storageAccountName, storageAccountKey, jobName);
                var blobData = container.GetBlockBlobClient(storageName);
                using (var fs = new FileStream(localName, FileMode.Open))
                {
                    await blobData.UploadAsync(fs);
                }
                log.LogInformation($"File {localName} uploaded to {storageName} in container [{jobName}]");
                return true;
            }
            catch(Exception exe)
            {
                log.LogError($"Problem uploading file {localName} to storage: " + exe.Message);
                return false;
            }
        }
    }

}
