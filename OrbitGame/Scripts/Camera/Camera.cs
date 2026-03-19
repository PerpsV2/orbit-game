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
    
    private SDecimal _width;
    public SDecimal Width
    {
        get => _width;
        private set
        {
            _width = value;
            MaximumRadiusSquared = _height * _height + _width * _width;
            UpdateViewMatrix(); 
        }
    }

    private SDecimal _height;

    public SDecimal Height
    {
        get => _height;
        private set
        {
            _height = value;
            MaximumRadiusSquared = _height * _height + _width * _width;
            UpdateViewMatrix();
        }
    }

    public SDecimal MaximumRadiusSquared;
    public SDecimal MaximumRadius => SDecimal.Sqrt(MaximumRadiusSquared);
    public DVector2<SDecimal> TopLeft 
        => Position + DVector2<SDecimal>.RotatePoint(new(-Width * 0.5, Height * 0.5), Angle);
    public DVector2<SDecimal> TopRight
        => Position + DVector2<SDecimal>.RotatePoint(new(Width * 0.5, Height * 0.5), Angle);
    public DVector2<SDecimal> BottomLeft 
        => Position + DVector2<SDecimal>.RotatePoint(new(-Width * 0.5, -Height * 0.5), Angle);
    public DVector2<SDecimal> BottomRight 
        => Position + DVector2<SDecimal>.RotatePoint(new(Width * 0.5, -Height * 0.5), Angle);

    private Matrix3X3<SDecimal> _viewMatrix;
    private Matrix3X3<SDecimal> _inverseViewMatrix;
    
    public Camera(string identifier, SpatialInfo spatialInfo, SDecimal width, SDecimal height,
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

    public Camera(string identifier, SpatialInfo spatialInfo, SDecimal width, SDecimal height)
        : this(identifier, spatialInfo, width, height, Options.ScreenSize.width, Options.ScreenSize.height, 
            new TrackingCameraScheme(spatialInfo, null))
    { }

    public void Focus() 
        => MovementScheme.Focus();
    public void MovePerpendicular(SDecimal distance) 
        => MovementScheme.MovePerpendicular(distance, ref SpatialInfo);
    public void MoveParallel(SDecimal distance)
        => MovementScheme.MoveParallel(distance, ref SpatialInfo);
    public void RotateBy(double angle)
        => MovementScheme.RotateBy(angle, ref SpatialInfo);

    public void Update()
    {
        SpatialInfo prevSpatialInfo = SpatialInfo;
        MovementScheme.Update(ref SpatialInfo);
        if (prevSpatialInfo != SpatialInfo) UpdateViewMatrix();
    }

    public void ScaleZoom(SDecimal scale)
    {
        Width *= scale;
        Height *= scale;
    }
    
    private void UpdateViewMatrix()
    {
        _viewMatrix = Matrix3X3<SDecimal>.Scale(_screenWidth / Width, _screenHeight / Height) *
                     Matrix3X3<SDecimal>.Translation(Width / 2, Height / 2) *
                     Matrix3X3<SDecimal>.Rotation(-Angle) *
                     Matrix3X3<SDecimal>.Scale(1, -1);
        _inverseViewMatrix = Matrix3X3<SDecimal>.Scale(1, -1) * 
                            Matrix3X3<SDecimal>.Rotation(Angle) *
                            Matrix3X3<SDecimal>.Translation(-Width / 2, -Height / 2) *
                            Matrix3X3<SDecimal>.Scale(Width / _screenWidth, Height / _screenHeight);
    }

    public DVector2<SDecimal> SD_ConvertToWorldCoordinates(DVector2<SDecimal> point)
        => _inverseViewMatrix * point + Position;

    public DVector2<SDecimal> ConvertToWorldCoordinates(Vector2 point)
        => SD_ConvertToWorldCoordinates(new(point.X, point.Y));
    
    public DVector2<SDecimal> SD_ConvertToScreenCoordinates(DVector2<SDecimal> point)
        => _viewMatrix * (point - Position);
    
    public Vector2 ConvertToScreenCoordinates(DVector2<SDecimal> point)
    {
        DVector2<SDecimal> transformedPoint = SD_ConvertToScreenCoordinates(point);
        return new((float)transformedPoint.X, (float)transformedPoint.Y);
    }

    public SDecimal SD_ConvertToScreenDistance(SDecimal distance, bool xAxis = true)
    {
        if (xAxis) return distance / Width * _screenWidth;
        return distance / Height * _screenHeight;
    }
    
    public float ConvertToScreenDistance(SDecimal distance, bool xAxis = true)
    {
        if (xAxis) return (float)SD_ConvertToScreenDistance(distance);
        return (float)SD_ConvertToScreenDistance(distance, false);
    }
}