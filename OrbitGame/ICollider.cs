using SkiaSharp;

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
    public IIntersection? IntersectsWith(object? obj);
    
    /// <summary>
    /// Determines whether two objects intersect and executes the expected response using the intersection information
    /// </summary>
    public void CollidesWith(object? obj, SKCanvas canvas, Camera camera);
    
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
public readonly struct PhysicsCollision(CompactCollider reference, CompactCollider collider, Vector2 collisionPoint, Vector2 penetrationVector)
    : IIntersection, IFormattable
{
    public readonly CompactCollider Reference = reference;
    public readonly CompactCollider Collider = collider;
    public readonly Vector2 CollisionPoint = collisionPoint;
    public readonly Vector2 PenetrationVector = penetrationVector;

    public ScientificDecimal Mass => Reference.Parent.Mass;
    public ScientificDecimal Inertia => Reference.Inertia;
    public Vector2 Velocity => Reference.Parent.Velocity;
    public double AngularVelocity => Reference.Parent.AngularVelocity;
    public Vector2 CollisionNormal => PenetrationVector.Normalize();
    public Vector2 CollisionTangent => new(CollisionNormal.Y, -CollisionNormal.X);
    public ScientificDecimal Restitution =>
        (Reference.Material.RestitutionCoefficient + Collider.Material.RestitutionCoefficient) / 2;

    public PhysicsCollision GetInverse()
    {
        return new PhysicsCollision(
            Collider,
            Reference,
            Reference.Parent.ObjectToObjectSpace(CollisionPoint, Collider.Parent),
            Matrix3X3.Rotation(Collider.Parent.Angle - Reference.Parent.Angle) * -PenetrationVector
        );
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"({Collider}, {Reference}, {CollisionPoint}, {PenetrationVector})";
    }
}