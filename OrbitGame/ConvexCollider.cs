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
        throw new NotImplementedException();
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
        Vector2 relativeCenter = Parent.WorldToObjectSpace(collider.Position);
        
        // edge case (literally)
        for (int i = 0; i < _points.Length; ++i)
        {
            Vector2 currentVertex = _points[i];
            Vector2 nextVertex = _points[(i + 1) % _points.Length];
            double edgeAngle = Vector2.GetPrincipalAngle(currentVertex, nextVertex);
            
            Matrix3X3 transformation = Matrix3X3.Rotation(-edgeAngle) * Matrix3X3.Translation(-currentVertex);
            Matrix3X3 invTransformation = Matrix3X3.Translation(currentVertex) * Matrix3X3.Rotation(edgeAngle);
            
            ScientificDecimal edgeUpperBound = (transformation * nextVertex).X;
            Vector2 transformedCenter = transformation * relativeCenter;
            if (transformedCenter.X >= 0 && transformedCenter.X <= edgeUpperBound &&
                transformedCenter.Y.Abs() <= collider.Radius)
            {
                ScientificDecimal penetrationDistance = -collider.Radius - transformedCenter.Y;
                if (penetrationDistance.Abs() < minPenetrationVector.Magnitude() || minPenetrationVector.Equals(Vector2.Zero))
                {
                    minPenetrationVector = Matrix3X3.Rotation(Parent.Angle) * Vector2.FromPolar(edgeAngle + Math.PI / 2, -penetrationDistance);
                    collisionPoint = Matrix3X3.Rotation(Parent.Angle) * invTransformation * new Vector2(transformedCenter.X, 0);
                }
            }
        }
        
        if (!minPenetrationVector.Equals(Vector2.Zero))
            return new PhysicsCollision(this, collider, [collisionPoint], minPenetrationVector);
        
        // vertex case
        minPenetrationVector = Vector2.Zero;
        Vector2[] sortedPoints = _points.OrderBy(v => (v - relativeCenter).Magnitude()).ToArray();
        
        Vector2 diffVector = sortedPoints[0] - relativeCenter;
        if (diffVector.Magnitude() <= collider.Radius) {
            collisionPoint = Matrix3X3.Rotation(Parent.Angle) * sortedPoints[0];
            minPenetrationVector = Matrix3X3.Rotation(Parent.Angle) * diffVector.Normalize() * (collider.Radius - diffVector.Magnitude());
        }

        if (!minPenetrationVector.Equals(Vector2.Zero))
            return new PhysicsCollision(this, collider, [collisionPoint], minPenetrationVector);

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