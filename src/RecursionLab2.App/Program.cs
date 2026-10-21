namespace RecursionLab2.App;

internal static class Program
{
    /// <summary>Точка входа GUI-приложения с демонстрациями и стендом замеров.</summary>
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        Application.Run(new MainForm());
    }
}
