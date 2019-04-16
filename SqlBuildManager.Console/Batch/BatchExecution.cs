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
using SqlSync.SqlBuild.MultiDb;
using System.Text;

namespace SqlBuildManager.Console.Batch
{
    public class Execution
    {
        private static log4net.ILog log = log4net.LogManager.GetLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private string[] args;

        // Batch resource settings
        private const string PoolIdFormat = "SqlBuildManagerPool";
        private const string JobIdFormat = "SqlBuildManagerJob_{0}";

        private const string baseTargetFormat = "target_{0}.cfg";

        public Execution(string[] args)
        {
            this.args = args;
        }

        public int StartBatch()
        {

            string jobToken = DateTime.Now.ToString("yyyyMMddHHmm");
            string jobId = string.Format(JobIdFormat, jobToken);
            string poolId = PoolIdFormat;
            string inputContainerName = jobToken + "-input";
            string outputContainerName = jobToken + "-output";
            int? myExitCode = 0;


            log.Info("Validating general command parameters");
            CommandLineArgs cmdLine = CommandLine.ParseCommandLineArg(args);
            string[] errorMessages;
            int tmpReturn = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
            if (tmpReturn != 0)
            {
                foreach(var msg in errorMessages)
                {
                    log.Error(msg);
                }
                return tmpReturn;
            }

            log.Info("Validating batch command parameters");
            tmpReturn = Validation.ValidateBatchArguments(ref cmdLine, out errorMessages);
            if (tmpReturn != 0)
            {
                foreach (var msg in errorMessages)
                {
                    log.Error(msg);
                }
                return tmpReturn;
            }

            tmpReturn = BatchCommandValidation.ValidateBatchCommandLine(cmdLine);
            if (tmpReturn != 0)
            {
                return tmpReturn;
            }

            BatchClient batchClient = null;
            try
            {

                log.InfoFormat("Batch job start: {0}", DateTime.Now);
                Stopwatch timer = new Stopwatch();
                timer.Start();

                //Get storage ready
                CloudBlobClient blobClient = CreateStorageAndGetBlobClient(cmdLine.BatchArgs.StorageAccountName, cmdLine.BatchArgs.StorageAccountKey, inputContainerName);
                string containerSasToken = GetOutputContainerSasUrl(blobClient, outputContainerName, false);
                if (log.IsDebugEnabled)
                {
                    log.DebugFormat($"Output write SAS token: {containerSasToken}");
                }


                // Get a Batch client using account creds, and create the pool
                BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(cmdLine.BatchArgs.BatchAccountUrl, cmdLine.BatchArgs.BatchAccountName, cmdLine.BatchArgs.BatchAccountKey);
                batchClient = BatchClient.Open(cred);

                // Create a Batch pool, VM configuration, Windows Server image
                bool success = CreateBatchPool(batchClient, poolId, cmdLine.BatchArgs.BatchNodeCount, cmdLine.BatchArgs.BatchVmSize);


                // The collection of data files that are to be processed by the tasks
                List<string> inputFilePaths = new List<string>();
                if (!string.IsNullOrEmpty(cmdLine.DacPacArgs.PlatinumDacpac))
                {
                    inputFilePaths.Add(cmdLine.DacPacArgs.PlatinumDacpac);
                }
                if (!string.IsNullOrEmpty(cmdLine.PackageName))
                {
                    inputFilePaths.Add(cmdLine.PackageName);
                }
                if (!string.IsNullOrEmpty(cmdLine.MultiDbRunConfigFileName))
                {
                    inputFilePaths.Add(cmdLine.MultiDbRunConfigFileName);
                }

                //Get the list of DB targets and distribute across batch count
                var allTargets = GetTargetConfigValues(cmdLine.MultiDbRunConfigFileName);
                var splitTargets = SplitLoadEvenly(allTargets, cmdLine.BatchArgs.BatchNodeCount);
                if (splitTargets.Count < cmdLine.BatchArgs.BatchNodeCount)
                {
                    log.WarnFormat("NOTE! The number of targets ({0}) is less than the requested node count ({1}). Changing the pool node count to {0}", splitTargets.Count, cmdLine.BatchArgs.BatchNodeCount);
                    cmdLine.BatchArgs.BatchNodeCount = splitTargets.Count;
                }


                //Write out each split file
                string rootPath = Path.GetDirectoryName(cmdLine.MultiDbRunConfigFileName);
                for (int i = 0; i < splitTargets.Count; i++)
                {
                    var tmpName = rootPath + string.Format(baseTargetFormat, i);
                    File.WriteAllLines(tmpName, splitTargets[i]);
                    inputFilePaths.Add(tmpName);
                }

                // Upload the data files to Azure Storage. This is the data that will be processed by each of the tasks that are
                // executed on the compute nodes within the pool.
                List<ResourceFile> inputFiles = new List<ResourceFile>();

                foreach (string filePath in inputFilePaths)
                {
                    inputFiles.Add(UploadFileToContainer(blobClient, inputContainerName, filePath));
                }

                //Create the individual command lines for each node
                IList<string> commandLines = CompileCommandLines(args, cmdLine, inputFiles, containerSasToken, cmdLine.BatchArgs.BatchNodeCount, jobId);
                foreach (var s in commandLines)
                    log.Debug(s);

                try
                {
                    // Create a Batch job
                    log.InfoFormat("Creating job [{0}]...", jobId);
                    CloudJob job = batchClient.JobOperations.CreateJob();
                    job.Id = jobId;
                    job.PoolInformation = new PoolInformation { PoolId = poolId };
                    job.Commit();
                }
                catch (BatchException be)
                {
                    // Accept the specific error code JobExists as that is expected if the job already exists
                    if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.JobExists)
                    {
                        log.InfoFormat("The job {0} already existed when we tried to create it", jobId);
                    }
                    else
                    {
                        throw; // Any other exception is unexpected
                    }
                }

                // Create a collection to hold the tasks that we'll be adding to the job
                log.InfoFormat("Adding {0} tasks to job [{1}]...", splitTargets.Count, jobId);

                List<CloudTask> tasks = new List<CloudTask>();

                // Create each of the tasks to process on each node 
                for (int i = 0; i < commandLines.Count; i++)
                {
                    string taskId = String.Format($"Task{i}");
                    string taskCommandLine = commandLines[i];

                    CloudTask task = new CloudTask(taskId, taskCommandLine);
                    task.ResourceFiles = inputFiles;
                    task.ApplicationPackageReferences = new List<ApplicationPackageReference>
                        {
                            new ApplicationPackageReference { ApplicationId = "sqlbuildmanager" }
                        };
                    task.OutputFiles = new List<OutputFile>
                        {
                            new OutputFile(
                                filePattern: cmdLine.RootLoggingPath + @"\SqlBuildManager.Console.Execution.log",
                                destination: new OutputFileDestination(new OutputFileBlobContainerDestination(containerUrl: containerSasToken, path: taskId)),
                                uploadOptions: new OutputFileUploadOptions(uploadCondition: OutputFileUploadCondition.TaskCompletion))
                        };
                    tasks.Add(task);
                }

                // Add all tasks to the job.
                batchClient.JobOperations.AddTask(jobId, tasks);

                // Monitor task success/failure, specifying a maximum amount of time to wait for the tasks to complete.
                TimeSpan timeout = TimeSpan.FromMinutes(30);
                log.InfoFormat("Monitoring all tasks for 'Completed' state, timeout in {0}...", timeout);

                IEnumerable<CloudTask> addedTasks = batchClient.JobOperations.ListTasks(jobId);

                batchClient.Utilities.CreateTaskStateMonitor().WaitAll(addedTasks, TaskState.Completed, timeout);

                log.Info("All tasks reached state Completed.");

                // Print task output
                log.Info("Printing task output...\r\n");

                IEnumerable<CloudTask> completedtasks = batchClient.JobOperations.ListTasks(jobId);

                foreach (CloudTask task in completedtasks)
                {
                    string nodeId = String.Format(task.ComputeNodeInformation.ComputeNodeId);
                    log.Info("---------------------------------");
                    log.InfoFormat("Task: {0}", task.Id);
                    log.InfoFormat("Node: {0}", nodeId);
                    log.InfoFormat("Exit Code: {0}", task.ExecutionInformation.ExitCode);
                    if (log.IsDebugEnabled)
                    {
                        log.DebugFormat("Standard out:");
                        log.DebugFormat(task.GetNodeFile(Constants.StandardOutFileName).ReadAsString());
                    }
                    if (task.ExecutionInformation.ExitCode != 0)
                    {
                        myExitCode = task.ExecutionInformation.ExitCode;
                    }
                }
                log.Info("---------------------------------");

                // Print out some timing info
                timer.Stop();
                log.InfoFormat("Batch job end: {0}", DateTime.Now);
                log.InfoFormat("Elapsed time: {0}", timer.Elapsed);

                // Clean up Batch resources
                batchClient.JobOperations.DeleteJob(jobId);
                if (cmdLine.BatchArgs.DeleteBatchPool)
                {
                    batchClient.PoolOperations.DeletePool(poolId);
                }

                string readOnlySasToken = GetOutputContainerSasUrl(blobClient, outputContainerName, true);
                log.InfoFormat("Log files can be found here: {0}", readOnlySasToken);
                log.Info("The read-only SAS token URL is valid for 7 days.");
                log.Info("You can download \"Azure Storage Explorer\" from here: https://azure.microsoft.com/en-us/features/storage-explorer/");
                log.Info("You can also get details on your Azure Batch execution from the \"Azure Batch Explorer\" found here: https://azure.github.io/BatchExplorer/");

            }
            finally
            {
                log.InfoFormat("Batch complete");
                if (batchClient != null)
                {
                    batchClient.Dispose();
                }
            }

