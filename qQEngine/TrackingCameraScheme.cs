using System;

namespace qQEngine;

/// <summary>
/// Camera movement scheme where:
/// <para> - The rotation of the camera is independent to any game objects.</para>
/// <para> - The position of the camera has its origin at a tracking object.</para>
/// <para> - Focus moves the camera to a tracking object.</para>
/// If the tracking object is null, the camera's position is relative to the world origin.
/// </summary>
public class TrackingCameraScheme : ICameraMovementScheme
{
    /// <summary>
    /// Position of the camera relative to a tracking object.
    /// </summary>
    private Vec2Double _localPosition;

    private double _localAngle;
    private readonly KinematicObject? _tracking;

    /// <summary>
    /// Create a TrackingCameraScheme from an existing camera SpatialInfo.
    /// </summary>
    /// <param name="cameraSpatialInfo">Existing camera SpatialInfo.</param>
    /// <param name="tracking">Tracking object.</param>
    public TrackingCameraScheme(SpatialInfo cameraSpatialInfo, KinematicObject? tracking)
    {
        _tracking = tracking;
        _localPosition = cameraSpatialInfo.Position - (_tracking?.Position ?? Vec2Double.Zero);
        _localAngle = cameraSpatialInfo.Angle - (_tracking?.Angle ?? 0);
    }

    public void Focus()
    {
        _localPosition = Vec2Double.Zero;
        _localAngle = 0;
    }

    public void MovePerpendicular(SDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += Vec2Double.FromPolar(-spatialInfo.Angle, (double)distance);
    }

    public void MoveParallel(SDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += Vec2Double.FromPolar(-spatialInfo.Angle + Math.PI / 2, (double)distance);
    }

    public void RotateBy(double angle, ref SpatialInfo spatialInfo)
    {
        _localAngle += angle;
    }

    public void Update(ref SpatialInfo spatialInfo)
    {
        spatialInfo.Position = (_tracking?.Position ?? Vec2Double.Zero) + _localPosition;
        spatialInfo.Angle = _localAngle;
    }
}