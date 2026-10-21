using System.Diagnostics;
using RecursionLab2.Core.Sierpinski;

namespace RecursionLab2.App.Demos;

/// <summary>Демонстрация «Треугольник Серпинского»: рекурсивное деление с регулировкой глубины.</summary>
public sealed class SierpinskiDemoControl : UserControl
{
    private readonly TrackBar _depth = new() { Minimum = 0, Maximum = 9, Value = 5, TickFrequency = 1, Width = 230 };
    private readonly CheckBox _animate = new() { Text = "Анимация глубины", AutoSize = true };
    private readonly Label _infoLabel = new() { AutoSize = true };
    private readonly Label _timeLabel = new() { AutoSize = true };
    private readonly Button _saveButton = new() { Text = "Сохранить PNG", Width = 150 };
    private readonly BufferedPanel _canvas = new() { Dock = DockStyle.Fill, BackColor = Color.FromArgb(21, 23, 31) };
    private readonly System.Windows.Forms.Timer _timer = new() { Interval = 700 };

    private List<TriangleF> _triangles = [];

    public SierpinskiDemoControl()
    {
        var side = new FlowLayoutPanel
        {
            Dock = DockStyle.Left,
            Width = 290,
            FlowDirection = FlowDirection.TopDown,
            WrapContents = false,
            Padding = new Padding(12),
        };

        side.Controls.Add(new Label { Text = "Глубина рекурсии:", AutoSize = true, Margin = new Padding(0, 8, 0, 0) });
        side.Controls.Add(_depth);
        side.Controls.Add(_animate);
        side.Controls.Add(_saveButton);
        side.Controls.Add(_infoLabel);
        side.Controls.Add(_timeLabel);

        Controls.Add(_canvas);
        Controls.Add(side);

        _canvas.Paint += DrawTriangles;
        _depth.ValueChanged += (_, _) => Rebuild();
        _animate.CheckedChanged += (_, _) =>
        {
            if (_animate.Checked) _timer.Start(); else _timer.Stop();
        };
        _timer.Tick += (_, _) =>
        {
            _depth.Value = _depth.Value >= _depth.Maximum ? 0 : _depth.Value + 1;
        };
        _saveButton.Click += (_, _) => SavePng();

        Theme.Apply(this);
        _canvas.BackColor = Theme.Background;
        _infoLabel.ForeColor = Theme.Subtle;
        _timeLabel.ForeColor = Theme.Accent;

        Rebuild();
    }

    private void Rebuild()
    {
        int d = _depth.Value;
        var watch = Stopwatch.StartNew();
        _triangles = SierpinskiTriangle.Build(0, 0, 800, 693, 400, 0, d);
        watch.Stop();

        _infoLabel.Text = $"Глубина: {d}\nТреугольников: 3^{d} = {_triangles.Count}";
        _timeLabel.Text = $"Построение: {watch.Elapsed.TotalMilliseconds:0.###} мс";
        _canvas.Invalidate();
    }

    private void DrawTriangles(object? sender, PaintEventArgs e)
    {
        var g = e.Graphics;
        g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        int w = _canvas.ClientSize.Width, h = _canvas.ClientSize.Height;
        if (w <= 0 || h <= 0) return;

        // масштабируем исходный треугольник (0..800, 0..693) в клиентскую область
        float scale = Math.Min((w - 40) / 800f, (h - 60) / 693f);
        float ox = (w - 800 * scale) / 2, oy = 20 + (h - 40 - 693 * scale) / 2;

        foreach (var t in _triangles)
        {
            // цвет зависит от вертикального положения — так видна самоподобность
            float rel = t.Ay / 693f;
            using var brush = new SolidBrush(Lerp(
                Color.FromArgb(105, 145, 255), Color.FromArgb(95, 218, 165), rel));
            g.FillTriangle(brush,
                ox + t.Ax * scale, oy + t.Ay * scale,
                ox + t.Bx * scale, oy + t.By * scale,
                ox + t.Cx * scale, oy + t.Cy * scale);
        }

        TextRenderer.DrawText(g,
            $"Треугольник Серпинского: каждый треугольник делится на 4, центральный выбрасывается (глубина {_depth.Value})",
            Font, new Point(12, h - 24), Theme.Subtle);
    }

    private static Color Lerp(Color a, Color b, float t)
    {
        t = Math.Clamp(t, 0f, 1f);
        return Color.FromArgb(
            (int)(a.R + (b.R - a.R) * t),
            (int)(a.G + (b.G - a.G) * t),
            (int)(a.B + (b.B - a.B) * t));
    }

    private void SavePng()
    {
        using var sfd = new SaveFileDialog { Filter = "PNG|*.png", FileName = $"sierpinski_depth{_depth.Value}.png" };
        if (sfd.ShowDialog(FindForm()) != DialogResult.OK) return;

        using var bmp = new Bitmap(_canvas.ClientSize.Width, _canvas.ClientSize.Height);
        _canvas.DrawToBitmap(bmp, new Rectangle(Point.Empty, bmp.Size));
        bmp.Save(sfd.FileName, System.Drawing.Imaging.ImageFormat.Png);
    }
}

internal static class GraphicsExtensions
{
    public static void FillTriangle(this Graphics g, Brush brush, float ax, float ay, float bx, float by, float cx, float cy)
    {
        using var path = new System.Drawing.Drawing2D.GraphicsPath();
        path.AddPolygon(new[] { new PointF(ax, ay), new PointF(bx, by), new PointF(cx, cy) });
        g.FillPath(brush, path);
    }
}
