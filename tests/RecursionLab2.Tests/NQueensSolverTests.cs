using RecursionLab2.Core.NQueens;

namespace RecursionLab2.Tests;

public class NQueensSolverTests
{
    [Theory]
    [InlineData(1, 1)]
    [InlineData(2, 0)]
    [InlineData(3, 0)]
    [InlineData(4, 2)]
    [InlineData(5, 10)]
    [InlineData(6, 4)]
    [InlineData(7, 40)]
    [InlineData(8, 92)]
    [InlineData(9, 352)]
    [InlineData(10, 724)]
    [InlineData(11, 2680)]
    [InlineData(12, 14200)]
    public void CountSolutions_MatchesKnownValues(int n, long expected)
    {
        Assert.Equal(expected, new NQueensSolver().CountSolutions(n));
    }

    [Theory]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    [InlineData(8)]
    [InlineData(10)]
    [InlineData(12)]
    public void SolveFirst_ReturnsValidArrangement(int n)
    {
        var solution = new NQueensSolver().SolveFirst(n);
        Assert.NotNull(solution);
        Assert.Equal(n, solution!.Length);
        Assert.All(solution, c => Assert.InRange(c, 0, n - 1));
        Assert.True(NQueensSolver.IsValid(solution), "найденная расстановка содержит конфликтующие ферзи");
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    public void SolveFirst_NoSolution_ForTwoAndThree(int n)
    {
        Assert.Null(new NQueensSolver().SolveFirst(n));
    }

    [Fact]
    public void StepEvents_FireForPlaceAndRemove()
    {
        var solver = new NQueensSolver();
        var actions = new List<QueenStep>();
        solver.Step += s => actions.Add(s);

        solver.SolveFirst(4);

        Assert.Contains(actions, a => a.Action == QueenStepAction.Place);
        Assert.Contains(actions, a => a.Action == QueenStepAction.Remove); // бэктрекинг был
        Assert.Contains(actions, a => a.Action == QueenStepAction.Solution);
    }

    [Fact]
    public void IsValid_DetectsConflicts()
    {
        Assert.True(NQueensSolver.IsValid([1, 3, 0, 2]));  // решение для n = 4
        Assert.False(NQueensSolver.IsValid([0, 0, 0, 0])); // один столбец
        Assert.False(NQueensSolver.IsValid([0, 1, 2, 3])); // одна диагональ
    }
}
