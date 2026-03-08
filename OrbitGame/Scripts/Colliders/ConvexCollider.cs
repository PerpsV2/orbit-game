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
    private readonly SD_Vector2[] _points;

    public ConvexCollider(SD_Vector2[] points)
    {
        _points = points;
        ScientificDecimal maxRadius = _points.Select(x => x.Magnitude()).Max();
        _defaultBoundingBox = new BoundingBox(SD_Vector2.Zero, maxRadius * 2, maxRadius * 2);
    }

    public override ScientificDecimal CalculateInertia(ScientificDecimal mass)
    {
        return Inertia = Utils.CalculateConvexInertia(_points, mass);
    }

    protected override BoundingBox GetBoundingBox(double angle)
        => _defaultBoundingBox;

    public override PointCollision IntersectsWith(SD_Vector2 point, SpatialInfo spatial)
    {
        throw new NotImplementedException();
    }
    
    protected override PhysicsCollision? IntersectsWith(CircularCollider collider, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;
        SD_Vector2 collisionPoint = SD_Vector2.Zero;
        SD_Vector2 minPenetrationVector = SD_Vector2.Zero;
        SD_Vector2 relativeCenter = SD_Vector2.RotatePoint(
            incidentSpatial.Position - referenceSpatial.Position,
            incidentSpatial.Angle - referenceSpatial.Angle
        );
        
        // // edge case (literally)
        // // TODO: fix cases where one object is fully within the other
        for (int i = 0; i < _points.Length; ++i)
        {
            SD_Vector2 currentVertex = _points[i];
            SD_Vector2 nextVertex = _points[(i + 1) % _points.Length];
            double edgeAngle = SD_Vector2.Direction(currentVertex, nextVertex);
            
            ScientificDecimal edgeUpperBound = SD_Vector2.RotatePoint(nextVertex - currentVertex, -edgeAngle).X;
            SD_Vector2 transformedCenter = SD_Vector2.RotatePoint(relativeCenter - currentVertex, -edgeAngle);
            if (transformedCenter.X >= 0 && transformedCenter.X <= edgeUpperBound &&
                ScientificDecimal.Abs(transformedCenter.Y) <= collider.Radius)
            {
                ScientificDecimal penetrationDistance = -collider.Radius + transformedCenter.Y;
                if (ScientificDecimal.Abs(penetrationDistance) < minPenetrationVector.Magnitude() ||
                    minPenetrationVector.Equals(SD_Vector2.Zero))
                {
                    minPenetrationVector = SD_Vector2.RotatePoint(
                        SD_Vector2.FromPolar(edgeAngle + Math.PI / 2, penetrationDistance), referenceSpatial.Angle
                    );
                    
                    collisionPoint = SD_Vector2.RotatePoint(
                        SD_Vector2.RotatePoint(new SD_Vector2(transformedCenter.X, 0), edgeAngle) + currentVertex,
                        referenceSpatial.Angle);
                }
            }
        }
        
        if (!minPenetrationVector.Equals(SD_Vector2.Zero))
            return new PhysicsCollision(referenceSpatial, incidentSpatial, [collisionPoint], minPenetrationVector);
        
        // vertex case
        // sort vertices by distance to relative circular center
        minPenetrationVector = SD_Vector2.Zero;
        SD_Vector2 closestVertex = _points.MinBy(v => (v - relativeCenter).Magnitude());
        SD_Vector2 diffVector = closestVertex - relativeCenter;
        if (diffVector.Magnitude() <= collider.Radius) {
            collisionPoint = Matrix3X3.Rotation(referenceSpatial.Angle) * closestVertex;
            minPenetrationVector = Matrix3X3.Rotation(referenceSpatial.Angle) * diffVector.Normalize() * 
                                   (collider.Radius - diffVector.Magnitude());
        }
        if (!minPenetrationVector.Equals(SD_Vector2.Zero))
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