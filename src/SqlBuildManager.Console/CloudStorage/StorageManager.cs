using Azure.Storage.Blobs;
using Azure.Storage;
using System;
using System.Collections.Generic;
using System.Text;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Blobs.Specialized;
using Microsoft.Extensions.Logging;
using System.IO;
using Azure.Storage.Sas;
using Microsoft.Azure.Batch;
using System.Threading.Tasks;
using SqlBuildManager.Console.CommandLine;
using System.Text.RegularExpressions;
using System.Linq;

namespace SqlBuildManager.Console.CloudStorage
{
    public class StorageManager
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static BlobServiceClient CreateStorageClient(string storageAccountName, string storageAccountKey)
        {
            StorageSharedKeyCredential creds = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            var serviceClient = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"), creds);

            return serviceClient;
        }
        internal static string GetStorageConnectionString(string storageAccountName, string storageAccountKey)
        {
            return $"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey};EndpointSuffix=core.windows.net";
        }

        internal static bool ConsolidateLogFiles(BlobServiceClient storageSvcClient, string outputContainerName, List<string> workerFiles)
        {
            workerFiles.AddRange(new string[] { "dacpac", "sbm", "sql", "execution.log", "csv" });
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
                                }
                                else if (exe.ErrorCode == "InvalidHeaderValue")
                                {
                                    log.LogWarning($"Unable to consolidate log file, '{blob.Name}': Problem with appendind the consolidated file. {Environment.NewLine}{exe.Message}");
                                }
                                else
                                {
                                    throw;
                                }
                                hadErrors = true;
                            }
                        }
                    }
                }
                catch (Azure.RequestFailedException exe)
                {
                    if (exe.ErrorCode == "BlobAlreadyExists")
                    {
                        log.LogWarning($"Unable to consolidate log file, '{blob.Name}': That file already exists. This can happen when you run two Batch jobs with the same job name");
                    }
                    hadErrors = true;
                }
            }
            return !hadErrors;

        }

        internal static async Task<string> WriteFileToLocalStorage(string storageAccountName, string storageAccountKey, string sourceContainerName, string filename)
        {
            var client = CreateStorageClient(storageAccountName, storageAccountKey);
            return await WriteFileToLocalStorage(client, sourceContainerName,filename);
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

                using (FileStream fileStream = File.OpenWrite(localPath))
                {
                    await blobDownload.Content.CopyToAsync(fileStream);
                }
                return localPath;
            }
            catch(Exception ex)
            {
                log.LogError($"Unable to save file {filename} to local storage: {ex.Message}");
                return string.Empty;
            }
        }
        internal static string GetOutputContainerSasUrl(string storageAccountName, string storageAccountKey, string outputContainerName, bool forRead)
        {
            var cred = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            return GetOutputContainerSasUrl(storageAccountName, outputContainerName,cred,forRead);
        }
        internal static string GetOutputContainerSasUrl(string storageAccountName, string outputContainerName, StorageSharedKeyCredential storageCreds, bool forRead)
        {
            log.LogDebug($"Ensuring presence of output blob container '{outputContainerName}'");
            var container = new BlobContainerClient(new Uri($"https://{storageAccountName}.blob.core.windows.net/{outputContainerName}"), storageCreds);
            container.CreateIfNotExists();

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
                    throw new ArgumentException("The job name must be lower case, between 3 and 41 characters in length, and the only special character allowed are dashes '-'");
                }
                jobId = cmdLine.BatchArgs.BatchJobName;
                storageContainerName = cmdLine.BatchArgs.BatchJobName + "-" + jobToken; ;
            }
            else
            {
                throw new ArgumentException("The job name is required and must be lower case, between 3 and 41 characters in length, and the only special character allowed are dashes '-'");
            }

            return (jobId, storageContainerName);
        }
        internal static async Task<bool> StorageContainerExists(string storageAccountName, string storageAccountKey, string containerName)
        {
            var connstr = GetStorageConnectionString(storageAccountName, storageAccountKey);
            BlobContainerClient container = new BlobContainerClient(connstr, containerName);
            var r = await container.ExistsAsync();
            return r.Value;

        }
        internal static async Task<bool> DeleteStorageContainer(string storageAccountName, string storageAccountKey, string containerName)
        {
            try
            {
                var connstr = GetStorageConnectionString(storageAccountName, storageAccountKey);
                BlobContainerClient container = new BlobContainerClient(connstr, containerName);
                var result = await container.DeleteAsync();
                if(result.Status == 202)
                {
                    while(container.Exists())
                    {
                        System.Threading.Thread.Sleep(1000);
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to delete container {containerName} in storage account {storageAccountName}: {ex.Message}");
                return false;
            }
        }
        internal static async Task<bool> UploadFileToContainer(string storageAccountName, string storageAccountKey, string containerName, string filePath, bool isRetry = false)
        {

            try
            {
                if (!isRetry)
                {
                    log.LogInformation($"Uploading file {filePath} to container [{containerName}]...");
                }
                string blobName = Path.GetFileName(filePath);

                var connstr = GetStorageConnectionString(storageAccountName, storageAccountKey);
                BlobContainerClient container = new BlobContainerClient(connstr, containerName);
                await container.CreateIfNotExistsAsync();

                var storageCreds = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
                BlockBlobClient blobData = new BlockBlobClient(new Uri($"https://{storageAccountName}.blob.core.windows.net/{containerName}/{blobName}"), storageCreds);   //container.GetBlockBlobClient(blobName);
                using (var fs = new FileStream(filePath, FileMode.Open))
                {
                    blobData.Upload(fs);
                }
                log.LogInformation($"File {filePath} uploaded to container [{containerName}]");
                return true;
            }
            catch(Azure.RequestFailedException rfExe)
            {
                if (rfExe.ErrorCode == "ContainerBeingDeleted")
                {
                    log.LogInformation("Existing container is still being deleted. waiting...");
                    System.Threading.Thread.Sleep(2000);
                    return await UploadFileToContainer(storageAccountName, storageAccountKey, containerName, filePath, true); ;
                }
                else
                {
                    log.LogError($"Unable to upload file {filePath} to container {containerName}: {rfExe.Message}");
                    return false;
                }
            }catch (Exception ex)
            {
                log.LogError($"Unable to upload file {filePath} to container {containerName}: {ex.Message}");
                return false;
            }
        }
        internal static ResourceFile UploadFileToBatchContainer(string storageAcctName, string containerName, StorageSharedKeyCredential storageCreds, string filePath)
        {
            log.LogInformation($"Uploading file {filePath} to container [{containerName}]...");

            string blobName = Path.GetFileName(filePath);

            // BlobContainerClient container = new BlobContainerClient(new Uri($"https://{storageAcctName}.blob.core.windows.net storage/{containerName}"), storageCreds);  //SvcClient.GetBlobContainerClient(containerName);
            BlockBlobClient blobData = new BlockBlobClient(new Uri($"https://{storageAcctName}.blob.core.windows.net/{containerName}/{blobName}"), storageCreds);   //container.GetBlockBlobClient(blobName);
            using (var fs = new FileStream(filePath, FileMode.Open))
            {
                blobData.Upload(fs);
            }
            // Set the expiry time and permissions for the blob shared access signature. In this case, no start time is specified,
            // so the shared access signature becomes valid immediately
            var sasPermissions = new BlobSasBuilder(BlobSasPermissions.Read, new DateTimeOffset(DateTime.UtcNow, new TimeSpan(0, 0, 0)));
            sasPermissions.BlobName = blobName;
            sasPermissions.BlobContainerName = containerName;
            sasPermissions.StartsOn = DateTime.UtcNow.AddHours(-1);
            sasPermissions.ExpiresOn = DateTime.UtcNow.AddHours(3);
            // Construct the SAS URL for blob
            blobData.GenerateSasUri(sasPermissions);
            string blobSasUri = blobData.GenerateSasUri(sasPermissions).ToString();

            return ResourceFile.FromUrl(blobSasUri, blobName, null);
        }
        internal static bool CombineBatchQueryOutputfiles(BlobServiceClient storageSvcClient, string storageContainerName, string outputFile)
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
                                reader.BaseStream.Position = Encoding.UTF8.GetBytes(s).Length;
                                destinationBlob.AppendBlock(reader.BaseStream);

                            }

                            if (File.Exists(tmp))
                            {
                                File.Delete(tmp);
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
            try
            {
                //var writeTasks = new List<Task>();
                log.LogInformation($"Writing log files to blob storage at {storageAccountName}");
                var renameLogFiles = new string[] { "sqlbuildmanager", "csv" };
                var connstr = GetStorageConnectionString(storageAccountName, storageAccountKey);
                BlobContainerClient container = new BlobContainerClient(connstr, outputContainerName);
                var fileList = Directory.GetFiles(rootLoggingPath, "*.*", SearchOption.AllDirectories);
                var taskId = Environment.GetEnvironmentVariable("HOSTNAME");
                string machine = Environment.MachineName;

                foreach (var f in fileList)
                {
                    try
                    {
                        var tmp = Path.GetRelativePath(rootLoggingPath, f);

                        if (Program.AppendLogFiles.Any(a => tmp.ToLower().IndexOf(a) > -1))
                        {

                            tmp = $"{taskId}/{tmp}";
                            log.LogInformation($"Saving File '{f}' as '{tmp}'");
                            var rename = container.GetBlockBlobClient(tmp);
                            using (var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                await rename.UploadAsync(fs);

                            }
                        }
                        else if (renameLogFiles.Any(a => tmp.ToLower().IndexOf(a) > -1))
                        {

                            //var localTemp = Path.Combine(Path.GetDirectoryName(f), tmp);
                            //File.Copy(f, localTemp);
                            tmp = $"{taskId}/{tmp}";
                            log.LogInformation($"Saving File '{f}' as '{tmp}'");
                            var rename = container.GetBlockBlobClient(tmp);
                            using (var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                await rename.UploadAsync(fs);

                            }
                        }
                        else
                        {
                            log.LogInformation($"Saving File '{f}' as '{tmp}'");
                            var b = container.GetBlockBlobClient(tmp);
                            using (var fs = new FileStream(f, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                            {
                                await b.UploadAsync(fs);

                            }
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

    }

}

