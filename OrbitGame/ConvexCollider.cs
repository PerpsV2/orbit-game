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

    /// <summary>
    /// Projects the shape onto a line parallel with a chosen edge.
    /// </summary>
    /// <param name="edge">Index of an edge. Ranges between 0 and n - 1 where n is the number of vertices</param>
    /// <returns> The closed interval of the projected convex shape with an arbitrary origin</returns>
    private (ScientificDecimal, ScientificDecimal) GetProjectionInterval(uint edge)
    {
        Vector2[] projection = new Vector2[_points.Length];
        for (int i = 0; i < _points.Length; ++i)
        {
            projection[i] = _points[i];
        }
    }

    private bool CheckEdges()
    {
        for (int i = 0; i < _points.Length - 1; ++i)
        {
            Vector2 edgePoint1 = _points[i];
            Vector2 edgePoint2 = _points[i + 1];
            
            
        }
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