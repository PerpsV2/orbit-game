using System;
using Xunit;

namespace OrbitGame.Tests;

public class CircularCollider_Tests
{
    private readonly CircularCollider _testEmptyCollider = new(0);
    private readonly CircularCollider _testUnitCollider = new(1);
    private readonly ConvexCollider _testConvexCollider = new([new(1, 2), new(1, -2), new(-1, -2), new(-1, 2)]);
    private readonly PDecimal _testMass = 10;

    [Fact]
    public void CircularCollider_Constructor()
    {
        Assert.Throws<ArgumentException>(() => new CircularCollider(-1));
    }
    
    [Fact]
    public void CircularCollider_CalculateInertiaMethod()
    {
        Assert.Equal(5, _testUnitCollider.CalculateInertia(_testMass));
    }

    [Fact]
    public void CircularCollider_IsEmptyMethod()
    {
        Assert.True(_testEmptyCollider.IsEmpty());
        Assert.False(_testUnitCollider.IsEmpty());
    }

    [Fact]
    public void CircularCollider_IntersectsWithPointMethod()
    {
        SpatialInfo originSpatialInfo = new(DVector2<>.Zero, 0);
        SpatialInfo touchingSpatialInfo = new(new DVector2<>(1, 0), 0);
        SpatialInfo notTouchingSpatialInfo = new(new DVector2<>(2, 0), 0);

        Assert.Equal(new PointCollision(false),
            _testEmptyCollider.IntersectsWith(DVector2<>.Zero, originSpatialInfo));
        Assert.Equal(new PointCollision(true),
            _testUnitCollider.IntersectsWith(DVector2<>.Zero, originSpatialInfo));
        Assert.Equal(new PointCollision(true),
            _testUnitCollider.IntersectsWith(DVector2<>.Zero, touchingSpatialInfo));
        Assert.Equal(new PointCollision(false),
            _testUnitCollider.IntersectsWith(DVector2<>.Zero, notTouchingSpatialInfo));
    }

    [Fact]
    public void CircularCollider_IntersectsWithCircularMethod()
    {
        SpatialInfo originSpatialInfo = new(DVector2<>.Zero, 0);
        SpatialInfo overlappingSpatialInfo = new(new DVector2<>(1, 0), 0);
        SpatialInfo touchingSpatialInfo = new(new DVector2<>(2, 0), 0);
        SpatialInfo notTouchingSpatialInfo = new(new DVector2<>(3, 0), 0);

        PhysicsCollision? overlappingIntersection = _testUnitCollider
            .IntersectsWith(_testUnitCollider, originSpatialInfo, overlappingSpatialInfo);
        PhysicsCollision? touchingIntersection = _testUnitCollider
            .IntersectsWith(_testUnitCollider, originSpatialInfo, touchingSpatialInfo);
        PhysicsCollision? notTouchingIntersection = _testUnitCollider
            .IntersectsWith(_testUnitCollider, originSpatialInfo, notTouchingSpatialInfo);

        Assert.Null(_testEmptyCollider.IntersectsWith(_testUnitCollider, originSpatialInfo, originSpatialInfo));
        Assert.NotNull(_testUnitCollider.IntersectsWith(_testUnitCollider, originSpatialInfo, originSpatialInfo));

        if (!overlappingIntersection.HasValue) { Assert.Fail("Null collision"); return; }
        Assert.Equal(new PhysicsCollision(originSpatialInfo, overlappingSpatialInfo, [new(1, 0)], new(-1, 0)),
            overlappingIntersection.Value);

        if (!touchingIntersection.HasValue) { Assert.Fail("Null collision"); return; }
        Assert.Equal(new PhysicsCollision(originSpatialInfo, touchingSpatialInfo, [new(1, 0)], new(0, 0)),
            touchingIntersection.Value);

        Assert.Null(notTouchingIntersection);
    }

    [Fact]
    public void CircularCollider_IntersectsWithConvexMethod()
    {
        SpatialInfo originSpatialInfo = new(DVector2<>.Zero, 0);
        SpatialInfo overlappingSpatialInfo = new(new DVector2<>(2, 0), Math.PI / 2);
        SpatialInfo touchingSpatialInfo = new(new DVector2<>(2, 0), 0);
        SpatialInfo notTouchingSpatialInfo = new(new DVector2<>(3, 0), 0);

        PhysicsCollision? overlappingIntersection = _testUnitCollider
            .IntersectsWith(_testConvexCollider, originSpatialInfo, overlappingSpatialInfo);
        PhysicsCollision? touchingIntersection = _testUnitCollider
            .IntersectsWith(_testConvexCollider, originSpatialInfo, touchingSpatialInfo);
        PhysicsCollision? notTouchingIntersection = _testUnitCollider
            .IntersectsWith(_testConvexCollider, originSpatialInfo, notTouchingSpatialInfo);

        Assert.Null(_testEmptyCollider.IntersectsWith(_testConvexCollider, originSpatialInfo, originSpatialInfo));

        if (!overlappingIntersection.HasValue) { Assert.Fail("Null collision"); return; }
        Assert.Equal(new PhysicsCollision(originSpatialInfo, overlappingSpatialInfo, 
            [new(1, 0)], new(-1, 0)), overlappingIntersection.Value);

        if (!touchingIntersection.HasValue) { Assert.Fail("Null collision"); return; }
        Assert.Equal(new PhysicsCollision(originSpatialInfo, touchingSpatialInfo, 
            [new(1, 0)], new(0, 0)), touchingIntersection.Value);

        Assert.Null(notTouchingIntersection);
    }

    [Fact]
    public void CircularCollider_NearsWithMethod()
    {
        SpatialInfo originSpatialInfo = new(DVector2<>.Zero, 0);
        SpatialInfo nearsCircularSpatialInfo = new(new DVector2<>(1.5, 1.5), 0);
        SpatialInfo nearsConvexSpatialInfo = new(new DVector2<>(3 - 0.1, 2 - 0.1), Math.PI / 2);
        SpatialInfo notNearsSpatialInfo = new(new DVector2<>(5, 0), 0);
            
        Assert.False(_testEmptyCollider.NearsWith(_testUnitCollider, originSpatialInfo, originSpatialInfo));
        Assert.True(_testUnitCollider.NearsWith(_testUnitCollider, originSpatialInfo, nearsCircularSpatialInfo));
        Assert.False(_testUnitCollider.NearsWith(_testUnitCollider, originSpatialInfo, notNearsSpatialInfo));
        Assert.True(_testUnitCollider.NearsWith(_testConvexCollider, originSpatialInfo, nearsConvexSpatialInfo));
    }
}