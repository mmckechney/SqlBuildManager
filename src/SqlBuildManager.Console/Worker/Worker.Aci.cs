using Microsoft.Azure.Amqp.Framing;
using Microsoft.Azure.Management.Batch.Models;
using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ContainerApp.Internal;
using SqlBuildManager.Console.KeyVault;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using SqlBuildManager.Console.ContainerShared;
namespace SqlBuildManager.Console
{
    internal partial class Worker
    {
        internal static int SaveAndEncryptAciSettings(CommandLineArgs cmdLine, bool clearText)
        {
            cmdLine.BatchArgs = null;
            cmdLine.KubernetesArgs = null;
            cmdLine.ContainerAppArgs = null;
            cmdLine.ConnectionArgs.BatchAccountKey = null;
            cmdLine.ConnectionArgs.BatchAccountName = null;
            cmdLine.ConnectionArgs.BatchAccountUrl = null;
            cmdLine.IdentityArgs.PrincipalId = null;
            cmdLine.IdentityArgs.ResourceId = null;
            

            return SaveAndEncryptSettings(cmdLine, clearText);
        }

        internal static async Task<int> AciRun(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac, bool unittest, bool stream, bool monitor, bool force)
        {
            if (packageName != null)
            {
                cmdLine.BuildFileName = packageName.FullName;
            }
            if (platinumDacpac != null)
            {
                cmdLine.PlatinumDacpac = platinumDacpac.FullName;
            }
            (var x, cmdLine) = Init(cmdLine);
            var res = await AciPrepAndUploadBuildPackage(cmdLine, packageName, platinumDacpac, force);
            if (res != 0)
            {
                log.LogError("Failed to upload build package to Blob storage");
                return 1;
            }

            return await AciApplyForRunOrQuery(cmdLine, unittest, stream, monitor);

        }

        internal static async Task<int> AciApplyForRunOrQuery(CommandLineArgs cmdLine, bool unittest, bool stream, bool monitor)
        {
            var res = await EnqueueOverrideTargets(cmdLine);
            if (res != 0)
            {
                log.LogError("Failed to enqueue override targets");
                return 1;
            }

            res = await AciDeploy(cmdLine, monitor, unittest, stream);
            if (res != 0)
            {
                log.LogError("Failed to deploy or run ACI app");
                log.LogInformation("Cleaning up any remaining queue messages...");
                await DeQueueOverrideTargets(cmdLine);
                return res;
            }
            else
            {
                return res;
            }
        }
        internal static async Task<int> AciPrepAndUploadBuildPackage(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac, bool force)
        {
            Init(cmdLine);
            if (packageName != null) cmdLine.BuildFileName = packageName.FullName;
            var valErrors = Validation.ValidateAciAppArgs(cmdLine);
            if (valErrors.Count > 0)
            {
                valErrors.ForEach(m => log.LogError(m));
                return 1;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                bool kvSuccess;
                (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
                if (!kvSuccess)
                {
                    log.LogError("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                    return -4;
                }
            }
            (bool na, cmdLine) = Cryptography.DecryptSensitiveFields(cmdLine);

            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountName) && string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                log.LogError("If --keyvaultname is not provided as an argument or in the --settingsfile, then --storageaccountname is required");
                return -1;
            }
            (bool retVal, string sbmName) = await GenericContainer.ValidateAndUploadContainerBuildFilesToStorage(cmdLine, packageName, platinumDacpac, force);
            if (!retVal)
            {
                return -1;
            }
            if (!string.IsNullOrWhiteSpace(sbmName))
            {
                cmdLine.BuildFileName = sbmName;
            }


            return 0;
        }

