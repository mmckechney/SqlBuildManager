using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System.Text;
using SqlBuildManager.Interfaces.Console;
using System.Text.RegularExpressions;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Specialized;
using Azure.Storage.Blobs.Models;
using Azure.Storage.Sas;
using System.Threading.Tasks;
using Microsoft.Extensions.Azure;
using Azure.Storage;
using SqlBuildManager.Console.Threaded;
using SqlSync.Connection;
using Microsoft.Extensions.Logging;
namespace SqlBuildManager.Console.Batch
{
    public class Execution
    {
        readonly bool isDebug;
        private enum BatchType
        {
            Run,
            Query
        }
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private CommandLineArgs cmdLine;

        // Batch resource settings
        private string PoolName = "SqlBuildManagerPoolWindows";
        private const string JobIdFormat = "SqlBuildManagerJob{0}_{1}";

        private string queryFile = string.Empty;
        private string outputFile = string.Empty;
        private BatchType batchType = BatchType.Run;
        private const string baseTargetFormat = "target_{0}.cfg";

        public Execution(CommandLineArgs cmdLine)
        {
            this.cmdLine = cmdLine;

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchPoolName))
            {
                this.PoolName = cmdLine.BatchArgs.BatchPoolName;
            }

            isDebug = SqlBuildManager.Logging.ApplicationLogging.IsDebug();
        }
        public Execution(CommandLineArgs cmdLine, string queryFile, string outputFile) : this(cmdLine)
        {
            this.queryFile = queryFile;
            this.outputFile = outputFile;
            this.batchType = BatchType.Query;
        }

        public (int retval, string readOnlySas) StartBatch()
        {
            string applicationPackage = string.Empty;
            if (string.IsNullOrWhiteSpace(cmdLine.BatchArgs.ApplicationPackage))
            {
                switch (cmdLine.BatchArgs.BatchPoolOs)
                {
                    case OsType.Linux:
                        applicationPackage = "SqlBuildManagerLinux";
                        break;
                    case OsType.Windows:
                    default:
                        applicationPackage = "SqlBuildManagerWindows";
                        break;
                }
            }
            else
            {
                applicationPackage = cmdLine.BatchArgs.ApplicationPackage;
            }
            
            int cmdValid = ValidateBatchArgs(cmdLine, batchType);
            if(cmdValid != 0)
            {
                return (cmdValid, string.Empty);
            }


            //if extracting scripts from a platinum copy.. create the DACPAC here
            if(!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) && !string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource)) //using a platinum database as the source
            {
                log.LogInformation("$Extracting Platinum Dacpac from {cmdLine.DacPacArgs.PlatinumServerSource} : {cmdLine.DacPacArgs.PlatinumDbSource}");
                string dacpacName = Path.Combine(cmdLine.RootLoggingPath, cmdLine.DacPacArgs.PlatinumDbSource + ".dacpac");

                if (!DacPacHelper.ExtractDacPac(cmdLine.DacPacArgs.PlatinumDbSource, cmdLine.DacPacArgs.PlatinumServerSource, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, dacpacName))
                {
                    log.LogError($"Error creating the Platinum dacpac from {cmdLine.DacPacArgs.PlatinumServerSource} : {cmdLine.DacPacArgs.PlatinumDbSource}");
                }
                cmdLine.DacPacArgs.PlatinumDacpac = dacpacName;
            }

            //Check for the platinum dacpac and configure it if necessary
            log.LogInformation("Validating database overrides");
            MultiDbData buildData;
            int? myExitCode = 0;

            //Validate the override settings
            int tmpReturn = 0;
            string[] errorMessages;
            int tmpVal = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, cmdLine, out buildData, out errorMessages);
            if (tmpVal != 0)
            {
                log.LogError($"Unable to validate database config\r\n{string.Join("\r\n", errorMessages)}");
                return (tmpVal, string.Empty); 
            }

            //Validate the platinum dacpac
            var tmpValReturn = Validation.ValidateAndLoadPlatinumDacpac(ref cmdLine, ref buildData);
            if (tmpValReturn == (int)ExecutionReturn.DacpacDatabasesInSync)
            {
                return ((int)ExecutionReturn.DacpacDatabasesInSync, string.Empty);
            }
            else if (tmpReturn != 0)
            {
                return (tmpValReturn, string.Empty);
            }

            BatchClient batchClient = null;

            //Get the batch and storage values
            string jobId, poolId, storageContainerName;
            (jobId, poolId, storageContainerName) = SetBatchJobAndStorageNames(cmdLine);
            log.LogInformation($"Using Azure Batch account: {cmdLine.BatchArgs.BatchAccountName} ({cmdLine.BatchArgs.BatchAccountUrl})");
            log.LogInformation($"Setting job id to: {jobId}");

            string readOnlySasToken = string.Empty;
            try
            {

                log.LogInformation($"Batch job start: {DateTime.Now}");
                Stopwatch timer = new Stopwatch();
                timer.Start();

                //Get storage ready
                BlobServiceClient storageSvcClient = CreateStorageClient(cmdLine.BatchArgs.StorageAccountName, cmdLine.BatchArgs.StorageAccountKey);
                StorageSharedKeyCredential storageCreds = new StorageSharedKeyCredential(cmdLine.BatchArgs.StorageAccountName, cmdLine.BatchArgs.StorageAccountKey);
                string containerSasToken = GetOutputContainerSasUrl(cmdLine.BatchArgs.StorageAccountName, storageContainerName, storageCreds, false);
                log.LogDebug($"Output write SAS token: {containerSasToken}");


                // Get a Batch client using account creds, and create the pool
                BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(cmdLine.BatchArgs.BatchAccountUrl, cmdLine.BatchArgs.BatchAccountName, cmdLine.BatchArgs.BatchAccountKey);
                batchClient = BatchClient.Open(cred);

                // Create a Batch pool, VM configuration, Windows Server image
                bool success = CreateBatchPool(batchClient, poolId, cmdLine.BatchArgs.BatchNodeCount, cmdLine.BatchArgs.BatchVmSize, cmdLine.BatchArgs.BatchPoolOs);


                // The collection of data files that are to be processed by the tasks
                List<string> inputFilePaths = new List<string>();
                if (!string.IsNullOrEmpty(cmdLine.DacPacArgs.PlatinumDacpac))
                {
                    inputFilePaths.Add(cmdLine.DacPacArgs.PlatinumDacpac);
                }
                if (!string.IsNullOrEmpty(cmdLine.BuildFileName))
                {
                    inputFilePaths.Add(cmdLine.BuildFileName);
                }
                if (!string.IsNullOrEmpty(cmdLine.MultiDbRunConfigFileName))
                {
                    inputFilePaths.Add(cmdLine.MultiDbRunConfigFileName);
                }
                if (!string.IsNullOrEmpty(this.queryFile))
                {
                    inputFilePaths.Add(this.queryFile);
                }


                //Get the list of DB targets and distribute across batch count
                int valRet = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, null, out MultiDbData multiDb, out errorMessages);
                List<IEnumerable<(string, List<DatabaseOverride>)>> concurrencyBuckets = null;
                if (valRet == 0)
                {
                    if(cmdLine.ConcurrencyType == ConcurrencyType.Count)
                    {
                        //If it's just by count.. split evenly by the number of nodes
                        concurrencyBuckets = Concurrency.ConcurrencyByType(multiDb, cmdLine.BatchArgs.BatchNodeCount, cmdLine.ConcurrencyType);
                    }
                    else
                    {
                        //splitting by server is a little trickier, but run it and see what it does...
                        concurrencyBuckets = Concurrency.ConcurrencyByType(multiDb, cmdLine.Concurrency, cmdLine.ConcurrencyType);
                    }
                   
                    //If we end up with fewer splits, then reduce the node count...
                    if(concurrencyBuckets.Count() < cmdLine.BatchArgs.BatchNodeCount)
                    {
                        log.LogWarning($"NOTE! The number of targets ({concurrencyBuckets.Count()}) is less than the requested node count ({cmdLine.BatchArgs.BatchNodeCount}). Changing the pool node count to {concurrencyBuckets.Count()}");
                        cmdLine.BatchArgs.BatchNodeCount = concurrencyBuckets.Count();
                    }
                    else if(concurrencyBuckets.Count() > cmdLine.BatchArgs.BatchNodeCount) //need to do some consolidating
                    {
                        log.LogWarning($"NOTE! When splitting by {cmdLine.ConcurrencyType.ToString()}, the number of targets ({concurrencyBuckets.Count()}) is greater than the requested node count ({cmdLine.BatchArgs.BatchNodeCount}). Will consolidate to fit within the number of nodes");
                        concurrencyBuckets = Concurrency.RecombineServersToFixedBucketCount(multiDb, cmdLine.BatchArgs.BatchNodeCount);
                    }
                }
                else
                {
                    throw new ArgumentException($"Error parsing database targets. {String.Join(Environment.NewLine, errorMessages)}");
                }
                var splitTargets = Concurrency.ConvertBucketsToConfigLines(concurrencyBuckets);

                //Write out each split file
                string rootPath = Path.GetDirectoryName(cmdLine.MultiDbRunConfigFileName);
                for (int i = 0; i < splitTargets.Count; i++)
                {
                    var tmpName = Path.Combine(rootPath,string.Format(baseTargetFormat, i));
                    File.WriteAllLines(tmpName, splitTargets[i]);
                    inputFilePaths.Add(tmpName);
                }

                // Upload the data files to Azure Storage. This is the data that will be processed by each of the tasks that are
                // executed on the compute nodes within the pool.
                List<ResourceFile> inputFiles = new List<ResourceFile>();

                foreach (string filePath in inputFilePaths)
                {
                    inputFiles.Add(UploadFileToContainer(cmdLine.BatchArgs.StorageAccountName, storageContainerName,storageCreds, filePath));
                }

                //Create the individual command lines for each node
                IList<string> commandLines = CompileCommandLines(cmdLine, inputFiles, containerSasToken, cmdLine.BatchArgs.BatchNodeCount, jobId, cmdLine.BatchArgs.BatchPoolOs, applicationPackage, this.batchType);
                foreach (var s in commandLines)
                    log.LogDebug(s);

                try
                {
                    // Create a Batch job
                    log.LogInformation($"Creating job [{jobId}]...");
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
                        log.LogInformation($"The job {jobId} already existed when we tried to create it");
                    }
                    else
                    {
                        throw; // Any other exception is unexpected
                    }
                }

                // Create a collection to hold the tasks that we'll be adding to the job
                log.LogInformation($"Adding {splitTargets.Count} tasks to job [{jobId}]...");

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
                            new ApplicationPackageReference { ApplicationId = applicationPackage }
                        };
                    task.OutputFiles = new List<OutputFile>
                        {
                            new OutputFile(
                                filePattern: @"..\std*.txt",
                                destination: new OutputFileDestination(new OutputFileBlobContainerDestination(containerUrl: containerSasToken, path: taskId)),
                                uploadOptions: new OutputFileUploadOptions(uploadCondition: OutputFileUploadCondition.TaskCompletion))
                        };
                    tasks.Add(task);
                }

                // Add all tasks to the job.
                batchClient.JobOperations.AddTask(jobId, tasks);

                // Monitor task success/failure, specifying a maximum amount of time to wait for the tasks to complete.
                TimeSpan timeout = TimeSpan.FromMinutes(30);
                log.LogInformation($"Monitoring all tasks for 'Completed' state, timeout in {timeout}...");

                IEnumerable<CloudTask> addedTasks = batchClient.JobOperations.ListTasks(jobId);

                batchClient.Utilities.CreateTaskStateMonitor().WaitAll(addedTasks, TaskState.Completed, timeout);

                log.LogInformation("All tasks reached state Completed.");

                // Print task output
                log.LogInformation("Printing task output...\r\n");

                IEnumerable<CloudTask> completedtasks = batchClient.JobOperations.ListTasks(jobId);

                foreach (CloudTask task in completedtasks)
                {
                    string nodeId = String.Format(task.ComputeNodeInformation.ComputeNodeId);
                    log.LogInformation("---------------------------------");
                    log.LogInformation($"Task: {task.Id}");
                    log.LogInformation($"Node: {nodeId}");
                    log.LogInformation($"Exit Code: {task.ExecutionInformation.ExitCode}");
                    if (isDebug)
                    {
                        log.LogDebug("Standard out:");
                        log.LogDebug(task.GetNodeFile(Constants.StandardOutFileName).ReadAsString());
                    }
                    if (task.ExecutionInformation.ExitCode != 0)
                    {
                        myExitCode = task.ExecutionInformation.ExitCode;
                    }
                }
                log.LogInformation("---------------------------------");

                // Print out some timing info
                timer.Stop();
                log.LogInformation($"Batch job end: {DateTime.Now}");
                log.LogInformation($"Elapsed time: {timer.Elapsed}");


               // Clean up Batch resources
                if (cmdLine.BatchArgs.DeleteBatchJob)
                {
                    batchClient.JobOperations.DeleteJob(jobId);
                }
                if (cmdLine.BatchArgs.DeleteBatchPool)
                {
                    batchClient.PoolOperations.DeletePool(poolId);
                }

                log.LogInformation("Consolidating log files");
                ConsolidateLogFiles(storageSvcClient, storageContainerName, inputFilePaths);

                if(batchType == BatchType.Query)
                {
                    CombineBatchQueryOutputfiles(storageSvcClient, storageContainerName, this.outputFile);
                }

                //Finish the job out
                if (myExitCode == 0)
                {
                    log.LogInformation($"Setting job {jobId} status to Finished");
                    CloudJob j = batchClient.JobOperations.GetJob(jobId);
                    j.Terminate("Finished");
                }
                else
                {
                    log.LogInformation($"Setting job {jobId} status to exit code: {myExitCode}");
                    CloudJob j = batchClient.JobOperations.GetJob(jobId);
                    j.Terminate("Error");
                }                    


                readOnlySasToken = GetOutputContainerSasUrl(cmdLine.BatchArgs.StorageAccountName, storageContainerName, storageCreds, true);
                log.LogInformation($"Log files can be found here: {readOnlySasToken}");
                log.LogInformation("The read-only SAS token URL is valid for 7 days.");
                log.LogInformation("You can download \"Azure Storage Explorer\" from here: https://azure.microsoft.com/en-us/features/storage-explorer/");
                log.LogInformation("You can also get details on your Azure Batch execution from the \"Azure Batch Explorer\" found here: https://azure.github.io/BatchExplorer/");

            }
            catch(Exception exe)
            {
                log.LogError($"Exception when running batch job\r\n{exe.ToString()}");
                log.LogInformation($"Setting job {jobId} status to Failed");
                try
                {
                    CloudJob j = batchClient.JobOperations.GetJob(jobId);
                    j.Terminate("Failed");
                }
                catch { }
                myExitCode = 486;

            }
            finally
            {
                log.LogInformation("Batch complete");
                if (batchClient != null)
                {
                    batchClient.Dispose();
                }
            }

            if (myExitCode.HasValue)
            {

                log.LogInformation($"Exit Code: {myExitCode.Value}");
                return (myExitCode.Value, readOnlySasToken);
            }
            else
            {
                log.LogInformation($"Exit Code: {-100009}");
                return (-100009, readOnlySasToken);
            }

        }

        private void CombineBatchQueryOutputfiles(BlobServiceClient storageSvcClient, string storageContainerName, string outputFile)
        {
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

                            if(File.Exists(tmp))
                            {
                                File.Delete(tmp);
                            }
                        }
                        counter++;
                        //sourceBlob.Delete();
                    }
                }
            }
        }

        private int ValidateBatchArgs(CommandLineArgs cmdLine, BatchType batchType)
        {
            int tmpReturn;
            string[] errorMessages;
            if (batchType == BatchType.Run)
            {
                log.LogInformation("Validating general command parameters");

                tmpReturn = Validation.ValidateCommonCommandLineArgs(ref cmdLine, out errorMessages);
                if (tmpReturn != 0)
                {
                    foreach (var msg in errorMessages)
                    {
                        log.LogError(msg);
                    }
                    return tmpReturn;
                }
            }

            log.LogInformation("Validating batch command parameters");
            tmpReturn = Validation.ValidateBatchArguments(ref cmdLine, out errorMessages);
            if (tmpReturn != 0)
            {
                foreach (var msg in errorMessages)
                {
                    log.LogError(msg);
                }
                return tmpReturn;
            }

            return 0;
        }

        private (string jobId, string poolId, string storageContainerName) SetBatchJobAndStorageNames(CommandLineArgs cmdLine)
        {
            string jobId, poolId, storageContainerName;
            string jobToken = DateTime.Now.ToString("yyyy-MM-dd-HHmm-ss-fff");
            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchJobName))
            {
                cmdLine.BatchArgs.BatchJobName = Regex.Replace(cmdLine.BatchArgs.BatchJobName, "[^a-zA-Z0-9]", "");
                cmdLine.BatchArgs.BatchJobName = cmdLine.BatchArgs.BatchJobName.ToLower();
                if (cmdLine.BatchArgs.BatchJobName.Length > 47)
                {
                    cmdLine.BatchArgs.BatchJobName = cmdLine.BatchArgs.BatchJobName.Substring(0, 47);
                }
                jobId = cmdLine.BatchArgs.BatchJobName + "-" + jobToken;
                poolId = PoolName;
                storageContainerName = cmdLine.BatchArgs.BatchJobName;
            }
            else
            {
                jobId = string.Format(JobIdFormat, cmdLine.BatchArgs.BatchPoolOs, jobToken);
                poolId = PoolName;
                storageContainerName = jobToken;
            }

            return (jobId, poolId, storageContainerName);
        }

        private void ConsolidateLogFiles(BlobServiceClient storageSvcClient, string outputContainerName, List<string> workerFiles)
        {
            workerFiles.AddRange(new string[] { "dacpac", "sbm", "sql","execution.log", "csv"});
            var container = storageSvcClient.GetBlobContainerClient(outputContainerName);
            container.CreateIfNotExists();
            var blobs = container.GetBlobs();

            //Move worker files to "Working" directory
            foreach (var blob in blobs)
            {
                if (blob.Properties.BlobType == BlobType.Block )
                {
                    if (workerFiles.Any(a => blob.Name.ToLower().Contains(a)))
                    {
                        var sourceBlob = container.GetBlobClient(blob.Name);
                        var destBlob = container.GetBlobClient("Working/" + blob.Name);
                        using (var stream = sourceBlob.OpenRead())
                        {
                            destBlob.Upload(stream);
                        }
                        log.LogInformation($"Moved {blob.Name} to storage as Working/{blob.Name}");
                        sourceBlob.Delete();
                    }

                    foreach (string append in Program.AppendLogFiles)
                    {
                        if (blob.Name.ToLower() == append.ToLower())
                        {
                            var destinationBlob = container.GetAppendBlobClient(append);
                            try
                            {
                                destinationBlob.CreateIfNotExists();
                            }
                            catch { }

                            var sourceBlob = container.GetBlobClient(blob.Name);
                            using (var stream = sourceBlob.OpenRead())
                            {
                                destinationBlob.AppendBlock(stream);
                            }
                            log.LogInformation($"Consolidated {blob.Name} to {append}");
                            sourceBlob.Delete();
                        }
                    }
                }
            }

        }
        

        private bool CreateBatchPool(BatchClient batchClient, string poolId, int nodeCount, string vmSize, OsType os)
        {
            log.LogInformation($"Creating pool [{poolId}]...");

            ImageReference imageReference;
            VirtualMachineConfiguration virtualMachineConfiguration;
            switch (os)
            {
                
                case OsType.Linux:
                    imageReference = new ImageReference(publisher: "Canonical", offer: "UbuntuServer", sku: "18.04-lts", version: "latest");
                    virtualMachineConfiguration = new VirtualMachineConfiguration(imageReference: imageReference, nodeAgentSkuId: "batch.node.ubuntu 18.04");
                    break;

                case OsType.Windows:
                default:
                    imageReference = new ImageReference(publisher: "MicrosoftWindowsServer", offer: "WindowsServer", sku: "2016-Datacenter-with-containers", version: "latest");
                    virtualMachineConfiguration = new VirtualMachineConfiguration(imageReference: imageReference, nodeAgentSkuId: "batch.node.windows amd64");

                    break;
            }

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
                        log.LogInformation($"The pool {poolId} already existed when we tried to create it");
                        var pool = batchClient.PoolOperations.GetPool(poolId);
                        log.LogInformation($"Pre-existing node count {pool.CurrentDedicatedComputeNodes}");
                        if (pool.CurrentDedicatedComputeNodes != nodeCount)
                        {
                            log.LogWarning($"The pool {poolId} node count of {pool.CurrentDedicatedComputeNodes} does not match the requested node count of {nodeCount}");
                            if (pool.CurrentDedicatedComputeNodes < nodeCount)
                            {
                                log.LogWarning($"Requested node count is greater then existing node count. Resizing pool to {nodeCount}");
                                pool.Resize(targetDedicatedComputeNodes: nodeCount);
                            }
                            else
                            {
                                log.LogWarning("Existing node count is larger than requested node count. No pool changes bring made");
                            }
                        }
                    }
                    catch (Exception exe)
                    {
                        log.LogWarning($"Unable to get information on existing pool. {exe.ToString()}");
                        return false;
                    }
                }
                else
                {
                    log.LogError($"Received unexpected pool status: {be.RequestInformation?.BatchError.Code}");
                    log.LogError("Unable to proceed!");
                    throw; // Any other exception is unexpected
                }
            }
            //if (os == OsType.Linux)
            //{
           // string shellScript = $"$AZ_BATCH_APP_PACKAGE_{applicationPackage.ToLower()}/ install_sqlpackage.sh";
            //    CloudPool pool = batchClient.PoolOperations.GetPool(poolId);
            //    pool.StartTask.CommandLine =
            //}                    

            return true;
        }

        private BlobServiceClient CreateStorageClient(string storageAccountName, string storageAccountKey)
        {
            StorageSharedKeyCredential creds = new StorageSharedKeyCredential(storageAccountName, storageAccountKey);
            var serviceClient = new BlobServiceClient(new Uri($"https://{storageAccountName}.blob.core.windows.net"), creds);

            return serviceClient;
        }

        /// <summary>
        /// Builds commandlines for reach batch server based in the pool node count
        /// </summary>
        /// <param name="args"></param>
        /// <param name="cmdLine"></param>
        /// <param name="poolNodeCount"></param>
        /// <returns></returns>
        private IList<string> CompileCommandLines(CommandLineArgs cmdLine, List<ResourceFile> inputFiles,string containerSasToken,  int poolNodeCount,  string jobId, OsType os, string applicationPackage, BatchType bType)
        {
           // var z = inputFiles.Where(x => x.FilePath.ToLower().Contains(cmdLine.PackageName.ToLower())).FirstOrDefault();

            List<string> commandLines = new List<string>();
            //Need to replace the paths to 
            for (int i=0; i< poolNodeCount; i++)
            {
                var threadCmdLine = (CommandLineArgs)cmdLine.Clone(); 

                threadCmdLine.Action = CommandLineArgs.ActionType.Threaded; // set action to threaded (used only in classic command line)

                //Set package name to the path on the node (if set)
                if (!string.IsNullOrWhiteSpace(threadCmdLine.BuildFileName))
                {
                    var pkg = inputFiles.Where(x => x.FilePath.ToLower().Contains(Path.GetFileName(cmdLine.BuildFileName.ToLower()))).FirstOrDefault();
                    threadCmdLine.BuildFileName = pkg.FilePath;
                }

                //Set the DacPac name to the path on the node (if set)
                if (!string.IsNullOrWhiteSpace(threadCmdLine.DacPacArgs.PlatinumDacpac))
                {
                    var dac = inputFiles.Where(x => x.FilePath.ToLower().Contains(Path.GetFileName(cmdLine.DacPacArgs.PlatinumDacpac.ToLower()))).FirstOrDefault();
                    threadCmdLine.DacPacArgs.PlatinumDacpac = dac.FilePath;
                }

                //Set the queryfile name to the path on the node (if set)
                if(threadCmdLine.QueryFile != null && !string.IsNullOrWhiteSpace(threadCmdLine.QueryFile.Name))
                {
                    var qu = inputFiles.Where(x => x.FilePath.ToLower().Contains(Path.GetFileName(threadCmdLine.QueryFile.Name.ToLower()))).FirstOrDefault();
                    threadCmdLine.QueryFile = new FileInfo(qu.FilePath);
                }

                //Update the RootLoggingPath as appropriate
                switch (os)
                {
                    case OsType.Windows:
                        threadCmdLine.RootLoggingPath = string.Format("D:\\{0}", jobId);
                        break;

                    case OsType.Linux:
                        threadCmdLine.RootLoggingPath = "$AZ_BATCH_TASK_WORKING_DIR";
                        break;
                }
                


                //Set the name of the output file (if set)
                if (threadCmdLine.OutputFile != null)
                {
                    var tmpName = $"{threadCmdLine.RootLoggingPath}/{threadCmdLine.OutputFile.Name}{i}.csv"; //use forward slash for Linux compat.
                    threadCmdLine.OutputFile = new FileInfo(tmpName);
                }
             

                //Set the override file for this node.
                var tmp = string.Format(baseTargetFormat, i);
                var target = inputFiles.Where(x => x.FilePath.ToLower().Contains(tmp.ToLower())).FirstOrDefault();
                threadCmdLine.MultiDbRunConfigFileName = target.FilePath;

                //Set set the Sas URL
                threadCmdLine.BatchArgs.OutputContainerSasUrl = containerSasToken;

                StringBuilder sb = new StringBuilder();
                switch(os)
                {
                    case OsType.Windows:
                        sb.Append($"cmd /c set &&  %AZ_BATCH_APP_PACKAGE_{applicationPackage}%\\sbm.exe ");
                        sb.Append($"--loglevel {threadCmdLine.LogLevel} batch ");
                        switch (bType)
                        {
                            case BatchType.Run:
                                sb.Append(threadCmdLine.ToBatchThreadedExeString());
                                break;
                            case BatchType.Query:
                                sb.Append(threadCmdLine.ToBatchQueryThreadedExeString());
                                break;
                        }
                        break;

                    case OsType.Linux:
                        sb.Append($"/bin/sh -c 'printenv && $AZ_BATCH_APP_PACKAGE_{applicationPackage.ToLower()}/sbm ");
                        sb.Append($"--loglevel {threadCmdLine.LogLevel} batch ");
                        switch (bType)
                        {
                            case BatchType.Run:
                                sb.Append(threadCmdLine.ToBatchThreadedExeString() + "'");
                                break;
                            case BatchType.Query:
                                sb.Append(threadCmdLine.ToBatchQueryThreadedExeString() + "'");
                                break;
                        }
                        break;
                }
 
                commandLines.Add(sb.ToString());
            }

            return commandLines;
        }
        /// <summary>
        /// Uploads the specified file to the specified Blob container.
        /// </summary>
        /// <param name="storageSvcClient">A <see cref="CloudBlobClient"/>.</param>
        /// <param name="containerName">The name of the blob storage container to which the file should be uploaded.</param>
        /// <param name="filePath">The full path to the file to upload to Storage.</param>
        /// <returns>A ResourceFile instance representing the file within blob storage.</returns>
        private static ResourceFile UploadFileToContainer(string storageAcctName, string containerName, StorageSharedKeyCredential storageCreds,  string filePath)
        {
            log.LogInformation($"Uploading file {filePath} to container [{containerName}]...");

            string blobName = Path.GetFileName(filePath);

           // BlobContainerClient container = new BlobContainerClient(new Uri($"https://{storageAcctName}.blob.core.windows.net storage/{containerName}"), storageCreds);  //SvcClient.GetBlobContainerClient(containerName);
            BlockBlobClient blobData = new BlockBlobClient(new Uri($"https://{storageAcctName}.blob.core.windows.net/{containerName}/{blobName}"), storageCreds);   //container.GetBlockBlobClient(blobName);
            using (var fs = new FileStream(filePath,FileMode.Open))
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
                log.LogError(String.Format("Divided targets and execution server count do not match: {0} and {1} respectively", dividedDbTargets.Count().ToString(), batchNodeCount));
            }

            return splitLoad;
        }


        public int PreStageBatchNodes()
        {
            string[] errorMessages;
            log.LogInformation("Validating batch pre-stage command parameters");
            int tmpReturn = Validation.ValidateBatchPreStageArguments(ref cmdLine, out errorMessages);
            if (tmpReturn != 0)
            {
                foreach (var msg in errorMessages)
                {
                    log.LogError(msg);
                }
                return tmpReturn;
            }

            log.LogInformation("Creating Batch pool nodes ");

            // Get a Batch client using account creds, and create the pool
            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(cmdLine.BatchArgs.BatchAccountUrl, cmdLine.BatchArgs.BatchAccountName, cmdLine.BatchArgs.BatchAccountKey);
            var batchClient = BatchClient.Open(cred);

            // Create a Batch pool, VM configuration, Windows Server image
            bool success = CreateBatchPool(batchClient, PoolName, cmdLine.BatchArgs.BatchNodeCount, cmdLine.BatchArgs.BatchVmSize, cmdLine.BatchArgs.BatchPoolOs);

            if (cmdLine.BatchArgs.PollBatchPoolStatus)
            {
                log.LogInformation($"Waiting for pool {this.PoolName} to be created");
                while (true)
                {
                    var status = batchClient.PoolOperations.GetPool(PoolName, null, null);
                    if (status.AllocationState != AllocationState.Steady)
                    {
                        log.LogInformation($"Pool status: {status.AllocationState}");
                        System.Threading.Thread.Sleep(10000);
                    }
                    else
                    {
                        break;
                    }
                }

                log.LogInformation("Waiting for all nodes to complete creation");
                while (true)
                {
                    var status = batchClient.PoolOperations.GetPool(PoolName, null, null);
                    var nodes = status.ListComputeNodes(null, null);
                    if (nodes.Where(n => n.State != ComputeNodeState.Idle).Any())
                    {
                        if(isDebug)
                        {
                            nodes.ForEachAsync(n =>
                            {
                                log.LogDebug($"Node '{n.Id}' state = '{n.State}'");
                            });
                        }
                        else
                        {
                            var grp = nodes.GroupBy(n => n.State);
                            grp.ToList().ForEach(g =>
                            {
                                var cnt = g.First().State;
                                log.LogInformation($"State: {g.Count().ToString().PadLeft(2, '0')} nodes at {g.First().State}");
                            });
                        }

                        System.Threading.Thread.Sleep(15000);
                    }
                    else
                    {

                        nodes.ToList().ForEach(n =>
                        {
                            log.LogInformation($"Node '{n.Id}' state = '{n.State}'");
                        });
                        log.LogInformation("All nodes ready for work!");
                        break;
                    }
                }
            }
            else
            {
                log.LogInformation($"PollBatchPoolStatus set to 'false'. Pool is being created, but you will not get updates on the status. If you want to attach to pool to get status, you rerun the same command with /PollBatchPoolStatus=true at any time.");
            }
        
            if(success)
            {
                log.LogInformation($"Batch pool of {cmdLine.BatchArgs.BatchNodeCount} nodes created for account {cmdLine.BatchArgs.BatchAccountName} ");
                return 0;
            }
            else
            {
                log.LogError("There was a problem creating the Batch pool. Please see prior log messages");
                return -65643;
            }

        }

        public int CleanUpBatchNodes()
        {
            string[] errorMessages;
            log.LogInformation("Validating batch pre-stage command parameters");
            int tmpReturn = Validation.ValidateBatchCleanUpArguments(ref cmdLine, out errorMessages);
            if (tmpReturn != 0)
            {
                foreach (var msg in errorMessages)
                {
                    log.LogError(msg);
                }
                return tmpReturn;
            }

            log.LogInformation("Cleaning up (deleting) Batch pool nodes ");

            try
            {
                // Get a Batch client using account creds, and create the pool
                BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(cmdLine.BatchArgs.BatchAccountUrl, cmdLine.BatchArgs.BatchAccountName, cmdLine.BatchArgs.BatchAccountKey);
                var batchClient = BatchClient.Open(cred);


                log.LogInformation($"Deleting batch pool {this.PoolName} from Batch account {cmdLine.BatchArgs.BatchAccountName}");

                if (cmdLine.BatchArgs.PollBatchPoolStatus)
                {
                    //Delete the pool
                    batchClient.PoolOperations.DeletePool(this.PoolName);

                    if (cmdLine.BatchArgs.PollBatchPoolStatus)
                    {
                        var status = batchClient.PoolOperations.GetPool(PoolName, null, null);
                        var count = batchClient.PoolOperations.ListComputeNodes(PoolName, null, null).Count();
                        while (status != null && status.State == PoolState.Deleting && count > 0)
                        {
                            count = batchClient.PoolOperations.ListComputeNodes(PoolName, null, null).Count();
                            log.LogInformation($"Pool delete in progress. Current node count: {count}");
                            System.Threading.Thread.Sleep(15000);

                        }

                        log.LogInformation($"Pool {this.PoolName} successfully deleted");
                    }
                    return 0;
                }
                else
                {
                    log.LogInformation($"PollBatchPoolStatus set to 'false'. Pool is being delted, but you will not get updates on the status. If you want to attach to pool to get status, you rerun the same command with /PollBatchPoolStatus=true at any time.");
                    return 0;
                }

        }
            catch (Exception exe)
            {
                if (exe.Message.ToLower().IndexOf("notfound") > -1)
                {
                    log.LogInformation($"The {this.PoolName} pool was not found. Was it already deleted?");
                    return 0;
                }
                else
                {
                    log.LogError($"Error encountered trying to delete pool {this.PoolName} from Batch account {cmdLine.BatchArgs.BatchAccountName}.\r\n{exe.ToString()}");
                    return 42345346;
                }
            }
        }

    }
}