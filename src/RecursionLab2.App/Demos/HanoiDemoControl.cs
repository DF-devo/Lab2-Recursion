using RecursionLab2.Core.Hanoi;

namespace RecursionLab2.App.Demos;

/// <summary>Демонстрация «Ханойские башни»: анимация переноса дисков по рекурсивному решению.</summary>
public sealed class HanoiDemoControl : UserControl
{
    private readonly NumericUpDown _diskCount = new() { Minimum = 1, Maximum = 12, Value = 5, Width = 80 };
    private readonly Button _startButton = new() { Text = "Пуск", Width = 90 };
    private readonly Button _pauseButton = new() { Text = "Пауза", Width = 90, Enabled = false };
    private readonly Button _resetButton = new() { Text = "Сброс", Width = 90 };
    private readonly TrackBar _speed = new() { Minimum = 10, Maximum = 400, Value = 90, Width = 230 };
    private readonly Label _totalLabel = new() { AutoSize = true };
    private readonly Label _doneLabel = new() { AutoSize = true };
    private readonly Label _currentLabel = new() { AutoSize = true };
    private readonly TextBox _log = new()
    {
        Width = 250,
        Height = 160,
        Multiline = true,
        ReadOnly = true,
        ScrollBars = ScrollBars.Vertical,
        Anchor = AnchorStyles.Top | AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right,
        BackColor = Color.FromArgb(29, 32, 42),
        ForeColor = Color.FromArgb(233, 236, 244),
    };
    private readonly BufferedPanel _board = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(21, 23, 31) };
    private readonly System.Windows.Forms.Timer _timer = new();

    private List<HanoiMove> _moves = [];
    private List<int>[] _pegs = [[], [], []];
    private int _moveIndex;

    public HanoiDemoControl()
    {
        var side = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 290,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12),
        };
        _board.Paint += DrawBoard;

        _totalLabel.Text = "Всего переносов: —";
        _doneLabel.Text = "Выполнено: 0";
        _currentLabel.Text = "Текущий ход: —";

        side.Controls.Add(new Label { Text = "Число дисков (n):", AutoSize = true, Margin = new Padding(0, 8, 0, 0) });
        var diskRow = new FlowLayoutPanel { AutoSize = true, WrapContents = false };
        diskRow.Controls.Add(_diskCount);
        diskRow.Controls.Add(new Label { Text = "  2ⁿ−1 = ходов", AutoSize = true, Margin = new Padding(4, 8, 0, 0) });
        side.Controls.Add(diskRow);
        side.Controls.Add(_startButton);
        side.Controls.Add(_pauseButton);
        side.Controls.Add(_resetButton);
        side.Controls.Add(new Label { Text = "Задержка между ходами, мс:", AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
        side.Controls.Add(_speed);
        side.Controls.Add(_totalLabel);
        side.Controls.Add(_doneLabel);
        side.Controls.Add(_currentLabel);
        side.Controls.Add(new Label { Text = "Журнал переносов:", AutoSize = true, Margin = new Padding(0, 10, 0, 0) });
        side.Controls.Add(_log);

        Controls.Add(_board);
        Controls.Add(side);

        _startButton.Click += (_, _) => Start();
        _pauseButton.Click += (_, _) => TogglePause();
        _resetButton.Click += (_, _) => Reset();
        _diskCount.ValueChanged += (_, _) => Reset();
        _timer.Tick += (_, _) => Tick();
        _speed.ValueChanged += (_, _) => _timer.Interval = _speed.Value;

        Theme.Apply(this);
        _board.BackColor = Theme.Background;
        _log.BorderStyle = BorderStyle.None;
        _log.BackColor = Theme.Surface;
        _log.ForeColor = Theme.Text;
        _totalLabel.ForeColor = Theme.Subtle;
        _doneLabel.ForeColor = Theme.Subtle;
        _currentLabel.ForeColor = Theme.Accent;
        foreach (var l in side.Controls.OfType<Label>()) l.ForeColor = Theme.Subtle;
        _currentLabel.ForeColor = Theme.Accent;

        Reset();
    }

    private void Start()
    {
        Reset();
        _timer.Interval = _speed.Value;
        _timer.Start();
        _startButton.Enabled = false;
        _pauseButton.Enabled = true;
    }

    private void TogglePause()
    {
        if (_timer.Enabled)
        {
            _timer.Stop();
            _pauseButton.Text = "Продолжить";
        }
        else
        {
            _timer.Start();
            _pauseButton.Text = "Пауза";
        }
    }

    private void Reset()
    {
        _timer.Stop();
        _pauseButton.Text = "Пауза";
        _pauseButton.Enabled = false;
        _startButton.Enabled = true;

        int n = (int)_diskCount.Value;
        _moves = HanoiSolver.Solve(n);
        _moveIndex = 0;
        _pegs = [[], [], []];
        for (int d = n; d >= 1; d--) _pegs[0].Add(d); // на первом стержне диски 1..n (n снизу)

        _totalLabel.Text = $"Всего переносов: 2^{n}−1 = {_moves.Count}";
        _doneLabel.Text = "Выполнено: 0";
        _currentLabel.Text = "Текущий ход: —";
        _log.Clear();
        _board.Invalidate();
    }

    private void Tick()
    {
        if (_moveIndex >= _moves.Count)
        {
            _timer.Stop(); // авто-пауза по завершении
            _pauseButton.Enabled = false;
            _pauseButton.Text = "Пауза";
            _startButton.Enabled = true;
            _currentLabel.Text = "Готово: башня перенесена!";
            return;
        }

        var m = _moves[_moveIndex++];
        _pegs[m.FromPeg].Remove(m.Disk);
        _pegs[m.ToPeg].Add(m.Disk);

        _doneLabel.Text = $"Выполнено: {_moveIndex} из {_moves.Count}";
        _currentLabel.Text = $"Текущий ход: {m}";
        _log.Text = $"{_moveIndex,4}. {m}{Environment.NewLine}" + _log.Text; // новейший сверху
        var lines = _log.Lines;
        if (lines.Length > 200)
            _log.Text = string.Join(Environment.NewLine, lines.Take(200));

        _board.Invalidate();
    }

    private void DrawBoard(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        int w = _board.ClientSize.Width, h = _board.ClientSize.Height;

        float baseY = h - 40;
        float pegSpacing = w / 3f;
        float maxDiskW = Math.Min(pegSpacing - 30, 240);
        float diskH = Math.Min(24, (baseY - 60) / Math.Max((int)_diskCount.Value, 1));
        int n = (int)_diskCount.Value;

        using (var basePen = new Pen(Theme.AccentHover, 6))
            g.DrawLine(basePen, 10, baseY, w - 10, baseY);

        using var rodPen = new Pen(Theme.Border, 4);
        using var pegFont = new Font(Font, FontStyle.Regular);
        for (int peg = 0; peg < 3; peg++)
        {
            float cx = pegSpacing * peg + pegSpacing / 2;
            g.DrawLine(rodPen, cx, baseY, cx, baseY - 40 - n * diskH);
            TextRenderer.DrawText(g, $"стержень {peg + 1}", pegFont, new Point((int)cx - 34, (int)baseY + 8), Theme.Subtle);

            var disks = _pegs[peg];
            for (int i = 0; i < disks.Count; i++)
            {
                int d = disks[i];
                float dw = maxDiskW * d / Math.Max(n, 1) + 18;
                float y = baseY - (i + 1) * diskH;
                using var brush = new SolidBrush(DiskColor(d, n));
                g.FillRectangle(brush, cx - dw / 2, y, dw, diskH - 2);
                using var highlight = new Pen(Color.FromArgb(60, 255, 255, 255), 1);
                g.DrawRectangle(highlight, cx - dw / 2, y, dw, diskH - 2);
                if (diskH >= 12)
                    TextRenderer.DrawText(g, d.ToString(), pegFont, new Point((int)cx - 5, (int)(y + diskH / 2 - 9)), Color.White);
            }
        }

        TextRenderer.DrawText(g, "Перенести всю башню со стержня 1 на стержень 3 (нельзя класть больший диск на меньший)",
            pegFont, new Point(12, 12), Theme.Subtle);
    }

    private static Color DiskColor(int disk, int n)
    {
        float hue = 200f * (disk - 1) / Math.Max(n, 1); // от голубого к красному
        return ColorFromHsl(hue, 0.7f, 0.56f);
    }

    private static Color ColorFromHsl(float h, float s, float l)
    {
        float c = (1 - Math.Abs(2 * l - 1)) * s;
        float x = c * (1 - Math.Abs((h / 60f) % 2 - 1));
        float m = l - c / 2;
        (float r, float g, float b) = h switch
        {
            < 60 => (c, x, 0f),
            < 120 => (x, c, 0f),
            < 180 => (0f, c, x),
            < 240 => (0f, x, c),
            < 300 => (x, 0f, c),
            _ => (c, 0f, x),
        };
        return Color.FromArgb((int)((r + m) * 255), (int)((g + m) * 255), (int)((b + m) * 255));
    }
}
