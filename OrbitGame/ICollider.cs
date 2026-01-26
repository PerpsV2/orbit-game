using System.ComponentModel;
using SkiaSharp;

namespace OrbitGame;

/// <summary>
/// Interface containing methods for any object on screen which provides intersection information with an object
/// </summary>
public interface ICollider
{
    /// <summary>
    /// Returns whether two objects are close to each other and collision is possible i.e. broad phase collision detection
    /// </summary>
    public bool NearsWith(object? obj);
    
    /// <summary>
    /// Returns whether two objects are intersecting and provides information about the intersection
    /// </summary>
    public IIntersection? IntersectsWith(object? obj);
    
    /// <summary>
    /// Determines whether two objects intersect and executes the expected response using the intersection information
    /// </summary>
    public void CollidesWith(object? obj);
    
    /// <summary>
    /// Returns true if the collider has no area (an intersection is impossible)
    /// </summary>
    public bool IsEmpty();
}