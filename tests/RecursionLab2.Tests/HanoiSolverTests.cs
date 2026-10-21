using RecursionLab2.Core.Hanoi;

namespace RecursionLab2.Tests;

public class HanoiSolverTests
{
    [Theory]
    [InlineData(0, 0)]
    [InlineData(1, 1)]
    [InlineData(3, 7)]
    [InlineData(5, 31)]
    [InlineData(10, 1023)]
    [InlineData(15, 32767)]
    public void Solve_ProducesExactlyTwoPowNMinusOneMoves(int n, long expected)
    {
        Assert.Equal(expected, HanoiSolver.Solve(n).Count);
        Assert.Equal(expected, HanoiSolver.SolveCountingOnly(n));
        Assert.Equal(expected, HanoiSolver.FormulaMoveCount(n));
    }

    [Fact]
    public void Solve_EveryMoveIsLegalAndAllDisksEndOnTargetPeg()
    {
        const int n = 8;
        var moves = HanoiSolver.Solve(n, fromPeg: 0, toPeg: 2, auxPeg: 1);

        var pegs = new Stack<int>[3];
        for (int i = 0; i < 3; i++) pegs[i] = new Stack<int>();
        for (int d = n; d >= 1; d--) pegs[0].Push(d); // снизу вверх убывание: на вершине стека диск 1

        foreach (var m in moves)
        {
            Assert.NotEmpty(pegs[m.FromPeg]);
            int disk = pegs[m.FromPeg].Pop();
            Assert.Equal(m.Disk, disk);
            // нельзя класть больший диск на меньший
            if (pegs[m.ToPeg].Count > 0)
                Assert.True(pegs[m.ToPeg].Peek() > disk, $"диск {disk} положен на диск {pegs[m.ToPeg].Peek()}");
            pegs[m.ToPeg].Push(disk);
        }

        Assert.Empty(pegs[0]);
        Assert.Empty(pegs[1]);
        Assert.Equal(n, pegs[2].Count);
        for (int d = 1; d <= n; d++) Assert.Equal(d, pegs[2].Pop()); // сверху вниз 1..n
    }

    [Fact]
    public void Solve_FourDisks_MatchesKnownSequenceStart()
    {
        // известное начало решения для 4 дисков: (1:0->1) (2:0->2) (1:1->2) (3:0->1)
        var moves = HanoiSolver.Solve(4);
        Assert.Equal(new HanoiMove(1, 0, 1), moves[0]);
        Assert.Equal(new HanoiMove(2, 0, 2), moves[1]);
        Assert.Equal(new HanoiMove(1, 1, 2), moves[2]);
        Assert.Equal(new HanoiMove(3, 0, 1), moves[3]);
    }
}
