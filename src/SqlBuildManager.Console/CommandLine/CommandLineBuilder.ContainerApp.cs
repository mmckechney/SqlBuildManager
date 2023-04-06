using SqlBuildManager.Console.ContainerShared;
using System;
using System.Collections.Generic;
using System.CommandLine.NamingConventionBinder;
using System.CommandLine;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.CommandLine
{
    public partial class CommandLineBuilder
    {
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
                    silentOption,
                    eventHubLoggingTypeOption,
                };
                cmd.AddRange(SettingsFileNewOptions);
                cmd.AddRange(ContainerAppOptions);
                IdentityArgumentsForContainerApp.ForEach(o => { if (o.Name == "identityname") { o.IsRequired = false; } cmd.Add(o); });
                cmd.AddRange(ConnectionAndSecretsOptions);
                ////TODO: Enable Managed Identity. For now, ManagedIdentity for SQL Auth is not available on Container Apps...
                //cmd.AddRange(DatabaseAuthArgs);
                cmd.Add(usernameOption);
                cmd.Add(passwordOption);
                //end
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.AddRange(ConcurrencyOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool>(Worker.SaveAndEncryptContainerAppSettings);
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
                    overrideOption.Copy(false),
                    jobnameOption.Copy(true),
                    decryptedOption,
                    allowForObjectDeletionOption,
                    unitTestOption,
                    streamEventsOption,
                    eventHubLoggingTypeOption,
                    new Option<bool>("--monitor", () => true, "Immediately start monitoring progress after successful Container App container deployment"),
                    containerAppEnvOnly,
                    containerAppDeleteAppUponCompletion,
                    forceOption

                });

                cmd.AddRange(ContainerAppOptions);
                IdentityArgumentsForContainerApp.ForEach(o => { if (o.Name == "identityname") { o.IsRequired = false; } cmd.Add(o); });
                cmd.AddRange(ConnectionAndSecretsOptions);

                //TODO: Enable Managed Identity. For now, ManagedIdentity for SQL Auth is not available on Container Apps...
                //cmd.AddRange(DatabaseAuthArgs);
                cmd.Add(usernameOption);
                cmd.Add(passwordOption);
                //end
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.AddRange(ConcurrencyOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool, bool, bool, bool, bool>(Worker.ContainerAppsRun);
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
        /// Sends database override targets to Service Bus 
        /// </summary>
        private static Command ContainerAppEnqueueTargetsCommand
        {
            get
            {
                var cmd = new Command("enqueue", "Sends database override targets to Service Bus Topic")
                {
                    jobnameOption.Copy(true),
                    threadedConcurrencyTypeOption.Copy(true),
                    serviceBusconnectionOption,
                    overrideOption.Copy(true),
                    decryptedOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.EnqueueOverrideTargets);

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
                    overrideOption.Copy(false),
                    jobnameOption.Copy(true),
                    decryptedOption,
                    allowForObjectDeletionOption,
                    unitTestOption,
                    streamEventsOption,
                    eventHubLoggingTypeOption,
                    new Option<bool>("--monitor", () => true, "Immediately start monitoring progress after successful Container App container deployment"),
                    containerAppEnvOnly,
                    containerAppDeleteAppUponCompletion

                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(ContainerAppOptions);
                IdentityArgumentsForContainerApp.ForEach(o => { if (o.Name == "identityname") { o.IsRequired = false; } cmd.Add(o); });
                cmd.AddRange(ConnectionAndSecretsOptions);

                //TODO: Enable Managed Identity. For now, ManagedIdentity for SQL Auth is not available on Container Apps...
                //DatabaseAuthArgs.ForEach(o => cmd.Add(o));
                cmd.Add(usernameOption);
                cmd.Add(passwordOption);
                //end
                cmd.AddRange(ContainerRegistryAndImageOptions);
                cmd.AddRange(ConcurrencyOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool, bool, bool, bool>(Worker.ContainerAppDeploy);

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
                    streamEventsOption,
                    decryptedOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.Add(keyVaultNameOption);
                cmd.Add(serviceBusconnectionOption);
                cmd.Add(eventhubconnectionOption);
                cmd.Handler = CommandHandler.Create<CommandLineArgs, bool, DateTime?, bool>(Worker.MonitorContainerAppRuntimeProgress);

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
                    jobnameOption.Copy(true),
                    threadedConcurrencyTypeOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.DeQueueOverrideTargets);

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
                    overrideOption.Copy(true),
                    jobnameOption.Copy(true),
                    decryptedOption
                };
                cmd.AddRange(SettingsFileExistingOptions);
                cmd.AddRange(ConcurrencyOptions);
                cmd.Handler = CommandHandler.Create<CommandLineArgs>(Worker.ContainerAppTestSettings);

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
                var tmp = new Command("worker", "[Used by Container Apps] Starts the pod as a worker - polling and retrieving items from target service bus queue topic");
                tmp.Handler = CommandHandler.Create<CommandLineArgs>(Worker.ContainerAppWorker_RunBuild);
                tmp.Add(ContainerAppWorkerTestCommand);

                return tmp;
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
                cmd.AddAlias("ca");
                return cmd;
            }
        }
    }
}
