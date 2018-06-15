using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SqlSync.SqlBuild;
namespace SqlBuildManager.Console.Batch
{
    public class Execution
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        // Update the Batch and Storage account credential strings below with the values unique to your accounts.
        // These are used when constructing connection strings for the Batch and Storage client objects.

        // Batch account credentials
        private const string BatchAccountName = "sqlbuildmanager";
        private const string BatchAccountKey = "tEtBZirucqRIRIWGzoiF5df6uAjK1ReFv8n0Wj0Ab4/CwzVT+xMwUzceGn8zXhKk165uzklzlj6cKRrFxqWIMg==";
        private const string BatchAccountUrl = "https://sqlbuildmanager.eastus.batch.azure.com";

        // Storage account credentials
        private const string StorageAccountName = "sqlbuildmanager";
        private const string StorageAccountKey = "H966woP+Uf7AYxCrDyGlJ27OzOedvWpgLWaAaV/gdOXHIi5qp0UQIReiRK44VzzsU4cvtvKTAwkIU6ldkebChg==";

        // Batch resource settings
        private const string PoolId = "DotNetQuickstartPool";
        private const string JobId = "DotNetQuickstartJob";
        private const int PoolNodeCount = 2;
        private const string PoolVMSize = "STANDARD_A1_v2";

