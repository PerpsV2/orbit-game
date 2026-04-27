using System;

namespace OrbitGame;

/// <summary>
/// Game collider with only a radius.
/// </summary>
public class CircularCollider : CompactCollider
{
    /// <summary>
    /// Radius of the collider.
    /// </summary>
    public double Radius { get; }
    private readonly BoundingBox _boundingBox;

    /// <summary>
    /// Creates a circular collider from a radius.
    /// </summary>
    /// <param name="radius">The radius of the circular collider.</param>
    /// <exception cref="ArgumentException">Radius is less than zero</exception>
    public CircularCollider(double radius)
    {
        if (double.IsNegative(radius)) throw new ArgumentException("Circular collider radius cannot be negative.");
        Radius = radius;
        MaxRadius = radius;
        _boundingBox = new(Vec2Double.Zero, Radius * 2, Radius * 2);
    }
    
    public override SDecimal CalculateInertia(SDecimal mass)
        => Inertia = mass * Radius * Radius / 2;

    protected override BoundingBox GetBoundingBox() =>
        _boundingBox;

    public override PointCollision? IntersectsWith(Vec2<SDecimal> point, SpatialInfo spatial)
    {
        throw new NotImplementedException();
    }

    protected override PhysicsCollision? IntersectsWith(
        CircularCollider collider, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial
        )
    {
        if (IsEmpty() || collider.IsEmpty()) return null;

        Vec2Double diffVector = (Vec2Double)(incidentSpatial.Position - referenceSpatial.Position);
        double distance = diffVector.Magnitude();
        if (distance == 0) return PhysicsCollision.CreateUnresolvable(referenceSpatial, incidentSpatial);
        if (distance <= Radius + collider.Radius)
        {
            Vec2Double dirVector = diffVector.Normalize();
            Vec2Double collisionPoint = dirVector * Radius;
            Vec2Double penetrationVector = -dirVector * (Radius + collider.Radius - distance);
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