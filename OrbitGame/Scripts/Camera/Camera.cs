using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// In-game camera class containing transformations between world and screen space.
/// </summary>
public class Camera : KinematicObject
{
    public ICameraMovementScheme MovementScheme { get; set; }
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    
    private ScientificDecimal _width;
    public ScientificDecimal Width
    {
        get => _width;
        private set
        {
            _width = value;
            MaximumRadiusSquared = _height * _height + _width * _width;
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
            MaximumRadiusSquared = _height * _height + _width * _width;
            UpdateViewMatrix();
        }
    }

    public ScientificDecimal MaximumRadiusSquared;
    public ScientificDecimal MaximumRadius => MaximumRadiusSquared.Sqrt();
    public SD_Vector2 TopLeft => Position + SD_Vector2.RotatePoint(new(-Width * 0.5, Height * 0.5), Angle);
    public SD_Vector2 TopRight => Position + SD_Vector2.RotatePoint(new(Width * 0.5, Height * 0.5), Angle);
    public SD_Vector2 BottomLeft => Position + SD_Vector2.RotatePoint(new(-Width * 0.5, -Height * 0.5), Angle);
    public SD_Vector2 BottomRight => Position + SD_Vector2.RotatePoint(new(Width * 0.5, -Height * 0.5), Angle);

    private Matrix3X3 _viewMatrix;
    private Matrix3X3 _inverseViewMatrix;
    
    public Camera(string identifier, SpatialInfo spatialInfo, ScientificDecimal width, ScientificDecimal height,
        int screenWidth, int screenHeight, ICameraMovementScheme movementScheme) 
        : base(identifier, spatialInfo)
    {
        _width = width;
        _height = height;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        MaximumRadiusSquared = _height * _height + _width * _width;
        MovementScheme = movementScheme;
        UpdateViewMatrix();
        OriginBody.OnResetOrigin += Camera_OnResetOrigin;
    }

    private void Camera_OnResetOrigin(object? sender, OriginBodyEventArgs e)
    {
        Position += e.PositionOffset;
    }

    public Camera(string identifier, SpatialInfo spatialInfo, ScientificDecimal width, ScientificDecimal height)
        : this(identifier, spatialInfo, width, height, Options.ScreenSize.width, Options.ScreenSize.height, 
            new TrackingCameraScheme(spatialInfo, null))
    { }

    public void Focus() 
        => MovementScheme.Focus();
    public void MovePerpendicular(ScientificDecimal distance) 
        => MovementScheme.MovePerpendicular(distance, ref SpatialInfo);
    public void MoveParallel(ScientificDecimal distance)
        => MovementScheme.MoveParallel(distance, ref SpatialInfo);
    public void RotateBy(double angle)
        => MovementScheme.RotateBy(angle, ref SpatialInfo);

    public void Update()
    {
        SpatialInfo prevSpatialInfo = SpatialInfo;
        MovementScheme.Update(ref SpatialInfo);
        if (prevSpatialInfo != SpatialInfo) UpdateViewMatrix();
    }

    public void ScaleZoom(ScientificDecimal scale)
    {
        Width *= scale;
        Height *= scale;
    }
    
    private void UpdateViewMatrix()
    {
        _viewMatrix = Matrix3X3.Scale(_screenWidth / Width, _screenHeight / Height) *
                     Matrix3X3.Translation(Width / 2, Height / 2) *
                     Matrix3X3.Rotation(-Angle) *
                     Matrix3X3.Scale(1, -1);
        _inverseViewMatrix = Matrix3X3.Scale(1, -1) * 
                            Matrix3X3.Rotation(Angle) *
                            Matrix3X3.Translation(-Width / 2, -Height / 2) *
                            Matrix3X3.Scale(Width / _screenWidth, Height / _screenHeight);
    }

    public SD_Vector2 SD_ConvertToWorldCoordinates(SD_Vector2 point)
        => _inverseViewMatrix * point + Position;

    public SD_Vector2 ConvertToWorldCoordinates(Vector2 point)
        => SD_ConvertToWorldCoordinates(new(point.X, point.Y));
    
    public SD_Vector2 SD_ConvertToScreenCoordinates(SD_Vector2 point)
        => _viewMatrix * (point - Position);
    
    public Vector2 ConvertToScreenCoordinates(SD_Vector2 point)
    {
        SD_Vector2 transformedPoint = SD_ConvertToScreenCoordinates(point);
        return new((float)transformedPoint.X, (float)transformedPoint.Y);
    }

    public ScientificDecimal SD_ConvertToScreenDistance(ScientificDecimal distance, bool xAxis = true)
    {
        if (xAxis) return distance / Width * _screenWidth;
        return distance / Height * _screenHeight;
    }
    
    public float ConvertToScreenDistance(ScientificDecimal distance, bool xAxis = true)
    {
        if (xAxis) return (float)SD_ConvertToScreenDistance(distance);
        return (float)SD_ConvertToScreenDistance(distance, false);
    }
}