namespace OrbitGame;

public class RectangularCollider(
    ScientificDecimal top,
    ScientificDecimal right, 
    ScientificDecimal bottom,
    ScientificDecimal left, 
    KinematicObject parent) 
    : CompactCollider(parent), ICollider
{
    private readonly ScientificDecimal _top = top;
    private readonly ScientificDecimal _right = right;
    private readonly ScientificDecimal _bottom = bottom;
    private readonly ScientificDecimal _left = left;

    public RectangularCollider(Vector2 topRight, Vector2 bottomLeft, KinematicObject parent)
        : this(topRight.Y, topRight.X, bottomLeft.Y, bottomLeft.X, parent) { }

    public override RectangularCollider GetBoundingBox() => this;

    public override bool IntersectsWith(Vector2 point)
    {
        throw new NotImplementedException();
    }

    public override bool IntersectsWith(CircularCollider collider)
    {
        throw new NotImplementedException();
    }

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

    public override bool IsEmpty()
    {
        throw new NotImplementedException();
    }
}