using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OrbitGame;

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

    public Matrix3X3 ViewMatrix;
    
    public Camera(string identifier, SpatialInfo spatialInfo, ScientificDecimal width, ScientificDecimal height,
        int screenWidth, int screenHeight, ICameraMovementScheme movementScheme) 
        : base(identifier, spatialInfo)
    {
        _width = width;
        _height = height;
        _screenWidth = screenWidth;
        _screenHeight = screenHeight;
        MovementScheme = movementScheme;
        UpdateViewMatrix();
    }

    public Camera(string identifier, SpatialInfo spatialInfo, ScientificDecimal width, ScientificDecimal height)
        : this(identifier, spatialInfo, width, height, Options.ScreenSize.width, Options.ScreenSize.height, 
            new TrackingCameraScheme(spatialInfo, null))
    { }

    public void Focus() 
        => MovementScheme.Focus(ref SpatialInfo);
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
        ViewMatrix = Matrix3X3.Scale(_screenWidth / Width, _screenHeight / Height) *
                     Matrix3X3.Translation(Width / 2, Height / 2) *
                     Matrix3X3.Rotation(-Angle) *
                     Matrix3X3.Scale(1, -1);
    }
    
    public SD_Vector2 SD_ConvertToScreenCoordinates(SD_Vector2 point)
        => ViewMatrix * (point - Position);
    
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