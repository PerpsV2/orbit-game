using Silk.NET.OpenGL;

namespace OrbitGame;

public class ConvexCollider : CompactCollider, ICollider
{
    private readonly Vector2[] _points;
    private readonly Vector2[] _edges;
    private Vector2[] RotatedPoints => _points.Select(x => Vector2.ApplyRotation(x, Parent.Angle)).ToArray();
    private readonly double[] _edgeAngles;

    private double[] AbsoluteEdgeAngles => _edgeAngles.Select(x => x + Parent.Angle).ToArray();
    
    // edge normal axis angles in world space
    private double[] AbsoluteEdgeNormalAngles => AbsoluteEdgeAngles.Select(x => x + Math.PI / 2).ToArray();
    
    public ConvexCollider(Vector2[] points, Body parent) : base(parent)
    {
        _points = points.Distinct().ToArray();
        _edges = new Vector2[_points.Length];
        for (int i = 0; i < _points.Length; ++i)
            _edges[i] = _points[(i + 1) % _points.Length] - _points[i];
        _edgeAngles = _edges
            .Select(v => Math.Atan2((double)v.Y, (double)v.X)).ToArray();
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
        foreach (var angle in AbsoluteEdgeNormalAngles)
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

        Vector2 minPenetrationVector = Vector2.Zero;
        for (int curr = 0; curr < _points.Length; ++curr)
        {
            int next = (curr + 1) % _points.Length;
            double angle = -AbsoluteEdgeNormalAngles[curr] + Math.PI / 2;
            ScientificDecimal upperBound = Vector2.ApplyRotation(RotatedPoints[next] - RotatedPoints[curr], angle).X;
            Vector2 transformedCenter = Vector2.ApplyRotation(collider.Position - Position - RotatedPoints[curr], angle);
            if (transformedCenter.X >= 0 && ScientificDecimal.Abs(transformedCenter.Y) <= collider.Radius &&
                transformedCenter.X <= upperBound)
            {
                ScientificDecimal penetrationDistance = -collider.Radius - transformedCenter.Y;
                if (ScientificDecimal.Abs(penetrationDistance) < minPenetrationVector.Magnitude() ||
                    minPenetrationVector.Equals(Vector2.Zero))
                    minPenetrationVector = Vector2.DirectionVector(AbsoluteEdgeNormalAngles[curr]) * -penetrationDistance;
            }
        }

        if (!minPenetrationVector.Equals(Vector2.Zero)) return new Collision(minPenetrationVector);
        
        foreach (var vertex in _points.Select(Parent.ObjectToWorldSpace))
        {
            Vector2 diffVector = vertex - collider.Position;
            if (diffVector.Magnitude() <= collider.Radius)
                if (diffVector.Magnitude() < minPenetrationVector.Magnitude() ||
                    minPenetrationVector.Equals(Vector2.Zero))
                    minPenetrationVector =
                        Vector2.DirectionVector(Math.Atan2((double)diffVector.Y, (double)diffVector.X)) *
                        (collider.Radius - diffVector.Magnitude());
        }

        if (!minPenetrationVector.Equals(Vector2.Zero)) return new Collision(minPenetrationVector);
        return Collision.None;
    }

    public override Collision IntersectsWith(ConvexCollider collider)
    {
        Vector2 minPenetrationVector = Vector2.Zero;
        foreach (var angle in AbsoluteEdgeAngles.Concat(collider.AbsoluteEdgeAngles))
        {
            ScientificDecimal[] projectedCollider1 = _points.Select(Parent.ObjectToWorldSpace)
                .Select(v => v.Y * Math.Cos(angle) - v.X * Math.Sin(angle)).ToArray();
            ScientificDecimal[] projectedCollider2 = collider._points
                .Select(collider.Parent.ObjectToWorldSpace)
                .Select(v => v.Y * Math.Cos(angle) - v.X * Math.Sin(angle)).ToArray();
            if (!Utils.IntervalIntersects(projectedCollider1.Min(),
                    projectedCollider1.Max(), projectedCollider2.Min(), projectedCollider2.Max()))
                return Collision.None;
            ScientificDecimal penetrationDistance = Utils.IntervalPenetrationDistance(projectedCollider1.Min(),
                projectedCollider1.Max(), projectedCollider2.Min(), projectedCollider2.Max());
            if (ScientificDecimal.Abs(penetrationDistance) < minPenetrationVector.Magnitude() ||
                minPenetrationVector.Equals(Vector2.Zero))
                minPenetrationVector = Vector2.DirectionVector(angle + Math.PI / 2) * penetrationDistance;
        }

        if (minPenetrationVector.Equals(Vector2.Zero)) return Collision.None;
        return new Collision(minPenetrationVector);
    }

    public override Collision IntersectsWith(RectangularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return Collision.None;
        // convert rect collider to a convex collider and use the respective intersect method
        ConvexCollider convexRect = new ConvexCollider(
            [collider.TopLeft, collider.TopRight, collider.BottomRight, collider.BottomLeft], collider.Parent);
        return IntersectsWith(convexRect);
    }

    public override void CollidesWith(ICollider collider) => throw new NotImplementedException();
    
    public override void CollidesWith(CompactCollider collider)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty() => 
        _points.Length < 3;
}