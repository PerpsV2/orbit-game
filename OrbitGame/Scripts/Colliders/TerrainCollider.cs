using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class TerrainCollider
{
    private readonly SDecimal _maxRadius;
    private readonly (double angle, SDecimal distance)[] _elevationPoints;
    
    public TerrainCollider((double angle, SDecimal distance)[] elevationPoints)
    {
        _elevationPoints = elevationPoints.OrderBy(x => x.angle).ToArray();
        _maxRadius = _elevationPoints.MaxBy(x => x.distance).distance;
    }

    public bool NearsWith(CompactCollider collider, SpatialInfo referenceSpatial, SpatialInfo colliderSpatial)
    {
        return (colliderSpatial.Position - referenceSpatial.Position).MagnitudeSquared() <
               SDecimal.Square(collider.MaxRadius + _maxRadius);
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

    private (Vec2<SDecimal> penetrationVector, Vec2<SDecimal> collisionPoint) GetEdgeCollision(
        ConvexCollider collider, 
        SpatialInfo reference,
        SpatialInfo incident)
    {
        Vec2Double[] rotatedPoints = collider.Points.Select(x => Vec2Double.RotatePoint(x, reference.Angle)).ToArray();
        Vec2<SDecimal> minEdgePenetrationVector = Vec2<SDecimal>.Zero;
        Vec2<SDecimal> collisionPoint = Vec2<SDecimal>.Zero;
        foreach (var point in rotatedPoints)
        {
            Vec2<SDecimal> relPos = point + reference.Position - incident.Position;
            (int startIndex, int endIndex) pointSurfaceLineSegment = GetPointSegment(relPos);
            Vec2<SDecimal> segmentStart = Vec2<SDecimal>.FromPolar(
                _elevationPoints[pointSurfaceLineSegment.startIndex].angle,
                _elevationPoints[pointSurfaceLineSegment.startIndex].distance
            );
            Vec2<SDecimal> segmentEnd = Vec2<SDecimal>.FromPolar(
                _elevationPoints[pointSurfaceLineSegment.endIndex].angle,
                _elevationPoints[pointSurfaceLineSegment.endIndex].distance
            );
            double segmentSlope = (double)((segmentEnd.Y - segmentStart.Y) / (segmentEnd.X - segmentStart.X));
            SDecimal interceptionXPoint = (segmentSlope * segmentSlope * segmentStart.X -
                                           segmentSlope * segmentStart.Y +
                                           segmentSlope * relPos.Y + relPos.X) / (segmentSlope * segmentSlope + 1);
            SDecimal interceptionYPoint = segmentSlope * (interceptionXPoint - segmentStart.X) + segmentStart.Y;
            Vec2<SDecimal> surfaceImpactPoint = new Vec2<SDecimal>(interceptionXPoint, interceptionYPoint);
            Vec2<SDecimal> penetrationVector = surfaceImpactPoint - relPos;

            if (Vec2<SDecimal>.Dot(penetrationVector, relPos).Negative)
                continue;

            if (penetrationVector.MagnitudeSquared() > minEdgePenetrationVector.MagnitudeSquared() ||
                minEdgePenetrationVector == Vec2<SDecimal>.Zero)
            {
                minEdgePenetrationVector = penetrationVector;
                collisionPoint = point;
            }
        }

        return (minEdgePenetrationVector, collisionPoint);
    }

    private (Vec2<SDecimal> penetrationVector, Vec2<SDecimal> collisionPoint) GetVertexCollision(
        ConvexCollider collider, 
        SpatialInfo reference,
        SpatialInfo incident)
    {
        Vec2<SDecimal> minVertexPenetrationVector = Vec2<SDecimal>.Zero;
        Vec2<SDecimal> collisionPoint = Vec2<SDecimal>.Zero;

        foreach (var point in _elevationPoints)
        {
            Vec2<SDecimal> cartesianPoint = Vec2<SDecimal>.FromPolar(point.angle, point.distance);

            Vec2<SDecimal> minPenetrationVector = new Vec2Double(double.PositiveInfinity, double.PositiveInfinity);
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
                Vec2<SDecimal> penetrationVector = Vec2<SDecimal>.Dot(forwardsPenetrationVector, cartesianPoint) > 0
                    ? forwardsPenetrationVector
                    : backwardsPenetrationVector;
                if (penetrationVector.MagnitudeSquared() < minPenetrationVector.MagnitudeSquared())
                    minPenetrationVector = penetrationVector;
            }

            if (!SDecimal.IsInfinity(minPenetrationVector.MagnitudeSquared()))
                if (minPenetrationVector.MagnitudeSquared() < minVertexPenetrationVector.MagnitudeSquared() ||
                    minVertexPenetrationVector == Vec2<SDecimal>.Zero)
                {
                    minVertexPenetrationVector = minPenetrationVector;
                    collisionPoint = cartesianPoint + incident.Position - minVertexPenetrationVector;
                }
        }

        return (minVertexPenetrationVector, collisionPoint);
    }
    
    
    public PhysicsCollision? IntersectsWith(ConvexCollider collider, SpatialInfo reference, SpatialInfo incident)
    {
        Vec2<SDecimal> totalPenetrationVector = Vec2<SDecimal>.Zero;
        Vec2<SDecimal> collisionPoint = Vec2<SDecimal>.Zero;
        int iterations = 0;
        while (iterations < 10)
        {
            var edgeCollision = GetEdgeCollision(collider, new(reference.Position + totalPenetrationVector, reference.Angle), incident);
            if (edgeCollision.penetrationVector != Vec2<SDecimal>.Zero)
            {
                totalPenetrationVector += edgeCollision.penetrationVector;
                collisionPoint = edgeCollision.collisionPoint;
            }
            
            var vertexCollision = GetVertexCollision(collider, new(reference.Position + totalPenetrationVector, reference.Angle), incident);
            if (vertexCollision.penetrationVector != Vec2<SDecimal>.Zero)
            {
                totalPenetrationVector += vertexCollision.penetrationVector;
                collisionPoint = vertexCollision.collisionPoint;
            }

            iterations++;

            if (vertexCollision.penetrationVector.MagnitudeSquared() < 0.0001 && 
                edgeCollision.penetrationVector.MagnitudeSquared() < 0.0001)
                break;
        }
        
        Console.WriteLine(iterations);

        DrawDebug.Add(() =>
        {
            Camera cam = OrbitGame.Camera;
            IGraphicsHandler g = OrbitGame.Graphics;

            for (int i = 0; i < _elevationPoints.Length; ++i)
            {
                int nextIndex = (i + 1) % _elevationPoints.Length;
                g.SD_DrawPoint(cam, incident.Position + Vec2<SDecimal>.FromPolar(_elevationPoints[i].angle, _elevationPoints[i].distance), Color.Red);
                g.SD_DrawLine(cam,
                    incident.Position + Vec2<SDecimal>.FromPolar(_elevationPoints[i].angle, _elevationPoints[i].distance),
                    incident.Position + Vec2<SDecimal>.FromPolar(_elevationPoints[nextIndex].angle, _elevationPoints[nextIndex].distance),
                    Color.Red);
            }
            g.SD_DrawLineR(cam, reference.Position, totalPenetrationVector, Color.Gray);
        });

        if (collisionPoint != Vec2<SDecimal>.Zero)
            return new PhysicsCollision(reference, incident, [(Vec2Double)collisionPoint], (Vec2Double)totalPenetrationVector);
        return null;
    }
}

