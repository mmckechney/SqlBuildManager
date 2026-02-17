using SqlBuildManager.Console.ContainerShared;
using System;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
        // Local option for container app monitor flag
        private static Option<bool> containerAppMonitorOption = new Option<bool>("--monitor") { Description = "Immediately start monitoring progress after successful Container App deployment" };
        
        /// <summary>
        /// Saves settings file for Azure Container App deployments
        /// </summary>
        private static Command ContainerAppSaveSettingsCommand
        {
            get
            {
                var cmd = new Command("savesettings", "Saves settings file for Azure Container App deployments")
                {
                    defaultscripttimeoutOption,
                    cleartextOption,
                    silentOption
                };
                cmd.AddRange(SettingsFileNewOptions);
                cmd.AddRange(ContainerAppOptions);
                IdentityArgumentsForContainerApp.ForEach(o => { if (o.Name == "identityname") { o.Required = false; } cmd.Add(o); });
                cmd.AddRange(ConnectionAndSecretsOptions);
                cmd.AddRange(EventHubResourceOptions);
                cmd.AddRange(DatabaseAuthArgs);
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.AddRange(ConcurrencyOptions);
                cmd.SetAction((parseResult) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var unittest = parseResult.GetValue(unitTestOption);
                    return Worker.SaveAndEncryptContainerAppSettings(cmdLine, unittest);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Runs a build on Container Apps (orchestrates the prep, enqueue, deploy and montitor commands)
        /// </summary>
        private static Command ContainerAppRunCommand
        {
            get
            {
                var cmd = new Command("run", "Runs a build on Container Apps (orchestrates the prep, enqueue, deploy and montitor commands)");
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(new List<Option>
                {
                    sectionPlaceholderOption,
                    packagenameOption,
                    platinumdacpacOption,
                    defaultscripttimeoutOption,
                    overrideOption,
                    jobnameRequiredOption,
                    decryptedOption,
                    allowForObjectDeletionOption,
                    unitTestOption,
                    containerAppMonitorOption,
                    containerAppEnvOnly,
                    containerAppDeleteAppUponCompletion,
                    forceOption

                });

                cmd.AddRange(ContainerAppOptions);
                IdentityArgumentsForContainerApp.ForEach(o => { if (o.Name == "identityname") { o.Required = false; } cmd.Add(o); });
                cmd.AddRange(ConnectionAndSecretsOptions);
                cmd.AddRange(EventHubResourceOptions);
                //TODO: Enable Managed Identity. For now, ManagedIdentity for SQL Auth is not available on Container Apps...
                //cmd.AddRange(DatabaseAuthArgs);
                cmd.Add(usernameOption);
                cmd.Add(passwordOption);
                //end
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.AddRange(ConcurrencyOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    var monitor = parseResult.GetValue(containerAppMonitorOption);
                    var deleteApp = parseResult.GetValue(containerAppDeleteAppUponCompletion);
                    var force = parseResult.GetValue(forceOption);
                    return await Worker.ContainerAppsRun(cmdLine, unittest, stream, monitor, deleteApp, force);
                });
                return cmd;
            }
        }

        /// <summary>
        /// Creates an Azure storage container and uploads the SBM and/or DACPAC files that will be used for the build
        /// </summary>
        private static Command ContainerAppPrepCommand
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
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var packagename = parseResult.GetValue(packagenameAsFileToUploadOption);
                    var platinumdacpac = parseResult.GetValue(platinumdacpacFileInfoOption);
                    var force = parseResult.GetValue(forceOption);
                    return await GenericContainer.PrepAndUploadContainerBuildPackage(cmdLine, packagename, platinumdacpac, force);
                });

                return cmd;
            }
        }

        /// <summary>
        /// Sends database override targets to Service Bus 
        /// </summary>
        private static Command ContainerAppEnqueueTargetsCommand
        {
            get
            {
                var cmd = new Command("enqueue", "Sends database override targets to Service Bus Topic")
                {
                    jobnameRequiredOption,
                    threadedConcurrencyRequiredOption,
                    serviceBusconnectionOption,
                    overrideRequiredOption,
                    decryptedOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.EnqueueOverrideTargets(cmdLine);
                });

                return cmd;
            }
        }

        /// <summary>
        /// Deploy the Container App instance using the template file created from 'sbm containerapp prep' and start containers
        /// </summary>
        private static Command ContainerAppDeployCommand
        {
            get
            {
                var cmd = new Command("deploy", "Deploy the Container App instance using the template file created from 'sbm containerapp prep' and start containers")
                {
                    packagenameOption,
                    platinumdacpacOption,
                    defaultscripttimeoutOption,
                    overrideOption,
                    jobnameRequiredOption,
                    decryptedOption,
                    allowForObjectDeletionOption,
                    unitTestOption,
                    containerAppMonitorOption,
                    containerAppEnvOnly,
                    containerAppDeleteAppUponCompletion

                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(ContainerAppOptions);
                IdentityArgumentsForContainerApp.ForEach(o => { if (o.Name == "identityname") { o.Required = false; } cmd.Add(o); });
                cmd.AddRange(ConnectionAndSecretsOptions);
                cmd.AddRange(EventHubResourceOptions);
                //TODO: Enable Managed Identity. For now, ManagedIdentity for SQL Auth is not available on Container Apps...
                //DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                cmd.Add(usernameOption);
                cmd.Add(passwordOption);
                //end
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.AddRange(ConcurrencyOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    var monitor = parseResult.GetValue(containerAppMonitorOption);
                    var deleteApp = parseResult.GetValue(containerAppDeleteAppUponCompletion);
                    return await Worker.ContainerAppDeploy(cmdLine, unittest, stream, monitor, deleteApp);
                });

                return cmd;
            }
        }

        /// <summary>
        /// Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)
        /// </summary>
        private static Command ContainerAppMonitorCommand
        {
            get
            {
                var cmd = new Command("monitor", "Poll the Service Bus Topic to see how many messages are left to be processed and watch the Event Hub for build outcomes (commits & errors)")
                {
                    jobnameOption,
                    overrideOption,
                    threadedConcurrencyTypeOption,
                    unitTestOption,
                    decryptedOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Add(eventhubconnectionOption);
                cmd.AddRange(EventHubResourceOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    var unittest = parseResult.GetValue(unitTestOption);
                    var stream = parseResult.GetValue(streamEventsOption);
                    // DateTime is null when called from command line (vs. internally after deploy)
                    return await Worker.MonitorContainerAppRuntimeProgress(cmdLine, unittest, null, stream);
                });

                return cmd;
            }
        }

        /// <summary>
        /// "Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them
        /// </summary>
        private static Command ContainerAppDequeueTargetsCommand
        {
            get
            {
                var cmd = new Command("dequeue", "Careful! Removes the Service Bus Topic subscription and deletes the messages and deadletters without processing them")
                {
                    serviceBusconnectionOption,
                    jobnameRequiredOption,
                    threadedConcurrencyTypeOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.DeQueueOverrideTargets(cmdLine);
                });

                return cmd;
            }
        }

        /// <summary>
        /// Create environment variables for Container app and run local execution
        /// </summary>
        private static Command ContainerAppWorkerTestCommand
        {
            get
            {
                var cmd = new Command("test", "Create environment variables for Container app and run local execution")
                {
                    packagenameOption,
                    overrideRequiredOption,
                    jobnameRequiredOption,
                    decryptedOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(ConcurrencyOptions);
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.ContainerAppTestSettings(cmdLine);
                });

                return cmd;
            }
        }

        /// <summary>
        /// [Used by Container Apps] Starts the pod as a worker - polling and retrieving items from target service bus queue topic
        /// </summary>
        private static Command ContainerAppWorkerCommand
        {
            get
            {
                var nameArgument = new Argument<string[]>("placeholder") { Description = "Used to keep extraneous elements added by the platform from causing issues", Hidden = true, Arity = ArgumentArity.ZeroOrMore };
                var cmd = new Command("worker", "[Used by Container Apps] Starts the pod as a worker - polling and retrieving items from target service bus queue topic");
                cmd.Arguments.Add(nameArgument);
                
                cmd.SetAction(async (parseResult, ct) => {
                    var cmdLine = CommandLineArgsBinder.Bind(parseResult);
                    return await Worker.ContainerAppWorker_RunBuild(cmdLine);
                });
                cmd.Add(ContainerAppWorkerTestCommand);

                return cmd;
            }
        }

        /// <summary>
        /// Commands for setting and executing a build running in pods on Azure Container App
        /// </summary>
        private static Command ContainerAppCommand
        {
            get
            {
                var cmd = new Command("containerapp", "Commands for setting and executing a build running in pods on Azure Container App");
                cmd.Add(ContainerAppSaveSettingsCommand);
                cmd.Add(ContainerAppRunCommand);
                cmd.Add(ContainerAppPrepCommand);
                cmd.Add(ContainerAppEnqueueTargetsCommand);
                cmd.Add(ContainerAppDeployCommand);
                cmd.Add(ContainerAppMonitorCommand);
                cmd.Add(ContainerAppDequeueTargetsCommand);
                cmd.Add(ContainerAppWorkerCommand);
                cmd.Aliases.Add("ca");
                return cmd;
            }
        }
    }
}
