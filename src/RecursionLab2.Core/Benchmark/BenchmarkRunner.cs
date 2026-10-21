namespace RecursionLab2.Core.Benchmark;

/// <summary>Одна точка замера: размер задачи n, время (мс), объём работы (переносы/решения/треугольники).</summary>
public sealed record BenchmarkPoint(int N, double BestMs, double AvgMs, long WorkSize);

/// <summary>Серия замеров одного алгоритма.</summary>
public sealed record BenchmarkSeries(string Title, string WorkSizeTitle, List<BenchmarkPoint> Points)
{
    /// <summary>Сохраняет серию в CSV; время — в секундах.</summary>
    public void SaveCsv(string path)
    {
        using var w = new StreamWriter(path, append: false, System.Text.Encoding.UTF8);
        w.WriteLine($"n;лучшее_время_с;среднее_время_с;{WorkSizeTitle}");
        foreach (var p in Points)
            w.WriteLine(FormattableString.Invariant($"{p.N};{p.BestMs / 1000.0:0.########};{p.AvgMs / 1000.0:0.########};{p.WorkSize}"));
    }
}

/// <summary>
/// Стенд замеров времени, как в первой лабораторной: для каждого размера задачи
/// выполняется несколько прогонов, фиксируется лучшее и среднее время (Stopwatch).
/// </summary>
public static class BenchmarkRunner
{
    /// <summary>Ханойские башни: время рекурсивного решения (без хранения переносов), n = minN..maxN.</summary>
    public static BenchmarkSeries RunHanoi(int minN = 10, int maxN = 29, int repeats = 3, Action<int>? progress = null)
    {
        var points = new List<BenchmarkPoint>();
        for (int n = minN; n <= maxN; n++)
        {
            progress?.Invoke(n);
            var (best, avg) = Measure(() => Hanoi.HanoiSolver.SolveCountingOnly(n), repeats);
            points.Add(new BenchmarkPoint(n, best, avg, Hanoi.HanoiSolver.FormulaMoveCount(n)));
        }
        return new BenchmarkSeries("Ханойские башни (рекурсия)", "число_переносов", points);
    }

    /// <summary>Задача о N ферзях: время подсчёта всех решений, n = minN..maxN.</summary>
    public static BenchmarkSeries RunNQueens(int minN = 4, int maxN = 15, int repeats = 3, Action<int>? progress = null)
    {
        var points = new List<BenchmarkPoint>();
        for (int n = minN; n <= maxN; n++)
        {
            progress?.Invoke(n);
            long solutions = 0;
            var (best, avg) = Measure(() => solutions = new NQueens.NQueensSolver().CountSolutions(n), repeats);
            points.Add(new BenchmarkPoint(n, best, avg, solutions));
        }
        return new BenchmarkSeries("Задача о N ферзях (подсчёт всех решений)", "число_решений", points);
    }

    /// <summary>Треугольник Серпинского: время рекурсивного обхода без хранения (CountOnly), глубина = minD..maxD.</summary>
    public static BenchmarkSeries RunSierpinski(int minD = 0, int maxD = 19, int repeats = 3, Action<int>? progress = null)
    {
        var points = new List<BenchmarkPoint>();
        for (int d = minD; d <= maxD; d++)
        {
            progress?.Invoke(d);
            var (best, avg) = Measure(() => { Sierpinski.SierpinskiTriangle.CountOnly(d); }, repeats);
            points.Add(new BenchmarkPoint(d, best, avg, Sierpinski.SierpinskiTriangle.TriangleCount(d)));
        }
        return new BenchmarkSeries("Треугольник Серпинского (рекурсивный обход)", "число_треугольников", points);
    }

    /// <summary>
    /// Быстрая сортировка: время сортировки случайного массива, n = 2^minExp..2^maxExp.
    /// Для каждого n массив генерируется заново с фиксированным зерном — все прогоны
    /// сортируют одинаковые данные.
    /// </summary>
    public static BenchmarkSeries RunQuickSort(int minExp = 10, int maxExp = 24, int repeats = 3, Action<int>? progress = null)
    {
        var points = new List<BenchmarkPoint>();
        var sorter = new QuickSort.QuickSorter();
        for (int e = minExp; e <= maxExp; e++)
        {
            int n = 1 << e;
            progress?.Invoke(n);
            long work = (long)Math.Round((double)n * e); // теоретическая оценка сравнений n·log₂n
            var (best, avg) = Measure(() => sorter.Sort(MakeRandomArray(n, seed: 42)), repeats);
            points.Add(new BenchmarkPoint(n, best, avg, work));
        }
        return new BenchmarkSeries("Быстрая сортировка (случайный массив)", "n·log2n_оценка_сравнений", points);
    }

    /// <summary>Случайный массив из n чисел с фиксированным зерном (воспроизводимые замеры).</summary>
    private static int[] MakeRandomArray(int n, int seed)
    {
        var rnd = new Random(seed);
        var data = new int[n];
        for (int i = 0; i < n; i++) data[i] = rnd.Next(int.MaxValue);
        return data;
    }

    /// <summary>Замеряет действие repeats раз: прогрев + лучшее/среднее время в миллисекундах.</summary>
    private static (double best, double avg) Measure(Action work, int repeats)
    {
        work(); // прогревочный прогон (JIT, кэши)

        var watch = System.Diagnostics.Stopwatch.StartNew();
        var times = new double[repeats];
        for (int i = 0; i < repeats; i++)
        {
            watch.Restart();
            work();
            watch.Stop();
            times[i] = watch.Elapsed.TotalMilliseconds;
        }
        return (times.Min(), times.Average());
    }
}
