using MergeNani.Models;

namespace MergeNani;

partial class MainForm
{
    private readonly List<string> _batchNaniPaths = [];
    private readonly List<string> _batchTextPaths = [];
    private readonly List<MergePair> _batchPairs = [];

    private const int BatchTextColumnIndex = 3;

    private readonly DataGridView _batchGrid = new()
    {
        Dock = DockStyle.Fill,
        ReadOnly = false,
        AllowUserToAddRows = false,
        AllowUserToDeleteRows = false,
        AutoGenerateColumns = false,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
        SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        MultiSelect = false,
        RowHeadersVisible = false,
        ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize,
    };
    private readonly TextBox _batchOutputDirTextBox = new();
    private readonly CheckBox _batchFilenameMToFCheckBox = new()
    {
        Text = "Имя файла: ..._m → ..._f",
        AutoSize = true,
        Anchor = AnchorStyles.Left,
    };
    private readonly CheckBox _batchFilenameFToMCheckBox = new()
    {
        Text = "Имя файла: ..._f → ..._m",
        AutoSize = true,
        Anchor = AnchorStyles.Left,
    };
    private readonly CheckBox _batchSceneMToFCheckBox = new()
    {
        Text = "Заменить ссылки на сцены ..._m → ..._f",
        AutoSize = true,
        Anchor = AnchorStyles.Left,
    };
    private readonly CheckBox _batchSceneFToMCheckBox = new()
    {
        Text = "Заменить ссылки на сцены ..._f → ..._m",
        AutoSize = true,
        Anchor = AnchorStyles.Left,
    };
    private readonly Label _batchInfoLabel = new() { AutoSize = true, Text = "Пары: —" };

    private void WireBatchUiEvents()
    {
        _batchFilenameMToFCheckBox.CheckedChanged += (_, _) =>
        {
            if (_batchFilenameMToFCheckBox.Checked)
            {
                _batchFilenameFToMCheckBox.Checked = false;
            }

            RefreshBatchPairs();
        };

        _batchFilenameFToMCheckBox.CheckedChanged += (_, _) =>
        {
            if (_batchFilenameFToMCheckBox.Checked)
            {
                _batchFilenameMToFCheckBox.Checked = false;
            }

            RefreshBatchPairs();
        };

        _batchSceneMToFCheckBox.CheckedChanged += (_, _) =>
        {
            if (_batchSceneMToFCheckBox.Checked)
            {
                _batchSceneFToMCheckBox.Checked = false;
            }
        };

        _batchSceneFToMCheckBox.CheckedChanged += (_, _) =>
        {
            if (_batchSceneFToMCheckBox.Checked)
            {
                _batchSceneMToFCheckBox.Checked = false;
            }
        };

        _batchGrid.CurrentCellDirtyStateChanged += (_, _) =>
        {
            if (_batchGrid.IsCurrentCellDirty)
            {
                _batchGrid.CommitEdit(DataGridViewDataErrorContexts.Commit);
            }
        };
        _batchGrid.CellValueChanged += OnBatchGridCellChanged;
        _batchGrid.CellFormatting += OnBatchGridCellFormatting;
    }

    private void OnBatchGridCellFormatting(object? sender, DataGridViewCellFormattingEventArgs e)
    {
        if (e.ColumnIndex != BatchTextColumnIndex || e.Value is not string path || string.IsNullOrWhiteSpace(path))
        {
            return;
        }

        e.Value = Path.GetFileName(path);
        e.FormattingApplied = true;
    }

    private void OnBatchGridCellChanged(object? sender, DataGridViewCellEventArgs e)
    {
        if (e.RowIndex < 0 || e.ColumnIndex != BatchTextColumnIndex)
        {
            return;
        }

        if (_batchGrid.Rows[e.RowIndex].Tag is not MergePair pair)
        {
            return;
        }

        var selected = _batchGrid.Rows[e.RowIndex].Cells[BatchTextColumnIndex].Value as string;
        pair.TextPath = string.IsNullOrWhiteSpace(selected) ? null : selected;
        PairMatcher.RefreshLineCounts(pair);
        ApplyBatchRowStyle(_batchGrid.Rows[e.RowIndex], pair);
        UpdateBatchInfo();
    }

