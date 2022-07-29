using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.KeyVault;
using SqlBuildManager.Console.Kubernetes.Yaml;
using SqlBuildManager.Console.Shared;
using SqlSync.Connection;
using System;
using System.IO;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace SqlBuildManager.Console.Kubernetes
{
    public class KubernetesManager
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

                if (File.Exists("/etc/sbm/StorageAccountKey"))
                {
                    args.StorageAccountKey = File.ReadAllText("/etc/sbm/StorageAccountKey");
                    log.LogDebug($"storageaccountkey= {args.ConnectionArgs.StorageAccountKey}");
                }

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

        internal static bool ApplyDeployment(KubernetesFiles files)
        {
            var returnCode = 0;
            log.LogInformation($"Applying file {files.AzureIdentityFileName}");
            returnCode += KubectlProcess.ApplyFile(files.AzureIdentityFileName);

            log.LogInformation($"Applying file {files.AzureBindingFileName}");
            returnCode += KubectlProcess.ApplyFile(files.AzureBindingFileName);

            log.LogInformation($"Applying file {files.SecretsProviderFile}");
            returnCode += KubectlProcess.ApplyFile(files.SecretsProviderFile);

            log.LogInformation($"Applying file {files.RuntimeConfigMapFile}");
            returnCode += KubectlProcess.ApplyFile(files.RuntimeConfigMapFile);

            if (!string.IsNullOrWhiteSpace(files.SecretsFile) && File.Exists(files.SecretsFile))
            {
                log.LogInformation($"Applying file {files.SecretsFile}");
                returnCode += KubectlProcess.ApplyFile(files.SecretsFile);

            }
            log.LogInformation($"Applying file {files.JobFileName}");
            returnCode += KubectlProcess.ApplyFile(files.JobFileName);
            return returnCode == 0;
        }
        internal static bool CleanUpKubernetesResource(bool secretsExist)
        {
            var returnCode = 0;
            returnCode += KubectlProcess.DeleteKubernetesResource(JobYaml.Kind, JobYaml.Name);
            if (secretsExist)
            {
                returnCode += KubectlProcess.DeleteKubernetesResource(SecretYaml.Kind, SecretYaml.Name);
            }
            returnCode += KubectlProcess.DeleteKubernetesResource(ConfigmapYaml.Kind, ConfigmapYaml.Name);
            returnCode += KubectlProcess.DeleteKubernetesResource(SecretsProviderYaml.Kind, SecretsProviderYaml.Name);
            returnCode += KubectlProcess.DeleteKubernetesResource(AzureIdentityYaml.Kind, AzureIdentityYaml.Name);
            returnCode += KubectlProcess.DeleteKubernetesResource(AzureIdentityBindingYaml.Kind, AzureIdentityBindingYaml.Name);
            return returnCode == 0;
        }

        internal static bool MonitorForPodStart()
        {
            int pendingCounter = 0;
            int runningCounter = 0;
            while (true)
            {
                var stat = KubectlProcess.GetJobPodStatus();
                switch (stat)
                {
                    case PodStatus.Running:
                        if (runningCounter > 3)
                        {
                            return true;
                        }
                        else
                        {
                            runningCounter++;
                        }
                        break;
                    case PodStatus.Unknown:
                        continue;
                    case PodStatus.Error:
                        return false;
                    case PodStatus.Completed:
                        return true;
                    case PodStatus.Pending:
                        if (pendingCounter > 5)
                        {
                            return false;
                        }
                        else
                        {
                            pendingCounter++;
                        }
                        break;
                }
                System.Threading.Thread.Sleep(2000);
            }

        }

        public static Task<KubernetesFiles> SaveKubernetesYamlFiles(CommandLineArgs cmdLine, string prefix, DirectoryInfo path)
        {
            var dir = Directory.GetCurrentDirectory();
            var secretsName = "";
            if (path != null)
            {
                dir = path.FullName;
            }
            //Save secrets to KV or create settings file
            if (!string.IsNullOrWhiteSpace(cmdLine.ConnectionArgs.KeyVaultName))
            {
                var lst = KeyVaultHelper.SaveSecrets(cmdLine);
                log.LogInformation($"Saved secrets to Azure Key Vault {cmdLine.ConnectionArgs.KeyVaultName}: {string.Join(", ", lst)}");
            }
            else
            {
                secretsName = Path.Combine(dir, string.IsNullOrWhiteSpace(prefix) ? "secrets.yaml" : $"{prefix}-secrets.yaml");
                string secrets = KubernetesManager.GenerateSecretsYaml(cmdLine);
                if (!string.IsNullOrWhiteSpace(secrets))
                {
                    File.WriteAllText(secretsName, secrets);
                    log.LogInformation($"Secrets file written to: {secretsName}");
                }
                else
                {
                    log.LogInformation($"NO secrets are needed, NOT saving secrets yaml to : {secretsName}");
                }
            }

            //Create configmap file
            string cfgMap = KubernetesManager.GenerateConfigmapYaml(cmdLine);
            var cfgMapName = Path.Combine(dir, string.IsNullOrWhiteSpace(prefix) ? "configmap.yaml" : $"{prefix}-configmap.yaml");
            File.WriteAllText(cfgMapName, cfgMap);
            log.LogInformation($"Configmap file written to: {cfgMapName}");

            //Create secrets provider
            string secretsProvider = KubernetesManager.GenerateSecretsProviderYaml(cmdLine);
            var secretsProviderFileName = Path.Combine(dir, string.IsNullOrWhiteSpace(prefix) ? "secretProviderClass.yaml" : $"{prefix}-secretProviderClass.yaml");
            File.WriteAllText(secretsProviderFileName, secretsProvider);
            log.LogInformation($"Secrets Provider Class file written to: {secretsProviderFileName}");

            //Create Azure Identity
            string azureIdentity = KubernetesManager.GenerateIdentityYaml(cmdLine);
            var azureIdentityFileName = Path.Combine(dir, string.IsNullOrWhiteSpace(prefix) ? "identity.yaml" : $"{prefix}-identity.yaml");
            File.WriteAllText(azureIdentityFileName, azureIdentity);
            log.LogInformation($"Identity file written to: {azureIdentityFileName}");

            //Create Azure Identity Binding
            string azureIdentityBinding = KubernetesManager.GenerateIdentityBindingYaml(cmdLine);
            var azureIdentityBindingFileName = Path.Combine(dir, string.IsNullOrWhiteSpace(prefix) ? "identityBinding.yaml" : $"{prefix}-identityBinding.yaml");
            File.WriteAllText(azureIdentityBindingFileName, azureIdentityBinding);
            log.LogInformation($"Identity Binding file written to: {azureIdentityBindingFileName}");

            string jobYaml = KubernetesManager.GenerateJobYaml(cmdLine);
            var jobYamlFileName = Path.Combine(dir, string.IsNullOrWhiteSpace(prefix) ? "job.yaml" : $"{prefix}-job.yaml");
            File.WriteAllText(jobYamlFileName, jobYaml);
            log.LogInformation($"Job Yaml file written to: {jobYamlFileName}");

            var kf = new KubernetesFiles
            {
                RuntimeConfigMapFile = cfgMapName,
                SecretsFile = secretsName,
                SecretsProviderFile = secretsProviderFileName,
                AzureIdentityFileName = azureIdentityFileName,
                AzureBindingFileName = azureIdentityBindingFileName,
                JobFileName = jobYamlFileName,
            };

            return Task.Run(() => (kf));
        }

        #region Dynamic Yaml Generation
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

        internal static string GenerateIdentityYaml(CommandLineArgs args)
        {
            var yml = new Yaml.AzureIdentityYaml(args.IdentityArgs.IdentityName, args.IdentityArgs.ResourceId, args.IdentityArgs.ClientId);
            var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
            var identityYaml = serializer.Serialize(yml);

            return identityYaml;

        }
        internal static string GenerateIdentityBindingYaml(CommandLineArgs args)
        {
            var ymlB = new Yaml.AzureIdentityBindingYaml(args.IdentityArgs.IdentityName);
            var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
            var bindingYaml = serializer.Serialize(ymlB);

            return bindingYaml;

        }

        internal static string GenerateJobYaml(CommandLineArgs args)
        {
            bool hasKeyVault = !string.IsNullOrWhiteSpace(args.ConnectionArgs.KeyVaultName);
            bool useMangedIdenty = args.AuthenticationArgs.AuthenticationType == AuthenticationType.ManagedIdentity;
            var yml = new Yaml.JobYaml(args.ContainerRegistryArgs.RegistryServer,args.ContainerRegistryArgs.ImageName, args.ContainerRegistryArgs.ImageTag, hasKeyVault, useMangedIdenty);
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

        internal static string GenerateConfigmapYaml(CommandLineArgs args)
        {

            var yml = new Yaml.ConfigmapYaml();
            yml.data.DacpacName = Path.GetFileName(args.DacPacArgs.PlatinumDacpac);
            yml.data.PackageName = Path.GetFileName(args.BuildFileName);
            yml.data.JobName = args.JobName;
            yml.data.AllowObjectDelete = args.AllowObjectDelete.ToString();
            yml.data.Concurrency = args.Concurrency.ToString();
            yml.data.ConcurrencyType = args.ConcurrencyType.ToString();
            yml.data.AuthType = args.AuthenticationArgs.AuthenticationType.ToString();
            
            if(!string.IsNullOrWhiteSpace(args.ConnectionArgs.ServiceBusTopicConnectionString) && !ConnectionValidator.IsServiceBusConnectionString( args.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                yml.data.ServiceBusTopicConnectionString = args.ConnectionArgs.ServiceBusTopicConnectionString;
            }
            if(!string.IsNullOrWhiteSpace(args.ConnectionArgs.EventHubConnectionString) && !ConnectionValidator.IsEventHubConnectionString(args.ConnectionArgs.EventHubConnectionString))
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
        #endregion 

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
        private static string GetValueFromConfigMapstring(string fileName, string key)
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

        internal static CommandLineArgs SetCmdLineArgsFromSecretsAndConfigmap(CommandLineArgs args, string secretsFile, string configMapFile)
        {
            if (File.Exists(secretsFile))
            {
                args = GetArgumentsFromSecretsFile(secretsFile, args);
            }
            if (File.Exists(configMapFile))
            {
                args = GetArgumentsFromConfigMapFile(configMapFile, args);
            }
            return args;
        }

        private static CommandLineArgs GetArgumentsFromSecretsFile(string filename, CommandLineArgs args)
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
        private static CommandLineArgs GetArgumentsFromConfigMapFile(string filename, CommandLineArgs args)
        {
            if (args == null)
            {
                args = new CommandLineArgs();
            }

            var tmp = GetValueFromConfigMapstring(filename, "PackageName");
            if (!string.IsNullOrWhiteSpace(tmp)) args.BuildFileName = tmp;


            tmp  = GetValueFromConfigMapstring(filename, "JobName");
            if (!string.IsNullOrWhiteSpace(tmp)) args.JobName = tmp;


            tmp = GetValueFromConfigMapstring(filename, "DacpacName");
            if (!string.IsNullOrWhiteSpace(tmp)) args.PlatinumDacpac = tmp;

            if (Enum.TryParse<ConcurrencyType>(GetValueFromConfigMapstring(filename, "ConcurrencyType"), out ConcurrencyType con))
            {
                args.ConcurrencyType = con;
            }

            if (int.TryParse(GetValueFromConfigMapstring(filename, "Concurrency"), out int c))
            {
                args.Concurrency = c;
            }

            if (Enum.TryParse<AuthenticationType>(GetValueFromConfigMapstring(filename, "AuthType"), out AuthenticationType auth))
            {
                args.AuthenticationType = auth;
            }

            if (args.AuthenticationArgs.AuthenticationType == AuthenticationType.ManagedIdentity)
            {

                tmp = GetValueFromConfigMapstring(filename, "EventHubConnectionString");
                if (!string.IsNullOrWhiteSpace(tmp)) args.EventHubConnection = tmp;

                tmp = GetValueFromConfigMapstring(filename, "ServiceBusTopicConnectionString");
                if (!string.IsNullOrWhiteSpace(tmp)) args.ServiceBusTopicConnection = tmp;
            }

            tmp = GetValueFromConfigMapstring(filename, "StorageAccountName");
            if (!string.IsNullOrWhiteSpace(tmp)) args.ConnectionArgs.StorageAccountName = tmp;

            return args;

        }


    }
}
