using System;

namespace OrbitGame;

/// <summary>
/// A point mass with a spatial and rotational information in the game space. Does not have any physical shape.
/// </summary>
public abstract class KinematicObject
{
    public ScientificDecimal Mass;
    
    public SD_Vector2 Position;
    public SD_Vector2 Velocity;
    public SD_Vector2 Acceleration;
    public string Name;

    private double _angle;
    public double Angle
    {
        get => Utils.UnsignedMod(_angle, Math.Tau);
        set => _angle = value;
    }
    public double AngularVelocity;

    public SD_Vector2 ForwardVector => SD_Vector2.FromPolar(Angle);
    public SD_Vector2 RightVector => SD_Vector2.FromPolar(Angle - Math.PI / 2);
    
    protected KinematicObject(
        string name,
        ScientificDecimal? mass,
        SD_Vector2? position, 
        SD_Vector2? velocity, 
        double? angle = null, 
        double? angularVelocity = null
    )
    {
        Mass = mass ?? 1;
        Position = position ?? SD_Vector2.Zero;
        Velocity = velocity ?? SD_Vector2.Zero;
        Angle = angle ?? 0;
        AngularVelocity = angularVelocity ?? 0;
        Name = name;
    }

    private Matrix3X3 GetLocalSpaceMatrix()
        => Matrix3X3.Translation(Position) * Matrix3X3.Rotation(Angle);

    public SD_Vector2 ObjectToWorldSpace(SD_Vector2 point)
    {
        return GetLocalSpaceMatrix() * point;
    }

    public SD_Vector2 WorldToObjectSpace(SD_Vector2 point)
    {
        return Matrix3X3.Rotation(-Angle) * Matrix3X3.Translation(-Position) * point;
    }

    public SD_Vector2 ObjectToObjectSpace(SD_Vector2 point, KinematicObject newOriginObject)
    {
        return newOriginObject.WorldToObjectSpace(ObjectToWorldSpace(point));
    }
}