            if (myExitCode.HasValue)
            {
                log.InfoFormat("Exit Code: {0}", myExitCode.Value);
                return myExitCode.Value;
            }
            else
            {
                log.InfoFormat("Exit Code: {0}", -100009);
                return -100009;
            }

        }

        private bool CreateBatchPool(BatchClient batchClient, string poolId, int nodeCount, string vmSize)
        {
            log.InfoFormat("Creating pool [{0}]...", poolId);

            ImageReference imageReference = new ImageReference(publisher: "MicrosoftWindowsServer", offer: "WindowsServer",
                sku: "2016-Datacenter-with-containers", version: "latest");

            VirtualMachineConfiguration virtualMachineConfiguration = new VirtualMachineConfiguration(
                imageReference: imageReference, nodeAgentSkuId: "batch.node.windows amd64");

            try
            {
                CloudPool pool = batchClient.PoolOperations.CreatePool(
                    poolId: poolId, 
                    targetDedicatedComputeNodes: nodeCount,
                    virtualMachineSize: vmSize,
                    virtualMachineConfiguration: virtualMachineConfiguration);
                pool.Commit();
            }
            catch (BatchException be)
            {
                // Accept the specific error code PoolExists as that is expected if the pool already exists
                if (be.RequestInformation?.BatchError?.Code == BatchErrorCodeStrings.PoolExists)
                {
                    try
                    {
                        log.InfoFormat("The pool {0} already existed when we tried to create it", poolId);
                        var pool = batchClient.PoolOperations.GetPool(poolId);
                        log.InfoFormat("Pre-existing node count {0}", pool.CurrentDedicatedComputeNodes);
                        if (pool.CurrentDedicatedComputeNodes != nodeCount)
                        {
                            log.WarnFormat("The pool {0} node count of {1} does not match the requested node count of {2}", poolId, pool.CurrentDedicatedComputeNodes, nodeCount);
                            if (pool.CurrentDedicatedComputeNodes < nodeCount)
                            {
                                log.WarnFormat("Requested node count is greater then existing node count. Resizing pool to {0}", nodeCount);
                                pool.Resize(targetDedicatedComputeNodes: nodeCount);
                            }
                            else
                            {
                                log.Warn("Existing node count is larger than requested node count. No pool changes bring made");
                            }
                        }
                    }
                    catch (Exception exe)
                    {
                        log.Warn("Unable to get information on existing pool", exe);
                        return false;
                    }
                }
                else
                {
                    throw; // Any other exception is unexpected
                }
            }
            return true;
        }

