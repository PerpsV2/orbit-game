namespace OrbitGame;

/// <summary>
/// Struct containing information about the landing of a ship on an object
/// </summary>
/// <param name="Parent">Object the ship has landed on</param>
/// <param name="RelativePosition">Position of the ship relative to the object that has been landed on</param>
/// <param name="RelativeAngle">Angle of the ship relative to the object that has been landed on</param>
public record struct Landing(
    KinematicObject Parent,
    SD_Vector2 RelativePosition,
    double RelativeAngle
)
{
    public void ResetOrigin(SD_Vector2 origin)
    {
        RelativePosition -= origin;
    }
}