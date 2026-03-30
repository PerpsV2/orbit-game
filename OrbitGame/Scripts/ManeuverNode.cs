using System;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class ManeuverNode : IGameDrawable
{
    private readonly KeplerOrbitPoint _point;
    private Vec2<SDecimal> _velocity;

    public static ManeuverNode? SelectedNode;
    private static SDecimal? _minMouseDistanceToNode = SDecimal.PosInfinity; 
    
    public double TrueAnomaly => _point.TrueAnomaly;

    public ManeuverNode(KeplerOrbitPoint point, Vec2<SDecimal> velocity)
    {
        _point = point;
        _velocity = velocity;
        
        MouseHandler.MouseClickDown += ManeuverNode_MouseClickDown;
        MouseHandler.MouseDown += ManeuverNode_MouseDown;
        OrbitGame.UpdateFrame += ManeuverNode_UpdateFrame;
    }

    private void ManeuverNode_UpdateFrame(object? sender, EventArgs e)
    {
        _minMouseDistanceToNode = SDecimal.PosInfinity;
    }

    private void ManeuverNode_MouseDown(object? sender, MouseEventArgs e)
    {
        if (this != SelectedNode) return;
        Camera camera = OrbitGame.Camera;
        Vector2 nodeScreenPosition = camera.ConvertToScreenCoordinates(_point.GetWorldPosition());
        Vector2 mouseDisplacement = e.Position - nodeScreenPosition;
        _velocity += new Vec2<SDecimal>(mouseDisplacement.X, mouseDisplacement.Y);
    }

    private void ManeuverNode_MouseClickDown(object? sender, MouseEventArgs e)
    {
        Camera camera = OrbitGame.Camera;
        Vec2<SDecimal> mouseWorldPosition = camera.ConvertToWorldCoordinates(e.Position);
        SDecimal mouseDistanceSquared = (_point.GetWorldPosition() - mouseWorldPosition).MagnitudeSquared();

        if (mouseDistanceSquared < _minMouseDistanceToNode)
        {
            _minMouseDistanceToNode = mouseDistanceSquared;
            float screenMouseDistanceToOrbit = camera.ConvertToScreenDistance(SDecimal.Sqrt(mouseDistanceSquared));
            SelectedNode = screenMouseDistanceToOrbit < 10 ? this : null;
        }
    }

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

        graphicsDevice.SD_DrawPoint(camera, _point.GetWorldPosition(), 
            this == SelectedNode ? Color.Purple : Color.Chartreuse);
        graphicsDevice.DrawLineR(camera.ConvertToScreenCoordinates(_point.GetWorldPosition()), 
            (Vector2)_velocity.Normalize() * 20, Color.Chartreuse);
    }

    public void DrawCollider()
        => Draw();
}