        private CloudBlobClient CreateStorageAndGetBlobClient(string storageAccountName, string storageAccountKey, string inputContainerName)
        {
            // Construct the Storage account connection string
            string storageConnectionString = String.Format($"DefaultEndpointsProtocol=https;AccountName={storageAccountName};AccountKey={storageAccountKey}");

            // Retrieve the storage account
            CloudStorageAccount storageAccount = CloudStorageAccount.Parse(storageConnectionString);

            // Create the blob client, for use in obtaining references to blob storage containers
            CloudBlobClient blobClient = storageAccount.CreateCloudBlobClient();

            CloudBlobContainer container = blobClient.GetContainerReference(inputContainerName);
            container.CreateIfNotExists();

            return blobClient;
        }

        /// <summary>
        /// Builds commandlines for reach batch server based in the pool node count
        /// </summary>
        /// <param name="args"></param>
        /// <param name="cmdLine"></param>
        /// <param name="poolNodeCount"></param>
        /// <returns></returns>
        private IList<string> CompileCommandLines(string[] args, CommandLineArgs cmdLine, List<ResourceFile> inputFiles,string containerSasToken,  int poolNodeCount,  string jobId)
        {
            var z = inputFiles.Where(x => x.FilePath.ToLower().Contains(cmdLine.PackageName.ToLower())).FirstOrDefault();

            List<string> commandLines = new List<string>();
            //Need to replace the paths to 
            for (int i=0; i< poolNodeCount; i++)
            {
                StringBuilder sb = new StringBuilder("cmd /c %AZ_BATCH_APP_PACKAGE_SQLBUILDMANAGER%\\SqlBuildManager.Console.exe");
                foreach(var arg in args)
                {
                    if (arg.ToLower().Contains("/action"))
                    {
                        sb.Append(" /Action=Threaded");
                    }
                    else if (arg.ToLower().Contains("/packagename"))
                    {
                        var pkg = inputFiles.Where(x => x.FilePath.ToLower().Contains(Path.GetFileName(cmdLine.PackageName.ToLower()))).FirstOrDefault();
                        sb.Append(" /PackageName=\"" + pkg.FilePath + "\"");
                    }
                    else if (arg.ToLower().Contains("/platinumdacpac"))
                    {
                        var dac = inputFiles.Where(x => x.FilePath.ToLower().Contains(Path.GetFileName(cmdLine.DacPacArgs.PlatinumDacpac.ToLower()))).FirstOrDefault();
                        sb.Append(" /PlatinumDacpac=\"" + dac.FilePath + "\"");
                    }
                    else if (arg.ToLower().Contains("/rootloggingpath"))
                    {
                        //sb.Append(" /RootLoggingPath=\"D:\\runlogs\\\"");
                        //cmdLine.RootLoggingPath = @"D:\runlogs\";
                        sb.AppendFormat(" /RootLoggingPath=D:\\{0}", jobId);
                        cmdLine.RootLoggingPath = string.Format("D:\\{0}", jobId);
                    }
                    else if (arg.ToLower().Contains("/override"))
                    {
                        var tmp = string.Format(baseTargetFormat, i);
                        var tar = inputFiles.Where(x => x.FilePath.ToLower().Contains(tmp.ToLower())).FirstOrDefault();
                        sb.Append(" /Override=" + tar.FilePath);
                    }
                    else
                    {
                        sb.Append(" " + arg);
                    }
                }

                sb.Append(" /OutputContainerSasUrl=\"" + containerSasToken + "\"");
                commandLines.Add(sb.ToString());
            }

            return commandLines;
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

            return ResourceFile.FromUrl(blobSasUri, blobName, null);
        }
        private static string GetOutputContainerSasUrl(CloudBlobClient blobClient, string outputContainerName, bool forRead)
        {
            log.DebugFormat("Ensuring presence of output blob container '{0}'", outputContainerName);
            CloudBlobContainer container = blobClient.GetContainerReference(outputContainerName);
            container.CreateIfNotExists();
            SharedAccessBlobPolicy sasConstraints;
            if (!forRead)
            {
                sasConstraints = new SharedAccessBlobPolicy
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddHours(4),
                    Permissions = SharedAccessBlobPermissions.Add | SharedAccessBlobPermissions.Create | SharedAccessBlobPermissions.Write | SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List
                };
            }
            else
            {
                sasConstraints = new SharedAccessBlobPolicy
                {
                    SharedAccessExpiryTime = DateTime.UtcNow.AddDays(7),
                    Permissions =  SharedAccessBlobPermissions.Read | SharedAccessBlobPermissions.List
                };
            }
            string containerSasToken = container.GetSharedAccessSignature(sasConstraints);
            return String.Format("{0}{1}", container.Uri, containerSasToken);
        }
        private static bool CreateBatchPool(CommandLineArgs cmdLine)
        {
            return true;
        }
        /// <summary>
        /// Gets a string array for all of the target DB override settings
        /// </summary>
        /// <param name="multiDBFileName"></param>
        /// <returns></returns>
        private static string[] GetTargetConfigValues(string multiDBFileName)
        {
            MultiDbData multiDb;
            string[] errorMessages;
            int valRet = Validation.ValidateAndLoadMultiDbData(multiDBFileName, null, out multiDb, out errorMessages);
            string cfg = MultiDbHelper.ConvertMultiDbDataToTextConfig(multiDb);
            return cfg.Split(new char[] { '\r', '\n' }, StringSplitOptions.RemoveEmptyEntries);
        }
        /// <summary>
        /// Divde up the override targets amongst the nodes
        /// </summary>
        /// <param name="unifiedSettings"></param>
        /// <param name="batchNodeCount"></param>
        /// <returns></returns>
        internal IList<string[]> SplitLoadEvenly(string[] allTargetConfig, int batchNodeCount)
        {
            IEnumerable<string> allDbTargets = allTargetConfig.AsEnumerable();
            IEnumerable<IEnumerable<string>> dividedDbTargets = allDbTargets.SplitIntoChunks(batchNodeCount);
            List<string[]> splitLoad = new List<string[]>();
            if (dividedDbTargets.Count() <= batchNodeCount)
            {
                foreach (IEnumerable<string> targetList in dividedDbTargets)
                {
                    splitLoad.Add(targetList.ToArray());
                }
            }
            else
            {
                log.Error(String.Format("Divided targets and execution server count do not match: {0} and {1} respectively", dividedDbTargets.Count().ToString(), batchNodeCount));
            }

            return splitLoad;
        }




    }
}