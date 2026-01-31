using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

public class ConvexCollider : CompactCollider, ICollider
{
    private readonly SD_Vector2[] _points;
    private SD_Vector2[] RotatedPoints => _points.Select(x => Matrix3X3.Rotation(Parent.Angle) * x).ToArray();

    public ConvexCollider(SD_Vector2[] points, KinematicObject parent) : base(parent)
    {
        _points = points.Distinct().ToArray();
        Inertia = CalculateInertia();
    }

    private ScientificDecimal CalculateInertia()
    {
        var triangles = SD_Vector2.TriangulateConvex(_points);
        ScientificDecimal totalArea = triangles.Aggregate(new ScientificDecimal(0),
            (a, t) => a + Utils.CalculateTriangleArea(t.a, t.b, t.c));
        
        ScientificDecimal[] masses = new ScientificDecimal[triangles.Length];
        SD_Vector2[] centroids = new SD_Vector2[triangles.Length];
        ScientificDecimal[] inertias = new ScientificDecimal[triangles.Length];
        for (int i = 0; i < triangles.Length; ++i)
        {
            SD_Vector2 a = triangles[i].a;
            SD_Vector2 b = triangles[i].b;
            SD_Vector2 c = triangles[i].c;
            
            masses[i] = Parent.Mass / totalArea * Utils.CalculateTriangleArea(a, b, c);
            centroids[i] = (a + b + c) / 3;
            inertias[i] = masses[i] * (SD_Vector2.Dot(a, a) + SD_Vector2.Dot(b, b) + SD_Vector2.Dot(c, c) +
                SD_Vector2.Dot(c, c) + SD_Vector2.Dot(a, b) + SD_Vector2.Dot(b, c) + SD_Vector2.Dot(c, a))/ 6;
        }

        ScientificDecimal totalInertia = 0;
        for (int i = 0; i < triangles.Length; ++i)
            totalInertia += inertias[i] + masses[i] * (centroids[i].X.Square() + centroids[i].Y.Square());
        
        return totalInertia;
    }

    public override RectangularCollider GetBoundingBox()
    {
        
        ScientificDecimal minX = RotatedPoints[0].X;
        ScientificDecimal minY = RotatedPoints[0].Y;
        ScientificDecimal maxX = RotatedPoints[1].X;
        ScientificDecimal maxY = RotatedPoints[1].Y;
        foreach (var rotatedPoint in RotatedPoints)
        {
            if (rotatedPoint.X < minX) minX = rotatedPoint.X;
            if (rotatedPoint.X > maxX) maxX = rotatedPoint.X;
            if (rotatedPoint.Y < minY) minY = rotatedPoint.Y;
            if (rotatedPoint.Y > maxY) maxY = rotatedPoint.Y;
        }

        SD_Vector2 topRight = new SD_Vector2(maxX, maxY);
        SD_Vector2 bottomLeft = new SD_Vector2(minX, minY);
        
        return new RectangularCollider(topRight, bottomLeft, Parent);
    }
    
    public static explicit operator RectangularCollider(ConvexCollider value)
        => value.GetBoundingBox();

    protected override PointCollision IntersectsWith(SD_Vector2 point)
    {
        throw new NotImplementedException();
    }
    
