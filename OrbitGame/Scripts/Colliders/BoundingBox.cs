using System;

namespace OrbitGame;

/// <summary>
/// Represents an axis-aligned bounding box for a game object.
/// </summary>
/// <param name="Left">Positive distance from the center to the left edge of the BoundingBox.</param>
/// <param name="Right">Positive distance from the center to the right edge of the BoundingBox.</param>
/// <param name="Top">Positive distance from the center to the top edge of the BoundingBox.</param>
/// <param name="Bottom">Positive distance from the center to the bottom edge of the BoundingBox.</param>
public readonly record struct BoundingBox(
    double Left,
    double Right,
    double Top,
    double Bottom)
{
    
    /// <summary>
    /// Create a bounding box from a center location, a width, and a height.
    /// </summary>
    /// <param name="center">Center of the bounding box.</param>
    /// <param name="width">Width of the bounding box.</param>
    /// <param name="height">Height of the bounding box.</param>
    public BoundingBox(Vec2Double center, double width, double height) : 
        this(center.X - width / 2, center.X + width / 2, center.Y + height / 2, center.Y - height / 2)
    { }

    /// <summary>
    /// Returns whether two bounding boxes are intersecting with each other.
    /// </summary>
    /// <param name="other">The other bounding box.</param>
    /// <param name="referenceSpatial">The SpatialInfo of the first bounding box.</param>
    /// <param name="incidentSpatial">The SpatialInfo of the second bounding box.</param>
    /// <returns>Whether the two bounding boxes intersect.</returns>
    public bool IntersectsWith(BoundingBox other, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial)
    {
        if (IsEmpty() || other.IsEmpty()) return false;
        return referenceSpatial.Position.X + Left <= incidentSpatial.Position.X + other.Right &&
               referenceSpatial.Position.X + Right >= incidentSpatial.Position.X + other.Left &&
               referenceSpatial.Position.Y + Bottom <= incidentSpatial.Position.Y + other.Top &&
               referenceSpatial.Position.Y + Top >= incidentSpatial.Position.Y + other.Bottom;
    }

    /// <summary>
    /// Returns whether a bounding box is empty or not.
    /// </summary>
    /// <returns>Whether the bounding box is empty or not.</returns>
    public bool IsEmpty()
    {
        return Math.Abs(Left - Right) < Constants.ComparisonTolerance || 
               Math.Abs(Top - Bottom) < Constants.ComparisonTolerance;
    }
}