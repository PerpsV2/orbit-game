using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

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
public readonly struct PhysicsCollision(SpatialInfo reference, SpatialInfo incident, HashSet<SD_Vector2> manifold, SD_Vector2 penetrationVector)
    : IIntersection
{
    public readonly SpatialInfo Reference = reference;
    public readonly SpatialInfo Incident = incident;
    public readonly HashSet<SD_Vector2> CollisionManifold = manifold;
    public readonly SD_Vector2 PenetrationVector = penetrationVector;

    public PhysicsCollision GetInverse()
    {
        SD_Vector2 incidentPosition = Incident.Position;
        SD_Vector2 referencePosition = Reference.Position;
        return new(
            Incident,
            Reference,
            CollisionManifold.Select(v => v - incidentPosition + referencePosition).ToHashSet(),
            -PenetrationVector
        );
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"({Incident}, {Reference}, {CollisionManifold}, {PenetrationVector})";
    }
}