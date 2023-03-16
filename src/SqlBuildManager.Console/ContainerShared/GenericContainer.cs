using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Threaded;
using SqlBuildManager.Interfaces.Console;
using SqlSync.SqlBuild.MultiDb;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using sb = SqlSync.SqlBuild;

namespace SqlBuildManager.Console.ContainerShared
{
    internal class GenericContainer
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static async Task<int> GenericContainerWorker_RunQueueBuild(CommandLineArgs cmdLine)
        {
            int result = 1;
            var runId = ThreadedExecution.RunID;
            var threadLogger = new ThreadedLogging(cmdLine, runId);
            try
            {
                //If the provided build file name is the full path, just get the file name to find it in Blob storage
                cmdLine.BuildFileName = Path.GetFileName(cmdLine.BuildFileName);

                (string jobName, string throwaway) = CloudStorage.StorageManager.GetJobAndStorageNames(cmdLine);
                cmdLine.BatchJobName = jobName;

                bool keepGoing = true;
                cmdLine.BuildFileName = await CloudStorage.StorageManager.WriteFileToLocalStorage(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, cmdLine.BuildFileName);
                if (string.IsNullOrEmpty(cmdLine.BuildFileName))
                {
                    log.LogError("Unable to copy build package to local storage. Can not start execution");
                    keepGoing = false;
                }

                if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac))
                {
                    cmdLine.PlatinumDacpac = await CloudStorage.StorageManager.WriteFileToLocalStorage(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, cmdLine.DacPacArgs.PlatinumDacpac);
                    if (string.IsNullOrEmpty(cmdLine.DacPacArgs.PlatinumDacpac))
                    {
                        log.LogError("Unable to copy platinum dacpac package to local storage. Can not start execution");
                        keepGoing = false;
                    }
                }

                if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
                {
                    cmdLine.BatchArgs.OutputContainerSasUrl = CloudStorage.StorageManager.GetContainerRawUrl(cmdLine.ConnectionArgs.StorageAccountName, jobName);
                }
                else
                {
                    cmdLine.BatchArgs.OutputContainerSasUrl = CloudStorage.StorageManager.GetOutputContainerSasUrl(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, false);
                }


