namespace OrbitGame;

/// <summary>
/// Interface for camera movement schemes.
/// </summary>
public interface ICameraMovementScheme
{
    /// <summary>
    /// Method for when the camera's focus control is triggered.
    /// </summary>
    public void Focus();
    
    /// <summary>
    /// Method for when the camera moves perpendicular to its facing (left and right controls).
    /// </summary>
    /// <param name="distance">Distance to move</param>
    /// <param name="spatialInfo">SpatialInfo of the camera</param>
    public void MovePerpendicular(ScientificDecimal distance, ref SpatialInfo spatialInfo);
    
    /// <summary>
    /// Method for when the camera moves parallel to its facing (forwards and backwards controls).
    /// </summary>
    /// <param name="distance">Distance to move</param>
    /// <param name="spatialInfo">SpatialInfo of the camera</param>
    public void MoveParallel(ScientificDecimal distance, ref SpatialInfo spatialInfo);
    
    /// <summary>
    /// Method for when the camera changes its object rotation (clockwise and counterclockwise controls).
    /// </summary>
    /// <param name="angle">Angle to move</param>
    /// <param name="spatialInfo">SpatialInfo of the camera</param>
    public void RotateBy(double angle, ref SpatialInfo spatialInfo);
    
    /// <summary>
    /// Method to update the SpatialInfo of a stationary camera for every frame.
    /// </summary>
    /// <param name="spatialInfo"></param>
    public void Update(ref SpatialInfo spatialInfo);
}