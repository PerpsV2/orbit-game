namespace OrbitGame;

/// <summary>
/// Represents an axis-aligned bounding box for a game object.
/// </summary>
/// <param name="Left">Positive distance from the center to the left edge of the BoundingBox</param>
/// <param name="Right">Positive distance from the center to the right edge of the BoundingBox</param>
/// <param name="Top">Positive distance from the center to the top edge of the BoundingBox</param>
/// <param name="Bottom">Positive distance from the center to the bottom edge of the BoundingBox</param>
public readonly record struct BoundingBox(
    SDecimal Left,
    SDecimal Right,
    SDecimal Top,
    SDecimal Bottom)
{
    public BoundingBox(Vec2<SDecimal> center, SDecimal width, SDecimal height) : 
        this(center.X - width / 2, center.X + width / 2, center.Y + height / 2, center.Y - height / 2)
    { }

    public bool IntersectsWith(BoundingBox other, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial)
    {
        if (IsEmpty() || other.IsEmpty()) return false;
        return referenceSpatial.Position.X + Left <= incidentSpatial.Position.X + other.Right &&
               referenceSpatial.Position.X + Right >= incidentSpatial.Position.X + other.Left &&
               referenceSpatial.Position.Y + Bottom <= incidentSpatial.Position.Y + other.Top &&
               referenceSpatial.Position.Y + Top >= incidentSpatial.Position.Y + other.Bottom;
    }

    public bool IsEmpty()
    {
        return Left == Right || Top == Bottom;
    }
}