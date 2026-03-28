using System;

namespace OrbitGame;

/// <summary>
/// Camera movement scheme where:
/// - The rotation of the camera is independent to any game objects.
/// - The position of the camera has its origin at a tracking object.
/// - Focus moves the camera to a tracking object.
/// </summary>
public class TrackingCameraScheme : ICameraMovementScheme
{
    private Vec2<SDecimal> _localPosition;
    private readonly KinematicObject? _tracking;

    public TrackingCameraScheme(SpatialInfo cameraSpatialInfo, KinematicObject? tracking)
    {
        _tracking = tracking;
        _localPosition = cameraSpatialInfo.Position - (_tracking?.Position ?? Vec2<SDecimal>.Zero);
    }

    public void Focus()
    {
        _localPosition = Vec2<SDecimal>.Zero;
    }

    public void MovePerpendicular(SDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += Vec2<SDecimal>.FromPolar(-spatialInfo.Angle, distance);
    }

    public void MoveParallel(SDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += Vec2<SDecimal>.FromPolar(-spatialInfo.Angle + Math.PI / 2, distance);
    }

    public void RotateBy(double angle, ref SpatialInfo spatialInfo)
    {
        spatialInfo.Angle += angle;
    }

    public void Update(ref SpatialInfo spatialInfo)
    {
        spatialInfo.Position = (_tracking?.Position ?? Vec2<SDecimal>.Zero) + _localPosition;
    }
}