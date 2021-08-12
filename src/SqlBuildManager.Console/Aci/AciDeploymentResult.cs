using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SqlBuildManager.Console.Aci
{
    class AciDeploymentResult
    {
        
        public class EnvironmentVariable
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("value")]
            public string Value { get; set; }
        }

        public class CurrentState
        {
            [JsonPropertyName("state")]
            public string State { get; set; }

            [JsonPropertyName("startTime")]
            public DateTime StartTime { get; set; }

            [JsonPropertyName("exitCode")]
            public int ExitCode { get; set; }

            [JsonPropertyName("finishTime")]
            public DateTime FinishTime { get; set; }

            [JsonPropertyName("detailStatus")]
            public string DetailStatus { get; set; }
        }

        public class Event
        {
            [JsonPropertyName("count")]
            public int Count { get; set; }

            [JsonPropertyName("firstTimestamp")]
            public DateTime FirstTimestamp { get; set; }

            [JsonPropertyName("lastTimestamp")]
            public DateTime LastTimestamp { get; set; }

            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("message")]
            public string Message { get; set; }

            [JsonPropertyName("type")]
            public string Type { get; set; }
        }

        public class InstanceView
        {
            [JsonPropertyName("restartCount")]
            public int RestartCount { get; set; }

            [JsonPropertyName("currentState")]
            public CurrentState CurrentState { get; set; }

            [JsonPropertyName("events")]
            public List<Event> Events { get; set; }

            [JsonPropertyName("state")]
            public string State { get; set; }
        }

        public class Requests
        {
            [JsonPropertyName("memoryInGB")]
            public int MemoryInGB { get; set; }

            [JsonPropertyName("cpu")]
            public int Cpu { get; set; }
        }

        public class Resources
        {
            [JsonPropertyName("requests")]
            public Requests Requests { get; set; }
        }

        public class ContainerProperties
        {
            [JsonPropertyName("image")]
            public string Image { get; set; }

            [JsonPropertyName("command")]
            public List<string> Command { get; set; }

            [JsonPropertyName("ports")]
            public List<object> Ports { get; set; }

            [JsonPropertyName("environmentVariables")]
            public List<EnvironmentVariable> EnvironmentVariables { get; set; }

            [JsonPropertyName("instanceView")]
            public InstanceView InstanceView { get; set; }

            [JsonPropertyName("resources")]
            public Resources Resources { get; set; }
        }

        public class Container
        {
            [JsonPropertyName("name")]
            public string Name { get; set; }

            [JsonPropertyName("properties")]
            public ContainerProperties Properties { get; set; }
        }

        public class GroupProperties
        {
            [JsonPropertyName("sku")]
            public string Sku { get; set; }

            [JsonPropertyName("provisioningState")]
            public string ProvisioningState { get; set; }

            [JsonPropertyName("containers")]
            public List<Container> Containers { get; set; }

            [JsonPropertyName("initContainers")]
            public List<object> InitContainers { get; set; }

            [JsonPropertyName("restartPolicy")]
            public string RestartPolicy { get; set; }

            [JsonPropertyName("osType")]
            public string OsType { get; set; }

            [JsonPropertyName("instanceView")]
            public InstanceView InstanceView { get; set; }
        }

        public class Subscriptions1e1dcefcD8324de0889a2a48494c441bResourceGroupsSbm5RgProvidersMicrosoftManagedIdentityUserAssignedIdentitiesSbm5identity
        {
            [JsonPropertyName("PrincipalId")]
            public string PrincipalId { get; set; }

            [JsonPropertyName("ClientId")]
            public string ClientId { get; set; }
        }

        public class UserAssignedIdentities
        {
            [JsonPropertyName("/subscriptions/1e1dcefc-d832-4de0-889a-2a48494c441b/resourceGroups/sbm5-rg/providers/Microsoft.ManagedIdentity/userAssignedIdentities/sbm5identity")]
            public Subscriptions1e1dcefcD8324de0889a2a48494c441bResourceGroupsSbm5RgProvidersMicrosoftManagedIdentityUserAssignedIdentitiesSbm5identity Subscriptions1e1dcefcD8324de0889a2a48494c441bResourceGroupsSbm5RgProvidersMicrosoftManagedIdentityUserAssignedIdentitiesSbm5identity { get; set; }
        }

        public class Identity
        {
            [JsonPropertyName("PrincipalId")]
            public object PrincipalId { get; set; }

            [JsonPropertyName("TenantId")]
            public string TenantId { get; set; }

            [JsonPropertyName("Type")]
            public int Type { get; set; }

            [JsonPropertyName("UserAssignedIdentities")]
            public UserAssignedIdentities UserAssignedIdentities { get; set; }
        }

        public class Value
        {
            [JsonPropertyName("Plan")]
            public object Plan { get; set; }

            [JsonPropertyName("Properties")]
            public GroupProperties Properties { get; set; }

            [JsonPropertyName("Kind")]
            public object Kind { get; set; }

            [JsonPropertyName("ManagedBy")]
            public object ManagedBy { get; set; }

            [JsonPropertyName("Sku")]
            public object Sku { get; set; }

            [JsonPropertyName("Identity")]
            public Identity Identity { get; set; }

            [JsonPropertyName("Id")]
            public string Id { get; set; }

            [JsonPropertyName("Name")]
            public string Name { get; set; }

            [JsonPropertyName("Type")]
            public string Type { get; set; }

            [JsonPropertyName("Location")]
            public string Location { get; set; }

            [JsonPropertyName("Tags")]
            public object Tags { get; set; }
        }

        public class Result
        {
            [JsonPropertyName("Value")]
            public Value Value { get; set; }
        }


    }


}
