using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

public class ConvexCollider : CompactCollider
{
    private readonly BoundingBox _defaultBoundingBox;
    private readonly SD_Vector2[] _points;

    public ConvexCollider(SD_Vector2[] points)
    {
        _points = points;
        ScientificDecimal maxRadius = _points.Select(x => x.Magnitude()).Max();
        _defaultBoundingBox = new BoundingBox(SD_Vector2.Zero, maxRadius, maxRadius);
    }

    public override void CalculateInertia(ScientificDecimal mass)
    {
        Inertia = Utils.CalculateConvexInertia(_points, mass);
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
        SD_Vector2 relativeCenter = Matrix3X3.Rotation(incidentSpatial.Angle - referenceSpatial.Angle) * 
                                    (incidentSpatial.Position - referenceSpatial.Position);
        
        // // edge case (literally)
        // // TODO: fix cases where one object is fully within the other
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
                ScientificDecimal penetrationDistance = -collider.Radius + transformedCenter.Y;
                if (penetrationDistance.Abs() < minPenetrationVector.Magnitude() || minPenetrationVector.Equals(SD_Vector2.Zero))
                {
                    minPenetrationVector = Matrix3X3.Rotation(referenceSpatial.Angle) * 
                                           SD_Vector2.FromPolar(edgeAngle + Math.PI / 2, penetrationDistance);
                    collisionPoint = Matrix3X3.Rotation(referenceSpatial.Angle) * invTransformation * 
                                     new SD_Vector2(transformedCenter.X, 0);
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