using System.Collections.Concurrent;
using RecursionLab2.Core.QuickSort;

namespace RecursionLab2.App.Demos;

/// <summary>
/// Демонстрация «Быстрая сортировка»: массив рисуется столбиками, анимация
/// показывает выбор опорного элемента, сравнения, обмены и постановку опорного
/// на финальное место.
/// </summary>
public sealed class QuickSortDemoControl : UserControl
{
    private readonly NumericUpDown _size = new() { Minimum = 8, Maximum = 400, Value = 60, Width = 80 };
    private readonly Button _startButton = new() { Text = "Запуск", Width = 90 };
    private readonly Button _pauseButton = new() { Text = "Пауза", Width = 90, Enabled = false };
    private readonly Button _newArrayButton = new() { Text = "Новый массив", Width = 130 };
    private readonly TrackBar _speed = new() { Minimum = 1, Maximum = 300, Value = 12, Width = 230 };
    private readonly Label _comparisonsLabel = new() { AutoSize = true };
    private readonly Label _swapsLabel = new() { AutoSize = true };
    private readonly Label _statusLabel = new() { AutoSize = true };
    private readonly BufferedPanel _board = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(21, 23, 31) };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 20 };

    private int[] _array = [];
    private BlockingCollection<SortStep>? _queue;
    private CancellationTokenSource? _cts;
    private Task? _worker;
    private volatile bool _workerDone;
    private SortStep _lastStep;
    private int _pivotIndex = -1;
    private long _comparisons, _swaps;

    public QuickSortDemoControl()
    {
        var side = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 290,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12),
        };
        _board.Paint += DrawBars;

        side.Controls.Add(new Label { Text = "Число элементов (n):", AutoSize = true, Margin = new Padding(0, 8, 0, 0) });
        side.Controls.Add(_size);
        side.Controls.Add(_startButton);
        side.Controls.Add(_pauseButton);
        side.Controls.Add(_newArrayButton);
        side.Controls.Add(new Label { Text = "Скорость (шагов за тик):", AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
        side.Controls.Add(_speed);
        side.Controls.Add(_comparisonsLabel);
        side.Controls.Add(_swapsLabel);
        side.Controls.Add(_statusLabel);

        Controls.Add(_board);
        Controls.Add(side);

        _startButton.Click += (_, _) => Start();
        _pauseButton.Click += (_, _) => TogglePause();
        _newArrayButton.Click += (_, _) => Reset();
        _size.ValueChanged += (_, _) => Reset();
        _timer.Tick += (_, _) => ConsumeSteps(_speed.Value);

        Theme.Apply(this);
        _board.BackColor = Theme.Background;
        _comparisonsLabel.ForeColor = Theme.Subtle;
        _swapsLabel.ForeColor = Theme.Subtle;
        _statusLabel.ForeColor = Theme.Subtle;

        Reset();
    }

    private void Start()
    {
        Reset();
        _cts = new CancellationTokenSource();
        _queue = new BlockingCollection<SortStep>(100000);
        _workerDone = false;
        _comparisons = _swaps = 0;

        var sorter = new QuickSorter();
        sorter.Step += step => { try { _queue.Add(step, _cts.Token); } catch (OperationCanceledException) { } };

        _worker = Task.Run(() =>
        {
            try { sorter.Sort(_array); }
            catch (OperationCanceledException) { }
            finally
            {
                _workerDone = true;
                _queue.CompleteAdding();
            }
        }, _cts.Token);

        _startButton.Enabled = false;
        _pauseButton.Enabled = true;
        _statusLabel.Text = "Идёт сортировка...";
        _timer.Start();
    }

    private void TogglePause()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
            _pauseButton.Text = "Продолжить";
            _statusLabel.Text = "Пауза.";
        }
        else
        {
            _timer.Start();
            _pauseButton.Text = "Пауза";
            _statusLabel.Text = _workerDone ? "Досматриваем шаги..." : "Идёт сортировка...";
        }
    }

    private void ConsumeSteps(int count)
    {
        if (_queue is null) return;

        bool finished = false;
        for (int i = 0; i < count; i++)
        {
            if (!_queue.TryTake(out var step, TimeSpan.Zero))
            {
                if (_workerDone) finished = true;
                break;
            }

            _lastStep = step;
            switch (step.Action)
            {
                case SortAction.Compare:
                    _comparisons++;
                    break;
                case SortAction.Swap:
                    _swaps++;
                    break;
                case SortAction.Pivot:
                    _pivotIndex = step.I;
                    break;
                case SortAction.Fixed:
                    _pivotIndex = -1;
                    break;
            }
        }

        _comparisonsLabel.Text = $"Сравнений: {_comparisons}";
        _swapsLabel.Text = $"Обменов: {_swaps}";

        if (finished)
        {
            _timer.Stop(); // авто-пауза по завершении
            _pauseButton.Enabled = false;
            _pauseButton.Text = "Пауза";
            _startButton.Enabled = true;
            _pivotIndex = -1;
            _statusLabel.Text = "Готово: массив отсортирован!";
        }

        _board.Invalidate();
    }

    private void Reset()
    {
        _timer.Stop();
        _cts?.Cancel();
        try { _worker?.Wait(100); } catch (AggregateException) { }
        _cts?.Dispose();
        _cts = null;
        if (_queue is not null)
        {
            try { _queue.CompleteAdding(); } catch (InvalidOperationException) { }
            while (_queue.TryTake(out _)) { }
            _queue.Dispose();
            _queue = null;
        }

        int n = (int)_size.Value;
        var rnd = new Random(Environment.TickCount);
        _array = new int[n];
        for (int i = 0; i < n; i++) _array[i] = rnd.Next(100, 1000);

        _lastStep = default;
        _pivotIndex = -1;
        _comparisons = _swaps = 0;
        _workerDone = false;

        _startButton.Enabled = true;
        _pauseButton.Enabled = false;
        _pauseButton.Text = "Пауза";
        _statusLabel.Text = "Нажмите «Запуск»: опорный элемент (зелёный) делит массив,\nменьшие уходят влево, большие — вправо, затем рекурсия в обеих частях.";
        _comparisonsLabel.Text = "Сравнений: 0";
        _swapsLabel.Text = "Обменов: 0";
        _board.Invalidate();
    }

    private void DrawBars(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        int w = _board.ClientSize.Width, h = _board.ClientSize.Height;
        int n = _array.Length;
        if (n == 0 || w <= 0) return;

        float barWidth = (float)w / n;
        int max = Math.Max(_array.Max(), 1);
        float plotH = h - 60;

        for (int i = 0; i < n; i++)
        {
            float barH = _array[i] / (float)max * plotH;
            var rect = new RectangleF(i * barWidth, h - 30 - barH, Math.Max(barWidth - 1, 1), barH);

            Color color;
            if (_lastStep.Action == SortAction.Compare && (_lastStep.I == i || _lastStep.J == i))
                color = Color.FromArgb(255, 166, 87);          // сравниваемые — оранжевые
            else if (i == _pivotIndex)
                color = Color.FromArgb(92, 204, 150);          // опорный — зелёный
            else if (_lastStep.Action == SortAction.Fixed && _lastStep.I == i)
                color = Color.FromArgb(92, 204, 150);          // встал на место — зелёный
            else
                color = Color.FromArgb(101, 143, 255);         // остальные — синие

            using var brush = new SolidBrush(color);
            g.FillRectangle(brush, rect);
        }

        TextRenderer.DrawText(g,
            $"Быстрая сортировка, n = {n}: зелёный — опорный элемент, оранжевый — текущее сравнение",
            Font, new Point(12, 10), Theme.Subtle);
        TextRenderer.DrawText(g, "значения", Font, new Point(12, h - 24), Theme.Subtle);
    }
}
