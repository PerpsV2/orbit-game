using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// In-game CompactCollider with a set of points that form a convex hull.
/// </summary>
public class ConvexCollider : CompactCollider
{
    private readonly BoundingBox _defaultBoundingBox;
    private readonly DVector2<SDecimal>[] _points;

    public ConvexCollider(DVector2<SDecimal>[] points)
    {
        _points = points;
        SDecimal maxRadius = _points.Select(x => x.Magnitude()).Max();
        _defaultBoundingBox = new BoundingBox(DVector2<SDecimal>.Zero, maxRadius * 2, maxRadius * 2);
    }

    public override SDecimal CalculateInertia(SDecimal mass)
    {
        return Inertia = Utils.CalculateConvexInertia(_points, mass);
    }

    protected override BoundingBox GetBoundingBox(double angle)
        => _defaultBoundingBox;

    public override PointCollision IntersectsWith(DVector2<SDecimal> point, SpatialInfo spatial)
    {
        throw new NotImplementedException();
    }
    
    protected override PhysicsCollision? IntersectsWith(
        CircularCollider collider, 
        SpatialInfo referenceSpatial, 
        SpatialInfo incidentSpatial)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;
        DVector2<SDecimal> collisionPoint = DVector2<SDecimal>.Zero;
        DVector2<SDecimal> minPenetrationVector = DVector2<SDecimal>.Zero;
        DVector2<SDecimal> relativeCenter = DVector2<SDecimal>.RotatePoint(
            incidentSpatial.Position - referenceSpatial.Position,
            incidentSpatial.Angle - referenceSpatial.Angle
        );
        
        // edge case (literally)
        // TODO: fix cases where one object is fully within the other
        for (int i = 0; i < _points.Length; ++i)
        {
            DVector2<SDecimal> currentVertex = _points[i];
            DVector2<SDecimal> nextVertex = _points[(i + 1) % _points.Length];
            double edgeAngle = DVector2<SDecimal>.Direction(currentVertex, nextVertex);
            
            SDecimal edgeUpperBound = DVector2<SDecimal>.RotatePoint(nextVertex - currentVertex, -edgeAngle).X;
            DVector2<SDecimal> transformedCenter = DVector2<SDecimal>.RotatePoint(relativeCenter - currentVertex, -edgeAngle);
            if (transformedCenter.X >= 0 && transformedCenter.X <= edgeUpperBound &&
                SDecimal.Abs(transformedCenter.Y) <= collider.Radius)
            {
                SDecimal penetrationDistance = -collider.Radius + transformedCenter.Y;
                if (SDecimal.Abs(penetrationDistance) < minPenetrationVector.Magnitude() ||
                    minPenetrationVector.Equals(DVector2<SDecimal>.Zero))
                {
                    minPenetrationVector = DVector2<SDecimal>.RotatePoint(
                        DVector2<SDecimal>.FromPolar(edgeAngle + Math.PI / 2, penetrationDistance), referenceSpatial.Angle
                    );
                    
                    collisionPoint = DVector2<SDecimal>.RotatePoint(
                        DVector2<SDecimal>.RotatePoint(new DVector2<SDecimal>(transformedCenter.X, 0), edgeAngle) + currentVertex,
                        referenceSpatial.Angle);
                }
            }
        }
        
        if (!minPenetrationVector.Equals(DVector2<SDecimal>.Zero))
            return new PhysicsCollision(referenceSpatial, incidentSpatial, [collisionPoint], minPenetrationVector);
        
