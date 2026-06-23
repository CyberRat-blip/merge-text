namespace MergeNani.Models;

internal sealed record BatchMergeOptions
{
    public string OutputDirectory { get; init; } = string.Empty;
    public string NameSuffix { get; init; } = string.Empty;
    public bool FilenameMToF { get; init; }
    public bool FilenameFToM { get; init; }
    public TransformOptions Transforms { get; init; } = new();
}
