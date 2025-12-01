namespace OrbitGame;

public class ConvexCollider
    : CompactCollider, ICollider
{
    private readonly Vector2[] _points;
    
    public ConvexCollider(Vector2[] points, KinematicObject parent) : base(parent) 
    {
        _points = points.Distinct().ToArray();
    }

    public override RectangularCollider GetBoundingBox()
    {
        ScientificDecimal top = ScientificDecimal.MinValue;
        ScientificDecimal right = ScientificDecimal.MinValue;
        ScientificDecimal bottom = ScientificDecimal.MaxValue;
        ScientificDecimal left = ScientificDecimal.MaxValue;
        foreach (var point in _points)
        {
            if (point.Y > top) top = point.Y;
            if (point.Y < bottom) bottom = point.Y;
            if (point.X > right) right = point.X;
            if (point.X < left) left = point.X;
        }

        return new(top, right, bottom, left, Parent);
    }

    public override bool IntersectsWith(Vector2 point)
    {
        throw new NotImplementedException();
    }

    public override bool IntersectsWith(CircularCollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IntersectsWith(ConvexCollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IntersectsWith(RectangularCollider collider)
    {
        throw new NotImplementedException();
    }

    public override void CollidesWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty() => 
        _points.Length <= 1;
}