using System;
using Microsoft.Xna.Framework;

namespace OrbitGame;

/// <summary>
/// Camera movement scheme where:
/// <para> - The default rotation of the camera is always pointed away from a surface object.</para>
/// <para> - Perpendicular movement maintains the same altitude from a surface object.</para>
/// <para> - Parallel movement alters only the altitude from a surface object.</para>
/// <para> - Focus moves the camera to a tracking object.</para>
/// </summary>
public class SurfaceCameraScheme : ICameraMovementScheme
{
    /// <summary>
    /// Position of the camera relative to a tracking object.
    /// </summary>
    private Vec2<SDecimal> _localPosition;
    /// <summary>
    /// Angle of the camera relative to the normal vector of the surface object.
    /// </summary>
    private double _localAngle;
    private readonly KinematicObject _surface;
    private readonly KinematicObject _tracking;
    
    /// <summary>
    /// Create a SurfaceCameraScheme from an existing camera SpatialInfo.
    /// </summary>
    /// <param name="cameraSpatialInfo">Existing camera SpatialInfo.</param>
    /// <param name="surface">Surface object.</param>
    /// <param name="tracking">Tracking object.</param>
    public SurfaceCameraScheme(SpatialInfo cameraSpatialInfo, KinematicObject surface, KinematicObject tracking)
    {
        _surface = surface;
        _tracking = tracking;
        _localPosition = cameraSpatialInfo.Position - _tracking.Position;
        _localAngle = cameraSpatialInfo.Angle - (_surface.Position - cameraSpatialInfo.Position).Direction() - Math.PI / 2;
    }

    public void Focus()
    {
        _localAngle = 0;
        _localPosition = Vec2<SDecimal>.Zero;
    }

    public void MovePerpendicular(SDecimal distance, ref SpatialInfo spatialInfo)
    {
        Vec2<SDecimal> camSurfaceVector = _localPosition + _tracking.Position - _surface.Position;
        double deltaAngle = (double)(distance / camSurfaceVector.Magnitude());
        // handle case where the camera is at the same position as the surface object
        Vec2<SDecimal> newCamSurfacePosition;
        if (camSurfaceVector == Vec2<SDecimal>.Zero) newCamSurfacePosition = Vec2<SDecimal>.Zero;
        else newCamSurfacePosition = Vec2<SDecimal>.FromPolar(
            camSurfaceVector.Direction() - deltaAngle, camSurfaceVector.Magnitude()
        );
        _localPosition = newCamSurfacePosition + _surface.Position - _tracking.Position;
    }

    public void MoveParallel(SDecimal distance, ref SpatialInfo spatialInfo)
    {
        // handle case where the camera is at the same position as the surface object
        Vec2<SDecimal> camSurfaceVector = _surface.Position - _tracking.Position - _localPosition; 
        if (camSurfaceVector == Vec2<SDecimal>.Zero) return;
        _localPosition += Vec2<SDecimal>.FromPolar(camSurfaceVector.Direction(), -distance);
    }

    public void RotateBy(double angle, ref SpatialInfo spatialInfo)
    {
        _localAngle += angle;
    }

    public void Update(ref SpatialInfo spatialInfo)
    {
        spatialInfo.Position = _tracking.Position + _localPosition;
        // handle case where the camera is at the same position as the surface object
        Vec2<SDecimal> camSurfaceVector = _surface.Position - _tracking.Position - _localPosition;
        if (camSurfaceVector == Vec2<SDecimal>.Zero) spatialInfo.Angle = _localAngle;
        else spatialInfo.Angle = -camSurfaceVector.Direction() - Math.PI / 2 - _localAngle;
    }
}