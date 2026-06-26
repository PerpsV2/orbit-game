namespace qQEngine;

public class CircularOccluder : IOccluder
{
    public Vec2 LocalPosition { get; set; }

    public SDecimal Radius { get; set; }
    
    public CircularOccluder(SDecimal radius)
    {
        Radius = radius;
    }

    public CircularOccluder(SDecimal radius, Vec2 position)
        : this(radius)
    {
        LocalPosition = position;
    }
}