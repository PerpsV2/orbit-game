namespace OrbitGame;

/// <summary>
/// An object with a spatial position in the game space.
/// </summary>
public abstract class KinematicObject(Vector2 position, Vector2 velocity)
{
    public Vector2 Position = position;
    public Vector2 Velocity = velocity;
    public Vector2 Acceleration;
}