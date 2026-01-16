namespace OrbitGame;

public class CircularCollider
    : CompactCollider, ICollider
{
    public readonly ScientificDecimal Radius;

    public CircularCollider(ScientificDecimal radius, KinematicObject parent, Material material) 
        : base(parent, material)
    {
        if (radius.Negative) throw new ArgumentException();
        Radius = radius;
    }

    public override RectangularCollider GetBoundingBox() =>
        new (Radius, Radius, Radius, Radius, Parent, Material);

    protected override PointCollision IntersectsWith(Vector2 point) =>
        new((point - Position).Magnitude() <= Radius || IsEmpty());

    protected override PhysicsCollision IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return PhysicsCollision.None;
        Vector2 displacementVector = Position - collider.Position;
        ScientificDecimal distance = displacementVector.Magnitude();
        if (distance <= collider.Radius + Radius)
            return new PhysicsCollision(displacementVector.Normalize() * (collider.Radius + Radius - distance
                + new ScientificDecimal(1m, -10))); // TODO: crashes if the two objects are barely touching idk why help
        return PhysicsCollision.None;
    }

    protected override PhysicsCollision IntersectsWith(ConvexCollider collider) 
        => ((PhysicsCollision)collider.IntersectsWith(this)).GetInverse();

    protected override PhysicsCollision IntersectsWith(RectangularCollider collider)
        => ((PhysicsCollision)collider.IntersectsWith(this)).GetInverse();

    protected override void CollidesWith(CompactCollider collider)
    {
        PhysicsCollision collisionInfo = IntersectsWith(collider);
        
        Vector2 penetrationVector = collisionInfo.PenetrationVector;
        ScientificDecimal mass1 = Parent.Mass;
        ScientificDecimal mass2 = collider.Parent.Mass;
        
        // apply projection method
        Parent.Position += penetrationVector * mass2 / (mass1 + mass2);
        collider.Parent.Position -= penetrationVector * mass1 / (mass1 + mass2);
        
        if (!collisionInfo.Intersects) return;
        Vector2 relativeVelocity = Parent.Velocity - collider.Parent.Velocity;
        ScientificDecimal totalRestitution = (Material.RestitutionCoefficient + collider.Material.RestitutionCoefficient) / 2;
        ScientificDecimal totalVelocity = Vector2.Dot(relativeVelocity, penetrationVector.Normalize()) * -(1 + totalRestitution);
        ScientificDecimal impulse = totalVelocity * mass1 * mass2 / (mass1 + mass2);
        
        // apply impulse along collision normal
        Parent.Velocity += penetrationVector.Normalize() * impulse / mass1;
        collider.Parent.Velocity -= penetrationVector.Normalize() * impulse / mass2;
    }

    public override bool IsEmpty() =>
        Radius == 0;
}