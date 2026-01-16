namespace OrbitGame;

public interface IIntersection;

public readonly struct Collision(Vector2 penetrationVector, bool intersects = true)
    : IIntersection
{
    public static Collision None = new Collision(Vector2.Zero, false);
    
    public readonly bool Intersects = intersects;
    public readonly Vector2 PenetrationVector = penetrationVector;

    public Collision GetInverse()
    {
        return new Collision(
            -PenetrationVector,
            Intersects
        );
    }
}

public interface ICollider
{
    public bool NearsWith(object? obj);
    public bool IntersectsWith(Vector2 point);
    public IIntersection IntersectsWith(object? obj);
    public void CollidesWith(object? obj);
    public bool IsEmpty();
}