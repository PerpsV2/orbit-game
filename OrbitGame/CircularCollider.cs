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

    public override RectangularCollider GetBoundingBox() =>
        new (Vector2.Zero, Radius * 2, Radius * 2, Parent, Material);

    protected override PointCollision IntersectsWith(Vector2 point) =>
        new((point - Position).Magnitude() <= Radius || IsEmpty());

    protected override PhysicsCollision? IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;
        
        Vector2 diffVector = collider.Position - Position;
        ScientificDecimal distance = diffVector.Magnitude();
        if (distance <= Radius + collider.Radius)
        {
            Vector2 dirVector = diffVector.Normalize();
            Vector2 collisionPoint = dirVector * Radius;
            Vector2 penetrationVector = -dirVector * (Radius + collider.Radius - distance);
            return new PhysicsCollision(this, collider, [collisionPoint], penetrationVector);
        }
        
        return null;
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider) 
        => ((PhysicsCollision?)collider.IntersectsWith(this))?.GetInverse() ?? null;

    protected override PhysicsCollision? IntersectsWith(RectangularCollider collider)
        => ((PhysicsCollision?)collider.IntersectsWith(this))?.GetInverse() ?? null;

    public override bool IsEmpty() =>
        Radius == 0;
}