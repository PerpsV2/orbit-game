using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public delegate void DrawOperation(GraphicsDevice graphics, Camera camera);

public static class DrawDebug
{
    private static readonly List<DrawOperation> DrawOperationBuffer = new();

    public static void Add(DrawOperation drawOperation)
    {
        DrawOperationBuffer.Add(drawOperation);
    }

    public static void ClearBuffer()
    {
        DrawOperationBuffer.Clear();
    }

    public static void Draw(GraphicsDevice graphics, Camera camera)
    {
        foreach (var operation in DrawOperationBuffer)
            operation(graphics, camera);
    }
}