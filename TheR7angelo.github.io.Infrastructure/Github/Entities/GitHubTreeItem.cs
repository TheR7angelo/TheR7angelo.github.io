using System.Text.Json.Serialization;

namespace TheR7angelo.github.io.Infrastructure.Github.Entities;

public class GitHubTreeResponse
{
    [JsonPropertyName("tree")]
    public List<GitHubTreeItem> Tree { get; init; } = [];
}

public class GitHubTreeItem
{
    [JsonPropertyName("path")]
    public string Path { get; set; } = string.Empty;
}