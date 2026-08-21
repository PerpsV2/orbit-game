namespace qQEngine;

public class PolyOccluder : IOccluder
{
    public Vec2Double LocalPosition { get; set; }

    public List<Vec2Double> Vertices { get; set; }
    
    public PolyOccluder(List<Vec2Double> vertices)
    {
        Vertices = vertices;
    }

    public PolyOccluder(List<Vec2Double> vertices, Vec2Double position)
        : this(vertices)
    {
        LocalPosition = position;
    }
}