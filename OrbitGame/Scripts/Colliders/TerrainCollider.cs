using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class TerrainCollider
{
    private Vec2Double _endSegment;
    
    public TerrainCollider(Vec2Double endSegment)
    {
        _endSegment = endSegment;
    }

    public PhysicsCollision? IntersectsWith(SpatialInfo reference, SpatialInfo incident)
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
            g.SD_DrawLineR(cam, reference.Position, -penetrationVector, Color.Orange);
        });

        return new PhysicsCollision(reference, incident, [], penetrationVector);
    }
}