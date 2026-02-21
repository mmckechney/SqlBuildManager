using System.Text.Json.Serialization;

namespace SqlBuildManager.Console.ContainerApp.Internal
{
    internal class ContainerAppParameters
    {
        // Root myDeserializedClass = JsonSerializer.Deserialize<Root>(myJsonResponse);

        [JsonPropertyName("environmentName")]
        public EnvironmentName EnvironmentName { get; set; } = null!;

        [JsonPropertyName("location")]
        public Location Location { get; set; } = null!;

        [JsonPropertyName("maxContainers")]
        public MaxContainers MaxContainers { get; set; } = null!;

        [JsonPropertyName("imageTag")]
        public ImageTag ImageTag { get; set; } = null!;

        [JsonPropertyName("imageName")]
        public ImageName ImageName { get; set; } = null!;

        [JsonPropertyName("registryServer")]
        public RegistryServer RegistryServer { get; set; } = null!;

        [JsonPropertyName("registryUserName")]
        public RegistryUserName RegistryUserName { get; set; } = null!;

        [JsonPropertyName("registryPassword")]
        public RegistryPassword RegistryPassword { get; set; } = null!;

        [JsonPropertyName("jobname")]
        public Jobname Jobname { get; set; } = null!;

        [JsonPropertyName("packageName")]
        public PackageName PackageName { get; set; } = null!;

        [JsonPropertyName("dacpacName")]
        public DacpacName DacpacName { get; set; } = new DacpacName();

        [JsonPropertyName("concurrency")]
        public Concurrency Concurrency { get; set; } = null!;

        [JsonPropertyName("concurrencyType")]
        public ConcurrencyType ConcurrencyType { get; set; } = null!;

        [JsonPropertyName("storageAccountKey")]
        public StorageAccountKey StorageAccountKey { get; set; } = null!;

        [JsonPropertyName("storageAccountName")]
        public StorageAccountName StorageAccountName { get; set; } = null!;

        [JsonPropertyName("eventHubConnectionString")]
        public EventHubConnectionString EventHubConnectionString { get; set; } = null!;

        [JsonPropertyName("serviceBusTopicConnectionString")]
        public ServiceBusTopicConnectionString ServiceBusTopicConnectionString { get; set; } = null!;

        [JsonPropertyName("username")]
        public Username Username { get; set; } = null!;

        [JsonPropertyName("password")]
        public Password Password { get; set; } = null!;

        [JsonPropertyName("identityResourceGroup")]
        public IdentityResourceGroup IdentityResourceGroup { get; set; } = null!;

        [JsonPropertyName("identityName")]
        public IdentityName IdentityName { get; set; } = null!;

        [JsonPropertyName("identityClientId")]
        public IdentityClientId IdentityClientId { get; set; } = null!;

        [JsonPropertyName("keyVaultName")]
        public KeyVaultName KeyVaultName { get; set; } = null!;

        [JsonPropertyName("allowObjectDelete")]
        public AllowObjectDelete AllowObjectDelete { get; set; } = null!;

        [JsonPropertyName("authType")]
        public AuthType AuthType { get; set; } = null!;
    }

    public class AuthType
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
    public class AllowObjectDelete
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
    public class IdentityClientId
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
    public class KeyVaultName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
    public class IdentityResourceGroup
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
    public class IdentityName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }
    public class EnvironmentName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class Location
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class MaxContainers
    {
        [JsonPropertyName("value")]
        public int Value { get; set; }
    }

    public class ImageTag
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class ImageName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class RegistryServer
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class RegistryUserName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class RegistryPassword
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class Jobname
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class PackageName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class DacpacName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class Concurrency
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class ConcurrencyType
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class StorageAccountKey
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class StorageAccountName
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class EventHubConnectionString
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class ServiceBusTopicConnectionString
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class Username
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

    public class Password
    {
        [JsonPropertyName("value")]
        public string Value { get; set; } = string.Empty;
    }

}
