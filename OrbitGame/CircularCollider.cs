namespace OrbitGame;

public class CircularCollider
    : CompactCollider, ICollider
{
    public readonly ScientificDecimal Radius;

    public CircularCollider(ScientificDecimal radius, KinematicObject parent) 
        : base(parent)
    {
        if (radius.Negative) throw new ArgumentException();
        Radius = radius;
    }

    public override RectangularCollider GetBoundingBox() =>
        new (Radius, Radius, Radius, Radius, Parent);

    public override bool IntersectsWith(Vector2 point) =>
        (point - Position).Magnitude() <= Radius || IsEmpty();

    public override Collision IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return Collision.None;
        Vector2 displacementVector = Position - collider.Position;
        ScientificDecimal distance = displacementVector.Magnitude();
        if (distance <= collider.Radius + Radius)
            return new Collision(displacementVector.Normalize() * (collider.Radius + Radius - distance));

        return Collision.None;
    }

    public override Collision IntersectsWith(ConvexCollider collider) 
    {
        return collider.IntersectsWith(this).GetInverse();
    }

    public override Collision IntersectsWith(RectangularCollider collider)
    {
        return collider.IntersectsWith(this).GetInverse();
    }

    public override void CollidesWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty() =>
        Radius == 0;
}