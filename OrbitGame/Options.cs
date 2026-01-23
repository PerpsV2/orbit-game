using Silk.NET.Input;
using SkiaSharp;

namespace OrbitGame;

public static class Options
{
    public static readonly (int width, int height) ScreenSize = (1600, 1200);
    public static readonly ScientificDecimal DefaultZoomScale = new(8);
    public static readonly ScientificDecimal DefaultTimeStep = 1;
    public static readonly bool EnablePhysics = true;
    public static readonly bool DrawColliders = false;
    
    public static readonly bool EnableCollisionDebug = false;

    public static readonly NumericalIntegrator IntegratorMethod = NumericalIntegrator.RungeKutta4;

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
    
    /// <summary>
    /// Number of points that should be drawn to represent an orbit.
    /// </summary>
    public const uint OrbitResolutionNumPoints = 100;
    /// <summary>
    /// If the angle range of the orbit within the camera's region is less than the zoom fraction multiplied by PI,
    /// approximate the section of orbit visible with points instead of drawing an ellipse for the whole orbit.
    /// </summary>
    public const float OrbitApproximationZoomFraction = 0.01f;
    /// <summary>
    /// Strength of bias for orbit points to be near the apoapsis of an elliptic orbit.
    /// Increases resolution of areas on the orbit sensitive to large angle change (apoapsis).
    /// </summary>
    public const float EllipsePointDistributionBiasStrength = 2;
    /// <summary>
    /// Maximum scale of planet radius with respect to the camera's height before the surface of the planet is
    /// drawn as a line rather than a curve.
    /// </summary>
    public const float SurfaceApproximationRadiusZoomFraction = 1000;
    /// <summary>
    /// Minimum scale of the planet radius with respect to the camera's height before the planet is drawn as a marker.
    /// </summary>
    public const float LocationApproximationRadiusZoomFraction = 0.005f;
}