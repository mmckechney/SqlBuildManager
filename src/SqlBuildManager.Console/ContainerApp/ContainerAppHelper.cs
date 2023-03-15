using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.Arm;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ContainerApp.Internal;
using SqlBuildManager.Console.ContainerShared;
using System;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;
using Azure.ResourceManager.AppContainers;
using Azure.ResourceManager.AppContainers.Models;
using Azure.ResourceManager;
using Azure.ResourceManager.Resources;
using Azure.ResourceManager.Models;
using Microsoft.IdentityModel.Tokens;
using System.Collections.Generic;
using Azure.Core;
using System.Net.Sockets;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace SqlBuildManager.Console.ContainerApp
{
    internal class ContainerAppHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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

         private static (Dictionary<string, string>, Dictionary<string, string>) GetSecretsAndRefsDictionaries(CommandLineArgs cmdLine)
        {
            var secrets = new Dictionary<string, string>();
            var secretRefs = new Dictionary<string, string>();
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
                {
                    secrets.Add("password", cmdLine.AuthenticationArgs.Password);
                    secretRefs.Add(ContainerEnvVariables.Password, "password");
                }

                if (!string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName))
                {
                    secrets.Add("username", cmdLine.AuthenticationArgs.UserName);
                    secretRefs.Add(ContainerEnvVariables.UserName, "username");
                }

                if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
                {
                    secrets.Add("storageaccountkey", cmdLine.ConnectionArgs.StorageAccountKey);
                    secretRefs.Add(ContainerEnvVariables.StorageAccountKey, "storageaccountkey");
                }
               
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                secrets.Add("eventhubconnectionstring", cmdLine.ConnectionArgs.EventHubConnectionString);
                secretRefs.Add(ContainerEnvVariables.EventHubConnectionString, "eventhubconnectionstring");
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                secrets.Add("servicebustopicconnectionstring", cmdLine.ConnectionArgs.ServiceBusTopicConnectionString);
                secretRefs.Add(ContainerEnvVariables.ServiceBusTopicConnectionString, "servicebustopicconnectionstring");
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountName))
            {
                secrets.Add("storageaccountname", cmdLine.ConnectionArgs.StorageAccountName);
                secretRefs.Add(ContainerEnvVariables.StorageAccountName, "storageaccountname");
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryServer) && !string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryUserName))
            {
                var regPwName = cmdLine.ContainerRegistryArgs.RegistryServer.Replace(".", "") + "-" + cmdLine.ContainerRegistryArgs.RegistryUserName;
                secrets.Add(regPwName, cmdLine.ContainerRegistryArgs.RegistryPassword);
            }

            return (secrets, secretRefs);

        }
        private static Dictionary<string, string> GetParametersDictionary(CommandLineArgs cmdLine)
        {
            var parms = new Dictionary<string, string>();
            parms.Add(ContainerEnvVariables.JobName, cmdLine.JobName);
            parms.Add(ContainerEnvVariables.PackageName, Path.GetFileName(cmdLine.BuildFileName));
            if (!string.IsNullOrWhiteSpace(cmdLine.DacPacArgs.PlatinumDacpac))
            {
                parms.Add(ContainerEnvVariables.DacpacName, Path.GetFileName(cmdLine.DacPacArgs.PlatinumDacpac));
            }
            parms.Add(ContainerEnvVariables.Concurrency, cmdLine.Concurrency.ToString());
            parms.Add(ContainerEnvVariables.ConcurrencyType, cmdLine.ConcurrencyType.ToString());
            parms.Add(ContainerEnvVariables.KeyVaultName, cmdLine.ConnectionArgs.KeyVaultName);

            if (!string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ClientId))
            {
                parms.Add(ContainerEnvVariables.IdentityClientId, cmdLine.IdentityArgs.ClientId);
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ResourceGroup))
            {
                parms.Add(ContainerEnvVariables.IdentityResourceGroup, cmdLine.IdentityArgs.ResourceGroup);
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.IdentityName))
            {
                parms.Add(ContainerEnvVariables.IdentityName, cmdLine.IdentityArgs.IdentityName);
            }
            parms.Add(ContainerEnvVariables.AllowObjectDelete, cmdLine.AllowObjectDelete.ToString());
            parms.Add(ContainerEnvVariables.AuthType, cmdLine.AuthenticationArgs.AuthenticationType.ToString());

            if (cmdLine.QueryFile != null)
            {
                parms.Add(ContainerEnvVariables.QueryFile, cmdLine.QueryFile.Name);
            }
            if (cmdLine.OutputFile != null)
            {
                parms.Add(ContainerEnvVariables.OutputFile, cmdLine.OutputFile.Name);
            }

            return parms;

        }

        internal static async Task<bool> DeployContainerApp(CommandLineArgs cmdLine)
        {
            string containerAppName = $"sbm{cmdLine.JobName}";
            var containerAppEnvId = ContainerAppManagedEnvironmentResource.CreateResourceIdentifier(cmdLine.ContainerAppArgs.SubscriptionId, cmdLine.ContainerAppArgs.ResourceGroup, cmdLine.ContainerAppArgs.EnvironmentName);
            var containerAppEnv = (await ArmHelper.SbmArmClient.GetContainerAppManagedEnvironmentResource(containerAppEnvId).GetAsync()).Value;

            var containerAppId = ContainerAppResource.CreateResourceIdentifier(cmdLine.ContainerAppArgs.SubscriptionId, cmdLine.ContainerAppArgs.ResourceGroup, $"sbm{cmdLine.JobName}");
            var containerApp = ArmHelper.SbmArmClient.GetContainerAppResource(containerAppId);

            var containerAppData = new ContainerAppData(containerAppEnv.Data.Location);
            containerAppData.ManagedEnvironmentId = containerAppEnvId;
            containerAppData.Configuration = new ContainerAppConfiguration();
            containerAppData.Configuration.ActiveRevisionsMode = ContainerAppActiveRevisionsMode.Single;
            containerAppData.Configuration.Dapr = new ContainerAppDaprConfiguration()
            {
                IsEnabled = false
            };

            //Add Identity if Applicable
            if (!string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.IdentityName))
            {
                var mi = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned);
                mi.UserAssignedIdentities.Add(new ResourceIdentifier(cmdLine.IdentityArgs.ResourceId), new UserAssignedIdentity());
                containerAppData.Identity = mi;
            }

            //Set container registry creds if needed
            ContainerAppRegistryCredentials registryCreds;
            if (!string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryServer))
            {
                if (!string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.IdentityName))
                {
                    registryCreds = new ContainerAppRegistryCredentials()
                    {
                        Server = cmdLine.ContainerRegistryArgs.RegistryServer,
                        Identity = cmdLine.IdentityArgs.ResourceId
                    };
                }
                else
                {
                    registryCreds = new ContainerAppRegistryCredentials()
                    {
                        Server = cmdLine.ContainerRegistryArgs.RegistryServer,
                        Username = cmdLine.ContainerRegistryArgs.RegistryUserName,
                        PasswordSecretRef = cmdLine.ContainerRegistryArgs.RegistryServer.Replace(".", "") + "-" + cmdLine.ContainerRegistryArgs.RegistryUserName
                    };
                }
                containerAppData.Configuration.Registries.Add(registryCreds);
            }
            



         //Add Secrets to the ContainerAppData object
         (var secrets, var secretRefs) = GetSecretsAndRefsDictionaries(cmdLine);
            foreach (var secret in secrets)
            {
                containerAppData.Configuration.Secrets.Add(new ContainerAppWritableSecret()
                {
                    Name = secret.Key,
                    Value = secret.Value
                });
            }
            
            //Configure the container 
            var container = new ContainerAppContainer();
            string image;
            if (string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryServer))
            {
                image = $"ghcr.io/mmckechney/{cmdLine.ContainerRegistryArgs.ImageName}:{cmdLine.ContainerRegistryArgs.ImageTag}";
            }
            else
            {
                image = $"{cmdLine.ContainerRegistryArgs.RegistryServer}/{cmdLine.ContainerRegistryArgs.ImageName}:{cmdLine.ContainerRegistryArgs.ImageTag}";
            }
            container.Image = image;
            container.Name = containerAppName;
            //Add commmand args
            container.Command.Add("dotnet");
            container.Command.Add("sbm.dll");
            container.Command.Add("--loglevel");
            container.Command.Add("information");
            container.Command.Add("containerapp");
            container.Command.Add("worker");
            if(cmdLine.QueryFile != null)
            {
                container.Command.Add("query");
            }
      

            //Add Parameters/Values to the ContainerAppData object
            var parms = GetParametersDictionary(cmdLine);
            foreach (var parm in parms)
            {
                container.Env.Add(new ContainerAppEnvironmentVariable()
                {
                    Name = parm.Key,
                    Value = parm.Value
                });
            }

            //Add Parameter SecretRefs to the ContainerAppData object
            foreach (var sref in secretRefs)
            {
                container.Env.Add(new ContainerAppEnvironmentVariable()
                {
                    Name = sref.Key,
                    SecretRef = sref.Value
                });
            }

            ContainerAppScale scaleSettings = new ContainerAppScale();
            scaleSettings.MinReplicas = 0;
            scaleSettings.MaxReplicas = cmdLine.ContainerAppArgs.MaxContainerCount;

            var customScale = new ContainerAppCustomScaleRule()
            {
                CustomScaleRuleType = "azure-servicebus",
            };
            customScale.Metadata.Add("topicName", "sqlbuildmanager");
            customScale.Metadata.Add("subscriptionName", cmdLine.JobName);
            customScale.Metadata.Add("messageCount", "2");
            customScale.Auth.Add(new ContainerAppScaleRuleAuth()
            {
                SecretRef = "azure-servicebus",
                TriggerParameter = "connection"
            });
                    


            scaleSettings.Rules.Add(new ContainerAppScaleRule()
            {
                Name = "servicebusscalingrule",
                Custom = customScale
            });
            containerAppData.Template = new ContainerAppTemplate();
            containerAppData.Template.Scale = scaleSettings;
           
            //Add Container to the ContainerAppData object
            containerAppData.Template.Containers.Add(container);

            // get the collection of this ContainerAppResource
            ResourceIdentifier resourceGroupResourceId = ResourceGroupResource.CreateResourceIdentifier(cmdLine.ContainerAppArgs.SubscriptionId, cmdLine.ContainerAppArgs.ResourceGroup);
            ResourceGroupResource resourceGroupResource = ArmHelper.SbmArmClient.GetResourceGroupResource(resourceGroupResourceId);
            ContainerAppCollection collection = resourceGroupResource.GetContainerApps();

            log.LogDebug(JsonSerializer.Serialize<ContainerAppData>(containerAppData, new JsonSerializerOptions() { WriteIndented = true }));
            var result = await collection.CreateOrUpdateAsync(Azure.WaitUntil.Completed, containerAppName, containerAppData);


            if (result.GetRawResponse().Status < 300)
            {
                //Introduce a delay to see if that will help with what seems to be an issue with a timely managed identity assignment
                Thread.Sleep(10000);
                log.LogInformation($"Completed Container App deployment for App Name: '{containerAppName}'");
                return true;
            }
            else
            {
                log.LogError("Container App deployment failed. Unable to proceed.");
                return false;
            }


        }
    }
}