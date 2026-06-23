namespace MergeNani;

partial class MainForm
{
    private void InitializeLayout()
    {
        var root = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 3,
            Padding = new Padding(16),
        };
        root.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        AddAutoSizeRows(root, 2);
        root.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        Controls.Add(root);

        root.Controls.Add(CreateTitleLabel(), 0, 0);
        root.Controls.Add(CreateSubtitleLabel(), 0, 1);
        root.Controls.Add(CreateTabSection(), 0, 2);
    }

    private Control CreateTabSection()
    {
        var tabs = new TabControl { Dock = DockStyle.Fill, Margin = new Padding(0, 0, 0, 8) };
        tabs.TabPages.Add(CreateSingleFileTab());
        tabs.TabPages.Add(CreateBatchTab());
        return tabs;
    }

    private TabPage CreateSingleFileTab()
    {
        var page = new TabPage("Один файл") { Padding = new Padding(8) };
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 4,
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));

        panel.Controls.Add(CreateFilesSection(), 0, 0);
        panel.Controls.Add(CreateOptionsSection(), 0, 1);
        panel.Controls.Add(_infoLabel, 0, 2);
        panel.Controls.Add(CreateSingleButtonSection(), 0, 3);
        page.Controls.Add(panel);
        return page;
    }

    private TabPage CreateBatchTab()
    {
        var page = new TabPage("Пакет") { Padding = new Padding(8) };
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill,
            ColumnCount = 1,
            RowCount = 5,
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Percent, 100F));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.Absolute, 36F));

        panel.Controls.Add(CreateBatchToolbar(), 0, 0);
        panel.Controls.Add(CreateBatchOutputSection(), 0, 1);
        panel.Controls.Add(CreateBatchGridSection(), 0, 2);
        panel.Controls.Add(_batchInfoLabel, 0, 3);
        panel.Controls.Add(CreateBatchButtonSection(), 0, 4);
        page.Controls.Add(panel);
        return page;
    }

    private Control CreateFilesSection()
    {
        var panel = new Panel { Dock = DockStyle.Top, Height = 108, Margin = new Padding(0, 0, 0, 8) };

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

    private Control CreateBatchToolbar()
    {
        var panel = new FlowLayoutPanel
        {
            Dock = DockStyle.Fill,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = false,
            Margin = new Padding(0, 0, 0, 8),
        };

        var addNani = new Button { Text = "Добавить .nani…", AutoSize = true, Margin = new Padding(0, 0, 8, 0) };
        addNani.Click += OnPickBatchNani;
        panel.Controls.Add(addNani);

        var addText = new Button { Text = "Добавить текст…", AutoSize = true };
        addText.Click += OnPickBatchText;
        panel.Controls.Add(addText);

        return panel;
    }

    private Control CreateBatchOutputSection()
    {
        var panel = new TableLayoutPanel
        {
            Dock = DockStyle.Top,
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            ColumnCount = 3,
            RowCount = 3,
            Margin = new Padding(0, 0, 0, 8),
        };
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 110F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F));
        panel.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 90F));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        panel.RowStyles.Add(new RowStyle(SizeType.AutoSize));

        AddPathRow(panel, 0, "Результат:", _batchOutputDirTextBox, OnPickBatchOutputDir, OnBatchOutputDirDropped);

        var filenamePanel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 4, 0, 0),
        };
        filenamePanel.Controls.Add(_batchFilenameMToFCheckBox);
        filenamePanel.Controls.Add(_batchFilenameFToMCheckBox);
        panel.SetColumnSpan(filenamePanel, 3);
        panel.Controls.Add(filenamePanel, 0, 1);

        var scenePanel = new FlowLayoutPanel
        {
            AutoSize = true,
            AutoSizeMode = AutoSizeMode.GrowAndShrink,
            FlowDirection = FlowDirection.LeftToRight,
            WrapContents = true,
            Margin = new Padding(0, 4, 0, 0),
        };
        scenePanel.Controls.Add(_batchSceneMToFCheckBox);
        scenePanel.Controls.Add(_batchSceneFToMCheckBox);
        panel.SetColumnSpan(scenePanel, 3);
        panel.Controls.Add(scenePanel, 0, 2);

        return panel;
    }

    private void OnBatchOutputDirDropped(object? sender, DragEventArgs e)
    {
        foreach (var path in GetDroppedPaths(e))
        {
            if (!Directory.Exists(path))
            {
                continue;
            }

            _batchOutputDirTextBox.Text = path;
            RefreshBatchPairs();
            return;
        }
    }

    private Control CreateBatchGridSection()
    {
        var panel = new Panel { Dock = DockStyle.Fill };
        panel.Controls.Add(_batchGrid);
        EnableFileDrop(panel, OnBatchFilesDropped);
        EnableFileDrop(_batchGrid, OnBatchFilesDropped);
        return panel;
    }

    private Control CreateOptionsSection()
    {
        var group = new GroupBox
        {
            Text = "Замены после подстановки текста",
            Dock = DockStyle.Top,
            Height = 170,
            Margin = new Padding(0, 0, 0, 8),
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

    private Control CreateSingleButtonSection()
        => CreateButtonSection("Обновить проверку", (_, _) => UpdateLineCount(), "Собрать .nani", (_, _) => RunMerge());

    private Control CreateBatchButtonSection()
        => CreateButtonSection("Обновить проверку", (_, _) => RefreshBatchPairs(), "Собрать все", (_, _) => RunBatchMerge());

    private static Control CreateButtonSection(
        string refreshText,
        EventHandler onRefresh,
        string mergeText,
        EventHandler onMerge)
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

        var refresh = new Button { Text = refreshText, AutoSize = true, Anchor = AnchorStyles.Left };
        refresh.Click += onRefresh;
        panel.Controls.Add(refresh, 0, 0);

        var merge = new Button { Text = mergeText, AutoSize = true, Anchor = AnchorStyles.Right };
        merge.Click += onMerge;
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
            Text = "Перетащите файлы в поля или выберите через «Обзор». Во вкладке «Пакет» можно обработать сразу много файлов.",
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

    private static void AddPathRow(
        TableLayoutPanel parent,
        int row,
        string caption,
        TextBox pathBox,
        EventHandler onBrowse,
        DragEventHandler onDrop)
    {
        parent.Controls.Add(FieldLabel(caption), 0, row);

        pathBox.Multiline = true;
        pathBox.ReadOnly = true;
        pathBox.BorderStyle = BorderStyle.FixedSingle;
        pathBox.ScrollBars = ScrollBars.None;
        pathBox.WordWrap = true;
        pathBox.Dock = DockStyle.Top;
        pathBox.Margin = new Padding(0, 4, 8, 4);
        pathBox.TextChanged += (_, _) => AdjustPathBoxHeight(pathBox, parent);
        parent.Controls.Add(pathBox, 1, row);
        parent.Layout += (_, _) => AdjustPathBoxHeight(pathBox, parent);

        var browse = new Button { Text = "Обзор...", AutoSize = true, Anchor = AnchorStyles.Top | AnchorStyles.Right, Margin = new Padding(0, 4, 0, 4) };
        browse.Click += onBrowse;
        parent.Controls.Add(browse, 2, row);

        EnableFileDrop(pathBox, onDrop);
    }

    private static void AdjustPathBoxHeight(TextBox pathBox, Control widthReference)
    {
        if (string.IsNullOrEmpty(pathBox.Text))
        {
            pathBox.Height = 23;
            return;
        }

        var width = Math.Max(100, widthReference.ClientSize.Width - 220);
        var size = TextRenderer.MeasureText(
            pathBox.Text,
            pathBox.Font,
            new Size(width, int.MaxValue),
            TextFormatFlags.WordBreak | TextFormatFlags.TextBoxControl);

        pathBox.Height = Math.Max(23, size.Height + 6);
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
