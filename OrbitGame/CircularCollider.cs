namespace OrbitGame;

public class CircularCollider
    : CompactCollider, ICollider
{
    private readonly ScientificDecimal _radius;

    public CircularCollider(ScientificDecimal radius, KinematicObject parent) 
        : base(parent)
    {
        if (radius.Negative) throw new ArgumentException();
        _radius = radius;
    }

    public override RectangularCollider GetBoundingBox() =>
        new (_radius, _radius, _radius, _radius, Parent);

    public override bool IntersectsWith(Vector2 point) =>
        (point - Parent.Position).Magnitude() <= _radius;

    public override bool IntersectsWith(CircularCollider collider) =>
        (collider.Parent.Position - Parent.Position).Magnitude() <= collider._radius + _radius;

    public override bool IntersectsWith(ConvexCollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IntersectsWith(RectangularCollider collider)
    {
        throw new NotImplementedException();
    }
 
    public override void CollidesWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty() =>
        _radius == 0;
}