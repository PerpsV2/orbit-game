using System;
using Xunit;

namespace OrbitGame.Tests;

public class ConvexCollider_Tests
{
    private readonly ConvexCollider _testEmptyPointCollider = new([SD_Vector2.Zero]);
    private readonly ConvexCollider _testEmptyLineCollider = new([SD_Vector2.Zero, new SD_Vector2(5, 0)]);
    private readonly ConvexCollider _testCollider = new([new(1, 2), new(1, -2), new(-1, -2), new(-1, 2)]);
    private readonly CircularCollider _testCircularCollider = new(1);
    private readonly ScientificDecimal _testMass = 10;

    [Fact]
    public void ConvexCollider_CalculateInertiaMethod()
    {
        Assert.Equal(50f/3, _testCollider.CalculateInertia(_testMass));
    }

    [Fact]
    public void ConvexCollider_IsEmptymethod()
    {
        Assert.True(_testEmptyPointCollider.IsEmpty());
        Assert.True(_testEmptyLineCollider.IsEmpty());
        Assert.False(_testCollider.IsEmpty());
    }

    [Fact]
    public void ConvexCollider_IntersectsWithPointMethod()
    {
        SpatialInfo originSpatialInfo = new(SD_Vector2.Zero, 0);
        SD_Vector2 overlappingPoint = SD_Vector2.Zero;
        SD_Vector2 touchingPoint = new SD_Vector2(1, 0);
        SD_Vector2 notTouchingPoint = new SD_Vector2(5, 0);
        
        Assert.Equal(new PointCollision(false), _testEmptyLineCollider.IntersectsWith(notTouchingPoint, originSpatialInfo));
        Assert.Equal(new PointCollision(true), _testCollider.IntersectsWith(overlappingPoint, originSpatialInfo));
        Assert.Equal(new PointCollision(true), _testCollider.IntersectsWith(touchingPoint, originSpatialInfo));
        Assert.Equal(new PointCollision(false), _testCollider.IntersectsWith(notTouchingPoint, originSpatialInfo));
    }

    [Fact]
    public void ConvexCollider_IntersectsWithCircularMethod()
    {
        SpatialInfo originSpatialInfo = new(SD_Vector2.Zero);
        SpatialInfo overlappingSpatialInfo = new(new(1, 0), 0);
        SpatialInfo touchingSpatialInfo = new(new(2, 0), 0);
        SpatialInfo notTouchingSpatialInfo = new(new(5, 0), 0);

        PhysicsCollision? overlappingIntersection =
            _testCollider.IntersectsWith(_testCircularCollider, originSpatialInfo, overlappingSpatialInfo);
        PhysicsCollision? touchingIntersection =
            _testCollider.IntersectsWith(_testCircularCollider, originSpatialInfo, touchingSpatialInfo);
        PhysicsCollision? notTouchingIntersection =
            _testCollider.IntersectsWith(_testCircularCollider, originSpatialInfo, notTouchingSpatialInfo);

        Assert.Null(_testCollider.IntersectsWith(_testEmptyPointCollider, originSpatialInfo, originSpatialInfo));
        Assert.NotNull(_testCollider.IntersectsWith(_testCircularCollider, originSpatialInfo, originSpatialInfo));
        
        if (!overlappingIntersection.HasValue) { Assert.Fail("Null collision"); return; }
        Assert.Equal(new PhysicsCollision(originSpatialInfo, overlappingSpatialInfo, [new(1, 0)], new(-1, 0)),
            overlappingIntersection.Value);
        
        if (!touchingIntersection.HasValue) { Assert.Fail("Null collision"); return; }
        Assert.Equal(new PhysicsCollision(originSpatialInfo, touchingSpatialInfo, [new(1, 0)], SD_Vector2.Zero),
            touchingIntersection.Value);
        
        Assert.Null(notTouchingIntersection);
    }

    [Fact]
    public void ConvexCollider_IntersectsWithConvexMethod()
    {
        SpatialInfo originSpatialInfo = new(SD_Vector2.Zero, 0);
        SpatialInfo overlappingSpatialInfo = new(new(1, 0), Math.PI / 2);
        SpatialInfo touchingSpatialInfo = new(new(3, 0), Math.PI / 2);
        SpatialInfo notTouchingSpatialInfo = new(new(3, 0), 0);

        PhysicsCollision? overlappingIntersection =
            _testCollider.IntersectsWith(_testCollider, originSpatialInfo, overlappingSpatialInfo);
        PhysicsCollision? touchingIntersection =
            _testCollider.IntersectsWith(_testCollider, originSpatialInfo, touchingSpatialInfo);
        PhysicsCollision? notTouchingIntersection =
            _testCollider.IntersectsWith(_testCollider, originSpatialInfo, notTouchingSpatialInfo);

        Assert.NotNull(_testCollider.IntersectsWith(_testCollider, originSpatialInfo, originSpatialInfo));
        
        if (!overlappingIntersection.HasValue) { Assert.Fail("Null collision"); return; }
        Assert.Equal(new PhysicsCollision(originSpatialInfo, overlappingSpatialInfo, [new(1, 0)], new(-2, 0)),
            overlappingIntersection.Value);
        
        if (!touchingIntersection.HasValue) { Assert.Fail("Null collision"); return; }
        Assert.Equal(new PhysicsCollision(originSpatialInfo, touchingSpatialInfo, [new(1, 0)], SD_Vector2.Zero),
            touchingIntersection.Value);
        
        Assert.Null(notTouchingIntersection);
    }

    [Fact]
    public void ConvexCollider_NearsWithMethod()
    {
        SpatialInfo originSpatialInfo = new(SD_Vector2.Zero, 0);
        SpatialInfo nearsCircularSpatialInfo = new(new SD_Vector2(2 - 0.1, 3 - 0.1), 0);
        SpatialInfo nearsConvexSpatialInfo = new(new SD_Vector2(3, 0), Math.PI / 2);
        SpatialInfo notNearsSpatialInfo = new(new SD_Vector2(5, 0), 0);
        
        Assert.False(_testEmptyPointCollider.NearsWith(_testCollider, originSpatialInfo, originSpatialInfo));
        Assert.True(_testCollider.NearsWith(_testCollider, originSpatialInfo, nearsConvexSpatialInfo));
        Assert.False(_testCollider.NearsWith(_testCollider, originSpatialInfo, notNearsSpatialInfo));
        Assert.True(_testCollider.NearsWith(_testCircularCollider, originSpatialInfo, nearsCircularSpatialInfo));
    }
}