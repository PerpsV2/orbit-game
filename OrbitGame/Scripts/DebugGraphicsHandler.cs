using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public class DebugGraphicsHandler : IGraphicsHandler
{
    public void DrawPoly(List<Vector2> points, Color colour)
    {
        throw new System.NotImplementedException();
    }

    public void DrawLine(Vector2 start, Vector2 end, Color colour)
    {
        throw new System.NotImplementedException();
    }

    public void GS_DrawLine(Camera camera, SD_Vector2 start, SD_Vector2 end, Color colour)
    {
        throw new System.NotImplementedException();
    }

    public void GS_DrawLineR(Camera camera, SD_Vector2 start, SD_Vector2 displacement, Color colour)
    {
        throw new System.NotImplementedException();
    }

    public void GS_DrawPoint(Camera camera, SD_Vector2 position, Color colour)
    {
        throw new System.NotImplementedException();
    }

    public void DrawMesh(IMesh mesh, Matrix transform, Dictionary<string, object> shaderParameters)
    {
        throw new System.NotImplementedException();
    }
}