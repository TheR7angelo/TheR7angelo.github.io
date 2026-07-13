using System.Text.Json.Serialization;

namespace TheR7angelo.github.io.Infrastructure.Github.Entities;

public sealed class GithubLicense
{
    [JsonPropertyName("key")]
    public string Key { get; set; } = string.Empty;

    [JsonPropertyName("name")]
    public string Name { get; set; } = string.Empty;

    [JsonPropertyName("spdx_id")]
    public string SpdxId { get; set; } = string.Empty;
}