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
    private SD_Vector2 _localPosition;
    private double _localAngle;
    private readonly KinematicObject _surface;
    private readonly KinematicObject _tracking;
    
    public SurfaceCameraScheme(KinematicObject surface, KinematicObject tracking)
    {
        _surface = surface;
        _tracking = tracking;
        _localPosition = new SD_Vector2();
        _localAngle = 0;
    }

    public void Focus()
    {
        _localAngle = 0;
        _localPosition = SD_Vector2.Zero;
    }

    public void MovePerpendicular(ScientificDecimal distance, ref SpatialInfo spatialInfo)
    {
        SD_Vector2 camSurfaceVector = _localPosition + _tracking.Position - _surface.Position;
        double deltaAngle = (double)(distance / camSurfaceVector.Magnitude());
        SD_Vector2 newCamSurfacePosition = SD_Vector2.FromPolar(
            camSurfaceVector.Direction() - deltaAngle, camSurfaceVector.Magnitude()
        );
        _localPosition = newCamSurfacePosition + _surface.Position - _tracking.Position;
    }

    public void MoveParallel(ScientificDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += SD_Vector2.FromPolar(
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
        spatialInfo.Angle = -(_surface.Position - _tracking.Position - _localPosition).Direction() 
                            - Math.PI / 2 - _localAngle;
    }
}