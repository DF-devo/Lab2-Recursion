namespace RecursionLab2.Core.NQueens;

/// <summary>Действие одного шага бэктрекинга.</summary>
public enum QueenStepAction
{
    /// <summary>Ферзя поставили на клетку (пробуем вариант).</summary>
    Place,
    /// <summary>Вариант не подошёл / решение закончено — ферзя сняли (откат).</summary>
    Remove,
    /// <summary>Найдено полное решение.</summary>
    Solution
}

/// <summary>Шаг алгоритма для визуализации.</summary>
public readonly record struct QueenStep(int Row, int Col, QueenStepAction Action, int PlacedCount);

/// <summary>
/// Задача о N ферзях: расставить на доске N×N N ферзей так, чтобы никакие два не били друг друга.
/// Рекурсивный бэктрекинг по строкам: в каждой строке перебираем столбцы;
/// клетка безопасна, если столбец и обе диагонали свободны.
/// База рекурсии: строка вышла за пределы доски — расстановка найдена.
/// Сложность в худшем случае O(N!) — дерево вариантов усекается проверками на каждом уровне.
/// </summary>
public sealed class NQueensSolver
{
    private bool[] _cols = Array.Empty<bool>();
    private bool[] _diag1 = Array.Empty<bool>(); // диагонали «\»: индекс row + col
    private bool[] _diag2 = Array.Empty<bool>(); // диагонали «/»: индекс row − col + n − 1
    private int[] _queens = Array.Empty<int>();  // _queens[row] = столбец ферзя в этой строке

    /// <summary>Срабатывает на каждом действии алгоритма (нужен для анимации).</summary>
    public event Action<QueenStep>? Step;

    /// <summary>Сколько попыток поставить ферзя сделано (для статистики).</summary>
    public long Attempts { get; private set; }

    /// <summary>Сколько откатов сделано.</summary>
    public long Backtracks { get; private set; }

    private bool RaiseEvents => Step is not null;

    /// <summary>Ищет первое решение. Возвращает массив, где index = строка, значение = столбец ферзя; null, если решений нет.</summary>
    public int[]? SolveFirst(int n)
    {
        Reset(n);
        return PlaceRow(0, n, stopAtFirst: true) > 0 ? (int[])_queens.Clone() : null;
    }

    /// <summary>Ищет все решения (для замеров времени и проверки известного числа решений).</summary>
    public long CountSolutions(int n, CancellationToken cancellation = default)
    {
        Reset(n);
        return PlaceRow(0, n, stopAtFirst: false, cancellation);
    }

    private void Reset(int n)
    {
        _cols = new bool[n];
        _diag1 = new bool[2 * n - 1];
        _diag2 = new bool[2 * n - 1];
        _queens = new int[n];
        Array.Fill(_queens, -1);
        Attempts = 0;
        Backtracks = 0;
    }

    /// <summary>
    /// Рекурсия: расставить ферзей, начиная со строки row.
    /// Возвращает число найденных решений (при stopAtFirst останавливается на первом).
    /// </summary>
    private long PlaceRow(int row, int n, bool stopAtFirst, CancellationToken cancellation = default)
    {
        if (row == n) // база рекурсии: все строки заполнены — решение найдено
        {
            if (RaiseEvents) Step?.Invoke(new QueenStep(row, -1, QueenStepAction.Solution, row));
            return 1;
        }

        long found = 0;
        for (int col = 0; col < n; col++)
        {
            cancellation.ThrowIfCancellationRequested();
            if (_cols[col] || _diag1[row + col] || _diag2[row - col + n - 1])
                continue; // клетка под боем — вариант отбрасываем

            _cols[col] = _diag1[row + col] = _diag2[row - col + n - 1] = true;
            _queens[row] = col;
            Attempts++;
            if (RaiseEvents) Step?.Invoke(new QueenStep(row, col, QueenStepAction.Place, row + 1));

            found += PlaceRow(row + 1, n, stopAtFirst, cancellation);
            if (stopAtFirst && found > 0)
                return found; // решение найдено — ферзи не снимаем, доска остаётся заполненной

            // откат: снимаем ферзя и пробуем следующий столбец
            _cols[col] = _diag1[row + col] = _diag2[row - col + n - 1] = false;
            _queens[row] = -1;
            Backtracks++;
            if (RaiseEvents) Step?.Invoke(new QueenStep(row, col, QueenStepAction.Remove, row));
        }
        return found;
    }

    /// <summary>Проверяет, что расстановка корректна (никакие два ферзя не бьют друг друга).</summary>
    public static bool IsValid(int[] queens)
    {
        for (int i = 0; i < queens.Length; i++)
            for (int j = i + 1; j < queens.Length; j++)
                if (queens[i] == queens[j] || Math.Abs(queens[i] - queens[j]) == j - i)
                    return false;
        return true;
    }
}
