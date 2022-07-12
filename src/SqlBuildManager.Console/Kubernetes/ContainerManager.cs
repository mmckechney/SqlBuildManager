using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Shared;
using SqlSync.Connection;
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using YamlDotNet.Serialization;

namespace SqlBuildManager.Console.Kubernetes
{
    public class ContainerManager
    {
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static (bool, CommandLineArgs) ReadSecrets(CommandLineArgs args)
        {
            if(args == null)
            {
                args = new CommandLineArgs();
            }

            try
            {
                if (File.Exists("/etc/sbm/EventHubConnectionString"))
                {
                    args.EventHubConnection = File.ReadAllText("/etc/sbm/EventHubConnectionString");
                    log.LogDebug($"eventhub= {args.ConnectionArgs.EventHubConnectionString}");
                }

                if (File.Exists("/etc/sbm/ServiceBusTopicConnectionString"))
                {
                    args.ServiceBusTopicConnection = File.ReadAllText("/etc/sbm/ServiceBusTopicConnectionString");
                    log.LogDebug($"serviceBus= {args.ConnectionArgs.ServiceBusTopicConnectionString}");
                }

                args.StorageAccountKey = File.ReadAllText("/etc/sbm/StorageAccountKey");
                log.LogDebug($"storageaccountkey= {args.ConnectionArgs.StorageAccountKey}");

                if(args.AuthenticationArgs.AuthenticationType != AuthenticationType.ManagedIdentity)
                {
                    args.Password = File.ReadAllText("/etc/sbm/Password");
                    log.LogDebug($"password= {args.AuthenticationArgs.Password}");

                    args.UserName = File.ReadAllText("/etc/sbm/UserName");
                    log.LogDebug($"username= {args.AuthenticationArgs.UserName}");
                }

                return (true,args);
            }
            catch(Exception ex)
            {
                log.LogError($"Unable to read secrets: {ex.Message}");
                log.LogWarning("The secrets needed for running a container/pod worker have not been set.They should be set as secrets mounted to /etc/sbm");
                return (false,args);
            }
        }

        internal static (bool, CommandLineArgs) ReadRuntimeParameters(CommandLineArgs args)
        {

            try
            {
                //these are required and should fail of not there...
                args.BuildFileName = File.ReadAllText("/etc/runtime/PackageName");
                log.LogDebug($"sbmName= { args.BuildFileName}");

                args.BatchJobName = File.ReadAllText("/etc/runtime/JobName");
                log.LogDebug($"storageContainerName= {args.BatchArgs.BatchJobName}");
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to read runtime parameters: {ex.Message}");
                log.LogWarning("The runtime variables for running a container/pod worker have not been set. They should be set as a Configmap mounted to /etc/runtime");
                return (false,args);
            }

            if (File.Exists("/etc/runtime/DacpacName"))
            {
                args.PlatinumDacpac = File.ReadAllText("/etc/runtime/DacpacName");
                log.LogDebug($"dacpacName= {args.DacPacArgs.PlatinumDacpac}");
            }

            if (File.Exists("/etc/runtime/Concurrency"))
            {
                if(int.TryParse(File.ReadAllText("/etc/runtime/Concurrency"), out int val))
                {
                    args.Concurrency = val;
                }
                log.LogDebug($"concurrency= {args.Concurrency}");
            }

            if (File.Exists("/etc/runtime/ConcurrencyType"))
            {
                if(Enum.TryParse<ConcurrencyType>(File.ReadAllText("/etc/runtime/ConcurrencyType"), out ConcurrencyType ct))
                {
                    args.ConcurrencyType = ct;
                }
                log.LogDebug($"concurrencyType= {args.ConcurrencyType}");
            }

            if (File.Exists("/etc/runtime/AllowObjectDelete"))
            {
                if (bool.TryParse(File.ReadAllText("/etc/runtime/AllowObjectDelete"), out bool allow))
                {
                    args.AllowObjectDelete = allow;
                }
                log.LogDebug($"allowObjectDelete= {args.AllowObjectDelete}");
            }

            if (File.Exists("/etc/runtime/AuthType"))
            {
                if (Enum.TryParse<AuthenticationType>(File.ReadAllText("/etc/runtime/AuthType"), out AuthenticationType auth))
                {
                    args.AuthenticationType = auth;
                }
                log.LogDebug($"authType= {args.AllowObjectDelete}");
            }

            if (File.Exists("/etc/runtime/EventHubConnectionString"))
            {
                args.EventHubConnection = File.ReadAllText("/etc/runtime/EventHubConnectionString");
                log.LogDebug($"EventHubConnectionString= {args.ConnectionArgs.EventHubConnectionString}");
            }

            if (File.Exists("/etc/runtime/ServiceBusTopicConnectionString"))
            {
                args.ServiceBusTopicConnection = File.ReadAllText("/etc/runtime/ServiceBusTopicConnectionString");
                log.LogDebug($"ServiceBusTopicConnectionString= {args.ConnectionArgs.ServiceBusTopicConnectionString}");
            }

            if (File.Exists("/etc/runtime/StorageAccountName"))
            {
                args.ConnectionArgs.StorageAccountName = File.ReadAllText("/etc/runtime/StorageAccountName");
                log.LogDebug($"StorageAccountName= {args.ConnectionArgs.StorageAccountName}");
            }


            return (true, args);
        }

