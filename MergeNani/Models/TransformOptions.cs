namespace MergeNani.Models;

internal sealed record TransformOptions
{
    public string NameFrom { get; init; } = string.Empty;
    public string NameTo { get; init; } = string.Empty;
    public bool SceneMToF { get; init; }
    public bool SceneFToM { get; init; }

    public bool HasNameReplace => !string.IsNullOrWhiteSpace(NameFrom);

    public bool HasAnyReplace => HasNameReplace || SceneMToF || SceneFToM;
}
