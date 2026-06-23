namespace MergeNani.Models;

internal sealed class MergePair
{
    public required string NaniPath { get; init; }
    public string? TextPath { get; set; }
    public string? MatchKey { get; init; }
    public string OutputPath { get; set; } = string.Empty;
    public int? NaniLines { get; set; }
    public int? TextLines { get; set; }
    public MergePairStatus Status { get; set; }
    public string? ErrorMessage { get; set; }

    public bool IsReady => Status == MergePairStatus.Ok
        && !string.IsNullOrWhiteSpace(TextPath)
        && !string.IsNullOrWhiteSpace(OutputPath);

    public string DisplayName => Path.GetFileName(NaniPath);

    public string StatusText => Status switch
    {
        MergePairStatus.Ok => "совпадают",
        MergePairStatus.Unmatched => "нет пары",
        MergePairStatus.Mismatch => $"строки: .nani={NaniLines}, текст={TextLines}",
        MergePairStatus.Conflict => ErrorMessage ?? "конфликт",
        _ => "—",
    };
}