        // vertex case
        // sort vertices by distance to relative circular center
        minPenetrationVector = DVector2<SDecimal>.Zero;
        DVector2<SDecimal> closestVertex = _points.MinBy(v => (v - relativeCenter).Magnitude());
        DVector2<SDecimal> diffVector = closestVertex - relativeCenter;
        if (diffVector.Magnitude() <= collider.Radius) {
            collisionPoint = Matrix3X3<SDecimal>.Rotation(referenceSpatial.Angle) * closestVertex;
            minPenetrationVector = Matrix3X3<SDecimal>.Rotation(referenceSpatial.Angle) * diffVector.Normalize() * 
                                   (collider.Radius - diffVector.Magnitude());
        }
        if (!minPenetrationVector.Equals(DVector2<SDecimal>.Zero))
            return new PhysicsCollision(referenceSpatial, incidentSpatial, [collisionPoint], minPenetrationVector);
        return null;
    }

    protected override PhysicsCollision? IntersectsWith(
        ConvexCollider collider, 
        SpatialInfo referenceSpatial, 
        SpatialInfo incidentSpatial)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;

        DVector2<SDecimal>[] referencePoints = _points.Select(x => DVector2<SDecimal>.RotatePoint(x, referenceSpatial.Angle)).ToArray();
        DVector2<SDecimal>[] incidentPoints = collider._points
            .Select(x => DVector2<SDecimal>.RotatePoint(x, incidentSpatial.Angle))
            .Select(x => x + incidentSpatial.Position - referenceSpatial.Position).ToArray();
        
        SDecimal minPenetrationDistance = SDecimal.PosInfinity;
        DVector2<SDecimal> minPenetrationVector = DVector2<SDecimal>.Zero;

        DVector2<SDecimal>[] referenceEdges = new DVector2<SDecimal>[referencePoints.Length];
        for (int i = 0; i < referenceEdges.Length; i++)
            referenceEdges[i] = referencePoints[(i + 1) % referencePoints.Length] - referencePoints[i];
        DVector2<SDecimal>[] incidentEdges = new DVector2<SDecimal>[incidentPoints.Length];
        for (int i = 0; i < incidentEdges.Length; i++) 
            incidentEdges[i] = incidentPoints[(i + 1) % incidentPoints.Length] - incidentPoints[i];

        foreach (var edge in referenceEdges.Concat(incidentEdges))
        {
            double edgeAngle = edge.Direction();
            SDecimal[] projectedReferencePoints = referencePoints
                .Select(x => DVector2<SDecimal>.RotatePoint(x, -edgeAngle - Math.PI / 2).X).ToArray();
            (SDecimal min, SDecimal max) referenceRange = (projectedReferencePoints.Min(), projectedReferencePoints.Max());
            SDecimal[] projectedIncidentPoints = incidentPoints
                .Select(x => DVector2<SDecimal>.RotatePoint(x, -edgeAngle - Math.PI / 2).X).ToArray();
            (SDecimal min, SDecimal max) incidentRange = (projectedIncidentPoints.Min(), projectedIncidentPoints.Max());
            
            // check for SAT projection intersection
            if (referenceRange.min <= incidentRange.max && incidentRange.min <= referenceRange.max)
            {
                SDecimal forwardsPenetrationDistance = referenceRange.max - incidentRange.min;
                SDecimal backwardsPenetrationDistance = incidentRange.max - incidentRange.min;
                SDecimal penetrationDistance =
                    SDecimal.Abs(forwardsPenetrationDistance - backwardsPenetrationDistance) > 0
                        ? -forwardsPenetrationDistance
                        : backwardsPenetrationDistance;
                if (SDecimal.Abs(penetrationDistance) < minPenetrationDistance)
                {
                    minPenetrationDistance = SDecimal.Abs(penetrationDistance);
                    minPenetrationVector = DVector2<SDecimal>.FromPolar(
                        edgeAngle - Math.PI / 2,
                        minPenetrationDistance
                    );
                }
            }
            // no intersection, therefore no collision
            else return null;
        }
        
        // determine collision point
        double collisionNormalAngle = 0;
        if (minPenetrationVector.MagnitudeSquared() > 0) 
            collisionNormalAngle = minPenetrationVector.Direction();
        var indexedColliderPoints = referencePoints.Index();
        var collisionPoint = indexedColliderPoints
            .MinBy(x => DVector2<SDecimal>.RotatePoint(x.Item, -collisionNormalAngle).X).Item;
        
        return new PhysicsCollision(referenceSpatial, incidentSpatial, [collisionPoint], minPenetrationVector);
    }

    public override bool IsEmpty()
    {
        return false;
        throw new NotImplementedException();
    }
}