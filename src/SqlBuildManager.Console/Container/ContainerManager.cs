using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using SqlBuildManager.Console.CloudStorage;
using Azure.Storage;
using System.Threading.Tasks;
using SqlBuildManager.Console.CommandLine;
using Microsoft.SqlServer.Management.SqlParser.Metadata;
using System.Net.Sockets;
using YamlDotNet.Serialization;
namespace SqlBuildManager.Console.Container
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
                args.EventHubConnection = File.ReadAllText("/etc/sbm/EventHubConnectionString");
                log.LogDebug($"eventhub= {args.ConnectionArgs.EventHubConnectionString}");

                args.ServiceBusTopicConnection=  File.ReadAllText("/etc/sbm/ServiceBusTopicConnectionString");
                log.LogDebug($"serviceBus= {args.ConnectionArgs.ServiceBusTopicConnectionString}");

                args.Password = File.ReadAllText("/etc/sbm/Password");
                log.LogDebug($"password= {args.AuthenticationArgs.Password}");

                args.UserName = File.ReadAllText("/etc/sbm/UserName");
                log.LogDebug($"username= {args.AuthenticationArgs.UserName}");

                args.StorageAccountName = File.ReadAllText("/etc/sbm/StorageAccountName");
                log.LogDebug($"storageaccountname= {args.ConnectionArgs.StorageAccountName}");

                args.StorageAccountKey = File.ReadAllText("/etc/sbm/StorageAccountKey");
                log.LogDebug($"storageaccountkey= {args.ConnectionArgs.StorageAccountKey}");

                return (true,args);
            }
            catch(Exception ex)
            {
                log.LogError($"Unable to read secrets: {ex.Message}");
                log.LogWarning("The secrets needed for running a container worker have not been set.They should be set as secrets mounted to /etc/sbm");
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
                log.LogWarning("The runtime variables for running a container worker have not been set. They should be set as a Configmap mounted to /etc/runtime");
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
            return (true, args);
        }

        internal static string GenerateSecretsYaml(CommandLineArgs args)
        {

            string template = @"
apiVersion: v1
kind: Secret
metadata:
  name: connection-secrets
type: Opaque
data: 
  EventHubConnectionString: {0}
  ServiceBusTopicConnectionString: {1}
  UserName: {2}
  Password: {3}
  StorageAccountName:  {4}
  StorageAccountKey: {5}";

            var complete = string.Format(template,
                    args.ConnectionArgs.EventHubConnectionString.EncodeBase64(),
                    args.ConnectionArgs.ServiceBusTopicConnectionString.EncodeBase64(),
                    args.AuthenticationArgs.UserName.EncodeBase64(),
                    args.AuthenticationArgs.Password.EncodeBase64(),
                    args.ConnectionArgs.StorageAccountName.EncodeBase64(),
                    args.ConnectionArgs.StorageAccountKey.EncodeBase64());

            return complete;
        }

        internal static string GenerateRuntimeYaml(CommandLineArgs args)
        {
            string template = @"
kind: ConfigMap 
apiVersion: v1 
metadata:
  name: runtime-properties
data:
  DacpacName: '{0}'
  PackageName: '{1}'
  JobName: '{2}'
  Concurrency: '{3}'
  ConcurrencyType: '{4}'";

            var complete = string.Format(template,
                args.DacPacArgs.PlatinumDacpac,
                args.BuildFileName,
                args.JobName,
                args.Concurrency,
                args.ConcurrencyType);

            return complete;
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
            args.UserName = GetValueFromSecrets(filename, "UserName");
            args.Password = GetValueFromSecrets(filename, "Password");
            args.EventHubConnection = GetValueFromSecrets(filename, "EventHubConnectionString");
            args.ServiceBusTopicConnection = GetValueFromSecrets(filename,"ServiceBusTopicConnectionString");
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

            if(int.TryParse(GetValueFromRuntimestring(filename, "Concurrency"), out int c))
            { 
                args.Concurrency = c;
            }

            return args;

        }


    }
}
