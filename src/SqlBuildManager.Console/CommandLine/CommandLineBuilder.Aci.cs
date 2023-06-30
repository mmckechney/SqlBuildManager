using SqlBuildManager.Console.ContainerShared;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.NamingConventionBinder;
using System.IO;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        /// <summary>
        /// Saves settings file for Azure Container Instances container deployments
        /// </summary>
        private static Command AciSaveSettingsCommand
        {
            get
            {
                var cmd = new Command("savesettings", "Saves settings file for Azure Container Instances container deployments")
                {
                    settingsfileNewOption,
                    aciIResourceGroupNameOption,
                    aciInstanceNameOption,
                    subscriptionIdOption,
                    eventHubLoggingTypeOption,
                    //Key value option
                    sectionPlaceholderOption,
                    keyVaultNameOption.Copy(true),
                    storageaccountnameOption,
                    storageaccountkeyOption,
                    eventhubconnectionOption,
                    defaultscripttimeoutOption,
                    //threadedConcurrencyOption,
                    //threadedConcurrencyTypeOption,
                    //Service Bus queue options
                    serviceBusconnectionOption,
                    //Additional settings
                    timeoutretrycountOption,
                    silentOption,
                    cleartextOption

                };
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(VnetOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(IdentityArgumentsForContainerApp);
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool>(Worker.SaveAndEncryptAciSettings);

                return cmd;
            }
        }

        /// <summary>
        /// Runs an ACI build (orchestrates the prep, enqueue, deploy and monitor commands
        /// </summary>
        private static Command AciRun
        {

            get
            {
                var cmd = new Command("run", "Runs an ACI build (orchestrates the prep, enqueue, deploy and monitor commands")
                {
                    sectionPlaceholderOption,
                    jobnameOption.Copy(true),
                    packagenameAsFileToUploadOption,
                    overrideOption.Copy(true),
                    platinumdacpacFileInfoOption,
                    allowForObjectDeletionOption,
                    eventHubLoggingTypeOption,
                    new Option<bool>("--monitor", () => true, "Immediately start monitoring progress after successful ACI container deployment"),
                    unitTestOption,
                    streamEventsOption,
                    forceOption,

                    settingsfileExistingOption,
                    aciIResourceGroupNameNotReqOption,
                    aciInstanceNameNotReqOption,
                    aciContainerCountOption.Copy(true),

                };
                cmd.AddRange(VnetOptions);
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.Add(keyVaultNameOption);
                cmd.Add(storageaccountnameOption);
                cmd.Add(storageaccountkeyOption);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.Add(clientIdOption);
                cmd.Add(identityNameOption.Copy(false));
                cmd.Add(identityResourceGroupOption.Copy(false));
                cmd.Add(subscriptionIdOption.Copy(false));
                cmd.AddRange(ConcurrencyRequiredOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, FileInfo, bool, bool, bool, bool>(Worker.AciRun);
                return cmd;
            }
        }

        /// <summary>
        /// Run a SELECT query across multiple databases using ACI
        /// </summary>
        private static Command AciQuery
        {
            get
            {

                var cmd = new Command("query", "Run a SELECT query across multiple databases using ACI.");
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(new List<Option>
                {
                    sectionPlaceholderOption,
                    jobnameOption,
                    new Option<bool>("--monitor", () => true, "Immediately start monitoring progress after successful ACI container deployment"),
                     unitTestOption,
                    overrideAsFileOption,
                    queryFileOption,
                    outputFileOption,

                    forceOption,
                    streamEventsOption,
                    aciIResourceGroupNameNotReqOption,
                    aciInstanceNameNotReqOption,
                    aciContainerCountOption.Copy(true),
                    eventHubLoggingTypeOption,
                });
                cmd.AddRange(VnetOptions);
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.Add(keyVaultNameOption);
                cmd.Add(storageaccountnameOption);
                cmd.Add(storageaccountkeyOption);
                cmd.Add(authtypeOption);
                cmd.Add(clientIdOption);
                cmd.Add(identityNameOption.Copy(false));
                cmd.Add(identityResourceGroupOption.Copy(false));
                cmd.Add(subscriptionIdOption.Copy(false));
                cmd.AddRange(ConcurrencyRequiredOptions);


                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, bool, bool, bool, bool>(Worker.AciQuery);
                return cmd;

            }
        }

        /// <summary>
        /// Creates an Azure storage container and uploads the SBM and/or DACPAC files that will be used for the build.
        /// </summary>
        private static Command AciPrepCommand
        {
            get
            {
                var cmd = new Command("prep", "Creates an Azure storage container and uploads the SBM and/or DACPAC files that will be used for the build.")
                {
                    jobnameOption.Copy(true),
                    storageaccountnameOption,
                    storageaccountkeyOption,
                    packagenameAsFileToUploadOption,
                    platinumdacpacFileInfoOption,
                    overrideOption,
                    decryptedOption,
                    forceOption,
                    allowForObjectDeletionOption

                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, FileInfo, bool>(GenericContainer.PrepAndUploadContainerBuildPackage);

                return cmd;
            }
        }

        /// <summary>
        /// Sends database override targets to Service Bus Topic
        /// </summary>
        private static Command AciEnqueueTargetsCommand
        {
            get
            {
                var cmd = new Command("enqueue", "Sends database override targets to Service Bus Topic")
                {
                    settingsfileExistingOption,
                    jobnameOption.Copy(true),
                    threadedConcurrencyTypeOption.Copy(true),
                    overrideOption.Copy(true)
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.EnqueueOverrideTargets);

                return cmd;
            }
        }

        /// <summary>
        /// Deploy the ACI instance and start containers
        /// </summary>
        private static Command AciDeployCommand
        {
            get
            {
                var cmd = new Command("deploy", "Deploy the ACI instance and start containers")
                {
                    settingsfileExistingOption,
                    aciIResourceGroupNameNotReqOption,
                    aciInstanceNameNotReqOption,
                    aciContainerCountOption.Copy(true),
                    eventHubLoggingTypeOption,
                    identityNameOption.Copy(false),
                    identityResourceGroupOption.Copy(false),
                    sectionPlaceholderOption,
                    jobnameOption.Copy(true),
                    packagenameAsFileToUploadOption,
                    overrideOption.Copy(false),
                    platinumdacpacFileInfoOption,
                    allowForObjectDeletionOption,
                    new Option<bool>("--monitor", () => true, "Immediately start monitoring progress after successful ACI container deployment"),
                    unitTestOption,
                    streamEventsOption
                };
                cmd.AddRange(VnetOptions);
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.Add(keyVaultNameOption);
                cmd.Add(storageaccountnameOption);
                cmd.Add(storageaccountkeyOption);
                cmd.Add(authtypeOption);
                cmd.Add(clientIdOption);
                cmd.Add(subscriptionIdOption.Copy(false));
                cmd.AddRange(ConcurrencyRequiredOptions);

                cmd.Handler = CommandHandler.Create<CommandLineArgs, FileInfo, FileInfo, bool, bool, bool>(Worker.AciDeploy);
                return cmd;
            }
        }
        
        /// <summary>
        /// Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)
        /// </summary>
        private static Command AciMonitorCommand
        {
            get
            {
                var cmd = new Command("monitor", "Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)")
                {
                    settingsfileExistingOption,
                    jobnameOption,
                    overrideOption,
                    threadedConcurrencyTypeOption,
                    unitTestOption,
                    streamEventsOption
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Add(eventhubconnectionOption);
                cmd.AddRange(EventHubResourceOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, DateTime?, bool, bool>(Worker.AciMonitorRuntimeProgress);

                return cmd;
            }
        }

        /// <summary>
        /// Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them"
        /// </summary>
        private static Command AciDequeueTargetsCommand
        {
            get
            {
                var cmd = new Command("dequeue", "Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them")
                {
                    settingsfileExistingOption,
                    jobnameOption.Copy(true),
                    threadedConcurrencyTypeOption
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.DeQueueOverrideTargets);

                return cmd;
            }
        }

        /// <summary>
        /// [Used by ACI] Starts the container(s) as a worker for database querying - polling and retrieving items from target service bus queue topic
        /// </summary>
        private static Command AciQueryWorkerCommand
        {
            get
            {
                var cmd = new Command("query", "[Used by ACI] Starts the container(s) as a worker for database querying - polling and retrieving items from target service bus queue topic");
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.AciWorker_RunQueueQuery);
                return cmd;
            }
        }

        /// <summary>
        /// [Used by ACI] Starts the container(s) as a worker - polling and retrieving items from target service bus queue topic
        /// </summary>
        private static Command AciWorkerCommand
        {
            get
            {
                var cmd = new Command("worker", "[Used by ACI] Starts the container(s) as a worker - polling and retrieving items from target service bus queue topic");
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.AciWorker_RunQueueBuild);
                cmd.Add(AciQueryWorkerCommand);
                return cmd;
            }
        }
        
        /// <summary>
        /// Commands for setting and executing a build running in containers on Azure Container Instances. ACI Containers will always leverage Azure Key Vault.
        /// </summary>
        private static Command AciCommand
        {
            get
            {
                var cmd = new Command("aci", "Commands for setting and executing a build running in containers on Azure Container Instances. ACI Containers will always leverage Azure Key Vault.");
                cmd.Add(AciSaveSettingsCommand);
                cmd.Add(AciRun);
                cmd.Add(AciPrepCommand);
                cmd.Add(AciEnqueueTargetsCommand);
                cmd.Add(AciDeployCommand);
                cmd.Add(AciMonitorCommand);
                cmd.Add(AciQuery);
                cmd.Add(AciDequeueTargetsCommand);
                cmd.Add(AciWorkerCommand);

                return cmd;
            }
        }
    }
}
