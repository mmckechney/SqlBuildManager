using System;
using System.Collections.Generic;
using System.Text;
using System.Runtime.Serialization;
namespace SqlBuildManager.Interfaces.Console
{
    public enum RunnerReturn
    {
        RolledBack = -400,
        BuildCommitted = 0,
        SuccessWithTrialRolledBack = 5,
        BuildResultInconclusive = 10,
        BuildErrorNonTransactional = 20
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
        CheckingConnections = 7000

    }

    public enum LogType
    {
        Message,
        Error,
        Commit,
        SuccessDatabases, 
        FailureDatabases
    }
}
