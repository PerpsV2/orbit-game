namespace qQEngine;

/// <summary>
/// Base interface for camera movement schemes which define the motion of the camera for various actions.
/// </summary>
public interface ICameraMovementScheme
{
    /// <summary>
    /// Action for when the camera focuses on a currently tracking object.
    /// </summary>
    public void Focus();
    
    /// <summary>
    /// Action for when the camera moves perpendicular to its facing (left and right movement).
    /// </summary>
    /// <param name="distance">Distance to move.</param>
    /// <param name="spatialInfo">Reference SpatialInfo of the camera.</param>
    public void MovePerpendicular(SDecimal distance, ref SpatialInfo spatialInfo);
    
    /// <summary>
    /// Action for when the camera moves parallel to its facing (forwards and backwards movement).
    /// </summary>
    /// <param name="distance">Distance to move.</param>
    /// <param name="spatialInfo">Reference SpatialInfo of the camera.</param>
    public void MoveParallel(SDecimal distance, ref SpatialInfo spatialInfo);
    
    /// <summary>
    /// Action for when the camera changes its local rotation (clockwise and counterclockwise movement).
    /// </summary>
    /// <param name="angle">Angle to rotate (radians).</param>
    /// <param name="spatialInfo">Reference SpatialInfo of the camera.</param>
    public void RotateBy(double angle, ref SpatialInfo spatialInfo);
    
    /// <summary>
    /// Update the SpatialInfo of a stationary camera.
    /// </summary>
    /// <param name="spatialInfo">Reference SpatialInfo of the camera.</param>
    public void Update(ref SpatialInfo spatialInfo);
}