                if (keepGoing)
                {
                    result = Worker.RunThreadedExecution(cmdLine);
                }
            }
            catch (Exception exe)
            {
                log.LogError(exe.ToString());
                log.LogWarning("Error starting and running container processing");
                result = 2;
            }
            finally
            {
                threadLogger.WriteToLog(new LogMsg() { LogType = LogType.WorkerCompleted, RunId = runId, Message = System.Environment.MachineName });
            }
            
            return result;
        }
        internal static async Task<int> GenericContainerWorker_RunQueueQuery(CommandLineArgs cmdLine)
        {
            int result = 1;
           var runId = ThreadedExecution.RunID;
            var threadLogger = new ThreadedLogging(cmdLine, runId);

            try
            {

                (string jobName, string throwaway) = CloudStorage.StorageManager.GetJobAndStorageNames(cmdLine);
                cmdLine.BatchJobName = jobName;
                log.LogInformation($"Starting query job: '{jobName}'");
                log.LogInformation(cmdLine.ToString());
                log.LogInformation($"Using Query file '{cmdLine.QueryFile?.Name}'");

                bool keepGoing = true;
                log.LogInformation($"Writing query file '{cmdLine.QueryFile.Name}' to local storage");
                cmdLine.QueryFile = new FileInfo(await CloudStorage.StorageManager.WriteFileToLocalStorage(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, cmdLine.QueryFile.Name));
                if (cmdLine.QueryFile == null)
                {
                    log.LogError("Unable to save query file to local storage. Can not start execution");
                    keepGoing = false;
                }
                
                if (keepGoing)
                {
                    
                    
                    result = new ThreadedQuery().QueryDatabases(cmdLine, runId);
                    var outputFileName = runId + "_" + cmdLine.OutputFile.Name;
                    
                    //Copy file to storage with unique prefix
                    log.LogInformation($"Copying local results output file '{outputFileName}' to blob storage container '{jobName}");
                    StorageManager.CopyFileToStorage(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, jobName, cmdLine.OutputFile.FullName, outputFileName);
                    log.LogInformation($"Exiting query job: '{jobName}'");
                    
                }
                else
                {
                    log.LogInformation($"Exiting failed query job: '{jobName}'");
                }
               
            }
            catch (Exception exe)
            {
                log.LogError(exe.ToString());
                log.LogWarning("Error starting and running container processing");
                result = 2;
            }
            finally
            {
                threadLogger.WriteToLog(new LogMsg() { LogType = LogType.WorkerCompleted, RunId = runId, Message = System.Environment.MachineName });
            }
              return result;
        }

        internal static async Task<int> PrepAndUploadContainerBuildPackage(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac, bool force)
        {
            var success = false;
            (success, cmdLine) = Worker.Init(cmdLine);
            if (packageName != null)
            {
                cmdLine.BuildFileName = packageName.FullName;
            }
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountName))
            {
                log.LogError("--storageaccountname is required");
                return -1;
            }
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey) && string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ClientId))
            {
                log.LogError("--storageaccountkey is required if a Managed Identity is not included");
                return -1;
            }

            (bool retVal, string sbmName) = await ValidateAndUploadContainerBuildFilesToStorage(cmdLine, packageName, platinumDacpac, force);
            if (retVal)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
        internal static async Task<(bool, string)> ValidateAndUploadContainerBuildFilesToStorage(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac, bool force)
        {
            if (packageName == null && platinumDacpac == null)
            {
                log.LogError("Either a --packagename or --platinumdacpac argument is required");
                return (false, "");
            }

            bool sbmGenerated = false;
            //Need to build the SBM package 
            if (platinumDacpac != null && packageName == null)
            {
                if (string.IsNullOrEmpty(cmdLine.MultiDbRunConfigFileName))
                {
                    log.LogError("When a --platinumdacpac argument is specified without a --packagename, then an --override value is required so that the SBM package can be generated");
                    return (false, "");
                }

                var multiData = MultiDbHelper.ImportMultiDbTextConfig(cmdLine.MultiDbRunConfigFileName);
                if (multiData == null)
                {
                    log.LogError($"Unable to derive database targets from specified --override setting of '{cmdLine.MultiDbRunConfigFileName}' . Please check that the file exists and is properly formatted.");
                    return (false, "");
                }
                string sbmName;
                cmdLine.PlatinumDacpac = platinumDacpac.FullName;
                var stat = Worker.GetSbmFromDacPac(cmdLine, multiData, out sbmName, true);
                if (stat == sb.DacpacDeltasStatus.Success)
                {
                    if (Path.GetFileNameWithoutExtension(sbmName) != Path.GetFileNameWithoutExtension(platinumDacpac.FullName))
                    {
                        var newSbmName = Path.Combine(Path.GetDirectoryName(platinumDacpac.FullName), Path.GetFileNameWithoutExtension(platinumDacpac.FullName) + ".sbm");
                        File.Copy(sbmName, newSbmName, true);
                        packageName = new FileInfo(newSbmName);
                    }
                    else
                    {
                        packageName = new FileInfo(sbmName);
                    }
                    sbmGenerated = true;
                }
                else
                {
                    log.LogError("Unable to create an SBM package from the specified --platinumdacpac and --override settings. Please check their values.");
                    return (false, "");
                }
            }

            if (StorageManager.StorageContainerHasExistingFiles(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName))
            {
                if (!force)
                {
                    System.Console.Write($"The container {cmdLine.JobName} already exists in storage account {cmdLine.ConnectionArgs.StorageAccountName}. Do you want to delete any existing files and continue upload? (Y/n)");
                    var key = System.Console.ReadKey().Key;
                    System.Console.WriteLine();
                    if (key == ConsoleKey.Y)
                    {
                        force = true;
                    }
                    else
                    {
                        System.Console.WriteLine("Exiting. The package file was not uploaded and no files were deleted from storage");
                        return (true, "");
                    }
                }
                if (force)
                {
                    if (!await StorageManager.DeleteStorageContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName))
                    {
                        log.LogError("Unable to delete container. The package file was not uploaded");
                        return (false, "");
                    }
                }
            }
            List<string> filePaths = new List<string>();
            if (packageName != null) filePaths.Add(packageName.FullName);
            if (platinumDacpac != null) filePaths.Add(platinumDacpac.FullName);

            if (!await StorageManager.UploadFilesToStorageContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, filePaths.ToArray()))
            {
                log.LogError("Unable to upload files to storage");
                return (false, "");
            }

            if (sbmGenerated)
            {
                log.LogInformation($"An SBM Package file was generated and uploaded with the DACPAC. When running the `deploy` command, please use this argument: --packagename \"{packageName.FullName}\"");
                return (true, packageName.FullName);
            }

            return (true, "");
        }
        internal static async Task<(bool, string)> ValidateAndUploadContainerQueryToStorage(CommandLineArgs cmdLine, bool force)
        {
            if (cmdLine.QueryFile == null)
            {
                log.LogError("The  --queryfileargument is required");
                return (false, "");
            }


            if (StorageManager.StorageContainerHasExistingFiles(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName))
            {
                if (!force)
                {
                    System.Console.Write($"The container {cmdLine.JobName} already exists in storage account {cmdLine.ConnectionArgs.StorageAccountName}. Do you want to delete any existing files and continue upload? (Y/n)");
                    var key = System.Console.ReadKey().Key;
                    System.Console.WriteLine();
                    if (key == ConsoleKey.Y)
                    {
                        force = true;
                    }
                    else
                    {
                        System.Console.WriteLine("Exiting. The package file was not uploaded and no files were deleted from storage");
                        return (true, "");
                    }
                }
                if (force)
                {
                    if (!await StorageManager.DeleteStorageContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName))
                    {
                        log.LogError("Unable to delete container. The package file was not uploaded");
                        return (false, "");
                    }
                }
            }
            List<string> filePaths = new List<string>();
            filePaths.Add(cmdLine.QueryFile.FullName);

            if (!await StorageManager.UploadFilesToStorageContainer(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey, cmdLine.JobName, filePaths.ToArray()))
            {
                log.LogError("Unable to upload files to storage");
                return (false, "");
            }

            return (true, "");
        }
    }
}
