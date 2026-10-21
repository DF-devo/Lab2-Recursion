using RecursionLab2.App.Demos;

namespace RecursionLab2.App;

/// <summary>Главное окно: вкладки с демонстрациями алгоритмов и стендом замеров.</summary>
public sealed class MainForm : Form
{
    private readonly TabControl _tabs;

    public MainForm()
    {
        Text = "Лабораторная №2 — Рекурсивные алгоритмы";
        Font = new Font("Segoe UI", 9.5f);
        StartPosition = FormStartPosition.CenterScreen;
        ClientSize = new Size(1180, 760);
        MinimumSize = new Size(960, 620);
        BackColor = Theme.Background;

        _tabs = new TabControl
        {
            Dock = DockStyle.Fill,
            DrawMode = TabDrawMode.OwnerDrawFixed,
            SizeMode = TabSizeMode.Fixed,
            ItemSize = new Size(240, 40),
            Padding = new Point(16, 10),
        };
        _tabs.TabPages.Add(new TabPage("Ханойские башни"));
        _tabs.TabPages.Add(new TabPage("Задача о N ферзях"));
        _tabs.TabPages.Add(new TabPage("Быстрая сортировка"));
        _tabs.TabPages.Add(new TabPage("Треугольник Серпинского"));
        _tabs.TabPages.Add(new TabPage("Замеры времени"));
        _tabs.DrawItem += DrawTab;
        _tabs.SelectedIndexChanged += (_, _) => _tabs.Invalidate();

        _tabs.TabPages[0].Controls.Add(new HanoiDemoControl { Dock = DockStyle.Fill });
        _tabs.TabPages[1].Controls.Add(new QueensDemoControl { Dock = DockStyle.Fill });
        _tabs.TabPages[2].Controls.Add(new QuickSortDemoControl { Dock = DockStyle.Fill });
        _tabs.TabPages[3].Controls.Add(new SierpinskiDemoControl { Dock = DockStyle.Fill });
        _tabs.TabPages[4].Controls.Add(new BenchmarkControl { Dock = DockStyle.Fill });

        Controls.Add(_tabs);
        Theme.Apply(this);
        foreach (TabPage page in _tabs.TabPages)
            page.BackColor = Theme.Surface;
        Theme.EnableDarkChrome(this); // тёмный заголовок окна и тёмные полосы прокрутки
    }

    private void DrawTab(object? sender, DrawItemEventArgs e)
    {
        var g = e.Graphics;
        var rect = e.Bounds;
        bool selected = e.Index == _tabs.SelectedIndex;

        using (var bg = new SolidBrush(Theme.Background))
            g.FillRectangle(bg, rect);
        if (selected)
        {
            using var fill = new SolidBrush(Theme.Surface);
            g.FillRectangle(fill, rect);
            using var accent = new SolidBrush(Theme.Accent);
            g.FillRectangle(accent, rect.X + 14, rect.Bottom - 4, rect.Width - 28, 3);
        }

        TextRenderer.DrawText(g, _tabs.TabPages[e.Index].Text, Font, rect,
            selected ? Color.White : Theme.Subtle,
            TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
    }
}
