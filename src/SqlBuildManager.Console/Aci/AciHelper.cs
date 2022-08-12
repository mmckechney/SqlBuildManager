using SqlBuildManager.Console.CommandLine;
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
using System.Linq;
using SqlBuildManager.Console.Shared;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.Arm;
using SqlBuildManager.Console.Aci.Arm;
using Microsoft.Azure.Management.ContainerRegistry.Fluent;
using SqlSync.Connection;

namespace SqlBuildManager.Console.Aci
{
    class AciHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        internal static string CreateAciArmTemplate(CommandLineArgs cmdLine)
        {
            //TODO: Accomodate custom container repositories
            string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string pathToTemplates = Path.Combine(exePath, "Aci");
            string template = File.ReadAllText(Path.Combine(pathToTemplates, "aci_arm_template.json"));
            template = template.Replace("{{identityName}}", cmdLine.IdentityArgs.IdentityName);
            template = template.Replace("{{identityResourceGroup}}", cmdLine.IdentityArgs.ResourceGroup);
            template = template.Replace("{{aciName}}", cmdLine.AciArgs.AciName);

            string credsTemplate = "";
            if (!string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryServer))
            {
                credsTemplate = File.ReadAllText(Path.Combine(pathToTemplates, "registryCredentialsSnippet.txt"));
                credsTemplate = credsTemplate.Replace("{{registryServer}}", cmdLine.ContainerRegistryArgs.RegistryServer);
                credsTemplate = credsTemplate.Replace("{{registryPassword}}", cmdLine.ContainerRegistryArgs.RegistryPassword);
                credsTemplate = credsTemplate.Replace("{{registryUserName}}", cmdLine.ContainerRegistryArgs.RegistryUserName);
}
            template = template.Replace("\"{{registryCredentials}}\"", credsTemplate);

            string containerTemplate = File.ReadAllText(Path.Combine(pathToTemplates, "container_template.json"));
            List<string> containers = new List<string>();
            int padding = cmdLine.AciArgs.ContainerCount.ToString().Length;
            for(int i=0;i<cmdLine.AciArgs.ContainerCount;i++)
            {
                var tmpContainer = containerTemplate.Replace("{{counter}}", i.ToString().PadLeft(padding, '0'));
                tmpContainer = tmpContainer.Replace("{{tag}}", cmdLine.ContainerRegistryArgs.ImageTag);
                tmpContainer = tmpContainer.Replace("{{keyVaultName}}", cmdLine.ConnectionArgs.KeyVaultName);
                tmpContainer = tmpContainer.Replace("{{dacpacName}}", Path.GetFileName(cmdLine.DacPacArgs.PlatinumDacpac));
                tmpContainer = tmpContainer.Replace("{{jobname}}", cmdLine.JobName);
                tmpContainer = tmpContainer.Replace("{{packageName}}", Path.GetFileName(cmdLine.BuildFileName));
                tmpContainer = tmpContainer.Replace("{{concurrency}}", cmdLine.Concurrency.ToString());
                tmpContainer = tmpContainer.Replace("{{concurrencyType}}", cmdLine.ConcurrencyType.ToString());
                tmpContainer = tmpContainer.Replace("{{identityClientId}}", cmdLine.IdentityArgs.ClientId.ToString());
                tmpContainer = tmpContainer.Replace("{{allowObjectDelete}}", cmdLine.AllowObjectDelete.ToString());
                tmpContainer = tmpContainer.Replace("{{authType}}", cmdLine.AuthenticationArgs.AuthenticationType.ToString());
                tmpContainer = tmpContainer.Replace("{{eventHub}}", cmdLine.ConnectionArgs.EventHubConnectionString);
                tmpContainer = tmpContainer.Replace("{{serviceBus}}", cmdLine.ConnectionArgs.ServiceBusTopicConnectionString);


                if (string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryServer))
                {
                    tmpContainer = tmpContainer.Replace("{{registryServer}}", "ghcr.io/mmckechney");
                }
                else
                {
                    tmpContainer = tmpContainer.Replace("{{registryServer}}", cmdLine.ContainerRegistryArgs.RegistryServer);
                }

