using System;
using System.Xml;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OrbitGame;

public static class Options
{
    // initial states
    public static readonly (int width, int height) ScreenSize = (800, 600);
    public static readonly qQEngine.SDecimal DefaultZoomScale = new(0);
    public static readonly qQEngine.SDecimal DefaultTimeStep = 1;
    
    // debug options
    public static readonly bool EnablePhysics = true;
    public static readonly bool EnableCollisions = true;
    public static readonly bool EnablePhysicsCollisionDebug = false;
    public const bool DisplayFPS = true;
    public const int ScientificPrintPrecision = 5;

    public static readonly NumericalIntegrator IntegratorMethod = NumericalIntegrator.VelocityVerlet;
    public static readonly double TimeWarpStepMultiplier = 10;

    public static readonly double CamMoveSpeed = 1;
    public static readonly double CamRotateSpeed = 1;
    public static readonly double CamZoomSpeed = 4;
    
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
    public static readonly Keys FastForwardKey = Keys.T;
    public static readonly Keys ChangeCameraSchemeKey = Keys.C;

    public static readonly Color BackgroundColour = Color.Black;

    public const double EaseFastForwardStartTime = 0.2;
    public const double EaseFastForwardEndTime = 0.2;
    public const double EaseTimeStepChangeTime = 0.5;

    public static readonly SDecimal EccentricAnomalyApproximationTolerance = new(-10);
    public const uint EccentricAnomalyApproximationMaxIterations = 5;

    public const int PatchedConicDetail = 150;
    public const uint IntegratorIterationAmount = 1;
    public const float MinimumShipCrashSpeed = 50f;
    /// <summary>
    /// Maximum time warp adjusted speed until an object is no longer resting
    /// </summary>
    public const float MaximumShipRestingSpeed = 0.001f;
    public const float MaximumShipRestingAngularSpeed = 0.001f;
    /// <summary>
    /// Number of points that should be drawn to represent a partial orbit.
    /// </summary>
    public const int OrbitResolutionNumPoints = 100;
    /// <summary>
    /// Number of points that should be drawn to represent a full elliptic orbit.
    /// </summary>
    public const int OrbitVertices = 400;
    /// <summary>
    /// If the angle range of the orbit within the camera's region is less than the zoom fraction multiplied by PI,
    /// approximate the section of orbit visible with points instead of drawing an ellipse for the whole orbit.
    /// </summary>
    public const float OrbitApproximationZoomFraction = 0.1f;
    /// <summary>
    /// Transparency of planet sphere of influence zones
    /// </summary>
    public const float SOIAlpha = 0.05f;
    /// <summary>
    /// Minimum scale of the planet radius with respect to the camera's height before the planet is drawn as a marker.
    /// </summary>
    public const float LocationApproximationRadiusZoomFraction = 0.005f;

    // ----- stuff that should probably not be touched -----
    public const double ScientificComparisonTolerance = 1e-30;
}