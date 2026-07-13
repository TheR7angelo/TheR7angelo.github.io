namespace TheR7angelo.github.io.Domain.Models.GitHub;

public class GithubRepositoryInformationDomain
{
    public required string RepositoryUrl { get; set; }
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required string LogoUrl { get; init; }
    public required string? ReadmeHtml { get; init; }
    public required IEnumerable<GithubBadgeDomain> LanguagesBadges { get; init; }
    public required List<GithubBadgeDomain> StatsBadges { get; init; }
}