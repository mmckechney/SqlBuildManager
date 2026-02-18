using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;
using Microsoft.Extensions.Logging;
using SqlSync.Connection;

namespace SqlBuildManager.Console.CommandLine
{
    /// <summary>
    /// Provides explicit binding from System.CommandLine ParseResult to CommandLineArgs.
    /// This replaces the deprecated NamingConventionBinder's reflection-based magic with type-safe, explicit binding.
    /// </summary>
    public static class CommandLineArgsBinder
    {
        private static readonly OptionRegistry _registry = new();
        private static bool _initialized = false;
        private static readonly object _lock = new();

        /// <summary>
        /// Ensures the registry is initialized with all option-to-property mappings.
        /// </summary>
        private static void EnsureInitialized()
        {
            if (_initialized) return;
            lock (_lock)
            {
                if (_initialized) return;
                RegisterAllOptions();
                _initialized = true;
            }
        }

        /// <summary>
        /// Binds a ParseResult to a new CommandLineArgs instance.
        /// </summary>
        /// <param name="parseResult">The parse result containing parsed option values.</param>
        /// <returns>A populated CommandLineArgs instance.</returns>
        public static CommandLineArgs Bind(ParseResult parseResult)
        {
            if (parseResult == null) throw new ArgumentNullException(nameof(parseResult));

            EnsureInitialized();
            var args = new CommandLineArgs();
            _registry.ApplyValues(parseResult, args);
            return args;
        }

        /// <summary>
        /// Binds a ParseResult to an existing CommandLineArgs instance.
        /// </summary>
        public static void BindTo(ParseResult parseResult, CommandLineArgs args)
        {
            if (parseResult == null) throw new ArgumentNullException(nameof(parseResult));
            if (args == null) throw new ArgumentNullException(nameof(args));

            EnsureInitialized();
            _registry.ApplyValues(parseResult, args);
        }

        /// <summary>
        /// Gets the number of registered options.
        /// </summary>
        public static int RegisteredOptionCount
        {
            get
            {
                EnsureInitialized();
                return _registry.Count;
            }
        }

        /// <summary>
        /// Registers all options with their corresponding property setters.
        /// </summary>
        private static void RegisterAllOptions()
        {
            // Core/General Options
            RegisterCoreOptions();

            // Authentication Options
            RegisterAuthenticationOptions();

            // Connection Options
            RegisterConnectionOptions();

            // Batch Options
            RegisterBatchOptions();

            // Identity Options
            RegisterIdentityOptions();

            // DacPac Options
            RegisterDacPacOptions();

            // EventHub Options
            RegisterEventHubOptions();

            // Container Registry Options
            RegisterContainerRegistryOptions();

            // Container App Options
            RegisterContainerAppOptions();

            // ACI Options
            RegisterAciOptions();

            // Kubernetes Options
            RegisterKubernetesOptions();

            // Network Options
            RegisterNetworkOptions();

            // Concurrency Options
            RegisterConcurrencyOptions();

            // Settings File Options
            RegisterSettingsFileOptions();
        }

        #region Core Options

