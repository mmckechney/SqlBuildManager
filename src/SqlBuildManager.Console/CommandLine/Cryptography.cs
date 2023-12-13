using Microsoft.Extensions.Logging;
using System;
using System.IO;
using sqlB = SqlSync.SqlBuild;
namespace SqlBuildManager.Console.CommandLine
{

    /// <summary>
    /// http://www.codeproject.com/Articles/769741/Csharp-AES-bits-Encryption-Library-with-Salt
    /// </summary>
    public static class Cryptography
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        private static readonly string keyEnvronmentVariableName = "sbm-settingsfilekey";

        public static CommandLineArgs EncryptSensitiveFields(CommandLineArgs cmdLine)
        {
            //Don't double in encrypt..
            bool tmp;
            (tmp, cmdLine) = DecryptSensitiveFields(cmdLine, true);
            (bool success, string key) = GetSettingsFileEncryptionKey(cmdLine);
            if(!success && string.IsNullOrEmpty(key))
            {
                log.LogError("Unable to encrypt sensitive fields. No encryption key found.");
                return cmdLine;
            }


            if (cmdLine.ContainerRegistryArgs != null && !string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryPassword))
            {
                cmdLine.ContainerRegistryArgs.RegistryPassword = sqlB.Cryptography.EncryptText(cmdLine.ContainerRegistryArgs.RegistryPassword, key);
            }

            if (cmdLine.AuthenticationArgs != null && !string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName))
            {
                cmdLine.AuthenticationArgs.UserName = sqlB.Cryptography.EncryptText(cmdLine.AuthenticationArgs.UserName, key);
            }

            if (cmdLine.AuthenticationArgs != null && !string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                cmdLine.AuthenticationArgs.Password = sqlB.Cryptography.EncryptText(cmdLine.AuthenticationArgs.Password, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.BatchAccountKey))
            {
                cmdLine.ConnectionArgs.BatchAccountKey = sqlB.Cryptography.EncryptText(cmdLine.ConnectionArgs.BatchAccountKey, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
            {
                cmdLine.ConnectionArgs.StorageAccountKey = sqlB.Cryptography.EncryptText(cmdLine.ConnectionArgs.StorageAccountKey, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString) && cmdLine.ConnectionArgs.EventHubConnectionString.Trim().ToLower().StartsWith("endpoint="))
            {
                cmdLine.ConnectionArgs.EventHubConnectionString = sqlB.Cryptography.EncryptText(cmdLine.ConnectionArgs.EventHubConnectionString, key);
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString) && cmdLine.ConnectionArgs.ServiceBusTopicConnectionString.Trim().ToLower().StartsWith("endpoint="))
            {
                cmdLine.ConnectionArgs.ServiceBusTopicConnectionString = sqlB.Cryptography.EncryptText(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, key);
            }


            return cmdLine;
        }

        public static (bool, CommandLineArgs) DecryptSensitiveFields(CommandLineArgs cmdLine, bool suppressLog = false)
        {
            if (cmdLine.Decrypted)
            {
                log.LogDebug("Command line arguments already decrypted.");
                return (true, cmdLine);
            }
            //Nothing to do if none of the settings came from a settings file!
            if (string.IsNullOrWhiteSpace(cmdLine.SettingsFile))
            {
                return (true, cmdLine);
            }
            bool consolidated = true;

            //Look for a encryption key in the settings file, if not found, and there is a value for keyvault, then skip decryption and return true
            (bool success, string key) = GetSettingsFileEncryptionKey(cmdLine);
            if (!success && string.IsNullOrEmpty(key))
            {
                if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
                {
                    log.LogError("Unable to decrypt sensitive fields. No encryption key found.");
                    return (false, cmdLine);
                }
                else
                {
                    log.LogInformation("No SettingsFileKey found, but KeyVaultName was provided. Assuming Key Vault will be used to retrieve sensitive fields.");
                    return (true, cmdLine);
                }
            }

            if (cmdLine.ContainerRegistryArgs != null && !string.IsNullOrWhiteSpace(cmdLine.ContainerRegistryArgs.RegistryPassword))
            {
                (success, cmdLine.ContainerRegistryArgs.RegistryPassword) = sqlB.Cryptography.DecryptText(cmdLine.ContainerRegistryArgs.RegistryPassword, key, "--registrypassword", suppressLog);
                consolidated = consolidated & success;
            }

            if (cmdLine.AuthenticationArgs != null && !string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.UserName) && string.IsNullOrWhiteSpace(cmdLine.IdentityArgs.ClientId)) //if there is a managed Id then we not need to decrypt the user name
            {
                (success, cmdLine.AuthenticationArgs.UserName) = sqlB.Cryptography.DecryptText(cmdLine.AuthenticationArgs.UserName, key, "--username", suppressLog);
                consolidated = consolidated & success;
            }
            if (cmdLine.AuthenticationArgs != null && !string.IsNullOrWhiteSpace(cmdLine.AuthenticationArgs.Password))
            {
                (success, cmdLine.AuthenticationArgs.Password) = sqlB.Cryptography.DecryptText(cmdLine.AuthenticationArgs.Password, key, "--password", suppressLog);
                consolidated = consolidated & success;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.BatchAccountKey))
            {
                (success, cmdLine.ConnectionArgs.BatchAccountKey) = sqlB.Cryptography.DecryptText(cmdLine.ConnectionArgs.BatchAccountKey, key, "--batchaccountkey", suppressLog);
                consolidated = consolidated & success;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.StorageAccountKey))
            {
                (success, cmdLine.ConnectionArgs.StorageAccountKey) = sqlB.Cryptography.DecryptText(cmdLine.ConnectionArgs.StorageAccountKey, key, "--storageaccountkey", suppressLog);
                consolidated = consolidated & success;
            }


            //Ignore possible decryption errors from EH and SB. They may be just the plain text namesspaces 
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.EventHubConnectionString))
            {
                (success, cmdLine.ConnectionArgs.EventHubConnectionString) = sqlB.Cryptography.DecryptText(cmdLine.ConnectionArgs.EventHubConnectionString, key, "--eventhubconnection", suppressLog);
                // consolidated = consolidated & success;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                (success, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString) = sqlB.Cryptography.DecryptText(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString, key, "--servicebustopicconnection", suppressLog);
                //consolidated = consolidated & success;
            }


            if (consolidated)
            {
                cmdLine.Decrypted = true;
            }
            return (consolidated, cmdLine);
        }


        private static (bool success, string key) GetSettingsFileEncryptionKey(CommandLineArgs cmdLine)
        {
            string encryptionKey = string.Empty;
            if (!string.IsNullOrWhiteSpace(cmdLine.SettingsFileKey))
            {
                if (File.Exists(Path.GetFullPath(cmdLine.SettingsFileKey)))
                {
                    encryptionKey = File.ReadAllText(cmdLine.SettingsFileKey).Trim();
                }
                else
                {
                    encryptionKey = cmdLine.SettingsFileKey;
                }
            }

            if (encryptionKey.Length == 0)
            {
                var ev = Environment.GetEnvironmentVariable(keyEnvronmentVariableName);
                if (!string.IsNullOrWhiteSpace(ev))
                {
                    encryptionKey = ev;
                }
            }


            if(encryptionKey.Length == 0)
            {
                return (false, encryptionKey);
            }
            else
            {
               return (true, encryptionKey);
            }
        }
        private static readonly string store = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Sql Build Manager", "sbm-store.txt");

    }
}
