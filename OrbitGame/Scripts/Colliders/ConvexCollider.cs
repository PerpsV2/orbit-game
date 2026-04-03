using System;
using System.Linq;

namespace OrbitGame;

/// <summary>
/// Game collider with a convex hull defining its bounds.
/// </summary>
public class ConvexCollider : CompactCollider
{
    private readonly Vec2<SDecimal>[] _points;
    private readonly BoundingBox _boundingBox;

    /// <summary>
    /// Creates a convex collider from a convex hull.
    /// </summary>
    /// <param name="points">Convex hull of the collider.</param>
    public ConvexCollider(Vec2<SDecimal>[] points)
    {
        _points = points;
        // Compute the minimum bounding box which guarantees all points are contained within.
        SDecimal maxRadius = _points.Select(x => x.Magnitude()).Max();
        _boundingBox = new BoundingBox(Vec2<SDecimal>.Zero, maxRadius * 2, maxRadius * 2);
    }

    public override SDecimal CalculateInertia(SDecimal mass)
        => Inertia = Utils.GetConvexHullInertia(_points, mass);

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
        Vec2<SDecimal> collisionPoint = Vec2<SDecimal>.Zero;
        Vec2<SDecimal> minPenetrationVector = Vec2<SDecimal>.Zero;
        Vec2<SDecimal> relativeCenter = Vec2<SDecimal>.RotatePoint(
            incidentSpatial.Position - referenceSpatial.Position,
            incidentSpatial.Angle - referenceSpatial.Angle
        );
        
        // edge case (literally)
        // TODO: fix cases where one object is fully within the other
        for (int i = 0; i < _points.Length; ++i)
        {
            Vec2<SDecimal> currentVertex = _points[i];
            Vec2<SDecimal> nextVertex = _points[(i + 1) % _points.Length];
            double edgeAngle = Vec2<SDecimal>.Direction(currentVertex, nextVertex);
            
            SDecimal edgeUpperBound = Vec2<SDecimal>.RotatePoint(nextVertex - currentVertex, -edgeAngle).X;
            Vec2<SDecimal> transformedCenter = Vec2<SDecimal>.RotatePoint(relativeCenter - currentVertex, -edgeAngle);
            if (transformedCenter.X >= 0 && transformedCenter.X <= edgeUpperBound &&
                SDecimal.Abs(transformedCenter.Y) <= collider.Radius)
            {
                SDecimal penetrationDistance = -collider.Radius + transformedCenter.Y;
                if (SDecimal.Abs(penetrationDistance) < minPenetrationVector.Magnitude() ||
                    minPenetrationVector.Equals(Vec2<SDecimal>.Zero))
                {
                    minPenetrationVector = Vec2<SDecimal>.RotatePoint(
                        Vec2<SDecimal>.FromPolar(edgeAngle + Math.PI / 2, penetrationDistance), referenceSpatial.Angle
                    );
                    
                    collisionPoint = Vec2<SDecimal>.RotatePoint(
                        Vec2<SDecimal>.RotatePoint(new Vec2<SDecimal>(transformedCenter.X, 0), edgeAngle) + currentVertex,
                        referenceSpatial.Angle);
                }
            }
        }
        
        if (!minPenetrationVector.Equals(Vec2<SDecimal>.Zero))
            return new PhysicsCollision(referenceSpatial, incidentSpatial, [collisionPoint], minPenetrationVector);
        
        // vertex case
        // sort vertices by distance to relative circular center
        minPenetrationVector = Vec2<SDecimal>.Zero;
        Vec2<SDecimal> closestVertex = _points.MinBy(v => (v - relativeCenter).Magnitude());
        Vec2<SDecimal> diffVector = closestVertex - relativeCenter;
        if (diffVector.Magnitude() <= collider.Radius) {
            collisionPoint = Matrix3X3<SDecimal>.Rotation(referenceSpatial.Angle) * closestVertex;
            minPenetrationVector = Matrix3X3<SDecimal>.Rotation(referenceSpatial.Angle) * diffVector.Normalize() * 
                                   (collider.Radius - diffVector.Magnitude());
        }
        if (!minPenetrationVector.Equals(Vec2<SDecimal>.Zero))
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
        Vec2<SDecimal>[] referencePoints = _points.Select(x => Vec2<SDecimal>.RotatePoint(x, referenceSpatial.Angle)).ToArray();
        Vec2<SDecimal>[] incidentPoints = collider._points
            .Select(x => Vec2<SDecimal>.RotatePoint(x, incidentSpatial.Angle))
            .Select(x => x + incidentSpatial.Position - referenceSpatial.Position).ToArray();
        
        SDecimal minPenetrationDistance = SDecimal.PositiveInfinity;
        Vec2<SDecimal> minPenetrationVector = Vec2<SDecimal>.Zero;

        // set up edges to apply separating axis theorem for
        Vec2<SDecimal>[] referenceEdges = new Vec2<SDecimal>[referencePoints.Length];
        for (int i = 0; i < referenceEdges.Length; i++)
            referenceEdges[i] = referencePoints[(i + 1) % referencePoints.Length] - referencePoints[i];
        Vec2<SDecimal>[] incidentEdges = new Vec2<SDecimal>[incidentPoints.Length];
        for (int i = 0; i < incidentEdges.Length; i++) 
            incidentEdges[i] = incidentPoints[(i + 1) % incidentPoints.Length] - incidentPoints[i];

        foreach (var edge in referenceEdges.Concat(incidentEdges))
        {
            // apply separating axis theorem projection
            double edgeAngle = edge.Direction();
            SDecimal[] projectedReferencePoints = referencePoints
                .Select(x => Vec2<SDecimal>.RotatePoint(x, -edgeAngle - Math.PI / 2).X).ToArray();
            (SDecimal min, SDecimal max) referenceRange = (projectedReferencePoints.Min(), projectedReferencePoints.Max());
            SDecimal[] projectedIncidentPoints = incidentPoints
                .Select(x => Vec2<SDecimal>.RotatePoint(x, -edgeAngle - Math.PI / 2).X).ToArray();
            (SDecimal min, SDecimal max) incidentRange = (projectedIncidentPoints.Min(), projectedIncidentPoints.Max());
            
            // check for intersections
            if (referenceRange.min <= incidentRange.max && incidentRange.min <= referenceRange.max)
            {
                // calculate the penetration distance and update the min penetration distance
                SDecimal forwardsPenetrationDistance = referenceRange.max - incidentRange.min;
                SDecimal backwardsPenetrationDistance = incidentRange.max - incidentRange.min;
                SDecimal penetrationDistance =
                    SDecimal.Abs(forwardsPenetrationDistance - backwardsPenetrationDistance) > 0
                        ? -forwardsPenetrationDistance
                        : backwardsPenetrationDistance;
                if (SDecimal.Abs(penetrationDistance) < minPenetrationDistance)
                {
                    minPenetrationDistance = SDecimal.Abs(penetrationDistance);
                    minPenetrationVector = Vec2<SDecimal>.FromPolar(
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
            .MinBy(x => Vec2<SDecimal>.RotatePoint(x.Item, -collisionNormalAngle).X);
        var significantIncidentVertex = indexedIncidentPoints
            .MaxBy(x => Vec2<SDecimal>.RotatePoint(x.Item, -collisionNormalAngle).X);
        
        return new PhysicsCollision(referenceSpatial, incidentSpatial, [significantReferenceVertex.Item], minPenetrationVector);
    }

    public override bool IsEmpty()
        => Utils.GetConvexHullArea(_points) == 0;
}