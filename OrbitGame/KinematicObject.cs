namespace OrbitGame;

/// <summary>
/// A point mass with a spatial and rotational information in the game space. Does not have any physical shape.
/// </summary>
public abstract class KinematicObject
{
    public ScientificDecimal Mass;
    
    public Vector2 Position;
    public Vector2 Velocity;
    public Vector2 Acceleration;
    public string Name;

    private double _angle;
    public double Angle
    {
        get => Utils.UnsignedMod(_angle, Math.Tau);
        set => _angle = value;
    }
    public double AngularVelocity;

    public Vector2 ForwardVector => Vector2.FromPolar(Angle);
    public Vector2 RightVector => Vector2.FromPolar(Angle - Math.PI / 2);
    
    protected KinematicObject(
        string name,
        ScientificDecimal? mass,
        Vector2? position, 
        Vector2? velocity, 
        double? angle = null, 
        double? angularVelocity = null
        )
    {
        Mass = mass ?? 1;
        Position = position ?? Vector2.Zero;
        Velocity = velocity ?? Vector2.Zero;
        Angle = angle ?? 0;
        AngularVelocity = angularVelocity ?? 0;
        Name = name;
    }

    private Matrix3X3 GetLocalSpaceMatrix()
        => Matrix3X3.Translation(Position) * Matrix3X3.Rotation(Angle);

    public Vector2 ObjectToWorldSpace(Vector2 point)
    {
        return GetLocalSpaceMatrix() * point;
    }

    public Vector2 WorldToObjectSpace(Vector2 point)
    {
        return Matrix3X3.Rotation(-Angle) * Matrix3X3.Translation(-Position) * point;
    }

    public Vector2 ObjectToObjectSpace(Vector2 point, KinematicObject newOriginObject)
    {
        return newOriginObject.WorldToObjectSpace(ObjectToWorldSpace(point));
    }
}