using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.KeyVault;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console
{
    internal partial class Worker
    {
        internal static int SaveAndEncryptAciSettings(CommandLineArgs cmdLine, bool clearText)
        {
            cmdLine.BatchArgs = null;
            cmdLine.ConnectionArgs.BatchAccountKey = null;
            cmdLine.ConnectionArgs.BatchAccountName = null;
            cmdLine.ConnectionArgs.BatchAccountUrl = null;
            cmdLine.IdentityArgs.PrincipalId = null;
            cmdLine.IdentityArgs.ResourceId = null;
            cmdLine.KubernetesArgs = null;

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

            var res = await AciPrepAndUploadBuildPackage(cmdLine, packageName, platinumDacpac, force);
            if (res != 0)
            {
                log.LogError("Failed to upload build package to Blob storage");
                return 1;
            }

            res = await EnqueueOverrideTargets(cmdLine);
            if (res != 0)
            {
                log.LogError("Failed to enqueue override targets");
                return 1;
            }

            res = await AciDeploy(cmdLine, packageName, platinumDacpac, monitor, unittest, stream);
            if (res != 0)
            {
                log.LogError("Failed to deploy ACI app");
                log.LogInformation("Cleaning up any remaining queue messages...");
                await DeQueueOverrideTargets(cmdLine);
                return res;
            }
            else
            {
                return res;
            }
        }
        internal static async Task<int> AciPrepAndUploadBuildPackage(CommandLineArgs cmdLine, FileInfo packageName, FileInfo platinumDacpac,  bool force)
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
            (bool retVal, string sbmName) = await ValidateAndUploadContainerBuildFilesToStorage(cmdLine, packageName, platinumDacpac, force);
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
            success = await Aci.AciHelper.DeployAci(cmdLine);
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
                    var stat = await Aci.AciHelper.AciIsInErrorState(cmdLine.IdentityArgs.SubscriptionId, cmdLine.AciArgs.ResourceGroup, cmdLine.AciArgs.AciName);
                    aciIsInErrorState = stat;
                }
                catch { }
                System.Threading.Thread.Sleep(15000);
            }
        }

        

       

        
    }
}
