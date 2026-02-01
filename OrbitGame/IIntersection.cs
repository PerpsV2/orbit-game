using System;
using System.Collections.Generic;

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
public readonly struct PhysicsCollision(CompactCollider reference, CompactCollider incident, HashSet<SD_Vector2> manifold, SD_Vector2 penetrationVector)
{
    /*public readonly CompactCollider Reference = reference;
    public readonly CompactCollider Incident = incident;
    public readonly HashSet<SD_Vector2> CollisionManifold = manifold;
    public readonly SD_Vector2 PenetrationVector = penetrationVector;
    
    public ScientificDecimal Restitution =>
        (Reference.Parent.Material.RestitutionCoefficient + Incident.Parent.Material.RestitutionCoefficient) / 2;

    public PhysicsCollision GetInverse()
    {
        HashSet<SD_Vector2> newManifold = new();
        foreach (SD_Vector2 point in CollisionManifold)
            newManifold.Add(point + PenetrationVector + Reference.Parent.Position - Incident.Parent.Position);
        
        return new PhysicsCollision(
            Incident, Reference, newManifold, -PenetrationVector
        );
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"({Incident}, {Reference}, {CollisionManifold}, {PenetrationVector})";
    }*/
}