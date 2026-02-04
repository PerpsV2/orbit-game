using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

public class ConvexCollider(SD_Vector2[] points) : CompactCollider
{
    public override void CalculateInertia(ScientificDecimal mass)
    {
        Inertia = Utils.CalculateConvexInertia(points, mass);
    }

    public override RectangularCollider GetBoundingBox()
    {
        SD_Vector2[] points1 = points;
        ScientificDecimal minX = points1.MinBy(v => v.X).X;
        ScientificDecimal maxX = points1.MaxBy(v => v.X).X;
        ScientificDecimal minY = points1.MinBy(v => v.Y).Y;
        ScientificDecimal maxY = points1.MaxBy(v => v.Y).Y;

        SD_Vector2 topRight = new SD_Vector2(maxX, maxY);
        SD_Vector2 bottomLeft = new SD_Vector2(minX, minY);
        
        return new RectangularCollider(topRight, bottomLeft);
    }
    
    public static explicit operator RectangularCollider(ConvexCollider value)
        => value.GetBoundingBox();

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
        for (int i = 0; i < points.Length; ++i)
        {
            SD_Vector2 currentVertex = points[i];
            SD_Vector2 nextVertex = points[(i + 1) % points.Length];
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
        SD_Vector2 closestVertex = points.MinBy(v => (v - relativeCenter).Magnitude());
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

    protected override PhysicsCollision? IntersectsWith(RectangularCollider collider, SpatialInfo referenceSpatial, SpatialInfo incidentSpatial)
    {
        if (IsEmpty() || collider.IsEmpty()) return null;
        // convert rect collider to a convex collider and use the respective intersect method
        ConvexCollider convexRect = new ConvexCollider(
            [collider.TopLeft, collider.TopRight, collider.BottomRight, collider.BottomLeft]);
        return IntersectsWith(convexRect, referenceSpatial, incidentSpatial);
    }

    public override bool IsEmpty()
    {
        return false;
        throw new NotImplementedException();
    }
}