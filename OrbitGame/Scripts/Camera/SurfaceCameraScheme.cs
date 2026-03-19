using System;
using Microsoft.Xna.Framework;

namespace OrbitGame;

/// <summary>
/// Camera movement scheme where:
/// - The default rotation of the camera is always pointed away from a surface object.
/// - Perpendicular movement maintains the same altitude from a surface object.
/// - Parallel movement alters only the altitude from a surface object.
/// - Focus moves the camera to a tracking object.
/// </summary>
public class SurfaceCameraScheme : ICameraMovementScheme
{
    private DVector2<SDecimal> _localPosition;
    private double _localAngle;
    private readonly KinematicObject _surface;
    private readonly KinematicObject _tracking;
    
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
        _localPosition = DVector2<SDecimal>.Zero;
    }

    public void MovePerpendicular(SDecimal distance, ref SpatialInfo spatialInfo)
    {
        DVector2<SDecimal> camSurfaceVector = _localPosition + _tracking.Position - _surface.Position;
        double deltaAngle = (double)(distance / camSurfaceVector.Magnitude());
        DVector2<SDecimal> newCamSurfacePosition = DVector2<SDecimal>.FromPolar(
            camSurfaceVector.Direction() - deltaAngle, camSurfaceVector.Magnitude()
        );
        _localPosition = newCamSurfacePosition + _surface.Position - _tracking.Position;
    }

    public void MoveParallel(SDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += DVector2<SDecimal>.FromPolar(
            (_surface.Position - _tracking.Position - _localPosition).Direction(), -distance
        );
    }

    public void RotateBy(double angle, ref SpatialInfo spatialInfo)
    {
        _localAngle += angle;
    }

    public void Update(ref SpatialInfo spatialInfo)
    {
        spatialInfo.Position = _tracking.Position + _localPosition;
        if (_surface.Position - _tracking.Position - _localPosition == DVector2<SDecimal>.Zero) spatialInfo.Angle = _localAngle;
        else spatialInfo.Angle = -(_surface.Position - _tracking.Position - _localPosition).Direction() 
                                 - Math.PI / 2 - _localAngle;
    }
}