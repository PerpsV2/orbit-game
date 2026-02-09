namespace OrbitGame;

public readonly record struct BoundingBox(
    ScientificDecimal Left,
    ScientificDecimal Right,
    ScientificDecimal Top,
    ScientificDecimal Bottom)
{
    public BoundingBox(SD_Vector2 center, ScientificDecimal width, ScientificDecimal height) : 
        this(center.X - width / 2, center.X + width / 2, center.Y + height / 2, center.Y - height / 2)
    { }

    public bool IntersectsWith(BoundingBox other, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial)
    {
        return referenceSpatial.Position.X + Left <= incidentSpatial.Position.X + other.Right &&
               referenceSpatial.Position.X + Right >= incidentSpatial.Position.X + other.Left &&
               referenceSpatial.Position.Y + Bottom <= incidentSpatial.Position.Y + other.Top &&
               referenceSpatial.Position.Y + Top >= incidentSpatial.Position.Y + other.Bottom;
    }
}