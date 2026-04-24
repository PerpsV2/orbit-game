using System;

namespace OrbitGame;

/// <summary>
/// A finite-sized bounded collider with methods for checking intersections with other CompactColliders.
/// </summary>
/// <remarks>
/// Classes which inherit from CompactCollider should all be able to collide with each other.
/// To prevent unnecessary code, there should only be one implementation for collisions between two types of colliders
/// located in the more 'complex' collider.
/// Complexity of colliders follows this order
/// <para>Poly > Convex > Circular</para>
/// </remarks>
public abstract class CompactCollider
{
    /// <summary>
    /// The inertia of the collider.
    /// </summary>
    public SDecimal Inertia { get; protected set; }
    
    public double MaxRadius { get; protected set; }

    /// <summary>
    /// Whether the collider should be moved during a collision.
    /// </summary>
    public bool Fixed { get; set; }
    
    /// <summary>
    /// Calculate the inertia of an object.
    /// </summary>
    /// <param name="mass">Mass of the object.</param>
    /// <returns>The inertia of the object with this as the collider.</returns>
    public abstract SDecimal CalculateInertia(SDecimal mass);
    
    /// <summary>
    /// Returns the bounding-box which contains the set of all points in the collider.
    /// </summary>
    /// <returns>The axis-aligned bounding-box which contains the set of all points in the collider.</returns>
    protected abstract BoundingBox GetBoundingBox(double angle);
    
    /// <summary>
    /// Checks whether two objects are near each other by detecting intersections of their bounding boxes.
    /// </summary>
    /// <param name="collider">The other object's collider.</param>
    /// <param name="referenceSpatial">The SpatialInfo of the first object.</param>
    /// <param name="colliderSpatial">The SpatialInfo of the second object.</param>
    /// <returns>Whether the two objects are near each other.</returns>
    public bool NearsWith(CompactCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial)
    {
        return GetBoundingBox(referenceSpatial.Angle).IntersectsWith(
            collider.GetBoundingBox(colliderSpatial.Angle), referenceSpatial, colliderSpatial
        );
    }
    
    /// <summary>
    /// Returns the collision of an object with a point.
    /// </summary>
    /// <param name="point">Point to check.</param>
    /// <param name="referenceSpatial">SpatialInfo of the object.</param>
    /// <returns>The point collision information.</returns>
    public abstract PointCollision? IntersectsWith(Vec2<SDecimal> point, SpatialInfo referenceSpatial);
    
    /// <summary>
    /// Returns the collision of an object with another object. 
    /// </summary>
    /// <param name="collider">The collider of the second object.</param>
    /// <param name="referenceSpatial">The SpatialInfo of the first object.</param>
    /// <param name="colliderSpatial">The SpatialInfo of the second object.</param>
    /// <returns>The physics collision information.</returns>
    /// <exception cref="ArgumentException">The provided collider's type is invalid.</exception>
    public PhysicsCollision? IntersectsWith(CompactCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial)
    {
        switch (collider)
        {
            case CircularCollider c: return IntersectsWith(c, referenceSpatial, colliderSpatial);
            case ConvexCollider c: return IntersectsWith(c, referenceSpatial, colliderSpatial);
            default: throw new ArgumentException();
        }
    }

    /// <summary>
    /// Returns the collision of an object with a circular collider object.
    /// </summary>
    /// <param name="collider">Circular collider to collide with.</param>
    /// <param name="referenceSpatial">The SpatialInfo of this object.</param>
    /// <param name="colliderSpatial">The SpatialInfo of the circular collider.</param>
    /// <returns>The physics collision information.</returns>
    protected abstract PhysicsCollision? IntersectsWith(
        CircularCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial
    );
    
    /// <summary>
    /// Returns the collision of an object with a convex collider object.
    /// </summary>
    /// <param name="collider">Convex collider to collide with.</param>
    /// <param name="referenceSpatial">The SpatialInfo of this object.</param>
    /// <param name="colliderSpatial">The SpatialInfo of the convex collider.</param>
    /// <returns>The physics collision information.</returns>
    protected abstract PhysicsCollision? IntersectsWith(
        ConvexCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial
    );
    
    /// <summary>
    /// Returns whether a collider is empty or not.
    /// </summary>
    /// <returns>Whether a collider is empty or not (It has an area of zero).</returns>
    public abstract bool IsEmpty();
}