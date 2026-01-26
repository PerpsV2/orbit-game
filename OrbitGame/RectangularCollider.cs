namespace OrbitGame;

/// <summary>
/// Rectangular collider is unaffected by rotations
/// Ambiguous collision penetration vectors favour the positive x and y directions (Right and Top)
/// </summary>
public class RectangularCollider(
    Vector2 center,
    ScientificDecimal width,
    ScientificDecimal height, 
    KinematicObject parent,
    Material material) 
    : CompactCollider(parent, material), ICollider
{
    // Values are the signed ordinates of the vertex points of the collider
    private readonly ScientificDecimal _top = center.Y + height / 2;
    private readonly ScientificDecimal _right = center.X + width / 2;
    private readonly ScientificDecimal _bottom = center.Y - height / 2;
    private readonly ScientificDecimal _left = center.X - width / 2;

    public Vector2 TopRight => new(_right, _top);
    public Vector2 TopLeft => new(_left, _top);
    public Vector2 BottomRight => new(_right, _bottom);
    public Vector2 BottomLeft => new(_left, _bottom);

    public RectangularCollider(Vector2 topRight, Vector2 bottomLeft, KinematicObject parent, Material material)
        : this(
            (topRight + bottomLeft) / 2, 
            topRight.X - bottomLeft.X, 
            topRight.Y - bottomLeft.Y, 
            parent, material
            ) 
    { }

    public override RectangularCollider GetBoundingBox() => this;

    protected override bool NearsWith(ICollider collider)
    {
        if (collider is RectangularCollider rect)
            return Position.Y + _top >= rect.Position.Y + rect._bottom && 
                   Position.Y + _bottom <= rect.Position.Y + rect._top && 
                   Position.X + _right >= rect.Position.X + rect._left && 
                   Position.X + _left <= rect.Position.X + rect._right;
        // convert the other collider into a rectangular collider if it is not one
        else if (collider is CompactCollider compact)
            return NearsWith(compact.GetBoundingBox());
        else throw new ArgumentException("RectangularCollider cannot be near invalid collider type");
    }

    protected override PointCollision IntersectsWith(Vector2 point)
    {
        if (IsEmpty()) return new(false);
        point -= Parent.Position;
        return new(point.Y <= _top && point.Y >= _bottom && point.X <= _right && point.X >= _left);
    }
    
    protected override PhysicsCollision? IntersectsWith(CircularCollider collider)
    {
        throw new NotImplementedException();
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider)
        => ((PhysicsCollision?)collider.IntersectsWith(this))?.GetInverse() ?? null;

    protected override PhysicsCollision? IntersectsWith(RectangularCollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty() =>
        _top == _bottom || _left == _right;
}