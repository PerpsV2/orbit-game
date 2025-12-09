namespace OrbitGame;

public class ConvexCollider(Vector2[] points, KinematicObject parent) : CompactCollider(parent), ICollider
{
    private readonly Vector2[] _points = points.Distinct().ToArray();

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
        for (int i = 0; i < _points.Length; ++i)
        {
            Vector2 edgeVector = _points[(i + 1) % _points.Length] - _points[i];
            double edgeAngle = Math.Atan2((double)edgeVector.Y, (double)edgeVector.X) + Math.PI / 2;
            ScientificDecimal[] projectedCollider = _points.Select(x => Utils.ProjectPoint(x, edgeAngle)).ToArray();
            ScientificDecimal projectedPoint = Utils.ProjectPoint(point, edgeAngle);
            if (projectedCollider.Min() > projectedPoint || projectedPoint > projectedCollider.Max()) return false;
        }
        return true;
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