        private static void RegisterCoreOptions()
        {
            // Server and Database
            _registry.Register(CommandLineBuilder.serverOption, (args, v) => args.Server = v);
            _registry.Register(CommandLineBuilder.serverRequiredOption, (args, v) => args.Server = v);
            _registry.Register(CommandLineBuilder.databaseOption, (args, v) => args.Database = v);
            _registry.Register(CommandLineBuilder.databaseRequiredOption, (args, v) => args.Database = v);

            // Build file and package
            _registry.Register(CommandLineBuilder.packagenameOption, (args, v) => args.BuildFileName = v);
            _registry.Register(CommandLineBuilder.packagenameRequiredOption, (args, v) => args.BuildFileName = v);
            _registry.Register(CommandLineBuilder.packagenameNotReqOption, (args, v) => args.BuildFileName = v);
            _registry.Register(CommandLineBuilder.outputsbmOption, (args, v) => args.OutputSbm = v);
            _registry.Register(CommandLineBuilder.outputsbmRequiredOption, (args, v) => args.OutputSbm = v);

            // Override options (string and FileInfo variants)
            _registry.Register(CommandLineBuilder.overrideOption, (args, v) => args.Override = v);
            _registry.Register(CommandLineBuilder.overrideRequiredOption, (args, v) => args.Override = v);
            _registry.Register(CommandLineBuilder.overrideAsFileOption, (args, v) => { if (v != null) args.Override = v.FullName; });
            _registry.Register(CommandLineBuilder.overrideAsFileNotRequiredOption, (args, v) => { if (v != null) args.Override = v.FullName; });

            // Logging and paths
            _registry.Register(CommandLineBuilder.rootloggingpathOption, (args, v) => args.RootLoggingPath = v);
            _registry.Register(CommandLineBuilder.logLevelOption, (args, v) => args.LogLevel = v);
            _registry.Register(CommandLineBuilder.logLevelWithInfoDefaultOption, (args, v) => args.LogLevel = v);
            _registry.Register(CommandLineBuilder.logtodatabasenamedOption, (args, v) => args.LogToDatabaseName = v);

            // Execution flags
            _registry.Register(CommandLineBuilder.trialOption, (args, v) => args.Trial = v);
            _registry.Register(CommandLineBuilder.transactionalOption, (args, v) => args.Transactional = v);
            _registry.Register(CommandLineBuilder.continueonfailureOption, (args, v) => args.ContinueOnFailure = v);
            _registry.Register(CommandLineBuilder.silentOption, (args, v) => args.Silent = v);

            // Timeouts
            _registry.Register(CommandLineBuilder.timeoutretrycountOption, (args, v) => args.TimeoutRetryCount = v);
            _registry.Register(CommandLineBuilder.defaultscripttimeoutOption, (args, v) => args.DefaultScriptTimeout = v);

            // Description and revision
            _registry.Register(CommandLineBuilder.descriptionOption, (args, v) => args.Description = v);
            _registry.Register(CommandLineBuilder.buildrevisionOption, (args, v) => args.BuildRevision = v);

            // Script source
            _registry.Register(CommandLineBuilder.scriptsrcdirOption, (args, v) => args.ScriptSrcDir = v);

            // Job name
            _registry.Register(CommandLineBuilder.jobnameOption, (args, v) => args.JobName = v);
            _registry.Register(CommandLineBuilder.jobnameRequiredOption, (args, v) => args.JobName = v);

            // Query and output files
            _registry.Register(CommandLineBuilder.queryFileOption, (args, v) => args.QueryFile = v);
            _registry.Register(CommandLineBuilder.queryFileRequiredOption, (args, v) => args.QueryFile = v);
            _registry.Register(CommandLineBuilder.outputFileOption, (args, v) => args.OutputFile = v);
            _registry.Register(CommandLineBuilder.outputFileRequiredOption, (args, v) => args.OutputFile = v);

            // Directory
            _registry.Register(CommandLineBuilder.unpackDirectoryOption, (args, v) => args.Directory = v?.FullName);

            // DACPAC name
            _registry.Register(CommandLineBuilder.dacpacOutputOption, (args, v) => args.DacpacName = v);

            // Scripts list
            _registry.Register(CommandLineBuilder.scriptListOption, (args, v) => args.Scripts = v);

            // AllowObjectDelete
            _registry.Register(CommandLineBuilder.allowForObjectDeletionOption, (args, v) => args.AllowObjectDelete = v);

            // Gold database/server (Synchronize)
            _registry.Register(CommandLineBuilder.golddatabaseOption, (args, v) => args.GoldDatabase = v);
            _registry.Register(CommandLineBuilder.goldserverOption, (args, v) => args.GoldServer = v);
        }

        #endregion

        #region Authentication Options

        private static void RegisterAuthenticationOptions()
        {
            _registry.Register(CommandLineBuilder.usernameOption, (args, v) => args.UserName = v);
            _registry.Register(CommandLineBuilder.passwordOption, (args, v) => args.Password = v);
            _registry.Register(CommandLineBuilder.authtypeOption, (args, v) => args.AuthenticationType = v);
        }

        #endregion

        #region Connection Options

        private static void RegisterConnectionOptions()
        {
            _registry.Register(CommandLineBuilder.keyVaultNameOption, (args, v) => args.KeyVaultName = v);
            _registry.Register(CommandLineBuilder.eventhubconnectionOption, (args, v) => args.EventHubConnection = v);
            _registry.Register(CommandLineBuilder.serviceBusconnectionOption, (args, v) => args.ServiceBusTopicConnection = v);
            _registry.Register(CommandLineBuilder.storageaccountnameOption, (args, v) => args.StorageAccountName = v);
            _registry.Register(CommandLineBuilder.storageaccountkeyOption, (args, v) => args.StorageAccountKey = v);
            _registry.Register(CommandLineBuilder.batchaccountnameOption, (args, v) => args.BatchAccountName = v);
            _registry.Register(CommandLineBuilder.batchaccountkeyOption, (args, v) => args.BatchAccountKey = v);
            _registry.Register(CommandLineBuilder.batchaccounturlOption, (args, v) => args.BatchAccountUrl = v);
        }

        #endregion

        #region Batch Options

