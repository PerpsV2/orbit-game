namespace OrbitGame.Tests;

public class ConvexHull_Tests
{
    private Vec2Double[] _pointCloud = [
        new(2, 2), new(6, 2), new(4, 4), new(2, 6), new(-2, 2), new(0, 2), new(2, -2)
    ];
    private ConvexHull _convexHull;
    
    [Fact]
    public void ConvexHull_Constructor()
    {
        _convexHull = new ConvexHull(_pointCloud);
        Assert.Equal(new Vec2Double[] { new(-4, 0), new(0, -4), new(4, 0), new(0, 4) }, _convexHull.Points);
    }

    [Fact]
    public void ConvexHull_CenterOfMassMethod()
    {
        
    }
}