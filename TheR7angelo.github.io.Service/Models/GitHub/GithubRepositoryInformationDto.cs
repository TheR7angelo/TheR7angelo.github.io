namespace TheR7angelo.github.io.Service.Models.GitHub;

public class GithubRepositoryInformationDto
{
    public required string Name { get; init; }
    public required string? Description { get; init; }
    public required string LogoUrl { get; init; }
    public required string? ReadmeHtml { get; init; }
    public required IEnumerable<string> LanguagesBadges { get; init; }
    public required List<string> StatsBadges { get; init; }
}