        private static void RegisterBatchOptions()
        {
            _registry.Register(CommandLineBuilder.batchjobnameOption, (args, v) => args.BatchJobName = v);
            _registry.Register(CommandLineBuilder.batchjobnameRequiredOption, (args, v) => args.BatchJobName = v);
            _registry.Register(CommandLineBuilder.deletebatchjobOption, (args, v) => args.DeleteBatchJob = v);
            _registry.Register(CommandLineBuilder.deletebatchpoolOption, (args, v) => args.DeleteBatchPool = v);
            _registry.Register(CommandLineBuilder.pollbatchpoolstatusOption, (args, v) => args.PollBatchPoolStatus = v);
            _registry.Register(CommandLineBuilder.batchpoolOsOption, (args, v) => args.BatchPoolOs = v);
            _registry.Register(CommandLineBuilder.batchnodecountOption, (args, v) => args.BatchNodeCount = v);
            _registry.Register(CommandLineBuilder.batchvmsizeOption, (args, v) => args.BatchVmSize = v);
            _registry.Register(CommandLineBuilder.batchResourceGroupOption, (args, v) => args.BatchResourceGroup = v);
            _registry.Register(CommandLineBuilder.batchpoolnameOption, (args, v) => args.BatchPoolName = v);
            _registry.Register(CommandLineBuilder.batchApplicationOption, (args, v) => args.ApplicationPackage = v);
            _registry.Register(CommandLineBuilder.outputcontainersasurlOption, (args, v) => args.OutputContainerSasUrl = v);
            _registry.Register(CommandLineBuilder.batchJobMonitorTimeoutMin, (args, v) => args.BatchJobMonitorTimeout = v);
        }

        #endregion

        #region Identity Options

        private static void RegisterIdentityOptions()
        {
            _registry.Register(CommandLineBuilder.clientIdOption, (args, v) => args.ClientId = v);
            _registry.Register(CommandLineBuilder.principalIdOption, (args, v) => args.PrincipalId = v);
            _registry.Register(CommandLineBuilder.resourceIdOption, (args, v) => args.ResourceId = v);
            _registry.Register(CommandLineBuilder.identityResourceGroupOption, (args, v) => args.IdentityResourceGroup = v);
            _registry.Register(CommandLineBuilder.identityResourceGroupNotReqOption, (args, v) => args.IdentityResourceGroup = v);
            _registry.Register(CommandLineBuilder.identityNameOption, (args, v) => args.IdentityName = v);
            _registry.Register(CommandLineBuilder.identityNameNotReqOption, (args, v) => args.IdentityName = v);
            _registry.Register(CommandLineBuilder.subscriptionIdOption, (args, v) => args.SubscriptionId = v);
            _registry.Register(CommandLineBuilder.subscriptionIdNotReqOption, (args, v) => args.SubscriptionId = v);
            _registry.Register(CommandLineBuilder.tenantIdOption, (args, v) => args.TenantId = v);
            _registry.Register(CommandLineBuilder.serviceAccountNameOption, (args, v) => args.ServiceAccountName = v);
        }

        #endregion

        #region DacPac Options

        private static void RegisterDacPacOptions()
        {
            _registry.Register(CommandLineBuilder.platinumdacpacOption, (args, v) => args.PlatinumDacpac = v);
            _registry.Register(CommandLineBuilder.platinumdacpacRequiredOption, (args, v) => args.PlatinumDacpac = v);
            _registry.Register(CommandLineBuilder.platinumdacpacFileInfoOption, (args, v) => { if (v != null) args.PlatinumDacpac = v.FullName; });
            _registry.Register(CommandLineBuilder.platinumdacpacSourceOption, (args, v) => { if (v != null) args.PlatinumDacpac = v.FullName; });
            _registry.Register(CommandLineBuilder.targetdacpacOption, (args, v) => args.TargetDacpac = v);
            _registry.Register(CommandLineBuilder.targetdacpacSourceOption, (args, v) => { if (v != null) args.TargetDacpac = v.FullName; });
            _registry.Register(CommandLineBuilder.forcecustomdacpacOption, (args, v) => args.ForceCustomDacPac = v);
            _registry.Register(CommandLineBuilder.platinumdbsourceOption, (args, v) => args.PlatinumDbSource = v);
            _registry.Register(CommandLineBuilder.platinumserversourceOption, (args, v) => args.PlatinumServerSource = v);
        }

        #endregion

        #region EventHub Options

        private static void RegisterEventHubOptions()
        {
            _registry.Register(CommandLineBuilder.eventHubLoggingTypeOption, (args, v) => args.EventHubLogging = v);
            _registry.Register(CommandLineBuilder.eventhubResourceGroupOption, (args, v) => args.EventHubResourceGroup = v);
            _registry.Register(CommandLineBuilder.eventhubSubscriptionOption, (args, v) => args.EventHubSubscriptionId = v);
            // Note: StreamEvents is used as a direct handler parameter, not a CommandLineArgs property
        }

