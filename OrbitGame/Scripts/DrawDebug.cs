using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

public delegate void DrawOperation(GraphicsDevice graphics, Camera camera);

public static class DrawDebug
{
    public static readonly Color Red = Color.Red;
    public static readonly Color Orange = Color.Orange;
    public static readonly Color Yellow = Color.Yellow;
    public static readonly Color Green = Color.Green;
    public static readonly Color Blue = Color.DodgerBlue;
    public static readonly Color Purple = Color.Purple;
    public static readonly Color White = Color.White;
    
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