                if (string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.ImageName))
                {
                    tmpContainer = tmpContainer.Replace("{{imagename}}", "sqlbuildmanager");
                }
                else
                {
                    tmpContainer = tmpContainer.Replace("{{imagename}}", cmdLine.ContainerRegistryArgs.ImageName);
                }

                containers.Add(tmpContainer);
            }
            string allContainers = string.Join("," + Environment.NewLine, containers);

            template = template.Replace("\"{{Container_Placeholder}}\"", allContainers);
            return template;
        }

        internal static async Task<bool> DeployAciInstance(string templateFileName, string subscriptionId, string resourceGroupName, string aciName, string jobName)
        {
            try
            {
                if (await ShouldDeleteExistingInstance(templateFileName, subscriptionId, resourceGroupName, aciName))
                {
                    await DeleteAciInstance(subscriptionId,resourceGroupName,aciName);
                }
                var rgName = resourceGroupName;
                var deploymentName = jobName;
                var templateFileContents = File.ReadAllText(templateFileName);

                log.LogInformation($"Starting a deployment for ACI '{aciName}' for job: {deploymentName}");
                bool success = await ArmHelper.SubmitDeployment(subscriptionId, rgName, templateFileContents, "{}", deploymentName);

                log.LogInformation($"Completed ACI deployment: {deploymentName}. Waiting for identity assignment completion....");


                //Introduce a delay to see if that will help with what seems to be an issue with a timely managed identity assignment
                Thread.Sleep(10000);

                log.LogInformation("Completed ACI deployment");
                return success;
            }
            catch(Exception ex)
            {
                log.LogError($"Unable to deploy ACI instance: {ex.Message}");
                return false;
            }

        }

        private static async Task<bool> ShouldDeleteExistingInstance(string templateFileName, string subscriptionId, string resourceGroupName, string aciName)
        {
            try
            {
                (int containerCount, double memory, int cpu) = await GetAciCountMemoryAndCpu(subscriptionId, resourceGroupName, aciName);

                var j = JsonSerializer.Deserialize<TemplateClass>(File.ReadAllText(templateFileName));
                var templateContainerCount = j.Resources.First().Properties.Containers.Count();
                var templateMemory = j.Resources.First().Properties.Containers.First().Properties.Resources.Requests.MemoryInGB;
                var templateCpu = j.Resources.First().Properties.Containers.First().Properties.Resources.Requests.Cpu;

                return !(containerCount == templateContainerCount && memory == templateMemory && cpu == templateCpu);
            }
            catch(Azure.RequestFailedException rexe)
            {
                if(rexe.Status == 404)
                {
                    return false;
                }
                else
                {
                    log.LogError(rexe.Message);
                    return true;
                }
            }
            catch(Exception ex)
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
                log.LogInformation("Pre-existing ACI deployment removed");
                return success; 
            }
            catch(Exception exe)
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

        internal static async Task<(int,double, int)> GetAciCountMemoryAndCpu(string subscriptionId, string resourceGroupName, string aciName)
        {
            var aciResult = await GetAciInstanceData(subscriptionId, resourceGroupName, aciName);
            var containerCount = aciResult.Properties.Containers.Count;
            var memoryPer = aciResult.Properties.Containers.Select(c => c.Properties.Resources.Requests.MemoryInGB).FirstOrDefault();
            var cpuPer = aciResult.Properties.Containers.Select(c => c.Properties.Resources.Requests.Cpu).FirstOrDefault();

            return (containerCount, memoryPer, cpuPer);
        }

        private static async Task<Aci.Arm.Deployment> GetAciInstanceData(string subscriptionId, string resourceGroupName, string aciName)
        {
            var resp = await ArmHelper.GetAciDeploymentDetails(subscriptionId, resourceGroupName, aciName);
            var aciResult = JsonSerializer.Deserialize<Aci.Arm.Deployment>(JsonSerializer.Serialize(resp));

            return aciResult;
        }

        internal static CommandLineArgs GetRuntimeValuesFromDeploymentTempate(CommandLineArgs cmdLine, string templateFileName)
        {
            var j = System.Text.Json.JsonSerializer.Deserialize<Aci.TemplateClass>(File.ReadAllText(templateFileName));
            cmdLine.JobName = j.Resources[0].Properties.Containers[0].Properties.EnvironmentVariables.Where(e => e.Name == "Sbm_JobName").FirstOrDefault().Value;
            var tmp = j.Resources[0].Properties.Containers[0].Properties.EnvironmentVariables.Where(e => e.Name == "Sbm_ConcurrencyType").FirstOrDefault().Value;
            if (Enum.TryParse<ConcurrencyType>(tmp, out ConcurrencyType concurrencyType))
            {
                cmdLine.ConcurrencyType = concurrencyType;
            }
            cmdLine.AciName = j.variables.aciName;

            return cmdLine;
        }


    }
}
