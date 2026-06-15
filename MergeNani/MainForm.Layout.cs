namespace MergeNani;

partial class MainForm
{
    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 6,
            Padding = new Padding(16),
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        AddAutoSizeRows(root, 6);
        Controls.Add(root);

        root.Controls.Add(CreateTitleLabel(), 0, 0);
        root.Controls.Add(CreateSubtitleLabel(), 0, 1);
        root.Controls.Add(CreateFilesSection(), 0, 2);
        root.Controls.Add(CreateOptionsSection(), 0, 3);
        root.Controls.Add(_infoLabel, 0, 4);
        root.Controls.Add(CreateButtonSection(), 0, 5);
    }

    private Control CreateFilesSection()
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 108, Margin = new Padding(0, 0, 0, 12) };

        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 3, RowCount = 3 };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
        AddAutoSizeRows(grid, 3, SizeType.Percent, 33.33F);

        AddFileRow(grid, 0, "Файл .nani:", _naniPathTextBox, OnPickNani, OnNaniDropped);
        AddFileRow(grid, 1, "Текст:", _textPathTextBox, OnPickText, OnTextDropped);
        AddFileRow(grid, 2, "Результат:", _outputPathTextBox, OnPickOutput, OnOutputDropped);

        panel.Controls.Add(grid);
        EnableFileDrop(panel, OnFilesAreaDropped);
        return panel;
    }

    private Control CreateOptionsSection()
    {
        var group = new GroupBox
        {
            Text = "Замены после подстановки текста",
            Dock = DockStyle.Top,
            Height = 170,
            Margin = new Padding(0, 0, 0, 12),
            Padding = new Padding(12, 12, 12, 8),
        };

        var grid = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 5 };
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 100F));
        grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        AddFixedHeightRows(grid, 28, 28, 24, 24, 24);

        grid.Controls.Add(FieldLabel("Найти:"), 0, 0);
        _nameFromTextBox.Dock = DockStyle.Fill;
        grid.Controls.Add(_nameFromTextBox, 1, 0);

        grid.Controls.Add(FieldLabel("Заменить на:"), 0, 1);
        _nameToTextBox.Dock = DockStyle.Fill;
        grid.Controls.Add(_nameToTextBox, 1, 1);

        var hint = new Label
        {
            Text = "Имя ищется во всём файле: @char, диалоги, @goto и т.д.",
            ForeColor = Color.FromArgb(85, 85, 85),
            AutoSize = true,
            Anchor = AnchorStyles.Left,
        };
        grid.SetColumnSpan(hint, 2);
        grid.Controls.Add(hint, 0, 2);

        grid.SetColumnSpan(_sceneMToFCheckBox, 2);
        grid.Controls.Add(_sceneMToFCheckBox, 0, 3);

        grid.SetColumnSpan(_sceneFToMCheckBox, 2);
        grid.Controls.Add(_sceneFToMCheckBox, 0, 4);

        group.Controls.Add(grid);
        return group;
    }

    private Control CreateButtonSection()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            Height = 36,
            ColumnCount = 2,
            Margin = new Padding(0, 8, 0, 0),
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));

        var refresh = new Button { Text = "Обновить проверку", AutoSize = true, Anchor = AnchorStyles.Left };
        refresh.Click += (_, _) => UpdateLineCount();
        panel.Controls.Add(refresh, 0, 0);

        var merge = new Button { Text = "Собрать .nani", AutoSize = true, Anchor = AnchorStyles.Right };
        merge.Click += (_, _) => RunMerge();
        panel.Controls.Add(merge, 1, 0);

        return panel;
    }

    private static Label CreateTitleLabel()
        => new()
        {
            Text = "Подстановка текста в .nani",
            Font = new Font("Segoe UI", 14F, FontStyle.Bold),
            AutoSize = true,
            Margin = new Padding(0, 0, 0, 4),
            Dock = DockStyle.Top,
        };

    private static Label CreateSubtitleLabel()
        => new()
        {
            Text = "Необходимо перетащить файлы в поля или выбрать через «Обзор». 👇",
            AutoSize = true,
            MaximumSize = new Size(700, 0),
            Margin = new Padding(0, 0, 0, 12),
            Dock = DockStyle.Top,
        };

    private static Label FieldLabel(string text)
        => new()
        {
            Text = text,
            AutoSize = true,
            Anchor = AnchorStyles.Left,
            TextAlign = ContentAlignment.MiddleLeft,
        };

    private static void AddAutoSizeRows(TableLayoutPanel panel, int count)
        => AddAutoSizeRows(panel, count, SizeType.AutoSize);

    private static void AddAutoSizeRows(TableLayoutPanel panel, int count, SizeType type, float height = 0)
    {
        for (var i = 0; i < count; i++)
        {
            panel.RowStyles.Add(new RowStyle(type, height));
        }
    }

    private static void AddFixedHeightRows(TableLayoutPanel panel, params int[] heights)
    {
        foreach (var height in heights)
        {
            panel.RowStyles.Add(new RowStyle(SizeType.Absolute, height));
        }
    }

    private static void AddFileRow(
        TableLayoutPanel parent,
        int row,
        string caption,
        TextBox pathBox,
        EventHandler onBrowse,
        DragEventHandler onDrop)
    {
        parent.Controls.Add(FieldLabel(caption), 0, row);

        pathBox.Dock = DockStyle.Fill;
        pathBox.Margin = new Padding(0, 4, 8, 4);
        parent.Controls.Add(pathBox, 1, row);

        var browse = new Button { Text = "Обзор...", Dock = DockStyle.Fill, Margin = new Padding(0, 4, 0, 4) };
        browse.Click += onBrowse;
        parent.Controls.Add(browse, 2, row);

        EnableFileDrop(pathBox, onDrop);
    }

    private static void EnableFileDrop(Control control, DragEventHandler onDrop)
    {
        control.AllowDrop = true;
        control.DragEnter += (_, e) =>
        {
            if (e.Data?.GetDataPresent(DataFormats.FileDrop) == true)
            {
                e.Effect = DragDropEffects.Copy;
            }
        };
        control.DragDrop += onDrop;
    }
}
