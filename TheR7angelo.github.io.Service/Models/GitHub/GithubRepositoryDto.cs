namespace TheR7angelo.github.io.Service.Models.GitHub;

public sealed class GithubRepositoryDto
{
    public long Id { get; set; }

    public string NodeId { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string HtmlUrl { get; set; } = string.Empty;

    public string? Homepage { get; set; }

    public string? Language { get; set; }

    public string Visibility { get; set; } = string.Empty;

    public bool Private { get; set; }

    public bool Fork { get; set; }

    public bool Archived { get; set; }

    public bool Disabled { get; set; }

    public int Size { get; set; }

    public int Stars { get; set; }

    public int WatchersCount { get; set; }

    public int ForksCount { get; set; }

    public int OpenIssuesCount { get; set; }

    public string DefaultBranch { get; set; } = string.Empty;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public DateTime PushedAt { get; set; }

    public GithubOwnerDto? OwnerDto { get; set; }

    public GithubLicenseDto? LicenseDto { get; set; }
}