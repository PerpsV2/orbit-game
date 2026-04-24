using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;

namespace OrbitGame;

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