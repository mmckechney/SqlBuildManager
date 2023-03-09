using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CloudStorage;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.KeyVault;
using SqlBuildManager.Console.Kubernetes;
using SqlSync.Connection;
using System;
using System.IO;
using System.Threading.Tasks;

namespace SqlBuildManager.Console
{
    internal partial class Worker
    {
        internal static int SaveAndEncryptKubernetesSettings(CommandLineArgs cmdLine, bool clearText)
        {
            cmdLine.BatchArgs = null;
            cmdLine.ConnectionArgs.BatchAccountKey = null;
            cmdLine.ConnectionArgs.BatchAccountName = null;
            cmdLine.ConnectionArgs.BatchAccountUrl = null;
            cmdLine.AciArgs = null;
            cmdLine.ContainerAppArgs = null;
            cmdLine.IdentityArgs.PrincipalId = null;
            cmdLine.IdentityArgs.ResourceId = null;
            cmdLine.IdentityArgs.IdentityName = null;
            cmdLine.IdentityArgs.ClientId = null;
            cmdLine.IdentityArgs.ResourceGroup = null;
            cmdLine.IdentityArgs.SubscriptionId = null;
            cmdLine.ContainerRegistryArgs.RegistryUserName = null;
            cmdLine.ContainerRegistryArgs.RegistryPassword = null;
            if (cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.ManagedIdentity)
            {
                cmdLine.KeyVaultName = null;
            }

            return SaveAndEncryptSettings(cmdLine, clearText);
        }
        internal static async Task<int> KubernetesRun(CommandLineArgs cmdLine, FileInfo @override, FileInfo packagename, FileInfo platinumdacpac, bool force, bool allowObjectDelete, bool unittest, bool stream, bool cleanupOnFailure)
        {
            (var x, cmdLine) = Init(cmdLine);
            if (packagename == null && platinumdacpac == null)
            {
                log.LogError("Either an SBM package or DACPAC file is required.");
                return -1;
            }
            //Save secrets
            if (packagename != null)
            {
                cmdLine.BuildFileName = packagename.FullName;
            }
            if (platinumdacpac != null)
            {
                cmdLine.PlatinumDacpac = platinumdacpac.FullName;
            }
            cmdLine.MultiDbRunConfigFileName = @override.FullName;

            //Upload 'prep'
            (var retVal, cmdLine) = await KubernetesUploadBuildPackage(cmdLine, null, null, packagename, platinumdacpac, force);
            if (retVal != 0)
            {
                log.LogError("There was a problem uploading the build package to Azure storage");
                return -1;
            }

            return await KubernetesApplyJobs(cmdLine, @override, cleanupOnFailure, stream, unittest);



        }
        private static async Task<int> KubernetesApplyJobs(CommandLineArgs cmdLine, FileInfo @override, bool cleanupOnFailure, bool stream, bool unittest)
        {
            string k8Jobname = KubernetesManager.KubernetesJobName(cmdLine);
            if (@override != null && string.IsNullOrWhiteSpace(cmdLine.MultiDbRunConfigFileName))
            {
                cmdLine.MultiDbRunConfigFileName = @override.FullName;
            }

            //Create YAML files
            var kubernetesFiles = await KubernetesManager.SaveKubernetesYamlFiles(cmdLine, cmdLine.JobName, new DirectoryInfo(Environment.CurrentDirectory));
            var runtimeFileInfo = new FileInfo(kubernetesFiles.RuntimeConfigMapFile);
            FileInfo secretsFileInfo = null;
            bool secretsExist = false;
            if (!string.IsNullOrWhiteSpace(kubernetesFiles.SecretsFile))
            {
                secretsFileInfo = new FileInfo(kubernetesFiles.SecretsFile);
                secretsExist = true;
            }

            //Enqueue
            int retVal = await EnqueueContainerOverrideTargets(cmdLine, secretsFileInfo, runtimeFileInfo, cmdLine.ConnectionArgs.KeyVaultName, cmdLine.JobName, cmdLine.ConcurrencyType, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, @override);
            if (retVal != 0)
            {
                log.LogError("There was a problem enqueuing the database targest to Service Bus");
                return -1;
            }
            //Kubernetes apply
            var success = KubernetesManager.ApplyDeployment(kubernetesFiles);
            if (!success)
            {
                log.LogError("There was a problem deploying to Kubernetes");
                log.LogInformation("Attempting to clean up Kubernetes resources...");
                if (cleanupOnFailure)
                {
                    await KubernetesDequeueOverrideTargets(cmdLine, secretsFileInfo, runtimeFileInfo, cmdLine.ConnectionArgs.KeyVaultName, cmdLine.JobName, cmdLine.ConcurrencyType, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString);
                    KubernetesManager.CleanUpKubernetesResource(secretsExist);
                }
                return -1;
            }
            //Monitor pod creation

            log.LogInformation("Checking for successful job start...");

            if (!Kubernetes.KubernetesManager.MonitorForPodStart(k8Jobname))
            {
                log.LogError("Failed to start Kubernetes jobs. Running clean-up");
                await DeQueueOverrideTargets(cmdLine);
                if (cleanupOnFailure)
                {
                    KubernetesManager.CleanUpKubernetesResource(secretsExist);
                }
                return -1;
            }

            //Monitor service bus
            retVal = await KubernetesMonitorRuntimeProgress(cmdLine, secretsFileInfo, runtimeFileInfo, unittest, stream, false);

            //Clean-up
            log.LogInformation("Cleaning up Kubernetes resources...");
            KubernetesManager.CleanUpKubernetesResource(secretsExist, k8Jobname);

            ConsolidateRuntimeLogFiles(cmdLine);
            return retVal;
        }
        internal static async Task<int> KubernetesSaveYamlFiles(CommandLineArgs cmdLine, DirectoryInfo path, string prefix, FileInfo packagename, FileInfo platinumdacpac, bool force)
        {
            (var x, cmdLine) = Init(cmdLine);
            if (packagename != null)
            {
                cmdLine.BuildFileName = packagename.FullName;
            }
            if (platinumdacpac != null)
            {
                cmdLine.PlatinumDacpac = platinumdacpac.FullName;
            }
            await KubernetesManager.SaveKubernetesYamlFiles(cmdLine, prefix, path);
            return 0;
        }
        internal static async Task<int> KubernetesQuery(CommandLineArgs cmdLine, FileInfo @override, bool force, bool stream, bool unittest, bool cleanupOnFailure)
        {
            (int success, cmdLine) = PrepForRemoteQueryExecution(cmdLine);
            if (success != 0)
            {
                return success;
            }

            log.LogDebug("Entering Kubernetes Query Execution");
            log.LogInformation("Running Kubernetes Query Execution...");

            //Upload 'prep'
            (var retVal, string tmp) = await ValidateAndUploadContainerQueryToStorage(cmdLine, force);
            if (!retVal)
            {
                log.LogError("There was a problem uploading the query file to Azure storage");
                return -1;
            }

            success = await KubernetesApplyJobs(cmdLine, @override, cleanupOnFailure, stream, unittest);

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

        internal static async Task<int> KubernetesMonitorRuntimeProgress(CommandLineArgs cmdLine, FileInfo secretsFile, FileInfo runtimeFile, bool unittest = false, bool stream = false, bool consolidateLogs = true)
        {
            if (cmdLine == null)
            {
                cmdLine = new CommandLineArgs();
            }
            (var x, cmdLine) = Init(cmdLine);
            if (runtimeFile != null && secretsFile != null)
            {
                cmdLine = KubernetesManager.SetCmdLineArgsFromSecretsAndConfigmap(cmdLine, secretsFile.Name, runtimeFile.FullName);
            }

            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            if (!kvSuccess)
            {
                log.LogInformation("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                return -4;
            }

            var retVal = await MonitorServiceBusRuntimeProgress(cmdLine, stream, DateTime.UtcNow.AddMinutes(-15), unittest);
            if (consolidateLogs)
            {
                ConsolidateRuntimeLogFiles(cmdLine);
            }
            return retVal;

        }

        internal static async Task<int> KubernetesDequeueOverrideTargets(CommandLineArgs cmdLine, FileInfo secretsFile, FileInfo runtimeFile, string keyvaultname, string jobname, ConcurrencyType concurrencytype, string servicebustopicconnection)
        {
            bool valid;
            if (cmdLine == null)
            {
                cmdLine = new CommandLineArgs();
            }
            (var x, cmdLine) = Init(cmdLine);
            if (secretsFile != null && runtimeFile != null)
            {
                KubernetesManager.SetCmdLineArgsFromSecretsAndConfigmap(cmdLine, secretsFile.Name, runtimeFile.Name);
            }

            (valid, cmdLine) = Validation.ValidateContainerQueueArgs(cmdLine, keyvaultname, jobname, concurrencytype, servicebustopicconnection);
            if (!valid)
            {
                return 1;
            }


            var val = await DeQueueOverrideTargets(cmdLine);
            return val;


        }


        internal static async Task<(int, CommandLineArgs)> KubernetesUploadBuildPackage(CommandLineArgs cmdLine, FileInfo secretsFile, FileInfo runtimeFile, FileInfo packageName, FileInfo platinumDacpac, bool force)
        {
            (var x, cmdLine) = Init(cmdLine);
            if (runtimeFile != null && secretsFile != null)
            {
                cmdLine = KubernetesManager.SetCmdLineArgsFromSecretsAndConfigmap(cmdLine, secretsFile.Name, runtimeFile.Name);
            }

            bool kvSuccess;
            (kvSuccess, cmdLine) = KeyVaultHelper.GetSecrets(cmdLine);
            if (!kvSuccess)
            {
                log.LogError("A Key Vault name was provided, but unable to retrieve secrets from the Key Vault");
                return (-4, cmdLine);
            }

            if (string.IsNullOrWhiteSpace(cmdLine.JobName) || string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountName))
            {
                log.LogError("Values for --jobname, and --storageaccountname are required as prameters or included in the --secretsfile and --runtimefile");
                return (1, cmdLine);

            }
            //if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey) && string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ClientId))
            //{
            //    log.LogError("A value for --storageaccountkey are required as prameters or included in the --secretsfile and --runtimefile");
            //    return 1;

            //}
            (bool retVal, string sbmName) = await ValidateAndUploadContainerBuildFilesToStorage(cmdLine, packageName, platinumDacpac, force);
            if (!retVal)
            {
                return (1, cmdLine);
            }
            else
            {
                if (!string.IsNullOrEmpty(sbmName))
                {
                    cmdLine.BuildFileName = Path.GetFileName(sbmName);
                }
                if (packageName != null)
                {
                    cmdLine.BuildFileName = packageName.Name;
                }
                if (platinumDacpac != null)
                {
                    cmdLine.PlatinumDacpac = platinumDacpac.Name;
                }
                if (runtimeFile != null)
                {

                    string runtimeContents = KubernetesManager.GenerateConfigmapYaml(cmdLine);
                    File.WriteAllText(runtimeFile.FullName, runtimeContents);
                    log.LogInformation($"Updated runtime file '{runtimeFile.FullName}' with job and package name");
                }
                return (0, cmdLine);
            }

        }

    }
}
