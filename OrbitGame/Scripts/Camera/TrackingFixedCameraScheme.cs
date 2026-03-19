using System;

namespace OrbitGame;

/// <summary>
/// Camera movement scheme where:
/// - The rotation of the camera is by default the rotation of a tracking object.
/// - The position of the camera has its origin at a tracking object.
/// - Focus moves the camera to a tracking object.
/// </summary>
public class TrackingFixedCameraScheme : ICameraMovementScheme
{
    private DVector2<SDecimal> _localPosition;
    private double _localAngle;
    private readonly KinematicObject? _tracking;

    public TrackingFixedCameraScheme(SpatialInfo cameraSpatialInfo, KinematicObject? tracking)
    {
        _tracking = tracking;
        _localPosition = cameraSpatialInfo.Position - (_tracking?.Position ?? DVector2<SDecimal>.Zero);
        _localAngle = cameraSpatialInfo.Angle - (_tracking?.Angle ?? 0);
    }

    public void Focus()
    {
        _localPosition = DVector2<SDecimal>.Zero;
        _localAngle = 0;
    }

    public void MovePerpendicular(SDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += DVector2<SDecimal>.FromPolar(-spatialInfo.Angle, distance);
    }

    public void MoveParallel(SDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += DVector2<SDecimal>.FromPolar(-spatialInfo.Angle + Math.PI / 2, distance);
    }

    public void RotateBy(double angle, ref SpatialInfo spatialInfo)
    {
        _localAngle += angle;
    }

    public void Update(ref SpatialInfo spatialInfo)
    {
        spatialInfo.Position = (_tracking?.Position ?? DVector2<SDecimal>.Zero) + _localPosition;
        spatialInfo.Angle = -(_tracking?.Angle ?? 0) - _localAngle;
    }
}