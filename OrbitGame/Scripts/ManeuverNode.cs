using System;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class ManeuverNode(KeplerOrbitPoint point, Vec2<SDecimal> velocity) : IGameDrawable
{
    private readonly KeplerOrbitPoint _point = point;
    private readonly Vec2<SDecimal> _velocity = velocity;
    public double TrueAnomaly => _point.TrueAnomaly;

    public KeplerOrbit GenerateAppliedKeplerOrbit(SDecimal currentTime)
    {
        KeplerOrbit orbit = _point.ConicPath.Orbit ?? throw new NullReferenceException("ManeuverNode has no orbit");
        SDecimal maneuverNodeTime = _point.GetNextTime(currentTime);
        SpatialInfo nodeSpatialInfo = orbit.GetSpatialInfoAtTime(maneuverNodeTime);
        nodeSpatialInfo.Velocity += _velocity;
        return new KeplerOrbit(
            orbit.Body, orbit.Parent, nodeSpatialInfo, orbit.Parent.SpatialInfo, maneuverNodeTime
        );
    }

    public void Draw()
    {
        Camera camera = OrbitGame.Camera;
        IGraphicsHandler graphicsDevice = OrbitGame.Graphics;
        
        graphicsDevice.SD_DrawPoint(camera, _point.GetWorldPosition(), Color.Chartreuse);
        graphicsDevice.DrawLineR(camera.ConvertToScreenCoordinates(_point.GetWorldPosition()), 
            (Vector2)_velocity.Normalize() * 20, Color.Chartreuse);
    }

    public void DrawCollider()
        => Draw();
}