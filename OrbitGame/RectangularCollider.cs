namespace OrbitGame;

public class RectangularCollider(
    ScientificDecimal top,
    ScientificDecimal right, 
    ScientificDecimal bottom,
    ScientificDecimal left, 
    KinematicObject parent) 
    : CompactCollider(parent), ICollider
{
    // Values are the signed ordinates of the vertex points of the collider
    public readonly ScientificDecimal Top = top;
    public readonly ScientificDecimal Right = right;
    public readonly ScientificDecimal Bottom = bottom;
    public readonly ScientificDecimal Left = left;

    public Vector2 TopRight => new(Right, Top);
    public Vector2 TopLeft => new(Left, Top);
    public Vector2 BottomRight => new(Right, Bottom);
    public Vector2 BottomLeft => new(Left, Bottom);

    public RectangularCollider(Vector2 topRight, Vector2 bottomLeft, KinematicObject parent)
        : this(topRight.Y, topRight.X, bottomLeft.Y, bottomLeft.X, parent) { }

    public override RectangularCollider GetBoundingBox() => this;

    public override bool IntersectsWith(Vector2 point)
    {
        if (IsEmpty()) return false;
        point -= Parent.Position;
        return point.Y < Top && point.Y > Bottom && point.X < Right && point.X > Left;
    }

    public override bool IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return false;
        Vector2 closestPoint = new(Utils.Clamp(collider.Position.X, Position.X - Left, Position.X - Right),
            Utils.Clamp(collider.Position.Y, Position.Y - Bottom, Position.Y - Top));
        return (collider.Position - closestPoint).Magnitude() <= collider.Radius;
    }

    public override bool IntersectsWith(ConvexCollider collider)
        => collider.IntersectsWith(this);

    public override bool IntersectsWith(RectangularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return false;
        Vector2 posDiff = collider.Position - Position;
        return Utils.IntervalIntersects(Top, Bottom, collider.Top + posDiff.Y, collider.Bottom + posDiff.Y) &&
               Utils.IntervalIntersects(Left, Right, collider.Left + posDiff.X, collider.Right + posDiff.X);
    }

    public override void CollidesWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty() =>
        Top == Bottom || Left == Right;
}