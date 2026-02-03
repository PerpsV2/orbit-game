using System;

namespace OrbitGame;

public class CircularCollider : CompactCollider
{
    public readonly ScientificDecimal Radius;

    public CircularCollider(ScientificDecimal radius)
    {
        if (radius.Negative) throw new ArgumentException("Circular collider radius cannot be negative.");
        Radius = radius;
        // Parent.Mass * Radius * Radius / 2
    }

    public override RectangularCollider GetBoundingBox() =>
        new (SD_Vector2.Zero, Radius * 2, Radius * 2);
    
    public override void CalculateInertia(ScientificDecimal mass)
    {
        Inertia = mass * Radius * Radius / 2;
    }

    public override PointCollision IntersectsWith(SD_Vector2 point, SpatialInfo spatial) =>
        new((point - spatial.Position).Magnitude() <= Radius || IsEmpty());

    protected override PhysicsCollision? IntersectsWith(
        CircularCollider collider, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial
        )
    {
        if (IsEmpty() || collider.IsEmpty()) return null;

        SD_Vector2 diffVector = incidentSpatial.Position - referenceSpatial.Position;
        ScientificDecimal distance = diffVector.Magnitude();
        if (distance <= Radius + collider.Radius)
        {
            SD_Vector2 dirVector = diffVector.Normalize();
            SD_Vector2 collisionPoint = dirVector * Radius;
            SD_Vector2 penetrationVector = -dirVector * (Radius + collider.Radius - distance);
            return new PhysicsCollision(referenceSpatial, incidentSpatial, [collisionPoint], penetrationVector);
        }

        return null;
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider, SpatialInfo referenceSpatial,
        SpatialInfo incidentSpatial)
        => collider.IntersectsWith(this, incidentSpatial, referenceSpatial);

    protected override PhysicsCollision? IntersectsWith(RectangularCollider collider, SpatialInfo referenceSpatial,
        SpatialInfo incidentSpatial)
        => collider.IntersectsWith(this, incidentSpatial, referenceSpatial)?.GetInverse() ?? null;

    public override bool IsEmpty() =>
        Radius == 0;
}