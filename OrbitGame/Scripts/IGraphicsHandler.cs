using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using qQEngine;

namespace OrbitGame;

public interface IGraphicsHandler
{
    public void DrawShadowMask(qQEngine.Camera camera, qQEngine.CircularLight light, qQEngine.Body occluder);
    public void DrawLighting(qQEngine.Camera camera, qQEngine.CircularLight light, RenderTarget2D shadowMask, RenderTarget2D occluderMask);
    public void DrawOccluder(qQEngine.Camera camera, qQEngine.Vec2Double position, qQEngine.IOccluder occluder, Color colour);
    public void DrawCircle(Vector2 center, float radius, Color colour);
    public void DrawPoly(List<Vector2> points, Color colour);
    public void DrawPath(IEnumerable<Vector2> points, Color colour);
    public void SD_DrawPath(Camera camera, IEnumerable<Vec2<SDecimal>> points, Color colour);
    public void DrawLine(Vector2 start, Vector2 end, Color colour);
    public void DrawLineR(Vector2 start, Vector2 displacement, Color colour);
    public void SD_DrawLine(Camera camera, Vec2<SDecimal> start, Vec2<SDecimal> end, Color colour);
    public void SD_DrawLineR(Camera camera, Vec2<SDecimal> start, Vec2<SDecimal> displacement, Color colour);
    public void DrawPoint(Vector2 position, Color colour);
    public void SD_DrawPoint(Camera camera, Vec2<SDecimal> position, Color colour);
    public void DrawScreenMesh(Dictionary<string, object> shaderParameters, Effect? effect);
    public void DrawMesh(IMesh mesh, Matrix transform, Dictionary<string, object> shaderParameters, Effect? effect = null);
    public void DrawText(SpriteFont spriteFont, string text, Vector2 position, Color colour);
    public void DrawText(SpriteFont spriteFont, string text, Vector2 position, Vector2 scale, float rotation, Color colour);
}