using Silk.NET.OpenGL;

namespace OrbitGame;

public class ConvexCollider : CompactCollider, ICollider
{
    private readonly Vector2[] _points;
    private Vector2[] RotatedPoints => _points.Select(x => Matrix3X3.Rotation(Parent.Angle) * x).ToArray();

    public ConvexCollider(Vector2[] points, KinematicObject parent, Material material) : base(parent, material)
    {
        _points = points.Distinct().ToArray();
        Inertia = CalculateInertia();
    }

    private ScientificDecimal CalculateInertia()
    {
        var triangles = Vector2.TriangulateConvex(_points);
        ScientificDecimal totalArea = triangles.Aggregate(new ScientificDecimal(0),
            (a, t) => a + Utils.CalculateTriangleArea(t.a, t.b, t.c));
        
        ScientificDecimal[] masses = new ScientificDecimal[triangles.Length];
        Vector2[] centroids = new Vector2[triangles.Length];
        ScientificDecimal[] inertias = new ScientificDecimal[triangles.Length];
        for (int i = 0; i < triangles.Length; ++i)
        {
            var t = triangles[i];
            Vector2 a = t.a;
            Vector2 b = t.b;
            Vector2 c = t.c;
            
            masses[i] = Parent.Mass / totalArea * Utils.CalculateTriangleArea(a, b, c);
            centroids[i] = new Vector2(a.X + b.X + c.X, a.Y + b.Y + c.Y)/ 3;
            inertias[i] = masses[i] * (Vector2.Dot(a, a) + Vector2.Dot(b, b) + Vector2.Dot(c, c) +
                Vector2.Dot(c, c) + Vector2.Dot(a, b) + Vector2.Dot(b, c) + Vector2.Dot(c, a))/ 6;
        }

        ScientificDecimal totalInertia = 0;
        for (int i = 0; i < triangles.Length; ++i)
            totalInertia += inertias[i] + masses[i] * (centroids[i].X.Square() + centroids[i].Y.Square());
        
        return totalInertia;
    }

    protected override RectangularCollider GetBoundingBox()
    {
        if (IsEmpty()) return new(0, 0, 0, 0, Parent, Material);
        ScientificDecimal top = RotatedPoints[0].Y;
        ScientificDecimal right = RotatedPoints[0].X;
        ScientificDecimal bottom = RotatedPoints[0].Y;
        ScientificDecimal left = RotatedPoints[0].X;
        foreach (var point in RotatedPoints)
        {
            if (point.Y > top) top = point.Y;
            if (point.Y < bottom) bottom = point.Y;
            if (point.X > right) right = point.X;
            if (point.X < left) left = point.X;
        }

        return new(top, right, bottom, left, Parent, Material);
    }

    protected override PointCollision IntersectsWith(Vector2 point)
    {
        throw new NotImplementedException();
    }

    protected override PhysicsCollision? IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;
        
        Vector2 collisionPoint = Vector2.Zero;
        Vector2 minPenetrationVector = Vector2.Zero;
        
        for (int curr = 0; curr < _points.Length; ++curr)
        {
            int next = (curr + 1) % _points.Length;
            double angle = Vector2.GetPrincipalAngle(_points[curr], _points[next]);
            Matrix3X3 transformation = Matrix3X3.Rotation(-angle) * Matrix3X3.Translation(-_points[curr]);
            Matrix3X3 invTransformation = Matrix3X3.Translation(_points[curr]) * Matrix3X3.Rotation(angle);
            ScientificDecimal upperBound = (transformation * _points[next]).X;
            Vector2 transformedCenter = transformation * Parent.WorldToObjectSpace(collider.Position);
            if (transformedCenter.X >= 0 && transformedCenter.Y.Abs() <= collider.Radius &&
                transformedCenter.X <= upperBound)
            {
                ScientificDecimal penetrationDistance = -collider.Radius - transformedCenter.Y;
                if (penetrationDistance.Abs() < minPenetrationVector.Magnitude() ||
                    minPenetrationVector.Equals(Vector2.Zero))
                {
                    minPenetrationVector = Vector2.FromPolar(angle + Math.PI / 2, -penetrationDistance);
                    collisionPoint = invTransformation * new Vector2(transformedCenter.X, 0);
                }
            }
        }

        if (!minPenetrationVector.Equals(Vector2.Zero))
        {
            return new PhysicsCollision(this, collider, collisionPoint, minPenetrationVector);
        }

        Vector2 relativeCenter = Parent.WorldToObjectSpace(collider.Position);
        Vector2[] sortedPoints = _points.OrderBy(v => (v - relativeCenter).Magnitude()).ToArray();
        
        Vector2 diffVector1 = sortedPoints[0] - Parent.WorldToObjectSpace(collider.Position);
        Vector2 diffVector2 = sortedPoints[1] - Parent.WorldToObjectSpace(collider.Position);
        if (diffVector1.Magnitude() <= collider.Radius) {
            collisionPoint = sortedPoints[0];
            if (diffVector2.Magnitude() <= collider.Radius)
                collisionPoint = (sortedPoints[0] + sortedPoints[1]) / 2;
            minPenetrationVector = diffVector1.Normalize() * (collider.Radius - diffVector1.Magnitude());
        }

        if (!minPenetrationVector.Equals(Vector2.Zero))
        {
            return new PhysicsCollision(this, collider, collisionPoint, minPenetrationVector);
        }

        return null;
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider)
    {
        throw new NotImplementedException();
    }

    protected override PhysicsCollision? IntersectsWith(RectangularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;
        // convert rect collider to a convex collider and use the respective intersect method
        ConvexCollider convexRect = new ConvexCollider(
            [collider.TopLeft, collider.TopRight, collider.BottomRight, collider.BottomLeft], 
            collider.Parent, collider.Material);
        return IntersectsWith(convexRect);
    }

    public override bool IsEmpty() => 
        _points.Length < 3;
}