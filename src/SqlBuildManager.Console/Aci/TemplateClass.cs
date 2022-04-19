using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace SqlBuildManager.Console.Aci
{

    public class Variables
    {
        public string identityName { get; set; }
        public string identityResourceGroup { get; set; }
        public string aciName { get; set; }
    }

    public class EnvironmentVariable
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("value")]
        public string Value { get; set; }
    }

    public class Requests
    {
        [JsonPropertyName("cpu")]
        public int Cpu { get; set; }

        [JsonPropertyName("memoryInGB")]
        public double MemoryInGB { get; set; }
    }

    public class Resource
    {
        [JsonPropertyName("requests")]
        public Requests Requests { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("apiVersion")]
        public string ApiVersion { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("location")]
        public string Location { get; set; }

        [JsonPropertyName("properties")]
        public Properties Properties { get; set; }
    }

    public class Properties
    {
        [JsonPropertyName("image")]
        public string Image { get; set; }

        [JsonPropertyName("command")]
        public List<string> Command { get; set; }

        [JsonPropertyName("environmentVariables")]
        public List<EnvironmentVariable> EnvironmentVariables { get; set; }

        [JsonPropertyName("resources")]
        public Resource Resources { get; set; }

        [JsonPropertyName("containers")]
        public List<Container> Containers { get; set; }

        [JsonPropertyName("osType")]
        public string OsType { get; set; }

        [JsonPropertyName("restartPolicy")]
        public string RestartPolicy { get; set; }
    }

    public class Container
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("properties")]
        public Properties Properties { get; set; }
    }

    public class TemplateClass
    {
        [JsonPropertyName("$schema")]
        public string Schema { get; set; }

        [JsonPropertyName("contentVersion")]
        public string ContentVersion { get; set; }

        public Variables variables { get; set; }

        [JsonPropertyName("functions")]
        public List<object> Functions { get; set; }


        [JsonPropertyName("resources")]
        public List<Resource> Resources { get; set; }
    }


}
