using System.ComponentModel;
using System.Runtime.Serialization;

namespace SqlBuildManager.Interfaces.Console
{
    public enum RunnerReturn
    {
        [Description("Rolled Back")]
        RolledBack = -400,
        [Description("Committed")]
        BuildCommitted = 0,
        [Description("Success - Trial Rolled Back")]
        SuccessWithTrialRolledBack = 5,
        [Description("Inconclusive")]
        BuildResultInconclusive = 10,
        [Description("Error - Non Transactional")]
        BuildErrorNonTransactional = 20,
        [Description("Dacpac Databases In Sync")]
        DacpacDatabasesInSync = 87598,
        [Description("Committed - With Custom Dacpac")]
        CommittedWithCustomDacpac = 87599,
        [Description("Package Creation Error")]
        PackageCreationError = 87600,
        [Description("Process Build Error")]
        ProcessBuildError = -300

    }

    [DataContract()]
    public enum ExecutionReturn
    {
        [Description("The operation was cancelled by the user.")]
        [EnumMember]
        UserCancelled = -3,
        [Description("A required command-line override was not provided.")]
        [EnumMember]
        MissingOverrideFlag = -100,
        [Description("A required build source was not provided.")]
        [EnumMember]
        MissingBuildFlag = -101,
        [Description("The override value or file type is invalid.")]
        [EnumMember]
        InvalidOverrideFlag = -102,
        [Description("Build data could not be loaded.")]
        [EnumMember]
        NullBuildData = -103,
        [Description("The multi-database configuration could not be loaded.")]
        [EnumMember]
        NullMultiDbConfig = -104,
        [Description("The script source directory is invalid.")]
        [EnumMember]
        InvalidScriptSourceDirectory = -105,
        [Description("The build file name or path is invalid.")]
        [EnumMember]
        InvalidBuildFileNameValue = -106,
        [Description("Trial mode requires transactional execution.")]
        [EnumMember]
        InvalidTransactionAndTrialCombo = -107,
        [Description("The target database override file is missing.")]
        [EnumMember]
        MissingTargetDbOverrideSetting = -108,
        [Description("The timeout retry count cannot be negative.")]
        [EnumMember]
        NegativeTimeoutRetryCount = -109,
        [Description("Timeout retries require transactional execution.")]
        [EnumMember]
        BadRetryCountAndTransactionalCombo = -110,
        [Description("One or more execution options are outside their supported range.")]
        [EnumMember]
        InvalidExecutionOption = -111,
        [Description("The selected authentication mode requires a valid username and password pair.")]
        [EnumMember]
        InvalidAuthenticationArguments = -112,
        [Description("One or more Azure Batch arguments are invalid or missing.")]
        [EnumMember]
        InvalidBatchArguments = -113,
        [Description("The requested output or settings file already exists or is missing.")]
        [EnumMember]
        InvalidOutputFile = -114,
        [Description("The settings encryption key is invalid.")]
        [EnumMember]
        InvalidSettingsKey = -115,
        [Description("The build package or DACPAC could not be extracted.")]
        [EnumMember]
        BuildFileExtractionError = -200,
        [Description("The project or package file could not be loaded.")]
        [EnumMember]
        LoadProjectFileError = -201,
        [Description("Execution initialization failed, including settings decryption or Key Vault loading.")]
        [EnumMember]
        RunInitializationError = -300,
        [Description("The build process failed.")]
        [EnumMember]
        ProcessBuildError = -301,
        [Description("An unexpected unhandled exception terminated the command.")]
        [EnumMember]
        UnhandledException = -302,
        [Description("The Azure Batch job did not complete within the configured monitoring timeout.")]
        [EnumMember]
        BatchJobMonitorTimeout = -602,
        [Description("Azure Batch execution failed before a task result was available.")]
        [EnumMember]
        BatchExecutionError = -603,
        [Description("The Azure Batch pool could not be created.")]
        [EnumMember]
        BatchPoolCreationError = -604,
        [Description("One or more Azure Batch nodes could not become usable.")]
        [EnumMember]
        BatchNodeUnavailable = -605,
        [Description("Azure Batch resources could not be deleted during cleanup.")]
        [EnumMember]
        BatchCleanupError = -606,
        [Description("Build settings could not be loaded.")]
        [EnumMember]
        UnableToLoadBuildSettings = -600,
        [Description("One or more remote workers reported an error.")]
        [EnumMember]
        OneOrMoreRemoteServersHadError = -601,
        [Description("One or more database targets were not assigned to a worker.")]
        [EnumMember]
        UnassignedDatabaseServers = -698,
        [Description("The command completed successfully.")]
        [EnumMember]
        Successful = 0,
        [Description("The command completed but one or more operations reported errors.")]
        [EnumMember]
        FinishingWithErrors = 1,
        [Description("The distributed operation is waiting for work or resources.")]
        [EnumMember]
        Waiting = 5000,
        [Description("The distributed operation is running.")]
        [EnumMember]
        Running = 6000,
        [Description("Database connectivity is being checked.")]
        [EnumMember]
        CheckingConnections = 7000,
        [Description("The compared DACPAC databases are already synchronized.")]
        [EnumMember]
        DacpacDatabasesInSync = 87598,
        [Description("Required override tags are missing.")]
        [EnumMember]
        MissingOverrideTags = 100323

    }

    public enum LogType
    {
        [Description("Message")]
        Message,
        [Description("Error")]
        Error,
        [Description("Commit")]
        Commit,
        [Description("SuccessDatabases")]
        SuccessDatabases,
        [Description("FailureDatabases")]
        FailureDatabases,
        [Description("WorkerCompleted")]
        WorkerCompleted,
        [Description("ScriptLog")]
        ScriptLog,
        [Description("ScriptError")]
        ScriptError
    }
}
