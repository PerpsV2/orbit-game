using Silk.NET.Input;

namespace OrbitGame;

public static class Options
{
    public static readonly (int width, int height) ScreenSize = (1600, 1200);
    public static readonly ScientificDecimal DefaultZoomScale = new(7);

    public static readonly NumericalIntegrator IntegratorMethod = NumericalIntegrator.ExplicitEuler;

    public static readonly double CamMoveSpeed = 1;
    public static readonly double CamRotateSpeed = 1;
    public static readonly double CamZoomSpeed = 0.05;
    
    public static readonly Key MoveUpKey = Key.Up;
    public static readonly Key MoveDownKey = Key.Down;
    public static readonly Key MoveLeftKey = Key.Left;
    public static readonly Key MoveRightKey = Key.Right;
    
    public static readonly Key ZoomOutKey = Key.Q;
    public static readonly Key ZoomInKey = Key.E;
    
    public static readonly Key RotateLeftKey = Key.A;
    public static readonly Key RotateRightKey = Key.D;
    
    public static readonly Key TimeWarpUpKey = Key.Equal;
    public static readonly Key TimeWarpDownKey = Key.Minus;

    public static readonly Key TrackNextBodyKey = Key.M;
    public static readonly Key TrackPrevBodyKey = Key.N;
    public static readonly Key FocusKey = Key.F;

    public const bool DisplayFPS = true;
    public const int ScientificPrintPrecision = 5;

    // Maximum scale of planet radius with respect to the camera's height before the surface of the planet is drawn as a line rather than a curve
    public const float SurfaceApproximationRadiusZoomFraction = 1000;
}