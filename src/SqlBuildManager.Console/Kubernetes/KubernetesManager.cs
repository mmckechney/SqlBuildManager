using Microsoft.Extensions.Logging;
using SqlBuildManager.Console.CommandLine;
using SqlBuildManager.Console.KeyVault;
using SqlBuildManager.Console.Kubernetes.Yaml;
using SqlBuildManager.Console.Shared;
using SqlSync.Connection;
using System;
using System.IO;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace SqlBuildManager.Console.Kubernetes
{
    public class KubernetesManager
    {
        public static readonly string SbmNamespace = "sqlbuildmanager";
        private static ILogger log = SqlBuildManager.Logging.ApplicationLogging.CreateLogger(System.Reflection.MethodBase.GetCurrentMethod().DeclaringType);

        internal static (bool, CommandLineArgs) ReadConfigmapParameters(CommandLineArgs args)
        {

            try
            {
                //these are required and should fail of not there...
                args.BuildFileName = File.ReadAllText("/etc/runtime/PackageName");
                log.LogDebug($"sbmName= {args.BuildFileName}");

                args.BatchJobName = File.ReadAllText("/etc/runtime/JobName");
                log.LogDebug($"storageContainerName= {args.BatchArgs.BatchJobName}");
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to read runtime parameters: {ex.Message}");
                log.LogWarning("The runtime variables for running a container/pod worker have not been set. They should be set as a Configmap mounted to /etc/runtime");
                return (false, args);
            }

            if (File.Exists("/etc/runtime/DacpacName"))
            {
                args.PlatinumDacpac = File.ReadAllText("/etc/runtime/DacpacName");
                log.LogDebug($"dacpacName= {args.DacPacArgs.PlatinumDacpac}");
            }

            if (File.Exists("/etc/runtime/Concurrency"))
            {
                if (int.TryParse(File.ReadAllText("/etc/runtime/Concurrency"), out int val))
                {
                    args.Concurrency = val;
                }
                log.LogDebug($"concurrency= {args.Concurrency}");
            }

            if (File.Exists("/etc/runtime/ConcurrencyType"))
            {
                if (Enum.TryParse<ConcurrencyType>(File.ReadAllText("/etc/runtime/ConcurrencyType"), out ConcurrencyType ct))
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

            if (File.Exists("/etc/runtime/KeyVaultName"))
            {
                args.ConnectionArgs.KeyVaultName = File.ReadAllText("/etc/runtime/KeyVaultName");
                log.LogDebug($"KeyVaultName= {args.ConnectionArgs.KeyVaultName}");
            }


            return (true, args);
        }

        internal static bool ApplyDeployment(KubernetesFiles files)
        {
            var returnCode = 0;
            log.LogInformation($"Creating 'sqlbuildmanager' namespace (if not already exists)");
            if (KubectlProcess.DescribeKubernetesResource("namespace", KubernetesManager.SbmNamespace) != 0)
            {
                returnCode += KubectlProcess.CreateKubernetesResource("namespace", KubernetesManager.SbmNamespace);
            }

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
        internal static bool CleanUpKubernetesResource(bool secretsExist, string k8jobName = "")
        {
            if (!string.IsNullOrWhiteSpace(k8jobName))
            {
                MonitorForPodComplete(k8jobName);
            }
            var returnCode = 0;
            returnCode += KubectlProcess.DeleteKubernetesResource(JobYaml.Kind, JobYaml.k8jobname, KubernetesManager.SbmNamespace);
            if (secretsExist)
            {
                returnCode += KubectlProcess.DeleteKubernetesResource(SecretYaml.Kind, SecretYaml.Name, KubernetesManager.SbmNamespace);
            }
            returnCode += KubectlProcess.DeleteKubernetesResource(ConfigmapYaml.Kind, ConfigmapYaml.k8ConfigMapName, KubernetesManager.SbmNamespace);
            return returnCode == 0;
        }

        internal static bool MonitorForPodStart(string k8Jobname)
        {
            int pendingCounter = 0;
            int runningCounter = 0;
            while (true)
            {
                var stat = KubectlProcess.GetJobPodStatus(k8Jobname, true);
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
        internal static bool MonitorForPodComplete(string k8Jobname)
        {
            log.LogInformation("Ensuring all pods have completed post-processing...");
            int errorCounter = 0;
            while (true)
            {
                var stat = KubectlProcess.GetJobPodStatus(k8Jobname, false);
                switch (stat)
                {
                    case PodStatus.Running:
                    case PodStatus.Pending:
                    case PodStatus.Unknown:
                        errorCounter = 0;
                        continue;
                    case PodStatus.Error:
                        if (errorCounter > 3)
                        {
                            return false;
                        }
                        else
                        {
                            errorCounter++;
                        }
                        break;
                    case PodStatus.Completed:
                        return true;
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

            string jobYaml = KubernetesManager.GenerateJobYaml(cmdLine);
            var jobYamlFileName = Path.Combine(dir, string.IsNullOrWhiteSpace(prefix) ? "job.yaml" : $"{prefix}-job.yaml");
            File.WriteAllText(jobYamlFileName, jobYaml);
            log.LogInformation($"Job Yaml file written to: {jobYamlFileName}");

            var kf = new KubernetesFiles
            {
                RuntimeConfigMapFile = cfgMapName,
                SecretsFile = secretsName,
                JobFileName = jobYamlFileName,
            };

            return Task.Run(() => (kf));
        }

        #region Dynamic Yaml Generation

        internal static string GenerateJobYaml(CommandLineArgs args)
        {
            bool hasKeyVault = !string.IsNullOrWhiteSpace(args.ConnectionArgs.KeyVaultName);
            bool useMangedIdenty = args.AuthenticationArgs.AuthenticationType == AuthenticationType.ManagedIdentity;
            string k8jobname = KubernetesJobName(args);
            string k8ConfigMapName = KubernetesConfigmapName(args);
            string k8SecretsName = KubernetesSecretsName(args);
            string k8SecretsProviderName = KubernetesSecretProviderClassName(args);
            var yml = new Yaml.JobYaml(k8jobname, k8ConfigMapName, k8SecretsName, k8SecretsProviderName, args.ContainerRegistryArgs.RegistryServer, args.ContainerRegistryArgs.ImageName, args.ContainerRegistryArgs.ImageTag, hasKeyVault, useMangedIdenty, args.IdentityArgs.ServiceAccountName);
            var serializer = new SerializerBuilder().ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull).Build();
            var jobYaml = serializer.Serialize(yml);


            return jobYaml;

        }
        internal static string GenerateSecretsYaml(CommandLineArgs args)
        {
            bool secretAdded = false;
            string k8SecretsName = KubernetesSecretsName(args);
            var yml = new Yaml.SecretYaml(k8SecretsName);


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
            string k8jobname = KubernetesJobName(args);
            string k8ConfigMapName = KubernetesConfigmapName(args);


            var yml = new Yaml.ConfigmapYaml(k8ConfigMapName);
            yml.data.DacpacName = Path.GetFileName(args.DacPacArgs.PlatinumDacpac);
            yml.data.PackageName = Path.GetFileName(args.BuildFileName);
            yml.data.JobName = args.JobName; //NOT the k8jobname!
            yml.data.AllowObjectDelete = args.AllowObjectDelete.ToString();
            yml.data.Concurrency = args.Concurrency.ToString();
            yml.data.ConcurrencyType = args.ConcurrencyType.ToString();
            yml.data.AuthType = args.AuthenticationArgs.AuthenticationType.ToString();
            yml.data.KeyVaultName = args.ConnectionArgs.KeyVaultName;

            if (!string.IsNullOrWhiteSpace(args.ConnectionArgs.ServiceBusTopicConnectionString) && !ConnectionValidator.IsServiceBusConnectionString(args.ConnectionArgs.ServiceBusTopicConnectionString))
            {
                yml.data.ServiceBusTopicConnectionString = args.ConnectionArgs.ServiceBusTopicConnectionString;
            }
            if (!string.IsNullOrWhiteSpace(args.ConnectionArgs.EventHubConnectionString) && !ConnectionValidator.IsEventHubConnectionString(args.ConnectionArgs.EventHubConnectionString))
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

        internal static string KubernetesJobName(CommandLineArgs args)
        {
            return $"sbm-{args.JobName.ToLower()}-job";
        }
        internal static string KubernetesConfigmapName(CommandLineArgs args)
        {
            return $"sbm-{args.JobName.ToLower()}-configmap";
        }
        internal static string KubernetesSecretsName(CommandLineArgs args)
        {
            return $"sbm-{args.JobName.ToLower()}-secrets";
        }
        internal static string KubernetesSecretProviderClassName(CommandLineArgs args)
        {
            return $"sbm-{args.JobName.ToLower()}-secprovider";
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
            if (args == null)
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

        internal static (bool, CommandLineArgs) ReadOpaqueSecrets(CommandLineArgs args)
        {
            if (args == null)
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

                if (args.AuthenticationArgs.AuthenticationType != AuthenticationType.ManagedIdentity)
                {
                    args.Password = File.ReadAllText("/etc/sbm/Password");
                    log.LogDebug($"password= {args.AuthenticationArgs.Password}");

                    args.UserName = File.ReadAllText("/etc/sbm/UserName");
                    log.LogDebug($"username= {args.AuthenticationArgs.UserName}");
                }

                return (true, args);
            }
            catch (Exception ex)
            {
                log.LogError($"Unable to read secrets: {ex.Message}");
                log.LogWarning("The secrets needed for running a container/pod worker have not been set.They should be set as secrets mounted to /etc/sbm");
                return (false, args);
            }
        }
        private static CommandLineArgs GetArgumentsFromConfigMapFile(string filename, CommandLineArgs args)
        {
            if (args == null)
            {
                args = new CommandLineArgs();
            }

            var tmp = GetValueFromConfigMapstring(filename, "PackageName");
            if (!string.IsNullOrWhiteSpace(tmp)) args.BuildFileName = tmp;


            tmp = GetValueFromConfigMapstring(filename, "JobName");
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

            tmp = GetValueFromConfigMapstring(filename, "KeyVaultName");
            if (!string.IsNullOrWhiteSpace(tmp)) args.ConnectionArgs.KeyVaultName = tmp;

            return args;

        }


    }
}
