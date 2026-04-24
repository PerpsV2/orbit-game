using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class TestTerrainCollider
{
    private (double angle, SDecimal distance)[] _elevationPoints;
    
    public TestTerrainCollider((double, SDecimal)[] elevationPoints)
    {
        _elevationPoints = elevationPoints;
    }

    public (int startIndex, int endIndex) GetPointSegment(Vec2<SDecimal> point)
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

        throw new NotImplementedException();
    }

    public PhysicsCollision? IntersectsWith(ConvexCollider collider, SpatialInfo reference, SpatialInfo incident)
    {
        IEnumerable<Vec2Double> rotatedPoints = collider.Points.Select(x => Vec2Double.RotatePoint(x, reference.Angle));
        Vec2<SDecimal> minPenetrationVector = Vec2<SDecimal>.Zero;
        Vec2<SDecimal> collisionPoint = Vec2<SDecimal>.Zero;
        foreach (var point in rotatedPoints)
        {
            Vec2<SDecimal> relPos = point - incident.Position;
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
            
            DrawDebug.Add(() =>
            {
                Camera cam = OrbitGame.Camera;
                IGraphicsHandler g = OrbitGame.Graphics;

                g.SD_DrawLineR(cam, incident.Position, penetrationVector, Color.LightBlue);
                g.SD_DrawLineR(cam, incident.Position, relPos, Color.LightBlue);
            });

            if (penetrationVector.MagnitudeSquared() > minPenetrationVector.MagnitudeSquared() ||
                minPenetrationVector == Vec2<SDecimal>.Zero)
            {
                minPenetrationVector = penetrationVector;
                collisionPoint = point;
            }
        }

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
            g.SD_DrawLineR(cam, reference.Position + collisionPoint, minPenetrationVector, Color.Gray);
        });

        if (collisionPoint != Vec2<SDecimal>.Zero)
            return new PhysicsCollision(reference, incident, [(Vec2Double)collisionPoint], (Vec2Double)minPenetrationVector);
        return null;
    }
    
    public PhysicsCollision? IntersectsWith2(ConvexCollider collider, SpatialInfo reference, SpatialInfo incident)
    {
        Vec2Double[] rotatedPoints = collider.Points.Select(x => Vec2Double.RotatePoint(x, reference.Angle)).ToArray();
        Vec2<SDecimal> minPenetrationVector = Vec2<SDecimal>.Zero;
        Vec2<SDecimal> collisionPoint = Vec2<SDecimal>.Zero;
        for (int i = 0; i < rotatedPoints.Length; ++i)
        {
            (Vec2<SDecimal> start, Vec2<SDecimal> edge) edge = (
                rotatedPoints[i] - incident.Position, 
                rotatedPoints[(i + 1) % rotatedPoints.Length] - rotatedPoints[i]
            );
            
            Vec2<SDecimal> relPos = edge.start;
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
            Vec2<SDecimal> surfaceImpactPoint = new(interceptionXPoint, interceptionYPoint);
            Vec2<SDecimal> vertexPenetrationVector = surfaceImpactPoint - relPos;
            
            if (vertexPenetrationVector.MagnitudeSquared() > minPenetrationVector.MagnitudeSquared() ||
                minPenetrationVector == Vec2<SDecimal>.Zero)
            {
                minPenetrationVector = vertexPenetrationVector;
                collisionPoint = edge.start + incident.Position;
            }

            double edgeSlope = (double)(edge.edge.Y / edge.edge.X);
            if (double.IsInfinity(edgeSlope)) continue;
            interceptionXPoint = (edgeSlope * edgeSlope * relPos.X - edgeSlope * relPos.Y + 
                                  edgeSlope * segmentStart.Y + segmentStart.X) / (edgeSlope * edgeSlope + 1);
            interceptionYPoint = edgeSlope * (interceptionXPoint - relPos.X) + relPos.Y;
            Vec2<SDecimal> colliderImpactPoint = new(interceptionXPoint, interceptionYPoint);
            Vec2<SDecimal> edgePenetrationVector = colliderImpactPoint - segmentStart;
            
            if (edgePenetrationVector.MagnitudeSquared() > minPenetrationVector.MagnitudeSquared() ||
                minPenetrationVector == Vec2<SDecimal>.Zero)
            {
                minPenetrationVector = edgePenetrationVector;
                collisionPoint = segmentStart + incident.Position - minPenetrationVector;
            }

            var point = collisionPoint;
            var vector = minPenetrationVector;
            DrawDebug.Add(() =>
            {
                Camera cam = OrbitGame.Camera;
                IGraphicsHandler g = OrbitGame.Graphics;

                g.SD_DrawLineR(cam, point, vector, Color.LightBlue);
            });

            if (vertexPenetrationVector.MagnitudeSquared() > minPenetrationVector.MagnitudeSquared() ||
                minPenetrationVector == Vec2<SDecimal>.Zero)
            {
                minPenetrationVector = vertexPenetrationVector;
                collisionPoint = edge.start + incident.Position;
            }
        }

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
            //g.SD_DrawLineR(cam, reference.Position + collisionPoint, minPenetrationVector, Color.Gray);
        });

        if (collisionPoint != Vec2<SDecimal>.Zero)
            return new PhysicsCollision(reference, incident, [(Vec2Double)collisionPoint], (Vec2Double)minPenetrationVector);
        return null;
    }
}