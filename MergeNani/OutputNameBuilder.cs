namespace MergeNani;

internal static class OutputNameBuilder
{
    public static string Build(
        string naniPath,
        string outputDirectory,
        string nameSuffix,
        bool filenameMToF,
        bool filenameFToM)
    {
        if (filenameMToF && filenameFToM)
        {
            throw new InvalidOperationException(
                "Включите только одну замену окончания имени: _m → _f или _f → _m");
        }

        var baseName = Path.GetFileNameWithoutExtension(naniPath);

        if (filenameMToF)
        {
            baseName = ReplaceFilenameSuffix(baseName, "_m", "_f");
        }
        else if (filenameFToM)
        {
            baseName = ReplaceFilenameSuffix(baseName, "_f", "_m");
        }

        if (!string.IsNullOrEmpty(nameSuffix))
        {
            baseName += nameSuffix;
        }

        return Path.Combine(outputDirectory, $"{baseName}.nani");
    }

    private static string ReplaceFilenameSuffix(string baseName, string from, string to)
        => baseName.EndsWith(from, StringComparison.OrdinalIgnoreCase)
            ? baseName[..^from.Length] + to
            : baseName;
}
