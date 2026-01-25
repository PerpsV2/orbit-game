namespace OrbitGame.Tests;

public class ConvexCollider_Tests
{
    private class TestKinematicObject(ScientificDecimal mass, Vector2 position, Vector2 velocity)
        : KinematicObject("", mass, position, velocity);

    private readonly ConvexCollider _collider1 = new(
        [new Vector2(3, 3), new Vector2(-3, 3), new Vector2(-3, -3), new Vector2(3, -3)],
        new TestKinematicObject(100, Vector2.Zero, Vector2.Zero), new Material(0.5f));
    private readonly ConvexCollider _collider2 = new(
        [new Vector2(0, 3), new Vector2(-3, 0), new Vector2(0, -3), new Vector2(3, 0)],
        new TestKinematicObject(100, Vector2.Zero, Vector2.Zero), new Material(0.5f));
    private readonly ConvexCollider _collider3 = new([
            new Vector2(0, 3), 
            new Vector2(-3, 3), 
            new Vector2(-3, -3), 
            new Vector2(0, -3), 
            new Vector2(3, 0)
        ],
        new TestKinematicObject(100, Vector2.Zero, Vector2.Zero), new Material(0.5f));
    private readonly ConvexCollider _emptyCollider = new(
        [new Vector2(-1, 0), new Vector2(0, 0), new Vector2(1, 0)],
        new TestKinematicObject(100, Vector2.Zero, Vector2.Zero), new Material(0.5f));
    private readonly CircularCollider _circularCollider = new(5, 
        new TestKinematicObject(100, Vector2.Zero, Vector2.Zero), 
        new Material(0.5f));
    private readonly RectangularCollider _rectangularCollider = new(new Vector2(2, 2), new Vector2(-2, -2),
        new TestKinematicObject(100, Vector2.Zero, Vector2.Zero),
        new Material(0.5f));

    [Fact]
    private void ConvexCollider_Constructor()
    {
        Assert.Equal(600, _collider1.Inertia);
    }

    [Fact]
    private void ConvexCollider_NearsWithMethod()
    {
        _collider2.Parent.Position = new Vector2(5, 5);
        Assert.True(_collider1.NearsWith(_collider2));
    }

    [Fact]
    private void ConvexCollider_IntersectsWithPointMethod()
    {
        Assert.Equal(new PointCollision(true), _collider1.IntersectsWith(new Vector2(0, 0)));
        Assert.Equal(new PointCollision(true), _collider1.IntersectsWith(new Vector2(0, -3)));
        Assert.Equal(new PointCollision(false), _collider1.IntersectsWith(new Vector2(0, 5)));
    }

    [Fact]
    private void ConvexCollider_IntersectsWithCircularMethod()
    {
        _circularCollider.Parent.Position = new Vector2(-2, 0);
        Assert.NotNull(_collider1.IntersectsWith(_circularCollider));
        Assert.IsType<PhysicsCollision>(_collider1.IntersectsWith(_circularCollider));
        AssertExtensions.Equal(new PhysicsCollision(_collider1, _circularCollider, 
                [new(-3, 0)], new Vector2(6, 0)),
            (PhysicsCollision)_collider1.IntersectsWith(_circularCollider)!);
        _circularCollider.Parent.Position = new Vector2(6, 0);
        Assert.NotNull(_collider2.IntersectsWith(_circularCollider));
        Assert.IsType<PhysicsCollision>(_collider2.IntersectsWith(_circularCollider));
        AssertExtensions.Equal(
            new PhysicsCollision(_collider2, _circularCollider, 
                [new(3, 0)], new Vector2(-2, 0)),
            (PhysicsCollision)_collider2.IntersectsWith(_circularCollider)!);
        Assert.NotNull(_collider1.IntersectsWith(_circularCollider));
        Assert.IsType<PhysicsCollision>(_collider1.IntersectsWith(_circularCollider));
        AssertExtensions.Equal(new PhysicsCollision(_collider1, _circularCollider, 
                [new(3, 0)], new Vector2(-2, 0)),
            (PhysicsCollision)_collider1.IntersectsWith(_circularCollider)!);
    }

    [Fact]
    private void ConvexCollider_IntersectsWithRectangularMethod()
    {
        _rectangularCollider.Parent.Position = new Vector2(4, 2);
        Assert.NotNull(_collider1.IntersectsWith(_rectangularCollider));
        Assert.IsType<PhysicsCollision>(_collider1.IntersectsWith(_rectangularCollider));
        AssertExtensions.Equal(new PhysicsCollision(_collider1, _rectangularCollider, 
                [new(3, 3), new(3, 0)], new Vector2(-1, 0)),
            (PhysicsCollision)_collider1.IntersectsWith(_rectangularCollider)!);
        _rectangularCollider.Parent.Position = new Vector2(3, 4);
        Assert.NotNull(_collider1.IntersectsWith(_rectangularCollider));
        Assert.IsType<PhysicsCollision>(_collider1.IntersectsWith(_rectangularCollider));
        AssertExtensions.Equal(new PhysicsCollision(_collider1, _rectangularCollider, 
                [new(3, 3), new(1, 3)], new Vector2(0, -1)),
            (PhysicsCollision)_collider1.IntersectsWith(_rectangularCollider)!);
    }

    [Fact]
    private void ConvexCollider_IntersectsWithConvexMethod()
    {
        _collider3.Parent.Position = new Vector2(-4.5, 5);
        Assert.Null(_collider1.IntersectsWith(_collider3));
        _collider3.Parent.Position = new Vector2(5, 0);
        Assert.NotNull(_collider1.IntersectsWith(_collider3));
        Assert.IsType<PhysicsCollision>(_collider1.IntersectsWith(_collider3));
        AssertExtensions.Equal(new PhysicsCollision(_collider1, _collider3, 
            [new(3, 3), new(3, -3)], new Vector2(-1, 0)),
            (PhysicsCollision)_collider1.IntersectsWith(_collider3)!);
    }

    [Fact]
    private void ConvexCollider_IsEmptyMethod()
    {
        Assert.True(_emptyCollider.IsEmpty());
        Assert.False(_collider1.IsEmpty());
    }
}