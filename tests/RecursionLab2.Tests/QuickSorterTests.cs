using RecursionLab2.Core.QuickSort;

namespace RecursionLab2.Tests;

public class QuickSorterTests
{
    [Fact]
    public void Sort_EmptyAndSingleElementArrays_DoesNotThrow()
    {
        var sorter = new QuickSorter();
        sorter.Sort([]);
        sorter.Sort([7]);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(10)]
    [InlineData(100)]
    [InlineData(1000)]
    [InlineData(10000)]
    public void Sort_RandomArray_MatchesArraySort(int n)
    {
        var data = RandomArray(n, seed: 1234 + n);
        var expected = (int[])data.Clone();
        Array.Sort(expected);

        new QuickSorter().Sort(data);

        Assert.Equal(expected, data);
    }

    [Fact]
    public void Sort_AlreadySortedAndReversedArrays_MatchesArraySort()
    {
        var sorter = new QuickSorter();

        var sorted = Enumerable.Range(-500, 1001).ToArray();
        sorter.Sort(sorted);
        Assert.Equal(Enumerable.Range(-500, 1001), sorted);

        var reversed = Enumerable.Range(0, 1001).Reverse().ToArray();
        sorter.Sort(reversed);
        Assert.Equal(Enumerable.Range(0, 1001), reversed);
    }

    [Fact]
    public void Sort_AllElementsEqual_MatchesArraySort()
    {
        var data = Enumerable.Repeat(42, 500).ToArray();
        new QuickSorter().Sort(data);
        Assert.All(data, x => Assert.Equal(42, x));
    }

    [Fact]
    public void Sort_LargeSortedArray_CompletesQuickly()
    {
        // на отсортированном входе наивный выбор опорного даёт худший случай O(n²)
        // и переполнение стека; медиана из трёх должна это предотвращать
        var data = Enumerable.Range(0, 200_000).ToArray();
        new QuickSorter().Sort(data);
        Assert.Equal(Enumerable.Range(0, 200_000), data);
    }

    [Fact]
    public void Sort_CountsComparisonsAndSwaps()
    {
        var sorter = new QuickSorter();
        sorter.Sort(RandomArray(1000, seed: 7));

        // на практике ~0.9·n·log₂n сравнений; берём широкий диапазон
        Assert.InRange(sorter.Comparisons, 1000L * 5, 1000L * 15);
        Assert.InRange(sorter.Swaps, 1, sorter.Comparisons);
    }

    [Fact]
    public void StepEvents_FireForCompareSwapPivotAndFixed()
    {
        var sorter = new QuickSorter();
        var actions = new List<SortStep>();
        sorter.Step += s => actions.Add(s);

        sorter.Sort(RandomArray(100, seed: 9));

        Assert.Contains(actions, s => s.Action == SortAction.Compare);
        Assert.Contains(actions, s => s.Action == SortAction.Swap);
        Assert.Contains(actions, s => s.Action == SortAction.Pivot);
        Assert.Contains(actions, s => s.Action == SortAction.Fixed);
    }

    private static int[] RandomArray(int n, int seed)
    {
        var rnd = new Random(seed);
        var data = new int[n];
        for (int i = 0; i < n; i++) data[i] = rnd.Next(1_000_000);
        return data;
    }
}
