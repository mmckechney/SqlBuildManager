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
using SqlBuildManager.Console.ContainerApp.Internal;
using static System.Net.WebRequestMethods;

namespace SqlBuildManager.Console.CloudStorage
{
    public class StorageManager
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static BlobServiceClient CreateStorageClient(string storageAccountName, string storageAccountKey)
        {
            BlobServiceClient serviceClient = null;
            if (string.IsNullOrWhiteSpace(storageAccountKey))
            {
                serviceClient = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"),Aad.AadHelper.TokenCredential);
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
            var client = CreateStorageClient(storageAccountName, storageAccountKey);
            return ConsolidateLogFiles(client,outputContainerName, workerFiles);
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

                using (FileStream fileStream = System.IO.File.OpenWrite(localPath))
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
       
        internal static string GetContainerRawUrl(string storageAccountName, string outputContainerName)
        {
            return $"https://{storageAccountName}.blob.core.windows.net/{outputContainerName}";
        }
        internal static string GetOutputContainerSasUrl(string storageAccountName, string storageAccountKey, string outputContainerName, bool forRead)
        {
            var cred = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            return GetOutputContainerSasUrl(storageAccountName, outputContainerName,cred,forRead);
        }
        private static string GetOutputContainerSasUrl(string storageAccountName, string outputContainerName, StorageSharedKeyCredential storageCreds, bool forRead)
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
        internal static async Task<bool> StorageContainerExists(string storageAccountName, string storageAccountKey, string containerName)
        {
            var container = GetBlobContainerClient(storageAccountName, storageAccountKey, containerName);
            var r = await container.ExistsAsync();
            return r.Value;

        }
        internal static async Task<bool> DeleteStorageContainer(string storageAccountName, string storageAccountKey, string containerName)
        {
            try
            {
                var container = GetBlobContainerClient(storageAccountName, storageAccountKey, containerName);
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
        internal static async Task<bool> UploadFilesToContainer(string storageAccountName, string storageAccountKey, string containerName, string[] filePaths, bool isRetry = false)
        {

            try
            {
                foreach (var filePath in filePaths)
                {
                    if (!isRetry)
                    {
                        log.LogInformation($"Uploading file {filePath} to container [{containerName}]...");
                    }
                    string blobName = Path.GetFileName(filePath);

                    var container = GetBlobContainerClient(storageAccountName, storageAccountKey, containerName);
                    await container.CreateIfNotExistsAsync();

                    var blobData = GetBlockBlobClient(storageAccountName, storageAccountKey, containerName, blobName);
                    using (var fs = new FileStream(filePath, FileMode.Open))
                    {
                        blobData.Upload(fs);
                    }
                    log.LogInformation($"File {filePath} uploaded to container [{containerName}]");
                }
                return true;
            }
            catch(Azure.RequestFailedException rfExe)
            {
                if (rfExe.ErrorCode == "ContainerBeingDeleted")
                {
                    log.LogInformation("Existing storage container is still being deleted. waiting...");
                    System.Threading.Thread.Sleep(3000);
                    return await UploadFilesToContainer(storageAccountName, storageAccountKey, containerName, filePaths, true); ;
                }
                else
                {
                    log.LogError($"Unable to upload files '{string.Join(',', filePaths)}' to container {containerName}: {rfExe.Message}");
                    return false;
                }
            }catch (Exception ex)
            {
                log.LogError($"Unable to upload files '{string.Join(',', filePaths)}' to container {containerName}: {ex.Message}");
                return false;
            }
        }
        internal static async Task<bool> DownloadBlobToLocal(string sasUrl, string localFileName)
        {
            try
            {
                var cloudBlobContainer = new BlobContainerClient(new Uri(sasUrl));
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
            catch (Exception ex)
            {
                log.LogError($"Unable to download file {Path.GetFileName(localFileName)} to {localFileName}: {ex.Message}");
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
        internal static bool CombineQueryOutputfiles(BlobServiceClient storageSvcClient, string storageContainerName, string outputFile)
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
            var container = GetBlobContainerClient(storageAccountName, storageAccountKey, outputContainerName);
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
                if(string.IsNullOrEmpty(taskId))
                {
                    taskId = Environment.GetEnvironmentVariable("HOSTNAME");
                }
                string machine = Environment.MachineName;

                foreach (var f in fileList)
                {
                    try
                    {
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
                        //if (bClient.Exists()) //Check for duplicates (in case of a previous crash, and create a new as needed
                        //{
                        //    tmp = tmp + DateTime.UtcNow.ToString("yyyy-MM-dd-HH-mm-ss");
                        //    bClient = containerClient.GetBlockBlobClient(tmp);
                        //}
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
            BlobContainerClient containerClient = null;
            if (string.IsNullOrWhiteSpace(storageAccountKey))
{
                var url = $"https://{storageAccountName}.blob.core.windows.net/{containerName}";
                containerClient = new BlobContainerClient(new Uri(url), Aad.AadHelper.TokenCredential);
            }
            else
            {
                var connstr = GetStorageConnectionString(storageAccountName, storageAccountKey);
                containerClient = new BlobContainerClient(connstr, containerName);
            }
            return containerClient;
        }
        private static BlockBlobClient GetBlockBlobClient(string storageAccountName, string storageAccountKey, string containerName, string blobName)
        {
            BlockBlobClient containerClient = null;
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

    }

}

