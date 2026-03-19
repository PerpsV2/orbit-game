using System;

namespace OrbitGame;

/// <summary>
/// In-game CompactCollider with only a radius.
/// </summary>
public class CircularCollider : CompactCollider
{
    public readonly SDecimal Radius;
    private readonly BoundingBox _defaultBoundingBox;

    public CircularCollider(SDecimal radius)
    {
        if (radius.Negative) throw new ArgumentException("Circular collider radius cannot be negative.");
        Radius = radius;
        // TODO: fix bounding box inaccuracies
        _defaultBoundingBox = new(DVector2<SDecimal>.Zero, Radius * 2.1, Radius * 2.1);
    }
    
    public override SDecimal CalculateInertia(SDecimal mass)
    {
        return Inertia = mass * Radius * Radius / 2;
    }

    protected override BoundingBox GetBoundingBox(double angle) =>
        _defaultBoundingBox;

    public override PointCollision IntersectsWith(DVector2<SDecimal> point, SpatialInfo spatial) =>
        new((point - spatial.Position).Magnitude() <= Radius && !IsEmpty());

    protected override PhysicsCollision? IntersectsWith(
        CircularCollider collider, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial
        )
    {
        if (IsEmpty() || collider.IsEmpty()) return null;

        DVector2<SDecimal> diffVector = incidentSpatial.Position - referenceSpatial.Position;
        SDecimal distance = diffVector.Magnitude();
        if (distance == 0) return PhysicsCollision.CreateUnresolvable(referenceSpatial, incidentSpatial);
        if (distance <= Radius + collider.Radius)
        {
            DVector2<SDecimal> dirVector = diffVector.Normalize();
            DVector2<SDecimal> collisionPoint = dirVector * Radius;
            DVector2<SDecimal> penetrationVector = -dirVector * (Radius + collider.Radius - distance);
            return new PhysicsCollision(referenceSpatial, incidentSpatial, [collisionPoint], penetrationVector);
        }

        return null;
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider, SpatialInfo referenceSpatial,
        SpatialInfo incidentSpatial)
        => collider.IntersectsWith(this, incidentSpatial, referenceSpatial)?.GetInverse();

    public override bool IsEmpty() =>
        Radius == 0;
}