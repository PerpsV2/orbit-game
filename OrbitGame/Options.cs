using Silk.NET.Input;

namespace OrbitGame;

public static class Options
{
    public static readonly (int width, int height) ScreenSize = (1600, 1200);

    public static readonly double CamMoveSpeed = 0.03;
    public static readonly double CamZoomSpeed = 0.05;
    
    public static readonly Key MoveUpKey = Key.Up;
    public static readonly Key MoveDownKey = Key.Down;
    public static readonly Key MoveLeftKey = Key.Left;
    public static readonly Key MoveRightKey = Key.Right;
    public static readonly Key ZoomOutKey = Key.Q;
    public static readonly Key ZoomInKey = Key.E;
    public static readonly Key TimeWarpUpKey = Key.Equal;
    public static readonly Key TimeWarpDownKey = Key.Minus;

    public const bool DisplayFPS = true;
    public const int ScientificPrintPrecision = 5;
}