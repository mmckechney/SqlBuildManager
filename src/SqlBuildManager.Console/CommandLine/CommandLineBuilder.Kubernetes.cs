using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SqlBuildManager.Console.ContainerShared;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        /// <summary>
        /// "Run a build in Kubernetes (Orchestrates the prep, enqueue and monitor commands as well as kubectl)
        /// </summary>
        private static Command KubernetesRunCommand
        {
            get
            {

                var cmd = new Command("run", "Run a build in Kubernetes (Orchestrates the prep, enqueue and monitor commands as well as kubectl). [NOTE: 'kubectl' must be installed and in your path]");
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(new List<Option>
                {
                    sectionPlaceholderOption,
                    jobnameOption,
                    podCountOption,
                    overrideAsFileOption,
                    packagenameAsFileToUploadOption,
                    platinumdacpacFileInfoOption,
                    forceOption,
                    allowForObjectDeletionOption,
                    streamEventsOption,
                    eventHubLoggingTypeOption,
                    new Option<bool>("--cleanup-onfailure", () => true, "Cleanup the Kubernetes applied resources (job, configmap, etc) if there is a job failure"),
                    imageTagOption,
                    imageNameOption,
                    imageRepositoryOption
                });
                cmd.AddRange(IdentityArgumentsForKubernetes);
                cmd.AddRange(ConnectionAndSecretsOptions);
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConcurrencyOptions);
                //IdentityArgumentsForContainerApp.ForEach(o => { if (o.Name == "identityname") { o.IsRequired = false; } cmd.Add(o); });
                cmd.Add(subscriptionIdOption);
                cmd.Add(unitTestOption);


                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, FileInfo, FileInfo, bool, bool, bool, bool, bool>(Worker.KubernetesRun);
                return cmd;

            }
        }
        
        /// <summary>
        /// Run a SELECT query across multiple databases using Kubernetes
        /// </summary>
        private static Command KubernetesQueryCommand
        {
            get
            {

                var cmd = new Command("query", "Run a SELECT query across multiple databases using Kubernetes. [NOTE: 'kubectl' must be installed and in your path]");
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(new List<Option>
                {
                    sectionPlaceholderOption,
                    jobnameOption,
                    podCountOption,
                    overrideAsFileOption,
                    queryFileOption,
                    outputFileOption,
                    forceOption,
                    streamEventsOption,
                    eventHubLoggingTypeOption,
                    new Option<bool>("--cleanup-onfailure", () => true, "Cleanup the Kubernetes applied resources (job, configmap, etc) if there is a job failure"),
                    imageTagOption,
                    imageNameOption,
                    imageRepositoryOption
                });
                cmd.AddRange(IdentityArgumentsForKubernetes);
                cmd.AddRange(ConnectionAndSecretsOptions);
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConcurrencyOptions);
                cmd.Add(subscriptionIdOption);
                cmd.Add(unitTestOption);


                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, bool, bool, bool, bool>(Worker.KubernetesQuery);
                return cmd;

            }
        }

        /// <summary>
        /// Helper command to create yaml files from a settings json file and runtime parameters
        /// </summary>
        private static Command KubernetesCreateYamlCommand
        {
            get
            {
                var cmd = new Command("createyaml", "Helper command to create yaml files from a settings json file and runtime parameters");
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(new List<Option>
                {
                    sectionPlaceholderOption,
                    pathOption,
                    prefixOption,
                    jobnameOption,
                    podCountOption,
                    packagenameAsFileToUploadOption,
                    platinumdacpacFileInfoOption,
                    forceOption,

                    imageTagOption,
                    imageNameOption,
                    imageRepositoryOption,

                    serviceAccountNameOption

                });
                cmd.Handler = CommandHandler.Create<CommandLineArgs, DirectoryInfo, string, FileInfo, FileInfo, bool>(Worker.KubernetesSaveYamlFiles);
                return cmd;
            }

        }

        /// <summary>
        /// [Used by Kubernetes] Starts the pod as a worker for database querying - polling and retrieving items from target service bus queue topic
        /// </summary>
        private static Command KubernetesQueryWorkerCommand
        {
            get
            {
                var cmd = new Command("query", "[Used by Kubernetes] Starts the pod as a worker for database querying - polling and retrieving items from target service bus queue topic");
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.KubernetesWorker_RunQueueQuery);

                return cmd;
            }
        }

        /// <summary>
        /// [Used by Kubernetes] Starts the pod as a worker - polling and retrieving items from target service bus queue topic
        /// </summary>
        private static Command KubernetesWorkerCommand
        {
            get
            {
                var cmd = new Command("worker", "[Used by Kubernetes] Starts the pod as a worker - polling and retrieving items from target service bus queue topic");
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.KubernetesWorker_RunQueueBuild);
                cmd.Add(KubernetesQueryWorkerCommand);
                return cmd;
            }
        }

        /// <summary>
        /// Sends database override targets to Service Bus Topic
        /// </summary>
        private static Command KubernetesEnqueueTargetsCommand
        {
            get
            {
                var cmd = new Command("enqueue", "Sends database override targets to Service Bus Topic");
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(new List<Option>
                {
                    sectionPlaceholderOption,
                    jobnameOption,
                    overrideAsFileOption,
                    threadedConcurrencyTypeOption,
                    keyVaultNameOption,
                    serviceBusconnectionOption,
                });
                cmd.AddRange(kubernetesYamlFileOptions);

                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, FileInfo, string, string, ConcurrencyType, string, FileInfo>(Worker.KubernetesEnqueueOverrideTargets);


                return cmd;
            }
        }

        /// <summary>
        /// Saves settings file for Kubernetes deployments
        /// </summary>
        private static Command KubernetesSaveSettingsCommand
        {
            get
            {
                var cmd = new Command("savesettings", "Saves settings file for Kubernetes deployments");
                cmd.AddRange(SettingsFileNewOptions);
                cmd.Add(podCountOption);
                cmd.Add(imageTagOption);
                cmd.Add(imageNameOption);
                cmd.Add(imageRepositoryOption);
                cmd.AddRange(IdentityArgumentsForKubernetes);
                cmd.Add(subscriptionIdOption);
                cmd.AddRange(ConnectionAndSecretsOptions);
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConcurrencyOptions);
                cmd.Add(sectionPlaceholderOption);
                cmd.Add(eventHubLoggingTypeOption);
                cmd.Add(silentOption);


                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool>(Worker.SaveAndEncryptKubernetesSettings);
                return cmd;
            }
        }

        /// <summary>
        /// Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)
        /// </summary>
        private static Command KubernetesMonitorCommand
        {
            get
            {
                var cmd = new Command("monitor", "Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)");
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(new List<Option>
                {
                    sectionPlaceholderOption,
                    jobnameOption,
                    overrideOption,
                    unitTestOption,
                    streamEventsOption,
                    storageaccountnameOption,
                    storageaccountkeyOption,
                    threadedConcurrencyTypeOption,
                });
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Add(eventhubconnectionOption);
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(kubernetesYamlFileOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, FileInfo, bool, bool, bool>(Worker.KubernetesMonitorRuntimeProgress);
                return cmd;
            }
        }

        /// <summary>
        /// Creates a storage container and uploads the SBM and/or DACPAC files that will be used for the build. If the --runtimefile option is provided, it will also update that file with the updated values
        /// </summary>
        private static Command KubernetesPrepCommand
        {
            get
            {
                var cmd = new Command("prep", "Creates a storage container and uploads the SBM and/or DACPAC files that will be used for the build. If the --runtimefile option is provided, it will also update that file with the updated values");
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(new List<Option>
                {
                    sectionPlaceholderOption,
                    jobnameOption,
                    packagenameAsFileToUploadOption,
                    platinumdacpacFileInfoOption,
                    overrideOption,
                    forceOption,
                    allowForObjectDeletionOption,

                    keyVaultNameOption,
                    storageaccountnameOption,
                    storageaccountkeyOption

                });
                cmd.AddRange(kubernetesYamlFileOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, FileInfo, FileInfo, FileInfo, bool>(Worker.KubernetesUploadBuildPackage);
                return cmd;
            }
        }

        /// <summary>
        /// Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them
        /// </summary>
        private static Command KubernetesDequeueTargetsCommand
        {
            get
            {
                var cmd = new Command("dequeue", "Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them");
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(new List<Option>
                {
                    sectionPlaceholderOption,
                    jobnameOption,
                    keyVaultNameOption,
                    serviceBusconnectionOption,
                    threadedConcurrencyTypeOption

                });
                cmd.AddRange(kubernetesYamlFileOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, FileInfo, string, string, ConcurrencyType, string>(Worker.KubernetesDequeueOverrideTargets);
                return cmd;
            }
        }

        /// <summary>
        /// Commands for setting and executing a build running in pods on Kubernetes
        /// </summary>
        private static Command KubernetesCommand
        {
            get
            {
                var cmd = new Command("k8s", "Commands for setting and executing a build running in pods on Kubernetes");
                cmd.Add(KubernetesSaveSettingsCommand);
                cmd.Add(KubernetesRunCommand);
                cmd.Add(KubernetesPrepCommand);
                cmd.Add(KubernetesEnqueueTargetsCommand);
                cmd.Add(KubernetesMonitorCommand);
                cmd.Add(KubernetesDequeueTargetsCommand);
                cmd.Add(KubernetesCreateYamlCommand);
                cmd.Add(KubernetesQueryCommand);
                cmd.Add(KubernetesWorkerCommand);
                cmd.AddAlias("aks");
                return cmd;
            }
        }
    }
}
