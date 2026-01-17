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

    public RectangularCollider(Vector2 topRight, Vector2 bottomLeft, Body parent, Material material)
        : this(topRight.Y, topRight.X, bottomLeft.Y, bottomLeft.X, parent, material) { }

    public override RectangularCollider GetBoundingBox() => this;

    protected override PointCollision IntersectsWith(Vector2 point)
    {
        if (IsEmpty()) return new(false);
        point -= Parent.Position;
        return new(point.Y < Top && point.Y > Bottom && point.X < Right && point.X > Left);
    }

    // TODO: Implement rectangle-circle and rectangle-rectangle collisions
    protected override PhysicsCollision IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return PhysicsCollision.None;
        Vector2 closestPoint = new(Utils.Clamp(collider.Position.X, Position.X - Left, Position.X - Right),
            Utils.Clamp(collider.Position.Y, Position.Y - Bottom, Position.Y - Top));
        if ((collider.Position - closestPoint).Magnitude() <= collider.Radius)
            return new PhysicsCollision(Vector2.Zero, Vector2.Zero);
        return PhysicsCollision.None;
    }

    protected override PhysicsCollision IntersectsWith(ConvexCollider collider)
        => ((PhysicsCollision)collider.IntersectsWith(this)).GetInverse(this, collider);

    protected override PhysicsCollision IntersectsWith(RectangularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return PhysicsCollision.None;
        Vector2 posDiff = collider.Position - Position;
        if (Utils.IntervalIntersects(Top, Bottom, collider.Top + posDiff.Y, collider.Bottom + posDiff.Y) &&
            Utils.IntervalIntersects(Left, Right, collider.Left + posDiff.X, collider.Right + posDiff.X))
        {
            List<Vector2> cardinalPenetrationDepths =
            [
                new(collider.Right - Left, 0),
                new(Right - collider.Left, 0),
                new(0, collider.Top - Bottom),
                new(0, Top - collider.Bottom)
            ];

            cardinalPenetrationDepths.Sort();
            
            return new PhysicsCollision(Vector2.Zero, cardinalPenetrationDepths[0]);
        }

        return PhysicsCollision.None;
    }

    protected override void CollidesWith(CompactCollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty() =>
        Top == Bottom || Left == Right;
}