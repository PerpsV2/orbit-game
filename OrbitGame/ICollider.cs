using System.ComponentModel;
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

/// <summary>
/// Contains information about a collision between two compact colliders.
/// Values are relative to the reference collider's parent without respect for angle
/// </summary>
public readonly struct PhysicsCollision(CompactCollider reference, CompactCollider incident, Vector2[] manifold, Vector2 penetrationVector)
    : IIntersection, IFormattable
{
    public readonly CompactCollider Reference = reference;
    public readonly CompactCollider Incident = incident;
    public readonly Vector2[] CollisionManifold = manifold;
    public readonly Vector2 PenetrationVector = penetrationVector;
    
    public ScientificDecimal Restitution =>
        (Reference.Material.RestitutionCoefficient + Incident.Material.RestitutionCoefficient) / 2;

    public PhysicsCollision GetInverse()
    {
        Vector2[] newManifold = new Vector2[CollisionManifold.Length];
        for (int i = 0; i < CollisionManifold.Length; ++i)
            newManifold[i] = CollisionManifold[i] + PenetrationVector + Reference.Position - Incident.Position;
        
        return new PhysicsCollision(
            Incident, Reference, newManifold, -PenetrationVector
        );
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"({Incident}, {Reference}, {CollisionManifold}, {PenetrationVector})";
    }
}