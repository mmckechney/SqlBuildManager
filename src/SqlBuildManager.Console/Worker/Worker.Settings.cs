using Microsoft.Extensions.Logging;
using System.Text.Json;
using System.Text.Json.Serialization;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.KeyVault;
using SqlSync.Connection;
using System;
using System.IO;

namespace SqlBuildManager.Console
{
    internal partial class Worker
    {
        internal static int SaveAndEncryptSettings(CommandLineArgs cmdLine, bool clearText)
        {

            if (string.IsNullOrWhiteSpace(cmdLine.SettingsFile))
            {
                log.LogError("When 'sbm batch/aci/containerapp/k8s savesettings' is specified the --settingsfile argument is also required");
                return -3;
            }
            if (!string.IsNullOrWhiteSpace(cmdLine.SettingsFileKey) && cmdLine.SettingsFileKey.Length < 16)
            {
                log.LogError("The value for the --settingsfilekey must be at least 16 characters long");
                return -4;
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                var lst = KeyVaultHelper.SaveSecrets(cmdLine);
                log.LogInformation($"Saved secrets to Azure Key Vault {cmdLine.ConnectionArgs.KeyVaultName}: {string.Join(", ", lst)}");
            }

            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                //remove secrets from the command line so they are not saved to the config.
                cmdLine.AuthenticationArgs.UserName = null;
                cmdLine.AuthenticationArgs.Password = null;
                cmdLine.ConnectionArgs.BatchAccountKey = null;
                cmdLine.ConnectionArgs.StorageAccountKey = null;
                if (cmdLine.ContainerRegistryArgs != null)
                {
                    cmdLine.ContainerRegistryArgs.RegistryPassword = null;
                }

                if (ConnectionStringValidator.IsEventHubConnectionString(cmdLine.ConnectionArgs.EventHubConnectionString))
                {
                    cmdLine.ConnectionArgs.EventHubConnectionString = null;
                }

                if (ConnectionStringValidator.IsServiceBusConnectionString(cmdLine.ConnectionArgs.ServiceBusTopicConnectionString))
                {
                    if (cmdLine.ContainerAppArgs == null || string.IsNullOrWhiteSpace(cmdLine.ContainerAppArgs.EnvironmentName))
                    {
                        //will need this for KEDA in ContainerApps, so only remove if NOT for ContainerApps
                        cmdLine.ConnectionArgs.ServiceBusTopicConnectionString = null;

                    }
                }
            }


            //Clear out username and password is AuthenticationType is ManagedIdentity
            if (cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.ManagedIdentity)
            {
                cmdLine.AuthenticationArgs.UserName = null;
                cmdLine.AuthenticationArgs.Password = null;
            }

            if (!clearText)
            {
                cmdLine = Cryptography.EncryptSensitiveFields(cmdLine);
            }

            var mystuff = JsonSerializer.Serialize<CommandLineArgs>(cmdLine.NullEmptyStrings(), new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull | JsonIgnoreCondition.WhenWritingDefault
            });


            try
            {
                string write = "y";
                if (File.Exists(cmdLine.SettingsFile) && !cmdLine.Silent)
                {
                    System.Console.WriteLine($"The settings file '{cmdLine.SettingsFile}' already exists. Overwrite (Y/N)?");
                    write = System.Console.ReadLine();
                }

                if (write.ToLower() == "y")
                {
                    File.WriteAllText(cmdLine.SettingsFile, mystuff);
                    log.LogInformation($"Settings file saved to '{cmdLine.SettingsFile}'");
                }
                else
                {
                    log.LogInformation("Settings file not saved");
                    return -2;
                }
                return 0;
            }
            catch (Exception exe)
            {
                log.LogError($"Unable to save settings file.\r\n{exe.ToString()}");
                return -1;
            }

        }
    }
}
