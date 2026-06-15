using MergeNani.Models;

namespace MergeNani;

partial class MainForm : Form
{
    private readonly TextBox _naniPathTextBox = new() { ReadOnly = true };
    private readonly TextBox _textPathTextBox = new() { ReadOnly = true };
    private readonly TextBox _outputPathTextBox = new();
    private readonly TextBox _nameFromTextBox = new();
    private readonly TextBox _nameToTextBox = new();
    private readonly CheckBox _sceneMToFCheckBox = new()
    {
        Text = "Заменить ссылки на сцены ..._m → ..._f",
        AutoSize = true,
        Anchor = AnchorStyles.Left,
    };
    private readonly CheckBox _sceneFToMCheckBox = new()
    {
        Text = "Заменить ссылки на сцены ..._f → ..._m",
        AutoSize = true,
        Anchor = AnchorStyles.Left,
    };
    private readonly Label _infoLabel = new() { AutoSize = true, Text = "Строки диалога: —" };

    private bool _outputEditedByUser;
    private bool _suppressOutputEvents;

    public MainForm()
    {
        Text = "Merge Nani";
        MinimumSize = new Size(720, 480);
        Size = new Size(760, 500);
        StartPosition = FormStartPosition.CenterScreen;
        Font = new Font("Segoe UI", 9F);

        var icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
        if (icon is not null)
        {
            Icon = icon;
        }

        InitializeLayout();
        WireUiEvents();
    }

    private void WireUiEvents()
    {
        _outputPathTextBox.TextChanged += (_, _) =>
        {
            if (!_suppressOutputEvents)
            {
                _outputEditedByUser = true;
            }
        };

        _sceneMToFCheckBox.CheckedChanged += (_, _) =>
        {
            if (_sceneMToFCheckBox.Checked)
            {
                _sceneFToMCheckBox.Checked = false;
            }
        };

        _sceneFToMCheckBox.CheckedChanged += (_, _) =>
        {
            if (_sceneFToMCheckBox.Checked)
            {
                _sceneMToFCheckBox.Checked = false;
            }
        };
    }

    private void SelectNaniFile(string path)
    {
        _outputEditedByUser = false;
        _naniPathTextBox.Text = path;
        ApplySuggestedOutput(path);
        UpdateLineCount();
    }

    private void ApplySuggestedOutput(string naniPath)
    {
        if (_outputEditedByUser)
        {
            return;
        }

        _suppressOutputEvents = true;
        _outputPathTextBox.Text = NaniMerger.SuggestOutputPath(naniPath);
        _suppressOutputEvents = false;
    }

    private void OnNaniDropped(object? sender, DragEventArgs e)
        => AssignFirstMatchingFile(e, TextSourceFiles.IsNani, SelectNaniFile);

    private void OnTextDropped(object? sender, DragEventArgs e)
        => AssignFirstMatchingFile(e, TextSourceFiles.IsSupported, path =>
        {
            _textPathTextBox.Text = path;
            UpdateLineCount();
        });

    private void OnOutputDropped(object? sender, DragEventArgs e)
        => AssignFirstMatchingFile(e, TextSourceFiles.IsNani, path =>
        {
            _outputEditedByUser = true;
            _outputPathTextBox.Text = path;
        });

    private void OnFilesAreaDropped(object? sender, DragEventArgs e)
    {
        var naniAssigned = false;
        var textAssigned = false;

        foreach (var path in GetDroppedPaths(e))
        {
            if (!naniAssigned && TextSourceFiles.IsNani(path))
            {
                SelectNaniFile(path);
                naniAssigned = true;
            }
            else if (!textAssigned && TextSourceFiles.IsSupported(path))
            {
                _textPathTextBox.Text = path;
                textAssigned = true;
            }
        }

        UpdateLineCount();
    }

    private static void AssignFirstMatchingFile(
        DragEventArgs e,
        Func<string, bool> matches,
        Action<string> assign)
    {
        foreach (var path in GetDroppedPaths(e))
        {
            if (matches(path))
            {
                assign(path);
                return;
            }
        }
    }

    private static IEnumerable<string> GetDroppedPaths(DragEventArgs e)
    {
        if (e.Data?.GetData(DataFormats.FileDrop) is not string[] paths)
        {
            return [];
        }

        return paths;
    }

