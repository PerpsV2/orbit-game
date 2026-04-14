using System;
using System.Linq;

namespace OrbitGame;

/// <summary>
/// Game collider with a convex hull defining its bounds.
/// </summary>
public class ConvexCollider : CompactCollider
{
    private readonly ConvexHull _convexHull;
    private readonly BoundingBox _boundingBox;

    /// <summary>
    /// Creates a convex collider from a convex hull.
    /// </summary>
    /// <param name="points">Convex hull of the collider.</param>
    public ConvexCollider(DoubleVec2[] points)
    {
        _convexHull = new(points);
        // Compute the minimum bounding box which guarantees all points are contained within.
        double maxRadius = _convexHull.Points.Select(x => x.Magnitude()).Max();
        _boundingBox = new BoundingBox(DoubleVec2.Zero, maxRadius * 2, maxRadius * 2);
    }

    public override SDecimal CalculateInertia(SDecimal mass)
        => Inertia = _convexHull.CalculateInertia(mass);

    protected override BoundingBox GetBoundingBox(double angle)
        => _boundingBox;

    public override PointCollision IntersectsWith(Vec2<SDecimal> point, SpatialInfo spatial)
    {
        throw new NotImplementedException();
    }
    
    protected override PhysicsCollision? IntersectsWith(
        CircularCollider collider, 
        SpatialInfo referenceSpatial, 
        SpatialInfo incidentSpatial)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;
        DoubleVec2 collisionPoint = DoubleVec2.Zero;
        DoubleVec2 minPenetrationVector = DoubleVec2.Zero;
        DoubleVec2 relativeCenter = (DoubleVec2)Vec2<SDecimal>.RotatePoint(
            incidentSpatial.Position - referenceSpatial.Position,
            incidentSpatial.Angle - referenceSpatial.Angle
        );
        
        // edge case (literally)
        // TODO: fix cases where one object is fully within the other
        for (int i = 0; i < _convexHull.Points.Length; ++i)
        {
            DoubleVec2 currentVertex = _convexHull.Points[i];
            DoubleVec2 nextVertex = _convexHull.Points[(i + 1) % _convexHull.Points.Length];
            double edgeAngle = DoubleVec2.Direction(currentVertex, nextVertex);
            
            double edgeUpperBound = DoubleVec2.RotatePoint(nextVertex - currentVertex, -edgeAngle).X;
            DoubleVec2 transformedCenter = DoubleVec2.RotatePoint(relativeCenter - currentVertex, -edgeAngle);
            if (transformedCenter.X >= 0 && transformedCenter.X <= edgeUpperBound &&
                double.Abs(transformedCenter.Y) <= collider.Radius)
            {
                double penetrationDistance = -collider.Radius + transformedCenter.Y;
                if (double.Abs(penetrationDistance) < minPenetrationVector.Magnitude() ||
                    minPenetrationVector.Equals(DoubleVec2.Zero))
                {
                    minPenetrationVector = DoubleVec2.RotatePoint(
                        DoubleVec2.FromPolar(edgeAngle + Math.PI / 2, penetrationDistance), referenceSpatial.Angle
                    );
                    
                    collisionPoint = DoubleVec2.RotatePoint(
                        DoubleVec2.RotatePoint(new DoubleVec2(transformedCenter.X, 0), edgeAngle) + currentVertex,
                        referenceSpatial.Angle);
                }
            }
        }
        
        if (!minPenetrationVector.Equals(DoubleVec2.Zero))
            return new PhysicsCollision(referenceSpatial, incidentSpatial, [collisionPoint], minPenetrationVector);
        
