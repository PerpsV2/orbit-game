using Silk.NET.OpenGL;

namespace OrbitGame;

public class ConvexCollider : CompactCollider, ICollider
{
    private readonly Vector2[] _points;
    private Vector2[] RotatedPoints => _points.Select(x => Vector2.ApplyRotation(x, Parent.Angle)).ToArray();
    private readonly double[] _edgeNormalAxisAngles;
    
    // edge normal axis angles in world space
    private double[] AbsoluteEdgeNormalAxisAngles => _edgeNormalAxisAngles.Select(x => x + Parent.Angle).ToArray();
    
    public ConvexCollider(Vector2[] points, KinematicObject parent) : base(parent)
    {
        _points = points.Distinct().ToArray();
        Vector2[] edgeVectors = new Vector2[_points.Length];
        for (int i = 0; i < _points.Length; ++i)
            edgeVectors[i] = _points[(i + 1) % _points.Length] - _points[i];
        _edgeNormalAxisAngles = edgeVectors
            .Select(v => Math.Atan2((double)v.Y, (double)v.X) + Math.PI / 2).ToArray();
    }

    public override RectangularCollider GetBoundingBox()
    {
        if (IsEmpty()) return new(0, 0, 0, 0, Parent);
        ScientificDecimal top = ScientificDecimal.MinValue;
        ScientificDecimal right = ScientificDecimal.MinValue;
        ScientificDecimal bottom = ScientificDecimal.MaxValue;
        ScientificDecimal left = ScientificDecimal.MaxValue;
        foreach (var point in RotatedPoints)
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
        if (IsEmpty()) return false;
        foreach (var angle in AbsoluteEdgeNormalAxisAngles)
        {
            ScientificDecimal[] projectedCollider = _points.Select(
                x => Utils.ProjectPoint(Parent.ObjectToWorldSpace(x), angle))
                .ToArray();
            ScientificDecimal projectedPoint = Utils.ProjectPoint(point, angle);
            if (projectedPoint > projectedCollider.Max() || projectedPoint < projectedCollider.Min()) return false;
        }
        return true;
    }

    public override bool IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return false;
        // check if any vertices are inside the circle
        if (_points.Any(x => (Parent.ObjectToWorldSpace(x) - collider.Position).Magnitude() <= collider.Radius)) 
            return true;
        // check if the circle's center is inside the convex shape 
        if (IntersectsWith(collider.Position)) return true;
        for (int curr = 0; curr < _points.Length; ++curr)
        {
            int next = (curr + 1) % _points.Length;
            double angle = -AbsoluteEdgeNormalAxisAngles[curr] + Math.PI / 2;
            ScientificDecimal upperBound = Vector2.ApplyRotation(RotatedPoints[next] - RotatedPoints[curr], angle).X;
            Vector2 transformedCenter = Vector2.ApplyRotation(collider.Position - Position - RotatedPoints[curr], angle);
            if (transformedCenter.X >= 0 && ScientificDecimal.Abs(transformedCenter.Y) <= collider.Radius && 
                transformedCenter.X <= upperBound) return true;
        }

        return false;
    }

    public override bool IntersectsWith(ConvexCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return false;
        foreach (var angle in AbsoluteEdgeNormalAxisAngles.Concat(collider.AbsoluteEdgeNormalAxisAngles))
        {
            ScientificDecimal[] proj1 = _points.Select(
                x => Utils.ProjectPoint(Parent.ObjectToWorldSpace(x), angle)).ToArray();
            ScientificDecimal[] proj2 = collider._points.Select(
                x => Utils.ProjectPoint(collider.Parent.ObjectToWorldSpace(x), angle)).ToArray();
            if (!Utils.IntervalIntersects(proj1.Min(), proj1.Max(), proj2.Min(), proj2.Max())) return false;
        }
        return true;
    }

    public override bool IntersectsWith(RectangularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return false;
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
        _points.Length < 3;
}