    private void OnPickNani(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Выберите .nani файл",
            Filter = "Naninovel scripts (*.nani)|*.nani|All files (*.*)|*.*",
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            SelectNaniFile(dialog.FileName);
        }
    }

    private void OnPickText(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Выберите текстовый файл",
            Filter = TextSourceFiles.OpenFileFilter,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _textPathTextBox.Text = dialog.FileName;
            UpdateLineCount();
        }
    }

    private void OnPickOutput(object? sender, EventArgs e)
    {
        var current = string.IsNullOrWhiteSpace(_outputPathTextBox.Text)
            ? _naniPathTextBox.Text.Trim()
            : _outputPathTextBox.Text.Trim();

        using var dialog = new SaveFileDialog
        {
            Title = "Куда сохранить результат",
            Filter = "Naninovel scripts (*.nani)|*.nani|All files (*.*)|*.*",
            DefaultExt = "nani",
            FileName = string.IsNullOrWhiteSpace(current) ? "result.nani" : Path.GetFileName(current),
            InitialDirectory = string.IsNullOrWhiteSpace(current) ? null : Path.GetDirectoryName(current),
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _outputEditedByUser = true;
            _outputPathTextBox.Text = dialog.FileName;
        }
    }

    private void UpdateLineCount()
    {
        var naniPath = _naniPathTextBox.Text.Trim();
        var textPath = _textPathTextBox.Text.Trim();

        if (!File.Exists(naniPath) || !File.Exists(textPath))
        {
            _infoLabel.Text = "Строки диалога: выберите оба файла";
            return;
        }

        try
        {
            var naniLines = NaniMerger.CountDialogueLines(File.ReadAllLines(naniPath));
            var textLines = File.ReadAllLines(textPath).Count(line => !string.IsNullOrWhiteSpace(line));
            var status = naniLines == textLines ? "совпадают" : "НЕ совпадают";
            _infoLabel.Text = $"Строки диалога: .nani = {naniLines}, текст = {textLines} ({status})";
        }
        catch (Exception ex)
        {
            _infoLabel.Text = $"Ошибка чтения: {ex.Message}";
        }
    }

    private TransformOptions ReadTransformOptions()
        => new()
        {
            NameFrom = _nameFromTextBox.Text,
            NameTo = _nameToTextBox.Text,
            SceneMToF = _sceneMToFCheckBox.Checked,
            SceneFToM = _sceneFToMCheckBox.Checked,
        };

    private void RunMerge()
    {
        var naniPath = _naniPathTextBox.Text.Trim();
        var textPath = _textPathTextBox.Text.Trim();
        var transforms = ReadTransformOptions();

        if (!ValidateInputs(naniPath, textPath, transforms, out var outputPath))
        {
            return;
        }

        try
        {
            var result = NaniMerger.MergeFiles(naniPath, textPath, outputPath, transforms);
            MessageBox.Show(this, BuildSuccessMessage(outputPath, result, transforms), "Готово",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, ex.Message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private bool ValidateInputs(string naniPath, string textPath, TransformOptions transforms, out string outputPath)
    {
        outputPath = NaniMerger.ResolveOutputPath(naniPath, _outputPathTextBox.Text);

        if (!File.Exists(naniPath))
        {
            ShowError("Укажите существующий .nani файл.");
            return false;
        }

        if (!File.Exists(textPath))
        {
            ShowError("Укажите существующий текстовый файл.");
            return false;
        }

        if (!TextSourceFiles.IsSupported(textPath))
        {
            ShowError("Неподдерживаемый формат текстового файла.");
            return false;
        }

        if (string.IsNullOrWhiteSpace(Path.GetFileName(outputPath)))
        {
            ShowError("Укажите имя файла результата.");
            return false;
        }

        if (transforms.HasNameReplace && string.IsNullOrWhiteSpace(transforms.NameTo))
        {
            ShowError("Укажите, на какое имя заменить.");
            return false;
        }

        return true;
    }

    private void ShowError(string message)
        => MessageBox.Show(this, message, "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);

    private static string BuildSuccessMessage(string outputPath, MergeResult result, TransformOptions transforms)
    {
        var lines = new List<string>
        {
            $"Файл сохранён:\n{outputPath}",
            $"Заменено строк диалога: {result.DialogueLinesReplaced}",
        };

        var stats = result.TransformStats;
        if (stats is null)
        {
            return string.Join('\n', lines);
        }

        if (stats.NameRefs > 0)
        {
            lines.Add($"Замен имени: {stats.NameRefs}");
        }

        if (stats.SceneRefs > 0)
        {
            var direction = transforms.SceneFToM ? "_f → _m" : "_m → _f";
            lines.Add($"Замен ссылок на сцены ({direction}): {stats.SceneRefs}");
        }

        return string.Join('\n', lines);
    }
}
