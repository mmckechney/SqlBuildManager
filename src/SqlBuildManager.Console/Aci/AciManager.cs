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
    public class AciManager
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        public static async Task<string> DeployNetworkProfile(CommandLineArgs cmdLine)
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
            var logLevel = Logging.ApplicationLogging.GetLogLevelString();
            for (int i = 0; i < cmdLine.AciArgs.ContainerCount; i++)
            {
                var containerRequests = new ContainerResourceRequestsContent(1.0, 1.0);
                var containerReqs = new ContainerResourceRequirements(containerRequests);
                var container = new ContainerInstanceContainer($"sqlbuildmanager{i.ToString().PadLeft(3, '0')}", imageName, containerReqs);
                container.Command.Add("dotnet");
                container.Command.Add("sbm.dll");
                container.Command.Add("--loglevel");
                container.Command.Add(logLevel);
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
                log.LogDebug(
                    "Deploying ACI group '{AciName}' with {ContainerCount} Linux container(s), image '{ImageName}', managed identity enabled, VNet integration: {VnetIntegrated}",
                    cmdLine.AciArgs.AciName,
                    containerGroupData.Containers.Count,
                    imageName,
                    subnetId.Length > 0);

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
            if(cmdLine.AuthenticationArgs.AuthenticationType != SqlBuildManager.Connection.AuthenticationType.Password)
            {
                cmdLine.AuthenticationArgs.AuthenticationType = SqlBuildManager.Connection.AuthenticationType.ManagedIdentity;
            }
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.AuthType) { Value = cmdLine.AuthenticationArgs.AuthenticationType.ToString() });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.ConcurrencyType) { Value = cmdLine.ConcurrencyType.ToString() });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.AllowObjectDelete) { Value = cmdLine.AllowObjectDelete.ToString() });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.IdentityClientId) { Value = cmdLine.IdentityArgs.ClientId.ToString() });
            if (!string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.IdentityName))
            {
                lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.IdentityName) { Value = cmdLine.IdentityArgs.IdentityName });
            }
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.StorageAccountName) { Value = cmdLine.ConnectionArgs.StorageAccountName });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.EventHubLogging) { Value = string.Join("|",cmdLine.EventHubLogging) });
            lst.Add(new ContainerEnvironmentVariable(ContainerEnvVariables.DatabasePlatform) { Value = cmdLine.AuthenticationArgs.DatabasePlatform.ToString() });
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
                // Any non-404 ARM error means we cannot confirm existence — rethrow so
                // callers see a real failure rather than a false positive.
                log.LogError(rexe, "ARM request failed while checking ACI instance existence (status {Status}): {Message}", rexe.Status, rexe.Message);
                throw;
            }
            catch (Exception ex)
            {
                // Unknown failure — fail closed; do NOT treat as success.
                log.LogError(ex, "Unexpected error while checking ACI instance existence: {Message}", ex.Message);
                throw;
            }
        }

        internal static async Task<bool> DeleteAciInstance(string subscriptionId, string resourceGroupName, string aciName)
        {
            try
            {
                log.LogInformation("Removing any pre-existing ACI deployment");
                var success = await ArmHelper.DeleteResource(subscriptionId, resourceGroupName, aciName);
                //Wait for the delete to complete
                await Task.Delay(ExecutionOptions.ResourceProvisioningPollingInterval);
                log.LogInformation("Pre-existing ACI deployment removed");
                return success;
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to remove existing ACI instance: {exe.Message}");
                return false;
            }
        }

        public static async Task<bool> DeleteAciResources(
            string subscriptionId,
            string resourceGroupName,
            string aciName)
        {
            var containerDeleted = await DeleteAciInstance(subscriptionId, resourceGroupName, aciName);
            var networkProfileDeleted = await DeleteNetworkProfileIfExists(
                subscriptionId,
                resourceGroupName,
                $"{aciName}profile");
            return containerDeleted && networkProfileDeleted;
        }

        private static async Task<bool> DeleteNetworkProfileIfExists(
            string subscriptionId,
            string resourceGroupName,
            string networkProfileName)
        {
            var resourceId =
                $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}" +
                $"/providers/Microsoft.Network/networkProfiles/{networkProfileName}";
            try
            {
                await ArmHelper.DeleteResource(resourceId);
                log.LogInformation("ACI network profile '{NetworkProfileName}' removed", networkProfileName);
                return true;
            }
            catch (RequestFailedException exception) when (exception.Status == 404)
            {
                log.LogDebug("ACI network profile '{NetworkProfileName}' does not exist", networkProfileName);
                return true;
            }
            catch (Exception exception)
            {
                log.LogError(
                    exception,
                    "Unable to remove ACI network profile '{NetworkProfileName}'",
                    networkProfileName);
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
            return ParseAciDeployment(resp);
        }

        internal static Aci.Arm.Deployment ParseAciDeployment(string responseJson)
        {
            return JsonSerializer.Deserialize<Aci.Arm.Deployment>(responseJson)
                ?? throw new JsonException("The ACI deployment response was empty.");
        }

        /// <summary>
        /// Classifies an ARM HTTP status code: 404 means "not found" (returns false),
        /// any other non-success status should be treated as an error (returns null).
        /// Used by <see cref="AciInstanceExists"/> so the decision logic is unit-testable.
        /// </summary>
        internal static bool? ClassifyArmExistenceStatus(int httpStatus)
        {
            if (httpStatus == 404) return false;   // definitive "not found"
            return null;                             // non-404: cannot determine — caller must throw
        }

        #region Container Worker Methods


        #endregion

    }
}
