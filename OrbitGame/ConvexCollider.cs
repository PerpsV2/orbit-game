namespace OrbitGame;

public class ConvexCollider : CompactCollider, ICollider
{
    private readonly Vector2[] _points;
    private readonly Vector2[] _edgeVectors;
    private readonly double[] _edgeNormalAxisAngles;
    
    public ConvexCollider(Vector2[] points, KinematicObject parent) : base(parent)
    {
        _points = points.Distinct().ToArray();
        _edgeVectors = new Vector2[_points.Length];
        for (int i = 0; i < _points.Length; ++i)
            _edgeVectors[i] = _points[(i + 1) % _points.Length] - _points[i];
        _edgeNormalAxisAngles = _edgeVectors
            .Select(v => Math.Atan2((double)v.Y, (double)v.X) + Math.PI / 2).ToArray();
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
        foreach (var angle in _edgeNormalAxisAngles)
        {
            ScientificDecimal[] projectedCollider = _points.Select(x => Utils.ProjectPoint(x + Position, angle)).ToArray();
            ScientificDecimal projectedPoint = Utils.ProjectPoint(point, angle);
            if (projectedPoint > projectedCollider.Max() || projectedPoint < projectedCollider.Min()) return false;
        }
        return true;
    }

    public override bool IntersectsWith(CircularCollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IntersectsWith(ConvexCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return false;
        foreach (var angle in _edgeNormalAxisAngles.Concat(collider._edgeNormalAxisAngles))
        {
            ScientificDecimal[] proj1 = _points.Select(x => Utils.ProjectPoint(x + Position, angle)).ToArray();
            ScientificDecimal[] proj2 = collider._points.Select(x => Utils.ProjectPoint(x + collider.Position, angle)).ToArray();
            if (!Utils.IntervalIntersects(proj1.Min(), proj1.Max(), proj2.Min(), proj2.Max())) return false;
        }
        return true;
    }

    public override bool IntersectsWith(RectangularCollider collider)
    {
        // convert rect collider to a convex collider and use the respective intersect method
        ConvexCollider convexRect = new ConvexCollider(
            [collider.TopLeft, collider.TopRight, collider.BottomRight, collider.BottomLeft], collider.Parent);
        return IntersectsWith(convexRect);
    }

    public override void CollidesWith(ICollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty() => 
        _points.Length <= 1;
}