    private void AddBatchNaniFiles(IEnumerable<string> paths)
    {
        var added = false;
        foreach (var path in paths.Where(TextSourceFiles.IsNani))
        {
            if (_batchNaniPaths.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            _batchNaniPaths.Add(path);
            added = true;
        }

        if (added)
        {
            RefreshBatchPairs();
        }
    }

    private void AddBatchTextFiles(IEnumerable<string> paths)
    {
        var added = false;
        foreach (var path in paths.Where(TextSourceFiles.IsSupported))
        {
            if (_batchTextPaths.Contains(path, StringComparer.OrdinalIgnoreCase))
            {
                continue;
            }

            _batchTextPaths.Add(path);
            added = true;
        }

        if (added)
        {
            RefreshBatchPairs();
        }
    }

    private void OnBatchFilesDropped(object? sender, DragEventArgs e)
    {
        foreach (var path in GetDroppedPaths(e))
        {
            if (TextSourceFiles.IsNani(path))
            {
                AddBatchNaniFiles([path]);
            }
            else if (TextSourceFiles.IsSupported(path))
            {
                AddBatchTextFiles([path]);
            }
        }
    }

    private void OnPickBatchNani(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Добавить .nani файлы",
            Filter = "Naninovel scripts (*.nani)|*.nani|All files (*.*)|*.*",
            Multiselect = true,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            AddBatchNaniFiles(dialog.FileNames);
        }
    }

