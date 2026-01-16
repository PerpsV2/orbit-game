namespace OrbitGame;

/// <summary>
/// A standard finite-sized collider with a parent object at its origin
/// </summary>
/// <remarks>
/// Classes which inherit from CompactCollider should all be able to collide with each other.
/// To prevent unnecessary code, there should only be one implementation for collisions between two types of colliders
/// located in the more 'complex' collider.
/// Complexity of colliders follows this order
/// Convex > Rectangular > Circular
/// </remarks>
public abstract class CompactCollider(KinematicObject parent, Material material) : ICollider
{
    public readonly Material Material = material;
    public readonly KinematicObject Parent = parent;
    public readonly Matrix3X3 InertiaMatrix = Matrix3X3.Identity();
    public readonly Matrix3X3 InertiaInvMatrix = Matrix3X3.Identity();
        
    public Vector2 Position => Parent.Position;
    public bool Fixed;
    
    /// <summary>
    /// Method to return the axis-aligned rectangular collider which best fits the set of points in the collider
    /// </summary>
    public abstract RectangularCollider GetBoundingBox();
    
    
    public bool NearsWith(object? obj)
    {
        if (obj is ICollider col) return NearsWith(col);
        throw new ArgumentException();
    }
    
    public bool NearsWith(ICollider collider)
    {
        switch (collider)
        {
            case CompactCollider c: 
                return GetBoundingBox().IntersectsWith(c.GetBoundingBox()).Intersects;
            default: throw new NotSupportedException();
        }
    }
    
    
    public IIntersection IntersectsWith(object? obj)
    {
        switch (obj)
        {
            case CircularCollider c: return IntersectsWith(c);
            case ConvexCollider c: return IntersectsWith(c);
            case RectangularCollider c : return IntersectsWith(c);
            case PointCollision c : return IntersectsWith(c);
            default: throw new ArgumentException();
        }
    }
    
    protected abstract PointCollision IntersectsWith(Vector2 point);
    protected PhysicsCollision IntersectsWith(ICollider collider)
    {
        switch (collider)
        {
            case CircularCollider c: return IntersectsWith(c);
            case ConvexCollider c: return IntersectsWith(c);
            case RectangularCollider c : return IntersectsWith(c);
            default: throw new ArgumentException();
        }
    }

    protected abstract PhysicsCollision IntersectsWith(CircularCollider collider);
    protected abstract PhysicsCollision IntersectsWith(ConvexCollider collider);
    protected abstract PhysicsCollision IntersectsWith(RectangularCollider collider);
    
    
    public void CollidesWith(object? obj)
    {
        if (obj is ICollider col) CollidesWith(col);
        else throw new ArgumentException();
    }

    private void CollidesWith(ICollider collider)
    {
        if (collider is CompactCollider c) CollidesWith(c);
        else throw new ArgumentException(collider.GetType().Name);
    }
    
    protected abstract void CollidesWith(CompactCollider collider);
    
    
    public abstract bool IsEmpty();
}