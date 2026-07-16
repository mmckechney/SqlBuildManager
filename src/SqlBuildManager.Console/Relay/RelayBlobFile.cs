using System;
using System.Text.Json.Serialization;

namespace SqlBuildManager.Console.Relay
{
    public sealed class RelayBlobFile
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("contentLength")]
        public long ContentLength { get; set; }

        [JsonPropertyName("contentType")]
        public string? ContentType { get; set; }

        [JsonPropertyName("lastModified")]
        public DateTimeOffset? LastModified { get; set; }

        [JsonPropertyName("blobType")]
        public string? BlobType { get; set; }
    }
}
