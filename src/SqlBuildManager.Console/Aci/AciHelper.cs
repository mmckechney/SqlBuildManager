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

namespace SqlBuildManager.Console.Aci
{
    class AciHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        private static string KeyVaultName = "Sbm_KeyVaultName";
        private static string JobName = "Sbm_JobName";
        private static string PackageName = "Sbm_PackageName";
        private static string Concurrency = "Sbm_Concurrency";
        private static string ConcurrencyType = "Sbm_ConcurrencyType";
        private static string DacpacName = "Sbm_DacpacName";

        private static string GetAciResourceId(string subscriptionId, string resourceGroupName, string aciName)
        {
            return $"/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.ContainerInstance/containerGroups/{aciName}";
        }
        private static TokenCredential _tokenCred = null;
        internal static TokenCredential TokenCredential
        {
            get
            {
                if (_tokenCred == null)

                {
                    _tokenCred = new ChainedTokenCredential(
                        new ManagedIdentityCredential(),
                        new AzureCliCredential(),
                        new AzurePowerShellCredential(),
                        new VisualStudioCredential(),
                        new VisualStudioCodeCredential(),
                        new InteractiveBrowserCredential());
                }
                return _tokenCred;
            }
        }

        private static ResourcesManagementClient _resourceClient = null;
        internal static ResourcesManagementClient ResourceClient(string subscriptionId)
        {
            if (_resourceClient == null)
            {
                _resourceClient = new ResourcesManagementClient(subscriptionId, AciHelper.TokenCredential);
            }
            return _resourceClient;
        }
            
        internal static string CreateAciArmTemplate(CommandLineArgs cmdLine)
        {
            string exePath = System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location);
            string pathToTemplates = Path.Combine(exePath, "Aci");
            string template = File.ReadAllText(Path.Combine(pathToTemplates, "aci_arm_template.json"));
            template = template.Replace("{{identityName}}", cmdLine.IdentityArgs.IdentityName);
            template = template.Replace("{{identityResourceGroup}}", cmdLine.IdentityArgs.ResourceGroup);
            template = template.Replace("{{aciName}}", cmdLine.AciArgs.AciName);

            string containerTemplate = File.ReadAllText(Path.Combine(pathToTemplates, "container_template.json"));
            List<string> containers = new List<string>();
            int padding = cmdLine.AciArgs.ContainerCount.ToString().Length;
            for(int i=0;i<cmdLine.AciArgs.ContainerCount;i++)
            {
                var tmpContainer = containerTemplate.Replace("{{counter}}", i.ToString().PadLeft(padding, '0'));
                tmpContainer = tmpContainer.Replace("{{tag}}", cmdLine.AciArgs.ContainerTag);
                tmpContainer = tmpContainer.Replace("{{keyVaultName}}", cmdLine.ConnectionArgs.KeyVaultName);
                tmpContainer = tmpContainer.Replace("{{dacpacName}}", Path.GetFileName(cmdLine.DacPacArgs.PlatinumDacpac));
                tmpContainer = tmpContainer.Replace("{{jobname}}", cmdLine.JobName);
                tmpContainer = tmpContainer.Replace("{{packageName}}", Path.GetFileName(cmdLine.BuildFileName));
                tmpContainer = tmpContainer.Replace("{{concurrency}}", cmdLine.Concurrency.ToString());
                tmpContainer = tmpContainer.Replace("{{concurrencyType}}", cmdLine.ConcurrencyType.ToString());

                containers.Add(tmpContainer);
            }
            string allContainers = string.Join("," + Environment.NewLine, containers);

