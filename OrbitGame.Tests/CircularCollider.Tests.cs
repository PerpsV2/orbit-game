namespace OrbitGame.Tests;

public class CircularCollider_Tests
{
    private readonly CircularCollider _testEmptyCollider = new(0);
    private readonly CircularCollider _testCircularCollider = new(1);
    private readonly SpatialInfo _testOriginSpatialInfo = new(SD_Vector2.Zero, 0);
    private readonly SpatialInfo _testTouchingOriginSpatialInfo = new(new SD_Vector2(0, 1), Math.PI / 2);
    private readonly SpatialInfo _testNotTouchingOriginSpatialInfo = new(new SD_Vector2(0, 2), -Math.PI / 2);
    private readonly ScientificDecimal _testMass = 10;
    
    [Fact]
    public void CircularCollider_CalculateInertiaMethod()
    {
        Assert.Equal(5, _testCircularCollider.CalculateInertia(_testMass));
    }

    [Fact]
    public void CircularCollider_IntersectsWithPointMethod()
    {
        Assert.Equal(new PointCollision(false), 
            _testEmptyCollider.IntersectsWith(SD_Vector2.Zero, _testOriginSpatialInfo));
        Assert.Equal(new PointCollision(true), 
            _testCircularCollider.IntersectsWith(SD_Vector2.Zero, _testOriginSpatialInfo));
        Assert.Equal(new PointCollision(true), 
            _testCircularCollider.IntersectsWith(SD_Vector2.Zero, _testTouchingOriginSpatialInfo));
        Assert.Equal(new PointCollision(false), 
            _testCircularCollider.IntersectsWith(SD_Vector2.Zero, _testNotTouchingOriginSpatialInfo));
    }

    [Fact]
    public void CircularCollider_IntersectsWithCircularMethod()
    {
        Assert.Null(_testEmptyCollider.IntersectsWith(_testCircularCollider, _testOriginSpatialInfo, _testOriginSpatialInfo));
        Assert.NotNull(_testCircularCollider.IntersectsWith(_testCircularCollider, _testOriginSpatialInfo, _testOriginSpatialInfo));
    }
}