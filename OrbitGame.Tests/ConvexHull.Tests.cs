using Xunit;

namespace OrbitGame.Tests;

public class ConvexHull_Tests
{
    private readonly Vec2Double[] _symmetricPointCloud = [
        new(2, 2), new(6, 2), new(4, 4), new(2, 6), new(-2, 2), new(0, 2), new(2, -2)
    ];
    private readonly ConvexHull _convexHull;

    public ConvexHull_Tests()
    {
        _convexHull = new ConvexHull(_symmetricPointCloud);
    }
    
    [Fact]
    public void ConvexHull_Constructor()
    {
        Assert.Equal(new Vec2Double[] { new(-4, 0), new(0, -4), new(4, 0), new(0, 4) }, _convexHull.Points);
    }

    [Fact]
    public void ConvexHull_CalculateAreaMethod()
    {
        Assert.Equal(32, _convexHull.CalculateArea());
    }

    [Fact]
    public void ConvexHull_CalculateInertiaMethod()
    {
        Assert.Equal(16, _convexHull.CalculateInertia(3));
    }
}