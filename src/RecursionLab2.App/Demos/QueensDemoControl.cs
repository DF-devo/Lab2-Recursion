using System.Collections.Concurrent;
using RecursionLab2.Core.NQueens;

namespace RecursionLab2.App.Demos;

/// <summary>
/// Демонстрация «Задача о N ферзях»: пошаговая анимация рекурсивного бэктрекинга —
/// видно, как ферзи ставятся, отбраковываются и снимаются при откате.
/// Найденные решения накапливаются, и после завершения поиска их можно пролистать.
/// </summary>
public sealed class QueensDemoControl : UserControl
{
    private readonly NumericUpDown _boardSize = new() { Minimum = 4, Maximum = 12, Value = 8, Width = 80 };
    private readonly Button _startButton = new() { Text = "Пуск", Width = 90 };
    private readonly Button _pauseButton = new() { Text = "Пауза", Width = 90, Enabled = false };
    private readonly Button _stepButton = new() { Text = "Шаг", Width = 90, Enabled = false };
    private readonly Button _resetButton = new() { Text = "Сброс", Width = 90 };
    private readonly Button _prevSolutionButton = new() { Text = "◀ Пред. решение", Width = 140, Enabled = false };
    private readonly Button _nextSolutionButton = new() { Text = "След. решение ▶", Width = 140, Enabled = false };
    private readonly TrackBar _speed = new() { Minimum = 1, Maximum = 400, Value = 30, Width = 230 };
    private readonly CheckBox _findAll = new() { Text = "Искать все решения", AutoSize = true };
    private readonly Label _attemptsLabel = new() { AutoSize = true };
    private readonly Label _backtracksLabel = new() { AutoSize = true };
    private readonly Label _solutionsLabel = new() { AutoSize = true };
    private readonly Label _statusLabel = new() { AutoSize = true };
    private readonly BufferedPanel _board = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(21, 23, 31) };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 30 };

    private BlockingCollection<QueenStep>? _queue;
    private CancellationTokenSource? _cts;
    private Task? _worker;
    private volatile bool _workerDone;

    private int[] _queens = [];       // _queens[row] = столбец или −1 (текущее состояние анимации)
    private readonly List<int[]> _found = []; // все найденные решения
    private int _viewIndex = -1;      // −1 — «живой» режим анимации; иначе просмотр решения _viewIndex
    private int _n = 8;
    private QueenStep _lastStep;
    private long _attempts, _backtracks;

    public QueensDemoControl()
    {
        var side = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 300,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12),
        };
        _board.Paint += DrawBoard;

        var navRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        navRow.Controls.Add(_prevSolutionButton);
        navRow.Controls.Add(_nextSolutionButton);

        side.Controls.Add(new Label { Text = "Размер доски (N):", AutoSize = true, Margin = new Padding(0, 8, 0, 0) });
        side.Controls.Add(_boardSize);
        side.Controls.Add(_startButton);
        side.Controls.Add(_pauseButton);
        side.Controls.Add(_stepButton);
        side.Controls.Add(_resetButton);
        side.Controls.Add(new Label { Text = "Скорость (шагов за тик):", AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
        side.Controls.Add(_speed);
        side.Controls.Add(_findAll);
        side.Controls.Add(new Label { Text = "Просмотр найденных решений:", AutoSize = true, Margin = new Padding(0, 14, 0, 0) });
        side.Controls.Add(navRow);
        side.Controls.Add(_attemptsLabel);
        side.Controls.Add(_backtracksLabel);
        side.Controls.Add(_solutionsLabel);
        side.Controls.Add(_statusLabel);

        Controls.Add(_board);
        Controls.Add(side);

        _startButton.Click += (_, _) => Start(animated: true);
        _pauseButton.Click += (_, _) => TogglePause();
        _stepButton.Click += (_, _) => ConsumeSteps(1);
        _resetButton.Click += (_, _) => ResetState();
        _prevSolutionButton.Click += (_, _) => ShowSolution(_viewIndex < 0 ? _found.Count - 1 : _viewIndex - 1);
        _nextSolutionButton.Click += (_, _) => ShowSolution(_viewIndex < 0 ? 0 : _viewIndex + 1);
        _boardSize.ValueChanged += (_, _) => ResetState();
        _timer.Tick += (_, _) => ConsumeSteps(_speed.Value);

        Theme.Apply(this);
        _board.BackColor = Theme.Background;
        _statusLabel.ForeColor = Theme.Subtle;
        ResetState();
    }

    private void Start(bool animated)
    {
        ResetState();
        _n = (int)_boardSize.Value;
        _queens = new int[_n];
        Array.Fill(_queens, -1);
        _attempts = _backtracks = 0;

        _cts = new CancellationTokenSource();
        _queue = new BlockingCollection<QueenStep>(20000);
        _workerDone = false;
        bool findFirstOnly = !_findAll.Checked;

        var solver = new NQueensSolver();
        solver.Step += step => _queue.Add(step, _cts.Token);

        _worker = Task.Run(() =>
        {
            try
            {
                if (findFirstOnly) solver.SolveFirst(_n);
                else solver.CountSolutions(_n, _cts.Token);
            }
            catch (OperationCanceledException) { /* сброс */ }
            finally
            {
                _workerDone = true;
                _queue.CompleteAdding(); // очередь закрыта — потребитель поймёт, что поиск закончен
            }
        }, _cts.Token);

        _startButton.Enabled = false;
        _pauseButton.Enabled = true;
        _stepButton.Enabled = true;
        _statusLabel.Text = "Идёт поиск...";
        if (animated) _timer.Start();
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
            _viewIndex = -1; // возврат из просмотра решений к «живой» анимации
            _timer.Start();
            _pauseButton.Text = "Пауза";
            _statusLabel.Text = _workerDone ? "Досматриваем шаги..." : "Идёт поиск...";
        }
        _board.Invalidate();
    }

    /// <summary>Показать решение с индексом index (с автоматической паузой).</summary>
    private void ShowSolution(int index)
    {
        if (_found.Count == 0) return;
        PauseIfRunning();
        _viewIndex = Math.Clamp(index, 0, _found.Count - 1);
        _statusLabel.Text = $"Просмотр решения {_viewIndex + 1} из {_found.Count}  (пауза)";
        _board.Invalidate();
    }

    private void PauseIfRunning()
    {
        if (!_timer.Enabled) return;
        _timer.Stop();
        _pauseButton.Text = "Продолжить";
    }

    /// <summary>Забирает из очереди не более count шагов и применяет их к доске.</summary>
    private void ConsumeSteps(int count)
    {
        if (_queue is null) return;

        bool finished = false;
        for (int i = 0; i < count; i++)
        {
            if (!_queue.TryTake(out var step, TimeSpan.Zero))
            {
                if (_workerDone) finished = true; // очередь пуста и поиск завершён
                break;
            }

            _lastStep = step;
            switch (step.Action)
            {
                case QueenStepAction.Place:
                    _queens[step.Row] = step.Col;
                    _attempts++;
                    break;
                case QueenStepAction.Remove:
                    _queens[step.Row] = -1;
                    _backtracks++;
                    break;
                case QueenStepAction.Solution:
                    _found.Add((int[])_queens.Clone()); // сейчас все ферзи стоят — это и есть решение
                    break;
            }
        }

        _attemptsLabel.Text = $"Поставлено ферзей (попыток): {_attempts}";
        _backtracksLabel.Text = $"Откатов: {_backtracks}";
        _solutionsLabel.Text = $"Найдено решений: {_found.Count}";

        if (finished)
        {
            // авто-пауза: поиск завершён, таймер останавливается сам
            _timer.Stop();
            _pauseButton.Enabled = false;
            _pauseButton.Text = "Пауза";
            _stepButton.Enabled = false;
            _startButton.Enabled = true;
            if (_found.Count > 0)
            {
                _viewIndex = 0; // сразу показать первое решение
                _statusLabel.Text = $"Готово: найдено {_found.Count} реш. — листайте кнопками ниже.";
            }
            else
            {
                _statusLabel.Text = $"Готово: для N = {_n} решений не существует.";
            }
        }

        UpdateNavButtons();
        _board.Invalidate();
    }

    private void UpdateNavButtons()
    {
        bool has = _found.Count > 0;
        _prevSolutionButton.Enabled = has && (_viewIndex > 0 || _viewIndex < 0);
        _nextSolutionButton.Enabled = has && (_viewIndex < _found.Count - 1 || _viewIndex < 0);
    }

    private void ResetState()
    {
        _timer.Stop();
        _cts?.Cancel();
        try { _worker?.Wait(100); } catch (AggregateException) { }
        _cts?.Dispose();
        _cts = null;
        if (_queue is not null)
        {
            try { _queue.CompleteAdding(); } catch (InvalidOperationException) { } // worker мог закрыть её раньше
            while (_queue.TryTake(out _)) { }
            _queue.Dispose();
            _queue = null;
        }

        _n = (int)_boardSize.Value;
        _queens = new int[_n];
        Array.Fill(_queens, -1);
        _found.Clear();
        _viewIndex = -1;
        _lastStep = default;
        _workerDone = false;
        _attempts = _backtracks = 0;

        _startButton.Enabled = true;
        _pauseButton.Enabled = false;
        _pauseButton.Text = "Пауза";
        _stepButton.Enabled = false;
        _statusLabel.Text = "Нажмите «Пуск» — алгоритм перебирает строки сверху вниз,\nснимая ферзей и откатываясь при тупике (бэктрекинг).";
        _attemptsLabel.Text = "Поставлено ферзей (попыток): 0";
        _backtracksLabel.Text = "Откатов: 0";
        _solutionsLabel.Text = "Найдено решений: 0";
        UpdateNavButtons();
        _board.Invalidate();
    }

    private void DrawBoard(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        int w = _board.ClientSize.Width, h = _board.ClientSize.Height;
        int size = Math.Min(w, h) - 40;
        if (size <= 0 || _n == 0) return;
        float cell = size / (float)_n;
        float ox = (w - cell * _n) / 2, oy = (h - cell * _n) / 2;

        bool viewMode = _viewIndex >= 0 && _viewIndex < _found.Count;
        int[] shown = viewMode ? _found[_viewIndex] : _queens;

        // подсветка атакованных клеток поставленными ферзями (только в живом режиме)
        bool[,] attacked = new bool[_n, _n];
        if (!viewMode)
        {
            for (int r = 0; r < _n; r++)
            {
                if (_queens[r] < 0) continue;
                int c = _queens[r];
                for (int i = 0; i < _n; i++)
                {
                    attacked[r, i] = true;
                    attacked[i, c] = true;
                }
                for (int d = 1; d < _n; d++)
                {
                    if (r + d < _n && c + d < _n) attacked[r + d, c + d] = true;
                    if (r - d >= 0 && c - d >= 0) attacked[r - d, c - d] = true;
                    if (r + d < _n && c - d >= 0) attacked[r + d, c - d] = true;
                    if (r - d >= 0 && c + d < _n) attacked[r - d, c + d] = true;
                }
            }
        }

        for (int r = 0; r < _n; r++)
        {
            for (int c = 0; c < _n; c++)
            {
                var rect = new RectangleF(ox + c * cell, oy + r * cell, cell, cell);
                bool light = (r + c) % 2 == 0;
                using var brush = new SolidBrush(viewMode
                    ? (light ? Color.FromArgb(58, 63, 80) : Color.FromArgb(40, 45, 58)) // спокойная доска в режиме просмотра
                    : _queens[r] == c
                        ? Color.FromArgb(255, 196, 70)                                 // клетка с ферзём
                        : _lastStep.Action == QueenStepAction.Remove && _lastStep.Row == r && _lastStep.Col == c
                            ? Color.FromArgb(224, 108, 96)                             // только что сняли
                            : attacked[r, c]
                                ? (light ? Color.FromArgb(84, 60, 62) : Color.FromArgb(66, 48, 52)) // под боем
                                : (light ? Color.FromArgb(56, 62, 78) : Color.FromArgb(38, 42, 54)));  // обычная
                g.FillRectangle(brush, rect);
            }
        }

        using var gridPen = new Pen(Theme.Border, 1);
        for (int i = 0; i <= _n; i++)
        {
            g.DrawLine(gridPen, ox + i * cell, oy, ox + i * cell, oy + _n * cell);
            g.DrawLine(gridPen, ox, oy + i * cell, ox + _n * cell, oy + i * cell);
        }

        // ферзи
        using var queenFont = new Font("Segoe UI Symbol", Math.Max(cell * 0.62f, 8));
        using var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center };
        for (int r = 0; r < _n; r++)
        {
            if (shown[r] < 0) continue;
            g.DrawString("♛", queenFont, Brushes.White, ox + shown[r] * cell + cell / 2, oy + r * cell + cell / 2 + cell * 0.04f, sf);
        }

        // строка, обрабатываемая сейчас (только в живом режиме)
        if (!viewMode && _lastStep.Action is QueenStepAction.Place or QueenStepAction.Remove
            && _lastStep.Row < _n && _lastStep.Col >= 0)
        {
            int row = _lastStep.Row;
            using var rowPen = new Pen(Theme.Accent, 2);
            g.DrawRectangle(rowPen, ox, oy + row * cell, cell * _n, cell);
        }

        string header = viewMode
            ? $"Решение {_viewIndex + 1} из {_found.Count}  —  кнопками «Пред./След.» листайте решения, «Продолжить» вернёт к анимации"
            : $"Задача о N ферзях, N = {_n}: расставить {_n} ферзей так, чтобы никто никого не бил";
        TextRenderer.DrawText(g, header, Font, new Point((int)ox, (int)oy - 24), Theme.Subtle);
    }
}
