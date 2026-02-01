using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Microsoft.Xna.Framework;

namespace OrbitGame;

public abstract class KinematicObject
{
    public string Identifier;

    public IMesh Mesh { get; }
    public CompactCollider Collider { get; }
    public Material Material { get; }
    
    public ScientificDecimal Mass { get; protected set; }
    
    public SD_Vector2 Position { get; set; }
    public SD_Vector2 Velocity { get; set; }
    public SD_Vector2 Acceleration { get; set; }

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
        KinematicObjectTemplate template, 
        string identifier, 
        ScientificDecimal mass, 
        SD_Vector2 position, 
        SD_Vector2 velocity,
        IMesh? mesh = null,
        CompactCollider? collider = null,
        Material? material = null
        )
    {
        Mesh = template.Mesh ?? mesh ?? throw new NullReferenceException();
        Collider = template.Collider ?? collider ?? throw new NullReferenceException();
        Material = template.Material ?? material ?? throw new NullReferenceException();
        Identifier = identifier;
        Mass = mass;
        Position = position;
        Velocity = velocity;
    }
    
    #region Coordinate Transforms

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
    
    #endregion
    
    public abstract class KinematicObjectTemplate
    {
        public IMesh? Mesh { get; set; }
        public CompactCollider? Collider { get; set; }
        public Material? Material { get; set; }
        
        protected Dictionary<string, KinematicObject> Instances { get; } = new();
        
        protected KinematicObjectTemplate(IMesh? mesh, CompactCollider? collider, Material? material)
        {
            Mesh = mesh;
            if (mesh != null) mesh.GenerateBuffers();
            Collider = collider;
            Material = material;
        }
        
        public void AddInstance(string identifier, KinematicObject instance)
            => Instances.Add(identifier, instance);

        public bool Destroy(string identifier)
            => Instances.Remove(identifier);
    }
}