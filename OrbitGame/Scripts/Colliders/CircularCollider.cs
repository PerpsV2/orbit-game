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
        _boundingBox = new(DoubleVec2.Zero, Radius * 2, Radius * 2);
    }
    
    public override SDecimal CalculateInertia(SDecimal mass)
        => Inertia = mass * Radius * Radius / 2;

    protected override BoundingBox GetBoundingBox(double angle) =>
        _boundingBox;
    
    public override PointCollision IntersectsWith(Vec2<SDecimal> point, SpatialInfo spatial) =>
        new((point - spatial.Position).Magnitude() <= Radius && !IsEmpty());

    protected override PhysicsCollision? IntersectsWith(
        CircularCollider collider, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial
        )
    {
        if (IsEmpty() || collider.IsEmpty()) return null;

        DoubleVec2 diffVector = (DoubleVec2)(incidentSpatial.Position - referenceSpatial.Position);
        double distance = diffVector.Magnitude();
        if (distance == 0) return PhysicsCollision.CreateUnresolvable(referenceSpatial, incidentSpatial);
        if (distance <= Radius + collider.Radius)
        {
            DoubleVec2 dirVector = diffVector.Normalize();
            DoubleVec2 collisionPoint = dirVector * Radius;
            DoubleVec2 penetrationVector = -dirVector * (Radius + collider.Radius - distance);
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