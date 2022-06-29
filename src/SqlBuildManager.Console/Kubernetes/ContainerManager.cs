using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.Shared;
using SqlSync.Connection;
using System;
using System.IO;
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

                args.StorageAccountName = File.ReadAllText("/etc/sbm/StorageAccountName");
                log.LogDebug($"storageaccountname= {args.ConnectionArgs.StorageAccountName}");

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


            return (true, args);
        }

        internal static string GenerateSecretsYaml(CommandLineArgs args)
        {

            var yml = new SecretYaml();
            if (!string.IsNullOrWhiteSpace(args.ConnectionArgs.StorageAccountName))
            {
                yml.data.StorageAccountName = args.ConnectionArgs.StorageAccountName.EncodeBase64();
            }

            if (!string.IsNullOrWhiteSpace(args.ConnectionArgs.StorageAccountKey))
            {
                yml.data.StorageAccountKey = args.ConnectionArgs.StorageAccountKey.EncodeBase64();
            }

            if (!string.IsNullOrWhiteSpace(args.ConnectionArgs.EventHubConnectionString) && ConnectionValidator.IsServiceBusConnectionString(args.ConnectionArgs.EventHubConnectionString))
            {
                yml.data.EventHubConnectionString = args.ConnectionArgs.EventHubConnectionString.EncodeBase64();
            }

            if (!string.IsNullOrWhiteSpace(args.ConnectionArgs.ServiceBusTopicConnectionString) && ConnectionValidator.IsServiceBusConnectionString(args.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                yml.data.ServiceBusTopicConnectionString = args.ConnectionArgs.ServiceBusTopicConnectionString.EncodeBase64();
            }

            if (args.AuthenticationArgs.AuthenticationType != AuthenticationType.ManagedIdentity)
            {
                if (!string.IsNullOrWhiteSpace(args.AuthenticationArgs.UserName))
                {
                    yml.data.UserName = args.AuthenticationArgs.UserName.EncodeBase64();
                }

                if (!string.IsNullOrWhiteSpace(args.AuthenticationArgs.Password))
                {
                    yml.data.Password = args.AuthenticationArgs.Password.EncodeBase64();
                }
            }

            var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
            var yamlString = serializer.Serialize(yml);

            return yamlString;
          

        }

        internal static string GenerateRuntimeYaml(CommandLineArgs args)
        {

            var yml = new RuntimeYaml();
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
                args.UserName = GetValueFromSecrets(filename, "UserName");
                args.Password = GetValueFromSecrets(filename, "Password");
                args.EventHubConnection = GetValueFromSecrets(filename, "EventHubConnectionString");
                args.ServiceBusTopicConnection = GetValueFromSecrets(filename, "ServiceBusTopicConnectionString");
            }
            args.StorageAccountKey = GetValueFromSecrets(filename, "StorageAccountKey");
            args.StorageAccountName = GetValueFromSecrets(filename, "StorageAccountName");

            return args;

        }
        internal static CommandLineArgs GetArgumentsFromRuntimeFile(string filename, CommandLineArgs args)
        {
            if (args == null)
            {
                args = new CommandLineArgs();
            }
            args.BuildFileName = GetValueFromRuntimestring(filename, "PackageName");
            args.JobName = GetValueFromRuntimestring(filename, "JobName");
            args.PlatinumDacpac = GetValueFromRuntimestring(filename, "DacpacName");

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
                args.EventHubConnection = GetValueFromRuntimestring(filename, "EventHubConnectionString");
                args.ServiceBusTopicConnection = GetValueFromRuntimestring(filename, "ServiceBusTopicConnectionString");
            }

            return args;

        }


    }
}
