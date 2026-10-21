namespace RecursionLab2.Core.Sierpinski;

/// <summary>Треугольник на плоскости (координаты вершин).</summary>
public readonly record struct TriangleF(float Ax, float Ay, float Bx, float By, float Cx, float Cy);

/// <summary>
/// Треугольник Серпинского — фрактал, построенный честной рекурсией:
/// треугольник делится отрезками, соединяющими середины сторон, на 4 треугольника;
/// центральный выбрасывается, к трём оставшимся применяется та же процедура.
/// База рекурсии: глубина 0 — рисуем сам треугольник.
/// На глубине d получается 3^d треугольников.
/// </summary>
public static class SierpinskiTriangle
{
    /// <summary>Строит все треугольники фрактала глубины depth для вершин (A, B, C) исходного треугольника.</summary>
    public static List<TriangleF> Build(float ax, float ay, float bx, float by, float cx, float cy, int depth)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(depth);
        var result = new List<TriangleF>((int)Math.Pow(3, Math.Min(depth, 12)));
        Divide(ax, ay, bx, by, cx, cy, depth, result);
        return result;
    }

    private static void Divide(float ax, float ay, float bx, float by, float cx, float cy, int depth, List<TriangleF> acc)
    {
        if (depth == 0) // база рекурсии: треугольник больше не делим
        {
            acc.Add(new TriangleF(ax, ay, bx, by, cx, cy));
            return;
        }

        // середины сторон
        float abX = (ax + bx) / 2, abY = (ay + by) / 2;
        float bcX = (bx + cx) / 2, bcY = (by + cy) / 2;
        float caX = (cx + ax) / 2, caY = (cy + ay) / 2;

        Divide(ax, ay, abX, abY, caX, caY, depth - 1, acc); // верхний
        Divide(abX, abY, bx, by, bcX, bcY, depth - 1, acc); // левый
        Divide(caX, caY, bcX, bcY, cx, cy, depth - 1, acc); // правый
    }

    /// <summary>Число треугольников на глубине depth: 3^depth (для проверки рекурсии).</summary>
    public static long TriangleCount(int depth) => depth <= 0 ? 1 : (long)Math.Pow(3, depth);

    /// <summary>
    /// Рекурсивный подсчёт базовых треугольников без их хранения — та же структура вызовов,
    /// что и у Build, но без выделения памяти. Нужен для замеров на больших глубинах:
    /// Build на глубине ~17 хранит уже десятки миллионов треугольников и съедает гигабайты.
    /// </summary>
    public static long CountOnly(int depth)
    {
        if (depth == 0) return 1; // база рекурсии: один базовый треугольник
        return CountOnly(depth - 1) + CountOnly(depth - 1) + CountOnly(depth - 1);
    }
}
