using Azure.ResourceManager.AppContainers.Models;
using SqlBuildManager.Console.ContainerShared;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        // Local option for monitor flag used in ACI commands
        private static Option<bool> aciMonitorOption = new Option<bool>("--monitor") { Description = "Immediately start monitoring progress after successful ACI container deployment" };

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
                    //Key value option
                    sectionPlaceholderOption,
                    keyVaultNameOption,
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
                cmd.SetGroupedHelp(
                    new OptionGroup("Settings File", new List<Option> { settingsfileNewOption }),
                    new OptionGroup("ACI", new List<Option> { aciIResourceGroupNameOption, aciInstanceNameOption, subscriptionIdOption }),
                    new OptionGroup("Execution", new List<Option> { sectionPlaceholderOption, defaultscripttimeoutOption, timeoutretrycountOption, silentOption, cleartextOption }),
                    new OptionGroup("Connections & Secrets", new List<Option> { keyVaultNameOption, storageaccountnameOption, storageaccountkeyOption, eventhubconnectionOption, serviceBusconnectionOption }),
                    new OptionGroup("Event Hub", EventHubResourceOptions),
                    new OptionGroup("VNET", VnetOptions),
                    new OptionGroup("Authentication", DatabaseAuthArgs),
                    new OptionGroup("Identity", IdentityArgumentsForContainerApp),
                    new OptionGroup("Container Registry", ContainerRegistryAndImageOptions)
                );
                cmd.SetAction((parseResult) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var clearText = parseResult.GetValue(cleartextOption);
                    return Worker.SaveAndEncryptAciSettings(cmdLine: cmdLine, clearText: clearText);
                });

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
                    jobnameRequiredOption,
                    packagenameAsFileToUploadOption,
                    overrideRequiredOption,
                    platinumdacpacFileInfoOption,
                    allowForObjectDeletionOption,
                    aciMonitorOption,
                    unitTestOption,
                    forceOption,

                    settingsfileExistingOption,
                    aciIResourceGroupNameNotReqOption,
                    aciInstanceNameNotReqOption,
                    aciContainerCountOption,

                };
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(VnetOptions);
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.Add(keyVaultNameOption);
                cmd.Add(storageaccountnameOption);
                cmd.Add(storageaccountkeyOption);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.Add(clientIdOption);
                cmd.Add(identityNameOption);
                cmd.Add(identityResourceGroupOption);
                cmd.Add(subscriptionIdOption);
                cmd.AddRange(ConcurrencyRequiredOptions);
                cmd.SetGroupedHelp(
                    new OptionGroup("Build Options", new List<Option> { jobnameRequiredOption, packagenameAsFileToUploadOption, overrideRequiredOption, platinumdacpacFileInfoOption, allowForObjectDeletionOption, aciMonitorOption, forceOption }),
                    new OptionGroup("ACI", new List<Option> { aciIResourceGroupNameNotReqOption, aciInstanceNameNotReqOption, aciContainerCountOption }),
                    new OptionGroup("Event Hub", EventHubResourceOptions),
                    new OptionGroup("VNET", VnetOptions),
                    new OptionGroup("Container Registry", ContainerRegistryAndImageOptions),
                    new OptionGroup("Connections & Secrets", new List<Option> { keyVaultNameOption, storageaccountnameOption, storageaccountkeyOption }),
                    new OptionGroup("Authentication", DatabaseAuthArgs),
                    new OptionGroup("Identity", new List<Option> { clientIdOption, identityNameOption, identityResourceGroupOption, subscriptionIdOption }),
                    new OptionGroup("Concurrency", ConcurrencyRequiredOptions)
                );
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var packagename = parseResult.GetValue(packagenameAsFileToUploadOption);
                    var platinumdacpac = parseResult.GetValue(platinumdacpacFileInfoOption);
                    var monitor = parseResult.GetValue(aciMonitorOption);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var force = parseResult.GetValue(forceOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    return await Worker.AciRun(cmdLine: cmdLine, packageName: packagename!, platinumDacpac: platinumdacpac!, unittest: unittest, stream: stream, monitor: monitor, force: force);
                });
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
                    aciMonitorOption,
                    unitTestOption,
                    overrideRequiredOption,
                    queryFileOption,
                    outputFileOption,
                    forceOption,
                    aciIResourceGroupNameNotReqOption,
                    aciInstanceNameNotReqOption,
                    aciContainerCountOption
                });
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(VnetOptions);
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.Add(keyVaultNameOption);
                cmd.Add(storageaccountnameOption);
                cmd.Add(storageaccountkeyOption);
                cmd.Add(authtypeOption);
                cmd.Add(platformOption);
                cmd.Add(clientIdOption);
                cmd.Add(identityNameOption);
                cmd.Add(identityResourceGroupOption);
                cmd.Add(subscriptionIdOption);
                cmd.AddRange(ConcurrencyRequiredOptions);
                cmd.Add(streamEventsOption);
                cmd.SetGroupedHelp(
                    new OptionGroup("Query", new List<Option> { overrideRequiredOption, queryFileOption, outputFileOption }),
                    new OptionGroup("ACI", new List<Option> { aciIResourceGroupNameNotReqOption, aciInstanceNameNotReqOption, aciContainerCountOption }),
                    new OptionGroup("Execution", new List<Option> { jobnameOption, aciMonitorOption, forceOption }),
                    new OptionGroup("Settings File", SettingsFileExistingOptions),
                    new OptionGroup("Event Hub", EventHubResourceOptions),
                    new OptionGroup("VNET", VnetOptions),
                    new OptionGroup("Container Registry", ContainerRegistryAndImageOptions),
                    new OptionGroup("Connections & Secrets", new List<Option> { keyVaultNameOption, storageaccountnameOption, storageaccountkeyOption }),
                    new OptionGroup("Authentication", new List<Option> { authtypeOption, platformOption }),
                    new OptionGroup("Identity", new List<Option> { clientIdOption, identityNameOption, identityResourceGroupOption, subscriptionIdOption }),
                    new OptionGroup("Concurrency", ConcurrencyRequiredOptions)
                );

                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var monitor = parseResult.GetValue(aciMonitorOption);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var force = parseResult.GetValue(forceOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    return await Worker.AciQuery(cmdLine: cmdLine, force: force, monitor: monitor, stream: stream, unittest: unittest);
                });
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
                    jobnameRequiredOption,
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
                cmd.Add(platformOption);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var packagename = parseResult.GetValue(packagenameAsFileToUploadOption);
                    var platinumdacpac = parseResult.GetValue(platinumdacpacFileInfoOption);
                    var force = parseResult.GetValue(forceOption);
                    return await GenericContainer.PrepAndUploadContainerBuildPackage(cmdLine: cmdLine, packageName: packagename!, platinumDacpac: platinumdacpac!, force: force);
                });

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
                    jobnameRequiredOption,
                    threadedConcurrencyRequiredOption,
                    overrideRequiredOption
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.EnqueueOverrideTargets(cmdLine);
                });

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
                    aciContainerCountOption,
                    identityNameOption,
                    identityResourceGroupOption,
                    sectionPlaceholderOption,
                    jobnameRequiredOption,
                    packagenameAsFileToUploadOption,
                    overrideOption,
                    platinumdacpacFileInfoOption,
                    allowForObjectDeletionOption,
                    aciMonitorOption,
                    unitTestOption
                };
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(VnetOptions);
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.Add(keyVaultNameOption);
                cmd.Add(storageaccountnameOption);
                cmd.Add(storageaccountkeyOption);
                cmd.Add(authtypeOption);
                cmd.Add(platformOption);
                cmd.Add(clientIdOption);
                cmd.Add(subscriptionIdOption);
                cmd.AddRange(ConcurrencyRequiredOptions);
                cmd.SetGroupedHelp(
                    new OptionGroup("Build Options", new List<Option> { jobnameRequiredOption, packagenameAsFileToUploadOption, overrideOption, platinumdacpacFileInfoOption, allowForObjectDeletionOption, aciMonitorOption }),
                    new OptionGroup("ACI", new List<Option> { aciIResourceGroupNameNotReqOption, aciInstanceNameNotReqOption, aciContainerCountOption }),
                    new OptionGroup("Settings File", new List<Option> { settingsfileExistingOption }),
                    new OptionGroup("Event Hub", EventHubResourceOptions),
                    new OptionGroup("VNET", VnetOptions),
                    new OptionGroup("Container Registry", ContainerRegistryAndImageOptions),
                    new OptionGroup("Connections & Secrets", new List<Option> { keyVaultNameOption, storageaccountnameOption, storageaccountkeyOption }),
                    new OptionGroup("Authentication", new List<Option> { authtypeOption, platformOption }),
                    new OptionGroup("Identity", new List<Option> { identityNameOption, identityResourceGroupOption, clientIdOption, subscriptionIdOption }),
                    new OptionGroup("Concurrency", ConcurrencyRequiredOptions)
                );

                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var packagename = parseResult.GetValue(packagenameAsFileToUploadOption);
                    var platinumdacpac = parseResult.GetValue(platinumdacpacFileInfoOption);
                    var monitor = parseResult.GetValue(aciMonitorOption);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    return await Worker.AciDeploy(cmdLine: cmdLine, packageName: packagename!, platinumDacpac: platinumdacpac!, monitor: monitor, unittest: unittest, stream: stream);
                });
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
                    unitTestOption
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Add(eventhubconnectionOption);
                cmd.AddRange(EventHubResourceOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    // DateTime is null when called from command line (vs. internally after deploy)
                    return await Worker.AciMonitorRuntimeProgress(cmdLine: cmdLine, utcMonitorStart: null, unitest: unittest, stream: stream);
                });

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
                    jobnameRequiredOption,
                    threadedConcurrencyTypeOption
                };
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.DeQueueOverrideTargets(cmdLine);
                });

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
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.AciWorker_RunQueueQuery(cmdLine);
                });
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
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.AciWorker_RunQueueBuild(cmdLine);
                });
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
