using RecursionLab2.Core.Sierpinski;

namespace RecursionLab2.Tests;

public class SierpinskiTriangleTests
{
    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 3)]
    [InlineData(2, 9)]
    [InlineData(3, 27)]
    [InlineData(5, 243)]
    [InlineData(8, 6561)]
    public void Build_ProducesThreePowDepthTriangles(int depth, long expected)
    {
        var tris = SierpinskiTriangle.Build(0, 0, 600, 520, 300, 0, depth);
        Assert.Equal(expected, tris.Count);
        Assert.Equal(expected, SierpinskiTriangle.TriangleCount(depth));
    }

    [Fact]
    public void Build_DepthZero_ReturnsOriginalTriangle()
    {
        var tris = SierpinskiTriangle.Build(10, 20, 110, 20, 60, 100, 0);
        var t = Assert.Single(tris);
        Assert.Equal(new TriangleF(10, 20, 110, 20, 60, 100), t);
    }

    [Fact]
    public void Build_TrianglesShrinkWithDepth()
    {
        // на каждом шаге остаётся 3 треугольника из 4: суммарная площадь = (3/4)^d от исходной
        double area(TriangleF t) => Math.Abs((t.Bx - t.Ax) * (t.Cy - t.Ay) - (t.Cx - t.Ax) * (t.By - t.Ay)) / 2;

        const float ax = 0, ay = 0, bx = 400, by = 346, cx = 400, cy = 0;
        double total0 = SierpinskiTriangle.Build(ax, ay, bx, by, cx, cy, 0).Sum(area);
        double total4 = SierpinskiTriangle.Build(ax, ay, bx, by, cx, cy, 4).Sum(area);
        double expected = total0 * Math.Pow(0.75, 4);

        Assert.Equal(expected, total4, precision: 2);
    }
}
