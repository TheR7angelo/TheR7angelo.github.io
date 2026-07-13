using YamlDotNet.Serialization;

namespace TheR7angelo.github.io.Infrastructure.Github.Entities;

public class LanguageMetadata
{
    [YamlMember(Alias = "type")]
    public string Type { get; set; } = string.Empty;

    [YamlMember(Alias = "color")]
    public string? HexColor { get; set; }

    [YamlMember(Alias = "ace_mode")]
    public string AceMode { get; set; } = string.Empty;

    [YamlMember(Alias = "codemirror_mode")]
    public string CodemirrorMode { get; set; } = string.Empty;

    [YamlMember(Alias = "codemirror_mime_type")]
    public string CodemirrorMimeType { get; set; } = string.Empty;

    [YamlMember(Alias = "tm_scope")]
    public string TmScope { get; set; } = string.Empty;

    [YamlMember(Alias = "aliases")]
    public List<string> Aliases { get; set; } = [];

    [YamlMember(Alias = "extensions")]
    public List<string> Extensions { get; set; } = [];

    [YamlMember(Alias = "language_id")]
    public int LanguageId { get; set; }

    public string CleanColor => HexColor?.Replace("#", "") ?? "868686";
}