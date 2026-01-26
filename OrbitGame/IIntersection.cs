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
public readonly struct PhysicsCollision(CompactCollider reference, CompactCollider incident, HashSet<Vector2> manifold, Vector2 penetrationVector)
    : IIntersection, IFormattable
{
    public readonly CompactCollider Reference = reference;
    public readonly CompactCollider Incident = incident;
    public readonly HashSet<Vector2> CollisionManifold = manifold;
    public readonly Vector2 PenetrationVector = penetrationVector;
    
    public ScientificDecimal Restitution =>
        (Reference.Material.RestitutionCoefficient + Incident.Material.RestitutionCoefficient) / 2;

    public PhysicsCollision GetInverse()
    {
        HashSet<Vector2> newManifold = new();
        foreach (Vector2 point in CollisionManifold)
            newManifold.Add(point + PenetrationVector + Reference.Position - Incident.Position);
        
        return new PhysicsCollision(
            Incident, Reference, newManifold, -PenetrationVector
        );
    }

    public string ToString(string? format, IFormatProvider? formatProvider)
    {
        return $"({Incident}, {Reference}, {CollisionManifold}, {PenetrationVector})";
    }
}