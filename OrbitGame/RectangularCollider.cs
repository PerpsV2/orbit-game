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
    public readonly ScientificDecimal Top = center.Y + height / 2;
    public readonly ScientificDecimal Right = center.X + width / 2;
    public readonly ScientificDecimal Bottom = center.Y - height / 2;
    public readonly ScientificDecimal Left = center.X - width / 2;

    public Vector2 TopRight => new(Right, Top);
    public Vector2 TopLeft => new(Left, Top);
    public Vector2 BottomRight => new(Right, Bottom);
    public Vector2 BottomLeft => new(Left, Bottom);

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
            return Position.Y + Top >= rect.Position.Y + rect.Bottom && 
                   Position.Y + Bottom <= rect.Position.Y + rect.Top && 
                   Position.X + Right >= rect.Position.X + rect.Left && 
                   Position.X + Left <= rect.Position.X + rect.Right;
        // convert the other collider into a rectangular collider if it is not one
        else if (collider is CompactCollider compact)
            return NearsWith(compact.GetBoundingBox());
        else throw new ArgumentException("RectangularCollider cannot be near invalid collider type");
    }

    protected override PointCollision IntersectsWith(Vector2 point)
    {
        if (IsEmpty()) return new(false);
        point -= Parent.Position;
        return new(point.Y <= Top && point.Y >= Bottom && point.X <= Right && point.X >= Left);
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
        Top == Bottom || Left == Right;
}