using System.Text.RegularExpressions;
using MergeNani.Models;

namespace MergeNani;

internal static class PairMatcher
{
    private static readonly Regex SceneKeyPattern = new(
        @"scene_[\w\d_]+",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

    public static IReadOnlyList<MergePair> Build(
        IEnumerable<string> naniPaths,
        IEnumerable<string> textPaths,
        BatchMergeOptions options)
    {
        var naniList = naniPaths
            .Where(TextSourceFiles.IsNani)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var textList = textPaths
            .Where(TextSourceFiles.IsSupported)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
            .ToList();

        var textsByKey = GroupByMatchKey(textList);
        var naniByKey = GroupByMatchKey(naniList);

        var outputDirectory = ResolveOutputDirectory(naniList, options.OutputDirectory);
        var pairs = new List<MergePair>();

        foreach (var naniPath in naniList)
        {
            var key = GetMatchKey(naniPath) ?? Path.GetFileNameWithoutExtension(naniPath);
            string? textPath = null;
            MergePairStatus status;
            string? errorMessage = null;

            if (naniByKey.TryGetValue(key, out var naniCandidates) && naniCandidates.Count > 1)
            {
                status = MergePairStatus.Conflict;
                errorMessage = $"Несколько .nani для ключа {key}";
            }
            else if (textsByKey.TryGetValue(key, out var textCandidates) && textCandidates.Count > 1)
            {
                status = MergePairStatus.Conflict;
                errorMessage = $"Несколько текстов для ключа {key}";
            }
            else if (textsByKey.TryGetValue(key, out textCandidates) && textCandidates.Count == 1)
            {
                textPath = textCandidates[0];
                status = MergePairStatus.Ok;
            }
            else
            {
                var basename = Path.GetFileNameWithoutExtension(naniPath);
                var exact = textList.FirstOrDefault(
                    path => string.Equals(
                        Path.GetFileNameWithoutExtension(path),
                        basename,
                        StringComparison.OrdinalIgnoreCase));

                if (exact is not null)
                {
                    textPath = exact;
                    status = MergePairStatus.Ok;
                }
                else
                {
                    status = MergePairStatus.Unmatched;
                }
            }

            var pair = new MergePair
            {
                NaniPath = naniPath,
                TextPath = textPath,
                MatchKey = key,
                Status = status,
                ErrorMessage = errorMessage,
                OutputPath = OutputNameBuilder.Build(
                    naniPath,
                    outputDirectory,
                    options.NameSuffix,
                    options.FilenameMToF,
                    options.FilenameFToM),
            };

            RefreshLineCounts(pair);
            pairs.Add(pair);
        }

        return pairs;
    }

    public static void RefreshLineCounts(MergePair pair)
    {
        try
        {
            pair.NaniLines = NaniMerger.CountDialogueLines(File.ReadAllLines(pair.NaniPath));
        }
        catch (Exception ex)
        {
            pair.NaniLines = null;
            pair.Status = MergePairStatus.Conflict;
            pair.ErrorMessage = ex.Message;
            return;
        }

        if (string.IsNullOrWhiteSpace(pair.TextPath) || !File.Exists(pair.TextPath))
        {
            if (pair.Status == MergePairStatus.Ok)
            {
                pair.Status = MergePairStatus.Unmatched;
            }

            pair.TextLines = null;
            return;
        }

        try
        {
            pair.TextLines = File.ReadAllLines(pair.TextPath)
                .Count(line => !string.IsNullOrWhiteSpace(line));
        }
        catch (Exception ex)
        {
            pair.TextLines = null;
            pair.Status = MergePairStatus.Conflict;
            pair.ErrorMessage = ex.Message;
            return;
        }

        if (pair.Status == MergePairStatus.Conflict)
        {
            return;
        }

        pair.Status = pair.NaniLines == pair.TextLines
            ? MergePairStatus.Ok
            : MergePairStatus.Mismatch;
        pair.ErrorMessage = null;
    }

    public static string? GetMatchKey(string path)
    {
        var name = Path.GetFileNameWithoutExtension(path);
        var match = SceneKeyPattern.Match(name);
        if (!match.Success)
        {
            return null;
        }

        var key = match.Value;
        if (key.EndsWith("_m", StringComparison.OrdinalIgnoreCase)
            || key.EndsWith("_f", StringComparison.OrdinalIgnoreCase))
        {
            key = key[..^2];
        }

        return key.ToLowerInvariant();
    }

    private static Dictionary<string, List<string>> GroupByMatchKey(IEnumerable<string> paths)
    {
        var groups = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);

        foreach (var path in paths)
        {
            var key = GetMatchKey(path) ?? Path.GetFileNameWithoutExtension(path);
            if (!groups.TryGetValue(key, out var list))
            {
                list = [];
                groups[key] = list;
            }

            list.Add(path);
        }

        return groups;
    }

    public static string ResolveOutputDirectoryForPairs(
        IEnumerable<MergePair> pairs,
        string outputDirectory)
    {
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            return outputDirectory;
        }

        var firstNani = pairs.Select(pair => pair.NaniPath).FirstOrDefault();
        return firstNani is not null
            ? Path.GetDirectoryName(firstNani) ?? string.Empty
            : string.Empty;
    }

    private static string ResolveOutputDirectory(IReadOnlyList<string> naniList, string outputDirectory)
    {
        if (!string.IsNullOrWhiteSpace(outputDirectory))
        {
            return outputDirectory;
        }

        return naniList.Count > 0
            ? Path.GetDirectoryName(naniList[0]) ?? string.Empty
            : string.Empty;
    }
}
