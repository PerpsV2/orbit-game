namespace OrbitGame;

public class CircularCollider
    : CompactCollider, ICollider
{
    public readonly ScientificDecimal Radius;

    public CircularCollider(ScientificDecimal radius, KinematicObject parent, Material material) 
        : base(parent, material)
    {
        if (radius.Negative) throw new ArgumentException("Circular collider radius cannot be negative.");
        Radius = radius;
        Inertia = Parent.Mass * Radius * Radius / 2;
    }

    protected override RectangularCollider GetBoundingBox() =>
        new (Radius, Radius, Radius, Radius, Parent, Material);

    protected override PointCollision IntersectsWith(Vector2 point) =>
        new((point - Position).Magnitude() <= Radius || IsEmpty());

    protected override PhysicsCollision? IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;
        Vector2 displacementVector = Position - collider.Position;
        ScientificDecimal distance = displacementVector.Magnitude();
        if (distance <= collider.Radius + Radius)
            return new PhysicsCollision(this, collider, Vector2.Zero, 
                displacementVector.Normalize() * (collider.Radius + Radius - distance));
        return null;
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider) 
        => ((PhysicsCollision?)collider.IntersectsWith(this))?.GetInverse() ?? null;

    protected override PhysicsCollision? IntersectsWith(RectangularCollider collider)
        => ((PhysicsCollision?)collider.IntersectsWith(this))?.GetInverse() ?? null;

    public override bool IsEmpty() =>
        Radius == 0;
}