using System;
using System.Collections.Generic;

namespace OrbitGame;

public delegate void OriginBodyEventHandler(object? sender, OriginBodyEventArgs e);

public class OriginBodyEventArgs(SD_Vector2 positionOffset) : EventArgs
{
    public SD_Vector2 PositionOffset = positionOffset;
}

/// <summary>
/// Class which sets the origin body to where most physics calculations would take place to prevent floating point errors.
/// </summary>
public static class OriginBody
{
    public static Body? Body = null;
    
    public static event OriginBodyEventHandler? OnResetOrigin;

    public static void ResetOrigin()
    {
        OnResetOrigin?.Invoke(null, new OriginBodyEventArgs(-Body?.Position ?? SD_Vector2.Zero));
        /*if (Body == null) return;
        foreach (var obj in kinObjects)
        {
            if (obj == Body) continue;
            obj.ResetOrigin(Body.Position);
        }

        Body.Position = SD_Vector2.Zero;*/
    }
}