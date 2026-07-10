namespace TheR7angelo.github.io;

public sealed record AnchorSection
{
    public required string Id { get; init; }
    public required string Title { get; init; }
    public string? Icon { get; init; }
}
