namespace OrbitGame.Tests;

public class RectangularCollider_Tests
{
    private class TestKinematicObject(ScientificDecimal mass, Vector2 position, Vector2 velocity)
        : KinematicObject("", mass, position, velocity);
    
    private readonly RectangularCollider _collider1 = new (new Vector2(2, 2), new Vector2(-2, -2), 
        new TestKinematicObject(100, Vector2.Zero, Vector2.Zero), new Material(0.5f));
    private readonly RectangularCollider _collider2 = new (new Vector2(3, 3), new Vector2(-3, -3), 
        new TestKinematicObject(100, Vector2.Zero, Vector2.Zero), new Material(0.5f));
    private readonly RectangularCollider _emptyCollider = new(new Vector2(0, 3), new Vector2(0, -3), 
        new TestKinematicObject(100, Vector2.Zero, Vector2.Zero), new Material(0.5f));

    private readonly CircularCollider _circularCollider = new (3, new TestKinematicObject(100, 
        Vector2.Zero, Vector2.Zero), new Material(0.5f));
    
    [Fact]
    public void RectangularCollider_Constructor()
    {
        _collider1.Inertia = 266 + 2d / 3;
    }
    
    [Fact]
    public void RectangularCollider_NearsWithMethod()
    {
        _collider2.Parent.Position = new Vector2(3, 0);
        Assert.True(_collider1.NearsWith(_collider2));
        _collider2.Parent.Position = new Vector2(5, 0);
        Assert.True(_collider1.NearsWith(_collider2));
        _collider2.Parent.Position = new Vector2(5, 6);
        Assert.False(_collider1.NearsWith(_collider2));
    }

    [Fact]
    public void RectangularCollider_IntersectsWithPointMethod()
    {
        Assert.Equal(new PointCollision(true), _collider1.IntersectsWith(new Vector2(0, 0)));
        Assert.Equal(new PointCollision(true), _collider1.IntersectsWith(new Vector2(0, 2)));
        Assert.Equal(new PointCollision(false), _collider1.IntersectsWith(new Vector2(-3, 2)));
    }

    [Fact]
    public void RectangularCollider_IntersectsWithCircularMethod()
    {
        _circularCollider.Parent.Position = new Vector2(4, 0);
        Assert.NotNull(_collider1.IntersectsWith(_circularCollider));
        Assert.IsType<PhysicsCollision>(_collider1.IntersectsWith(_circularCollider));
        AssertExtensions.Equal(new PhysicsCollision(_collider1, _circularCollider,
            [new Vector2(2, 0)], new Vector2(-1, 0)),
            (PhysicsCollision)_collider1.IntersectsWith(_circularCollider)!);
        _circularCollider.Parent.Position = new Vector2(4.5, 4.5);
        Assert.Null(_collider1.IntersectsWith(_circularCollider));
    }
    
    [Fact]
    public void RectangularCollider_IntersectsWithRectangularMethod()
    {
        _collider2.Parent.Position = new Vector2(4.5, 3);
        Assert.NotNull(_collider1.IntersectsWith(_collider2));
        Assert.IsType<PhysicsCollision>(_collider1.IntersectsWith(_collider2));
        AssertExtensions.Equal(new PhysicsCollision(_collider1, _collider2,
            [new Vector2(2, 2), new Vector2(2, 0)], new Vector2(-1, 0)),
        (PhysicsCollision)_collider1.IntersectsWith(_collider2)!);
        _collider2.Parent.Position = new Vector2(5, 0);
        Assert.NotNull(_collider2.IntersectsWith(_collider1));
        Assert.IsType<PhysicsCollision>(_collider2.IntersectsWith(_collider1));
        AssertExtensions.Equal(new PhysicsCollision(_collider2, _collider1,
                [new Vector2(-3, 2), new Vector2(-3, -2)], Vector2.Zero),
            (PhysicsCollision)_collider2.IntersectsWith(_collider1)!);
        _collider2.Parent.Position = new Vector2(5.5, 0);
        Assert.Null(_collider1.IntersectsWith(_collider2));
    }
    
    [Fact]
    public void RectangularCollider_IsEmpty()
    {
        Assert.False(_collider1.IsEmpty());
        Assert.True(_emptyCollider.IsEmpty());
    }
}