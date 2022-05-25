﻿using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Azure.ResourceManager.Resources;
using Microsoft.Azure.Batch;
using Microsoft.Extensions.Logging;
using Azure.Core;
using Azure.Identity;
using Azure.ResourceManager.Resources.Models;
using System.Threading.Tasks;
using System.Threading;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
using SqlBuildManager.Console.Shared;
using SqlBuildManager.Console.ContainerApp.Internal;
using System.Diagnostics;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.Arm;

namespace SqlBuildManager.Console.ContainerApp
{
    internal class ContainerAppHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static async Task<bool> DeployContainerApp(CommandLineArgs cmdLine)
        {
            try
            {

                // https://github.com/Azure-Samples/azure-samples-net-management/blob/master/samples/resources/deploy-using-arm-template/Program.cs
                var rgName = cmdLine.ContainerAppArgs.ResourceGroup;
                var subId = cmdLine.ContainerAppArgs.SubscriptionId;
                var deploymentName = cmdLine.JobName;
                string parametersString = GetParametersString(cmdLine);
                log.LogDebug($"ContainerApp parameters:{Environment.NewLine}{parametersString}");
                log.LogDebug(GetSecretsString(cmdLine));
                log.LogDebug(GetEnvironmentVariablesString(cmdLine));

                string templateString = GetTemplateFileContents(cmdLine);
                log.LogDebug($"ContainerApp template:{Environment.NewLine}{templateString}");
                log.LogInformation($"Starting a deployment for Container App 'sbm{deploymentName}' in environment '{cmdLine.ContainerAppArgs.EnvironmentName}'");
                bool success = await ArmHelper.SubmitDeployment(subId, rgName, templateString, parametersString, $"sbm{deploymentName}");

                if (success)
                {
                    //Introduce a delay to see if that will help with what seems to be an issue with a timely managed identity assignment
                    Thread.Sleep(10000);
                    log.LogInformation("Completed Container App deployment: " + deploymentName);
                }
                else
                {
                    log.LogError("Container App deployment failed. Unablet to proceed.");
                }    
                return success;
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to deploy Container App instance: {ex.Message}");
                return false;
            }
        }

        internal static async Task<bool> DeleteContainerApp(CommandLineArgs cmdLine)
        {
            if (await ArmHelper.DeleteResource(cmdLine.ContainerAppArgs.SubscriptionId, cmdLine.ContainerAppArgs.ResourceGroup, $"sbm{cmdLine.JobName}"))
            {
                log.LogInformation($"Successfully deleted ContainerApp: sbm{cmdLine.JobName}");
                return true;
            }
            else
            {
                log.LogError($"Unable to delete ContainerApp: sbm{cmdLine.JobName}");
                return false;
            }
        }

