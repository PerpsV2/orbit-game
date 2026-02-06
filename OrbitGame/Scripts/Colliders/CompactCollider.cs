using System;
using System.Drawing;

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
public abstract class CompactCollider
{
    public ScientificDecimal Inertia;
        
    public bool Fixed;

    public abstract void CalculateInertia(ScientificDecimal mass);
    
    /// <summary>
    /// Method to return the axis-aligned rectangular collider which best fits the set of points in the collider
    /// </summary>
    protected abstract BoundingBox GetBoundingBox(double angle);
    
    public bool NearsWith(CompactCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial)
    {
        return GetBoundingBox(referenceSpatial.Angle).IntersectsWith(
            collider.GetBoundingBox(colliderSpatial.Angle), referenceSpatial, colliderSpatial
        );
    }
    
    public abstract PointCollision IntersectsWith(SD_Vector2 point, SpatialInfo referenceSpatial);
    public PhysicsCollision? IntersectsWith(CompactCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial)
    {
        switch (collider)
        {
            case CircularCollider c: return IntersectsWith(c, referenceSpatial, colliderSpatial);
            case ConvexCollider c: return IntersectsWith(c, referenceSpatial, colliderSpatial);
            default: throw new ArgumentException();
        }
    }

    protected abstract PhysicsCollision? IntersectsWith(
        CircularCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial
    );
    protected abstract PhysicsCollision? IntersectsWith(
        ConvexCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial
    );
    
    public abstract bool IsEmpty();
}