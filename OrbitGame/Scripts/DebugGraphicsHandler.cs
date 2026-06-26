using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public class DebugGraphicsHandler : IGraphicsHandler
{
    public readonly record struct DrawPolyCall(List<Vector2> Points, Color Color);
    public readonly record struct DrawLineCall(Vector2 Start, Vector2 End, Color Colour);
    public readonly record struct DrawPointCall(Vector2 Position, Color Colour);
    public readonly record struct DrawMeshCall(
        IMesh Mesh,
        Matrix Transform,
        Dictionary<string, object> ShaderParameters
    );
    
    public List<DrawPolyCall> DrawPolyCalls = [];
    public List<DrawLineCall> DrawLineCalls = [];
    public List<DrawPointCall> DrawPointCalls = [];
    public List<DrawMeshCall> DrawMeshCalls = [];

    public void DrawBody(qQEngine.Camera camera, qQEngine.Body body, Color colour)
    {
        throw new NotImplementedException();
    }
    
    public void DrawPoly(List<Vector2> points, Color colour)
    {
        DrawPolyCalls.Add(new DrawPolyCall(points, colour));
    }

    public void DrawPath(IEnumerable<Vector2> points, Color colour)
    {
        throw new System.NotImplementedException();
    }

    public void SD_DrawPath(Camera camera, IEnumerable<Vec2<SDecimal>> points, Color colour)
    {
        throw new System.NotImplementedException();
    }

    public void DrawLine(Vector2 start, Vector2 end, Color colour)
    {
        DrawLineCalls.Add(new DrawLineCall(start, end, colour));
    }

    public void DrawLineR(Vector2 start, Vector2 displacement, Color colour)
    {
        DrawLine(start, start + displacement, colour);
    }

    public void SD_DrawLine(Camera camera, Vec2<SDecimal> start, Vec2<SDecimal> end, Color colour)
    {
        DrawLine(camera.ConvertToScreenCoordinates(start), camera.ConvertToScreenCoordinates(end), colour);
    }

    public void SD_DrawLineR(Camera camera, Vec2<SDecimal> start, Vec2<SDecimal> displacement, Color colour)
    {
        SD_DrawLine(camera, start, start + displacement, colour);
    }

    public void DrawPoint(Vector2 position, Color colour)
    {
        DrawPointCalls.Add(new DrawPointCall(position, colour));
    }

    public void SD_DrawPoint(Camera camera, Vec2<SDecimal> position, Color colour)
    {
        DrawPoint(camera.ConvertToScreenCoordinates(position), colour);
    }

    public void DrawMesh(IMesh mesh, Matrix transform, Dictionary<string, object> shaderParameters, Effect? effect)
    {
        DrawMeshCalls.Add(new DrawMeshCall(mesh, transform, shaderParameters));
    }

    public void DrawText(SpriteFont spriteFont, string text, Vector2 position, Color colour)
    {
        throw new System.NotImplementedException();
    }

    public void DrawText(SpriteFont spriteFont, string text, Vector2 position, Vector2 scale, float rotation, Color colour)
    {
        throw new System.NotImplementedException();
    }

    public void ResetCalls()
    {
        DrawPolyCalls.Clear();
        DrawMeshCalls.Clear();
        DrawLineCalls.Clear();
        DrawPointCalls.Clear();
    }
}