            template = template.Replace("\"{{Container_Placeholder}}\"", allContainers);
            return template;
        }

        internal static CommandLineArgs ReadRuntimeEnvironmentVariables(CommandLineArgs cmdLine)
        {
            cmdLine.KeyVaultName = Environment.GetEnvironmentVariable(AciHelper.KeyVaultName);
            cmdLine.JobName = Environment.GetEnvironmentVariable(AciHelper.JobName);
            cmdLine.BuildFileName = Environment.GetEnvironmentVariable(AciHelper.PackageName);
            cmdLine.PlatinumDacpac = Environment.GetEnvironmentVariable(AciHelper.DacpacName);
            if (int.TryParse(Environment.GetEnvironmentVariable(AciHelper.Concurrency), out int c))
            {
                cmdLine.Concurrency = c;
            }
            if(Enum.TryParse<ConcurrencyType>(Environment.GetEnvironmentVariable(AciHelper.ConcurrencyType), out ConcurrencyType ct))
            {
                cmdLine.ConcurrencyType = ct;
            }

            return cmdLine;
        }

        internal static async Task<bool> DeployAciInstance(string templateFileName, string subscriptionId, string resourceGroupName, string aciName, string jobName)
        {
            try
            {
                if (await ShouldDeleteExistingInstance(templateFileName, subscriptionId, resourceGroupName, aciName))
                {
                    await DeleteAciInstance(subscriptionId,resourceGroupName,aciName);
                }

                // https://github.com/Azure-Samples/azure-samples-net-management/blob/master/samples/resources/deploy-using-arm-template/Program.cs
                var rgName = resourceGroupName;
                var deploymentName = jobName;
                var templateFileContents = File.ReadAllText(templateFileName);
                var deployments = ResourceClient(subscriptionId).Deployments;

                log.LogInformation($"Starting a deployment for ACI '{aciName}' for job: {deploymentName}");

                var parameters = new Deployment
                (
                    new DeploymentProperties(DeploymentMode.Incremental)
                    {
                        Template = templateFileContents,
                        Parameters = "{}"
                    }
                 );
                var rawResult = await deployments.StartCreateOrUpdateAsync(rgName, deploymentName, parameters);
                await rawResult.WaitForCompletionAsync();

                //Introduce a delay to see if that will help with what seems to be an issue with a timely managed identity assignment
                Thread.Sleep(10000);

                log.LogInformation("Completed ACI deployment: " + deploymentName);
                return true;
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
                (int containerCount, int memory, int cpu) = await GetAciCountMemoryAndCpu(subscriptionId, resourceGroupName, aciName);

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
            string aciResourceId = GetAciResourceId(subscriptionId, resourceGroupName, aciName);
            //var resp = await ResourceClient(subscriptionId).Resources.CheckExistenceByIdAsync(aciResourceId, "2021-03-01");
            try
            {
                
                log.LogInformation("Removing any pre-existing ACI deployment");
                var resp = await ResourceClient(subscriptionId).Resources.StartDeleteByIdAsync(aciResourceId, "2021-03-01");
                await resp.WaitForCompletionAsync();
                log.LogInformation("Pre-existing ACI deployment removed");
            }
            catch(Exception exe)
            {
                log.LogError($"Unable to remove existing ACI instance: {exe.Message}");
                return false;
            }
            return true;
        }

        internal static async Task<bool> AciIsInErrorState(string subscriptionId, string resourceGroupName, string aciName)
        {
            var aciResult = await GetAciInstanceData(subscriptionId, resourceGroupName, aciName);
            var containerCount = aciResult.Value.Properties.Containers.Count;
            var status = aciResult.Value.Properties.Containers.Where(c => c.Properties.InstanceView.CurrentState.DetailStatus.ToLower() == "error").Count();

            return status == containerCount;
        }

        internal static async Task<(int,int, int)> GetAciCountMemoryAndCpu(string subscriptionId, string resourceGroupName, string aciName)
        { 
            var aciResult = await GetAciInstanceData( subscriptionId,  resourceGroupName, aciName);
            var containerCount = aciResult.Value.Properties.Containers.Count;
            var memoryPer = aciResult.Value.Properties.Containers.Select(c => c.Properties.Resources.Requests.MemoryInGB).FirstOrDefault();
            var cpuPer = aciResult.Value.Properties.Containers.Select(c => c.Properties.Resources.Requests.Cpu).FirstOrDefault();

            return(containerCount, memoryPer, cpuPer);
        }

        private static async Task<AciDeploymentResult.Result> GetAciInstanceData(string subscriptionId, string resourceGroupName, string aciName)
        {
            string aciResourceId = GetAciResourceId(subscriptionId, resourceGroupName, aciName);

            var resp = await ResourceClient(subscriptionId).Resources.GetByIdAsync(aciResourceId, "2021-03-01");
            var aciResult = JsonSerializer.Deserialize<AciDeploymentResult.Result>(JsonSerializer.Serialize(resp));

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
