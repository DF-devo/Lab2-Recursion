namespace RecursionLab2.Core.Hanoi;

/// <summary>Один перенос диска: диск <see cref="Disk"/> со стержня <see cref="FromPeg"/> на стержень <see cref="ToPeg"/>.</summary>
public readonly record struct HanoiMove(int Disk, int FromPeg, int ToPeg)
{
    public override string ToString() => $"диск {Disk}: {FromPeg + 1} -> {ToPeg + 1}";
}

/// <summary>
/// Ханойские башни — классическая задача о переносе башни дисков между тремя стержнями.
/// Рекуррентная идея: чтобы перенести n дисков с A на C, нужно
/// 1) перенести n−1 дисков с A на B (вспомогательный),
/// 2) перенести самый большой диск с A на C,
/// 3) перенести n−1 дисков с B на C.
/// База рекурсии: n = 0 — переносить нечего.
/// Минимальное число переносов: T(n) = 2·T(n−1) + 1 = 2^n − 1.
/// </summary>
public static class HanoiSolver
{
    /// <summary>Строит полную последовательность переносов n дисков (нумерация стержней: 0, 1, 2).</summary>
    public static List<HanoiMove> Solve(int n, int fromPeg = 0, int toPeg = 2, int auxPeg = 1)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(n);
        var moves = new List<HanoiMove>(Math.Min((1 << Math.Min(n, 24)) - 1, 33_554_431));
        Move(n, fromPeg, toPeg, auxPeg, moves);
        return moves;
    }

    /// <summary>Сама рекурсия: перенести n дисков from -> to через aux.</summary>
    private static void Move(int n, int from, int to, int aux, List<HanoiMove> acc)
    {
        if (n == 0) return;              // база рекурсии
        Move(n - 1, from, aux, to, acc); // шаг 1: убираем n−1 дисков на вспомогательный стержень
        acc.Add(new HanoiMove(n, from, to)); // шаг 2: переносим самый большой диск
        Move(n - 1, aux, to, from, acc); // шаг 3: переносим башню n−1 на целевой стержень
    }

    /// <summary>
    /// Рекурсивно решает задачу, не сохраняя переносы (только считает их число) —
    /// вариант для замеров времени на больших n, где список переносов уже не помещается в память.
    /// </summary>
    public static long SolveCountingOnly(int n, int fromPeg = 0, int toPeg = 2, int auxPeg = 1)
    {
        if (n == 0) return 0;
        return 1
             + SolveCountingOnly(n - 1, fromPeg, auxPeg, toPeg)
             + SolveCountingOnly(n - 1, auxPeg, toPeg, fromPeg);
    }

    /// <summary>Минимальное число переносов по формуле 2^n − 1 (для проверки рекурсии).</summary>
    public static long FormulaMoveCount(int n) => n <= 0 ? 0 : (1L << n) - 1;
}
