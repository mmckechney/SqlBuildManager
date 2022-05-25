using Azure.ResourceManager;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SqlBuildManager.Console.Aad;
using Azure.ResourceManager.Resources;
using Azure.Core.Extensions;
using Azure.Core;
using Microsoft.SqlServer.Management.Smo;
using Azure.ResourceManager.Resources.Models;
using Azure;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.Management.Fluent;
using Microsoft.Azure.Management.ResourceManager.Fluent;
using Microsoft.Azure.Management.ResourceManager;
using Microsoft.Azure.Management.ResourceManager.Fluent.Core;
using Microsoft.Azure.Management.ResourceManager.Fluent.Authentication;
using Microsoft.Azure.Management.ContainerInstance.Fluent;
using System.Net.Http;
using System.Threading;
using System.Drawing;

namespace SqlBuildManager.Console.Arm
{
    public class ArmHelper
    {
        #region Private

        internal static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger<ArmHelper>();
        private static ArmClient _sbmarmClient = null;
        private static ArmClient SbmArmClient { 
            get 
            { 
                if(_sbmarmClient == null)
                {
                    _sbmarmClient = new ArmClient(AadHelper.TokenCredential);
                }
                return _sbmarmClient;
            }
        }

        private static SubscriptionResource GetSubscriptionResource(string subscriptionId)
        {
            var subResid = SubscriptionResource.CreateResourceIdentifier(subscriptionId);
            SubscriptionResource subscription = SbmArmClient.GetSubscriptionResource(subResid);
            return subscription;
        }

        private static ResourceGroupResource GetResourceGroup(string subscriptionId, string resourceGroupName)
        {
            var sub = GetSubscriptionResource(subscriptionId);
            var rg = sub.GetResourceGroup(resourceGroupName);

            return rg;
        }
        private static ResourceGroupResource GetResourceGroup(SubscriptionResource sub, string resourceGroupName)
        {
            ResourceGroupResource rg =  sub.GetResourceGroup(resourceGroupName);
            return rg;
        }
        #endregion
        public static GenericResource GetResourceByName(string subscriptionId, string resourceGroupName, string resourceName)
        {
            var rg = GetResourceGroup(subscriptionId, resourceGroupName);
            var res = rg.GetGenericResources();
            var resource = res.Where(r => r.Id.Name.ToLower() == resourceName.ToLower()).FirstOrDefault();
            return resource;
        }


        public static async Task<bool> SubmitDeployment(string subscriptionId, string resourceGroupName, string templateFileContents, string parameters, string deploymentName)
        {
            try
            {
                var sub = GetSubscriptionResource(subscriptionId);
                var rg = GetResourceGroup(sub, resourceGroupName);
                var deployments = rg.GetArmDeployments();

                var input = new ArmDeploymentContent(new ArmDeploymentProperties(ArmDeploymentMode.Incremental)
                {
                    Template = BinaryData.FromString(templateFileContents),
                    Parameters = BinaryData.FromString(parameters)
                });
                ArmOperation<ArmDeploymentResource> lro = await deployments.CreateOrUpdateAsync(WaitUntil.Completed, deploymentName, input);
                ArmDeploymentResource dep = lro.Value;
                return true;
            }
            catch(Exception exe)
            {
                log.LogError($"Deployment '{deploymentName}' failed. {exe.Message}");
                return false;
            }
        }
        public static async Task<bool> DeleteResource(string resourceId)
        {
            //var armResource = new ArmClient(AadHelper.TokenCredential).GetArmDeploymentResource(new ResourceIdentifier(resourceId));
            var armResource = new ArmClient(AadHelper.TokenCredential).GetGenericResource(new ResourceIdentifier(resourceId));
            var result = await armResource.DeleteAsync(WaitUntil.Completed);
            while (!result.HasCompleted)
            {
                result.UpdateStatus();
                Thread.Sleep(1000);
            }
            if(result.GetRawResponse().Status >= 300)
            {
                log.LogError($"Unable to delete resource {resourceId}. {result.GetRawResponse().ReasonPhrase}");
                return false;
            }
            else
            {
                log.LogInformation($"Successfully deleted resource {resourceId}");
                return true;
            }
        }
        public static async Task<bool> DeleteResource(string subscriptionId, string resourceGroupName, string resourceName)
        {
            try
            {
                var res = GetResourceByName(subscriptionId, resourceGroupName, resourceName);
                if(res != null)
                {
                    await res.DeleteAsync(WaitUntil.Completed);
                }

                return true;
            }
            catch (Exception exe)
            {
                log.LogError($"Deletion of resource '{resourceName}' failed. {exe.Message}");
                return false;
            }
        }

        public static async Task<string> GetAciDeploymentDetails(string subscriptionId, string resourceGroupName,string aciName)
        {
            try
            {
                var cancelSource = new CancellationTokenSource();
                var tokenObj = AadHelper.TokenCredential.GetToken(new TokenRequestContext(new string[] { "https://management.azure.com/" }), cancelSource.Token);
                var url = $"https://management.azure.com/subscriptions/{subscriptionId}/resourceGroups/{resourceGroupName}/providers/Microsoft.ContainerInstance/containerGroups/{aciName}?api-version=2021-09-01";
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.Add("Authorization", "Bearer " + tokenObj.Token);
                    var returnVal = await client.GetAsync(url);
                    if (returnVal.IsSuccessStatusCode)
                    {
                        var details = returnVal.Content.ReadAsStringAsync();
                        return details.Result;
                    }else
                    {
                        log.LogError($"Unable to retrieve the status of ACI deployment {aciName}. Status code: {returnVal.StatusCode}");
                    }
                }
                return string.Empty;
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to retrieve the status of ACI deployment {aciName}. {exe.Message}");
                return string.Empty; ;
            }
        }

    }
}
