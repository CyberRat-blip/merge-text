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
            var result = NaniMerger.MergeFiles(
                options!.NaniPath,
                options.TextPath,
                options.OutputPath,
                options.Transforms);

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

    private static bool TryParse(string[] args, out CliOptions? options, out string error)
    {
        options = null;
        error = string.Empty;

        string? naniPath = null;
        string? textPath = null;
        string? outputPath = null;
        var transforms = new TransformOptions();

        for (var i = 0; i < args.Length; i++)
        {
            switch (args[i])
            {
                case "-o":
                case "--output":
                    if (!TryReadNext(args, ref i, out outputPath))
                    {
                        error = "Не указан путь для -o";
                        return false;
                    }
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
                "[--find NAME] [--replace-with NAME] [--scene-suffix] [--scene-suffix-f]";
            return false;
        }

        options = new CliOptions
        {
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
        public required string NaniPath { get; init; }
        public required string TextPath { get; init; }
        public required string OutputPath { get; init; }
        public TransformOptions Transforms { get; init; } = new();
    }
}
