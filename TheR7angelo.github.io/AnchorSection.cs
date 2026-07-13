namespace TheR7angelo.github.io;

public sealed record AnchorSection
{
    private static int _globalCounter;

    public string Id { get; }

    public required string Title { get; init; }
    public string? Icon { get; init; }
    public required Type ComponentType { get; init; }

    public AnchorSection()
    {
        var uniqueId = Interlocked.Increment(ref _globalCounter);
        Id = $"section-{uniqueId}";
    }
}