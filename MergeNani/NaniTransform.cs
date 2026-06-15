using System.Text.RegularExpressions;
using MergeNani.Models;

namespace MergeNani;

internal static class NaniTransform
{
    private static readonly Regex SceneMToF = new(
        @"(.+?_scene_[\w\d_]+)_m\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    private static readonly Regex SceneFToM = new(
        @"(.+?_scene_[\w\d_]+)_f\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant);

    public static (List<string> Lines, TransformStats Stats) Apply(
        IReadOnlyList<string> lines,
        TransformOptions options)
    {
        if (!options.HasAnyReplace)
        {
            return (lines.ToList(), new TransformStats());
        }

        if (options.SceneMToF && options.SceneFToM)
        {
            throw new InvalidOperationException(
                "Включите только одну замену ссылок: _m → _f или _f → _m");
        }

        var namePattern = options.HasNameReplace
            ? new Regex($@"\b{Regex.Escape(options.NameFrom.Trim())}\b", RegexOptions.Compiled)
            : null;

        var stats = new TransformStats();
        var result = lines
            .Select(line => TransformLine(line, options, namePattern, stats))
            .ToList();

        return (result, stats);
    }

    private static string TransformLine(
        string line,
        TransformOptions options,
        Regex? namePattern,
        TransformStats stats)
    {
        var text = line;

        if (options.SceneMToF)
        {
            text = ReplaceSceneRefs(text, SceneMToF, "_f", stats);
        }

        if (options.SceneFToM)
        {
            text = ReplaceSceneRefs(text, SceneFToM, "_m", stats);
        }

        if (namePattern is not null)
        {
            text = ReplaceName(text, namePattern, options.NameTo.Trim(), stats);
        }

        return text;
    }

    private static string ReplaceSceneRefs(
        string line,
        Regex pattern,
        string suffix,
        TransformStats stats)
    {
        return pattern.Replace(line, match =>
        {
            stats.SceneRefs++;
            return $"{match.Groups[1].Value}{suffix}";
        });
    }

    private static string ReplaceName(
        string line,
        Regex pattern,
        string to,
        TransformStats stats)
    {
        var count = 0;
        var result = pattern.Replace(line, _ =>
        {
            count++;
            return to;
        });

        stats.NameRefs += count;
        return result;
    }
}
