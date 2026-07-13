namespace TheR7angelo.github.io.Domain.Models.GitHub;

public sealed class GithubOwnerDomain
{
    public string Login { get; set; } = string.Empty;

    public long Id { get; set; }

    public string AvatarUrl { get; set; } = string.Empty;

    public string HtmlUrl { get; set; } = string.Empty;

    public string Type { get; set; } = string.Empty;
}