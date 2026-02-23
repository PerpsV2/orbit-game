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
    private SD_Vector2 _localPosition;
    private readonly KinematicObject? _tracking;

    public TrackingCameraScheme(SpatialInfo spatialInfo, KinematicObject? tracking)
    {
        _tracking = tracking;
        _localPosition = spatialInfo.Position - (_tracking?.Position ?? SD_Vector2.Zero);
    }

    public void Focus()
    {
        _localPosition = SD_Vector2.Zero;
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
        spatialInfo.Angle += angle;
    }

    public void Update(ref SpatialInfo spatialInfo)
    {
        spatialInfo.Position = (_tracking?.Position ?? SD_Vector2.Zero) + _localPosition;
    }
}