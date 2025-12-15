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

    public override bool IntersectsWith(CircularCollider collider) =>
        (collider.Position - Position).Magnitude() <= collider.Radius + Radius || IsEmpty() || collider.IsEmpty();

    public override bool IntersectsWith(ConvexCollider collider) =>
        collider.IntersectsWith(this);

    public override bool IntersectsWith(RectangularCollider collider) =>
        collider.IntersectsWith(this);
 
    public override void CollidesWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty() =>
        Radius == 0;
}