namespace MergeNani.Models;

internal sealed record BatchMergeResult(
    IReadOnlyList<BatchMergeItemResult> Items,
    int SuccessCount,
    int FailureCount);

internal sealed record BatchMergeItemResult(
    string NaniPath,
    string? OutputPath,
    bool Success,
    string? ErrorMessage,
    MergeResult? MergeResult);
