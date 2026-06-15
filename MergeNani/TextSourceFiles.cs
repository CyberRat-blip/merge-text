namespace MergeNani;

internal static class TextSourceFiles
{
    private static readonly HashSet<string> Extensions = new(StringComparer.OrdinalIgnoreCase)
    {
        ".txt", ".text", ".md", ".markdown", ".csv", ".tsv", ".log", ".rtf",
    };

    public const string OpenFileFilter =
        "Text (*.txt;*.md;*.csv)|*.txt;*.md;*.csv|" +
        "Markdown (*.md;*.markdown)|*.md;*.markdown|" +
        "CSV (*.csv;*.tsv)|*.csv;*.tsv|" +
        "All (*.*)|*.*";

    public static bool IsSupported(string path)
        => Extensions.Contains(Path.GetExtension(path));

    public static bool IsNani(string path)
        => path.EndsWith(".nani", StringComparison.OrdinalIgnoreCase);
}