    protected override PhysicsCollision? IntersectsWith(CircularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;

        SD_Vector2 collisionPoint = SD_Vector2.Zero;
        SD_Vector2 minPenetrationVector = SD_Vector2.Zero;
        SD_Vector2 relativeCenter = Parent.WorldToObjectSpace(collider.Position);
        
        // edge case (literally)
        // TODO: fix cases where one object is fully within the other
        for (int i = 0; i < _points.Length; ++i)
        {
            SD_Vector2 currentVertex = _points[i];
            SD_Vector2 nextVertex = _points[(i + 1) % _points.Length];
            double edgeAngle = SD_Vector2.GetPrincipalAngle(currentVertex, nextVertex);
            
            Matrix3X3 transformation = Matrix3X3.Rotation(-edgeAngle) * Matrix3X3.Translation(-currentVertex);
            Matrix3X3 invTransformation = Matrix3X3.Translation(currentVertex) * Matrix3X3.Rotation(edgeAngle);
            
            ScientificDecimal edgeUpperBound = (transformation * nextVertex).X;
            SD_Vector2 transformedCenter = transformation * relativeCenter;
            if (transformedCenter.X >= 0 && transformedCenter.X <= edgeUpperBound &&
                transformedCenter.Y.Abs() <= collider.Radius)
            {
                ScientificDecimal penetrationDistance = -collider.Radius - transformedCenter.Y;
                if (penetrationDistance.Abs() < minPenetrationVector.Magnitude() || minPenetrationVector.Equals(SD_Vector2.Zero))
                {
                    minPenetrationVector = Matrix3X3.Rotation(Parent.Angle) * SD_Vector2.FromPolar(edgeAngle + Math.PI / 2, -penetrationDistance);
                    collisionPoint = Matrix3X3.Rotation(Parent.Angle) * invTransformation * new SD_Vector2(transformedCenter.X, 0);
                }
            }
        }
        
        if (!minPenetrationVector.Equals(SD_Vector2.Zero))
            return new PhysicsCollision(this, collider, [collisionPoint], minPenetrationVector);
        
        // vertex case
        minPenetrationVector = SD_Vector2.Zero;
        SD_Vector2[] sortedPoints = _points.OrderBy(v => (v - relativeCenter).Magnitude()).ToArray();
        
        SD_Vector2 diffVector = sortedPoints[0] - relativeCenter;
        if (diffVector.Magnitude() <= collider.Radius) {
            collisionPoint = Matrix3X3.Rotation(Parent.Angle) * sortedPoints[0];
            minPenetrationVector = Matrix3X3.Rotation(Parent.Angle) * diffVector.Normalize() * (collider.Radius - diffVector.Magnitude());
        }

        if (!minPenetrationVector.Equals(SD_Vector2.Zero))
            return new PhysicsCollision(this, collider, [collisionPoint], minPenetrationVector);

        return null;
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider)
    {
        double GetEdgeAngle((SD_Vector2 a, SD_Vector2 b) edge)
            => (edge.b - edge.a).GetPrincipalAngle();
        
        // calculate minimum penetration vector
        (SD_Vector2 a, SD_Vector2 b)[] refEdges = new (SD_Vector2, SD_Vector2)[_points.Length];
        for (int i = 0; i < _points.Length; ++i)
            refEdges[i] = (RotatedPoints[i], RotatedPoints[(i + 1) % RotatedPoints.Length]);
        
        (SD_Vector2 a, SD_Vector2 b)[] incEdges = new (SD_Vector2, SD_Vector2)[collider._points.Length];
        for (int i = 0; i <collider. _points.Length; ++i)
            incEdges[i] = (collider.RotatedPoints[i], collider.RotatedPoints[(i + 1) % collider.RotatedPoints.Length]);
        
        // create a list of all edges involved
        (SD_Vector2 a, SD_Vector2 b)[] edges = refEdges.Concat(incEdges).ToArray();
        
        // create an array for the penetration vector of the SAT applied onto each edge
        (SD_Vector2 vector, double angle)[] penetrationVectors = new (SD_Vector2 vector, double angle)[edges.Length];
        for (int e = 0; e < penetrationVectors.Length; ++e)
        {
            // current edge
            (SD_Vector2 a, SD_Vector2 b) edge = edges[e];
            // angle to project SAT onto
            double projectionAngle = GetEdgeAngle(edge) + Math.PI / 2;
            
            // project points from both polygons
            ScientificDecimal[] referenceProjectedPoints = new ScientificDecimal[_points.Length];
            for (int p = 0; p < referenceProjectedPoints.Length; ++p)
                referenceProjectedPoints[p] = (Matrix3X3.Rotation(-projectionAngle) * (Position + RotatedPoints[p])).X;
            ScientificDecimal[] incidenceProjectedPoints = new ScientificDecimal[_points.Length];
            for (int p = 0; p < incidenceProjectedPoints.Length; ++p)
                incidenceProjectedPoints[p] = (Matrix3X3.Rotation(-projectionAngle) * (collider.Position + collider.RotatedPoints[p])).X;

            // check if there is not an intersection in SAT range
            if (referenceProjectedPoints.Max() < incidenceProjectedPoints.Min() || 
                incidenceProjectedPoints.Max() < referenceProjectedPoints.Min()) return null;
            
            // find intersection distance
            ScientificDecimal posPenetrationDist = referenceProjectedPoints.Max() - incidenceProjectedPoints.Min();
            ScientificDecimal negPenetrationDist = incidenceProjectedPoints.Max() - referenceProjectedPoints.Min();
            
            // return penetration vector with minimum magnitude for this edge
            penetrationVectors[e].vector = SD_Vector2.FromPolar(projectionAngle,
                posPenetrationDist.Abs() <= negPenetrationDist.Abs() ? -posPenetrationDist : negPenetrationDist);
            penetrationVectors[e].angle = projectionAngle;
        }
        
        var minPenetrationVector = penetrationVectors.MinBy(x => x.vector.Magnitude());
        
        HashSet<SD_Vector2> manifold = new HashSet<SD_Vector2>();
        // calculate collision manifold
        double minPenetrationAngle = minPenetrationVector.angle;
        SD_Vector2 refFurthestPoint = RotatedPoints
            .MinBy(v => (Matrix3X3.Rotation(-minPenetrationAngle) * (Position + v)).X);
        manifold.Add(refFurthestPoint);
        
        /*Vector2 incFurthestPoint = collider._points
            .MinBy(v => (Matrix3X3.Rotation(Math.PI -minPenetrationAngle) * (collider.Position + v)).X);

        double refCollisionNormalAngle = Utils.UnsignedMod(minPenetrationAngle - Math.PI / 2, Math.Tau);
        double incCollisionNormalAngle = Utils.UnsignedMod(minPenetrationAngle + Math.PI / 2, Math.Tau);
        (Vector2 a, Vector2 b) refCollisionEdge = refEdges
            .Where(x => x.a == refFurthestPoint || x.b == refFurthestPoint)
            .MinBy(x => Math.Abs(refCollisionNormalAngle - GetEdgeAngle(x)));
        (Vector2 a, Vector2 b) incCollisionEdge = incEdges
            .Where(x => x.a == incFurthestPoint || x.b == incFurthestPoint)
            .MinBy(x => Math.Abs(incCollisionNormalAngle - GetEdgeAngle(x)));

        if ((refCollisionEdge.b - refCollisionEdge.a).Normalize() !=
            -(incCollisionEdge.b - incCollisionEdge.a).Normalize())
        {
            manifold.Add(refFurthestPoint);
        }
        else
        {
        
        }*/
        
        return new PhysicsCollision(this, collider, manifold, minPenetrationVector.vector);
    }

    protected override PhysicsCollision? IntersectsWith(RectangularCollider collider)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;
        // convert rect collider to a convex collider and use the respective intersect method
        ConvexCollider convexRect = new ConvexCollider(
            [collider.TopLeft, collider.TopRight, collider.BottomRight, collider.BottomLeft], 
            collider.Parent);
        return IntersectsWith(convexRect);
    }

    public override bool IsEmpty()
    {
        return false;
        throw new NotImplementedException();
    }
}