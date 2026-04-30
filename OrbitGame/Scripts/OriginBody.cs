using System;
using System.Collections.Generic;

namespace OrbitGame;

public delegate void OriginBodyEventHandler(object? sender, OriginBodyEventArgs e);

public class OriginBodyEventArgs(Vec2Double positionOffset) : EventArgs
{
    public Vec2Double PositionOffset = positionOffset;
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
        OnResetOrigin?.Invoke(null, new OriginBodyEventArgs(-Body?.Position ?? Vec2Double.Zero));
    }
}