        internal static string GenerateSecretsProviderYaml(CommandLineArgs args)
        {
            var yml = new Yaml.SecretsProviderYaml();
            if(!string.IsNullOrWhiteSpace(args.ConnectionArgs.KeyVaultName))
            {
                yml.spec.parameters.keyvaultName = args.ConnectionArgs.KeyVaultName;
            }

            if (!string.IsNullOrWhiteSpace(args.IdentityArgs.TenantId))
            {
                yml.spec.parameters.tenantId = args.IdentityArgs.TenantId;
            }

            if (!string.IsNullOrWhiteSpace(args.IdentityArgs.ClientId))
            {
                yml.spec.parameters.userAssignedIdentityID = args.IdentityArgs.ClientId;
            }

            var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
            var yamlString = serializer.Serialize(yml);
            yamlString = yamlString.TrimEnd() +  Yaml.Objects.definition;
            return yamlString;

        }

        internal static string GenerateIdentityAndBindingYaml(CommandLineArgs args)
        {
            var yml = new Yaml.AzureIdentity(args.IdentityArgs.IdentityName, args.IdentityArgs.ResourceGroup, args.IdentityArgs.ResourceId);
            var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
            var identityYaml = serializer.Serialize(yml);

            var ymlB = new Yaml.AzureIdentityBinding(args.IdentityArgs.IdentityName);
            var bindingYaml = serializer.Serialize(ymlB);

            var sb = new StringBuilder();
            sb.AppendLine(identityYaml);
            sb.AppendLine("---");
            sb.AppendLine(bindingYaml);

            return sb.ToString();

        }

