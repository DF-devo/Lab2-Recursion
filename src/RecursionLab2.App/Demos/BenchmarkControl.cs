using RecursionLab2.Core.Benchmark;
using RecursionLab2.Core.Charting;

namespace RecursionLab2.App.Demos;

/// <summary>Стенд замеров времени: запускает серии, показывает таблицу и график, сохраняет CSV + PNG.</summary>
public sealed class BenchmarkControl : UserControl
{
    private readonly Button _hanoiButton = new() { Text = "Ханойские башни (n = 10..29)", Width = 260 };
    private readonly Button _queensButton = new() { Text = "N ферзей (n = 4..15)", Width = 260 };
    private readonly Button _quicksortButton = new() { Text = "Быстрая сортировка (n = 2¹⁰..2²⁴)", Width = 260 };
    private readonly Button _sierpinskiButton = new() { Text = "Серпинский (глубина 0..19)", Width = 260 };
    private readonly Button _saveButton = new() { Text = "Сохранить CSV + PNG", Width = 260, Enabled = false };
    private readonly CheckBox _logScale = new() { Text = "Логарифмическая шкала Y", AutoSize = true, Checked = true };
    private readonly Label _statusLabel = new() { AutoSize = true, Text = "Выберите алгоритм и запустите серию замеров.", Width = 260 };
    private readonly DataGridView _grid = new() { Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill };
    private readonly BufferedPanel _chart = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(21, 23, 31) };

    private BenchmarkSeries? _series;

    public BenchmarkControl()
    {
        var side = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 300,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12),
        };
        var chartHost = new Panel { Dock = DockStyle.Fill, Padding = new Padding(6) };
        chartHost.Controls.Add(_chart);

        var gridSplit = new SplitContainer
        {
            Dock = DockStyle.Fill,
            Orientation = Orientation.Horizontal,
            SplitterDistance = 220,
        };
        gridSplit.Panel1.Controls.Add(_grid);
        gridSplit.Panel2.Controls.Add(chartHost);

        side.Controls.Add(new Label { Text = "Серия замеров:", AutoSize = true, Margin = new Padding(0, 8, 0, 4), Font = new Font(Font, FontStyle.Bold) });
        side.Controls.Add(_hanoiButton);
        side.Controls.Add(_queensButton);
        side.Controls.Add(_quicksortButton);
        side.Controls.Add(_sierpinskiButton);
        side.Controls.Add(_logScale);
        side.Controls.Add(new Label { Text = " ", AutoSize = true });
        side.Controls.Add(_saveButton);
        side.Controls.Add(_statusLabel);

        Controls.Add(gridSplit);
        Controls.Add(side);

        _hanoiButton.Click += async (_, _) => await RunAsync(progress => BenchmarkRunner.RunHanoi(progress: progress));
        _queensButton.Click += async (_, _) => await RunAsync(progress => BenchmarkRunner.RunNQueens(progress: progress));
        _quicksortButton.Click += async (_, _) => await RunAsync(progress => BenchmarkRunner.RunQuickSort(progress: progress));
        _sierpinskiButton.Click += async (_, _) => await RunAsync(progress => BenchmarkRunner.RunSierpinski(progress: progress));
        _logScale.CheckedChanged += (_, _) => _chart.Invalidate();
        _saveButton.Click += (_, _) => Save();
        _chart.Paint += (_, e) =>
        {
            if (_series is not null)
                ChartPainter.Draw(e.Graphics, new RectangleF(0, 0, _chart.ClientSize.Width, _chart.ClientSize.Height), _series, _logScale.Checked);
        };

        Theme.Apply(this);
        StyleGrid();
        _chart.BackColor = Theme.Background;
        _statusLabel.ForeColor = Theme.Subtle;
    }

    private void StyleGrid()
    {
        _grid.EnableHeadersVisualStyles = false;
        _grid.BackgroundColor = Theme.Surface;
        _grid.BorderStyle = BorderStyle.None;
        _grid.GridColor = Theme.Border;
        _grid.RowHeadersVisible = false;

        _grid.ColumnHeadersDefaultCellStyle.BackColor = Theme.SurfaceAlt;
        _grid.ColumnHeadersDefaultCellStyle.ForeColor = Theme.Text;
        _grid.ColumnHeadersDefaultCellStyle.SelectionBackColor = Theme.SurfaceAlt;
        _grid.ColumnHeadersDefaultCellStyle.Padding = new Padding(6, 6, 6, 6);
        _grid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        _grid.ColumnHeadersHeight = 38;

        _grid.DefaultCellStyle.BackColor = Theme.Surface;
        _grid.DefaultCellStyle.ForeColor = Theme.Text;
        _grid.DefaultCellStyle.SelectionBackColor = Theme.AccentHover;
        _grid.DefaultCellStyle.SelectionForeColor = Color.White;
        _grid.DefaultCellStyle.Padding = new Padding(4, 2, 4, 2);
        _grid.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(33, 36, 47);
        _grid.RowTemplate.Height = 30;
    }

    private async Task RunAsync(Func<Action<int>?, BenchmarkSeries> run)
    {
        SetBusy(true);
        var progress = new Progress<int>(n => _statusLabel.Text = $"Выполняется замер: n = {n}...");
        try
        {
            _series = await Task.Run(() => run(n => ((IProgress<int>)progress).Report(n)));
            FillGrid(_series);
            _statusLabel.Text = $"Готово: {_series.Points.Count} точек. Можно сохранить CSV + PNG.";
            _saveButton.Enabled = true;
            _chart.Invalidate();
        }
        catch (Exception ex)
        {
            _statusLabel.Text = "Ошибка: " + ex.Message;
        }
        finally
        {
            SetBusy(false);
        }
    }

    private void SetBusy(bool busy)
    {
        _hanoiButton.Enabled = _queensButton.Enabled = _quicksortButton.Enabled = _sierpinskiButton.Enabled = !busy;
        if (busy) _saveButton.Enabled = false;
    }

    private void FillGrid(BenchmarkSeries series)
    {
        _grid.Columns.Clear();
        _grid.Columns.Add("n", "n (размер)");
        _grid.Columns.Add("best", "лучшее время, с");
        _grid.Columns.Add("avg", "среднее время, с");
        _grid.Columns.Add("work", series.WorkSizeTitle.Replace('_', ' '));
        _grid.Rows.Clear();
        foreach (var p in series.Points)
            _grid.Rows.Add(p.N, $"{p.BestMs / 1000.0:0.######}", $"{p.AvgMs / 1000.0:0.######}", p.WorkSize.ToString("N0", System.Globalization.CultureInfo.GetCultureInfo("ru-RU")));
    }

    private void Save()
    {
        if (_series is null) return;
        using var fbd = new FolderBrowserDialog { Description = "Куда сохранить результаты?" };
        if (fbd.ShowDialog(FindForm()) != DialogResult.OK) return;

        string key = _series.Title.StartsWith("Ханой") ? "hanoi"
                   : _series.Title.StartsWith("Задача") ? "nqueens"
                   : _series.Title.StartsWith("Быстрая") ? "quicksort"
                   : "sierpinski";
        string csv = Path.Combine(fbd.SelectedPath, key + ".csv");
        string png = Path.Combine(fbd.SelectedPath, key + ".png");
        _series.SaveCsv(csv);
        ChartPainter.SavePng(_series, png, _logScale.Checked);
        _statusLabel.Text = $"Сохранено: {csv} и {png}";
    }
}
