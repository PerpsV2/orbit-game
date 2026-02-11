using System;

namespace OrbitGame;

public class TrackingFixedCameraScheme : ICameraMovementScheme
{
    private SD_Vector2 _localPosition;
    private double _localAngle;
    private readonly KinematicObject? _tracking;

    public TrackingFixedCameraScheme(SpatialInfo spatialInfo, KinematicObject? tracking)
    {
        _tracking = tracking;
        _localPosition = spatialInfo.Position - (_tracking?.Position ?? SD_Vector2.Zero);
        _localAngle = spatialInfo.Angle - (_tracking?.Angle ?? 0);
    }

    public void Focus(ref SpatialInfo spatialInfo)
    {
        _localPosition = SD_Vector2.Zero;
        _localAngle = 0;
    }

    public void MovePerpendicular(ScientificDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += SD_Vector2.FromPolar(-spatialInfo.Angle, distance);
    }

    public void MoveParallel(ScientificDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += SD_Vector2.FromPolar(-spatialInfo.Angle + Math.PI / 2, distance);
    }

    public void RotateBy(double angle, ref SpatialInfo spatialInfo)
    {
        _localAngle += angle;
    }

    public void Update(ref SpatialInfo spatialInfo)
    {
        spatialInfo.Position = (_tracking?.Position ?? SD_Vector2.Zero) + _localPosition;
        spatialInfo.Angle = -(_tracking?.Angle ?? 0) - _localAngle;
    }
}