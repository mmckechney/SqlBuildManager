using SqlBuildManager.Enterprise;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace SqlBuildManager.Console.ContainerApp.Internal
{
    internal class ContainerAppParameters
    {
        // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);

        [JsonPropertyName("environmentName")]
        public EnvironmentName EnvironmentName { get; set; }

        [JsonPropertyName("location")]
        public Location Location { get; set; }

        [JsonPropertyName("maxContainers")]
        public MaxContainers MaxContainers { get; set; }

        [JsonPropertyName("imageTag")]
        public ImageTag ImageTag { get; set; }

        [JsonPropertyName("imageName")]
        public ImageName ImageName { get; set; }

        [JsonPropertyName("registryServer")]
        public RegistryServer RegistryServer { get; set; }

        [JsonPropertyName("registryUserName")]
        public RegistryUserName RegistryUserName { get; set; }

        [JsonPropertyName("registryPassword")]
        public RegistryPassword RegistryPassword { get; set; }

        [JsonPropertyName("jobname")]
        public Jobname Jobname { get; set; }

        [JsonPropertyName("packageName")]
        public PackageName PackageName { get; set; }

        [JsonPropertyName("dacpacName")]
        public DacpacName DacpacName { get; set; } = new DacpacName();

        [JsonPropertyName("concurrency")]
        public Concurrency Concurrency { get; set; }

        [JsonPropertyName("concurrencyType")]
        public ConcurrencyType ConcurrencyType { get; set; }

        [JsonPropertyName("storageAccountKey")]
        public StorageAccountKey StorageAccountKey { get; set; }

        [JsonPropertyName("storageAccountName")]
        public StorageAccountName StorageAccountName { get; set; }

        [JsonPropertyName("eventHubConnectionString")]
        public EventHubConnectionString EventHubConnectionString { get; set; }

        [JsonPropertyName("serviceBusTopicConnectionString")]
        public ServiceBusTopicConnectionString ServiceBusTopicConnectionString { get; set; }

        [JsonPropertyName("username")]
        public Username Username { get; set; }

        [JsonPropertyName("password")]
        public Password Password { get; set; }

        [JsonPropertyName("identityResourceGroup")]
        public IdentityResourceGroup IdentityResourceGroup { get; set; }

        [JsonPropertyName("identityName")]
        public IdentityName IdentityName { get; set; }

        [JsonPropertyName("identityClientId")]
        public IdentityClientId IdentityClientId { get; set; }

        [JsonPropertyName("keyVaultName")]
        public KeyVaultName KeyVaultName { get; set; }

        [JsonPropertyName("allowObjectDelete")]
        public AllowObjectDelete AllowObjectDelete { get; set; }

        [JsonPropertyName("authType")]
        public AuthType AuthType { get; set; }
    }

    public class AuthType
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
    public class AllowObjectDelete
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
    public class IdentityClientId
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
    public class KeyVaultName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
    public class IdentityResourceGroup
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
    public class IdentityName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }
    public class EnvironmentName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class Location
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class MaxContainers
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    public class ImageTag
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class ImageName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class RegistryServer
    { 
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class RegistryUserName
{
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class RegistryPassword
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class Jobname
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class PackageName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class DacpacName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class Concurrency
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class ConcurrencyType
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class StorageAccountKey
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class StorageAccountName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class EventHubConnectionString
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class ServiceBusTopicConnectionString
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class Username
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class Password
    {
        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

}
