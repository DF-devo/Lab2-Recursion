using System.Runtime.InteropServices;

namespace RecursionLab2.App;

/// <summary>Тёмная современная тема оформления приложения.</summary>
public static class Theme
{
    public static readonly Color Background = Color.FromArgb(21, 23, 31);
    public static readonly Color Surface = Color.FromArgb(29, 32, 42);
    public static readonly Color SurfaceAlt = Color.FromArgb(40, 44, 57);
    public static readonly Color Border = Color.FromArgb(54, 59, 76);
    public static readonly Color Accent = Color.FromArgb(101, 143, 255);
    public static readonly Color AccentHover = Color.FromArgb(78, 114, 214);
    public static readonly Color Text = Color.FromArgb(233, 236, 244);
    public static readonly Color Subtle = Color.FromArgb(150, 156, 172);
    public static readonly Color Good = Color.FromArgb(92, 204, 150);

    /// <summary>Рекурсивно применяет тему к дереву контролов.</summary>
    public static void Apply(Control root)
    {
        ApplyOne(root);
        foreach (Control child in root.Controls)
            Apply(child);
    }

    /// <summary>
    /// Тёмная рамка окна (заголовок) и тёмные системные полосы прокрутки.
    /// Работает на Windows 10 1809+; на старых сборках просто не применяется.
    /// </summary>
    public static void EnableDarkChrome(Form form)
    {
        form.HandleCreated += (_, _) => UseDarkTitleBar(form);
        if (form.IsHandleCreated) UseDarkTitleBar(form);
        DarkScrollbars(form);
    }

    private static void UseDarkTitleBar(Control form)
    {
        int on = 1;
        // DWMWA_USE_IMMERSIVE_DARK_MODE: 20 на сборках 1903+, 19 — на ранних 1809
        _ = DwmSetWindowAttribute(form.Handle, 19, ref on, sizeof(int));
        _ = DwmSetWindowAttribute(form.Handle, 20, ref on, sizeof(int));
    }

    private static void DarkScrollbars(Control root)
    {
        if (root is not Form && root is ListBox or DataGridView or NumericUpDown or ScrollableControl)
        {
            root.HandleCreated += (_, _) => SetDarkExplorerTheme(root);
            if (root.IsHandleCreated) SetDarkExplorerTheme(root);
        }
        foreach (Control child in root.Controls)
            DarkScrollbars(child);
    }

    private static void SetDarkExplorerTheme(Control c) =>
        _ = SetWindowTheme(c.Handle, "DarkMode_Explorer", null);

    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attribute, ref int value, int size);

    [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
    private static extern int SetWindowTheme(IntPtr hWnd, string? subAppName, string? subIdList);

    private static void ApplyOne(Control c)
    {
        switch (c)
        {
            case Button b: // обычные кнопки (CheckBox — наследник ButtonBase, сюда не попадает)
                b.FlatStyle = FlatStyle.Flat;
                b.FlatAppearance.BorderSize = 0;
                b.FlatAppearance.MouseOverBackColor = AccentHover;
                b.FlatAppearance.MouseDownBackColor = AccentHover;
                b.BackColor = SurfaceAlt;
                b.ForeColor = Text;
                b.Cursor = Cursors.Hand;
                b.AutoSize = true; // текст всегда помещается («Пауза» → «Продолжить»)
                b.MinimumSize = new Size(b.Width, 34);
                b.Padding = new Padding(10, 0, 10, 0);
                break;
            case DataGridView:
                return; // стилизуется отдельно в BenchmarkControl
            case TextBoxBase tb:
                tb.BorderStyle = BorderStyle.None;
                break;
            case CheckBox:
                c.ForeColor = Text;
                c.BackColor = Surface;
                break;
            case NumericUpDown nud:
                nud.BorderStyle = BorderStyle.FixedSingle;
                nud.BackColor = SurfaceAlt;
                nud.ForeColor = Text;
                break;
            case TrackBar:
                c.BackColor = Surface;
                break;
            default:
                c.BackColor = Surface;
                c.ForeColor = Text;
                break;
        }
    }
}

/// <summary>Панель с двойной буферизацией: перерисовка без мерцания.</summary>
public sealed class BufferedPanel : Panel
{
    public BufferedPanel()
    {
        DoubleBuffered = true;
        ResizeRedraw = true;
    }
}
