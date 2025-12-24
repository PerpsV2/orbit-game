using Silk.NET.OpenGL;

namespace OrbitGame;

public class ConvexCollider : CompactCollider, ICollider
{
    private readonly Vector2[] _points;
    private Vector2[] RotatedPoints => _points.Select(x => Vector2.ApplyRotation(x, Parent.Angle)).ToArray();
    private readonly Vector2[] _edges;
    private readonly double[] _edgeNormalAxisAngles;
    
    // edge normal axis angles in world space
    private double[] AbsoluteEdgeNormalAxisAngles => _edgeNormalAxisAngles.Select(x => x + Parent.Angle).ToArray();
    
    public ConvexCollider(Vector2[] points, KinematicObject parent) : base(parent)
    {
        _points = points.Distinct().ToArray();
        Console.Write("Points: ");
        Utils.LogEnumerable(_points);
        _edges = new Vector2[_points.Length];
        for (int i = 0; i < _points.Length; ++i)
            _edges[i] = _points[(i + 1) % _points.Length] - _points[i];
        Console.Write("Edges: ");
        Utils.LogEnumerable(_edges);
        _edgeNormalAxisAngles = _edges
            .Select(v => Utils.UnsignedMod(Math.Atan2((double)v.Y, (double)v.X) - Math.PI / 2, Math.Tau)).ToArray();
        Console.Write("Edge Normals: ");
        Utils.LogEnumerable(_edgeNormalAxisAngles.Select(double.RadiansToDegrees));
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

    public override Collision IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return Collision.None;
        // check if any vertices are inside the circle
        if (_points.Any(x => (Parent.ObjectToWorldSpace(x) - collider.Position).Magnitude() <= collider.Radius)) 
            return new Collision(Vector2.Zero);
        // check if the circle's center is inside the convex shape 
        if (IntersectsWith(collider.Position)) return new Collision(Vector2.Zero);
        for (int curr = 0; curr < _points.Length; ++curr)
        {
            int next = (curr + 1) % _points.Length;
            double angle = -AbsoluteEdgeNormalAxisAngles[curr] + Math.PI / 2;
            ScientificDecimal upperBound = Vector2.ApplyRotation(RotatedPoints[next] - RotatedPoints[curr], angle).X;
            Vector2 transformedCenter = Vector2.ApplyRotation(collider.Position - Position - RotatedPoints[curr], angle);
            if (transformedCenter.X >= 0 && ScientificDecimal.Abs(transformedCenter.Y) <= collider.Radius && 
                transformedCenter.X <= upperBound) return new Collision(Vector2.Zero);
        }

        return Collision.None;
    }

    public override Collision IntersectsWith(ConvexCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return Collision.None;
        Vector2 penetrationVector = new Vector2(new(10), new(10));
        foreach (var angle in _edgeNormalAxisAngles.Concat(collider._edgeNormalAxisAngles))
        {
            double edgeAngle = angle + Math.PI / 2;
            ScientificDecimal[] proj1 = _points
                .Select(x => Parent.ObjectToWorldSpace(x))
                .Select(v => v.Y * Math.Cos(edgeAngle) - v.X * Math.Sin(edgeAngle))
                .ToArray();
            ScientificDecimal[] proj2 = collider._points
                .Select(x => collider.Parent.ObjectToWorldSpace(x))
                .Select(v => v.Y * Math.Cos(edgeAngle) - v.X * Math.Sin(edgeAngle))
                .ToArray();
            if (!Utils.IntervalIntersects(proj1.Min(), proj1.Max(), proj2.Min(), proj2.Max()))
                return Collision.None;
            
            ScientificDecimal pd = 
                Utils.IntervalPenetrationDistance(proj1.Min(), proj1.Max(), proj2.Min(), proj2.Max());
            Vector2 newPenetrationVector = -new Vector2(Math.Cos(angle), Math.Sin(angle)) * pd;
            if (newPenetrationVector.Magnitude() < penetrationVector.Magnitude())
                penetrationVector = newPenetrationVector;
        }
        return new Collision(penetrationVector);
    }

    public override Collision IntersectsWith(RectangularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return Collision.None;
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