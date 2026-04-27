using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class TerrainCollider : CompactCollider
{
    private readonly (double angle, double distance)[] _elevationPoints;
    
    public TerrainCollider((double angle, double distance)[] elevationPoints)
    {
        _elevationPoints = elevationPoints.OrderBy(x => x.angle).ToArray();
        MaxRadius = _elevationPoints.MaxBy(x => x.distance).distance;
    }

    public override SDecimal CalculateInertia(SDecimal mass)
    {
        return SDecimal.PositiveInfinity;
    }

    protected override BoundingBox GetBoundingBox()
    {
        return new BoundingBox(new Vec2Double(0, 0), MaxRadius, MaxRadius);
    }

    public override bool NearsWith(CompactCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial)
    {
        return (colliderSpatial.Position - referenceSpatial.Position).MagnitudeSquared() <
               SDecimal.Square(collider.MaxRadius + MaxRadius);
    }

    public override PointCollision? IntersectsWith(Vec2<SDecimal> point, SpatialInfo referenceSpatial)
    {
        throw new NotImplementedException();
    }

    private (int startIndex, int endIndex) GetPointSegment(Vec2<SDecimal> point)
    {
        for (int i = 0; i < _elevationPoints.Length; ++i)
        {
            double segmentStartAngle = _elevationPoints[i].angle;
            double segmentEndAngle = i == _elevationPoints.Length - 1 ? 
                Math.Tau + _elevationPoints[0].angle : _elevationPoints[i + 1].angle;

            double pointAngle = point.Direction();
            if (pointAngle > segmentStartAngle && pointAngle <= segmentEndAngle || 
                pointAngle + Math.Tau > segmentStartAngle && pointAngle + Math.Tau <= segmentEndAngle)
                return (i, (i + 1) % _elevationPoints.Length);
        }

        throw new UnreachableException("Point is outside of the range of every terrain segment");
    }

    private (Vec2Double penetrationVector, Vec2Double collisionPoint) GetEdgeCollision(
        ConvexCollider collider, 
        SpatialInfo reference,
        SpatialInfo incident)
    {
        Vec2Double[] rotatedPoints = collider.Points.Select(x => Vec2Double.RotatePoint(x, reference.Angle)).ToArray();
        Vec2Double minEdgePenetrationVector = Vec2Double.Zero;
        Vec2Double collisionPoint = Vec2Double.Zero;
        foreach (var point in rotatedPoints)
        {
            Vec2Double relPos = (Vec2Double)(point + reference.Position - incident.Position);
            (int startIndex, int endIndex) pointSurfaceLineSegment = GetPointSegment(relPos);
            Vec2Double segmentStart = Vec2Double.FromPolar(
                _elevationPoints[pointSurfaceLineSegment.startIndex].angle,
                _elevationPoints[pointSurfaceLineSegment.startIndex].distance
            );
            Vec2Double segmentEnd = Vec2Double.FromPolar(
                _elevationPoints[pointSurfaceLineSegment.endIndex].angle,
                _elevationPoints[pointSurfaceLineSegment.endIndex].distance
            );
            double segmentSlope = ((segmentEnd.Y - segmentStart.Y) / (segmentEnd.X - segmentStart.X));
            double interceptionXPoint = (segmentSlope * segmentSlope * segmentStart.X -
                                           segmentSlope * segmentStart.Y +
                                           segmentSlope * relPos.Y + relPos.X) / (segmentSlope * segmentSlope + 1);
            double interceptionYPoint = segmentSlope * (interceptionXPoint - segmentStart.X) + segmentStart.Y;
            Vec2Double surfaceImpactPoint = new Vec2Double(interceptionXPoint, interceptionYPoint);
            Vec2Double penetrationVector = surfaceImpactPoint - relPos;

            if (double.IsNegative(Vec2Double.Dot(penetrationVector, relPos)))
                continue;

            if (penetrationVector.MagnitudeSquared() > minEdgePenetrationVector.MagnitudeSquared() ||
                minEdgePenetrationVector == Vec2Double.Zero)
            {
                minEdgePenetrationVector = penetrationVector;
                collisionPoint = point;
            }
        }

        return (minEdgePenetrationVector, collisionPoint);
    }

    private (Vec2Double penetrationVector, Vec2Double collisionPoint) GetVertexCollision(
        ConvexCollider collider, 
        SpatialInfo reference,
        SpatialInfo incident)
    {
        Vec2Double minVertexPenetrationVector = Vec2Double.Zero;
        Vec2Double collisionPoint = Vec2Double.Zero;

        foreach (var point in _elevationPoints)
        {
            Vec2Double cartesianPoint = Vec2Double.FromPolar(point.angle, point.distance);

            Vec2Double minPenetrationVector = new Vec2Double(double.PositiveInfinity, double.PositiveInfinity);
            List<Vec2Double> rotatedPoint =
                collider.Points.Select(x => Vec2Double.RotatePoint(x, reference.Angle)).ToList();
            for (int i = 0; i < rotatedPoint.Count; ++i)
            {
                Vec2Double currentVertex = rotatedPoint[i];
                Vec2Double nextVertex = rotatedPoint[(i + 1) % rotatedPoint.Count];
                double edgeAngle = Vec2Double.Direction(currentVertex, nextVertex);

                double[] projectedColliderPoints = rotatedPoint
                    .Select(x => Vec2Double.RotatePoint(x, -edgeAngle - Math.PI / 2).X).ToArray();
                (double min, double max) projectedRange =
                    (projectedColliderPoints.Min(), projectedColliderPoints.Max());
                double projectedPoint = Vec2Double
                    .RotatePoint((Vec2Double)(cartesianPoint + incident.Position - reference.Position),
                        -edgeAngle - Math.PI / 2).X;
                if (projectedPoint < projectedRange.min || projectedPoint > projectedRange.max)
                {
                    minPenetrationVector = new Vec2Double(double.PositiveInfinity, double.PositiveInfinity);
                    break;
                }

                double forwardsPenetrationDistance = projectedRange.max - projectedPoint;
                double backwardsPenetrationDistance = projectedRange.min - projectedPoint;
                Vec2Double forwardsPenetrationVector =
                    Vec2Double.FromPolar(edgeAngle - Math.PI / 2, forwardsPenetrationDistance);
                Vec2Double backwardsPenetrationVector =
                    Vec2Double.FromPolar(edgeAngle - Math.PI / 2, backwardsPenetrationDistance);
                Vec2Double penetrationVector = Vec2Double.Dot(forwardsPenetrationVector, cartesianPoint) > 0
                    ? forwardsPenetrationVector
                    : backwardsPenetrationVector;
                if (penetrationVector.MagnitudeSquared() < minPenetrationVector.MagnitudeSquared())
                    minPenetrationVector = penetrationVector;
            }

            if (!double.IsInfinity(minPenetrationVector.MagnitudeSquared()))
                if (minPenetrationVector.MagnitudeSquared() < minVertexPenetrationVector.MagnitudeSquared() ||
                    minVertexPenetrationVector == Vec2<SDecimal>.Zero)
                {
                    minVertexPenetrationVector = minPenetrationVector;
                    collisionPoint = (Vec2Double)(cartesianPoint + incident.Position - minVertexPenetrationVector);
                }
        }

        return (minVertexPenetrationVector, collisionPoint);
    }


    protected override PhysicsCollision? IntersectsWith(CircularCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial)
    {
        throw new NotImplementedException();
    }

    protected override PhysicsCollision? IntersectsWith(ConvexCollider collider, SpatialInfo reference, SpatialInfo incident)
    {
        Vec2Double totalPenetrationVector = Vec2Double.Zero;
        Vec2Double collisionPoint = Vec2Double.Zero;
        int iterations = 0;
        while (iterations < 10)
        {
            var edgeCollision = GetEdgeCollision(collider, new(reference.Position + totalPenetrationVector, reference.Angle), incident);
            if (edgeCollision.penetrationVector != Vec2Double.Zero)
            {
                totalPenetrationVector += edgeCollision.penetrationVector;
                collisionPoint = edgeCollision.collisionPoint;
            }
            
            var vertexCollision = GetVertexCollision(collider, new(reference.Position + totalPenetrationVector, reference.Angle), incident);
            if (vertexCollision.penetrationVector != Vec2Double.Zero)
            {
                totalPenetrationVector += vertexCollision.penetrationVector;
                collisionPoint = vertexCollision.collisionPoint;
            }

            iterations++;

            if (vertexCollision.penetrationVector.MagnitudeSquared() < 0.0001 && 
                edgeCollision.penetrationVector.MagnitudeSquared() < 0.0001)
                break;
        }

        if (collisionPoint != Vec2Double.Zero)
            return new PhysicsCollision(reference, incident, [collisionPoint], totalPenetrationVector);
        return null;
    }

    public override bool IsEmpty()
    {
        return false;
    }
}