        // vertex case
        // sort vertices by distance to relative circular center
        minPenetrationVector = DoubleVec2.Zero;
        DoubleVec2 closestVertex = _convexHull.Points.MinBy(v => (v - relativeCenter).Magnitude());
        DoubleVec2 diffVector = closestVertex - relativeCenter;
        if (diffVector.Magnitude() <= collider.Radius) {
            collisionPoint = Matrix3X3<SDecimal>.Rotation(referenceSpatial.Angle) * closestVertex;
            minPenetrationVector = Matrix3X3<SDecimal>.Rotation(referenceSpatial.Angle) * diffVector.Normalize() * 
                                   (collider.Radius - diffVector.Magnitude());
        }
        if (!minPenetrationVector.Equals(DoubleVec2.Zero))
            return new PhysicsCollision(referenceSpatial, incidentSpatial, [collisionPoint], minPenetrationVector);
        return null;
    }
    
    protected override PhysicsCollision? IntersectsWith(
        ConvexCollider collider, 
        SpatialInfo referenceSpatial, 
        SpatialInfo incidentSpatial)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;

        // adjust points to spatial infos
        DoubleVec2[] referencePoints = _convexHull.Points.Select(x => DoubleVec2.RotatePoint(x, referenceSpatial.Angle)).ToArray();
        DoubleVec2[] incidentPoints = collider._convexHull.Points
            .Select(x => DoubleVec2.RotatePoint(x, incidentSpatial.Angle))
            .Select(x => x + (DoubleVec2)(incidentSpatial.Position - referenceSpatial.Position)).ToArray();
        
        double minPenetrationDistance = double.PositiveInfinity;
        DoubleVec2 minPenetrationVector = DoubleVec2.Zero;

        // set up edges to apply separating axis theorem for
        DoubleVec2[] referenceEdges = new DoubleVec2[referencePoints.Length];
        for (int i = 0; i < referenceEdges.Length; i++)
            referenceEdges[i] = referencePoints[(i + 1) % referencePoints.Length] - referencePoints[i];
        DoubleVec2[] incidentEdges = new DoubleVec2[incidentPoints.Length];
        for (int i = 0; i < incidentEdges.Length; i++) 
            incidentEdges[i] = incidentPoints[(i + 1) % incidentPoints.Length] - incidentPoints[i];

        foreach (var edge in referenceEdges.Concat(incidentEdges))
        {
            // apply separating axis theorem projection
            double edgeAngle = edge.Direction();
            double[] projectedReferencePoints = referencePoints
                .Select(x => DoubleVec2.RotatePoint(x, -edgeAngle - Math.PI / 2).X).ToArray();
            (double min, double max) referenceRange = (projectedReferencePoints.Min(), projectedReferencePoints.Max());
            double[] projectedIncidentPoints = incidentPoints
                .Select(x => DoubleVec2.RotatePoint(x, -edgeAngle - Math.PI / 2).X).ToArray();
            (double min, double max) incidentRange = (projectedIncidentPoints.Min(), projectedIncidentPoints.Max());
            
            // check for intersections
            if (referenceRange.min <= incidentRange.max && incidentRange.min <= referenceRange.max)
            {
                // calculate the penetration distance and update the min penetration distance
                double forwardsPenetrationDistance = referenceRange.max - incidentRange.min;
                double backwardsPenetrationDistance = incidentRange.max - incidentRange.min;
                double penetrationDistance =
                    double.Abs(forwardsPenetrationDistance - backwardsPenetrationDistance) > 0
                        ? -forwardsPenetrationDistance
                        : backwardsPenetrationDistance;
                if (double.Abs(penetrationDistance) < minPenetrationDistance)
                {
                    minPenetrationDistance = double.Abs(penetrationDistance);
                    minPenetrationVector = DoubleVec2.FromPolar(
                        edgeAngle - Math.PI / 2,
                        minPenetrationDistance
                    );
                }
            }
            
            // no intersection, therefore no collision
            else return null;
        }
        
        // pick the furthest vertex along the collision normal
        double collisionNormalAngle = minPenetrationVector.Direction();
        var indexedReferencePoints = referencePoints.Index();
        var indexedIncidentPoints = incidentPoints.Index();
        var significantReferenceVertex = indexedReferencePoints
            .MinBy(x => DoubleVec2.RotatePoint(x.Item, -collisionNormalAngle).X);
        var significantIncidentVertex = indexedIncidentPoints
            .MaxBy(x => DoubleVec2.RotatePoint(x.Item, -collisionNormalAngle).X);
        
        return new PhysicsCollision(referenceSpatial, incidentSpatial, [significantReferenceVertex.Item], minPenetrationVector);
    }

    public override bool IsEmpty()
        => _convexHull.CalculateArea() == 0;
}