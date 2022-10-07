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
                if (!string.IsNullOrEmpty(_managedIdentityClientId))
                {
                    log.LogInformation($"ManagedIdentityClientId value set to {_managedIdentityClientId}");
                }
            }
        }
        private static string _tenantId = String.Empty;
        public static string TenantId
        {
            get => _tenantId; set
            {
                _tenantId = value;
                if (!string.IsNullOrEmpty(_tenantId))
                {
                    log.LogInformation($"TenantId value set to {_tenantId}");
                }
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
                        if (!string.IsNullOrEmpty(AadHelper.TenantId))
                        {
                            log.LogInformation($"Creating DefaultAzureCredential for Tenant '{AadHelper.TenantId}', no ManagedIdentityClientId specified");
                            _tokenCred = new DefaultAzureCredential(new DefaultAzureCredentialOptions() { TenantId = AadHelper.TenantId });
                        }
                        else
                        {
                            log.LogInformation("Creating DefaultAzureCredential, no ManagedIdentityClientId specified");
                            _tokenCred = new DefaultAzureCredential();
                        }
                        
                    }
                    else
                    {
                        AzureCliCredentialOptions cliOpts = new AzureCliCredentialOptions();
                        AzurePowerShellCredentialOptions pwshOpts = new AzurePowerShellCredentialOptions();
                        if (!string.IsNullOrEmpty(AadHelper.TenantId))
                        {
                            cliOpts.TenantId = AadHelper.TenantId;
                            pwshOpts.TenantId = AadHelper.TenantId;
                        }
                        
                        _tokenCred = new ChainedTokenCredential(new AzureCliCredential(cliOpts), new ManagedIdentityCredential(ManagedIdentityClientId = AadHelper.ManagedIdentityClientId), new AzurePowerShellCredential(pwshOpts));
                        log.LogInformation($"Creating ChainedTokenCredential with ManagedIdentityClientId of: '{AadHelper.ManagedIdentityClientId}'");
                    }
                }
                return _tokenCred;
            }
        }

        
    }
}
