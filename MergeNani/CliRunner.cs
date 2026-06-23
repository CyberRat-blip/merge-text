using MergeNani.Models;

namespace MergeNani;

internal static class CliRunner
{
    public static int Run(string[] args)
    {
        if (!TryParse(args, out var options, out var error))
        {
            Console.Error.WriteLine(error);
            return 1;
        }

        try
        {
            if (options!.IsBatch)
            {
                return RunBatch(options);
            }

            var result = NaniMerger.MergeFiles(
                options.NaniPath!,
                options.TextPath!,
                options.OutputPath!,
                options.Transforms.HasAnyReplace ? options.Transforms : null);

            Console.Error.WriteLine(
                $"Written: {options.OutputPath} ({result.DialogueLinesReplaced} dialogue lines replaced)");
            return 0;
        }
        catch (Exception ex)
        {
            Console.Error.WriteLine($"Error: {ex.Message}");
            return 1;
        }
    }

    private static int RunBatch(CliOptions options)
    {
        var naniPaths = ListFiles(options.NaniPath!, TextSourceFiles.IsNani);
        var textPaths = ListFiles(options.TextPath!, TextSourceFiles.IsSupported);

        if (naniPaths.Count == 0)
        {
            Console.Error.WriteLine("В папке .nani не найдено файлов.");
            return 1;
        }

        if (textPaths.Count == 0)
        {
            Console.Error.WriteLine("В папке текста не найдено поддерживаемых файлов.");
            return 1;
        }

        var batchOptions = new BatchMergeOptions
        {
            OutputDirectory = options.OutputPath ?? string.Empty,
            NameSuffix = options.NameSuffix,
            FilenameMToF = options.FilenameMToF,
            FilenameFToM = options.FilenameFToM,
            Transforms = options.Transforms,
        };

        var pairs = BatchMerger.BuildPairs(naniPaths, textPaths, batchOptions);
        var result = BatchMerger.MergeAll(pairs, batchOptions);
        Console.Error.WriteLine(BatchMerger.BuildSummary(result));
        return result.FailureCount > 0 ? 1 : 0;
    }

    private static List<string> ListFiles(string directory, Func<string, bool> matches)
        => Directory.Exists(directory)
            ? Directory.GetFiles(directory)
                .Where(matches)
                .OrderBy(path => path, StringComparer.OrdinalIgnoreCase)
                .ToList()
            : [];

    private static bool TryParse(string[] args, out CliOptions? options, out string error)
    {
        options = null;
        error = string.Empty;

        var isBatch = false;
        string? naniPath = null;
        string? textPath = null;
        string? outputPath = null;
        var nameSuffix = string.Empty;
        var filenameMToF = false;
        var filenameFToM = false;
        var transforms = new TransformOptions();

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "--batch":
                    isBatch = true;
                    break;

                case "-o":
                case "--output":
                    if (!TryReadNext(args, ref i, out outputPath))
                    {
                        error = "Не указан путь для -o";
                        return false;
                    }
                    break;

                case "--suffix":
                    if (!TryReadNext(args, ref i, out nameSuffix))
                    {
                        error = "Не указано значение для --suffix";
                        return false;
                    }
                    break;

                case "--filename-suffix-m2f":
                    filenameMToF = true;
                    break;

                case "--filename-suffix-f2m":
                    filenameFToM = true;
                    break;

                case "--find":
                    if (!TryReadNext(args, ref i, out var nameFrom))
                    {
                        error = "Не указано значение для --find";
                        return false;
                    }
                    transforms = transforms with { NameFrom = nameFrom };
                    break;

                case "--replace-with":
                    if (!TryReadNext(args, ref i, out var nameTo))
                    {
                        error = "Не указано значение для --replace-with";
                        return false;
                    }
                    transforms = transforms with { NameTo = nameTo };
                    break;

                case "--scene-suffix":
                    transforms = transforms with { SceneMToF = true };
                    break;

                case "--scene-suffix-f":
                    transforms = transforms with { SceneFToM = true };
                    break;

                default:
                    if (naniPath is null)
                    {
                        naniPath = args[i];
                    }
                    else if (textPath is null)
                    {
                        textPath = args[i];
                    }
                    else
                    {
                        error = $"Лишний аргумент: {args[i]}";
                        return false;
                    }
                    break;
            }
        }

        if (naniPath is null || textPath is null)
        {
            error =
                "Usage: MergeNani <file.nani> <text file> [-o output.nani] " +
                "[--find NAME] [--replace-with NAME] [--scene-suffix] [--scene-suffix-f]\n" +
                "   or: MergeNani --batch <nani-folder> <text-folder> -o <out-folder> " +
                "[--suffix SUFFIX] [--filename-suffix-m2f] [--filename-suffix-f2m] " +
                "[--find NAME] [--replace-with NAME] [--scene-suffix] [--scene-suffix-f]";
            return false;
        }

        if (isBatch)
        {
            if (string.IsNullOrWhiteSpace(outputPath))
            {
                error = "Для --batch укажите папку результата через -o";
                return false;
            }

            if (filenameMToF && filenameFToM)
            {
                error = "Включите только одну замену окончания имени файла.";
                return false;
            }

            options = new CliOptions
            {
                IsBatch = true,
                NaniPath = naniPath,
                TextPath = textPath,
                OutputPath = outputPath,
                NameSuffix = nameSuffix,
                FilenameMToF = filenameMToF,
                FilenameFToM = filenameFToM,
                Transforms = transforms,
            };

            return true;
        }

        options = new CliOptions
        {
            IsBatch = false,
            NaniPath = naniPath,
            TextPath = textPath,
            OutputPath = string.IsNullOrWhiteSpace(outputPath)
                ? NaniMerger.SuggestOutputPath(naniPath)
                : outputPath,
            Transforms = transforms,
        };

        return true;
    }

    private static bool TryReadNext(string[] args, ref int index, out string value)
    {
        if (index + 1 >= args.Length)
        {
            value = string.Empty;
            return false;
        }

        value = args[++index];
        return true;
    }

    private sealed record CliOptions
    {
        public bool IsBatch { get; init; }
        public string? NaniPath { get; init; }
        public string? TextPath { get; init; }
        public string? OutputPath { get; init; }
        public string NameSuffix { get; init; } = string.Empty;
        public bool FilenameMToF { get; init; }
        public bool FilenameFToM { get; init; }
        public TransformOptions Transforms { get; init; } = new();
    }
}
