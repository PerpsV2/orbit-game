using System;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class ManeuverNode : IGameDrawable
{
    public readonly KeplerOrbitPoint Point;
    private Vec2<SDecimal> _velocity;

    public static ManeuverNode? SelectedNode;
    private static SDecimal? _minMouseDistanceToNode = SDecimal.PositiveInfinity; 
    
    public double TrueAnomaly => Point.TrueAnomaly;

    public ManeuverNode(KeplerOrbitPoint point, Vec2<SDecimal> velocity)
    {
        Point = point;
        _velocity = velocity;
        
        MouseHandler.MouseClickDown += ManeuverNode_MouseClickDown;
        MouseHandler.MouseDown += ManeuverNode_MouseDown;
        OrbitGame.UpdateFrame += ManeuverNode_UpdateFrame;
    }

    private void ManeuverNode_UpdateFrame(object? sender, EventArgs e)
    {
        _minMouseDistanceToNode = SDecimal.PositiveInfinity;
    }

    private void ManeuverNode_MouseDown(object? sender, MouseEventArgs e)
    {
        if (this != SelectedNode) return;
        Camera camera = OrbitGame.Camera;
        Vector2 nodeScreenPosition = camera.ConvertToScreenCoordinates(Point.GetWorldPosition());
        Vector2 mouseDisplacement = e.Position - nodeScreenPosition;
        _velocity += new Vec2<SDecimal>(mouseDisplacement.X, mouseDisplacement.Y);
    }

    private void ManeuverNode_MouseClickDown(object? sender, MouseEventArgs e)
    {
        Camera camera = OrbitGame.Camera;
        Vec2<SDecimal> mouseWorldPosition = camera.ConvertToWorldCoordinates(e.Position);
        SDecimal mouseDistanceSquared = (Point.GetWorldPosition() - mouseWorldPosition).MagnitudeSquared();

        if (mouseDistanceSquared < _minMouseDistanceToNode)
        {
            _minMouseDistanceToNode = mouseDistanceSquared;
            float screenMouseDistanceToOrbit = camera.ConvertToScreenDistance(SDecimal.Sqrt(mouseDistanceSquared));
            SelectedNode = screenMouseDistanceToOrbit < 10 ? this : null;
        }
    }

    public KeplerOrbit GenerateAppliedKeplerOrbit(SDecimal currentTime)
    {
        KeplerOrbit orbit = Point.ConicPath.Orbit ?? throw new NullReferenceException("ManeuverNode has no orbit");
        SDecimal maneuverNodeTime = Point.GetNextTime(currentTime);
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

        Color nodeColour = this == SelectedNode ? Color.Chartreuse : Color.GreenYellow;
        graphicsDevice.SD_DrawPoint(camera, Point.GetWorldPosition(), nodeColour);
        Vector2 nodeScreenPosition = camera.ConvertToScreenCoordinates(Point.GetWorldPosition());
        Vector2 nodeHandleScreenOffset = (Vector2)_velocity.Normalize() * float.Log10((float)_velocity.Magnitude()) * 10;
        graphicsDevice.DrawLineR(nodeScreenPosition, nodeHandleScreenOffset, nodeColour);
        graphicsDevice.DrawText(OrbitGame.DefaultFont, ((double)_velocity.Magnitude()).ToString("N0"),
            nodeScreenPosition + nodeHandleScreenOffset, new Vector2(0.5f), 0, nodeColour);
    }

    public void DrawCollider()
        => Draw();
}