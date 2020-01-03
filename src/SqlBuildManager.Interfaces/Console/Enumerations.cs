using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;
using System.Reflection;

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
        PackageCreationError = 87600
    }
    

    [DataContract()]
    public enum ExecutionReturn
    {
        [EnumMember]
        MissingOverrideFlag = -100,
        [EnumMember]
        MissingBuildFlag = -101,
        [EnumMember]
        InvalidOverrideFlag = -102,
        [EnumMember]
        NullBuildData = -103,
        [EnumMember]
        NullMultiDbConfig = -104,
        [EnumMember]
        InvalidScriptSourceDirectory = -105,
        [EnumMember]
        InvalidBuildFileNameValue = -106,
        [EnumMember]
        InvalidTransactionAndTrialCombo = -107,
        [EnumMember]
        MissingTargetDbOverrideSetting = -108,
        [EnumMember]
        NegativeTimeoutRetryCount = -109,
        [EnumMember]
        BadRetryCountAndTransactionalCombo = -110,
        [EnumMember]
        BuildFileExtractionError = -200,
        [EnumMember]
        LoadProjectFileError = -201,
        [EnumMember]
        RunInitializationError = -300,
        [EnumMember]
        ProcessBuildError = -301,
        [EnumMember]
        UnableToLoadBuildSettings = -600,
        [EnumMember]
        OneOrMoreRemoteServersHadError = -601,
        [EnumMember]
        UnassignedDatabaseServers = -698,
        [EnumMember]
        Successful = 0,
        [EnumMember]
        FinishingWithErrors = 1,
        [EnumMember]
        Waiting = 5000,
        [EnumMember]
        Running = 6000,
        [EnumMember]
        CheckingConnections = 7000,
        [EnumMember]
        DacpacDatabasesInSync = 87598

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
        FailureDatabases
    }
}
