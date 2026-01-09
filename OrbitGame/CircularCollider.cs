namespace OrbitGame;

public class CircularCollider
    : CompactCollider, ICollider
{
    public readonly ScientificDecimal Radius;

    public CircularCollider(ScientificDecimal radius, Body parent) 
        : base(parent)
    {
        if (radius.Negative) throw new ArgumentException();
        Radius = radius;
    }

    public override RectangularCollider GetBoundingBox() =>
        new (Radius, Radius, Radius, Radius, Parent);

    public override bool IntersectsWith(Vector2 point) =>
        (point - Position).Magnitude() <= Radius || IsEmpty();

    public override Collision IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return Collision.None;
        Vector2 displacementVector = Position - collider.Position;
        ScientificDecimal distance = displacementVector.Magnitude();
        if (distance <= collider.Radius + Radius)
            return new Collision(displacementVector.Normalize() * (collider.Radius + Radius - distance
                + new ScientificDecimal(1m, -10))); // TODO: crashes if the two objects are barely touching idk why help
        return Collision.None;
    }

    public override Collision IntersectsWith(ConvexCollider collider) 
    {
        return collider.IntersectsWith(this).GetInverse();
    }

    public override Collision IntersectsWith(RectangularCollider collider)
    {
        return collider.IntersectsWith(this).GetInverse();
    }

    public override void CollidesWith(ICollider collider) => CollidesWith((CompactCollider)collider);

    public override void CollidesWith(CompactCollider collider)
    {
        Collision collisionInfo = IntersectsWith(collider);
        
        Vector2 penetrationVector = collisionInfo.PenetrationVector;
        ScientificDecimal mass1 = Parent.Mass;
        ScientificDecimal mass2 = collider.Parent.Mass;
        
        // apply projection method
        Parent.Position += penetrationVector * mass2 / (mass1 + mass2);
        collider.Parent.Position -= penetrationVector * mass1 / (mass1 + mass2);
        
        if (!collisionInfo.Intersects) return;
        Vector2 relativeVelocity = Parent.Velocity - collider.Parent.Velocity;
        ScientificDecimal totalRestitution =
            (Parent.Material.RestitutionCoefficient + collider.Parent.Material.RestitutionCoefficient) / 2;
        ScientificDecimal totalVelocity = relativeVelocity * penetrationVector.Normalize() * -(1 + totalRestitution);
        ScientificDecimal impulse = totalVelocity * mass1 * mass2 / (mass1 + mass2);
        
        // apply impulse along collision normal
        Parent.Velocity += penetrationVector.Normalize() * impulse / mass1;
        collider.Parent.Velocity -= penetrationVector.Normalize() * impulse / mass2;
    }

    public override bool IsEmpty() =>
        Radius == 0;
}