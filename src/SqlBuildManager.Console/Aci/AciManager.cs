using Azure;
using Azure.Core;
using Azure.ResourceManager.ContainerInstance;
using Azure.ResourceManager.ContainerInstance.Models;
using Azure.ResourceManager.Models;
using Azure.ResourceManager.Network;
using Azure.ResourceManager.Network.Models;
using Azure.ResourceManager.Resources;
using Microsoft.Extensions.Logging;
using Spectre.Console;
using SqlBuildManager.Console.Arm;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.ContainerShared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Aci
{
    class AciManager
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static async Task<string> DeployNetworkProfile(CommandLineArgs cmdLine)
        {
            var rgResourceId = ResourceGroupResource.CreateResourceIdentifier(cmdLine.AciArgs.SubscriptionId, cmdLine.AciArgs.ResourceGroup);
            var rgResourceGroup = ArmHelper.SbmArmClient.GetResourceGroupResource(rgResourceId).Get().Value;

            var vnetRg = string.IsNullOrWhiteSpace(cmdLine.NetworkArgs.ResourceGroup) ? cmdLine.AciArgs.ResourceGroup : cmdLine.NetworkArgs.ResourceGroup;
            string subnetId = $"/subscriptions/{cmdLine.IdentityArgs.SubscriptionId}/resourceGroups/{vnetRg}/providers/Microsoft.Network/virtualNetworks/{cmdLine.NetworkArgs.VnetName}/subnets/{cmdLine.NetworkArgs.SubnetName}";

            var data = new NetworkProfileData()
            {
                ContainerNetworkInterfaceConfigurations =
                    {
                        new ContainerNetworkInterfaceConfiguration()
                        {
                            IPConfigurations =
                            {
                                new NetworkIPConfigurationProfile()
                                {
                                Subnet = new SubnetData()
                                {
                                Id = new ResourceIdentifier(subnetId),
                                },
                            Name = "ipconfig1",
                            }
                        },
                        Name = "eth1",
                        }
                    },
                Location = rgResourceGroup.Data.Location
            };

            NetworkProfileCollection collection = rgResourceGroup.GetNetworkProfiles();
            var result = await collection.CreateOrUpdateAsync(WaitUntil.Completed, $"{cmdLine.AciArgs.AciName}profile", data);
            if(result.GetRawResponse().Status < 300)
            {
                log.LogInformation("Created ACI network profile");
                return subnetId;
            }
            else
            {
                log.LogError("Failed to create ACI network profile");
                return string.Empty;
            }


        }
        internal static async Task<bool> DeployAci(CommandLineArgs cmdLine)
        {
            if (await AciInstanceExists(cmdLine))
            {
                await DeleteAciInstance(cmdLine.AciArgs.SubscriptionId, cmdLine.AciArgs.ResourceGroup, cmdLine.AciArgs.AciName);
            }
            log.LogInformation("Starting ACI deployment");
            string subnetId = string.Empty;
            if (!string.IsNullOrWhiteSpace(cmdLine.NetworkArgs.VnetName) && !string.IsNullOrWhiteSpace(cmdLine.NetworkArgs.SubnetName))
            {
                subnetId = await DeployNetworkProfile(cmdLine);
                if (subnetId.Length == 0)
                {
                    return false;
                }
            }


            var rgResourceId = ResourceGroupResource.CreateResourceIdentifier(cmdLine.AciArgs.SubscriptionId, cmdLine.AciArgs.ResourceGroup);
            var rgResourceGroup = ArmHelper.SbmArmClient.GetResourceGroupResource(rgResourceId).Get().Value;

            //Init Container Group Data
            var containerGroupData = new ContainerGroupData(rgResourceGroup.Data.Location, new List<ContainerInstanceContainer>(), ContainerInstanceOperatingSystemType.Linux)
            {
                RestartPolicy = "Never",
            };
            containerGroupData.OSType = "Linux";
            //Add Identity
            var mi = new ManagedServiceIdentity(ManagedServiceIdentityType.UserAssigned);
            mi.UserAssignedIdentities.Add(new ResourceIdentifier(cmdLine.IdentityArgs.ResourceId), new UserAssignedIdentity());
            containerGroupData.Identity = mi;

            //Configure the containers..
            string imageName;
            if (string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.ImageName))
            {
                imageName = $"sqlbuildmanager:{cmdLine.ContainerRegistryArgs.ImageTag}";
            }
            else
            {
                imageName = $"{cmdLine.ContainerRegistryArgs.ImageName}:{cmdLine.ContainerRegistryArgs.ImageTag}";
            }
            if (string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryServer))
            {
                imageName = $"ghcr.io/mmckechney/{imageName}";
            }
            else
            {
                imageName = $"{cmdLine.ContainerRegistryArgs.RegistryServer}/{imageName}";
            }

            var envVariables = GetContainerEnvironmentVariables(cmdLine);
            for (int i = 0; i < cmdLine.AciArgs.ContainerCount; i++)
            {
                var containerRequests = new ContainerResourceRequestsContent(1.0, 1.0);
                var containerReqs = new ContainerResourceRequirements(containerRequests);
                var container = new ContainerInstanceContainer($"sqlbuildmanager{i.ToString().PadLeft(3, '0')}", imageName, containerReqs);
                container.Command.Add("dotnet");
                container.Command.Add("sbm.dll");
                container.Command.Add("aci");
                container.Command.Add("worker");
                if (cmdLine.QueryFile != null)
                {
                    container.Command.Add("query");
                }


                envVariables.ForEach(e => container.EnvironmentVariables.Add(e));

                containerGroupData.Containers.Add(container);
            }
           

            //Set container registry creds if needed
            ContainerGroupImageRegistryCredential registryCreds;
            if (!string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryServer))
            {
                if (string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.IdentityName))
                    {
                    registryCreds = new ContainerGroupImageRegistryCredential(cmdLine.ContainerRegistryArgs.RegistryServer)
                    {
                        Username = cmdLine.ContainerRegistryArgs.RegistryUserName,
                        Password = cmdLine.ContainerRegistryArgs.RegistryPassword,
                        Server = cmdLine.ContainerRegistryArgs.RegistryServer
                    };
                }else
                {
                    registryCreds = new ContainerGroupImageRegistryCredential(cmdLine.ContainerRegistryArgs.RegistryServer)
                    {
                        Identity = cmdLine.IdentityArgs.ResourceId,
                       // IdentityUri = cmdLine.IdentityArgs.ResourceId,
                        Server = cmdLine.ContainerRegistryArgs.RegistryServer
                    };
                }
                containerGroupData.ImageRegistryCredentials.Add(registryCreds);
            }
            

            if (subnetId.Length > 0)
            {
               containerGroupData.SubnetIds.Add(new ContainerGroupSubnetId(new ResourceIdentifier(subnetId)));
            }

            try
            {
                log.LogDebug(JsonSerializer.Serialize<ContainerGroupData>(containerGroupData, new JsonSerializerOptions() { WriteIndented = true }));

                var coll = rgResourceGroup.GetContainerGroups();
                var result = await coll.CreateOrUpdateAsync(WaitUntil.Completed, cmdLine.AciArgs.AciName, containerGroupData);
                if (result.GetRawResponse().Status < 300)
                {
                    log.LogInformation($"Completed ACI deployment for App Name: '{cmdLine.AciArgs.AciName}'");
                    return true;
                }
                else
                {
                    log.LogError("ACI deployment failed. Unable to proceed.");
                    return false;
                }
            }catch(Exception exe)
            {
                log.LogError(exe.Message);
                return false;
            }

        }
        internal static List<ContainerEnvironmentVariable> GetContainerEnvironmentVariables(CommandLineArgs cmdLine)
        {
            var lst = new List<ContainerEnvironmentVariable>();
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.KeyVaultName) { Value = cmdLine.ConnectionArgs.KeyVaultName });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.DacpacName) { Value = Path.GetFileName(cmdLine.DacPacArgs.PlatinumDacpac) });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.EventHubConnectionString) { Value = cmdLine.ConnectionArgs.EventHubConnectionString });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.JobName) { Value = cmdLine.JobName });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.ServiceBusTopicConnectionString) { Value = cmdLine.ConnectionArgs.ServiceBusTopicConnectionString });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.PackageName) { Value = Path.GetFileName(cmdLine.BuildFileName) });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.Concurrency) { Value = cmdLine.Concurrency.ToString() });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.AuthType) { Value = cmdLine.AuthenticationArgs.AuthenticationType.ToString() });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.ConcurrencyType) { Value = cmdLine.ConcurrencyType.ToString() });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.AllowObjectDelete) { Value = cmdLine.AllowObjectDelete.ToString() });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.IdentityClientId) { Value = cmdLine.IdentityArgs.ClientId.ToString() });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.StorageAccountName) { Value = cmdLine.ConnectionArgs.StorageAccountName });

            if (cmdLine.QueryFile != null)
            {
                lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.QueryFile) { Value = cmdLine.QueryFile.Name });
            }
            if (cmdLine.OutputFile != null)
            {
                lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.OutputFile) { Value = cmdLine.OutputFile.Name });
            }

            return lst;
        }

        private static async Task<bool> AciInstanceExists(CommandLineArgs cmdLine)
        {
            try
            {
                var rgResourceId = ResourceGroupResource.CreateResourceIdentifier(cmdLine.AciArgs.SubscriptionId, cmdLine.AciArgs.ResourceGroup);
                var rgResourceGroup = (await ArmHelper.SbmArmClient.GetResourceGroupResource(rgResourceId).GetAsync()).Value;
                var coll = (await rgResourceGroup.GetContainerGroups().GetAsync(cmdLine.AciArgs.AciName)).Value;
                if(coll.HasData)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Azure.RequestFailedException rexe)
            {
                if (rexe.Status == 404)
                {
                    return false;
                }
                else
                {
                    log.LogError(rexe.Message);
                    return true;
                }
            }
            catch (Exception ex)
            {

                log.LogError(ex.Message);
                return true;
            }
        }

        internal static async Task<bool> DeleteAciInstance(string subscriptionId, string resourceGroupName, string aciName)
        {
            try
            {
                log.LogInformation("Removing any pre-existing ACI deployment");
                var success = await ArmHelper.DeleteResource(subscriptionId, resourceGroupName, aciName);
                //Wait for the delete to complete
                Thread.Sleep(10000);
                log.LogInformation("Pre-existing ACI deployment removed");
                return success;
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to remove existing ACI instance: {exe.Message}");
                return false;
            }
        }

        internal static async Task<bool> AciIsInErrorState(string subscriptionId, string resourceGroupName, string aciName)
        {

            var aciResult = await GetAciInstanceData(subscriptionId, resourceGroupName, aciName);
            var containerCount = aciResult.Properties.Containers.Count;
            var status = aciResult.Properties.Containers.Where(c => c.Properties.InstanceView.CurrentState.DetailStatus.ToLower() == "error").Count();

            return status == containerCount;
        }

        private static async Task<Aci.Arm.Deployment> GetAciInstanceData(string subscriptionId, string resourceGroupName, string aciName)
        {
            var resp = await ArmHelper.GetAciDeploymentDetails(subscriptionId, resourceGroupName, aciName);
            var aciResult = JsonSerializer.Deserialize<Aci.Arm.Deployment>(JsonSerializer.Serialize(resp));

            return aciResult;
        }

        #region Container Worker Methods


        #endregion

    }
}
