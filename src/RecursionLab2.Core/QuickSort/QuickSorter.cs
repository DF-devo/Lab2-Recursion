namespace RecursionLab2.Core.QuickSort;

/// <summary>Действие одного шага сортировки (для визуализации).</summary>
public enum SortAction
{
    /// <summary>Сравнение элементов a[i] и a[j].</summary>
    Compare,
    /// <summary>Обмен элементов a[i] и a[j].</summary>
    Swap,
    /// <summary>Выбран опорный элемент в позиции i.</summary>
    Pivot,
    /// <summary>Опорный элемент встал на своё финальное место i.</summary>
    Fixed,
}

/// <summary>Шаг алгоритма для визуализации: индексы i, j и действие.</summary>
public readonly record struct SortStep(int I, int J, SortAction Action);

/// <summary>
/// Быстрая сортировка (QuickSort, Ч. Хоар, 1960): выбрать опорный элемент,
/// разделить массив на часть «≤ опорного» и часть «> опорного», затем рекурсивно
/// отсортировать обе части. База рекурсии: 0 или 1 элемент — уже отсортированы.
/// Средняя сложность O(n·log n), худшая O(n²); дополнительная память O(log n) —
/// глубина рекурсии (сортировка выполняется на месте).
/// Для защиты от худшего случая на уже упорядоченных входах опорный элемент
/// выбирается как медиана из трёх (первый, средний, последний элементы части).
/// Сортировка неустойчивая (порядок равных элементов не сохраняется).
/// </summary>
public sealed class QuickSorter
{
    private int[] _a = [];
    private bool RaiseEvents => Step is not null;

    /// <summary>Срабатывает на каждом действии алгоритма (нужен для анимации).</summary>
    public event Action<SortStep>? Step;

    /// <summary>Число выполненных сравнений.</summary>
    public long Comparisons { get; private set; }

    /// <summary>Число выполненных обменов.</summary>
    public long Swaps { get; private set; }

    /// <summary>Сортирует массив по возрастанию на месте.</summary>
    public void Sort(int[] array)
    {
        ArgumentNullException.ThrowIfNull(array);
        _a = array;
        Comparisons = 0;
        Swaps = 0;
        Sort(0, array.Length - 1);
    }

    /// <summary>Рекурсия: отсортировать часть массива от lo до hi включительно.</summary>
    private void Sort(int lo, int hi)
    {
        if (lo >= hi) return; // база рекурсии: 0 или 1 элемент
        int p = Partition(lo, hi);
        Sort(lo, p - 1); // левая часть: всё ≤ опорного
        Sort(p + 1, hi); // правая часть: всё > опорного
    }

    /// <summary>
    /// Разбиение Ломуто: опорный элемент ставится в конец части, затем элементы
    /// меньше/равные опорного собираются слева; возврат — финальная позиция опорного.
    /// </summary>
    private int Partition(int lo, int hi)
    {
        var a = _a;

        // медиана из трёх: упорядочиваем a[lo], a[mid], a[hi] и ставим медиану
        // опорным элементом в конец части — иначе на отсортированном входе
        // получилось бы худшее разбиение O(n²)
        if (hi - lo >= 2)
        {
            int mid = lo + (hi - lo) / 2;
            if (a[mid] < a[lo]) (a[lo], a[mid]) = (a[mid], a[lo]);
            if (a[hi] < a[lo]) (a[lo], a[hi]) = (a[hi], a[lo]);
            if (a[hi] < a[mid]) (a[mid], a[hi]) = (a[hi], a[mid]);
            (a[mid], a[hi]) = (a[hi], a[mid]);
            if (RaiseEvents) Step?.Invoke(new SortStep(hi, -1, SortAction.Pivot));
        }
        else if (RaiseEvents)
        {
            Step?.Invoke(new SortStep(hi, -1, SortAction.Pivot));
        }

        int pivot = a[hi];
        int i = lo - 1; // правая граница зоны «≤ опорного»
        for (int j = lo; j < hi; j++)
        {
            Comparisons++;
            if (RaiseEvents) Step?.Invoke(new SortStep(j, hi, SortAction.Compare));
            if (a[j] <= pivot)
            {
                i++;
                if (i != j)
                {
                    (a[i], a[j]) = (a[j], a[i]);
                    Swaps++;
                    if (RaiseEvents) Step?.Invoke(new SortStep(i, j, SortAction.Swap));
                }
            }
        }

        (a[i + 1], a[hi]) = (a[hi], a[i + 1]); // опорный — на финальное место
        Swaps++;
        if (RaiseEvents) Step?.Invoke(new SortStep(i + 1, -1, SortAction.Fixed));
        return i + 1;
    }
}
