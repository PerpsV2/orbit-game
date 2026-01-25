namespace OrbitGame.Tests;

public class CircularCollider_Tests
{
    private class TestKinematicObject(ScientificDecimal mass, Vector2 position, Vector2 velocity)
        : KinematicObject("", mass, position, velocity);
    
    private readonly CircularCollider _collider1 = new (5, new TestKinematicObject(100, 
            Vector2.Zero, Vector2.Zero), new Material(0.5f));
    private readonly CircularCollider _collider2 = new (3, new TestKinematicObject(100, 
        Vector2.Zero, Vector2.Zero), new Material(0.5f));
    private readonly CircularCollider _emptyCollider = new(0, new TestKinematicObject(100,
        Vector2.Zero, Vector2.Zero), new Material(0.5f));
    
    
    [Fact]
    public void CircularCollider_Constructor()
    {
        Assert.Equal(1250, _collider1.Inertia);
    }

    [Fact]
    public void CircularCollider_NearsWith()
    {
        _collider2.Parent.Position = new Vector2(4, 7);
        Assert.True(_collider1.NearsWith(_collider2));
        _collider2.Parent.Position = new Vector2(4, 10);
        Assert.False(_collider1.NearsWith(_collider2));
    }

    [Fact]
    public void CircularCollider_IntersectsWithPointMethod()
    {
        Assert.Equal(new PointCollision(true), _collider1.IntersectsWith(new Vector2(1, 0)));
        Assert.Equal(new PointCollision(true), _collider1.IntersectsWith(new Vector2(5, 0)));
        Assert.Equal(new PointCollision(false), _collider1.IntersectsWith(new Vector2(10, 0)));
    }
    
    [Fact]
    public void CircularCollider_IntersectsWithCircularMethod()
    {
        _collider2.Parent.Position = new Vector2(4, 4);
        Assert.NotNull(_collider1.IntersectsWith(_collider2));
        Assert.IsType<PhysicsCollision>(_collider1.IntersectsWith(_collider2));
        AssertExtensions.Equal(new PhysicsCollision(_collider1, _collider2, 
                [new Vector2(5 * Math.Cos(Math.PI / 4), 5 * Math.Sin(Math.PI / 4))], 
                new Vector2(4 - 8 * Math.Cos(Math.PI / 4), 4 - 8 * Math.Sin(Math.PI / 4))),
            (PhysicsCollision)_collider1.IntersectsWith(_collider2)!);
        _collider2.Parent.Position = new Vector2(4, 7);
        Assert.Null(_collider1.IntersectsWith(_collider2));
    }

    [Fact]
    public void CircularCollider_IsEmptyMethod()
    {
        Assert.False(_collider1.IsEmpty());
        Assert.True(_emptyCollider.IsEmpty());
    }
}