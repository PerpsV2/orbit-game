using System;
using System.Collections.Generic;
using System.Linq;

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
        
        // // edge case (literally)
        // // TODO: fix cases where one object is fully within the other
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

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial)
    {
        throw new NotImplementedException();
    }

    public override bool IsEmpty()
    {
        return false;
        throw new NotImplementedException();
    }
}