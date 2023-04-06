using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.CommandLine;
using SqlSync.Connection;
using System;
using System.Collections.Generic;
using System.IO;

namespace SqlBuildManager.Console.ContainerShared
{

    internal class EnvironmentVariableHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);
        internal static CommandLineArgs ReadRuntimeEnvironmentVariables(CommandLineArgs cmdLine)
        {
            log.LogInformation("Reading environment variables for Container worker");
            string tmp;

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.KeyVaultName);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.KeyVaultName = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.KeyVaultName}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.IdentityClientId);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.ClientId = tmp;
                AadHelper.ManagedIdentityClientId = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.IdentityClientId}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.JobName);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.JobName = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.JobName}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.PackageName);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.BuildFileName = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.PackageName}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.DacpacName);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.PlatinumDacpac = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.DacpacName}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.StorageAccountName);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.StorageAccountName = tmp;
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.StorageAccountName}");
            }

            if (int.TryParse(Environment.GetEnvironmentVariable(ContainerEnvVariables.Concurrency), out int c))
            {
                cmdLine.Concurrency = c;
            }
            else
            {
                log.LogWarning($"Unable to read or parse environment variable {ContainerEnvVariables.Concurrency}");
            }

            if (Enum.TryParse<CommandLine.ConcurrencyType>(Environment.GetEnvironmentVariable(ContainerEnvVariables.ConcurrencyType), out CommandLine.ConcurrencyType ct))
            {
                cmdLine.ConcurrencyType = ct;
            }
            else
            {
                log.LogWarning($"Unable to read or parse environment variable {ContainerEnvVariables.ConcurrencyType}");
            }

            if (Enum.TryParse<AuthenticationType>(Environment.GetEnvironmentVariable(ContainerEnvVariables.AuthType), out AuthenticationType auth))
            {
                cmdLine.AuthenticationType = auth;
            }
            else
            {
                log.LogWarning($"Unable to read or parse environment variable {ContainerEnvVariables.AuthType}");
            }

            var ehString = Environment.GetEnvironmentVariable(ContainerEnvVariables.EventHubLogging);
            if (!string.IsNullOrWhiteSpace(ehString))
            {
                var ehArr = ehString.Split('|');
                List<EventHubLogging> ehList = new List<EventHubLogging>();
                foreach (var e in ehArr)
                {
                    if (Enum.TryParse<EventHubLogging>(e, out EventHubLogging ehTmp))
                    {
                        ehList.Add(ehTmp);
                    }
                }
                cmdLine.EventHubLogging = ehList.ToArray();

            }
            else
            {
                log.LogWarning($"Unable to read or parse environment variable {ContainerEnvVariables.AuthType}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.AllowObjectDelete);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                bool allow;
                if (bool.TryParse(tmp, out allow))
                {
                    cmdLine.AllowObjectDelete = allow;
                }
                else
                {
                    log.LogWarning($"The environment variable {ContainerEnvVariables.AllowObjectDelete} is expecting a boolean value but retrieved '{tmp}'");
                }
            }
            else
            {
                log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.AllowObjectDelete}");
            }


            //If KeyVault is provided, these will get read from KeyVault Secrets
            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.StorageAccountKey);
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    cmdLine.StorageAccountKey = tmp;
                }
                else
                {
                    log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.StorageAccountKey}");
                }
                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.EventHubConnectionString);
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    cmdLine.EventHubConnection = tmp;
                }
                else
                {
                    log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.EventHubConnectionString}");
                }
                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.ServiceBusTopicConnectionString);
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    cmdLine.ServiceBusTopicConnection = tmp;
                }
                else
                {
                    log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.ServiceBusTopicConnectionString}");
                }

                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.UserName);
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    cmdLine.UserName = tmp;
                }
                else
                {
                    log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.UserName}");
                }

                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.Password);
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    cmdLine.Password = tmp;
                }
                else
                {
                    log.LogWarning($"Unable to read environment variable {ContainerEnvVariables.Password}");
                }
            }
            
            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.QueryFile);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.QueryFile = new FileInfo(tmp);
            }
            else
            {
                log.LogInformation($"Unable to read environment variable {ContainerEnvVariables.QueryFile}");
            }
            
            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.OutputFile);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.OutputFile = new FileInfo(tmp);
            }
            else
            {
                log.LogInformation($"Unable to read environment variable {ContainerEnvVariables.OutputFile}");
            }


            return cmdLine;
        }
    }
}
