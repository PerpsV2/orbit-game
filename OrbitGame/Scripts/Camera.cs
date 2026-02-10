using System;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public enum CameraMode
{
    None,
    Tracking,
    TrackingFixed,
    Surface
}

public class Camera
{
    public CameraMode Mode { get; set; }
    
    private SD_Vector2 _localPosition;
    private SD_Vector2 LocalPosition
    {
        get => _localPosition;
        set
        {
            _localPosition = value;
            UpdateViewMatrix();
        }
    }
    
    private double _localAngle;
    private double LocalAngle
    {
        get => Utils.UnsignedMod(_localAngle, Math.Tau);
        set
        {
            _localAngle = value;
            UpdateViewMatrix();
        }
    }

    private KinematicObject? _tracking;
    private KinematicObject? _surface;

    private ScientificDecimal _width;
    public ScientificDecimal Width
    {
        get => _width;
        private set
        {
            _width = value;
            UpdateViewMatrix(); 
        }
    }

    private ScientificDecimal _height;

    public ScientificDecimal Height
    {
        get => _height;
        private set
        {
            _height = value;
            UpdateViewMatrix();
        }
    }
    
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    
    public ScientificDecimal Left => GetAbsolutePosition().X - Width * 0.5f;
    public ScientificDecimal Top => GetAbsolutePosition().Y + Height * 0.5f;
    public ScientificDecimal Right => GetAbsolutePosition().X + Width * 0.5f;
    public ScientificDecimal Bottom => GetAbsolutePosition().Y - Height * 0.5f;

    public SD_Vector2 ForwardVector => SD_Vector2.FromPolar(GetAbsolutePosition().GetPrincipalAngle());

    public Matrix3X3 ViewMatrix;
    
    public Camera(SD_Vector2 position, double angle, ScientificDecimal width, ScientificDecimal height,
        int screenWidth, int screenHeight)
    {
        _localPosition = position;
        _width = width;
        _height = height;
        _localAngle = angle;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        UpdateViewMatrix();
    }

    public Camera(SD_Vector2 position, double angle, ScientificDecimal width, ScientificDecimal height) :
        this(position, angle, width, height, Options.ScreenSize.width, Options.ScreenSize.height) { }
    
    public Camera(SD_Vector2 position, ScientificDecimal width, ScientificDecimal height) :
        this(position, 0, width, height) { }
    
    public SD_Vector2 GetAbsolutePosition()
    {
        switch (Mode)
        {
            case CameraMode.None:
            case CameraMode.Tracking:
            case CameraMode.TrackingFixed:
            case CameraMode.Surface: return LocalPosition + (_tracking?.Position ?? SD_Vector2.Zero);
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public double GetAbsoluteAngle()
    {
        switch (Mode)
        {
            case CameraMode.None:
            case CameraMode.Tracking: return LocalAngle;
            case CameraMode.TrackingFixed: return LocalAngle - (_tracking?.Angle ?? 0);
            case CameraMode.Surface: return 
                LocalAngle - (GetAbsolutePosition() - (_surface?.Position ?? SD_Vector2.Zero)).GetPrincipalAngle();
            default: throw new ArgumentOutOfRangeException();
        }
    }

    public void MoveParallel(ScientificDecimal distance)
    {
        switch (Mode)
        {
            case CameraMode.None: LocalPosition += new SD_Vector2(0, distance); break;
            case CameraMode.Tracking: LocalPosition += new SD_Vector2(0, distance); break;
            case CameraMode.TrackingFixed: LocalPosition += new SD_Vector2(0, distance); break;
            case CameraMode.Surface: LocalPosition += SD_Vector2.FromPolar(
                    (GetAbsolutePosition() - (_surface?.Position ?? SD_Vector2.Zero)).GetPrincipalAngle(), distance);
                break;
        }
    }

    public void MovePerpendicular(ScientificDecimal distance)
    {
        switch (Mode)
        {
            case CameraMode.None: LocalPosition += new SD_Vector2(distance, 0); break;
            case CameraMode.Tracking: LocalPosition += new SD_Vector2(distance, 0); break;
            case CameraMode.TrackingFixed: LocalPosition += new SD_Vector2(distance, 0); break;
            case CameraMode.Surface: break;
        }
    }

    public void RotateBy(double angle)
    {
        LocalAngle += angle;
    }

    public void SetTracking(KinematicObject? tracking, bool preserveAbsolutePosition = true)
    {
        if (preserveAbsolutePosition)
            LocalPosition += (tracking?.Position ?? SD_Vector2.Zero) - (_tracking?.Position ?? SD_Vector2.Zero);
        _tracking = tracking;
    }

    public void SetSurface(KinematicObject? surface)
    {
        _surface = surface;
    }

    public void FocusTracking()
    {
        LocalPosition = SD_Vector2.Zero;
    }
    
    public void ScaleZoom(ScientificDecimal scale)
    {
        Width *= scale;
        Height *= scale;
    }
    
    private void UpdateViewMatrix()
    {
        ViewMatrix = Matrix3X3.Scale(_screenWidth / Width, _screenHeight / Height) *
                     Matrix3X3.Translation(Width / 2, Height / 2) *
                     Matrix3X3.Rotation(-GetAbsoluteAngle()) *
                     Matrix3X3.Scale(1, -1);
    }

    public void TryUpdateViewMatrix()
    {
        switch (Mode)
        {
            case CameraMode.None: return;
            case CameraMode.Tracking: return;
            case CameraMode.TrackingFixed: UpdateViewMatrix(); break;
            case CameraMode.Surface: UpdateViewMatrix(); break;
        }
    }
    
    public SD_Vector2 SD_ConvertToScreenCoordinates(SD_Vector2 point)
        => ViewMatrix * (point - GetAbsolutePosition());
    
    public Vector2 ConvertToScreenCoordinates(SD_Vector2 point)
    {
        SD_Vector2 transformedPoint = SD_ConvertToScreenCoordinates(point);
        return new((float)transformedPoint.X, (float)transformedPoint.Y);
    }

    public ScientificDecimal SD_ConvertToScreenDistance(ScientificDecimal distance, bool xAxis = true)
    {
        if (xAxis) return distance / (Right - Left) * _screenWidth;
        return distance / (Bottom - Top) * _screenHeight;
    }
    
    public float ConvertToScreenDistance(ScientificDecimal distance, bool xAxis = true)
    {
        if (xAxis) return (float)SD_ConvertToScreenDistance(distance);
        return (float)SD_ConvertToScreenDistance(distance, false);
    }
}