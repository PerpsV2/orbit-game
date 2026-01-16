namespace OrbitGame;

/// <summary>
/// A standard finite-sized collider with a parent object at its origin
/// </summary>
/// <remarks>
/// Classes which inherit from CompactCollider should all be able to collide with each other.
/// To prevent unnecessary code, there should only be one implementation for collisions between two colliders
/// located in the more 'complex' collider.
/// Complexity of colliders follows this order
/// Convex > Rectangular > Circular
/// </remarks>
public abstract class CompactCollider(Body parent) : ICollider
{
    public readonly Body Parent = parent;
    public Vector2 Position => Parent.Position;
    public bool Fixed;
    
    /// <summary>
    /// Method to return the rectangular collider which best fits the set of points in the collider
    /// </summary>
    public abstract RectangularCollider GetBoundingBox();
    
    public bool NearsWith(object? obj)
    {
        if (obj is ICollider col) return NearsWith(col);
        throw new NotSupportedException();
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
    
    public abstract bool IntersectsWith(Vector2 point);
    public IIntersection IntersectsWith(object? obj)
    {
        throw new NotImplementedException();
    }

    public void CollidesWith(object? obj)
    {
        throw new NotImplementedException();
    }

    public abstract Collision IntersectsWith(CircularCollider collider);
    public abstract Collision IntersectsWith(ConvexCollider collider);
    public abstract Collision IntersectsWith(RectangularCollider collider);
    
    public Collision IntersectsWith(ICollider collider)
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
    public abstract void CollidesWith(CompactCollider collider);

    public void CollidesWith(Body body)
    {
        if (body.Collider == null) return;
        CollidesWith(body.Collider);
    }
    
    public abstract bool IsEmpty();
}