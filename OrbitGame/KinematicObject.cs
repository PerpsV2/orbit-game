using System;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace OrbitGame;

/// <summary>
/// A point mass with a spatial and rotational information in the game space. Does not have any physical shape.
/// </summary>
public abstract class KinematicObject
{
    public string Identifier;
    
    public ScientificDecimal Mass;
    public Material Material;
    
    public SD_Vector2 Position;
    public SD_Vector2 Velocity;
    public SD_Vector2 Acceleration;

    private double _angle;
    public double Angle
    {
        get => Utils.UnsignedMod(_angle, Math.Tau);
        set => _angle = value;
    }
    public double AngularVelocity;

    public SD_Vector2 ForwardVector => SD_Vector2.FromPolar(Angle);
    public SD_Vector2 RightVector => SD_Vector2.FromPolar(Angle - Math.PI / 2);
    
    protected KinematicObject(string identifier, ScientificDecimal mass, Material material, SD_Vector2 position, SD_Vector2 velocity)
    {
        Identifier = identifier;
        Mass = mass;
        Material = material;
        Position = position;
        Velocity = velocity;
    }

    protected KinematicObject(string identifier, ScientificDecimal mass, Material material, SD_Vector2 position, SD_Vector2 velocity,
        double angle, double angularVelocity)
        : this(identifier, mass, material, position, velocity)
    {
        Angle = angle;
        AngularVelocity = angularVelocity;
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