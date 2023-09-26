using Azure.Core;
using Azure.ResourceManager;
using arb = Azure.ResourceManager.Batch;
using Azure.ResourceManager.Batch.Models;
using Azure.ResourceManager.Models;
using Microsoft.Azure.Batch;
using Microsoft.Azure.Batch.Auth;
using Microsoft.Azure.Batch.Common;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Interfaces.Console;
using SqlSync.Connection;
using SqlSync.SqlBuild;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Azure.ResourceManager.Batch;

namespace SqlBuildManager.Console.Batch
{
    public class BatchManager
    {
        readonly bool isDebug;
        public enum BatchType
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

        public BatchManager(CommandLineArgs cmdLine)
        {
            this.cmdLine = cmdLine;

            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchPoolName))
            {
                PoolName = cmdLine.BatchArgs.BatchPoolName;
            }

            isDebug = SqlBuildManager.Logging.ApplicationLogging.IsDebug();
        }
        public BatchManager(CommandLineArgs cmdLine, string queryFile, string outputFile) : this(cmdLine)
        {
            this.queryFile = queryFile;
            this.outputFile = outputFile;
            batchType = BatchType.Query;
        }

        public async Task<(int retval, string readOnlySas)> StartBatch(bool stream = false, bool unittest = false)
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
            if (cmdValid != 0)
            {
                return (cmdValid, string.Empty);
            }


            //if extracting scripts from a platinum copy.. create the DACPAC here
            if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDbSource) && !string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumServerSource)) //using a platinum database as the source
            {
                log.LogInformation($"Extracting Platinum Dacpac from {cmdLine.DacPacArgs.PlatinumServerSource} : {cmdLine.DacPacArgs.PlatinumDbSource}");
                string dacpacName = Path.Combine(cmdLine.RootLoggingPath, cmdLine.DacPacArgs.PlatinumDbSource + ".dacpac");

                if (!DacPacHelper.ExtractDacPac(cmdLine.DacPacArgs.PlatinumDbSource, cmdLine.DacPacArgs.PlatinumServerSource, cmdLine.AuthenticationArgs.AuthenticationType, cmdLine.AuthenticationArgs.UserName, cmdLine.AuthenticationArgs.Password, dacpacName, cmdLine.TimeoutRetryCount, cmdLine.IdentityArgs.ClientId))
                {
                    log.LogError($"Error creating the Platinum dacpac from {cmdLine.DacPacArgs.PlatinumServerSource} : {cmdLine.DacPacArgs.PlatinumDbSource}");
                }
                cmdLine.DacPacArgs.PlatinumDacpac = dacpacName;
            }

            //Check for the platinum dacpac and configure it if necessary
            log.LogInformation("Validating database overrides");
            MultiDbData multiData;
            int? myExitCode = 0;

            //TODO: fix this for queue!!!!
            //Validate the override settings (not needed if --servicebusconnection is provided
            string[] errorMessages;
            int tmpVal = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, cmdLine, out multiData, out errorMessages);
            if (tmpVal != 0)
            {
                log.LogError($"Unable to validate database config\r\n{string.Join("\r\n", errorMessages)}");
                return (tmpVal, string.Empty);
            }


            //Validate the platinum dacpac
            var tmpValReturn = Validation.ValidateAndLoadPlatinumDacpac(cmdLine, multiData);
            if (tmpValReturn.Item1 == (int)ExecutionReturn.DacpacDatabasesInSync)
            {
                return ((int)ExecutionReturn.DacpacDatabasesInSync, string.Empty);
            }
            else if (tmpValReturn.Item1 != 0)
            {
                return (tmpValReturn.Item1, string.Empty);
            }

            BatchClient batchClient = null;

            //Get the batch and storage values
            string jobId, poolId, storageContainerName;
            (jobId, poolId, storageContainerName) = SetBatchJobAndStorageNames(cmdLine);
            log.LogInformation($"Using Azure Batch account: {cmdLine.ConnectionArgs.BatchAccountName} ({cmdLine.ConnectionArgs.BatchAccountUrl})");
            log.LogInformation($"Setting job id to: {jobId}");

            //Use the storage container name as the job name if using an override config (vs. queue)
            if (!string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName) && string.IsNullOrWhiteSpace(cmdLine.JobName))
            {
                cmdLine.JobName = storageContainerName;
            }

            string readOnlySasToken = string.Empty;
            try
            {

                log.LogInformation($"Batch job start: {DateTime.Now}");
                Stopwatch timer = new Stopwatch();
                timer.Start();

                //Get storage ready
                var storageSvcClient = StorageManager.CreateStorageClient(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey);
                var storageCreds = StorageManager.GetStorageSharedKeyCredential(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey);
                string containerSasToken = StorageManager.GetOutputContainerSasUrl(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, storageContainerName, false);
                log.LogDebug($"Output write SAS token: {containerSasToken}");


                // Get a Batch client using account creds, and create the pool
                BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(cmdLine.ConnectionArgs.BatchAccountUrl, cmdLine.ConnectionArgs.BatchAccountName, cmdLine.ConnectionArgs.BatchAccountKey);
                batchClient = BatchClient.Open(cred);

                // Create a Batch pool, VM configuration, Windows Server image
                bool success = await CreateBatchPool(cmdLine, poolId);
                if (!success)
                {
                    log.LogError("Unable to create Batch node pool. Can not continue processing");
                    return (-324, "");
                }


                // The collection of data files that are to be processed by the tasks
                List<string> inputFilePaths = new List<string>();
                if (!string.IsNullOrEmpty(cmdLine.DacPacArgs.PlatinumDacpac))
                {
                    inputFilePaths.Add(cmdLine.DacPacArgs.PlatinumDacpac);
                }
                if (!string.IsNullOrEmpty(cmdLine.DacPacArgs.TargetDacpac))
                {
                    inputFilePaths.Add(cmdLine.DacPacArgs.TargetDacpac);
                }
                if (!string.IsNullOrEmpty(cmdLine.BuildFileName))
                {
                    inputFilePaths.Add(cmdLine.BuildFileName);
                }
                if (!string.IsNullOrEmpty(cmdLine.MultiDbRunConfigFileName))
                {
                    inputFilePaths.Add(cmdLine.MultiDbRunConfigFileName);
                }
                if (!string.IsNullOrEmpty(queryFile))
                {
                    inputFilePaths.Add(queryFile);
                }


                //Get the list of DB targets and distribute across batch count
                var splitTargets = new List<string[]>();
                if (string.IsNullOrEmpty(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
                {
                    int valRet = Validation.ValidateAndLoadMultiDbData(cmdLine.MultiDbRunConfigFileName, null, out MultiDbData multiDb, out errorMessages);
                    List<IEnumerable<(string, List<DatabaseOverride>)>> concurrencyBuckets = null;
                    if (valRet == 0)
                    {
                        if (cmdLine.ConcurrencyType == ConcurrencyType.Count)
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
                        if (concurrencyBuckets.Count() < cmdLine.BatchArgs.BatchNodeCount)
                        {
                            log.LogWarning($"NOTE! The number of targets ({concurrencyBuckets.Count()}) is less than the requested node count ({cmdLine.BatchArgs.BatchNodeCount}). Changing the pool node count to {concurrencyBuckets.Count()}");
                            cmdLine.BatchArgs.BatchNodeCount = concurrencyBuckets.Count();
                        }
                        else if (concurrencyBuckets.Count() > cmdLine.BatchArgs.BatchNodeCount) //need to do some consolidating
                        {
                            log.LogWarning($"NOTE! When splitting by {cmdLine.ConcurrencyType.ToString()}, the number of targets ({concurrencyBuckets.Count()}) is greater than the requested node count ({cmdLine.BatchArgs.BatchNodeCount}). Will consolidate to fit within the number of nodes");
                            concurrencyBuckets = Concurrency.RecombineServersToFixedBucketCount(multiDb, cmdLine.BatchArgs.BatchNodeCount);
                        }
                    }
                    else
                    {
                        throw new ArgumentException($"Error parsing database targets. {String.Join(Environment.NewLine, errorMessages)}");
                    }
                    splitTargets = Concurrency.ConvertBucketsToConfigLines(concurrencyBuckets);

                    //Write out each split file
                    string rootPath = Path.GetDirectoryName(cmdLine.MultiDbRunConfigFileName);
                    for (int i = 0; i < splitTargets.Count; i++)
                    {
                        var tmpName = Path.Combine(rootPath, string.Format(baseTargetFormat, i));
                        File.WriteAllLines(tmpName, splitTargets[i]);
                        inputFilePaths.Add(tmpName);
                    }
                }

                // Upload the data files to Azure Storage. This is the data that will be processed by each of the tasks that are
                // executed on the compute nodes within the pool.
                List<ResourceFile> inputFiles = new List<ResourceFile>();

                foreach (string filePath in inputFilePaths)
                {
                    inputFiles.Add(StorageManager.UploadFileToBatchContainer(cmdLine.ConnectionArgs.StorageAccountName, storageContainerName, storageCreds, filePath));
                }

                //Create the individual command lines for each node
                IList<string> commandLines = CompileCommandLines(cmdLine, inputFiles, containerSasToken, cmdLine.BatchArgs.BatchNodeCount, jobId, cmdLine.BatchArgs.BatchPoolOs, applicationPackage, batchType);
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
                if (splitTargets.Count != 0)
                {
                    log.LogInformation($"Adding {splitTargets.Count} tasks to job [{jobId}]...");
                }
                else if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
                {
                    log.LogInformation($"Adding tasks to job [{jobId}]...");
                }

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
                                filePattern: @"../std*.txt",
                                destination: new OutputFileDestination(new OutputFileBlobContainerDestination(containerUrl: containerSasToken, path: taskId)),
                                uploadOptions: new OutputFileUploadOptions(uploadCondition: OutputFileUploadCondition.TaskCompletion)),
                             new OutputFile(
                                filePattern: @"../*.csv",
                                destination: new OutputFileDestination(new OutputFileBlobContainerDestination(containerUrl: containerSasToken, path: taskId)),
                                uploadOptions: new OutputFileUploadOptions(uploadCondition: OutputFileUploadCondition.TaskCompletion)),
                             new OutputFile(
                                filePattern: @"../*.cfg",
                                destination: new OutputFileDestination(new OutputFileBlobContainerDestination(containerUrl: containerSasToken, path: taskId)),
                                uploadOptions: new OutputFileUploadOptions(uploadCondition: OutputFileUploadCondition.TaskCompletion)),
                             new OutputFile(
                                filePattern: @"../*.log",
                                destination: new OutputFileDestination(new OutputFileBlobContainerDestination(containerUrl: containerSasToken, path: taskId)),
                                uploadOptions: new OutputFileUploadOptions(uploadCondition: OutputFileUploadCondition.TaskCompletion))
                        };

                    tasks.Add(task);
                }

                // Add all tasks to the job.
                batchClient.JobOperations.AddTask(jobId, tasks);

                // Monitor task success/failure, specifying a maximum amount of time to wait for the tasks to complete.
                TimeSpan timeout = TimeSpan.FromMinutes(cmdLine.BatchArgs.JobMonitorTimeout);
                log.LogInformation($"Monitoring all tasks for 'Completed' state, timeout in {timeout}...");

                if (BatchProcessStartedEvent != null)
                {
                    BatchProcessStartedEvent(this, new BatchMonitorEventArgs(cmdLine, stream, unittest));
                }

                IEnumerable<CloudTask> addedTasks = batchClient.JobOperations.ListTasks(jobId);

                batchClient.Utilities.CreateTaskStateMonitor().WaitAll(addedTasks, TaskState.Completed, timeout);

                if (BatchExecutionCompletedEvent != null)
                {
                    BatchExecutionCompletedEvent(this, new BatchMonitorEventArgs(cmdLine, stream, unittest));
                }

                log.LogInformation("All tasks reached state Completed.");

                // Print task output
                log.LogInformation("Printing task output...");

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

                SqlBuildManager.Logging.Threaded.Configure.CloseAndFlushAllLoggers();

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

                log.LogInformation("Consolidating log files");
                StorageManager.ConsolidateLogFiles(storageSvcClient, storageContainerName, inputFilePaths);

                if (batchType == BatchType.Query)
                {
                    StorageManager.CombineQueryOutputfiles(storageSvcClient, storageContainerName, outputFile);
                }

                

                readOnlySasToken = StorageManager.GetOutputContainerSasUrl(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, storageContainerName, true);
                log.LogInformation($"The consolidated log files can be found in the Azure storage account '{cmdLine.ConnectionArgs.StorageAccountName}' in blob container '{storageContainerName}'");
                log.LogInformation("You can download \"Azure Storage Explorer\" from here: https://azure.microsoft.com/en-us/features/storage-explorer/");
                log.LogInformation("You can also get details on your Azure Batch execution from the \"Azure Batch Explorer\" found here: https://azure.github.io/BatchExplorer/");

            }
            catch (Exception exe)
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



        private int ValidateBatchArgs(CommandLineArgs cmdLine, BatchType batchType)
        {
            int tmpReturn;
            string[] errorMessages;
            if (batchType == BatchType.Run)
            {
                log.LogInformation("Validating general command parameters");

                tmpReturn = Validation.ValidateCommonCommandLineArgs(cmdLine, out errorMessages);
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
            tmpReturn = Validation.ValidateBatchArguments(cmdLine, out errorMessages);
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
                if (cmdLine.BatchArgs.BatchJobName.Length < 3 || cmdLine.BatchArgs.BatchJobName.Length > 41 || !Regex.IsMatch(cmdLine.BatchArgs.BatchJobName, @"^[a-z0-9]+(-[a-z0-9]+)*$"))
                {
                    throw new ArgumentException("The batch job name must be  lower case, between 3 and 41 characters in length, and the only special character allowed are dashes '-'");
                }

                jobId = cmdLine.BatchArgs.BatchJobName;
                poolId = PoolName;
                storageContainerName = cmdLine.BatchArgs.BatchJobName; // + "-" + jobToken; ;
            }
            else
            {
                jobId = string.Format(JobIdFormat, cmdLine.BatchArgs.BatchPoolOs, jobToken);
                poolId = PoolName;
                storageContainerName = jobToken;
            }

            return (jobId, poolId, storageContainerName);
        }



        public async Task<bool> CreateBatchPool(CommandLineArgs cmdLine, string poolId)
        {

            var batchAccountName = cmdLine.ConnectionArgs.BatchAccountName;
            var batchAccountKey = cmdLine.ConnectionArgs.BatchAccountKey.EncodeBase64();
            var batchUrl = cmdLine.ConnectionArgs.BatchAccountUrl;

            var nodeCount = cmdLine.BatchArgs.BatchNodeCount;
            var vmSize = cmdLine.BatchArgs.BatchVmSize;
            var os = cmdLine.BatchArgs.BatchPoolOs;

            var userAssignedResourceId = cmdLine.IdentityArgs.ResourceId;
            var userAssignedPrinId = cmdLine.IdentityArgs.PrincipalId;
            var userAssignedClientId = cmdLine.IdentityArgs.ClientId;
            string resourceGroupName;
            if (!string.IsNullOrWhiteSpace(cmdLine.BatchArgs.ResourceGroup))
            {
                resourceGroupName = cmdLine.BatchArgs.ResourceGroup;
            }
            else if (!string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ResourceGroup))
            {
                resourceGroupName = cmdLine.IdentityArgs.ResourceGroup;
            }
            else
            {
                throw new ArgumentException("Resource group must be specified either in for the Batch account or the Identity");
            }
            var subscriptionId = cmdLine.IdentityArgs.SubscriptionId;
            if (cmdLine.IdentityArgs != null) AadHelper.ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId;
            if (cmdLine.IdentityArgs != null) AadHelper.TenantId = cmdLine.IdentityArgs.TenantId;


            log.LogInformation($"Creating pool [{poolId}]...");


            // get your azure access token, for more details of how Azure SDK get your access token, please refer to https://learn.microsoft.com/en-us/dotnet/azure/sdk/authentication?tabs=command-line
            ArmClient client = new ArmClient(AadHelper.TokenCredential);

            ResourceIdentifier batchAccountResourceId = arb.BatchAccountResource.CreateResourceIdentifier(subscriptionId, resourceGroupName, batchAccountName);
            arb.BatchAccountResource batchAccount = client.GetBatchAccountResource(batchAccountResourceId);

            // get the collection of this BatchAccountPoolResource
            arb.BatchAccountPoolCollection collection = batchAccount.GetBatchAccountPools();

            // invoke the operation
            
            arb.BatchAccountPoolData data = new arb.BatchAccountPoolData();
         
            data.Identity = new ManagedServiceIdentity("UserAssigned")
            {
                UserAssignedIdentities = { [new ResourceIdentifier(cmdLine.IdentityArgs.ResourceId)] = new Azure.ResourceManager.Models.UserAssignedIdentity() }
            };

            BatchImageReference imageReference;
            BatchVmConfiguration virtualMachineConfiguration;
            switch (os)
            {
                case OsType.Linux:
                    imageReference = new BatchImageReference() { Publisher = "microsoft-azure-batch", Offer = "ubuntu-server-container", Sku = "20-04-lts", Version = "latest" };
                    virtualMachineConfiguration = new BatchVmConfiguration(imageReference: imageReference, nodeAgentSkuId: "batch.node.ubuntu 20.04");
                    break;

                case OsType.Windows:
                default:
                    imageReference = new BatchImageReference { Publisher = "MicrosoftWindowsServer", Offer = "WindowsServer", Sku = "2022-datacenter", Version = "latest" };
                    virtualMachineConfiguration = new BatchVmConfiguration(imageReference: imageReference, nodeAgentSkuId: "batch.node.windows amd64");

                    break;
            }
            data.DeploymentConfiguration = new BatchDeploymentConfiguration() { VmConfiguration = virtualMachineConfiguration };
            data.ScaleSettings = new BatchAccountPoolScaleSettings() { FixedScale = new BatchAccountFixedScaleSettings() { TargetDedicatedNodes = nodeCount } };
            data.VmSize = vmSize;

            if (!string.IsNullOrWhiteSpace(cmdLine.NetworkArgs.SubnetName) && !string.IsNullOrWhiteSpace(cmdLine.NetworkArgs.VnetName))
            {
                var vnetRg = string.IsNullOrWhiteSpace(cmdLine.NetworkArgs.ResourceGroup) ? resourceGroupName : cmdLine.NetworkArgs.ResourceGroup;
                var networkConfig = new BatchNetworkConfiguration()
                {
                    SubnetId =  new ResourceIdentifier($"/subscriptions/{subscriptionId}/resourceGroups/{vnetRg}/providers/Microsoft.Network/virtualNetworks/{cmdLine.NetworkArgs.VnetName}/subnets/{cmdLine.NetworkArgs.SubnetName}")
                };
                data.NetworkConfiguration = networkConfig;
            }


            try
            {

                ArmOperation<BatchAccountPoolResource> lro = await collection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, poolId, data);
                BatchAccountPoolResource result = lro.Value;
                BatchAccountPoolData resourceData = result.Data;
                log.LogInformation($"Successfully created {os} pool {poolId} with {nodeCount} nodes");
               
            }
            catch(Azure.RequestFailedException rfe)
            {
                if(rfe.ErrorCode == "PropertyCannotBeUpdated")
                {
                    try
                    {
                        
                        log.LogInformation($"The pool {poolId} already existed when we tried to create it");
                        var poolresult = await collection.GetAsync(poolId);
                        var currentNodeCount = poolresult.Value.Data.ScaleSettings.FixedScale.TargetDedicatedNodes;
                        log.LogInformation($"Pre-existing node count {currentNodeCount}");
                        if (currentNodeCount != nodeCount)
                        {
                            log.LogWarning($"The pool {poolId} node count of {currentNodeCount} does not match the requested node count of {nodeCount}");
                            if (currentNodeCount < nodeCount)
                            {
                                log.LogWarning($"Requested node count is greater then existing node count. Resizing pool to {nodeCount}");
                                var scaleSettings = new BatchAccountPoolScaleSettings() { FixedScale = new BatchAccountFixedScaleSettings() { TargetDedicatedNodes = nodeCount } };
                                poolresult.Value.Data.ScaleSettings = scaleSettings;

                                ArmOperation<BatchAccountPoolResource> lro = await collection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, poolId, poolresult.Value.Data);
                                BatchAccountPoolResource result = lro.Value;
                                BatchAccountPoolData resourceData = result.Data;
                                log.LogInformation($"Successfully created {os} pool {poolId} with {nodeCount} nodes");

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
                }else
                {
                    log.LogError(rfe, $"The pool {poolId} failed to create. Unexpected error");
                    return false;
                }
            }
            catch(Exception exe)
            {
                log.LogError(exe, $"The pool {poolId} failed to create. Unexpected error");
                return false;
            }
            return true;

        }



        /// <summary>
        /// Builds commandlines for reach batch server based in the pool node count
        /// </summary>
        /// <param name="args"></param>
        /// <param name="cmdLine"></param>
        /// <param name="poolNodeCount"></param>
        /// <returns></returns>
        public IList<string> CompileCommandLines(CommandLineArgs cmdLine, List<ResourceFile> inputFiles, string containerSasToken, int poolNodeCount, string jobId, OsType os, string applicationPackage, BatchType bType)
        {
            // var z = inputFiles.Where(x => x.FilePath.ToLower().Contains(cmdLine.PackageName.ToLower())).FirstOrDefault();

            List<string> commandLines = new List<string>();
            //Need to replace the paths to 
            for (int i = 0; i < poolNodeCount; i++)
            {
                var threadCmdLine = (CommandLineArgs)cmdLine.Clone();

                //Set package name to the path on the node (if set)
                if (!string.IsNullOrWhiteSpace(threadCmdLine.BuildFileName))
                {
                    var pkg = inputFiles.Where(x => x.FilePath.ToLower().Contains(Path.GetFileName(cmdLine.BuildFileName.ToLower()))).FirstOrDefault();
                    threadCmdLine.BuildFileName = pkg.FilePath;
                }

                //Set the Platinum DacPac name to the path on the node (if set)
                if (!string.IsNullOrWhiteSpace(threadCmdLine.DacPacArgs.PlatinumDacpac))
                {
                    var dac = inputFiles.Where(x => x.FilePath.ToLower().Contains(Path.GetFileName(cmdLine.DacPacArgs.PlatinumDacpac.ToLower()))).FirstOrDefault();
                    threadCmdLine.DacPacArgs.PlatinumDacpac = dac.FilePath;
                }

                //Set the target DacPac name to the path on the node (if set)
                if (!string.IsNullOrWhiteSpace(threadCmdLine.DacPacArgs.TargetDacpac))
                {
                    var tdac = inputFiles.Where(x => x.FilePath.ToLower().Contains(Path.GetFileName(cmdLine.DacPacArgs.TargetDacpac.ToLower()))).FirstOrDefault();
                    threadCmdLine.DacPacArgs.TargetDacpac = tdac.FilePath;
                }

                //Set the queryfile name to the path on the node (if set)
                if (threadCmdLine.QueryFile != null && !string.IsNullOrWhiteSpace(threadCmdLine.QueryFile.Name))
                {
                    var qu = inputFiles.Where(x => x.FilePath.ToLower().Contains(Path.GetFileName(threadCmdLine.QueryFile.Name.ToLower()))).FirstOrDefault();
                    threadCmdLine.QueryFile = new FileInfo(qu.FilePath);
                }

                //Update the RootLoggingPath as appropriate
                switch (os)
                {
                    case OsType.Windows:
                        threadCmdLine.RootLoggingPath = "%AZ_BATCH_TASK_DIR%";// string.Format("D:\\{0}", jobId);
                        break;

                    case OsType.Linux:
                        threadCmdLine.RootLoggingPath = "$AZ_BATCH_TASK_DIR";
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
                if (target != null)
                {
                    threadCmdLine.MultiDbRunConfigFileName = target.FilePath;
                }

                //Set set the Sas URL
                threadCmdLine.BatchArgs.OutputContainerSasUrl = containerSasToken;

                StringBuilder sb = new StringBuilder();
                switch (os)
                {
                    case OsType.Windows:
                        sb.Append($"cmd /c set &&  %AZ_BATCH_APP_PACKAGE_{applicationPackage}%\\sbm.exe ");
                        break;
                    case OsType.Linux:
                        sb.Append($"/bin/sh -c 'printenv && $AZ_BATCH_APP_PACKAGE_{applicationPackage.ToLower()}/sbm ");
                        break;
                }
                sb.Append($"--loglevel {threadCmdLine.LogLevel} batch ");

                switch (bType)
                {
                    case BatchType.Run:
                        sb.Append("runthreaded ");
                        break;
                    case BatchType.Query:
                        sb.Append("querythreaded ");
                        break;
                }
                switch (os)
                {
                    case OsType.Windows:
                        sb.Append(threadCmdLine.ToBatchString());
                        break;
                    case OsType.Linux:
                        sb.Append(threadCmdLine.ToBatchString() + "'");
                        break;
                }



                commandLines.Add(sb.ToString());
            }

            return commandLines;
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


        public async Task<int> PreStageBatchNodes()
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
            BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(cmdLine.ConnectionArgs.BatchAccountUrl, cmdLine.ConnectionArgs.BatchAccountName, cmdLine.ConnectionArgs.BatchAccountKey);
            var batchClient = BatchClient.Open(cred);

            // Create a Batch pool, VM configuration, Windows Server image
            // bool success = CreateBatchPoolLegacy(batchClient, poolId, cmdLine.BatchArgs.BatchNodeCount, cmdLine.BatchArgs.BatchVmSize, cmdLine.BatchArgs.BatchPoolOs);
            bool success = CreateBatchPool(cmdLine, PoolName).GetAwaiter().GetResult();
            if (!success)
            {
                log.LogError("Unable to create Batch node pool. Cannot continue processing");
                return (-324);
            }
            if (cmdLine.BatchArgs.PollBatchPoolStatus)
            {
                log.LogInformation($"Waiting for pool {PoolName} to be created");
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
                        if (isDebug)
                        {
                            await nodes.ForEachAsync(n =>
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
                log.LogInformation($"--pollbatchpoolstatus flag set to 'false'. Pool is being created, but you will not get updates on the status. If you want to attach to pool to get status, you rerun the same command with '--pollbatchpoolstatus true' at any time.");
            }

            if (success)
            {
                log.LogInformation($"Batch pool of {cmdLine.BatchArgs.BatchNodeCount} nodes created for account {cmdLine.ConnectionArgs.BatchAccountName} ");
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
            bool isPolling = false;
            try
            {
                // Get a Batch client using account creds, and create the pool
                BatchSharedKeyCredentials cred = new BatchSharedKeyCredentials(cmdLine.ConnectionArgs.BatchAccountUrl, cmdLine.ConnectionArgs.BatchAccountName, cmdLine.ConnectionArgs.BatchAccountKey);
                var batchClient = BatchClient.Open(cred);


                log.LogInformation($"Deleting batch pool {PoolName} from Batch account {cmdLine.ConnectionArgs.BatchAccountName}");

                //Delete the pool
                batchClient.PoolOperations.DeletePool(PoolName);

                if (cmdLine.BatchArgs.PollBatchPoolStatus)
                {

                    var status = batchClient.PoolOperations.GetPool(PoolName, null, null);
                    var count = batchClient.PoolOperations.ListComputeNodes(PoolName, null, null).Count();
                    while (status != null && status.State == PoolState.Deleting && count > 0)
                    {
                        isPolling = true;
                        count = batchClient.PoolOperations.ListComputeNodes(PoolName, null, null).Count();
                        log.LogInformation($"Pool delete in progress. Current node count: {count}");
                        System.Threading.Thread.Sleep(15000);

                    }
                    log.LogInformation($"Pool {PoolName} successfully deleted");
                    return 0;
                }
                else
                {
                    log.LogInformation($"--pollbatchpoolstatus flag set to 'false'. Pool is being deleted, but you will not get updates on the status. If you want to attach to pool to get status, you rerun the same command with '--pollbatchpoolstatus true' at any time.");
                    return 0;
                }

            }
            catch (Exception exe)
            {
                if (exe.Message.ToLower().IndexOf("notfound") > -1)
                {
                    if (isPolling)
                    {
                        log.LogInformation($"Pool {PoolName} successfully deleted");
                    }
                    else
                    {
                        log.LogInformation($"The {PoolName} pool was not found. Was it already deleted?");
                    }
                    return 0;
                }
                else
                {
                    log.LogError($"Error encountered trying to delete pool {PoolName} from Batch account {cmdLine.ConnectionArgs.BatchAccountName}.{Environment.NewLine}{exe.ToString()}");
                    return 42345346;
                }
            }
        }

        public event BatchMonitorEventHandler BatchProcessStartedEvent;
        public event BatchMonitorEventHandler BatchExecutionCompletedEvent;

    }

    

}