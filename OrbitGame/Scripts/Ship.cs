using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

public class Ship : Body, IGameDrawable
{
    private readonly ScientificDecimal _maximumRadius;
    private readonly OrbitMesh _orbitMesh;
    
    private SD_Vector2 _artificialAcceleration { get; set; }
    
    public bool DrawOrbitalPath { get; set; }
    public bool MarkedForRemoval { get; set; }
    
    public Landing? LandingState { get; private set; }

    private Ship(
        string identifier,
        SpatialInfo spatialInfo,
        ObjectInfo objectInfo,
        OrbitMesh orbitMesh,
        ScientificDecimal maximumRadius,
        ScientificDecimal mass,
        Color colour,
        Planet parent)
        : base(identifier, spatialInfo, objectInfo, mass, colour, parent)
    {
        _maximumRadius = maximumRadius;
        _orbitMesh = orbitMesh;
        Collider.CalculateInertia(mass);
    }

    public override void Draw(GraphicsDevice graphicsDevice, Camera camera)
    {
        if (Position.X < camera.Left - _maximumRadius) return;
        if (Position.X > camera.Right + _maximumRadius) return;
        if (Position.Y > camera.Top + _maximumRadius) return;
        if (Position.Y < camera.Bottom - _maximumRadius) return;
        
        Vector2 screenPosition = camera.ConvertToScreenCoordinates(Position);
        float screenDistance = camera.ConvertToScreenDistance(_maximumRadius);
        if (LandingState == null && DrawOrbitalPath) DrawOrbitalPathLRL(graphicsDevice, camera, _orbitMesh);
        if (screenDistance > 1)
        {
            Vector2 scale = new((float)(Options.ScreenSize.height / camera.Height),
                (float)(Options.ScreenSize.width / camera.Width));
            Matrix transform = Matrix.CreateScale(new Vector3(scale.X, scale.Y, 1)) *
                               Matrix.CreateRotationZ(-(float)(Angle + camera.GetAbsoluteAngle())) *
                               Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0));
            Mesh.Draw(graphicsDevice, transform, new()
            {
                { "colour", Colour.ToVector4() }
            });
        }
        if (screenDistance < 10)
        {
            float alpha = Utils.Clamp(1 - camera.ConvertToScreenDistance(_maximumRadius) / 10, 0, 255);
            Color colour = new Color(Colour, alpha);
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(10, -10), -Angle), 
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(10, 10), -Angle), colour);
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(10, 0), -Angle), 
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(-10, 00), -Angle), colour);
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(0, -10), -Angle), 
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(-10, 0), -Angle), colour);
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(0, 10), -Angle), 
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(-10, 0), -Angle), colour);
        }
    }

    public override void DrawCollider(GraphicsDevice graphicsDevice, Camera camera)
    {
        throw new NotImplementedException();
    }
    
    public void CalculateShipOrbit(List<Planet> planets)
    {
        if (Parent == null)
            throw new NullReferenceException($"Ship \"{Identifier}\" has no parent");
        
        ScientificDecimal? parentSOIRadius = Parent.Orbit?.SphereOfInfluenceRadius;
        if (parentSOIRadius != null)
            if ((Position - Parent.Position).Magnitude() > parentSOIRadius)
                Parent = Parent.Parent ?? throw new ArgumentException("Parent with SOI has no parent itself.");
        
        foreach (Planet planet in planets)
        {
            if (planet == Parent) continue;
            ScientificDecimal? bodySOIRadius = planet.Orbit?.SphereOfInfluenceRadius;
            if (bodySOIRadius != null)
                if ((Position - planet.Position).Magnitude() < bodySOIRadius)
                    Parent = planet;
        }
        
        Orbit = CalculateOrbit(false);
    }

    public override void ResetOrigin(SD_Vector2 origin)
    {
        base.ResetOrigin(origin);
        LandingState?.ResetOrigin(origin);
    }

    public void UpdatePosition_Landed()
    {
        if (LandingState == null)
            throw new NullReferenceException("Ship is not landed");
        Landing landing = LandingState.Value;
        Position = landing.Parent.Position + SD_Vector2.RotatePoint(landing.RelativePosition, landing.Parent.Angle);
        Velocity = landing.Parent.Velocity;
        Angle = landing.Parent.Angle + landing.RelativeAngle;
    }
    
    public void SetLandingState(KinematicObject parent)
    {
        LandingState = new Landing(parent, Position - parent.Position, Angle - parent.Angle);
    }

    private void DisturbLandingState()
    {
        LandingState = null;
    }

    public void ApplyThrust(SD_Vector2 thrust, SD_Vector2 position)
    {
        thrust = SD_Vector2.RotatePoint(thrust, Angle);
        position = SD_Vector2.RotatePoint(position, Angle);
        ScientificDecimal torque = SD_Vector2.Cross(thrust, position).Z;
        AngularVelocity += (double)(torque / Mass);
        _artificialAcceleration += thrust / Mass;
        DisturbLandingState();
    }

    public void ResetThrust()
    {
        _artificialAcceleration = SD_Vector2.Zero;
    }

    public override SD_Vector2 CalculateNetAcceleration()
    {
        return base.CalculateNetAcceleration() + _artificialAcceleration;
    }

    public void Destroy()
    {
        MarkedForRemoval = true;
    }
    
    public class ShipTemplate : KinematicObjectTemplate
    {
        private readonly IMesh _mesh;
        private readonly CompactCollider _collider;
        private readonly OrbitMesh _orbitMesh = new();
        private readonly ScientificDecimal _maximumRadius;
        private readonly Material _material;
        
        public ShipTemplate(SD_Vector2[] shipVertices, Material material)
        {
            _material = material;
            _mesh = new PolyMesh(shipVertices);
            _collider = new ConvexCollider(shipVertices);
            _maximumRadius = shipVertices.Select(x => x.Magnitude()).Max();
            _mesh.GenerateBuffers();
            _orbitMesh.GenerateBuffers();
        }

        public Ship CreateInstance(
            string identifier, 
            SpatialInfo spatialInfo,
            ScientificDecimal mass,
            Color colour,
            Planet parent
            )
        {
            ObjectInfo objectInfo = new ObjectInfo(_mesh, _collider, _material);
            Ship ship = new Ship(identifier, spatialInfo, objectInfo, _orbitMesh, _maximumRadius, mass, colour, parent);
            AddInstance(identifier, ship);
            return ship;
        }
    }
}