namespace OrbitGame;

/// <summary>
/// Rectangular collider is unaffected by rotations
/// Ambiguous collision penetration vectors favour the positive x and y directions (Right and Top)
/// </summary>
public class RectangularCollider(
    ScientificDecimal top,
    ScientificDecimal right, 
    ScientificDecimal bottom,
    ScientificDecimal left, 
    KinematicObject parent,
    Material material) 
    : CompactCollider(parent, material), ICollider
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

    public RectangularCollider(Vector2 topRight, Vector2 bottomLeft, KinematicObject parent, Material material)
        : this(topRight.Y, topRight.X, bottomLeft.Y, bottomLeft.X, parent, material) { }

    protected override RectangularCollider GetBoundingBox() => this;

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