        internal static async Task<int> AciDeploy(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac, bool monitor, bool unittest = false, bool stream = false)
        {
            (var success, cmdLine) = Init(cmdLine);
            if (packageName != null)
            {
                cmdLine.BuildFileName = packageName.FullName;
            }
            if (platinumDacpac != null)
            {
                cmdLine.PlatinumDacpac = platinumDacpac.FullName;
            }
            return await AciDeploy(cmdLine, monitor, unittest, stream);
        }
        internal static async Task<int> AciDeploy(CommandLineArgs cmdLine, bool monitor, bool unittest = false, bool stream = false)
        {
            (var success, cmdLine) = Init(cmdLine);
            if (monitor)
            {
                bool kvSuccess;
                (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
                if (!kvSuccess)
                {
                    log.LogError("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                    return -4;
                }
            }

            if (string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.SubscriptionId))
            {
                log.LogError("The value for --subscriptionid is required as a parameter or inclusion in the --settingsfile");
            }

            var utcMonitorStart = DateTime.UtcNow;
            success = await Aci.AciManager.DeployAci(cmdLine);
            if (success && monitor)
            {
                return await AciMonitorRuntimeProgress(cmdLine, utcMonitorStart, unittest, stream);
            }
            else if (success) return 0; else return 1;
        }
        internal static async Task<int> AciMonitorRuntimeProgress(CommandLineArgs cmdLine, DateTime? utcMonitorStart, bool unitest, bool stream = false)
        {
            if (string.IsNullOrWhiteSpace(cmdLine.JobName))
            {
                log.LogError("A --jobname value is required.");
                return 1;
            }

            var retVal = await MonitorServiceBusRuntimeProgress(cmdLine, stream, utcMonitorStart, unitest, true);
            ConsolidateRuntimeLogFiles(cmdLine);

            return retVal;
        }

        private static bool aciIsInErrorState = false;
        private static async Task AciGetErrorState(CommandLineArgs cmdLine)
        {
            while (true)
            {
                try
                {
                    var stat = await Aci.AciManager.AciIsInErrorState(cmdLine.IdentityArgs.SubscriptionId, cmdLine.AciArgs.ResourceGroup, cmdLine.AciArgs.AciName);
                    aciIsInErrorState = stat;
                }
                catch { }
                System.Threading.Thread.Sleep(15000);
            }
        }

        internal static async Task<int> AciQuery(CommandLineArgs cmdLine, FileInfo @override, bool force, bool monitor, bool stream, bool unittest)
        {
            if(@override != null)
            {
                cmdLine.Override = @override.FullName;
            }
            (int success, cmdLine) = PrepForRemoteQueryExecution(cmdLine);
            if (success != 0)
            {
                return success;
            }

            log.LogDebug("Entering ACI Query Execution");
            log.LogInformation("Running ACI Query Execution...");

            //Upload 'prep'
            (var retVal, string tmp) = await GenericContainer.ValidateAndUploadContainerQueryToStorage(cmdLine, force);
            if (!retVal)
            {
                log.LogError("There was a problem uploading the query file to Azure storage");
                return -1;
            }



            success = await AciApplyForRunOrQuery(cmdLine, unittest, stream, monitor);

            var storageSvcClient = StorageManager.CreateStorageClient(cmdLine.ConnectionArgs.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountKey);
            var saved = StorageManager.CombineQueryOutputfiles(storageSvcClient, cmdLine.JobName, cmdLine.OutputFile.FullName);

            if (saved)
            {
                var readOnlySas = cmdLine.BatchArgs.OutputContainerSasUrl;
                try
                {
                    if (await StorageManager.DownloadBlobToLocal(storageSvcClient, cmdLine.JobName, cmdLine.OutputFile.FullName))
                    {
                        log.LogInformation($"Output file copied locally to {cmdLine.OutputFile.FullName}");
                    }
                }
                catch (Exception exe)
                {
                    log.LogError($"Unable to download the output file:  {exe.Message}");
                }
            }
            if (saved && success == 0)
            {
                return 0;
            }
            else
            {
                return success;
            }

        }


        #region Container Execution Methods
        internal static async Task<int> AciWorker_RunQueueQuery(CommandLineArgs cmdLine)
        {
            (bool success, cmdLine) = AciWorker_PrepCommandLine(cmdLine);
            if (!success)
            {
                return -1;
            }
            return await GenericContainer.GenericContainerWorker_RunQueueQuery(cmdLine);
        }

        internal static async Task<int> AciWorker_RunQueueBuild(CommandLineArgs cmdLine)
        {
            (bool success, cmdLine) = AciWorker_PrepCommandLine(cmdLine);
            if (!success)
            {
                return -1;
            }
            return await GenericContainer.GenericContainerWorker_RunQueueBuild(cmdLine);
        }
        private static (bool, CommandLineArgs) AciWorker_PrepCommandLine(CommandLineArgs cmdLine)
        {
            SqlBuildManager.Logging.ApplicationLogging.SetLogLevel(cmdLine.LogLevel);
            cmdLine.RootLoggingPath = Path.Combine(Directory.GetCurrentDirectory(), "logs");
            if (!Directory.Exists(cmdLine.RootLoggingPath))
            {
                Directory.CreateDirectory(cmdLine.RootLoggingPath);
            }
            cmdLine.RunningAsContainer = true;



            int seconds = 5;
            log.LogInformation($"Waiting {seconds} for Managed Identity assignment");
            System.Threading.Thread.Sleep(seconds * 1000);

            cmdLine = ContainerShared.EnvironmentVariableHelper.ReadRuntimeEnvironmentVariables(cmdLine);

            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            if (!kvSuccess)
            {
                log.LogError("Unable to retrieve required connection secrets. Terminating container");
                return (false, cmdLine);
            }
            //Re-read (mostly for unit tests in case there is a connection string from the KV
            cmdLine = ContainerShared.EnvironmentVariableHelper.ReadRuntimeEnvironmentVariables(cmdLine);
            if (cmdLine.IdentityArgs != null)
            {
                AadHelper.ManagedIdentityClientId = cmdLine.IdentityArgs.ClientId;
                AadHelper.TenantId = cmdLine.IdentityArgs.TenantId;
            }
            return (true, cmdLine);
        }
        #endregion
    }

}