/*
public class TerrainCollider
{
    private Vec2Double[] _segments;
    
    public TerrainCollider(Vec2Double[] segments)
    {
        _segments = segments;
    }

    public PhysicsCollision? IntersectsWith(SpatialInfo reference, SpatialInfo incident)
    {
        return null;
        // Vec2Double relPos = (Vec2Double)(reference.Position - incident.Position);
        //
        // double intersectionX = (relPos.Y + _endSegment.X * relPos.X / _endSegment.Y) /
        //                        (_endSegment.X / _endSegment.Y + _endSegment.Y / _endSegment.X);
        // intersectionX = Math.Clamp(intersectionX, 0, _endSegment.X);
        // Vec2Double surfaceImpactPoint = new(intersectionX, _endSegment.Y / _endSegment.X * 
        //     (intersectionX - _endSegment.X) + _endSegment.Y);
        // Vec2Double penetrationVector = relPos - surfaceImpactPoint;
        //
        // DrawDebug.Add(() =>
        // {
        //     Camera cam = OrbitGame.Camera;
        //     IGraphicsHandler g = OrbitGame.Graphics;
        //     
        //     g.SD_DrawLineR(cam, incident.Position, _endSegment, Color.Red);
        //     g.SD_DrawPoint(cam, incident.Position, Color.Red);
        //     g.SD_DrawPoint(cam, incident.Position + _endSegment, Color.Red);
        //     g.SD_DrawLineR(cam, reference.Position, -penetrationVector, Color.Orange);
        // });
        //
        // return new PhysicsCollision(reference, incident, [], penetrationVector);
    }

    public (Vec2Double start, Vec2Double end)? GetPointSegment(Vec2Double point)
    {
        for (int i = 0; i < _segments.Length - 1; ++i)
        {
            if (point.X > _segments[i].X && point.X <= _segments[i + 1].X)
                return (_segments[i], _segments[i + 1]);
        }
        return null;
    }

    public Vec2Double? GetColliderPenetrationVector(ConvexCollider collider, SpatialInfo reference, SpatialInfo incident)
    {
        HashSet<(Vec2Double start, Vec2Double end)> intersectingSegmentsSet = new();
        List<Vec2Double> rotatedColliderPoints = collider.Points.Select(x => Vec2Double.RotatePoint(x, reference.Angle)).ToList();
        foreach (var point in rotatedColliderPoints)
        {
            (Vec2Double start, Vec2Double end)? intersectingSegment =
                GetPointSegment((Vec2Double)(reference.Position + point - incident.Position));
            if (intersectingSegment is not null)
                intersectingSegmentsSet.Add(intersectingSegment.Value);
        }

        List<(Vec2Double collisionPoint, Vec2Double penetrationVector)> collisionResolutions = [];
        foreach (var segment in intersectingSegmentsSet)
        {
            (Vec2Double collisionPoint, Vec2Double penetrationVector)? collisionResolution = null;
            foreach (var point in rotatedColliderPoints)
            {
                Vec2Double relPos = (Vec2Double)(point + reference.Position - incident.Position - segment.start);
                Vec2Double relSegment = segment.end - segment.start;
                double intersectionX = (relPos.Y + relSegment.X * relPos.X / relSegment.Y) /
                                       (relSegment.X / relSegment.Y + relSegment.Y / relSegment.X);
                Vec2Double surfaceImpactPoint = new(intersectionX, relSegment.Y / relSegment.X *
                    (intersectionX - relSegment.X) + relSegment.Y);
                Vec2Double penetrationVector = relPos - surfaceImpactPoint;
                if (relPos.Y < relSegment.Y / relSegment.X * (relPos.X - relSegment.X) + relSegment.Y && 
                    relPos.X > 0 && relPos.X <= relSegment.X)
                    if (penetrationVector.MagnitudeSquared() > 0.000001)
                        if (penetrationVector.MagnitudeSquared() > 
                            (collisionResolution?.penetrationVector.MagnitudeSquared() ?? 0))
                            collisionResolution = (point, penetrationVector);
            }
            if (collisionResolution is not null)
                collisionResolutions.Add(collisionResolution.Value);
        }

        (Vec2Double collisionPoint, Vec2Double penetrationVector)? minCollisionResolution = null;
        if (collisionResolutions.Count > 0)
            minCollisionResolution = collisionResolutions.MinBy(x => x.penetrationVector.MagnitudeSquared());

        return minCollisionResolution?.penetrationVector;
    }

    public PhysicsCollision? IntersectsWith(ConvexCollider collider, SpatialInfo reference, SpatialInfo incident)
    {
        Vec2Double penetrationVector = Vec2Double.Zero;
        while (true)
        {
            Vec2Double? p = GetColliderPenetrationVector(collider,
                new SpatialInfo(reference.Position - penetrationVector, reference.Angle), incident);
            if (p is null) break;
            penetrationVector += p.Value;
        }
        
        DrawDebug.Add(() => {
            Camera cam = OrbitGame.Camera;
            IGraphicsHandler g = OrbitGame.Graphics;

            for (int i = 0; i < _segments.Length - 1; ++i)
            {
                g.SD_DrawLine(cam, incident.Position + _segments[i], incident.Position + _segments[i + 1], Color.Red);
                g.SD_DrawPoint(cam, incident.Position + _segments[i], Color.Red);
            }
            g.SD_DrawPoint(cam, incident.Position + _segments[^1], Color.Red);
            
            g.SD_DrawLineR(cam, reference.Position, -penetrationVector, Color.Orange);
        });
        
        if (penetrationVector != Vec2Double.Zero)
            return new PhysicsCollision(reference, incident, [], penetrationVector);
        return null;
    }
}
*/