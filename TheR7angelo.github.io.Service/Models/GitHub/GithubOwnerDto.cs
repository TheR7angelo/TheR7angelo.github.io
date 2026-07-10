namespace TheR7angelo.github.io.Service.Models.GitHub;

public sealed class GithubOwnerDto
{
    public string Login { get; set; } = string.Empty;

    public long Id { get; set; }

    public string AvatarUrl { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}