using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.Shared
{
    internal class ContainerEnvVariables
    {
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

    }
}
