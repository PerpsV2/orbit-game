namespace OrbitGame;

public class SurfaceCameraScheme : ICameraMovementScheme
{
    private SD_Vector2 _localPosition;
    private double _localAngle;
    private readonly KinematicObject _surface;
    private readonly KinematicObject _tracking;
    
    public SurfaceCameraScheme(SpatialInfo spatialInfo, KinematicObject surface, KinematicObject tracking)
    {
        _surface = surface;
        _tracking = tracking;
        _localPosition = new SD_Vector2();
        _localAngle = 0;
    }

    public void Focus(ref SpatialInfo spatialInfo)
    {
        _localAngle = 0;
        _localPosition = SD_Vector2.Zero;
    }

    public void MovePerpendicular(ScientificDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition = SD_Vector2.FromPolar()
    }

    public void MoveParallel(ScientificDecimal distance, ref SpatialInfo spatialInfo)
    {
        _localPosition += SD_Vector2.FromPolar((_tracking.Position - _surface.Position).GetPrincipalAngle(), distance);
    }

    public void RotateBy(double angle, ref SpatialInfo spatialInfo)
    {
        throw new System.NotImplementedException();
    }

    public void Update(ref SpatialInfo spatialInfo)
    {
        spatialInfo.Position = _tracking.Position + _localPosition;
        spatialInfo.Angle = -_tracking.Angle- _localAngle;
    }
}