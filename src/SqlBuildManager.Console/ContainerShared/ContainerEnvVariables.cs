namespace SqlBuildManager.Console.ContainerShared
{
    internal class ContainerEnvVariables
    {
        internal static string IdentityClientId { get; } = "Sbm_IdentityClientId";
        internal static string KeyVaultName { get; } = "Sbm_KeyVaultName";
        internal static string JobName { get; } = "Sbm_JobName";
        internal static string PackageName { get; } = "Sbm_PackageName";
        internal static string Concurrency { get; } = "Sbm_Concurrency";
        internal static string ConcurrencyType { get; } = "Sbm_ConcurrencyType";
        internal static string DacpacName { get; } = "Sbm_DacpacName";
        internal static string StorageAccountKey { get; } = "Sbm_StorageAccountKey";
        internal static string StorageAccountName { get; } = "Sbm_StorageAccountName";
        internal static string EventHubConnectionString { get; } = "Sbm_EventHubConnectionString";
        internal static string ServiceBusTopicConnectionString { get; } = "Sbm_ServiceBusTopicConnectionString";
        internal static string UserName { get; } = "Sbm_UserName";
        internal static string Password { get; } = "Sbm_Password";
        internal static string AllowObjectDelete { get; } = "Sbm_AllowObjectDelete";
        internal static string AuthType { get; } = "Sbm_AuthType";
        internal static string IdentityResourceGroup { get; } = "Sbm_IdentityResourceGroup";
        internal static string IdentityName { get; } = "Sbm_IdentityName";
        internal static string QueryFile { get; } = "Sbm_QueryFile";
        internal static string OutputFile { get; } = "Sbm_OutputFile";
        internal static string EventHubLogging { get; } = "Sbm_EventHubLogging";
        internal static string DatabasePlatform { get; } = "Sbm_DatabasePlatform";
        internal static string Override { get; } = "Sbm_Override";
        internal static string TargetDacpac { get; } = "Sbm_TargetDacpac";
        internal static string OutputContainerSasUrl { get; } = "Sbm_OutputContainerSasUrl";
        internal static string Transactional { get; } = "Sbm_Transactional";
        internal static string TimeoutRetryCount { get; } = "Sbm_TimeoutRetryCount";
        internal static string DefaultScriptTimeout { get; } = "Sbm_DefaultScriptTimeout";
        internal static string ForceCustomDacPac { get; } = "Sbm_ForceCustomDacPac";
        internal static string PlatinumDbSource { get; } = "Sbm_PlatinumDbSource";
        internal static string PlatinumServerSource { get; } = "Sbm_PlatinumServerSource";
        internal static string TrustServerCertificate { get; } = "Sbm_TrustServerCertificate";
        internal static string Silent { get; } = "Sbm_Silent";
        internal static string TenantId { get; } = "Sbm_TenantId";
        internal static string UnitTest { get; } = "Sbm_UnitTest";

    }
}