        #endregion

        #region Container Registry Options

        private static void RegisterContainerRegistryOptions()
        {
            _registry.Register(CommandLineBuilder.imageTagOption, (args, v) => args.ImageTag = v);
            _registry.Register(CommandLineBuilder.imageNameOption, (args, v) => args.ImageName = v);
            _registry.Register(CommandLineBuilder.imageRepositoryOption, (args, v) => args.RegistryServer = v);
            _registry.Register(CommandLineBuilder.imageRepositoryUserNameOption, (args, v) => args.RegistryUserName = v);
            _registry.Register(CommandLineBuilder.imageRepositoryPasswordOption, (args, v) => args.RegistryPassword = v);
        }

        #endregion

        #region Container App Options

        private static void RegisterContainerAppOptions()
        {
            _registry.Register(CommandLineBuilder.containerAppEnvironmentOption, (args, v) => args.EnvironmentName = v);
            _registry.Register(CommandLineBuilder.containerAppLocationOption, (args, v) => args.Location = v);
            _registry.Register(CommandLineBuilder.containerAppResourceGroupOption, (args, v) => args.ResourceGroup = v);
            _registry.Register(CommandLineBuilder.containerAppMaxContainerCount, (args, v) => args.MaxContainers = v);
            // Note: DeleteContainerAppWhenDone doesn't have a direct setter property, handle in handler
        }

        #endregion

        #region ACI Options

        private static void RegisterAciOptions()
        {
            _registry.Register(CommandLineBuilder.aciContainerCountOption, (args, v) => args.ContainerCount = v);
            _registry.Register(CommandLineBuilder.aciContainerCountRequiredOption, (args, v) => args.ContainerCount = v);
            _registry.Register(CommandLineBuilder.aciInstanceNameOption, (args, v) => args.AciName = v);
            _registry.Register(CommandLineBuilder.aciInstanceNameNotReqOption, (args, v) => args.AciName = v);
            _registry.Register(CommandLineBuilder.aciIResourceGroupNameOption, (args, v) => args.AciResourceGroup = v);
            _registry.Register(CommandLineBuilder.aciIResourceGroupNameNotReqOption, (args, v) => args.AciResourceGroup = v);
        }

        #endregion

        #region Kubernetes Options

        private static void RegisterKubernetesOptions()
        {
            _registry.Register(CommandLineBuilder.podCountOption, (args, v) => args.PodCount = v);
            // Note: SecretsFile and RuntimeFile are FileInfo and need handler-level binding
        }

        #endregion

        #region Network Options

        private static void RegisterNetworkOptions()
        {
            _registry.Register(CommandLineBuilder.vnetNameOption, (args, v) => args.VnetName = v);
            _registry.Register(CommandLineBuilder.subnetNameOption, (args, v) => args.SubnetName = v);
            _registry.Register(CommandLineBuilder.vnetResourceGroupOption, (args, v) => args.VnetResourceGroup = v);
        }

        #endregion

        #region Concurrency Options

        private static void RegisterConcurrencyOptions()
        {
            _registry.Register(CommandLineBuilder.threadedConcurrencyOption, (args, v) => args.Concurrency = v);
            _registry.Register(CommandLineBuilder.threadedConcurrencyTypeOption, (args, v) => args.ConcurrencyType = v);
            _registry.Register(CommandLineBuilder.threadedConcurrencyTypeRequiredOption, (args, v) => args.ConcurrencyType = v);
            _registry.Register(CommandLineBuilder.threadedConcurrencyRequiredTypeOption, (args, v) => args.Concurrency = v);
            _registry.Register(CommandLineBuilder.threadedConcurrencyRequiredOption, (args, v) => args.ConcurrencyType = v);
        }

        #endregion

        #region Settings File Options

        private static void RegisterSettingsFileOptions()
        {
            // Use the actual options from CommandLineBuilder (not CommandLineBuilderOptions)
            // because those are the ones added to the commands
            _registry.Register(CommandLineBuilder.settingsfileExistingOption, (args, v) => args.FileInfoSettingsFile = v);
            _registry.Register(CommandLineBuilder.settingsfileNewOption, (args, v) => args.FileInfoSettingsFile = v);
            _registry.Register(CommandLineBuilder.settingsfileExistingRequiredOption, (args, v) => args.FileInfoSettingsFile = v);
            _registry.Register(CommandLineBuilder.settingsfileKeyOption, (args, v) => args.SettingsFileKey = v);
            _registry.Register(CommandLineBuilder.settingsfileKeyRequiredOption, (args, v) => args.SettingsFileKey = v);
        }

        #endregion
    }
}