        internal static string GenerateJobYaml(CommandLineArgs args)
        {
            bool hasSecrets = GenerateSecretsYaml(args).Length > 0;
            var yml = new Yaml.JobYaml(args.ContainerRegistryArgs.RegistryServer,args.ContainerRegistryArgs.ImageName, args.ContainerRegistryArgs.ImageTag, hasSecrets);
            var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
            var jobYaml = serializer.Serialize(yml);


            return jobYaml;

        } 
        internal static string GenerateSecretsYaml(CommandLineArgs args)
        {
            bool secretAdded = false;
            var yml = new Yaml.SecretYaml();
           

            if (!string.IsNullOrWhiteSpace(args.ConnectionArgs.EventHubConnectionString) && ConnectionValidator.IsServiceBusConnectionString(args.ConnectionArgs.EventHubConnectionString))
            {
                yml.data.EventHubConnectionString = args.ConnectionArgs.EventHubConnectionString.EncodeBase64();
                secretAdded = true;
            }

            if (!string.IsNullOrWhiteSpace(args.ConnectionArgs.ServiceBusTopicConnectionString) && ConnectionValidator.IsServiceBusConnectionString(args.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                yml.data.ServiceBusTopicConnectionString = args.ConnectionArgs.ServiceBusTopicConnectionString.EncodeBase64();
                secretAdded = true;
            }

            if (args.AuthenticationArgs.AuthenticationType != AuthenticationType.ManagedIdentity)
            {
                if (!string.IsNullOrWhiteSpace(args.AuthenticationArgs.UserName))
                {
                    yml.data.UserName = args.AuthenticationArgs.UserName.EncodeBase64();
                    secretAdded = true;
                }

                if (!string.IsNullOrWhiteSpace(args.AuthenticationArgs.Password))
                {
                    yml.data.Password = args.AuthenticationArgs.Password.EncodeBase64();
                    secretAdded = true;
                }

                if (!string.IsNullOrWhiteSpace(args.ConnectionArgs.StorageAccountKey))
                {
                    yml.data.StorageAccountKey = args.ConnectionArgs.StorageAccountKey.EncodeBase64();
                    secretAdded = true;
                }
            }

            if (secretAdded)
            {
                var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
                var yamlString = serializer.Serialize(yml);
                return yamlString;
            }
            else
            {
                log.LogInformation("No secrets were provided or needed. No secrets yaml is being generated");
                return string.Empty;
            }

            
          

        }

        internal static string GenerateRuntimeYaml(CommandLineArgs args)
        {

            var yml = new Yaml.RuntimeYaml();
            yml.data.DacpacName = args.DacPacArgs.PlatinumDacpac;
            yml.data.PackageName = args.BuildFileName;
            yml.data.JobName = args.JobName;
            yml.data.AllowObjectDelete = args.AllowObjectDelete.ToString();
            yml.data.Concurrency = args.Concurrency.ToString();
            yml.data.ConcurrencyType = args.ConcurrencyType.ToString();
            yml.data.AuthType = args.AuthenticationArgs.AuthenticationType.ToString();
            
            if(!ConnectionValidator.IsServiceBusConnectionString( args.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                yml.data.ServiceBusTopicConnectionString = args.ConnectionArgs.ServiceBusTopicConnectionString;
            }
            if(!ConnectionValidator.IsEventHubConnectionString(args.ConnectionArgs.EventHubConnectionString))
            {
                yml.data.EventHubConnectionString = args.ConnectionArgs.EventHubConnectionString;
            }

            if (!string.IsNullOrWhiteSpace(args.ConnectionArgs.StorageAccountName))
            {
                yml.data.StorageAccountName = args.ConnectionArgs.StorageAccountName;
            }
            
            var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
            var yamlString = serializer.Serialize(yml);

            return yamlString;

        }

        private static string GetValueFromSecrets(string fileName, string key)
        {
            try
            {
                var des = new DeserializerBuilder().Build();
                var sec = des.Deserialize<dynamic>(File.ReadAllText(fileName));
                var sb = (string)sec["data"][key];
                return sb.DecodeBase64();
            }
            catch
            {
                return string.Empty;
            }
        }
        private static string GetValueFromRuntimestring(string fileName, string key)
        {
            try
            {
                var des = new DeserializerBuilder().Build();
                var sec = des.Deserialize<dynamic>(File.ReadAllText(fileName));
                var name = sec["data"][key];
                return name as string;
            }
            catch
            {
                return string.Empty;
            }
        }

        internal static CommandLineArgs GetArgumentsFromSecretsFile(string filename, CommandLineArgs args)
        {
            if(args == null)
            {
                args = new CommandLineArgs();
            }

            if (args.AuthenticationArgs.AuthenticationType != AuthenticationType.ManagedIdentity)
            {
                var tmp = GetValueFromSecrets(filename, "UserName");
                if (!string.IsNullOrWhiteSpace(tmp)) args.UserName = tmp;
                
                tmp = GetValueFromSecrets(filename, "Password");
                if (!string.IsNullOrWhiteSpace(tmp)) args.Password = tmp;

                tmp = GetValueFromSecrets(filename, "EventHubConnectionString");
                if (!string.IsNullOrWhiteSpace(tmp)) args.EventHubConnection = tmp;

                tmp = GetValueFromSecrets(filename, "ServiceBusTopicConnectionString");
                if (!string.IsNullOrWhiteSpace(tmp)) args.ServiceBusTopicConnection = tmp;

                tmp = GetValueFromSecrets(filename, "StorageAccountKey");
                if (!string.IsNullOrWhiteSpace(tmp)) args.StorageAccountKey = tmp;

                tmp = GetValueFromSecrets(filename, "StorageAccountName");
                if (!string.IsNullOrWhiteSpace(tmp)) args.StorageAccountName = tmp;
            }
            

            return args;

        }
        internal static CommandLineArgs GetArgumentsFromRuntimeFile(string filename, CommandLineArgs args)
        {
            if (args == null)
            {
                args = new CommandLineArgs();
            }

            var tmp = GetValueFromRuntimestring(filename, "PackageName");
            if (!string.IsNullOrWhiteSpace(tmp)) args.BuildFileName = tmp;


            tmp  = GetValueFromRuntimestring(filename, "JobName");
            if (!string.IsNullOrWhiteSpace(tmp)) args.JobName = tmp;


            tmp = GetValueFromRuntimestring(filename, "DacpacName");
            if (!string.IsNullOrWhiteSpace(tmp)) args.PlatinumDacpac = tmp;

            if (Enum.TryParse<ConcurrencyType>(GetValueFromRuntimestring(filename, "ConcurrencyType"), out ConcurrencyType con))
            {
                args.ConcurrencyType = con;
            }

            if (int.TryParse(GetValueFromRuntimestring(filename, "Concurrency"), out int c))
            {
                args.Concurrency = c;
            }

            if (Enum.TryParse<AuthenticationType>(GetValueFromRuntimestring(filename, "AuthType"), out AuthenticationType auth))
            {
                args.AuthenticationType = auth;
            }

            if (args.AuthenticationArgs.AuthenticationType == AuthenticationType.ManagedIdentity)
            {

                tmp = GetValueFromRuntimestring(filename, "EventHubConnectionString");
                if (!string.IsNullOrWhiteSpace(tmp)) args.EventHubConnection = tmp;

                tmp = GetValueFromRuntimestring(filename, "ServiceBusTopicConnectionString");
                if (!string.IsNullOrWhiteSpace(tmp)) args.ServiceBusTopicConnection = tmp;
            }

            tmp = GetValueFromRuntimestring(filename, "StorageAccountName");
            if (!string.IsNullOrWhiteSpace(tmp)) args.ConnectionArgs.StorageAccountName = tmp;

            return args;

        }


    }
}
