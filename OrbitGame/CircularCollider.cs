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
    
    public static explicit operator RectangularCollider(CircularCollider value)
        => value.GetBoundingBox();

    public override PointCollision IntersectsWith(SD_Vector2 position, SD_Vector2 point) =>
        new((point - position).Magnitude() <= Radius || IsEmpty());

    protected override PhysicsCollision? IntersectsWith(CircularCollider collider, SD_Vector2 relPosition, double relAngle)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;
        
        ScientificDecimal distance = relPosition.Magnitude();
        if (distance <= Radius + collider.Radius)
        {
            SD_Vector2 dirVector = relPosition.Normalize();
            SD_Vector2 collisionPoint = dirVector * Radius;
            SD_Vector2 penetrationVector = -dirVector * (Radius + collider.Radius - distance);
            return new PhysicsCollision(this, collider, [collisionPoint], penetrationVector);
        }
        
        return null;
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider, SD_Vector2 relPosition, double relAngle) 
        => throw new NotImplementedException(); //collider.IntersectsWith(this, -relPosition, -relAngle)?.GetInverse() ?? null);

    protected override PhysicsCollision? IntersectsWith(RectangularCollider collider, SD_Vector2 relPosition, double relAngle)
        => throw new NotImplementedException(); //collider.IntersectsWith(this, -relPosition, -relAngle)?.GetInverse() ?? null;

    public override bool IsEmpty() =>
        Radius == 0;
}