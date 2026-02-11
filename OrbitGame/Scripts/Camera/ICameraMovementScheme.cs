namespace OrbitGame;

public interface ICameraMovementScheme
{
    public void Focus(ref SpatialInfo spatialInfo);
    public void MovePerpendicular(ScientificDecimal distance, ref SpatialInfo spatialInfo);
    public void MoveParallel(ScientificDecimal distance, ref SpatialInfo spatialInfo);
    public void RotateBy(double angle, ref SpatialInfo spatialInfo);
    public void Update(ref SpatialInfo spatialInfo);
}