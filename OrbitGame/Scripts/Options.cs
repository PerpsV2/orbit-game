using Microsoft.Xna.Framework.Input;

namespace OrbitGame;

public static class Options
{
    public static readonly (int width, int height) ScreenSize = (800, 600);
    public static readonly ScientificDecimal DefaultZoomScale = new(1);
    public static readonly ScientificDecimal DefaultTimeStep = 1;
    public static readonly double TimeWarpStepMultiplier = 5;
    public static readonly bool EnablePhysics = true;
    
    public static readonly bool EnablePhysicsCollisionDebug = true;

    public static readonly NumericalIntegrator IntegratorMethod = NumericalIntegrator.RungeKutta4;

    public static readonly double CamMoveSpeed = 1;
    public static readonly double CamRotateSpeed = 1;
    public static readonly double CamZoomSpeed = 0.05;
    
    public static readonly Keys MoveUpKey = Keys.Up;
    public static readonly Keys MoveDownKey = Keys.Down;
    public static readonly Keys MoveLeftKey = Keys.Left;
    public static readonly Keys MoveRightKey = Keys.Right;
    
    public static readonly Keys ZoomOutKey = Keys.Q;
    public static readonly Keys ZoomInKey = Keys.E;
    
    public static readonly Keys RotateLeftKey = Keys.A;
    public static readonly Keys RotateRightKey = Keys.D;
    
    public static readonly Keys TimeWarpUpKey = Keys.OemPlus;
    public static readonly Keys TimeWarpDownKey = Keys.OemMinus;

    public static readonly Keys TrackNextBodyKey = Keys.M;
    public static readonly Keys TrackPrevBodyKey = Keys.N;
    public static readonly Keys FocusKey = Keys.F;

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
    public const float OrbitApproximationZoomFraction = 0.1f;
    /// <summary>
    /// Transparency of planet sphere of influence zones
    /// </summary>
    public const int SOIAlpha = 8;
    /// <summary>
    /// Strength of bias for orbit points to be near the apoapsis of an elliptic orbit.
    /// Increases resolution of areas on the orbit sensitive to large angle change (apoapsis).
    /// </summary>
    public const float EllipsePointDistributionBiasStrength = 2;
    /// <summary>
    /// Maximum scale of planet radius with respect to the camera's height before the surface of the planet is
    /// drawn as a line rather than a curve.
    /// </summary>
    public const float SurfaceApproximationRadiusZoomFraction = 100;
    /// <summary>
    /// Minimum scale of the planet radius with respect to the camera's height before the planet is drawn as a marker.
    /// </summary>
    public const float LocationApproximationRadiusZoomFraction = 0.005f;
}