        internal static CommandLineArgs ReadRuntimeEnvironmentVariables(CommandLineArgs cmdLine)
        {
            log.LogInformation("Reading environment variables for Continer App worker");
            string tmp;

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.KeyVaultName);
            if (!string.IsNullOrEmpty(tmp))
            {
                cmdLine.KeyVaultName = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.KeyVaultName}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.IdentityClientId);
            if (!string.IsNullOrEmpty(tmp))
            {
                cmdLine.ClientId = tmp;
                AadHelper.ManagedIdentityClientId = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.IdentityClientId}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.JobName);
            if(!string.IsNullOrEmpty(tmp))
            {
               cmdLine.JobName = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.JobName}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.PackageName);
            if(!string.IsNullOrEmpty(tmp))
            {
               cmdLine.BuildFileName = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.PackageName}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.DacpacName);
            if (!string.IsNullOrEmpty(tmp))
            {
                cmdLine.PlatinumDacpac = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.DacpacName}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.StorageAccountName);
            if (!string.IsNullOrEmpty(tmp))
            {
                cmdLine.StorageAccountName = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.StorageAccountName}");
            }

            if (int.TryParse(Environment.GetEnvironmentVariable(ContainerEnvVariables.Concurrency), out int c))
            {
                cmdLine.Concurrency = c;
            }
            else
            {
                log.LogWarning($"Unable to read or parse environment variable {ContainerEnvVariables.Concurrency}");
            }

            if (Enum.TryParse<CommandLine.ConcurrencyType>(Environment.GetEnvironmentVariable(ContainerEnvVariables.ConcurrencyType), out CommandLine.ConcurrencyType ct))
            {
                cmdLine.ConcurrencyType = ct;
            }
            else
            {
                log.LogWarning($"Unable to read or parse environment variable {ContainerEnvVariables.ConcurrencyType}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.AllowObjectDelete);
            if (!string.IsNullOrEmpty(tmp))
            {
                bool allow;
                if (bool.TryParse(tmp, out allow))
                {
                    cmdLine.AllowObjectDelete = allow;
                }
                else
                {
                    log.LogWarning($"The environment variable {ContainerEnvVariables.AllowObjectDelete} is expecting a boolean value but retrieved '{tmp}'");
                }
             }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.AllowObjectDelete}");
            }


            //If KeyVault is provided, these will get read from KeyVault Secrets
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.StorageAccountKey);
                if (!string.IsNullOrEmpty(tmp))
                {
                    cmdLine.StorageAccountKey = tmp;
                }
                else
                {
                    log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.StorageAccountKey}");
                }

                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.EventHubConnectionString);
                if (!string.IsNullOrEmpty(tmp))
                {
                    cmdLine.EventHubConnection = tmp;
                }
                else
                {
                    log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.EventHubConnectionString}");
                }

                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.ServiceBusTopicConnectionString);
                if (!string.IsNullOrEmpty(tmp))
                {
                    cmdLine.ServiceBusTopicConnection = tmp;
                }
                else
                {
                    log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.ServiceBusTopicConnectionString}");
                }

                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.UserName);
                if (!string.IsNullOrEmpty(tmp))
                {
                    cmdLine.UserName = tmp;
                }
                else
                {
                    log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.UserName}");
                }

                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.Password);
                if (!string.IsNullOrEmpty(tmp))
                {
                    cmdLine.Password = tmp;
                }
                else
                {
                    log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.Password}");
                }
            }
           

            //cmdLine.KeyVaultName = Environment.GetEnvironmentVariable(ContainerEnvVariables.KeyVaultName);
            //cmdLine.PlatinumDacpac = Environment.GetEnvironmentVariable(ContainerEnvVariables.DacpacName);

            return cmdLine;
        }

        private static string GetParametersString(CommandLineArgs cmdLine)
        {
            var parms = new ContainerAppParameters();
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                parms.Password = new Password() { Value = cmdLine.AuthenticationArgs.Password };
                parms.StorageAccountKey = new StorageAccountKey() { Value = cmdLine.ConnectionArgs.StorageAccountKey };
                parms.EventHubConnectionString = new EventHubConnectionString() { Value = cmdLine.ConnectionArgs.EventHubConnectionString };
                parms.Username = new Username() { Value = cmdLine.AuthenticationArgs.UserName };
                
            }else
            {
                parms.IdentityName = new IdentityName() { Value = cmdLine.IdentityArgs.IdentityName };
                parms.IdentityResourceGroup = new IdentityResourceGroup { Value = cmdLine.IdentityArgs.ResourceGroup };
                parms.KeyVaultName = new KeyVaultName() { Value = cmdLine.ConnectionArgs.KeyVaultName };
                parms.IdentityClientId = new IdentityClientId() { Value = cmdLine.IdentityArgs.ClientId };
            }
            parms.Concurrency = new Concurrency() { Value = cmdLine.Concurrency.ToString() };
            parms.ConcurrencyType = new Internal.ConcurrencyType() { Value = cmdLine.ConcurrencyType.ToString() };
            parms.ImageTag = new ImageTag() { Value = cmdLine.ContainerRegistryArgs.ImageTag };
            parms.ImageName = new ImageName() { Value = cmdLine.ContainerRegistryArgs.ImageName };
            parms.RegistryServer = new RegistryServer() { Value = cmdLine.ContainerRegistryArgs.RegistryServer };
            parms.RegistryUserName = new RegistryUserName() { Value = cmdLine.ContainerRegistryArgs.RegistryUserName };
            parms.RegistryPassword = new RegistryPassword() { Value = cmdLine.ContainerRegistryArgs.RegistryPassword };
            parms.EnvironmentName = new EnvironmentName() { Value = cmdLine.ContainerAppArgs.EnvironmentName };
            parms.ServiceBusTopicConnectionString = new ServiceBusTopicConnectionString() { Value = cmdLine.ConnectionArgs.ServiceBusTopicConnectionString };
            parms.Jobname = new Jobname() { Value = cmdLine.JobName };
            parms.Location = new Internal.Location() { Value = cmdLine.ContainerAppArgs.Location };
            parms.MaxContainers = new MaxContainers() { Value = cmdLine.ContainerAppArgs.MaxContainerCount };
            parms.PackageName = new PackageName() { Value = Path.GetFileName(cmdLine.BuildFileName) };
            parms.StorageAccountName = new StorageAccountName() { Value = cmdLine.ConnectionArgs.StorageAccountName };
            if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac))
            {
                parms.DacpacName = new DacpacName() { Value = Path.GetFileName(cmdLine.DacPacArgs.PlatinumDacpac) };
            }
            parms.AllowObjectDelete = new AllowObjectDelete() { Value = cmdLine.AllowObjectDelete.ToString() };

            var jsonText = JsonSerializer.Serialize<ContainerAppParameters>(parms, new JsonSerializerOptions()
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
                
            });
            return jsonText;

        }
        private static string GetSecretsString(CommandLineArgs cmdLine)
        {
            var sb = new StringBuilder("--secrets ");
            sb.Append($"storageaccountkey=\"{cmdLine.ConnectionArgs.StorageAccountKey}\",");
            sb.Append($"storageaccountname=\"{cmdLine.ConnectionArgs.StorageAccountName}\",");
            sb.Append($"eventhubconnectionstring=\"{cmdLine.ConnectionArgs.EventHubConnectionString}\",");
            sb.Append($"servicebustopicconnectionstring=\"{cmdLine.ConnectionArgs.ServiceBusTopicConnectionString}\",");
            sb.Append($"username=\"{cmdLine.AuthenticationArgs.UserName}\",");
            sb.Append($"password=\"{cmdLine.AuthenticationArgs.Password}\",");
            sb.Append($"registrypassword=\"{cmdLine.ContainerRegistryArgs.RegistryPassword}\"");
            return sb.ToString();

        }
        private static string GetEnvironmentVariablesString(CommandLineArgs cmdLine)
        {
            var sb = new StringBuilder("--environment-variables ");
            sb.Append($"Sbm_JobName=\"{cmdLine.JobName}\",");
            sb.Append($"Sbm_PackageName=\"{Path.GetFileName(cmdLine.BuildFileName)}\",");
           // sb.Append($"Sbm_DacpacName=,");
            sb.Append($"Sbm_Concurrency=\"{cmdLine.Concurrency.ToString()}\",");
            sb.Append($"Sbm_ConcurrencyType=\"{cmdLine.ConcurrencyType.ToString()}\",");
            sb.Append($"Sbm_StorageAccountKey=secretref:storageaccountkey,");
            sb.Append($"Sbm_StorageAccountName=secretref:storageaccountname,");
            sb.Append($"Sbm_EventHubConnectionString=secretref:eventhubconnectionstring,");
            sb.Append($"Sbm_ServiceBusTopicConnectionString=secretref:servicebustopicconnectionstring,");
            sb.Append($"Sbm_UserName=secretref:username,");
            sb.Append($"Sbm_Password=secretref:password");
            return sb.ToString();
        }




        internal static string GetTemplateFileContents(CommandLineArgs cmdLine)
        {
            string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string pathToTemplates = Path.Combine(exePath, "ContainerApp");
            string template;

            //Pick the proper template
            if(!cmdLine.ContainerAppArgs.EnvironmentVariablesOnly)
            {
                if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
                {
                    template = File.ReadAllText(Path.Combine(pathToTemplates, "containerapp_arm_template.json"));
                }
                else
                {
                    template = File.ReadAllText(Path.Combine(pathToTemplates, "containerapp_identity_arm_template.json"));
                }
            }
            else
            {
                template =  File.ReadAllText(Path.Combine(pathToTemplates, "containerapp_env_arm_template.json"));
            }

            //Add Container Registry information if provided
            if(!string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryServer))
            {
                var registrySnippit = File.ReadAllText(Path.Combine(pathToTemplates, "registries.json"));
                template = template.Replace("\"registriesplaceholder\"", registrySnippit);
            }
            else
            {
                template = template.Replace("\"registriesplaceholder\"", "");
            }

            return template;
        }    
           
        

        internal static void SetEnvVariablesForTest(CommandLineArgs cmdLine)
        {
            log.LogInformation("Setting environment variables for local testing");
            var target = EnvironmentVariableTarget.Process;
            Environment.SetEnvironmentVariable(ContainerEnvVariables.Concurrency, cmdLine.Concurrency.ToString(), target);
            Environment.SetEnvironmentVariable(ContainerEnvVariables.ConcurrencyType, cmdLine.ConcurrencyType.ToString(), target);
            Environment.SetEnvironmentVariable(ContainerEnvVariables.DacpacName, cmdLine.DacpacName, target);
            Environment.SetEnvironmentVariable(ContainerEnvVariables.EventHubConnectionString, cmdLine.ConnectionArgs.EventHubConnectionString, target);
            Environment.SetEnvironmentVariable(ContainerEnvVariables.JobName, cmdLine.JobName, target);
            Environment.SetEnvironmentVariable(ContainerEnvVariables.PackageName, cmdLine.BuildFileName, target);
            Environment.SetEnvironmentVariable(ContainerEnvVariables.Password, cmdLine.AuthenticationArgs.Password, target);
            Environment.SetEnvironmentVariable(ContainerEnvVariables.ServiceBusTopicConnectionString, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, target);
            Environment.SetEnvironmentVariable(ContainerEnvVariables.StorageAccountKey, cmdLine.ConnectionArgs.StorageAccountKey, target);
            Environment.SetEnvironmentVariable(ContainerEnvVariables.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountName, target);
            Environment.SetEnvironmentVariable(ContainerEnvVariables.UserName, cmdLine.AuthenticationArgs.UserName, target);

        }
    }
}
