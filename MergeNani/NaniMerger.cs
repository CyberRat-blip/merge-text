using MergeNani.Models;

namespace MergeNani;

internal static class NaniMerger
{
    public static bool IsDialogueLine(string line)
    {
        var text = line.Trim();
        if (text.Length == 0)
        {
            return false;
        }

        return text[0] is not ('@' or '#' or ';' or '{');
    }

    public static int CountDialogueLines(IEnumerable<string> lines)
        => lines.Count(IsDialogueLine);

    public static string SuggestOutputPath(string naniPath)
    {
        var directory = Path.GetDirectoryName(naniPath) ?? string.Empty;
        var name = Path.GetFileNameWithoutExtension(naniPath);
        return Path.Combine(directory, $"{name}_new.nani");
    }

    public static string ResolveOutputPath(string naniPath, string outputInput)
    {
        var input = outputInput.Trim();
        if (input.Length == 0)
        {
            return SuggestOutputPath(naniPath);
        }

        var isFileNameOnly = !input.Contains(Path.DirectorySeparatorChar)
            && !input.Contains(Path.AltDirectorySeparatorChar);

        if (isFileNameOnly)
        {
            var directory = Path.GetDirectoryName(naniPath) ?? string.Empty;
            return Path.Combine(directory, EnsureNaniExtension(input));
        }

        return EnsureNaniExtension(input);
    }

    public static List<string> MergeDialogue(IReadOnlyList<string> naniLines, IReadOnlyList<string> textLines)
    {
        var replacements = textLines
            .Select(line => line.TrimEnd('\r', '\n'))
            .Where(line => !string.IsNullOrWhiteSpace(line))
            .ToList();

        var dialogueCount = CountDialogueLines(naniLines);
        if (dialogueCount != replacements.Count)
        {
            throw new InvalidOperationException(
                $"Строки не совпадают: в .nani — {dialogueCount}, в тексте — {replacements.Count}");
        }

        var result = new List<string>(naniLines.Count);
        var index = 0;

        foreach (var line in naniLines)
        {
            result.Add(IsDialogueLine(line)
                ? replacements[index++]
                : line.TrimEnd('\r', '\n'));
        }

        return result;
    }

    public static MergeResult MergeFiles(
        string naniPath,
        string textPath,
        string outputPath,
        TransformOptions? transforms = null)
    {
        if (!File.Exists(naniPath))
        {
            throw new FileNotFoundException($"Не найден .nani: {naniPath}");
        }

        if (!File.Exists(textPath))
        {
            throw new FileNotFoundException($"Не найден текстовый файл: {textPath}");
        }

        var lines = MergeDialogue(File.ReadAllLines(naniPath), File.ReadAllLines(textPath));

        TransformStats? stats = null;
        if (transforms is not null && transforms.HasAnyReplace)
        {
            (lines, stats) = NaniTransform.Apply(lines, transforms);
        }

        var outputDir = Path.GetDirectoryName(outputPath);
        if (!string.IsNullOrWhiteSpace(outputDir))
        {
            Directory.CreateDirectory(outputDir);
        }

        File.WriteAllText(outputPath, string.Join('\n', lines) + '\n');
        return new MergeResult(CountDialogueLines(lines), stats);
    }

    private static string EnsureNaniExtension(string path)
        => path.EndsWith(".nani", StringComparison.OrdinalIgnoreCase) ? path : $"{path}.nani";
}
