namespace OrbitGame;

/// <summary>
/// An object with a spatial and rotational position in the game space.
/// </summary>
public abstract class KinematicObject
{
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;

    private double _angle;
    public double Angle
    {
        get => Utils.UnsignedMod(_angle, Math.Tau);
        set => _angle = value;
    }
    public double AngularVelocity;
    
    protected KinematicObject(
        Vector2? position, 
        Vector2? velocity, 
        double? angle = null, 
        double? angularVelocity = null
        )
    {
        Position = position ?? Vector2.Zero;
        Velocity = velocity ?? Vector2.Zero;
        Angle = angle ?? 0;
        AngularVelocity = angularVelocity ?? 0;
    }

    public Vector2 ObjectToWorldSpace(Vector2 point)
    {
        return new Vector2(point.X * Math.Cos(Angle) - point.Y * Math.Sin(Angle),
            point.X * Math.Sin(Angle) + point.Y * Math.Cos(Angle)) + Position;
    }
}