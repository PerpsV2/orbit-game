using System;
using System.Collections.Generic;
using System.Linq;

namespace OrbitGame;

public class ConvexCollider : CompactCollider
{
    private readonly SD_Vector2[] _points;

    public ConvexCollider(SD_Vector2[] points)
    {
        _points = points.Distinct().ToArray();
    }

    public override void CalculateInertia(ScientificDecimal mass)
    {
        Inertia = Utils.CalculateConvexInertia(_points, mass);
    }

    public override RectangularCollider GetBoundingBox()
    {
        SD_Vector2[] points = _points;
        ScientificDecimal minX = points.MinBy(v => v.X).X;
        ScientificDecimal maxX = points.MaxBy(v => v.X).X;
        ScientificDecimal minY = points.MinBy(v => v.Y).Y;
        ScientificDecimal maxY = points.MaxBy(v => v.Y).Y;

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
        SD_Vector2 relativeCenter = Matrix3X3.Rotation(-referenceSpatial.Angle) * 
                                    Matrix3X3.Translation(-referenceSpatial.Position) * incidentSpatial.Position;

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
                    minPenetrationVector = Matrix3X3.Rotation(referenceSpatial.Angle) * 
                                           SD_Vector2.FromPolar(edgeAngle + Math.PI / 2, -penetrationDistance);
                    collisionPoint = Matrix3X3.Rotation(referenceSpatial.Angle) * invTransformation *
                                     new SD_Vector2(transformedCenter.X, 0);
                }
            }
        }

        if (!minPenetrationVector.Equals(SD_Vector2.Zero))
            return new PhysicsCollision(referenceSpatial, incidentSpatial, [collisionPoint], minPenetrationVector);

        // vertex case
        minPenetrationVector = SD_Vector2.Zero;
        SD_Vector2[] sortedPoints = _points.OrderBy(v => (v - relativeCenter).Magnitude()).ToArray();

        SD_Vector2 diffVector = sortedPoints[0] - relativeCenter;
        if (diffVector.Magnitude() <= collider.Radius) {
            collisionPoint = Matrix3X3.Rotation(referenceSpatial.Angle) * sortedPoints[0];
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