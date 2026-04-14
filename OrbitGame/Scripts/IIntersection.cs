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
    HashSet<DoubleVec2> manifold, DoubleVec2 penetrationVector)
    : IIntersection
{
    public readonly SpatialInfo Reference = reference;
    public readonly SpatialInfo Incident = incident;
    public readonly HashSet<DoubleVec2> CollisionManifold = manifold;
    public readonly DoubleVec2 PenetrationVector = penetrationVector;

    public static PhysicsCollision CreateUnresolvable(SpatialInfo reference, SpatialInfo incident)
    {
        return new(reference, incident, [], DoubleVec2.Zero);
    }
    
    public PhysicsCollision GetInverse()
    {
        SpatialInfo reference = Reference;
        SpatialInfo incident = Incident;
        DoubleVec2 penetrationVector = PenetrationVector;
        HashSet<DoubleVec2> newManifold = CollisionManifold
            .Select(v => v + (DoubleVec2)(reference.Position - incident.Position) + penetrationVector)
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