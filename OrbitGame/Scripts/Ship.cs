using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame;

namespace OrbitGame;

/// <summary>
/// Player-controllable celestial object.
/// </summary>
public class Ship : Body, IGameDrawable
{
    private readonly ScientificDecimal _maximumRadius;
    private readonly OrbitMesh _orbitMesh;
    
    private SD_Vector2 ArtificialAcceleration { get; set; }
    
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

    public void Draw()
    {
        Camera camera = OrbitGame.Camera;
        GraphicsDevice graphicsDevice = OrbitGame.Graphics;
        
        if ((Position - camera.Position).MagnitudeSquared() - 4 * _maximumRadius.Square() > camera.MaximumRadiusSquared) return;
        
        Vector2 screenPosition = camera.ConvertToScreenCoordinates(Position);
        float screenDistance = camera.ConvertToScreenDistance(_maximumRadius);
        if (LandingState == null && DrawOrbitalPath) KeplerOrbitPath.DrawOrbitalPath(_orbitMesh, Colour);
        if (screenDistance > 1)
        {
            Vector2 scale = new((float)(Options.ScreenSize.height / camera.Height),
                (float)(Options.ScreenSize.width / camera.Width));
            Matrix transform = Matrix.CreateScale(new Vector3(scale.X, scale.Y, 1)) *
                               Matrix.CreateRotationZ(-(float)(Angle + camera.Angle)) *
                               Matrix.CreateTranslation(new Vector3(screenPosition.X, screenPosition.Y, 0));
            Mesh.Draw(graphicsDevice, transform, new()
            {
                { "Colour", Colour.ToVector4() }
            });
        }
        if (screenDistance < 10)
        {
            double iconAngle = -Angle - camera.Angle;
            float alpha = Utils.Clamp(1 - camera.ConvertToScreenDistance(_maximumRadius) / 10, 0, 255);
            Color colour = Colour * alpha;
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(10, -5), iconAngle), 
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(10, 5), iconAngle), colour);
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(10, 0), iconAngle), 
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(-10, 0), iconAngle), colour);
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(5, -8), iconAngle), 
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(-10, 0), iconAngle), colour);
            graphicsDevice.DrawLine(
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(5, 8), iconAngle), 
                screenPosition + (Vector2)SD_Vector2.RotatePoint(new(-10, 0), iconAngle), colour);
        }
    }

    public void DrawCollider()
    {
        throw new NotImplementedException();
    }
    
    public void CalculateShipKeplerianOrbit(List<Planet> planets)
    {
        if (Parent == null)
            throw new NullReferenceException($"Ship \"{Identifier}\" has no parent");
        
        ScientificDecimal? parentSOIRadius = Parent.KeplerOrbitPath.Orbit?.SphereOfInfluenceRadius;
        if (parentSOIRadius != null)
            if ((Position - Parent.Position).Magnitude() > parentSOIRadius)
                Parent = Parent.Parent ?? throw new ArgumentException("Parent with SOI has no parent itself.");
        
        foreach (Planet planet in planets)
        {
            if (planet == Parent) continue;
            ScientificDecimal? bodySOIRadius = planet.KeplerOrbitPath.Orbit?.SphereOfInfluenceRadius;
            if (bodySOIRadius != null)
                if ((Position - planet.Position).Magnitude() < bodySOIRadius)
                    Parent = planet;
        }
        
        KeplerOrbitPath.Orbit = CalculateKeplerianOrbit(Parent, false);
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
        ArtificialAcceleration += thrust / Mass;
        DisturbLandingState();
    }

    public void ResetThrust()
    {
        ArtificialAcceleration = SD_Vector2.Zero;
    }

    public override SD_Vector2 CalculateNetAcceleration()
    {
        return base.CalculateNetAcceleration() + ArtificialAcceleration;
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