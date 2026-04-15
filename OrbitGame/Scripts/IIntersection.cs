using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

public interface IIntersection;

/// <summary>
/// Contains information about a collision between a collider and a point.
/// </summary>
/// <param name="intersects">Whether the point and the collider intersect</param>
public readonly struct PointCollision(bool intersects = true)
    : IIntersection
{
    public readonly bool Intersects = intersects;
}

/// <summary>
/// Contains information about a physics collision between two colliders.
/// </summary>
/// <param name="reference">SpatialInfo of the reference collider</param>
/// <param name="incident">SpatialInfo of the incident collider</param>
/// <param name="manifold">Collision manifold with respect to the reference</param>
/// <param name="penetrationVector">Minimum penetration vector with respect to the reference</param>
public readonly struct PhysicsCollision(SpatialInfo reference, SpatialInfo incident, 
    HashSet<Vec2Double> manifold, Vec2Double penetrationVector)
    : IIntersection
{
    public readonly SpatialInfo Reference = reference;
    public readonly SpatialInfo Incident = incident;
    public readonly HashSet<Vec2Double> CollisionManifold = manifold;
    public readonly Vec2Double PenetrationVector = penetrationVector;

    public static PhysicsCollision CreateUnresolvable(SpatialInfo reference, SpatialInfo incident)
    {
        return new(reference, incident, [], Vec2Double.Zero);
    }
    
    public PhysicsCollision GetInverse()
    {
        SpatialInfo reference = Reference;
        SpatialInfo incident = Incident;
        Vec2Double penetrationVector = PenetrationVector;
        HashSet<Vec2Double> newManifold = CollisionManifold
            .Select(v => v + (Vec2Double)(reference.Position - incident.Position) + penetrationVector)
            .ToHashSet();
        return new(
            Incident,
            Reference,
            newManifold,
            -penetrationVector
        );
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"({Incident}, {Reference}, {CollisionManifold}, {PenetrationVector})";
    }
}