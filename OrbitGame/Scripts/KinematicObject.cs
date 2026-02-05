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

    public SpatialInfo SpatialInfo;
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

    public SD_Vector2 ForwardVector => SD_Vector2.FromPolar(SpatialInfo.Angle);
    public SD_Vector2 RightVector => SD_Vector2.FromPolar(SpatialInfo.Angle - Math.PI / 2);
    
    protected KinematicObject(
        KinematicObjectTemplate template, 
        string identifier, 
        ScientificDecimal mass, 
        SpatialInfo spatialInfo,
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
        SpatialInfo = spatialInfo;
    }
    
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