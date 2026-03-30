using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

/// <summary>
/// Represents a in-game object camera which provides conversions from world-space to screen-space.
/// </summary>
public class Camera : KinematicObject
{
    /// <summary>
    /// Defines how the camera should move.
    /// </summary>
    public ICameraMovementScheme MovementScheme { get; set; }
    
    // screen dimensions
    private readonly int _screenWidth;
    private readonly int _screenHeight;
    
    // camera dimensions
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
    
    /// <summary>
    /// Squared radius of the circumscribed circle around camera view box.
    /// </summary>
    public SDecimal MaximumRadiusSquared { get; private set; }
    public SDecimal MaximumRadius => SDecimal.Sqrt(MaximumRadiusSquared);
    
    /// <summary>
    /// World position of the top left point of the camera view box.
    /// </summary>
    public Vec2<SDecimal> TopLeft 
        => Position + Vec2<SDecimal>.RotatePoint(new(-Width * 0.5, Height * 0.5), Angle);
    
    /// <summary>
    /// World position of the top right point of the camera view box.
    /// </summary>
    public Vec2<SDecimal> TopRight
        => Position + Vec2<SDecimal>.RotatePoint(new(Width * 0.5, Height * 0.5), Angle);
    
    /// <summary>
    /// World position of the bottom left point of the camera view box.
    /// </summary>
    public Vec2<SDecimal> BottomLeft 
        => Position + Vec2<SDecimal>.RotatePoint(new(-Width * 0.5, -Height * 0.5), Angle);
    
    /// <summary>
    /// World position of the bottom right point of the camera view box.
    /// </summary>
    public Vec2<SDecimal> BottomRight 
        => Position + Vec2<SDecimal>.RotatePoint(new(Width * 0.5, -Height * 0.5), Angle);

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

    public Vec2<SDecimal> SD_ConvertToWorldCoordinates(Vec2<SDecimal> point)
        => _inverseViewMatrix * point + Position;

    public Vec2<SDecimal> ConvertToWorldCoordinates(Vector2 point)
        => SD_ConvertToWorldCoordinates(new(point.X, point.Y));
    
    public Vec2<SDecimal> SD_ConvertToScreenCoordinates(Vec2<SDecimal> point)
        => _viewMatrix * (point - Position);
    
    public Vector2 ConvertToScreenCoordinates(Vec2<SDecimal> point)
    {
        Vec2<SDecimal> transformedPoint = SD_ConvertToScreenCoordinates(point);
        return new((float)transformedPoint.X, (float)transformedPoint.Y);
    }

    /// <summary>
    /// Convert from a distance in world space into a distance in screen space.
    /// </summary>
    /// <param name="distance">World space distance to be converted.</param>
    /// <param name="xAxis">
    /// Axis to use as a scale factor between world space and screen space.
    /// true - x-axis
    /// false - y-axis
    /// </param>
    /// <returns>Distance in screen space as an SDecimal.</returns>
    public SDecimal SD_ConvertToScreenDistance(SDecimal distance, bool xAxis = true)
    {
        if (xAxis) return distance / Width * _screenWidth;
        return distance / Height * _screenHeight;
    }
    
    /// <summary>
    /// Convert from a distance in world space into a distance in screen space.
    /// </summary>
    /// <param name="distance">World space distance to be converted.</param>
    /// <param name="xAxis">
    /// Axis to use as a scale factor between world space and screen space.
    /// true - x-axis
    /// false - y-axis
    /// </param>
    /// <returns>Distance in screen space as a float.</returns>
    public float ConvertToScreenDistance(SDecimal distance, bool xAxis = true)
    {
        if (xAxis) return (float)SD_ConvertToScreenDistance(distance);
        return (float)SD_ConvertToScreenDistance(distance, false);
    }
}