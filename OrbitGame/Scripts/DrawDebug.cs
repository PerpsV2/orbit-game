using System;
using System.Collections.Generic;

namespace OrbitGame;

/// <summary>
/// Class for drawing debugging objects in the game scene.
/// </summary>
public static class DrawDebug
{
    private static readonly List<Action> DrawOperationBuffer = new();

    public static void Add(Action drawOperation)
    {
        DrawOperationBuffer.Add(drawOperation);
    }

    public static void ClearBuffer()
    {
        DrawOperationBuffer.Clear();
    }

    public static void Draw()
    {
        foreach (var operation in DrawOperationBuffer)
            operation();
    }
}