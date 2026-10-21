using System.Drawing;
using System.Drawing.Drawing2D;
using RecursionLab2.Core.Benchmark;

namespace RecursionLab2.Core.Charting;

/// <summary>
/// Простой построитель графиков времени от размера задачи (рисует System.Drawing).
/// Время откладывается в секундах; оформление — тёмная тема, как в GUI.
/// Используется и в GUI-стенде, и для экспорта PNG из консольного стенда.
/// </summary>
public static class ChartPainter
{
    private static readonly Font TitleFont = new("Segoe UI Semibold", 13.5f);
    private static readonly Font AxisFont = new("Segoe UI", 9.5f);
    private static readonly Font SmallFont = new("Segoe UI", 8.5f);

    private static readonly Color Bg = Color.FromArgb(21, 23, 31);
    private static readonly Color Plot = Color.FromArgb(28, 31, 41);
    private static readonly Color Grid = Color.FromArgb(45, 50, 64);
    private static readonly Color Axis = Color.FromArgb(88, 95, 116);
    private static readonly Color TextColor = Color.FromArgb(233, 236, 244);
    private static readonly Color SubtleColor = Color.FromArgb(150, 156, 172);
    private static readonly Color LineColor = Color.FromArgb(101, 143, 255);
    private static readonly Color DotColor = Color.FromArgb(255, 166, 87);

    /// <summary>Рисует серию замеров на готовой графике.</summary>
    public static void Draw(Graphics g, RectangleF bounds, BenchmarkSeries series, bool logScaleY)
    {
        g.SmoothingMode = SmoothingMode.AntiAlias;
        g.Clear(Bg);

        if (series.Points.Count == 0) return;

        string title = series.Title + (logScaleY ? "  (лог. шкала)" : "");
        using (var sf = new StringFormat { Alignment = StringAlignment.Center })
            g.DrawString(title, TitleFont, new SolidBrush(TextColor), bounds.Width / 2 + bounds.Left, bounds.Top + 6, sf);

        // поле под заголовок и подписи осей
        float left = bounds.Left + 70, right = bounds.Right - 18,
              top = bounds.Top + 42, bottom = bounds.Bottom - 46;

        // всё в секундах
        double Sec(double ms) => ms / 1000.0;

        double maxY = series.Points.Max(p => logScaleY ? Math.Log10(Math.Max(Sec(p.AvgMs), 1e-7)) : Sec(p.AvgMs));
        double minY = 0;
        if (logScaleY) minY = Math.Log10(Math.Max(Sec(series.Points.Min(p => p.AvgMs)), 1e-7)) - 0.2;
        if (maxY <= minY) maxY = minY + 1;
        int maxX = series.Points.Max(p => p.N);
        int minX = series.Points.Min(p => p.N);

        double X(int n) => left + (double)(n - minX) / Math.Max(maxX - minX, 1) * (right - left);
        double Y(double sec)
        {
            double v = logScaleY ? Math.Log10(Math.Max(sec, 1e-7)) : sec;
            return bottom - (v - minY) / (maxY - minY) * (bottom - top);
        }

        // область графика
        using (var plotBrush = new SolidBrush(Plot))
            g.FillRectangle(plotBrush, left, top - 6, right - left, bottom - top + 6);

        // оси
        using var axisPen = new Pen(Axis, 1.2f);
        g.DrawLine(axisPen, left, top - 6, left, bottom);
        g.DrawLine(axisPen, left, bottom, right, bottom);

        // горизонтальная сетка и подписи Y
        int yTicks = 6;
        for (int i = 0; i <= yTicks; i++)
        {
            double v = minY + (maxY - minY) * i / yTicks;
            float y = (float)(bottom - (v - minY) / (maxY - minY) * (bottom - top));
            using (var gridPen = new Pen(Grid, 1f) { DashStyle = DashStyle.Dot })
                g.DrawLine(gridPen, left, y, right, y);
            string label = logScaleY
                ? FormatSeconds(Math.Pow(10, v))   // v = log10(секунды)
                : FormatSeconds(v);                // v — уже секунды
            g.DrawString(label, SmallFont, new SolidBrush(SubtleColor), left - 8, y - 7, new StringFormat { Alignment = StringAlignment.Far });
        }

        // подписи X (прореживаем, если точек много — иначе подписи слипаются)
        int xStep = (int)Math.Ceiling(series.Points.Count / 10.0);
        for (int i = 0; i < series.Points.Count; i += xStep)
        {
            var p = series.Points[i];
            float x = (float)X(p.N);
            g.DrawLine(axisPen, x, bottom, x, bottom + 4);
            g.DrawString(p.N.ToString("N0", System.Globalization.CultureInfo.InvariantCulture), SmallFont, new SolidBrush(SubtleColor), x, bottom + 6, new StringFormat { Alignment = StringAlignment.Center });
        }

        g.DrawString("n (размер задачи)", AxisFont, new SolidBrush(TextColor), (left + right) / 2, bottom + 24, new StringFormat { Alignment = StringAlignment.Center });
        g.DrawString("время, с", AxisFont, new SolidBrush(TextColor), bounds.Left + 6, top - 28);

        // линия и точки
        using (var linePen = new Pen(LineColor, 2.2f))
        {
            var pts = series.Points.Select(p => new PointF((float)X(p.N), (float)Y(Sec(p.AvgMs)))).ToArray();
            if (pts.Length > 1) g.DrawLines(linePen, pts);
            foreach (var pt in pts)
                g.FillEllipse(new SolidBrush(DotColor), pt.X - 3.5f, pt.Y - 3.5f, 7, 7);
        }

        // подпись значений возле точек (для читаемости — через одну, если точек много)
        int stepLabel = series.Points.Count > 10 ? 2 : 1;
        using var valueBrush = new SolidBrush(Color.FromArgb(185, 191, 205));
        for (int i = 0; i < series.Points.Count; i += stepLabel)
        {
            var p = series.Points[i];
            g.DrawString(FormatSeconds(Sec(p.AvgMs)) + " с", SmallFont, valueBrush, (float)X(p.N) + 6, (float)Y(Sec(p.AvgMs)) - 15);
        }
    }

    /// <summary>Читаемый формат секунд: от 0.000007 до 1.7.</summary>
    private static string FormatSeconds(double s) => s switch
    {
        0 => "0",
        >= 1 => s.ToString("0.##"),
        >= 0.001 => s.ToString("0.####"),
        _ => s.ToString("0.######"),
    };

    /// <summary>Сохраняет график серии в PNG-файл.</summary>
    public static void SavePng(BenchmarkSeries series, string path, bool logScaleY, int width = 1000, int height = 620)
    {
        using var bmp = new Bitmap(width, height);
        using var g = Graphics.FromImage(bmp);
        Draw(g, new RectangleF(0, 0, width, height), series, logScaleY);
        bmp.Save(path, System.Drawing.Imaging.ImageFormat.Png);
    }
}
