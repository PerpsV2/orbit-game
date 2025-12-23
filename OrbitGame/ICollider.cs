namespace OrbitGame;

public readonly struct Collision(Vector2 penetrationVector, bool intersects = true)
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
    public bool NearsWith(ICollider collider);
    public bool IntersectsWith(Vector2 point);
    public Collision IntersectsWith(ICollider collider);
    public void CollidesWith(ICollider collider);
    public bool IsEmpty();
}