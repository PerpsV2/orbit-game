namespace OrbitGame;

/// <summary>
/// A standard finite-sized collider with a parent object at its origin
/// </summary>
public abstract class CompactCollider(KinematicObject parent) : ICollider
{
    public readonly KinematicObject Parent = parent;
    public bool Fixed;
    
    public abstract RectangularCollider GetBoundingBox();
    
    public bool NearsWith(ICollider collider)
    {
        switch (collider)
        {
            case CompactCollider c: 
                return GetBoundingBox().IntersectsWith(c.GetBoundingBox());
            default: throw new NotSupportedException();
        }
    }

    public abstract bool IntersectsWith(Vector2 point);
    public abstract bool IntersectsWith(CircularCollider collider);
    public abstract bool IntersectsWith(ConvexCollider collider);
    public abstract bool IntersectsWith(RectangularCollider collider);
    
    public bool IntersectsWith(ICollider collider)
    {
        switch (collider)
        {
            case CircularCollider c: return IntersectsWith(c);
            case ConvexCollider c: return IntersectsWith(c);
            case RectangularCollider c : return IntersectsWith(c);
            default: throw new NotSupportedException();
        }
    }

    public abstract void CollidesWith(ICollider collider);

    public void CollidesWith(Body body)
    {
        if (body.Collider == null) return;
        CollidesWith(body.Collider);
    }
    
    public abstract bool IsEmpty();
}