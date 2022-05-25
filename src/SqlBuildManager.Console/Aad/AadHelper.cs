using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.HadrModel;
using Microsoft.SqlServer.Management.Smo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Aad
{
    internal class AadHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static string _managedIdentityClientId = string.Empty;
        internal static string ManagedIdentityClientId
        {
            get
            {
                return _managedIdentityClientId;
            }
            set
            {
                _managedIdentityClientId = value;
                log.LogInformation($"ManagedIdentityClientId value set to {_managedIdentityClientId}");
            }
        }
        private static TokenCredential _tokenCred = null;
        internal static TokenCredential TokenCredential
        {
            get
            {

                if (_tokenCred == null)
                {

                    if (string.IsNullOrWhiteSpace(AadHelper.ManagedIdentityClientId))
                    {
                        _tokenCred = new DefaultAzureCredential();
                        log.LogInformation("Creating DefaultAzureCredential, no ManagedIdentityClientId specified");
                    }
                    else
                    {
                        _tokenCred = new DefaultAzureCredential(
                            new DefaultAzureCredentialOptions() {
                                ManagedIdentityClientId = AadHelper.ManagedIdentityClientId,
                                ExcludeAzureCliCredential = false 
                            });
                        log.LogInformation($"Creating DefaultAzureCredential with ManagedIdentityClientId of: '{AadHelper.ManagedIdentityClientId}'");
                    }
                }
                return _tokenCred;
            }
        }
    }
}
