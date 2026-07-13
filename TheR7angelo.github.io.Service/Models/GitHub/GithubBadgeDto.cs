namespace TheR7angelo.github.io.Service.Models.GitHub;

public class GithubBadgeDto
{
    public required string Url { get; init; }
    public required string AltText { get; init; }

    public string? Text { get; init; }
    public long? Bytes { get; init; }
    public string? Color { get; init; }
}