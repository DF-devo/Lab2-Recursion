using RecursionLab2.Core.Benchmark;
using RecursionLab2.Core.Charting;

// Консольный стенд замеров: запускает серии, печатает таблицы и сохраняет
// результаты в results/ (CSV с числами + PNG с графиком).
// Аргумент "--only <hanoi|nqueens|sierpinski|quicksort>" запускает одну серию.
Console.OutputEncoding = System.Text.Encoding.UTF8;

string? only = null;
for (int i = 0; i < args.Length - 1; i++)
    if (args[i] == "--only") only = args[i + 1];

if (args.Contains("--quick"))
{
    if (only is null or "hanoi") RunAndSave("Ханойские башни", () => BenchmarkRunner.RunHanoi(10, 26, 2, n => Console.WriteLine($"  n = {n}...")), "hanoi");
    if (only is null or "nqueens") RunAndSave("Задача о N ферзях", () => BenchmarkRunner.RunNQueens(4, 14, 2, n => Console.WriteLine($"  n = {n}...")), "nqueens");
    if (only is null or "sierpinski") RunAndSave("Треугольник Серпинского", () => BenchmarkRunner.RunSierpinski(0, 16, 2, d => Console.WriteLine($"  глубина = {d}...")), "sierpinski");
    if (only is null or "quicksort") RunAndSave("Быстрая сортировка", () => BenchmarkRunner.RunQuickSort(10, 20, 2, n => Console.WriteLine($"  n = {n}...")), "quicksort");
}
else
{
    if (only is null or "hanoi") RunAndSave("Ханойские башни", () => BenchmarkRunner.RunHanoi(10, 29, 3, n => Console.WriteLine($"  n = {n}...")), "hanoi");
    if (only is null or "nqueens") RunAndSave("Задача о N ферзях", () => BenchmarkRunner.RunNQueens(4, 15, 3, n => Console.WriteLine($"  n = {n}...")), "nqueens");
    if (only is null or "sierpinski") RunAndSave("Треугольник Серпинского", () => BenchmarkRunner.RunSierpinski(0, 19, 3, d => Console.WriteLine($"  глубина = {d}...")), "sierpinski");
    if (only is null or "quicksort") RunAndSave("Быстрая сортировка", () => BenchmarkRunner.RunQuickSort(10, 24, 3, n => Console.WriteLine($"  n = {n}...")), "quicksort");
}

Console.WriteLine();
Console.WriteLine("Готово. Файлы в папке results/.");

static void RunAndSave(string name, Func<BenchmarkSeries> run, string fileKey)
{
    Console.WriteLine();
    Console.WriteLine($"=== {name} ===");
    var series = run();

    Console.WriteLine($"{"n",10} {"лучшее, с",12} {"среднее, с",12} {"объём работы",16}");
    foreach (var p in series.Points)
        Console.WriteLine($"{p.N,10} {p.BestMs / 1000.0,12:0.######} {p.AvgMs / 1000.0,12:0.######} {p.WorkSize,16}");

    Directory.CreateDirectory("results");
    string csv = Path.Combine("results", fileKey + ".csv");
    series.SaveCsv(csv);
    ChartPainter.SavePng(series, Path.Combine("results", fileKey + ".png"), logScaleY: false);
    Console.WriteLine($"Сохранено: {csv}, results/{fileKey}.png");
}
