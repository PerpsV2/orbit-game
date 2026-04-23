using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class TerrainCollider
{
    private Vec2Double[] _segments;
    
    public TerrainCollider(Vec2Double[] segments)
    {
        _segments = segments;
    }

    /*public PhysicsCollision? IntersectsWith(SpatialInfo reference, SpatialInfo incident)
    {
        Vec2Double relPos = (Vec2Double)(reference.Position - incident.Position);

        double intersectionX = (relPos.Y + _endSegment.X * relPos.X / _endSegment.Y) /
                               (_endSegment.X / _endSegment.Y + _endSegment.Y / _endSegment.X);
        intersectionX = Math.Clamp(intersectionX, 0, _endSegment.X);
        Vec2Double surfaceImpactPoint = new(intersectionX, _endSegment.Y / _endSegment.X * 
            (intersectionX - _endSegment.X) + _endSegment.Y);
        Vec2Double penetrationVector = relPos - surfaceImpactPoint;
        
        DrawDebug.Add(() =>
        {
            Camera cam = OrbitGame.Camera;
            IGraphicsHandler g = OrbitGame.Graphics;
            
            g.SD_DrawLineR(cam, incident.Position, _endSegment, Color.Red);
            g.SD_DrawPoint(cam, incident.Position, Color.Red);
            g.SD_DrawPoint(cam, incident.Position + _endSegment, Color.Red);
        });

        if (relPos.Y <= _endSegment.Y / _endSegment.X * (relPos.X - _endSegment.X) + _endSegment.Y)
        {
            DrawDebug.Add(() =>
            {
                Camera cam = OrbitGame.Camera;
                IGraphicsHandler g = OrbitGame.Graphics;
                g.SD_DrawLineR(cam, reference.Position, -penetrationVector, Color.Orange);
            });
            return new PhysicsCollision(reference, incident, [], penetrationVector);
        }
        return null;
    }*/
    
    public PhysicsCollision? IntersectsWith(ConvexCollider collider, SpatialInfo reference, SpatialInfo incident)
    {
        Vec2Double minPenetrationVector = new Vec2Double(0, 0);
        Vec2Double collisionPoint = new Vec2Double(0, 0);

        for (int i = 1; i < _segments.Length; ++i)
        {
            Vec2Double segment = _segments[i] - _segments[i - 1];
            foreach (var point in collider.Points)
            {
                Vec2Double relPos = (Vec2Double)(Vec2Double.RotatePoint(point, reference.Angle) + reference.Position -
                                                 incident.Position - _segments[i - 1]);

                if (relPos.Y <= segment.Y / segment.X * (relPos.X - segment.X) + segment.Y &&
                    relPos.X >= 0 && relPos.X <= segment.X)
                {
                    double intersectionX = (relPos.Y + segment.X * relPos.X / segment.Y) /
                                           (segment.X / segment.Y + segment.Y / segment.X);
                    Vec2Double surfaceImpactPoint = new(intersectionX, segment.Y / segment.X *
                        (intersectionX - segment.X) + segment.Y);
                    Vec2Double penetrationVector = relPos - surfaceImpactPoint;
                    if (penetrationVector.MagnitudeSquared() > minPenetrationVector.MagnitudeSquared())
                    {
                        minPenetrationVector = penetrationVector;
                        collisionPoint = point;
                    }
                }
            }
        }

        DrawDebug.Add(() =>
        {
            Camera cam = OrbitGame.Camera;
            IGraphicsHandler g = OrbitGame.Graphics;

            g.SD_DrawLineR(cam, incident.Position, _segments[0], Color.Red);
            for (int i = 0; i < _segments.Length - 1; ++i)
            {
                g.SD_DrawLine(cam, incident.Position + _segments[i], incident.Position + _segments[i + 1], Color.Red);
                g.SD_DrawPoint(cam, incident.Position + _segments[i], Color.Red);
            }
            g.SD_DrawLineR(cam, reference.Position + Vec2Double.RotatePoint(collisionPoint, reference.Angle), 
                -minPenetrationVector, Color.Orange);
        });

        if (minPenetrationVector == new Vec2Double(double.PositiveInfinity, double.PositiveInfinity))
            return null;
        return new PhysicsCollision(reference, incident, [collisionPoint], minPenetrationVector);
    }
}