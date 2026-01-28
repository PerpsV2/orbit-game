using System.Collections.Generic;

namespace OrbitGame;

/*public delegate void DrawOperation(SKCanvas canvas, Camera camera);

public static class DebugCanvas
{
    public static readonly SKPaint Red = new() { Color = SKColors.Red, StrokeWidth = 4 };
    public static readonly SKPaint Orange = new() { Color = SKColors.Orange, StrokeWidth = 4 };
    public static readonly SKPaint Yellow = new() { Color = SKColors.Yellow, StrokeWidth = 4 };
    public static readonly SKPaint Green = new() { Color = SKColors.Green, StrokeWidth = 4 };
    public static readonly SKPaint Blue = new() { Color = SKColors.DodgerBlue, StrokeWidth = 4 };
    public static readonly SKPaint Purple = new() { Color = SKColors.Purple, StrokeWidth = 4 };
    public static readonly SKPaint White = new() { Color = SKColors.White, StrokeWidth = 4 };
    
    private static readonly List<DrawOperation> DrawOperationBuffer = new();

    public static void Add(DrawOperation drawOperation)
    {
        DrawOperationBuffer.Add(drawOperation);
    }

    public static void ClearBuffer()
    {
        DrawOperationBuffer.Clear();
    }

    public static void Draw(SKCanvas canvas, Camera camera)
    {
        foreach (var operation in DrawOperationBuffer)
            operation(canvas, camera);
    }
}*/