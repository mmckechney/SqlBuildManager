using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.Aad;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Connection;
using System;
using System.Collections.Generic;
using System.IO;

namespace SqlBuildManager.Console.ContainerShared
{

    internal class EnvironmentVariableHelper
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod()!.DeclaringType!);

        internal static Dictionary<string, string> CreateRuntimeEnvironmentVariables(CommandLineArgs cmdLine, bool unitTest = false)
        {
            var authenticationType = cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.Password
                ? AuthenticationType.Password
                : AuthenticationType.ManagedIdentity;
            var values = new Dictionary<string, string>
            {
                [ContainerEnvVariables.JobName] = cmdLine.JobName,
                [ContainerEnvVariables.PackageName] = cmdLine.BuildFileName,
                [ContainerEnvVariables.Concurrency] = cmdLine.Concurrency.ToString(),
                [ContainerEnvVariables.ConcurrencyType] = cmdLine.ConcurrencyType.ToString(),
                [ContainerEnvVariables.KeyVaultName] = cmdLine.ConnectionArgs.KeyVaultName,
                [ContainerEnvVariables.StorageAccountName] = cmdLine.ConnectionArgs.StorageAccountName,
                [ContainerEnvVariables.AuthType] = authenticationType.ToString(),
                [ContainerEnvVariables.AllowObjectDelete] = cmdLine.AllowObjectDelete.ToString(),
                [ContainerEnvVariables.EventHubLogging] = string.Join("|", cmdLine.EventHubLogging),
                [ContainerEnvVariables.DatabasePlatform] = cmdLine.AuthenticationArgs.DatabasePlatform.ToString(),
                [ContainerEnvVariables.Transactional] = cmdLine.Transactional.ToString(),
                [ContainerEnvVariables.TimeoutRetryCount] = cmdLine.TimeoutRetryCount.ToString(),
                [ContainerEnvVariables.DefaultScriptTimeout] = cmdLine.DefaultScriptTimeout.ToString(),
                [ContainerEnvVariables.ForceCustomDacPac] = cmdLine.DacPacArgs.ForceCustomDacPac.ToString(),
                [ContainerEnvVariables.TrustServerCertificate] = cmdLine.AuthenticationArgs.TrustServerCertificate.ToString(),
                [ContainerEnvVariables.Silent] = cmdLine.Silent.ToString(),
                [ContainerEnvVariables.UnitTest] = unitTest.ToString()
            };

            AddIfPresent(values, ContainerEnvVariables.DacpacName, cmdLine.DacPacArgs.PlatinumDacpac);
            AddIfPresent(values, ContainerEnvVariables.TargetDacpac, cmdLine.DacPacArgs.TargetDacpac);
            AddIfPresent(values, ContainerEnvVariables.PlatinumDbSource, cmdLine.DacPacArgs.PlatinumDbSource);
            AddIfPresent(values, ContainerEnvVariables.PlatinumServerSource, cmdLine.DacPacArgs.PlatinumServerSource);
            AddIfPresent(values, ContainerEnvVariables.Override, cmdLine.MultiDbRunConfigFileName);
            AddIfPresent(values, ContainerEnvVariables.EventHubConnectionString, cmdLine.ConnectionArgs.EventHubConnectionString);
            AddIfPresent(values, ContainerEnvVariables.ServiceBusTopicConnectionString, cmdLine.ConnectionArgs.ServiceBusTopicConnectionString);
            AddIfPresent(values, ContainerEnvVariables.IdentityClientId, cmdLine.IdentityArgs.ClientId);
            AddIfPresent(values, ContainerEnvVariables.IdentityName, cmdLine.IdentityArgs.IdentityName);
            AddIfPresent(values, ContainerEnvVariables.TenantId, cmdLine.IdentityArgs.TenantId);
            AddIfPresent(values, ContainerEnvVariables.OutputContainerSasUrl, cmdLine.BatchArgs.OutputContainerSasUrl);
            AddIfPresent(values, ContainerEnvVariables.QueryFile, cmdLine.QueryFile?.ToString());
            AddIfPresent(values, ContainerEnvVariables.OutputFile, cmdLine.OutputFile?.ToString());

            if (string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                AddIfPresent(values, ContainerEnvVariables.StorageAccountKey, cmdLine.ConnectionArgs.StorageAccountKey);
                AddIfPresent(values, ContainerEnvVariables.UserName, cmdLine.AuthenticationArgs.UserName);
                AddIfPresent(values, ContainerEnvVariables.Password, cmdLine.AuthenticationArgs.Password);
            }

            return values;
        }

        private static void AddIfPresent(Dictionary<string, string> values, string name, string? value)
        {
            if (!string.IsNullOrWhiteSpace(value))
            {
                values[name] = value;
            }
        }

        internal static CommandLineArgs ReadRuntimeEnvironmentVariables(CommandLineArgs cmdLine)
        {
            log.LogInformation("Reading environment variables for Container worker");
            string? tmp;
            bool useManagedIdentity = false;

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.KeyVaultName);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.KeyVaultName = tmp;
            }
            else
            {
                log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.KeyVaultName}");
            }

            if(cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.AzureADDefault || cmdLine.AuthenticationArgs.AuthenticationType == AuthenticationType.ManagedIdentity)
            {
                useManagedIdentity = true;
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.IdentityClientId);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.ClientId = tmp;
                AadHelper.ManagedIdentityClientId = tmp;
            }
            else
            {
                log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.IdentityClientId}");
                if(useManagedIdentity)
                {
                    log.LogWarning($"No {ContainerEnvVariables.IdentityClientId} environment variable was found. If your container is configured to use a Managed Identity, it will not be able to authenticate.");
                }
                
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.IdentityName);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.IdentityArgs.IdentityName = tmp;
            }else
            {
                log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.IdentityName}");
                if(useManagedIdentity)
                {
                    log.LogWarning($"No {ContainerEnvVariables.IdentityName} environment variable was found. If your container is configured to use a Managed Identity, it will not be able to authenticate.");
                }
                
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
                log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.DacpacName}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.StorageAccountName);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.StorageAccountName = tmp;
            }
            else
            {
                log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.StorageAccountName}");
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

            if (Enum.TryParse<SqlBuildManager.Connection.DatabasePlatform>(Environment.GetEnvironmentVariable(ContainerEnvVariables.DatabasePlatform), out SqlBuildManager.Connection.DatabasePlatform dbPlatform))
            {
                cmdLine.DatabasePlatform = dbPlatform;
            }
            else
            {
                log.LogInformation($"Unable to read or parse environment variable {ContainerEnvVariables.DatabasePlatform}. Defaulting to SqlServer.");
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
                log.LogDebug($"Unable to read or parse environment variable {ContainerEnvVariables.EventHubLogging}");
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
                log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.AllowObjectDelete}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.EventHubConnectionString);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.EventHubConnection = tmp;
            }
            else
            {
                log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.EventHubConnectionString}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.ServiceBusTopicConnectionString);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.ServiceBusTopicConnection = tmp;
            }
            else
            {
                log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.ServiceBusTopicConnectionString}");
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
                    log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.StorageAccountKey}");
                }
              

                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.UserName);
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    cmdLine.UserName = tmp;
                }
                else
                {
                    log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.UserName}");
                }

                tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.Password);
                if (!string.IsNullOrWhiteSpace(tmp))
                {
                    cmdLine.Password = tmp;
                }
                else
                {
                    log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.Password}");
                }
            }
            
            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.QueryFile);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.QueryFile = new FileInfo(tmp);
            }
            else
            {
                log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.QueryFile}");
            }
            
            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.OutputFile);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.OutputFile = new FileInfo(tmp);
            }
            else
            {
                log.LogDebug($"Unable to read environment variable {ContainerEnvVariables.OutputFile}");
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.Override);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.Override = tmp;
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.TargetDacpac);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.TargetDacpac = tmp;
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.OutputContainerSasUrl);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.OutputContainerSasUrl = tmp;
            }

            ReadBoolean(ContainerEnvVariables.Transactional, value => cmdLine.Transactional = value);
            ReadBoolean(ContainerEnvVariables.ForceCustomDacPac, value => cmdLine.ForceCustomDacPac = value);
            ReadBoolean(ContainerEnvVariables.TrustServerCertificate, value => cmdLine.TrustServerCertificate = value);
            ReadBoolean(ContainerEnvVariables.Silent, value => cmdLine.Silent = value);
            ReadInteger(ContainerEnvVariables.TimeoutRetryCount, value => cmdLine.TimeoutRetryCount = value);
            ReadInteger(ContainerEnvVariables.DefaultScriptTimeout, value => cmdLine.DefaultScriptTimeout = value);

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.PlatinumDbSource);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.PlatinumDbSource = tmp;
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.PlatinumServerSource);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.PlatinumServerSource = tmp;
            }

            tmp = Environment.GetEnvironmentVariable(ContainerEnvVariables.TenantId);
            if (!string.IsNullOrWhiteSpace(tmp))
            {
                cmdLine.TenantId = tmp;
            }

            return cmdLine;
        }

        internal static bool IsUnitTest() =>
            bool.TryParse(Environment.GetEnvironmentVariable(ContainerEnvVariables.UnitTest), out var value) && value;

        private static void ReadBoolean(string name, Action<bool> setter)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (bool.TryParse(value, out var parsed))
            {
                setter(parsed);
            }
            else if (!string.IsNullOrWhiteSpace(value))
            {
                log.LogWarning($"Unable to parse environment variable {name} as a boolean");
            }
        }

        private static void ReadInteger(string name, Action<int> setter)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (int.TryParse(value, out var parsed))
            {
                setter(parsed);
            }
            else if (!string.IsNullOrWhiteSpace(value))
            {
                log.LogWarning($"Unable to parse environment variable {name} as an integer");
            }
        }
    }
}