       public void StartBatch(string[] args)
        {

            if (String.IsNullOrEmpty(BatchAccountName) ||
                String.IsNullOrEmpty(BatchAccountKey) ||
                String.IsNullOrEmpty(BatchAccountUrl) ||
                String.IsNullOrEmpty(StorageAccountName) ||
                String.IsNullOrEmpty(StorageAccountKey))
            {
                throw new InvalidOperationException("One or more account credential strings have not been populated. Please ensure that your Batch and Storage account credentials have been specified.");
            }

            try
            {

                log.InfoFormat("Sample start: {0}", DateTime.Now);
                Stopwatch timer = new Stopwatch();
                timer.Start();

                // Construct the Storage account connection string
                string storageConnectionString = String.Format("DefaultEndpointsProtocol=https;AccountName={0};AccountKey={1}",
                                                                StorageAccountName, StorageAccountKey);

                // Retrieve the storage account
                CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

                // Create the blob client, for use in obtaining references to blob storage containers
                CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

                // Use the blob client to create the input container in Azure Storage 
                const string inputContainerName = "input";

                CloudBlobContainer container = blobClient.GetContainerReference(inputContainerName);

                container.CreateIfNotExists();

                // The collection of data files that are to be processed by the tasks
                List<string> inputFilePaths = new List<string>
                {
                    @"F:\TempFiles\test1.txt",
                    @"F:\TempFiles\test2.txt",
                    @"F:\TempFiles\test3.txt"
                };

                // Upload the data files to Azure Storage. This is the data that will be processed by each of the tasks that are
                // executed on the compute nodes within the pool.
                List<ResourceFile> inputFiles = new List<ResourceFile>();

                foreach (string filePath in inputFilePaths)
                {
                    inputFiles.Add(UploadFileToContainer(blobClient, inputContainerName, filePath));
                }

                // Get a Batch client using account creds

                BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(BatchAccountUrl, BatchAccountName, BatchAccountKey);

                using (BatchClient batchClient = BatchClient.Open(cred))
                {
                    // Create a Batch pool, VM configuration, Windows Server image
                    log.InfoFormat("Creating pool [{0}]...", PoolId);

                    ImageReference imageReference = new ImageReference(
                        publisher: "MicrosoftWindowsServer",
                        offer: "WindowsServer",
                        sku: "2012-R2-datacenter-smalldisk",
                        version: "latest");

                    VirtualMachineConfiguration virtualMachineConfiguration =
                    new VirtualMachineConfiguration(
                        imageReference: imageReference,
                        nodeAgentSkuId: "batch.node.windows amd64");

                    try
                    {
                        CloudPool pool = batchClient.PoolOperations.CreatePool(
                            poolId: PoolId,
                            targetDedicatedComputeNodes: PoolNodeCount,
                            virtualMachineSize: PoolVMSize,
                            virtualMachineConfiguration: virtualMachineConfiguration);

                        pool.Commit();
                    }
                    catch (BatchException be)
                    {
                        // Accept the specific error code PoolExists as that is expected if the pool already exists
                        if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.PoolExists)
                        {
                            log.InfoFormat("The pool {0} already existed when we tried to create it", PoolId);
                        }
                        else
                        {
                            throw; // Any other exception is unexpected
                        }
                    }

                    // Create a Batch job
                    log.InfoFormat("Creating job [{0}]...", JobId);

                    try
                    {
                        CloudJob job = batchClient.JobOperations.CreateJob();
                        job.Id = JobId;
                        job.PoolInformation = new PoolInformation { PoolId = PoolId };

                        job.Commit();
                    }
                    catch (BatchException be)
                    {
                        // Accept the specific error code JobExists as that is expected if the job already exists
                        if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.JobExists)
                        {
                            log.InfoFormat("The job {0} already existed when we tried to create it", JobId);
                        }
                        else
                        {
                            throw; // Any other exception is unexpected
                        }
                    }

                    // Create a collection to hold the tasks that we'll be adding to the job

                    log.InfoFormat("Adding {0} tasks to job [{1}]...", inputFiles.Count, JobId);

                    List<CloudTask> tasks = new List<CloudTask>();

                    // Create each of the tasks to process one of the input files. 

                    for (int i = 0; i < inputFiles.Count; i++)
                    {
                        string taskId = String.Format("Task{0}", i);
                        string inputFilename = inputFiles[i].FilePath;
                        string taskCommandLine = String.Format("cmd /c type {0}", inputFilename);

                        CloudTask task = new CloudTask(taskId, taskCommandLine);
                        task.ResourceFiles = new List<ResourceFile> { inputFiles[i] };
                        tasks.Add(task);
                    }

                    // Add all tasks to the job.
                    batchClient.JobOperations.AddTask(JobId, tasks);


                    // Monitor task success/failure, specifying a maximum amount of time to wait for the tasks to complete.

                    TimeSpan timeout = TimeSpan.FromMinutes(30);
                    log.InfoFormat("Monitoring all tasks for 'Completed' state, timeout in {0}...", timeout);

                    IEnumerable<CloudTask> addedTasks = batchClient.JobOperations.ListTasks(JobId);

                    batchClient.Utilities.CreateTaskStateMonitor().WaitAll(addedTasks, TaskState.Completed, timeout);

                    log.Info("All tasks reached state Completed.");

                    // Print task output
                    log.Info("Printing task output...");

                    IEnumerable<CloudTask> completedtasks = batchClient.JobOperations.ListTasks(JobId);

                    foreach (CloudTask task in completedtasks)
                    {
                        string nodeId = String.Format(task.ComputeNodeInformation.ComputeNodeId);
                        log.InfoFormat("Task: {0}", task.Id);
                        log.InfoFormat("Node: {0}", nodeId);
                        log.InfoFormat("Standard out:");
                        log.InfoFormat(task.GetNodeFile(Constants.StandardOutFileName).ReadAsString());
                    }

                    // Print out some timing info
                    timer.Stop();
                    log.InfoFormat("Sample end: {0}", DateTime.Now);
                    log.InfoFormat("Elapsed time: {0}", timer.Elapsed);

                    // Clean up Storage resources
                    if (container.DeleteIfExists())
                    {
                        log.InfoFormat("Container [{0}] deleted.", inputContainerName);
                    }
                    else
                    {
                        log.InfoFormat("Container [{0}] does not exist, skipping deletion.", inputContainerName);
                    }

                    // Clean up Batch resources
                    batchClient.JobOperations.DeleteJob(JobId);
                    batchClient.PoolOperations.DeletePool(PoolId);
                }
            }
            finally
            {
                log.InfoFormat("Sample complete");
            }

        }


        /// <summary>
        /// Uploads the specified file to the specified Blob container.
        /// </summary>
        /// <param name="blobClient">A <see cref="CloudBlobClient"/>.</param>
        /// <param name="containerName">The name of the blob storage container to which the file should be uploaded.</param>
        /// <param name="filePath">The full path to the file to upload to Storage.</param>
        /// <returns>A ResourceFile instance representing the file within blob storage.</returns>
        private static ResourceFile UploadFileToContainer(CloudBlobClient blobClient, string containerName, string filePath)
        {
            log.InfoFormat("Uploading file {0} to container [{1}]...", filePath, containerName);

            string blobName = Path.GetFileName(filePath);

            CloudBlobContainer container = blobClient.GetContainerReference(containerName);
            CloudBlockBlob blobData = container.GetBlockBlobReference(blobName);
            blobData.UploadFromFile(filePath);

            // Set the expiry time and permissions for the blob shared access signature. In this case, no start time is specified,
            // so the shared access signature becomes valid immediately
            SharedAccessBlobPolicy sasConstraints = new SharedAccessBlobPolicy
            {
                SharedAccessExpiryTime = DateTime.UtcNow.AddHours(2),
                Permissions = SharedAccessBlobPermissions.Read
            };

            // Construct the SAS URL for blob
            string sasBlobToken = blobData.GetSharedAccessSignature(sasConstraints);
            string blobSasUri = String.Format("{0}{1}", blobData.Uri, sasBlobToken);

            return new ResourceFile(blobSasUri, blobName);
        }

        /// <summary>
        /// Divde up the targets amongst the nodes
        /// </summary>
        /// <param name="unifiedSettings"></param>
        /// <param name="batchNodeCount"></param>
        /// <returns></returns>
        internal IList<BuildSettings> SplitLoadEvenly(BuildSettings unifiedSettings, int batchNodeCount)
        {
            IList<BuildSettings> distributedConfig = new List<BuildSettings>();
            IEnumerable<string> allDbTargets = unifiedSettings.MultiDbTextConfig.AsEnumerable();
            IEnumerable<IEnumerable<string>> dividedDbTargets = allDbTargets.SplitIntoChunks(batchNodeCount);

            if (dividedDbTargets.Count() <= batchNodeCount)
            {
                int i = 0;
                foreach (IEnumerable<string> targetList in dividedDbTargets)
                {
                    BuildSettings tmpSetting = unifiedSettings.DeepClone<BuildSettings>();
                    tmpSetting.MultiDbTextConfig = targetList.ToArray();
                    distributedConfig.Add(tmpSetting);
                    i++;
                }
            }
            else
            {
                log.Error(String.Format("Divided targets and execution server count do not match: {0} and {1} respectively", dividedDbTargets.Count().ToString(), batchNodeCount));
            }

            return distributedConfig;
        }




    }
}