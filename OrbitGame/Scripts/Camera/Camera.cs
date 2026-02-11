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
    
    public SpatialInfo SpatialInfo;

    public ICameraMovementScheme MovementScheme;

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

    public SD_Vector2 AbsolutePosition => MovementScheme.GetAbsolutePosition();
    public double AbsoluteAngle => MovementScheme.GetAbsoluteAngle();
    
    public ScientificDecimal Left => AbsolutePosition.X - Width * 0.5f;
    public ScientificDecimal Top => AbsolutePosition.Y + Height * 0.5f;
    public ScientificDecimal Right => AbsolutePosition.X + Width * 0.5f;
    public ScientificDecimal Bottom => AbsolutePosition.Y - Height * 0.5f;

    private SD_Vector2 ForwardVector => SD_Vector2.FromPolar(-AbsoluteAngle + Math.PI / 2);
    private SD_Vector2 RightVector => SD_Vector2.FromPolar(-AbsoluteAngle);

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
                     Matrix3X3.Rotation(-AbsoluteAngle) *
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
        => ViewMatrix * (point - AbsolutePosition);
    
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