    private void OnPickBatchText(object? sender, EventArgs e)
    {
        using var dialog = new OpenFileDialog
        {
            Title = "Добавить текстовые файлы",
            Filter = TextSourceFiles.OpenFileFilter,
            Multiselect = true,
        };

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            AddBatchTextFiles(dialog.FileNames);
        }
    }

    private void OnPickBatchOutputDir(object? sender, EventArgs e)
    {
        using var dialog = new FolderBrowserDialog
        {
            Description = "Папка для результатов",
            UseDescriptionForTitle = true,
        };

        var current = _batchOutputDirTextBox.Text.Trim();
        if (!string.IsNullOrWhiteSpace(current) && Directory.Exists(current))
        {
            dialog.SelectedPath = current;
        }

        if (dialog.ShowDialog(this) == DialogResult.OK)
        {
            _batchOutputDirTextBox.Text = dialog.SelectedPath;
            RefreshBatchPairs();
        }
    }

    private BatchMergeOptions ReadBatchOptions()
        => new()
        {
            OutputDirectory = _batchOutputDirTextBox.Text.Trim(),
            FilenameMToF = _batchFilenameMToFCheckBox.Checked,
            FilenameFToM = _batchFilenameFToMCheckBox.Checked,
            Transforms = new TransformOptions
            {
                SceneMToF = _batchSceneMToFCheckBox.Checked,
                SceneFToM = _batchSceneFToMCheckBox.Checked,
            },
        };

    private void RefreshBatchPairs()
    {
        var previousSelections = _batchPairs.ToDictionary(
            pair => pair.NaniPath,
            pair => pair.TextPath,
            StringComparer.OrdinalIgnoreCase);

        _batchPairs.Clear();
        _batchPairs.AddRange(BatchMerger.BuildPairs(_batchNaniPaths, _batchTextPaths, ReadBatchOptions()));

        foreach (var pair in _batchPairs)
        {
            if (previousSelections.TryGetValue(pair.NaniPath, out var textPath)
                && !string.IsNullOrWhiteSpace(textPath)
                && _batchTextPaths.Contains(textPath, StringComparer.OrdinalIgnoreCase))
            {
                pair.TextPath = textPath;
                PairMatcher.RefreshLineCounts(pair);
            }
        }

        if (string.IsNullOrWhiteSpace(_batchOutputDirTextBox.Text) && _batchNaniPaths.Count > 0)
        {
            _batchOutputDirTextBox.Text = Path.GetDirectoryName(_batchNaniPaths[0]) ?? string.Empty;
            BatchMerger.RefreshPairs(_batchPairs, ReadBatchOptions());
        }

        BindBatchGrid();
        UpdateBatchInfo();
    }

    private void BindBatchGrid()
    {
        _batchGrid.DataSource = null;
        _batchGrid.Columns.Clear();
        _batchGrid.Rows.Clear();

        _batchGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Indicator",
            HeaderText = string.Empty,
            ReadOnly = true,
            Width = 8,
            Resizable = DataGridViewTriState.False,
            SortMode = DataGridViewColumnSortMode.NotSortable,
        });
        _batchGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Nani",
            HeaderText = ".nani",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            SortMode = DataGridViewColumnSortMode.NotSortable,
        });
        _batchGrid.Columns.Add(new DataGridViewTextBoxColumn
        {
            Name = "Key",
            HeaderText = "Ключ",
            ReadOnly = true,
            AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
            SortMode = DataGridViewColumnSortMode.NotSortable,
        });

        var textColumn = new DataGridViewComboBoxColumn
        {
            Name = "Text",
            HeaderText = "Текст",
            AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
            DisplayStyle = DataGridViewComboBoxDisplayStyle.DropDownButton,
            SortMode = DataGridViewColumnSortMode.NotSortable,
        };
        textColumn.Items.Add(string.Empty);
        foreach (var path in _batchTextPaths)
        {
            textColumn.Items.Add(path);
        }

        _batchGrid.Columns.Add(textColumn);

        foreach (var pair in _batchPairs)
        {
            var rowIndex = _batchGrid.Rows.Add(
                string.Empty,
                pair.DisplayName,
                pair.MatchKey ?? string.Empty,
                pair.TextPath ?? string.Empty);
            var row = _batchGrid.Rows[rowIndex];
            row.Tag = pair;
            ApplyBatchRowStyle(row, pair);
        }
    }

    private static void ApplyBatchRowStyle(DataGridViewRow row, MergePair pair)
    {
        var color = pair.IsReady
            ? Color.FromArgb(112, 173, 71)
            : Color.FromArgb(255, 0, 0);

        row.Cells[0].Style.BackColor = color;
        row.Cells[0].Style.SelectionBackColor = color;

        for (var i = 1; i < row.Cells.Count; i++)
        {
            row.Cells[i].Style.BackColor = Color.White;
            row.Cells[i].Style.SelectionBackColor = SystemColors.Highlight;
            row.Cells[i].Style.ForeColor = SystemColors.ControlText;
        }
    }

    private void UpdateBatchInfo()
    {
        if (_batchPairs.Count == 0)
        {
            _batchInfoLabel.Text = "Пары: добавьте .nani и текстовые файлы";
            return;
        }

        var ready = _batchPairs.Count(pair => pair.IsReady);
        _batchInfoLabel.Text =
            $"Пары: {_batchPairs.Count}, готово к сборке: {ready}, ошибок: {_batchPairs.Count - ready}";
    }

    private void RunBatchMerge()
    {
        if (_batchPairs.Count == 0)
        {
            ShowError("Добавьте .nani файлы для пакетной сборки.");
            return;
        }

        if (_batchFilenameMToFCheckBox.Checked && _batchFilenameFToMCheckBox.Checked)
        {
            ShowError("Включите только одну замену окончания имени файла.");
            return;
        }

        if (_batchSceneMToFCheckBox.Checked && _batchSceneFToMCheckBox.Checked)
        {
            ShowError("Включите только одну замену ссылок на сцены.");
            return;
        }

        BatchMerger.RefreshPairs(_batchPairs, ReadBatchOptions());
        BindBatchGrid();
        UpdateBatchInfo();

        var result = BatchMerger.MergeAll(_batchPairs, ReadBatchOptions());
        MessageBox.Show(this, BatchMerger.BuildSummary(result), "Пакетная сборка",
            MessageBoxButtons.OK,
            result.FailureCount > 0 ? MessageBoxIcon.Warning : MessageBoxIcon.Information);
    }
}
