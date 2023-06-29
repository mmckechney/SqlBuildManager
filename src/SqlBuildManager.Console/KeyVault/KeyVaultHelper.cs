using Azure.Identity;
using Azure.Security.KeyVault.Secrets;
using Microsoft.Extensions.Logging;
using Polly;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.CommandLine;
using System;
using System.Collections.Generic;
using System.Linq;

namespace SqlBuildManager.Console.KeyVault
{
    public class KeyVaultHelper
    {
        public const string BatchAccountKey = "BatchAccountKey";
        public const string StorageAccountKey = "StorageAccountKey";
        public const string StorageAccountName = "StorageAccountName";
        public const string EventHubConnectionString = "EventHubConnectionString";
        public const string ServiceBusTopicConnectionString = "ServiceBusTopicConnectionString";
        public const string UserName = "UserName";
        public const string Password = "Password";
        public const string ContainerRegistryPassword = "ContainerRegistryPassword";
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);


        private static SecretClient _secretClient = null;
        internal static SecretClient SecretClient(string keyVaultName)
        {

            if (_secretClient == null)
            {
                var cred = AadHelper.TokenCredential;
                string keyVaultUrl = $"https://{keyVaultName}.vault.azure.net/";
                _secretClient = new SecretClient(vaultUri: new Uri(keyVaultUrl), credential: cred);
            }
            return _secretClient;

        }

        public static string GetSecret(string keyVaultName, string secretName)
        {
            try
            {
                var pollyRetrySecrets = Policy.Handle<Azure.Identity.AuthenticationFailedException>().WaitAndRetry(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(1.3, retryAttempt)));
                var secret = pollyRetrySecrets.Execute(() => KeyVaultHelper.SecretClient(keyVaultName).GetSecret(secretName));
                return secret.Value.Value;
            }
            catch (Azure.RequestFailedException rfe)
            {
                log.LogWarning($"Unable to get secret '{secretName}' from vault {keyVaultName}: [RequestFailedException] {rfe.ErrorCode}");
                return null;
            }
            catch (AuthenticationFailedException afe)
            {
                log.LogError($"Unable to get secret '{secretName}' from vault {keyVaultName}: [AuthenticationFailedException] {afe.Message}");
                return null;
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to get secret '{secretName}' from vault {keyVaultName}:[{exe.GetType()}] {exe.Message}");
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

            if (ConnectionStringValidator.IsEventHubConnectionString(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                keys.Add(SaveSecret(kvName, KeyVaultHelper.EventHubConnectionString, cmdLine.ConnectionArgs.EventHubConnectionString));
            }

            if (ConnectionStringValidator.IsServiceBusConnectionString(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                keys.Add(SaveSecret(kvName, KeyVaultHelper.ServiceBusTopicConnectionString, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString));
            }

            keys.Add(SaveSecret(kvName, KeyVaultHelper.StorageAccountKey, cmdLine.ConnectionArgs.StorageAccountKey));
            keys.Add(SaveSecret(kvName, KeyVaultHelper.StorageAccountName, cmdLine.ConnectionArgs.StorageAccountName));


            keys.Add(SaveSecret(kvName, KeyVaultHelper.UserName, cmdLine.AuthenticationArgs.UserName));
            keys.Add(SaveSecret(kvName, KeyVaultHelper.Password, cmdLine.AuthenticationArgs.Password));

            
            keys.Add(SaveSecret(kvName, KeyVaultHelper.BatchAccountKey, cmdLine.ConnectionArgs.BatchAccountKey));
            if (cmdLine.ContainerRegistryArgs != null)
            {
                keys.Add(SaveSecret(kvName, KeyVaultHelper.ContainerRegistryPassword, cmdLine.ContainerRegistryArgs.RegistryPassword));
            }

            return keys.Where(k => !string.IsNullOrWhiteSpace(k)).ToList();

        }
        public static (bool, CommandLineArgs) GetSecrets(CommandLineArgs cmdLine)
        {
            if (cmdLine.KeyVaultSecretsRetrieved)
            {
                log.LogDebug("KeyVault Secrets already retrieved");
                return (true, cmdLine);
            }
            
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                log.LogInformation("No Key Vault name supplied. Unable to retrieve secrets");
                return (true, cmdLine);
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
            else
            {
                //short circuit. Storage key is always needed.
                return (false, cmdLine);
            }

            tmp = GetSecret(kvName, KeyVaultHelper.EventHubConnectionString);
            if (!string.IsNullOrWhiteSpace(tmp))
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
                retrieved.Add(KeyVaultHelper.StorageAccountName);
            }



            tmp = GetSecret(kvName, KeyVaultHelper.ContainerRegistryPassword);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.RegistryPassword = tmp;
                retrieved.Add(KeyVaultHelper.ContainerRegistryPassword);
            }

            if (cmdLine.AuthenticationArgs.AuthenticationType != SqlSync.Connection.AuthenticationType.ManagedIdentity)
            {
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
            }
            if (cmdLine.BatchArgs != null && !string.IsNullOrWhiteSpace(cmdLine.BatchArgs.BatchPoolName))
            {
                tmp = GetSecret(kvName, KeyVaultHelper.BatchAccountKey);
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    cmdLine.BatchAccountKey = tmp;
                    retrieved.Add(KeyVaultHelper.BatchAccountKey);
                }
            }
            if (retrieved.Count > 0)
            {
                cmdLine.KeyVaultSecretsRetrieved = true;
                log.LogInformation($"Retrieved secrets from Key Vault: {string.Join(", ", retrieved)}");
                return (true, cmdLine);
            }
            else
            {
                log.LogError($"No secrets received from Key Vault");
                return (false, cmdLine);
            }



        }
    }
}
