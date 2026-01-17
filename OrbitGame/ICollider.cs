namespace OrbitGame;

/// <summary>
/// Interface containing methods for any object on screen which provides intersection information with an object
/// </summary>
public interface ICollider
{
    /// <summary>
    /// Returns whether two objects are close to each other and collision is possible i.e. broad phase collision detection
    /// </summary>
    public bool NearsWith(object? obj);
    
    /// <summary>
    /// Returns whether two objects are intersecting and provides information about the intersection
    /// </summary>
    public IIntersection IntersectsWith(object? obj);
    
    /// <summary>
    /// Determines whether two objects intersect and executes the expected response using the intersection information
    /// </summary>
    public void CollidesWith(object? obj);
    
    /// <summary>
    /// Returns true if the collider has no area (an intersection is impossible)
    /// </summary>
    public bool IsEmpty();
}

public interface IIntersection;

public readonly struct PointCollision(bool intersects = true)
    : IIntersection
{
    public readonly bool Intersects = intersects;
}

// TODO: implement collider point field for physics collisions
public readonly struct PhysicsCollision(Vector2 collisionPoint, Vector2 penetrationVector, bool intersects = true)
    : IIntersection, IFormattable
{
    public static PhysicsCollision None = new (Vector2.Zero, Vector2.Zero, false);

    public readonly Vector2 CollisionPoint = collisionPoint;
    public readonly Vector2 PenetrationVector = penetrationVector;
    public readonly bool Intersects = intersects;

    public PhysicsCollision GetInverse(CompactCollider reference, CompactCollider collider)
    {
        return new PhysicsCollision(
            CollisionPoint + reference.Position - collider.Position,
            -PenetrationVector,
            Intersects
        );
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"({collisionPoint}, {penetrationVector}, {intersects})";
    }
}