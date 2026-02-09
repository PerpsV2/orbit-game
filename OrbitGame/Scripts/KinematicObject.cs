using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public abstract class KinematicObject(
    string identifier,
    ScientificDecimal mass,
    SpatialInfo spatialInfo,
    ObjectInfo objectInfo)
{
    public readonly string Identifier = identifier;
    
    public ScientificDecimal Mass { get; protected set; } = mass;

    public SpatialInfo SpatialInfo = spatialInfo;
    public SD_Vector2 Position
    {
        get => SpatialInfo.Position;
        set => SpatialInfo.Position = value;
    }
    public SD_Vector2 Velocity
    {
        get => SpatialInfo.Velocity;
        set => SpatialInfo.Velocity = value;
    }
    public SD_Vector2 Acceleration
    {
        get => SpatialInfo.Acceleration;
        set => SpatialInfo.Acceleration = value;
    }
    public double Angle
    {
        get => SpatialInfo.Angle;
        set => SpatialInfo.Angle = value;
    }
    public double AngularVelocity
    {
        get => SpatialInfo.AngularVelocity;
        set => SpatialInfo.AngularVelocity = value;
    }

    public IMesh Mesh => objectInfo.Mesh;
    public CompactCollider Collider => objectInfo.Collider;
    public Material Material => objectInfo.Material;

    public SD_Vector2 ForwardVector => SD_Vector2.FromPolar(SpatialInfo.Angle);
    public SD_Vector2 RightVector => SD_Vector2.FromPolar(SpatialInfo.Angle - Math.PI / 2);

    #region Coordinate Transforms
    
    public SD_Vector2 ObjectToWorldSpace(SD_Vector2 point)
    {
        return Matrix3X3.Translation(SpatialInfo.Position) * Matrix3X3.Rotation(SpatialInfo.Angle) * point;
    }

    public SD_Vector2 WorldToObjectSpace(SD_Vector2 point)
    {
        return Matrix3X3.Rotation(-SpatialInfo.Angle) * Matrix3X3.Translation(-SpatialInfo.Position) * point;
    }

    public SD_Vector2 ObjectToObjectSpace(SD_Vector2 point, KinematicObject newOriginObject)
    {
        return newOriginObject.WorldToObjectSpace(ObjectToWorldSpace(point));
    }
    
    #endregion

    public virtual void ResetOrigin(SD_Vector2 origin)
    {
        Position -= origin;
    }
    
    public abstract class KinematicObjectTemplate
    {
        private Dictionary<string, KinematicObject> Instances { get; } = new();
        
        public void AddInstance(string identifier, KinematicObject instance)
            => Instances.Add(identifier, instance);

        public bool Destroy(string identifier)
            => Instances.Remove(identifier);
    }
}