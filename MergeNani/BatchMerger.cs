using MergeNani.Models;

namespace MergeNani;

internal static class BatchMerger
{
    public static IReadOnlyList<MergePair> BuildPairs(
        IEnumerable<string> naniPaths,
        IEnumerable<string> textPaths,
        BatchMergeOptions options)
        => PairMatcher.Build(naniPaths, textPaths, options);

    public static void RefreshPairs(IEnumerable<MergePair> pairs, BatchMergeOptions options)
    {
        var outputDirectory = PairMatcher.ResolveOutputDirectoryForPairs(pairs, options.OutputDirectory);

        foreach (var pair in pairs)
        {
            pair.OutputPath = OutputNameBuilder.Build(
                pair.NaniPath,
                outputDirectory,
                options.NameSuffix,
                options.FilenameMToF,
                options.FilenameFToM);
            PairMatcher.RefreshLineCounts(pair);
        }
    }

    public static BatchMergeResult MergeAll(
        IEnumerable<MergePair> pairs,
        BatchMergeOptions options)
    {
        var items = new List<BatchMergeItemResult>();
        var successCount = 0;
        var failureCount = 0;

        foreach (var pair in pairs)
        {
            if (!pair.IsReady)
            {
                failureCount++;
                items.Add(new BatchMergeItemResult(
                    pair.NaniPath,
                    pair.OutputPath,
                    false,
                    pair.StatusText,
                    null));
                continue;
            }

            try
            {
                var result = NaniMerger.MergeFiles(
                    pair.NaniPath,
                    pair.TextPath!,
                    pair.OutputPath,
                    options.Transforms.HasAnyReplace ? options.Transforms : null);

                successCount++;
                items.Add(new BatchMergeItemResult(
                    pair.NaniPath,
                    pair.OutputPath,
                    true,
                    null,
                    result));
            }
            catch (Exception ex)
            {
                failureCount++;
                items.Add(new BatchMergeItemResult(
                    pair.NaniPath,
                    pair.OutputPath,
                    false,
                    ex.Message,
                    null));
            }
        }

        return new BatchMergeResult(items, successCount, failureCount);
    }

    public static string BuildSummary(BatchMergeResult result)
    {
        var lines = new List<string>
        {
            $"Собрано {result.SuccessCount} из {result.Items.Count}.",
        };

        var failures = result.Items.Where(item => !item.Success).ToList();
        if (failures.Count > 0)
        {
            lines.Add("Ошибки:");
            foreach (var failure in failures)
            {
                var name = Path.GetFileName(failure.NaniPath);
                lines.Add($"- {name}: {failure.ErrorMessage}");
            }
        }

        return string.Join('\n', lines);
    }
}
