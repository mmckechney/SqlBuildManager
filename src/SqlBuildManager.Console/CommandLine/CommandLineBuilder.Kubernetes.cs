using Microsoft.Extensions.Logging;
using Microsoft.SqlServer.Management.Smo;
using Spectre.Console;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Help;
using System.CommandLine.Parsing;
using System.Diagnostics;
using System.IO;
using System.Linq;
using SqlBuildManager.Console.ContainerShared;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        // Local option for kubernetes monitor flag
        private static Option<bool> k8sMonitorOption = new Option<bool>("--monitor") { Description = "Immediately start monitoring progress after successful pod deployment" };
        private static Option<bool> k8sCleanupOnFailureOption = new Option<bool>("--cleanup-onfailure") { Description = "Cleanup the Kubernetes applied resources (job, configmap, etc) if there is a job failure" };
        
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
                    k8sCleanupOnFailureOption,
                    imageTagOption,
                    imageNameOption,
                    imageRepositoryOption
                });
                cmd.AddRange(IdentityArgumentsForKubernetes);
                cmd.AddRange(ConnectionAndSecretsOptions);
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ConcurrencyOptions);
                //IdentityArgumentsForContainerApp.ForEach(o => { if (o.Name == "identityname") { o.Required = false; } cmd.Add(o); });
                cmd.Add(subscriptionIdOption);
                cmd.Add(unitTestOption);


                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var overrideFile = parseResult.GetValue(overrideAsFileOption);
                    var packagename = parseResult.GetValue(packagenameAsFileToUploadOption);
                    var platinumdacpac = parseResult.GetValue(platinumdacpacFileInfoOption);
                    var force = parseResult.GetValue(forceOption);
                    var allowObjectDelete = parseResult.GetValue(allowForObjectDeletionOption);
                    var cleanupOnFailure = parseResult.GetValue(k8sCleanupOnFailureOption);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    return await Worker.KubernetesRun(cmdLine, overrideFile!, packagename!, platinumdacpac!, force, allowObjectDelete, cleanupOnFailure, unittest, stream);
                });
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
                    overrideRequiredOption,
                    queryFileOption,
                    outputFileOption,
                    forceOption,
                    k8sCleanupOnFailureOption,
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


                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var force = parseResult.GetValue(forceOption);
                    var cleanupOnFailure = parseResult.GetValue(k8sCleanupOnFailureOption);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    return await Worker.KubernetesQuery(cmdLine, force, stream, unittest, cleanupOnFailure);
                });
                return cmd;

            }
        }

        ///// <summary>
        ///// Helper command to create yaml files from a settings json file and runtime parameters
        ///// </summary>
        //private static Command KubernetesCreateYamlCommand
        //{
        //    get
        //    {
        //        var cmd = new Command("createyaml", "Helper command to create yaml files from a settings json file and runtime parameters");
        //        cmd.AddRange(SettingsFileExistingOptions);
        //        cmd.AddRange(new List<Option>
        //        {
        //            sectionPlaceholderOption,
        //            pathOption,
        //            prefixOption,
        //            jobnameOption,
        //            podCountOption,
        //            packagenameAsFileToUploadOption,
        //            platinumdacpacFileInfoOption,
        //            forceOption,

        //            imageTagOption,
        //            imageNameOption,
        //            imageRepositoryOption,

        //            serviceAccountNameOption

        //        });
        //        cmd.SetAction(async (parseResult, ct) => {
        //            var cmdLine = CommandLineArgsBinder.Bind(parseResult);
        //            var path = parseResult.GetValue(pathOption);
        //            var prefix = parseResult.GetValue(prefixOption);
        //            var packagename = parseResult.GetValue(packagenameAsFileToUploadOption);
        //            var platinumdacpac = parseResult.GetValue(platinumdacpacFileInfoOption);
        //            var force = parseResult.GetValue(forceOption);
        //            return await Worker.KubernetesSaveYamlFiles(cmdLine, path, prefix, packagename, platinumdacpac, force);
        //        });
        //        return cmd;
        //    }

        //}

        /// <summary>
        /// [Used by Kubernetes] Starts the pod as a worker for database querying - polling and retrieving items from target service bus queue topic
        /// </summary>
        private static Command KubernetesQueryWorkerCommand
        {
            get
            {
                var cmd = new Command("query", "[Used by Kubernetes] Starts the pod as a worker for database querying - polling and retrieving items from target service bus queue topic");
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.KubernetesWorker_RunQueueQuery(cmdLine);
                });

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
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.KubernetesWorker_RunQueueBuild(cmdLine);
                });
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

                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var secretsFile = parseResult.GetValue(secretsFileOption);
                    var runtimeFile = parseResult.GetValue(runtimeFileOption);
                    var keyvaultname = parseResult.GetValue(keyVaultNameOption);
                    var jobname = parseResult.GetValue(jobnameOption);
                    var concurrencytype = parseResult.GetValue(threadedConcurrencyTypeOption);
                    var servicebustopicconnection = parseResult.GetValue(serviceBusconnectionOption);
                    var overrideFile = parseResult.GetValue(overrideAsFileOption);
                    return await Worker.KubernetesEnqueueOverrideTargets(cmdLine, secretsFile!, runtimeFile!, keyvaultname!, jobname!, concurrencytype, servicebustopicconnection!, overrideFile!);
                });


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
                cmd.Add(silentOption);


                cmd.SetAction((parseResult) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var unittest = parseResult.GetValue(unitTestOption);
                    return Worker.SaveAndEncryptKubernetesSettings(cmdLine, unittest);
                });
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
                    storageaccountnameOption,
                    storageaccountkeyOption,
                    threadedConcurrencyTypeOption,
                });
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Add(eventhubconnectionOption);
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(kubernetesYamlFileOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var secretsFile = parseResult.GetValue(secretsFileOption);
                    var runtimeFile = parseResult.GetValue(runtimeFileOption);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    var consolidateLogs = true; // default parameter
                    return await Worker.KubernetesMonitorRuntimeProgress(cmdLine, secretsFile!, runtimeFile!, unittest, stream, consolidateLogs);
                });
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
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var secretsFile = parseResult.GetValue(secretsFileOption);
                    var runtimeFile = parseResult.GetValue(runtimeFileOption);
                    var packagename = parseResult.GetValue(packagenameAsFileToUploadOption);
                    var platinumdacpac = parseResult.GetValue(platinumdacpacFileInfoOption);
                    var force = parseResult.GetValue(forceOption);
                    var (retVal, _) = await Worker.KubernetesUploadBuildPackage(cmdLine, secretsFile!, runtimeFile!, packagename!, platinumdacpac!, force);
                    return retVal;
                });
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
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var secretsFile = parseResult.GetValue(secretsFileOption);
                    var runtimeFile = parseResult.GetValue(runtimeFileOption);
                    var keyvaultname = parseResult.GetValue(keyVaultNameOption);
                    var jobname = parseResult.GetValue(jobnameOption);
                    var concurrencytype = parseResult.GetValue(threadedConcurrencyTypeOption);
                    var servicebustopicconnection = parseResult.GetValue(serviceBusconnectionOption);
                    return await Worker.KubernetesDequeueOverrideTargets(cmdLine, secretsFile!, runtimeFile!, keyvaultname!, jobname!, concurrencytype, servicebustopicconnection!);
                });
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
                //cmd.Add(KubernetesCreateYamlCommand);
                cmd.Add(KubernetesQueryCommand);
                cmd.Add(KubernetesWorkerCommand);
                cmd.Aliases.Add("aks");
                return cmd;
            }
        }
    }
}
