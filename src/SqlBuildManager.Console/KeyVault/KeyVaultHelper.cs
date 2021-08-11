using Azure.Core;
using Azure.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;
using Azure.Security.KeyVault.Secrets;
using Azure.Security.KeyVault;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;
using SqlBuildManager.Console.CommandLine;
using System.Linq;
using Polly;
using Azure.Messaging.ServiceBus.Administration;

namespace SqlBuildManager.Console.KeyVault
{
    internal class KeyVaultHelper
    {
        public const string BatchAccountKey = "BatchAccountKey";
        public const string StorageAccountKey = "StorageAccountKey";
        public const string StorageAccountName = "StorageAccountName";
        public const string EventHubConnectionString = "EventHubConnectionString";
        public const string ServiceBusTopicConnectionString = "ServiceBusTopicConnectionString";
        public const string UserName = "UserName";
        public const string Password = "Password";
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

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
                        new VisualStudioCodeCredential());
                }
                return _tokenCred;
            }
        }

        private static SecretClient _secretClient = null;
        internal static SecretClient SecretClient(string keyVaultName)
        {

            if(_secretClient == null)
            {
                var cred = KeyVaultHelper.TokenCredential;
                string keyVaultUrl = $"https://{keyVaultName}.vault.azure.net/";
                _secretClient = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: cred);
            }
            return _secretClient;
         
        }

        public static string GetSecret(string keyVaultName, string secretName)
        {
            try
            {
                var pollyRetrySecrets = Policy.Handle<Azure.Identity.AuthenticationFailedException>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.1, retryAttempt)));
                var secret =  pollyRetrySecrets.Execute(() => KeyVaultHelper.SecretClient(keyVaultName).GetSecret(secretName));
                return secret.Value.Value;
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to get secret '{secretName}' from vault {keyVaultName}: {exe.Message}");
                return null;
            }
        }
        public static string SaveSecret(string keyVaultName, string secretName, string secretValue)
        {
            if (string.IsNullOrEmpty(secretValue))
            {
                //log.LogWarning($"Secret value for {secretName} was blank. Will not save to Key Vault {keyVaultName} ");
                return null;
            }
            try
            {
                var secret = KeyVaultHelper.SecretClient(keyVaultName).SetSecret(new KeyVaultSecret(secretName, secretValue));
                log.LogDebug($"Saved value for {secretName} in Key Vault {keyVaultName} ");
                return secret.Value.Name;
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to save secret '{secretName}' to vault {keyVaultName}: {exe.ToString()}");
                return null;
            }
        }
        public static List<string> SaveSecrets(CommandLineArgs cmdLine)
        { 
            var keys = new List<string>();
            var kvName = cmdLine.ConnectionArgs.KeyVaultName;

            keys.Add(SaveSecret(kvName, KeyVaultHelper.EventHubConnectionString, cmdLine.ConnectionArgs.EventHubConnectionString));
            keys.Add(SaveSecret(kvName, KeyVaultHelper.ServiceBusTopicConnectionString, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString));
            keys.Add(SaveSecret(kvName, KeyVaultHelper.StorageAccountKey, cmdLine.ConnectionArgs.StorageAccountKey));
            keys.Add(SaveSecret(kvName, KeyVaultHelper.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountName));
            keys.Add(SaveSecret(kvName, KeyVaultHelper.UserName, cmdLine.AuthenticationArgs.UserName));
            keys.Add(SaveSecret(kvName, KeyVaultHelper.Password, cmdLine.AuthenticationArgs.Password));
            keys.Add(SaveSecret(kvName, KeyVaultHelper.BatchAccountKey, cmdLine.ConnectionArgs.BatchAccountKey));

            return keys.Where(k => !string.IsNullOrWhiteSpace(k)).ToList();

        }
        public static (bool,CommandLineArgs) GetSecrets(CommandLineArgs cmdLine)
        {
            if(string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                log.LogInformation("No Key Vault name supplied. Unable to retrieve secrets");
                return (true,cmdLine);
            }    
            var retrieved = new List<string>();
            log.LogInformation($"Retrieving secrets from KeyVault: {cmdLine.ConnectionArgs.KeyVaultName}");
            var keys = new List<string>();
            var kvName = cmdLine.ConnectionArgs.KeyVaultName;
            string tmp;

            tmp = GetSecret(kvName, KeyVaultHelper.StorageAccountKey);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.StorageAccountKey = tmp;
                retrieved.Add(KeyVaultHelper.StorageAccountKey);
            }
            else { return (false,cmdLine); } //short circuit. Storage key is always needed.

            tmp = GetSecret(kvName, KeyVaultHelper.EventHubConnectionString);
            if(!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.EventHubConnection = tmp;
                retrieved.Add(KeyVaultHelper.EventHubConnectionString);
            }

            tmp = GetSecret(kvName, KeyVaultHelper.ServiceBusTopicConnectionString);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.ServiceBusTopicConnection = tmp;
                retrieved.Add(KeyVaultHelper.ServiceBusTopicConnectionString);
            }

            tmp = GetSecret(kvName, KeyVaultHelper.StorageAccountName);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.StorageAccountName = tmp;
                retrieved.Add(KeyVaultHelper.StorageAccountKey);
            }

            tmp = GetSecret(kvName, KeyVaultHelper.UserName);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.UserName = tmp;
                retrieved.Add(KeyVaultHelper.UserName);
            }

            tmp = GetSecret(kvName, KeyVaultHelper.Password);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.Password = tmp;
                retrieved.Add(KeyVaultHelper.Password);
            }

            tmp = GetSecret(kvName, KeyVaultHelper.BatchAccountKey);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.BatchAccountKey = tmp;
                retrieved.Add(KeyVaultHelper.BatchAccountKey);
            }
            if (retrieved.Count > 0)
            {
                log.LogInformation($"Retrieved secrets from Key Vault: {string.Join(", ", retrieved)}");
                return (true,cmdLine);
            }
            else
            {
                log.LogError($"No secrets received from Key Vault");
                return (false, cmdLine);
            }

            

        }
    }
}
