namespace qQEngine;

public class CircularOccluder : IOccluder
{
    public Vec2Double LocalPosition { get; set; }

    public SDecimal Radius { get; set; }
    
    public CircularOccluder(SDecimal radius)
    {
        Radius = radius;
    }

    public CircularOccluder(SDecimal radius, Vec2Double position)
        : this(radius)
    {
        LocalPosition = position;
    }
}