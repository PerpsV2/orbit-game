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
        point -= Parent.Position;
        return point.Y < _top && point.Y > _bottom && point.X < _right && point.X > _left;
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
        return Utils.IntervalIntersects(_top, _bottom,
                   collider._top + collider.Parent.Position.Y - Parent.Position.Y,
                   collider._bottom + collider.Parent.Position.Y - Parent.Position.Y) &&
               Utils.IntervalIntersects(_left, _right,
                   collider._left + collider.Parent.Position.X - Parent.Position.X,
                   collider._right + collider.Parent.Position.X - Parent.Position.X);
    }

    public override void CollidesWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty() =>
        _top == _bottom || _left == _right;
}